using Photon.Pun;
using UnityEngine;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    void Start()
    {
        // Connect to the Photon Server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // Automatically join a random room once connected to the master server
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // If there is no room is available then create a new one
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 10 });
    }

    public override void OnJoinedRoom()
    {
        // Spawn player once the player has joined the room
        Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f); // Set Random spawn position
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
    }
}
