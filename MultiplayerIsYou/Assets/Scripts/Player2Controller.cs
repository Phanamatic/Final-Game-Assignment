using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player2Controller : MonoBehaviourPun
{
    private Vector3 moveDirection;
    public float pushDistance = 1f;
    private bool isMoving = false;

    private AudioSource audioSource;

    void Start()
    {
        if (photonView.IsMine)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (PauseManager.Instance != null && PauseManager.Instance.isGamePaused)
        {
            return; 
        }

        if (gameObject.CompareTag("You2"))
        {
            HandleMovementInput();
        }
    }

    void HandleMovementInput()
    {
        if (isMoving)
        {
            return;
        }

        moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W)) moveDirection = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.S)) moveDirection = Vector3.down;
        else if (Input.GetKeyDown(KeyCode.A)) moveDirection = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.D)) moveDirection = Vector3.right;
        else
        {
            return;
        }

        if (moveDirection != Vector3.zero)
        {
            MovePlayer();
        }
    }

    void MovePlayer()
    {
        isMoving = true;

        Vector3 targetPosition = transform.position + moveDirection;

        targetPosition = SnapToGrid(targetPosition);

        if (IsMovementBlocked(targetPosition))
        {
            isMoving = false;
            return;
        }

        transform.position = targetPosition;

        photonView.RPC("SyncMovePlayer", RpcTarget.Others, targetPosition);

        isMoving = false;
    }

    [PunRPC]
    void SyncMovePlayer(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    bool IsMovementBlocked(Vector3 targetPosition)
    {
        Collider2D shutCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

        if (shutCollider != null && shutCollider.CompareTag("Shut"))
        {
            if (!HasOpenAndPushTag(shutCollider.gameObject))
            {
                Debug.Log("Blocked by a Shut object!");
                return true;
            }
        }

        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

        if (hitCollider != null)
        {
            if (hitCollider.CompareTag("Stop"))
            {
                Debug.Log("Blocked by a Stop object!");
                return true;
            }
            else if (IsPushable(hitCollider.gameObject))
            {
                if (CanPushChain(hitCollider.gameObject, moveDirection))
                {
                    PushObject(hitCollider.gameObject, moveDirection);
                }
                else
                {
                    Debug.Log("Cannot push the chain due to blocking objects!");
                    return true;
                }
            }
        }

        return false;
    }

    void PushObject(GameObject obj, Vector3 direction)
    {
        List<GameObject> chain = GetPushChain(obj, direction);

        int[] viewIDs = new int[chain.Count];
        for (int i = 0; i < chain.Count; i++)
        {
            PhotonView pv = chain[i].GetComponent<PhotonView>();
            if (pv != null)
            {
                viewIDs[i] = pv.ViewID;
            }
            else
            {
                Debug.LogError("No PhotonView found on pushable object.");
            }
        }

        foreach (GameObject chainObj in chain)
        {
            Vector3 targetPosition = chainObj.transform.position + direction * pushDistance;

            Collider2D shutCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

            if (shutCollider != null && shutCollider.CompareTag("Shut"))
            {
                if (chainObj.CompareTag("OpenAndPush"))
                {
                    PhotonNetwork.Destroy(shutCollider.gameObject);
                    PhotonNetwork.Destroy(chainObj);
                    Debug.Log($"Shut and {chainObj.name} destroyed at {targetPosition}!");
                    return;
                }
            }

            chainObj.transform.position = targetPosition;
        }

        photonView.RPC("SyncPushObjects", RpcTarget.Others, viewIDs, direction);
    }

    [PunRPC]
    void SyncPushObjects(int[] viewIDs, Vector3 direction)
    {
        foreach (int viewID in viewIDs)
        {
            PhotonView pv = PhotonView.Find(viewID);
            if (pv != null)
            {
                GameObject chainObj = pv.gameObject;

                Vector3 targetPosition = chainObj.transform.position + direction * pushDistance;

                Collider2D shutCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

                if (shutCollider != null && shutCollider.CompareTag("Shut"))
                {
                    if (chainObj.CompareTag("OpenAndPush"))
                    {
                        PhotonNetwork.Destroy(shutCollider.gameObject);
                        PhotonNetwork.Destroy(chainObj);
                        Debug.Log($"Shut and {chainObj.name} destroyed at {targetPosition}!");
                        return;
                    }
                }

                chainObj.transform.position = targetPosition;
            }
            else
            {
                Debug.LogError($"No PhotonView found for ViewID {viewID}");
            }
        }
    }

    List<GameObject> GetPushChain(GameObject obj, Vector3 direction)
    {
        List<GameObject> chain = new List<GameObject>();
        Vector3 nextPosition = obj.transform.position;

        while (true)
        {
            Collider2D hit = Physics2D.OverlapCircle(nextPosition, 0.1f);
            if (hit != null && IsPushable(hit.gameObject))
            {
                chain.Add(hit.gameObject);
                nextPosition += direction * pushDistance;
            }
            else
            {
                break;
            }
        }

        return chain;
    }

    bool CanPushChain(GameObject firstObject, Vector3 direction)
    {
        List<GameObject> chain = GetPushChain(firstObject, direction);

        foreach (GameObject chainObj in chain)
        {
            Vector3 targetPosition = chainObj.transform.position + direction * pushDistance;
            Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

            if (hitCollider != null)
            {
                if (hitCollider.CompareTag("Stop"))
                {
                    Debug.Log($"Chain blocked at {targetPosition} by Stop.");
                    return false;
                }
                else if (hitCollider.CompareTag("Shut") && !chainObj.CompareTag("OpenAndPush"))
                {
                    Debug.Log($"Chain blocked at {targetPosition} by Shut.");
                    return false;
                }
            }
        }

        return true;
    }

    bool IsPushable(GameObject obj)
    {
        if (obj.CompareTag("Push") || obj.CompareTag("OpenAndPush"))
        {
            return true;
        }

        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Word"))
            {
                return true;
            }
        }

        return false;
    }

    bool HasOpenAndPushTag(GameObject obj)
    {
        return obj.CompareTag("OpenAndPush");
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        return position;
    }
}
