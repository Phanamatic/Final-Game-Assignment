using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Cameras")]
    public Camera player1Camera; // Assign Player 1 camera in the inspector
    public Camera player2Camera; // Assign Player 2 camera in the inspector

    [Header("Player Prefabs")]
    public GameObject player1Prefab; // Assign Player 1 prefab in the inspector (Baba)
    public GameObject player2Prefab; // Assign Player 2 prefab in the inspector (Lala)

    [Header("Spawn Points")]
    public Transform player1SpawnPoint; // Assign Player 1 spawn point in the inspector
    public Transform player2SpawnPoint; // Assign Player 2 spawn point in the inspector

    private static PhotonNetworkManager instance;
    private bool hasInstantiated = false; // Flag to prevent multiple instantiations

    private void Awake()
    {
        // Implement Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // Set Photon log level to Full for detailed debugging
        PhotonNetwork.LogLevel = PunLogLevel.Full;

        // Ensure scene synchronization is enabled
        PhotonNetwork.AutomaticallySyncScene = true;

        // Only instantiate the player if connected and in a room
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Check if "PlayerRole" is already assigned
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerRole"))
            {
                object roleObj;
                PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerRole", out roleObj);

                if (roleObj != null)
                {
                    string playerRole = roleObj.ToString();
                    Debug.Log($"[PhotonNetworkManager] PlayerRole found in Start(): {playerRole}");
                    InstantiatePlayer(playerRole);
                }
                else
                {
                    Debug.LogError("[PhotonNetworkManager] PlayerRole exists but is null.");
                }
            }
            else
            {
                Debug.Log("[PhotonNetworkManager] PlayerRole not assigned yet. Assigning now.");
                AssignPlayerRole();
                // Note: Instantiation will be handled in OnPlayerPropertiesUpdate
            }
        }
        else if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[PhotonNetworkManager] Connecting to Photon using settings...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonNetworkManager] Connected to Master Server.");

        // Join the lobby if not already in a room
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("[PhotonNetworkManager] Joining Lobby...");
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[PhotonNetworkManager] Joined Lobby.");
        // Join or create a room
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // If no random room is available, create a new one
        Debug.Log("[PhotonNetworkManager] No random room available, creating a new room.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PhotonNetworkManager] Joined Room as Player {PhotonNetwork.LocalPlayer.ActorNumber}");

        // Assign player roles if not already assigned
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PlayerRole"))
        {
            AssignPlayerRole();
            // Note: Instantiation will be handled in OnPlayerPropertiesUpdate
        }
        else
        {
            // If already has a role, instantiate immediately
            object roleObj;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerRole", out roleObj);

            if (roleObj != null)
            {
                string playerRole = roleObj.ToString();
                Debug.Log($"[PhotonNetworkManager] PlayerRole found in OnJoinedRoom(): {playerRole}");
                InstantiatePlayer(playerRole);
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] PlayerRole exists but is null in OnJoinedRoom().");
            }
        }
    }

    private void AssignPlayerRole()
    {
        // Get a list of existing player roles
        List<string> existingRoles = new List<string>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("PlayerRole"))
            {
                existingRoles.Add(player.CustomProperties["PlayerRole"].ToString());
            }
        }

        // Assign role based on existing roles
        string assignedRole = "Player1";
        if (existingRoles.Contains("Player1"))
        {
            assignedRole = "Player2";
        }

        // Set the custom property
        PhotonHashtable props = new PhotonHashtable { { "PlayerRole", assignedRole } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"[PhotonNetworkManager] Assigned PlayerRole: {assignedRole}");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        // Check if the updated properties include "PlayerRole" for the local player
        if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey("PlayerRole"))
        {
            object roleObj;
            targetPlayer.CustomProperties.TryGetValue("PlayerRole", out roleObj);

            if (roleObj != null)
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
                Debug.LogError("[PhotonNetworkManager] player1Prefab is not assigned in the inspector.");
                return;
            }

            if (player1SpawnPoint == null)
            {
                Debug.LogError("[PhotonNetworkManager] player1SpawnPoint is not assigned in the inspector.");
                return;
            }

            playerInstance = PhotonNetwork.Instantiate(player1Prefab.name, player1SpawnPoint.position, player1SpawnPoint.rotation);
            Debug.Log("[PhotonNetworkManager] Instantiated Player1 prefab.");

            // Enable Player 1 camera and disable Player 2 camera
            if (player1Camera != null)
            {
                player1Camera.gameObject.SetActive(true);
                Debug.Log("[PhotonNetworkManager] Player1 camera enabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player1Camera is not assigned in the inspector.");
            }

            if (player2Camera != null)
            {
                player2Camera.gameObject.SetActive(false);
                Debug.Log("[PhotonNetworkManager] Player2 camera disabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player2Camera is not assigned in the inspector.");
            }
        }
        else if (playerRole == "Player2")
        {
            if (player2Prefab == null)
            {
                Debug.LogError("[PhotonNetworkManager] player2Prefab is not assigned in the inspector.");
                return;
            }

            if (player2SpawnPoint == null)
            {
                Debug.LogError("[PhotonNetworkManager] player2SpawnPoint is not assigned in the inspector.");
                return;
            }

            playerInstance = PhotonNetwork.Instantiate(player2Prefab.name, player2SpawnPoint.position, player2SpawnPoint.rotation);
            Debug.Log("[PhotonNetworkManager] Instantiated Player2 prefab.");

            // Enable Player 2 camera and disable Player 1 camera
            if (player2Camera != null)
            {
                player2Camera.gameObject.SetActive(true);
                Debug.Log("[PhotonNetworkManager] Player2 camera enabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player2Camera is not assigned in the inspector.");
            }

            if (player1Camera != null)
            {
                player1Camera.gameObject.SetActive(false);
                Debug.Log("[PhotonNetworkManager] Player1 camera disabled.");
            }
            else
            {
                Debug.LogError("[PhotonNetworkManager] player1Camera is not assigned in the inspector.");
            }
        }
        else
        {
            Debug.LogError("[PhotonNetworkManager] Unknown player role.");
            return;
        }

        // Assign PhotonView to ensure ownership and synchronization
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

    // Method to return both players to the LevelSelector scene
    public void ReturnToLevelSelector()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PhotonNetworkManager] Master Client initiating scene change to LevelSelector.");
            // The Master Client initiates the scene change
            PhotonNetwork.LoadLevel("LevelSelector");
        }
        else
        {
            Debug.LogWarning("[PhotonNetworkManager] Only the Master Client can initiate scene changes.");
        }
    }

    // Add an OnClick method to be called by a UI button
    public void OnReturnToLevelSelectorButtonClicked()
    {
        ReturnToLevelSelector();
    }
}
