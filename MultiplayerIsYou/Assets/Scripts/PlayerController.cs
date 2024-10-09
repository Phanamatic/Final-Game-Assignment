using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    private Rigidbody2D rb;
    private Vector2 networkPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (!photonView.IsMine)
        {
            // Disable the input control for non-local players
            GetComponent<PlayerMovement>().enabled = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Synchronizes the player's position across the network
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
        }
        else
        {
            networkPosition = (Vector2)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            rb.position = Vector2.MoveTowards(rb.position, networkPosition, Time.deltaTime * 10);
        }
    }
}
