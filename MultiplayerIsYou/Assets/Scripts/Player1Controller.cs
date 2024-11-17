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
        Queue<GameObject> toPush = new Queue<GameObject>();
        toPush.Enqueue(firstObject);

        while (toPush.Count > 0)
        {
            GameObject obj = toPush.Dequeue();
            Vector3 targetPosition = obj.transform.position + direction * pushDistance;

            // Check if the target position is blocked
            Collider2D pushBlockCheck = Physics2D.OverlapCircle(targetPosition, 0.1f);

            if (pushBlockCheck != null)
            {
                // Block if the target is "Stop"
                if (pushBlockCheck.CompareTag("Stop"))
                {
                    Debug.Log($"Blocked by a Stop object at {targetPosition}.");
                    return false;
                }

                // Special handling for "Shut"
                if (pushBlockCheck.CompareTag("Shut"))
                {
                    if (!HasChildWithTag(obj, "Open") && !HasChildWithTag(obj, "OpenAndPush"))
                    {
                        Debug.Log($"Cannot push into Shut at {targetPosition} without Open or OpenAndPush!");
                        return false;
                    }
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

        // Destroy Shut objects if pushed into them by Open or OpenAndPush objects
        Collider2D shutCollider = Physics2D.OverlapCircle(pushTargetPosition, 0.1f);
        if (shutCollider != null && shutCollider.CompareTag("Shut"))
        {
            if (HasChildWithTag(obj, "Open") || HasChildWithTag(obj, "OpenAndPush"))
            {
                Destroy(shutCollider.gameObject);
                Debug.Log($"Shut object at {pushTargetPosition} destroyed!");
            }
            else
            {
                Debug.Log($"Shut object at {pushTargetPosition} not destroyed - missing Open or OpenAndPush!");
            }
        }
    }

    bool IsPushable(GameObject obj)
    {
        if (obj.CompareTag("Push") || obj.CompareTag("OpenAndPush"))
        {
            return true;
        }

        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Word") || child.CompareTag("Open") || child.CompareTag("OpenAndPush"))
            {
                return true;
            }
        }

        return false;
    }

    bool HasChildWithTag(GameObject obj, string tag)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }
}
