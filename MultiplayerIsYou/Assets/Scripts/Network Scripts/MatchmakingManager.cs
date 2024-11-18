using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MatchmakingManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public Text usernameText;
    public Image profileIconImage;
    public Transform lobbyListParent; // ScrollView Content Parent
    public GameObject lobbyPrefab;   // Prefab for displaying lobby details
    public GameObject matchmakingPanel;
    public GameObject lobbyPanel;    // Lobby details panel
    public Text lobbyNameText, playersConnectedText, publicPrivateText;
    public Image player1Icon, player2Icon;
    public Text player1Username, player2Username;
    public Button createGameButton, startGameButton, leaveButton;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    void Update()
    {
        // Update the UI with the current player's username and profile icon
        usernameText.text = PhotonNetwork.NickName;
        profileIconImage.sprite = (Sprite)PhotonNetwork.LocalPlayer.CustomProperties["Icon"];
    }

    public void Start()
    {
        // Connect to Photon Lobby
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
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

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear the cached room list and UI
        cachedRoomList.Clear();
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }

        // Populate the room list
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

        lobbyItem.transform.Find("LobbyName").GetComponent<Text>().text = room.Name;
        lobbyItem.transform.Find("PlayerCount").GetComponent<Text>().text = $"{room.PlayerCount}/{room.MaxPlayers}";
        lobbyItem.transform.Find("PublicPrivate").GetComponent<Text>().text = room.IsVisible ? "Public" : "Private";

        // Configure Join Button
        Button joinButton = lobbyItem.transform.Find("JoinButton").GetComponent<Button>();
        if (room.PlayerCount < room.MaxPlayers)
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
            IsVisible = !isPrivate
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
        ShowMatchmakingPanel();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("LevelSelectScene");
        }
    }
}
