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
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;    // Lobby details panel
    public GameObject createLobbyPanel; // Panel for creating lobbies
    public TMP_InputField lobbyNameInput;
    public TMP_Text lobbyNameText, playersConnectedText, publicPrivateText;
    public Image player1Icon, player2Icon;
    public TMP_Text player1Username, player2Username;
    public GameObject player1Panel, player2Panel; // Panels for player details
    public Button createGameButton, startGameButton, leaveButton, publicPrivateButton, confirmCreateLobbyButton;

    private bool isLobbyPrivate = false; // Tracks if the lobby is private
    private bool isReadyForOperations = false; // Tracks if the client is ready for room operations
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    public MainMenuManager mainMenuManager; // Reference to MainMenuManager

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon Master Server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InLobby)
        {
            Debug.Log("Joining Lobby...");
            PhotonNetwork.JoinLobby();
        }
    }

    private async void Update()
    {
        usernameText.text = PhotonNetwork.NickName;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Icon"))
        {
            string iconUrl = PhotonNetwork.LocalPlayer.CustomProperties["Icon"] as string;
            if (!string.IsNullOrEmpty(iconUrl) && mainMenuManager != null)
            {
                if (profileIconImage.sprite == null || !profileIconImage.sprite.name.Equals(iconUrl))
                {
                    Texture2D texture = await mainMenuManager.DownloadImageFromS3(iconUrl);
                    if (texture != null)
                    {
                        profileIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        profileIconImage.sprite.name = iconUrl;
                    }
                }
            }
        }

        createGameButton.gameObject.SetActive(!PhotonNetwork.InRoom);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log($"Joined Lobby: {PhotonNetwork.CurrentLobby.Name}");
        isReadyForOperations = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected from Photon: {cause}");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList.Clear();
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }

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

        Button joinButton = lobbyItem.transform.Find("JoinButton").GetComponent<Button>();
        joinButton.onClick.RemoveAllListeners();

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
        if (!isReadyForOperations)
        {
            Debug.LogError("Client is not ready for room operations.");
            return;
        }

        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogError("Lobby name cannot be empty.");
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = !isPrivate,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "Password", password }
            },
            CustomRoomPropertiesForLobby = new string[] { "Password" }
        };

        PhotonNetwork.CreateRoom(lobbyName, roomOptions);
        createLobbyPanel.SetActive(false);
        matchmakingPanel.SetActive(true); // Show the matchmaking panel again
    }

    public void OnCreateLobbyClicked()
    {
        string lobbyName = lobbyNameInput.text;
        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogError("Lobby name cannot be empty.");
            return;
        }

        CreateLobby(lobbyName, isLobbyPrivate);
    }

    public void TogglePublicPrivate()
    {
        isLobbyPrivate = !isLobbyPrivate;
        publicPrivateButton.GetComponentInChildren<TMP_Text>().text = isLobbyPrivate ? "Private" : "Public";
    }

    public void OpenCreateLobbyPanel()
    {
        createLobbyPanel.SetActive(true);
        matchmakingPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Player has joined the room: {PhotonNetwork.CurrentRoom.Name}");
        ShowLobbyPanel();

        lobbyNameText.text = PhotonNetwork.CurrentRoom.Name;
        publicPrivateText.text = PhotonNetwork.CurrentRoom.IsVisible ? "Public" : "Private";
        UpdateLobbyDetails();

        lobbyPanel.SetActive(true);
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLobbyDetails();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyDetails();
    }

    private async void UpdateLobbyDetails()
    {
        Player[] players = PhotonNetwork.PlayerList;

        player1Panel.SetActive(players.Length >= 1);
        player2Panel.SetActive(players.Length == 2);

        if (players.Length > 0)
        {
            player1Username.text = players[0].NickName;

            if (players[0].CustomProperties.ContainsKey("Icon"))
            {
                string iconUrl = players[0].CustomProperties["Icon"] as string;
                if (!string.IsNullOrEmpty(iconUrl))
                {
                    Texture2D texture = await mainMenuManager.DownloadImageFromS3(iconUrl);
                    if (texture != null)
                    {
                        player1Icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    }
                }
            }
            else
            {
                player1Icon.sprite = null;
            }
        }

        if (players.Length > 1)
        {
            player2Username.text = players[1].NickName;

            if (players[1].CustomProperties.ContainsKey("Icon"))
            {
                string iconUrl = players[1].CustomProperties["Icon"] as string;
                if (!string.IsNullOrEmpty(iconUrl))
                {
                    Texture2D texture = await mainMenuManager.DownloadImageFromS3(iconUrl);
                    if (texture != null)
                    {
                        player2Icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    }
                }
            }
            else
            {
                player2Icon.sprite = null;
            }
        }
        else
        {
            player2Username.text = "";
            player2Icon.sprite = null;
        }

        playersConnectedText.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2);
    }

    public void LeaveLobby()
    {
        Debug.Log($"Player has left the room: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LeaveRoom();
        ShowMatchmakingPanel();
    }

    public override void OnLeftRoom()
    {
        player1Username.text = "";
        player2Username.text = "";
        player1Icon.sprite = null;
        player2Icon.sprite = null;
        playersConnectedText.text = "";
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
        createLobbyPanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    public void ShowLobbyPanel()
    {
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
