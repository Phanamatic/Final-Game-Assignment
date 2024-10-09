using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
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

    // Local lobbies (Mirror)
    public Transform localLobbyList;     
    public GameObject localLobbyPrefab;  

    // Online lobbies (Photon)
    public Transform onlineLobbyList;    
    public GameObject onlineLobbyPrefab; 

    public Button createGameButton;     
    public Button joinGameButton;      

    private string selectedLocalLobby;
    private string selectedOnlineLobby;

    // Lobby creation settings
    public TMP_InputField lobbyNameInput;
    public Button publicButton;
    public Button privateButton;
    public GameObject passwordPanel;    
    public TMP_InputField passwordInput;
    public Button createButton;

    // Online/Local room creation buttons
    public Button onlineButton;
    public Button localButton;

    private List<RoomInfo> availableRooms = new List<RoomInfo>();  

    private bool isPrivate = false;      
    private bool isOnline = true;       
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
        onlineButton.onClick.AddListener(SetOnlineRoom);
        localButton.onClick.AddListener(SetLocalRoom);

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

    string selectedPresetIndex = PlayerPrefs.GetString("SelectedProfileIcon", "0");         // NEED TO CHANGE THIS TO DEFAULT TO PLAYER SELECTED ICON INSTEAD OF DEFAULT
    int iconIndex;
    if (int.TryParse(selectedPresetIndex, out iconIndex) && iconIndex >= 0 && iconIndex < profileManager.profileIconButtons.Length)
    {
        Debug.Log("Preset icon loaded from ProfileManager: Index " + iconIndex);
        return profileManager.profileIconButtons[iconIndex].sprite;
    }

    Debug.Log("Falling back to first preset icon in ProfileManager.");
    return profileManager.profileIconButtons[0].sprite; 
}




    // --- Local Lobbies (Mirror) ---
    public void PopulateLocalLobbies()
    {
        // Clear the current list
        foreach (Transform child in localLobbyList)
        {
            Destroy(child.gameObject);
        }

        // Need to implement
    }

    // --- Online Lobbies (Photon) ---
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
{
    foreach (RoomInfo room in roomList)
    {
        if (!room.RemovedFromList)
        {
            // Create a room entry in the UI
            GameObject roomEntry = Instantiate(onlineLobbyPrefab, onlineLobbyList);

            TMP_Text roomNameText = roomEntry.GetComponentInChildren<TMP_Text>();  //Get the room name text from the prefab
            roomNameText.text = room.Name;  // set the room name

            // Set up the player count and set the default values
            TMP_Text playerCountText = roomEntry.transform.Find("PlayerCountText").GetComponent<TMP_Text>();
            playerCountText.text = room.PlayerCount + "/" + room.MaxPlayers;

            // Check for custom player properties (icons and usernames)
            foreach (var player in PhotonNetwork.PlayerList)
            {
                ExitGames.Client.Photon.Hashtable playerProperties = player.CustomProperties;
                
                // If the player has an icon set, retrieve it and set it
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
                        // Load custom player icon
                        byte[] imageBytes = File.ReadAllBytes(iconPath);
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(imageBytes);
                        playerIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    }
                }
            }

            // Attach the join functionality --- Need to implement this
            roomEntry.GetComponent<Button>().onClick.AddListener(() => SelectOnlineLobby(room.Name));
        }
    }
}


    public void SelectOnlineLobby(string lobbyName)
{
    // Set the current selected lobby to join
    selectedOnlineLobby = lobbyName;
    Debug.Log("Selected Lobby: " + selectedOnlineLobby);

    // Enable the Join Game button
    joinGameButton.interactable = true;
}

    public void JoinSelectedLobby()
{
    if (!string.IsNullOrEmpty(selectedOnlineLobby))     // NEED TO FURTHER IMPLEMENT --- Join lobby system
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
        // Ensure we're connected to the photon master server before creating a room
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Cannot create room, not connected to the Master Server!");
            return;
        }

        // Ensure we're not on the Game Server, if so, return to the lobby
        if (PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            Debug.LogWarning("Currently on Game Server, rejoining the lobby...");
            pendingCreateLobby = true;  // Set flag to retry after rejoining the lobby
            PhotonNetwork.JoinLobby();  // Rejoin the lobby on the Master Server

            // Disable the "Create" button while rejoining the lobby        ---  ** Need to reenable and also add a leave lobby button once lobby si created  **
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
            password = passwordInput.text;  // Set the lobby password if set as private
        }

        // Create the lobby using Photon
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = !isPrivate,  // Set visibility based on public/private setting  --- shoudl work but is not currently
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

   public override void OnCreatedRoom()    // Fill the lobby info in the lobby prefab
{
    Debug.Log("Room successfully created!");

    createLobbyPanel.SetActive(false);
    matchmakingPanel.SetActive(true);

    GameObject roomEntry;
    TMP_Text lobbyStatusText;

    if (isOnline)
    {
        roomEntry = Instantiate(onlineLobbyPrefab, onlineLobbyList);
    }
    else
    {
        roomEntry = Instantiate(localLobbyPrefab, localLobbyList);
    }

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

    lobbyStatusText = roomEntry.transform.Find("lobbyStatusText").GetComponent<TMP_Text>();

    if (lobbyStatusText != null)
    {
        lobbyStatusText.text = isPrivate ? "Private" : "Public";
    }
}


    public void SetPublicLobby()   // Handles public lobby button press
    {
        isPrivate = false;
        password = "";

        publicButton.GetComponent<Image>().color = Color.green; 
        privateButton.GetComponent<Image>().color = Color.white; 

        passwordPanel.SetActive(false);
    }

    public void SetPrivateLobby()  // Handles private lobby button press
    {
        isPrivate = true;

        privateButton.GetComponent<Image>().color = Color.green;  
        publicButton.GetComponent<Image>().color = Color.white; 

        passwordPanel.SetActive(true);
    }

    public void SetOnlineRoom()  //Handle online button press
    {
        isOnline = true;

        onlineButton.GetComponent<Image>().color = Color.green;
        localButton.GetComponent<Image>().color = Color.white;
    }

    public void SetLocalRoom()  // Handle local button press
    {
        isOnline = false;

        localButton.GetComponent<Image>().color = Color.green;
        onlineButton.GetComponent<Image>().color = Color.white;
    }

    public void ReturnToMenu()
    {
        matchmakingPanel.SetActive(false); 
        createLobbyPanel.SetActive(false); 
        menuPanel.SetActive(true);         
    }

    public override void OnCreateRoomFailed(short returnCode, string message)   // Incase room creation fails
    {
        Debug.LogError("CreateRoom failed: " + message);
        createButton.interactable = true; 
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        isConnectedToMaster = true;
        PhotonNetwork.JoinLobby();  // Automatically join a lobby once connected  --- Need to change this so that the player is on the master and joins a lobby through the lobby screen
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
{
    ExitGames.Client.Photon.Hashtable playerProperties = newPlayer.CustomProperties;   // Retrieves a new players custom properties (username, icon, etc)

    if (playerProperties.ContainsKey("PlayerIcon"))    // set icon on lobby prefab
    {
        string iconPath = playerProperties["PlayerIcon"].ToString();
        Image playerIconImage = GetPlayerIconImage(newPlayer);
        
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

public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)   // For player changing icons and usernames
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

private Image GetPlayerIconImage(Player player)   // To set the player icon in lobbies  ** Not working currently
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
