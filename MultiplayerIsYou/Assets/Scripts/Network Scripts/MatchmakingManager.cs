using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Collections.Generic;

public class MatchmakingManager : MonoBehaviourPunCallbacks
{
    public GameObject matchmakingPanel;  
    public GameObject menuPanel;         
    public Button menuButton;            
    public GameObject createLobbyPanel;  

    public TMP_Text playerUsername;      
    public Image playerIcon;

    // Online lobbies (Photon)
    public Transform onlineLobbyList;    
    public GameObject onlineLobbyPrefab; 

    public Button createGameButton;     
    public Button joinGameButton;      

    private string selectedOnlineLobby;

    // Lobby creation settings
    public TMP_InputField lobbyNameInput;
    public Button publicButton;
    public Button privateButton;
    public GameObject passwordPanel;    
    public TMP_InputField passwordInput;
    public Button createButton;

    private List<RoomInfo> availableRooms = new List<RoomInfo>();  

    private bool isPrivate = false;      
    private string password = "";        
    private bool isConnectedToMaster = false;
    private bool pendingCreateLobby = false;

    public ProfileManager profileManager;
    public Sprite defaultPresetIcon;

    private void Start()
    {
        // Connect to the Photon server
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();  // This connects to Photon using the settings in the PhotonServerSettings file
        }

        // Set the player’s profile info
        playerUsername.text = PlayerPrefs.GetString("PlayerUsername", "Username");
        playerIcon.sprite = LoadPlayerIcon();

        // Add listeners to the buttons
        menuButton.onClick.AddListener(ReturnToMenu);
        createGameButton.onClick.AddListener(OpenCreateLobbyPanel);
        joinGameButton.onClick.AddListener(JoinSelectedLobby);
        publicButton.onClick.AddListener(SetPublicLobby);
        privateButton.onClick.AddListener(SetPrivateLobby);
        createButton.onClick.AddListener(CreateLobby);

        passwordPanel.SetActive(false);
    }

    private Sprite LoadPlayerIcon()
    {
        // Check for custom image
        string customImagePath = PlayerPrefs.GetString("CustomProfileImagePath", null);
        if (!string.IsNullOrEmpty(customImagePath) && File.Exists(customImagePath))
        {
            // Load custom image
            byte[] imageBytes = File.ReadAllBytes(customImagePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);
            Debug.Log("Custom icon loaded from: " + customImagePath);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        string selectedPresetIndex = PlayerPrefs.GetString("SelectedProfileIcon", "0"); 
        int iconIndex;
        if (int.TryParse(selectedPresetIndex, out iconIndex) && iconIndex >= 0 && iconIndex < profileManager.profileIconButtons.Length)
        {
            Debug.Log("Preset icon loaded from ProfileManager: Index " + iconIndex);
            return profileManager.profileIconButtons[iconIndex].sprite;
        }

        Debug.Log("Falling back to first preset icon in ProfileManager.");
        return profileManager.profileIconButtons[0].sprite; 
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                // Create a room entry in the UI
                GameObject roomEntry = Instantiate(onlineLobbyPrefab, onlineLobbyList);

                TMP_Text roomNameText = roomEntry.GetComponentInChildren<TMP_Text>(); 
                roomNameText.text = room.Name;

                TMP_Text playerCountText = roomEntry.transform.Find("PlayerCountText").GetComponent<TMP_Text>();
                playerCountText.text = room.PlayerCount + "/" + room.MaxPlayers;

                foreach (var player in PhotonNetwork.PlayerList)
                {
                    ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;

                    if (playerProperties.ContainsKey("PlayerIcon"))
                    {
                        string iconPath = playerProperties["PlayerIcon"].ToString();
                        Image playerIconImage = roomEntry.transform.Find("PlayerIcon").GetComponent<Image>();

                        if (iconPath == "Preset")
                        {
                            playerIconImage.sprite = defaultPresetIcon;
                        }
                        else
                        {
                            byte[] imageBytes = File.ReadAllBytes(iconPath);
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(imageBytes);
                            playerIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        }
                    }
                }

                roomEntry.GetComponent<Button>().onClick.AddListener(() => SelectOnlineLobby(room.Name));
            }
        }
    }

    public void SelectOnlineLobby(string lobbyName)
    {
        selectedOnlineLobby = lobbyName;
        Debug.Log("Selected Lobby: " + selectedOnlineLobby);
        joinGameButton.interactable = true;
    }

    public void JoinSelectedLobby()
    {
        if (!string.IsNullOrEmpty(selectedOnlineLobby))
        {
            PhotonNetwork.JoinRoom(selectedOnlineLobby);
        }
        else
        {
            Debug.LogError("No lobby selected!");
        }
    }

    public void OpenCreateLobbyPanel()
    {
        createLobbyPanel.SetActive(true);
        matchmakingPanel.SetActive(false);
    }

    public void CreateLobby()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Cannot create room, not connected to the Master Server!");
            return;
        }

        if (PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            Debug.LogWarning("Currently on Game Server, rejoining the lobby...");
            pendingCreateLobby = true;
            PhotonNetwork.JoinLobby();
            createButton.interactable = false;
            return;
        }

        AttemptCreateLobby();
    }

    private void AttemptCreateLobby()
    {
        string lobbyName = lobbyNameInput.text;

        if (isPrivate)
        {
            password = passwordInput.text;
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = !isPrivate,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "Password", password } }
        };

        PhotonNetwork.CreateRoom(lobbyName, options);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");

        if (pendingCreateLobby)
        {
            pendingCreateLobby = false;
            createButton.interactable = true;
            AttemptCreateLobby();
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room successfully created!");

        createLobbyPanel.SetActive(false);
        matchmakingPanel.SetActive(true);

        GameObject roomEntry = Instantiate(onlineLobbyPrefab, onlineLobbyList);

        TMP_Text roomNameText = roomEntry.GetComponentInChildren<TMP_Text>();
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        TMP_Text playerCountText = roomEntry.transform.Find("PlayerCountText").GetComponent<TMP_Text>();
        playerCountText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        Image playerIconImage = roomEntry.transform.Find("PlayerIcon").GetComponent<Image>();

        if (playerIconImage != null)
        {
            Debug.Log("PlayerIcon found in prefab, attempting to set sprite...");

            if (playerIcon.sprite != null)
            {
                playerIconImage.sprite = playerIcon.sprite;
                Debug.Log("PlayerIcon sprite set successfully.");
            }
            else
            {
                Debug.LogError("PlayerIcon sprite is null! Ensure the player icon is set correctly.");
            }
        }
        else
        {
            Debug.LogError("PlayerIcon Image component not found in the prefab!");
        }

        TMP_Text lobbyStatusText = roomEntry.transform.Find("lobbyStatusText").GetComponent<TMP_Text>();

        if (lobbyStatusText != null)
        {
            lobbyStatusText.text = isPrivate ? "Private" : "Public";
        }
    }

    public void SetPublicLobby()
    {
        isPrivate = false;
        password = "";

        publicButton.GetComponent<Image>().color = Color.green;
        privateButton.GetComponent<Image>().color = Color.white;

        passwordPanel.SetActive(false);
    }

    public void SetPrivateLobby()
    {
        isPrivate = true;

        privateButton.GetComponent<Image>().color = Color.green;
        publicButton.GetComponent<Image>().color = Color.white;

        passwordPanel.SetActive(true);
    }

    public void ReturnToMenu()
    {
        matchmakingPanel.SetActive(false);
        createLobbyPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("CreateRoom failed: " + message);
        createButton.interactable = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        isConnectedToMaster = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
{
    UpdatePlayerList(newPlayer);
}

    private void UpdatePlayerList(Player player)
{
    ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
    if (playerProperties != null)
    {
        string username = playerProperties.ContainsKey("PlayerUsername") ? playerProperties["PlayerUsername"].ToString() : "Unknown";
        string iconPath = playerProperties.ContainsKey("PlayerIcon") ? playerProperties["PlayerIcon"].ToString() : "Preset";

        TMP_Text playerNameText = onlineLobbyList.Find("PlayerNameText").GetComponent<TMP_Text>();
        Image playerIconImage = onlineLobbyList.Find("PlayerIcon").GetComponent<Image>();

        playerNameText.text = username;
        
        if (iconPath == "Preset")
        {
            playerIconImage.sprite = defaultPresetIcon;
        }
        else if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
        {
            byte[] imageBytes = File.ReadAllBytes(iconPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);
            playerIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
    }
}

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("PlayerIcon"))
        {
            string iconPath = changedProps["PlayerIcon"].ToString();
            Image playerIconImage = GetPlayerIconImage(targetPlayer);

            if (iconPath == "Preset")
            {
                playerIconImage.sprite = defaultPresetIcon;
            }
            else
            {
                byte[] imageBytes = File.ReadAllBytes(iconPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                playerIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }
    }

    private Image GetPlayerIconImage(Player player)
    {
        foreach (Transform child in onlineLobbyList)
        {
            TMP_Text playerNameText = child.Find("PlayerNameText").GetComponent<TMP_Text>();

            if (playerNameText != null && playerNameText.text == player.NickName)
            {
                Image playerIconImage = child.Find("PlayerIcon").GetComponent<Image>();
                if (playerIconImage != null)
                {
                    return playerIconImage;
                }
                else
                {
                    Debug.LogError("PlayerIcon Image component not found for player: " + player.NickName);
                }
            }
        }

        Debug.LogError("No matching player entry found for: " + player.NickName);
        return null;
    }
}
