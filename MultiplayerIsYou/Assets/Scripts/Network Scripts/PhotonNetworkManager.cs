using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player1Prefab; // Assign Player 1 prefab in the inspector
    public GameObject player2Prefab; // Assign Player 2 prefab in the inspector
    public Transform player1SpawnPoint; // Assign Player 1 spawn point in the inspector
    public Transform player2SpawnPoint; // Assign Player 2 spawn point in the inspector
    public Camera camera1; // Assign Player 1 camera in the inspector
    public Camera camera2; // Assign Player 2 camera in the inspector

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true; // Ensure scene synchronization
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        PhotonNetwork.JoinOrCreateRoom(
            "TestRoom",
            new RoomOptions { MaxPlayers = 2 },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
{
    Debug.Log($"Joined Room as Player {PhotonNetwork.LocalPlayer.ActorNumber}");

    GameObject playerInstance;
    if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
    {
        playerInstance = PhotonNetwork.Instantiate(player1Prefab.name, player1SpawnPoint.position, player1SpawnPoint.rotation);
        camera1.gameObject.SetActive(true); // Enable Player 1's camera
        camera2.gameObject.SetActive(false); // Disable Player 2's camera
    }
    else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
    {
        playerInstance = PhotonNetwork.Instantiate(player2Prefab.name, player2SpawnPoint.position, player2SpawnPoint.rotation);
        camera2.gameObject.SetActive(true); // Enable Player 2's camera
        camera1.gameObject.SetActive(false); // Disable Player 1's camera
    }
    else
    {
        Debug.LogError("Unexpected number of players.");
        return;
    }

    // Assign PhotonView to ensure ownership and synchronization
    PhotonView photonView = playerInstance.GetComponent<PhotonView>();
    if (photonView != null && photonView.IsMine)
    {
        Debug.Log("Player instance successfully instantiated and assigned.");
    }
}

    private void AssignCameraToLocalPlayer(GameObject playerInstance)
    {
        Camera[] cameras = playerInstance.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(true);
            }
        }
    }
}
