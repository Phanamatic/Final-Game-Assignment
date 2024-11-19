using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2Controller : MonoBehaviour
{
    private Vector3 moveDirection;
    public float pushDistance = 1f; // Distance to push objects

    void Update()
    {
        if (gameObject.CompareTag("You2"))
        {
            HandleMovementInput();
        }
    }

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

    void MovePlayer()
    {
        Vector3 targetPosition = transform.position + moveDirection;
        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f);

        // Check if the player is colliding with a Shut object
        if (hitCollider != null && hitCollider.CompareTag("Shut"))
        {
            // Only block movement if it's not a "OpenAndPush" object
            if (!HasOpenAndPushTag(hitCollider.gameObject))
            {
                Debug.Log("Blocked by a Shut object!");
                return; // Prevent moving into Shut objects
            }
        }

        // Handle other collisions and pushing logic
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
        Queue<GameObject> toPush = new Queue<GameObject>();
        toPush.Enqueue(firstObject);

        while (toPush.Count > 0)
        {
            GameObject obj = toPush.Dequeue();
            Vector3 targetPosition = obj.transform.position + direction * pushDistance;

            // Check if the target position is blocked by a Shut object
            Collider2D pushBlockCheck = Physics2D.OverlapCircle(targetPosition, 0.2f);
            if (pushBlockCheck != null && pushBlockCheck.CompareTag("Shut"))
            {
                // If it's an "OpenAndPush" object, don't block
                if (!HasOpenAndPushTag(obj))
                {
                    Debug.Log($"Blocked by a Shut object at {targetPosition}.");
                    return false;
                }
            }

            // Check for adjacent pushable objects
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

    void PushObject(GameObject obj, Vector3 direction)
    {
        Vector3 pushTargetPosition = obj.transform.position + direction * pushDistance;
        obj.transform.position = pushTargetPosition;

        // Check for adjacent pushable objects and push them
        Collider2D adjacentCollider = Physics2D.OverlapCircle(pushTargetPosition, 0.1f);
        if (adjacentCollider != null && IsPushable(adjacentCollider.gameObject))
        {
            PushObject(adjacentCollider.gameObject, direction);
        }

        // Destroy Shut objects if pushed into them by OpenAndPush objects
        Collider2D shutCollider = Physics2D.OverlapCircle(pushTargetPosition, 0.1f);
        if (shutCollider != null && shutCollider.CompareTag("Shut"))
        {
            if (HasOpenAndPushTag(obj)) // Only objects with "OpenAndPush" tag destroy Shut objects
            {
                Destroy(shutCollider.gameObject);
                Debug.Log($"Shut object at {pushTargetPosition} destroyed!");
            }
            else
            {
                Debug.Log($"Shut object at {pushTargetPosition} not destroyed - missing OpenAndPush!");
            }
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
