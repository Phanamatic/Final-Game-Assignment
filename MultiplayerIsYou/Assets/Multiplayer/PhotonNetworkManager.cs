using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player1; 
    public GameObject player2;

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
            AssignPlayerControl(player1, "Player 1");
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            AssignPlayerControl(player2, "Player 2");
        }
        else
        {
            Debug.LogError("Unexpected number of players.");
        }
    }

    private void AssignPlayerControl(GameObject playerObject, string playerName)
{
    if (playerObject != null)
    {
        PhotonView photonView = playerObject.GetComponent<PhotonView>();
        if (photonView != null)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            Debug.Log($"{playerName} assigned ownership.");

            if (playerName == "Player 1")
                playerObject.GetComponent<Player1Controller>().enabled = true;
            else if (playerName == "Player 2")
                playerObject.GetComponent<Player2Controller>().enabled = true;
        }
        else
        {
            Debug.LogError($"PhotonView not found on {playerObject.name}!");
        }
    }
    else
    {
        Debug.LogError($"{playerName} GameObject not assigned.");
    }
}

}
