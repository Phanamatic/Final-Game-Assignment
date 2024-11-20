using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1Controller : MonoBehaviour
{
    private Vector3 moveDirection;
    public float pushDistance = 1f; // Distance to push objects

    void Update()
    {
        if (gameObject.CompareTag("You1"))
        {
            HandleMovementInput();
        }
    }

    void HandleMovementInput()
    {
        moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W)) moveDirection = Vector3.up;
        if (Input.GetKeyDown(KeyCode.S)) moveDirection = Vector3.down;
        if (Input.GetKeyDown(KeyCode.A)) moveDirection = Vector3.left;
        if (Input.GetKeyDown(KeyCode.D)) moveDirection = Vector3.right;

        if (moveDirection != Vector3.zero)
        {
            MovePlayer();
        }
    }

    void MovePlayer()
    {
        Vector3 targetPosition = transform.position + moveDirection;

        // Check if the player is colliding with a Shut object
        Collider2D shutCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

        if (shutCollider != null && shutCollider.CompareTag("Shut"))
        {
            // Only block movement if it's not a "OpenAndPush" object
            if (!HasOpenAndPushTag(shutCollider.gameObject))
            {
                Debug.Log("Blocked by a Shut object!");
                return; // Prevent moving into Shut objects
            }
        }

        // Handle other collisions and pushing logic
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
                    transform.position = targetPosition;
                }
                else
                {
                    Debug.Log("Cannot push the chain due to blocking objects!");
                }
            }
            else
            {
                transform.position = targetPosition;
            }
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    bool CanPushChain(GameObject firstObject, Vector3 direction)
    {
        List<GameObject> chain = new List<GameObject>();
        Vector3 nextPosition = firstObject.transform.position;

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

        foreach (GameObject chainObj in chain)
        {
            Vector3 targetPosition = chainObj.transform.position + direction * pushDistance;
            Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

            if (hitCollider != null && hitCollider.CompareTag("Stop"))
            {
                Debug.Log($"Chain blocked at {targetPosition} by Stop.");
                return false;
            }
        }

        return true;
    }

    void PushObject(GameObject obj, Vector3 direction)
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

        foreach (GameObject chainObj in chain)
        {
            Vector3 targetPosition = chainObj.transform.position + direction * pushDistance;
            Collider2D shutCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

            // Handle Shut and OpenAndPush interaction
            if (shutCollider != null && shutCollider.CompareTag("Shut"))
            {
                if (chainObj.CompareTag("OpenAndPush"))
                {
                    Destroy(shutCollider.gameObject); // Destroy Shut object
                    Destroy(chainObj); // Destroy OpenAndPush object
                    Debug.Log($"Shut and {chainObj.name} destroyed at {targetPosition}!");
                    return; // Stop further movement for this chain
                }
            }

            chainObj.transform.position += direction * pushDistance;
        }
    }

    bool IsPushable(GameObject obj)
    {
        // Check if the object has the "Push" or "OpenAndPush" tag, or if it has any child with the "Word" tag
        if (obj.CompareTag("Push") || obj.CompareTag("OpenAndPush"))
        {
            return true;
        }

        // Also check if the object has any child with the "Word" tag
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
        // Only objects with "OpenAndPush" tag should push through Shut objects
        return obj.CompareTag("OpenAndPush");
    }
}
