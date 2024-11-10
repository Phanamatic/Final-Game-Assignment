using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player1Prefab; // Assign Player 1 prefab in the inspector
    public GameObject player2Prefab; // Assign Player 2 prefab in the inspector
    public Transform player1SpawnPoint; // Assign Player 1 spawn point in the inspector
    public Transform player2SpawnPoint; // Assign Player 2 spawn point in the inspector

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
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

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            // Instantiate Player 1 at the designated spawn point
            PhotonNetwork.Instantiate(player1Prefab.name, player1SpawnPoint.position, player1SpawnPoint.rotation);
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            // Instantiate Player 2 at the designated spawn point
            PhotonNetwork.Instantiate(player2Prefab.name, player2SpawnPoint.position, player2SpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("Unexpected number of players.");
        }
    }
}
