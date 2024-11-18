using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class MatchmakingManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public TMP_Text usernameText;
    public Image profileIconImage;
    public Transform lobbyListParent; // ScrollView Content Parent
    public GameObject lobbyPrefab;   // Prefab for displaying lobby details
    public GameObject matchmakingPanel;
    public GameObject lobbyPanel;    // Lobby details panel
    public TMP_Text lobbyNameText, playersConnectedText, publicPrivateText;
    public Image player1Icon, player2Icon;
    public TMP_Text player1Username, player2Username;
    public Button createGameButton, startGameButton, leaveButton;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    public MainMenuManager mainMenuManager; // Reference to MainMenuManager

    private void Start()
    {
        // Ensure the player is connected to the Photon Lobby
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    private async void Update()
    {
        // Continuously update the username
        usernameText.text = PhotonNetwork.NickName;

        // Check if the Icon property exists
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Icon"))
        {
            // Safely retrieve the Icon property as a string
            string iconUrl = PhotonNetwork.LocalPlayer.CustomProperties["Icon"] as string;

            if (!string.IsNullOrEmpty(iconUrl))
            {
                // Check if the current profileIconImage already matches to avoid redundant downloads
                if (profileIconImage.sprite == null || !profileIconImage.sprite.name.Equals(iconUrl))
                {
                    // Use the DownloadImageFromS3 method from MainMenuManager
                    if (mainMenuManager != null)
                    {
                        Texture2D texture = await mainMenuManager.DownloadImageFromS3(iconUrl);
                        if (texture != null)
                        {
                            profileIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                            profileIconImage.sprite.name = iconUrl; // Tag the sprite with the URL to prevent re-downloading
                        }
                    }
                }
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear the cached room list and update the UI
        cachedRoomList.Clear();
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }

        // Populate the room list with updated data
        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                cachedRoomList.Add(room);
                CreateRoomEntry(room);
            }
        }
    }

    private void CreateRoomEntry(RoomInfo room)
    {
        GameObject lobbyItem = Instantiate(lobbyPrefab, lobbyListParent);

        lobbyItem.transform.Find("LobbyName").GetComponent<TMP_Text>().text = room.Name;
        lobbyItem.transform.Find("PlayerCount").GetComponent<TMP_Text>().text = $"{room.PlayerCount}/{room.MaxPlayers}";
        lobbyItem.transform.Find("PublicPrivate").GetComponent<TMP_Text>().text = room.IsVisible ? "Public" : "Private";

        // Configure Join Button
        Button joinButton = lobbyItem.transform.Find("JoinButton").GetComponent<Button>();
        if (room.PlayerCount < room.MaxPlayers && !PhotonNetwork.InRoom)
        {
            joinButton.interactable = true;
            joinButton.onClick.AddListener(() => JoinLobby(room.Name));
        }
        else
        {
            joinButton.interactable = false;
        }
    }

    public void JoinLobby(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreateLobby(string lobbyName, bool isPrivate, string password = null)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = !isPrivate,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "Password", password } // Store the password in custom properties
            },
            CustomRoomPropertiesForLobby = new string[] { "Password" } // Expose this property to the lobby
        };

        PhotonNetwork.CreateRoom(lobbyName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        ShowLobbyPanel();

        // Update lobby panel details
        lobbyNameText.text = PhotonNetwork.CurrentRoom.Name;
        publicPrivateText.text = PhotonNetwork.CurrentRoom.IsVisible ? "Public" : "Private";
        UpdateLobbyDetails();

        // Only show the start button for the host
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        createGameButton.gameObject.SetActive(false); // Hide Create Game Button
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLobbyDetails();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyDetails();
    }

    private void UpdateLobbyDetails()
    {
        // Update player details
        Player[] players = PhotonNetwork.PlayerList;

        if (players.Length > 0)
        {
            player1Username.text = players[0].NickName;
            player1Icon.sprite = (Sprite)players[0].CustomProperties["Icon"];
        }

        if (players.Length > 1)
        {
            player2Username.text = players[1].NickName;
            player2Icon.sprite = (Sprite)players[1].CustomProperties["Icon"];
        }
        else
        {
            player2Username.text = "";
            player2Icon.sprite = null;
        }

        playersConnectedText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

        // Hide the start button if less than 2 players
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2);
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveRoom();
        ShowMatchmakingPanel();
    }

    public override void OnLeftRoom()
    {
        // Clear the lobby panel and return to matchmaking
        player1Username.text = "";
        player2Username.text = "";
        player1Icon.sprite = null;
        player2Icon.sprite = null;
        playersConnectedText.text = "";
        createGameButton.gameObject.SetActive(true); // Show Create Game Button
        ShowMatchmakingPanel();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("LevelSelectScene");
        }
    }

    public void ShowMatchmakingPanel()
    {
        matchmakingPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    public void ShowLobbyPanel()
    {
        matchmakingPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
