using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;
using System;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;
using dotenv.net;
using System.IO;
using ExitGames.Client.Photon;
using System.Collections;
using UnityEngine.Networking;
using System.Net;

// Add this alias to resolve ambiguity
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class MatchmakingManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public TMP_Text usernameText;
    public Image profileIconImage;

    [Header("Panels")]
    public GameObject matchmakingPanel;
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;
    public GameObject createLobbyPanel;

    [Header("Lobby Lists")]
    public Transform lobbyListParent; // ScrollView Content Parent
    public GameObject lobbyPrefab;   // Prefab for displaying lobby details

    [Header("Lobby Creation Panel")]
    public TMP_InputField lobbyNameInput;
    public GameObject passwordPanel; // Password panel for lobby creation
    public TMP_InputField passwordInput; // Password input field for lobby creation

    [Header("Password Input Panel")]
    public GameObject passwordInputPanel; // Panel to show when joining private room
    public TMP_InputField joinPasswordInput; // Input field for password
    private string roomToJoin; // Stores the name of the room the player wants to join

    [Header("Joined Lobby Info")]
    public TMP_Text lobbyNameText, playersConnectedText, publicPrivateText;
    public Image player1Icon, player2Icon;
    public TMP_Text player1Username, player2Username;
    public GameObject player1Panel, player2Panel; // Panels for player details
    public TMP_Text waitingText; // Waiting text
    private Coroutine waitingCoroutine;

    [Header("Buttons")]
    public Button createGameButton, startGameButton, leaveButton, publicPrivateButton, confirmCreateLobbyButton;

    private bool isLobbyPrivate = false; // Tracks if the lobby is private
    private bool isReadyForOperations = false; // Tracks if the client is ready for room operations
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    public MainMenuManager mainMenuManager; // Reference to MainMenuManager

    private Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

    // AWS S3 Configuration
    private const string bucketName = "multiplayerisyou";
    private AmazonS3Client s3Client;

    private void Awake()
    {
        // Ensure TLS 1.2 is used
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        // Load environment variables
        string envFilePath = "";

#if UNITY_EDITOR
        // In the Unity Editor, the project root is one level up from Application.dataPath
        envFilePath = Path.Combine(Application.dataPath, "..", ".env");
#else
        // In builds, include the .env file in StreamingAssets
        envFilePath = Path.Combine(Application.streamingAssetsPath, ".env");
#endif

        // Load environment variables from the specified path
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envFilePath }));

        // Fetch AWS credentials and region from environment variables
        string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
        string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        string region = Environment.GetEnvironmentVariable("AWS_REGION");

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(region))
        {
            Debug.LogError("AWS credentials or region are not set in the environment variables.");
            return;
        }

        // Initialize AmazonS3Client with credentials
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);

        s3Client = new AmazonS3Client(credentials, regionEndpoint);
    }

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

        // Update the profile icon at start
        UpdatePlayerProfileIcon();
    }

    private void Update()
    {
        usernameText.text = PhotonNetwork.NickName;
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
        Debug.Log($"Room list updated. Total rooms: {roomList.Count}");

        // Update the cached room list
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                // Remove room from cached list
                int index = cachedRoomList.FindIndex(r => r.Name == room.Name);
                if (index != -1)
                {
                    cachedRoomList.RemoveAt(index);
                }
            }
            else
            {
                // Add or update room in cached list
                int index = cachedRoomList.FindIndex(r => r.Name == room.Name);
                if (index == -1)
                {
                    // Add new room
                    cachedRoomList.Add(room);
                }
                else
                {
                    // Update existing room
                    cachedRoomList[index] = room;
                }
            }
        }

        // Refresh the room list UI
        RefreshRoomList();
    }

    private void CreateRoomEntry(RoomInfo room)
    {
        if (lobbyPrefab == null)
        {
            Debug.LogError("Lobby prefab is not assigned in the inspector.");
            return;
        }

        // Instantiate the prefab under the lobbyListParent
        GameObject lobbyItem = Instantiate(lobbyPrefab, lobbyListParent);
        if (lobbyItem == null)
        {
            Debug.LogError("Failed to instantiate lobbyPrefab.");
            return;
        }

        Debug.Log($"Creating room entry: {room.Name}, Players: {room.PlayerCount}/{room.MaxPlayers}, Visible: {room.IsVisible}");

        // Find and set lobby details
        TMP_Text lobbyNameText = lobbyItem.transform.Find("LobbyName")?.GetComponent<TMP_Text>();
        TMP_Text playerCountText = lobbyItem.transform.Find("PlayerCount")?.GetComponent<TMP_Text>();
        TMP_Text publicPrivateText = lobbyItem.transform.Find("PublicPrivate")?.GetComponent<TMP_Text>();
        Button joinButton = lobbyItem.transform.Find("JoinButton")?.GetComponent<Button>();

        if (lobbyNameText != null)
            lobbyNameText.text = room.Name;
        else
            Debug.LogError("LobbyNameText not found or missing TMP_Text component");

        if (playerCountText != null)
            playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
        else
            Debug.LogError("PlayerCountText not found or missing TMP_Text component");

        // Determine if the room is private based on the presence of a password
        bool isPrivateRoom = room.CustomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)room.CustomProperties["Password"]);
        if (publicPrivateText != null)
            publicPrivateText.text = isPrivateRoom ? "Private" : "Public";
        else
            Debug.LogError("PublicPrivateText not found or missing TMP_Text component");

        if (joinButton == null)
        {
            Debug.LogError("JoinButton not found or missing Button component");
            return;
        }

        // Configure Join Button
        joinButton.onClick.RemoveAllListeners();

        if (PhotonNetwork.InRoom)
        {
            joinButton.interactable = false;
        }
        else if (room.PlayerCount < room.MaxPlayers)
        {
            joinButton.interactable = true;

            if (isPrivateRoom)
            {
                joinButton.onClick.AddListener(() =>
                {
                    roomToJoin = room.Name;
                    passwordInputPanel.SetActive(true);
                });
            }
            else
            {
                joinButton.onClick.AddListener(() => JoinLobby(room.Name));
            }
        }
        else
        {
            joinButton.interactable = false;
        }

        // Get player data from custom properties
        string player1Name = room.CustomProperties.ContainsKey("Player1Name") ? (string)room.CustomProperties["Player1Name"] : "";
        string player1IconUrl = room.CustomProperties.ContainsKey("Player1Icon") ? (string)room.CustomProperties["Player1Icon"] : "";
        string player2Name = room.CustomProperties.ContainsKey("Player2Name") ? (string)room.CustomProperties["Player2Name"] : "";
        string player2IconUrl = room.CustomProperties.ContainsKey("Player2Icon") ? (string)room.CustomProperties["Player2Icon"] : "";

        // Update player names
        TMP_Text player1NameText = lobbyItem.transform.Find("Player1Name")?.GetComponent<TMP_Text>();
        TMP_Text player2NameText = lobbyItem.transform.Find("Player2Name")?.GetComponent<TMP_Text>();

        if (player1NameText != null)
            player1NameText.text = player1Name;
        if (player2NameText != null)
            player2NameText.text = player2Name;

        // Update player icons asynchronously
        Image player1Icon = lobbyItem.transform.Find("Player1Icon")?.GetComponent<Image>();
        Image player2Icon = lobbyItem.transform.Find("Player2Icon")?.GetComponent<Image>();

        // Start coroutine to download and set icons
        if (player1Icon != null)
            StartCoroutine(DownloadAndSetIcon(player1IconUrl, player1Icon));

        if (player2Icon != null)
            StartCoroutine(DownloadAndSetIcon(player2IconUrl, player2Icon));

        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(lobbyListParent.GetComponent<RectTransform>());
    }

    private IEnumerator DownloadAndSetIcon(string url, Image iconImage)
    {
        if (string.IsNullOrEmpty(url))
        {
            iconImage.sprite = null;
            yield break;
        }

        if (iconCache.ContainsKey(url))
        {
            Texture2D cachedTexture = iconCache[url];
            iconImage.sprite = Sprite.Create(cachedTexture, new Rect(0, 0, cachedTexture.width, cachedTexture.height), Vector2.zero);
            yield break;
        }

        // Start the download task
        Task<Texture2D> downloadTask = DownloadImageFromS3(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (downloadTask.Exception != null)
        {
            Debug.LogError($"Error downloading icon: {downloadTask.Exception.Message}");
            iconImage.sprite = null;
        }
        else
        {
            Texture2D texture = downloadTask.Result;
            if (texture != null)
            {
                iconCache[url] = texture;
                iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                iconImage.sprite = null;
            }
        }
    }

    private async Task<Texture2D> DownloadImageFromS3(string url)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = new Uri(url).LocalPath.TrimStart('/')
            };

            using (var response = await s3Client.GetObjectAsync(getRequest))
            using (var stream = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(stream);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(stream.ToArray());
                return texture;
            }
        }
        catch (AmazonS3Exception ex)
        {
            Debug.LogError($"Error downloading image from S3: {ex.Message}");
            return null;
        }
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

    public void JoinLobby(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreateLobby(string lobbyName, bool isPrivate)
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

        // Prepare initial room properties
        ExitGames.Client.Photon.Hashtable initialProperties = new ExitGames.Client.Photon.Hashtable();

        // Set the creator's data
        initialProperties["Player1Name"] = PhotonNetwork.NickName;
        initialProperties["Player1Icon"] = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Icon") ? PhotonNetwork.LocalPlayer.CustomProperties["Icon"] : "";
        initialProperties["Player2Name"] = "";
        initialProperties["Player2Icon"] = "";

        // Add password to custom properties if the lobby is private
        if (isPrivate && !string.IsNullOrEmpty(passwordInput.text))
        {
            initialProperties["Password"] = passwordInput.text;
        }
        else
        {
            initialProperties["Password"] = "";
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true, // Always set to true to show both public and private rooms
            CustomRoomProperties = initialProperties,
            CustomRoomPropertiesForLobby = new string[] { "Password", "Player1Name", "Player1Icon", "Player2Name", "Player2Icon" }
        };

        PhotonNetwork.CreateRoom(lobbyName, roomOptions);
        createLobbyPanel.SetActive(false);
        matchmakingPanel.SetActive(true); // Show the matchmaking panel again
    }

    public void TogglePublicPrivate()
    {
        isLobbyPrivate = !isLobbyPrivate;
        publicPrivateButton.GetComponentInChildren<TMP_Text>().text = isLobbyPrivate ? "Private" : "Public";
        passwordPanel.SetActive(isLobbyPrivate); // Show/hide password panel
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
        bool isPrivateRoom = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)PhotonNetwork.CurrentRoom.CustomProperties["Password"]);
        publicPrivateText.text = isPrivateRoom ? "Private" : "Public";
        UpdateLobbyDetails();
        UpdateRoomCustomProperties();

        lobbyPanel.SetActive(true);
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLobbyDetails();
        UpdateRoomCustomProperties();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyDetails();
        UpdateRoomCustomProperties();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        if (targetPlayer.IsLocal)
        {
            UpdatePlayerProfileIcon();
        }

        if (PhotonNetwork.InRoom && targetPlayer != null)
        {
            UpdateLobbyDetails();
            UpdateRoomCustomProperties();
        }
    }

    // New method to handle room property updates
    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        Debug.Log("Room properties updated.");
        UpdateLobbyDetails();
    }

    private async void UpdatePlayerProfileIcon()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Icon"))
        {
            string iconUrl = PhotonNetwork.LocalPlayer.CustomProperties["Icon"] as string;
            if (!string.IsNullOrEmpty(iconUrl))
            {
                if (iconCache.ContainsKey(iconUrl))
                {
                    Texture2D cachedTexture = iconCache[iconUrl];
                    profileIconImage.sprite = Sprite.Create(cachedTexture, new Rect(0, 0, cachedTexture.width, cachedTexture.height), Vector2.zero);
                    profileIconImage.sprite.name = iconUrl;
                }
                else
                {
                    Texture2D texture = await DownloadImageFromS3(iconUrl);
                    if (texture != null)
                    {
                        iconCache[iconUrl] = texture;
                        profileIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        profileIconImage.sprite.name = iconUrl;
                    }
                }
            }
        }
    }

    private async void UpdateLobbyDetails()
    {
        Player[] players = PhotonNetwork.PlayerList;

        player1Panel.SetActive(players.Length >= 1);
        player2Panel.SetActive(players.Length >= 2);

        if (players.Length > 0)
        {
            player1Username.text = players[0].NickName;
            string iconUrl = players[0].CustomProperties.ContainsKey("Icon") ? players[0].CustomProperties["Icon"] as string : "";

            if (!string.IsNullOrEmpty(iconUrl))
            {
                if (iconCache.ContainsKey(iconUrl))
                {
                    Texture2D cachedTexture = iconCache[iconUrl];
                    player1Icon.sprite = Sprite.Create(cachedTexture, new Rect(0, 0, cachedTexture.width, cachedTexture.height), Vector2.zero);
                }
                else
                {
                    Texture2D texture = await DownloadImageFromS3(iconUrl);
                    if (texture != null)
                    {
                        iconCache[iconUrl] = texture;
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
            string iconUrl = players[1].CustomProperties.ContainsKey("Icon") ? players[1].CustomProperties["Icon"] as string : "";

            if (!string.IsNullOrEmpty(iconUrl))
            {
                if (iconCache.ContainsKey(iconUrl))
                {
                    Texture2D cachedTexture = iconCache[iconUrl];
                    player2Icon.sprite = Sprite.Create(cachedTexture, new Rect(0, 0, cachedTexture.width, cachedTexture.height), Vector2.zero);
                }
                else
                {
                    Texture2D texture = await DownloadImageFromS3(iconUrl);
                    if (texture != null)
                    {
                        iconCache[iconUrl] = texture;
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

        // Handle waiting text
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            waitingText.gameObject.SetActive(true);
            if (waitingCoroutine == null)
            {
                waitingCoroutine = StartCoroutine(AnimateWaitingText());
            }
        }
        else
        {
            waitingText.gameObject.SetActive(false);
            if (waitingCoroutine != null)
            {
                StopCoroutine(waitingCoroutine);
                waitingCoroutine = null;
            }
        }
    }

    private IEnumerator AnimateWaitingText()
    {
        string baseText = "Waiting for one more player";
        string[] dots = { "", ".", "..", "..." };
        int index = 0;
        while (true)
        {
            waitingText.text = $"{baseText}{dots[index % dots.Length]}";
            index++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateRoomCustomProperties()
    {
        // Prepare the data to store
        ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();

        Player[] players = PhotonNetwork.PlayerList;

        // Loop through players and store their data
        for (int i = 0; i < players.Length; i++)
        {
            string playerNameKey = $"Player{i + 1}Name";
            string playerIconKey = $"Player{i + 1}Icon";

            roomProperties[playerNameKey] = players[i].NickName;

            if (players[i].CustomProperties.ContainsKey("Icon"))
            {
                string iconUrl = players[i].CustomProperties["Icon"] as string;
                roomProperties[playerIconKey] = iconUrl;
            }
            else
            {
                roomProperties[playerIconKey] = "";
            }
        }

        // Clear data for slots not occupied
        for (int i = players.Length; i < 2; i++)
        {
            string playerNameKey = $"Player{i + 1}Name";
            string playerIconKey = $"Player{i + 1}Icon";

            roomProperties[playerNameKey] = "";
            roomProperties[playerIconKey] = "";
        }

        // Update the room properties
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    public void LeaveLobby()
    {
        Debug.Log($"Player has left the room: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Player left the room.");

        // Clear lobby details
        player1Username.text = "";
        player2Username.text = "";
        player1Icon.sprite = null;
        player2Icon.sprite = null;
        playersConnectedText.text = "";
        ShowMatchmakingPanel();

        // Refresh the room list
        RefreshRoomList();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("LevelSelector");
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

    public void ShowMenuPanel()
    {
        mainMenuPanel.SetActive(true);
        matchmakingPanel.SetActive(false);
    }

    // Method to attempt joining a room with a password
    public void OnJoinRoomWithPasswordClicked()
    {
        string enteredPassword = joinPasswordInput.text;

        // Find the room info from cached list
        RoomInfo roomInfo = cachedRoomList.Find(r => r.Name == roomToJoin);

        if (roomInfo != null)
        {
            string roomPassword = roomInfo.CustomProperties.ContainsKey("Password") ? (string)roomInfo.CustomProperties["Password"] : "";

            if (enteredPassword == roomPassword)
            {
                // Password is correct
                PhotonNetwork.JoinRoom(roomToJoin);
                passwordInputPanel.SetActive(false);
            }
            else
            {
                // Show error message
                Debug.LogError("Incorrect password.");
                // Optionally, display this message in the UI
            }
        }
        else
        {
            Debug.LogError("Room not found.");
        }
    }

    // Method to cancel joining a room
    public void OnCancelJoinRoom()
    {
        passwordInputPanel.SetActive(false);
        roomToJoin = null;
    }

    // Method to refresh the room list UI
    public void RefreshRoomList()
    {
        // Clear existing room prefabs
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }

        // Recreate the room list UI
        foreach (RoomInfo room in cachedRoomList)
        {
            CreateRoomEntry(room);
        }
    }

    // Method to update the username text (if needed)
    public void UpdateUsernameText()
    {
        usernameText.text = PhotonNetwork.NickName;
    }
}
