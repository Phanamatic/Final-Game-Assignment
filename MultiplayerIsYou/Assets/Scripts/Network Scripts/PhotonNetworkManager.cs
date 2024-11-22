using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Cameras")]
    public Camera player1Camera;
    public Camera player2Camera; 

    [Header("Player Prefabs")]
    public GameObject player1Prefab; //Baba
    public GameObject player2Prefab; //Lala

    [Header("Spawn Points")]
    public Transform player1SpawnPoint; 
    public Transform player2SpawnPoint;

    private bool hasInstantiated = false; 

    private void Awake()
    {
        PhotonNetwork.LogLevel = PunLogLevel.Full;

        // Ensure AutomaticallySyncScene is set before connecting
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"[PhotonNetworkManager] AutomaticallySyncScene set to {PhotonNetwork.AutomaticallySyncScene}");

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            AssignPlayerRoleIfNeeded();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[PhotonNetworkManager] Connecting to Photon using settings...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PhotonNetworkManager] Scene loaded: {scene.name}");

        // Reset instantiation flag
        hasInstantiated = false;

        // Destroy existing player GameObjects
        DestroyExistingPlayers();

        // Re-assign player roles and instantiate players
        AssignPlayerRoleIfNeeded();
    }

    private void DestroyExistingPlayers()
    {
        Debug.Log("[PhotonNetworkManager] Destroying existing player GameObjects.");

        // Find all player GameObjects with PhotonViews owned by this client and destroy them
        foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView pv = go.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(go);
                Debug.Log($"[PhotonNetworkManager] Destroyed player GameObject: {go.name}");
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonNetworkManager] Connected to Master Server.");

        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("[PhotonNetworkManager] Joining Lobby...");
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[PhotonNetworkManager] Joined Lobby.");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("[PhotonNetworkManager] No random room available, creating a new room.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PhotonNetworkManager] Joined Room: {PhotonNetwork.CurrentRoom.Name} as Player {PhotonNetwork.LocalPlayer.ActorNumber}");

        // Assign player role if needed
        AssignPlayerRoleIfNeeded();

        // Load the appropriate level
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("CurrentLevel", out object levelName))
        {
            if (SceneManager.GetActiveScene().name != (string)levelName)
            {
                PhotonNetwork.LoadLevel((string)levelName);
            }
        }
    }

    private void AssignPlayerRoleIfNeeded()
    {
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerRole"))
        {
            string assignedRole = PhotonNetwork.IsMasterClient ? "Player1" : "Player2";
            PhotonHashtable props = new PhotonHashtable { { "PlayerRole", assignedRole } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log($"[PhotonNetworkManager] Assigned PlayerRole: {assignedRole}");
            // Player instantiation will be handled in OnPlayerPropertiesUpdate
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerRole", out object roleObj))
            {
                string playerRole = roleObj.ToString();
                Debug.Log($"[PhotonNetworkManager] PlayerRole found: {playerRole}");
                InstantiatePlayer(playerRole);
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] PlayerRole exists but is null.");
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey("PlayerRole"))
        {
            if (changedProps.TryGetValue("PlayerRole", out object roleObj) && roleObj != null)
            {
                string playerRole = roleObj.ToString();
                Debug.Log($"[PhotonNetworkManager] PlayerRole updated: {playerRole}");

                if (!hasInstantiated)
                {
                    InstantiatePlayer(playerRole);
                }
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] PlayerRole exists but is null in OnPlayerPropertiesUpdate().");
            }
        }
    }

    private void InstantiatePlayer(string playerRole)
    {
        if (hasInstantiated)
        {
            Debug.LogWarning("[PhotonNetworkManager] Player has already been instantiated.");
            return;
        }

        GameObject playerInstance = null;

        if (playerRole == "Player1")
        {
            if (player1Prefab == null)
            {
                Debug.LogError("[PhotonNetworkManager] player1Prefab is not assigned in the Inspector.");
                return;
            }

            if (player1SpawnPoint == null)
            {
                Debug.LogError("[PhotonNetworkManager] player1SpawnPoint is not assigned in the Inspector.");
                return;
            }

            playerInstance = PhotonNetwork.Instantiate(player1Prefab.name, player1SpawnPoint.position, player1SpawnPoint.rotation);
            Debug.Log("[PhotonNetworkManager] Instantiated Player1 prefab.");

            if (player1Camera != null)
            {
                player1Camera.gameObject.SetActive(true);
                Debug.Log("[PhotonNetworkManager] Player1 camera enabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player1Camera is not assigned in the Inspector.");
            }

            if (player2Camera != null)
            {
                player2Camera.gameObject.SetActive(false);
                Debug.Log("[PhotonNetworkManager] Player2 camera disabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player2Camera is not assigned in the Inspector.");
            }
        }
        else if (playerRole == "Player2")
        {
            if (player2Prefab == null)
            {
                Debug.LogError("[PhotonNetworkManager] player2Prefab is not assigned in the Inspector.");
                return;
            }

            if (player2SpawnPoint == null)
            {
                Debug.LogError("[PhotonNetworkManager] player2SpawnPoint is not assigned in the Inspector.");
                return;
            }

            playerInstance = PhotonNetwork.Instantiate(player2Prefab.name, player2SpawnPoint.position, player2SpawnPoint.rotation);
            Debug.Log("[PhotonNetworkManager] Instantiated Player2 prefab.");

            if (player2Camera != null)
            {
                player2Camera.gameObject.SetActive(true);
                Debug.Log("[PhotonNetworkManager] Player2 camera enabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player2Camera is not assigned in the Inspector.");
            }

            if (player1Camera != null)
            {
                player1Camera.gameObject.SetActive(false);
                Debug.Log("[PhotonNetworkManager] Player1 camera disabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player1Camera is not assigned in the Inspector.");
            }
        }
        else
        {
            Debug.LogError("[PhotonNetworkManager] Unknown player role.");
            return;
        }

        if (playerInstance != null)
        {
            PhotonView photonView = playerInstance.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                Debug.Log("[PhotonNetworkManager] Player instance successfully instantiated and PhotonView is mine.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] PhotonView is missing or not owned by the local player.");
            }

            hasInstantiated = true;
        }
        else
        {
            Debug.LogError("[PhotonNetworkManager] Player instance is null after instantiation.");
        }
    }

    public void ReturnToLevelSelector()
    {
            PhotonNetwork.LoadLevel("LevelSelector");

    }

    public void OnReturnToLevelSelectorButtonClicked()
    {
        ReturnToLevelSelector();
    }
}
