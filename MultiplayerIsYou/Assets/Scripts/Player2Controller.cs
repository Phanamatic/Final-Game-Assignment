using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player2Controller : MonoBehaviourPunCallbacks
{
    private Vector3 moveDirection;
    public float pushDistance = 1f; // Distance to push objects

    void Update()
    {
        // Ensure only the owner can control the GameObject
        if (photonView.IsMine && gameObject.CompareTag("You2"))
        {
            HandleMovementInput();
        }
    }

    // Handle movement input from the player
    void HandleMovementInput()
    {
        moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow)) moveDirection = Vector3.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) moveDirection = Vector3.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) moveDirection = Vector3.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) moveDirection = Vector3.right;

        if (moveDirection != Vector3.zero)
        {
            MovePlayer();
        }
    }

    // Move the player by 1 unit
    void MovePlayer()
    {
        Vector3 targetPosition = transform.position + moveDirection;
        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

        if (hitCollider != null)
        {
            if (hitCollider.CompareTag("Stop"))
            {
                Debug.Log("Blocked by a Stop object!");
                return;
            }
            else if (IsPushable(hitCollider.gameObject))
            {
                if (CanPushChain(hitCollider.gameObject, moveDirection))
                {
                    PushObject(hitCollider.gameObject, moveDirection);
                    transform.position = targetPosition; // Move the player after pushing
                }
                else
                {
                    Debug.Log("Cannot push the chain due to blocking objects!");
                }
            }
            else
            {
                transform.position = targetPosition; // Move the player if no pushable object or stop tag
            }
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    // Check if the entire chain of pushable objects can be pushed
    bool CanPushChain(GameObject firstObject, Vector3 direction)
    {
        Queue<GameObject> toPush = new Queue<GameObject>();
        toPush.Enqueue(firstObject);

        while (toPush.Count > 0)
        {
            GameObject obj = toPush.Dequeue();
            Vector3 targetPosition = obj.transform.position + direction * pushDistance;

            Collider2D pushBlockCheck = Physics2D.OverlapCircle(targetPosition, 0.1f);
            if (pushBlockCheck != null)
            {
                // Block pushing if encountering "Stop" or "Shut" objects without "Open" or "OpenAndPush" tags
                if (pushBlockCheck.CompareTag("Stop")) return false;
                if (pushBlockCheck.CompareTag("Shut") && !HasOpenTag(obj)) return false;
            }

            Collider2D adjacentCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);
            if (adjacentCollider != null && IsPushable(adjacentCollider.gameObject))
            {
                if (!toPush.Contains(adjacentCollider.gameObject))
                {
                    toPush.Enqueue(adjacentCollider.gameObject);
                }
            }
        }
        return true;
    }

    // Push the object and handle pushing of adjacent pushable objects
    void PushObject(GameObject obj, Vector3 direction)
    {
        Vector3 pushTargetPosition = obj.transform.position + direction * pushDistance;

        // Move the object on all clients
        photonView.RPC("RPC_MoveObject", RpcTarget.All, obj.GetComponent<PhotonView>().ViewID, pushTargetPosition);

        // Check for adjacent pushable objects and push them
        Collider2D adjacentCollider = Physics2D.OverlapCircle(pushTargetPosition, 0.1f);
        if (adjacentCollider != null && IsPushable(adjacentCollider.gameObject))
        {
            PushObject(adjacentCollider.gameObject, direction);
        }

        // Destroy Shut objects if pushed into them by Open or OpenAndPush objects
        Collider2D shutCollider = Physics2D.OverlapCircle(pushTargetPosition, 0.1f);
        if (shutCollider != null && shutCollider.CompareTag("Shut") && HasOpenTag(obj))
        {
            photonView.RPC("RPC_DestroyObject", RpcTarget.All, shutCollider.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    void RPC_MoveObject(int objectViewID, Vector3 newPosition)
    {
        PhotonView objView = PhotonView.Find(objectViewID);
        if (objView != null)
        {
            objView.transform.position = newPosition;
        }
    }

    [PunRPC]
    void RPC_DestroyObject(int objectViewID)
    {
        PhotonView objView = PhotonView.Find(objectViewID);
        if (objView != null)
        {
            Destroy(objView.gameObject);
        }
    }

    // Check if the object is pushable
    bool IsPushable(GameObject obj)
    {
        // Combine logic from both scripts
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Word")) return true; // Check for "Word" tag in children (from old script)
        }
        return obj.CompareTag("Push") || obj.CompareTag("OpenAndPush"); // Retain new logic
    }

    // Check if the object has "Open" or "OpenAndPush" tags
    bool HasOpenTag(GameObject obj)
    {
        return obj.CompareTag("Open") || obj.CompareTag("OpenAndPush");
    }
}
