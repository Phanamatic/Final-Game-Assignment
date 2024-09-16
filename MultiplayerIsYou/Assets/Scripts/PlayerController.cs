using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector3 moveDirection;
    public float pushDistance = 1f; // Distance to push objects

    void Update()
    {
        // Check if the GameObject has the tag 'You'
        if (gameObject.CompareTag("You"))
        {
            HandleMovementInput();
        }
    }

    // This method will handle movement input from the player
    void HandleMovementInput()
    {
        moveDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            moveDirection = Vector3.up; // Move up by 1 unit
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveDirection = Vector3.down; // Move down by 1 unit
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveDirection = Vector3.left; // Move left by 1 unit
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveDirection = Vector3.right; // Move right by 1 unit
        }

        // Move the player by 1 unit in the input direction
        if (moveDirection != Vector3.zero)
        {
            MovePlayer();
        }
    }

    // Move the player by 1 unit
    void MovePlayer()
    {
        Vector3 targetPosition = transform.position + moveDirection; // Calculate the new position

        // Check if there's an object at the target position
        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.1f); // Check for any collider at the target position

        if (hitCollider != null)
        {
            if (hitCollider.CompareTag("Stop"))
            {
                // If there's a "Stop" tagged object, don't move
                Debug.Log("Blocked by a Stop object!");
                return;
            }
            else if (IsPushable(hitCollider.gameObject))
            {
                // Try to push the object and any adjacent pushable objects
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
                // If the object is neither pushable nor tagged "Stop", move the player normally
                transform.position = targetPosition;
            }
        }
        else
        {
            // If no object in the way, move the player normally
            transform.position = targetPosition;
        }
    }

    // Check if the entire chain of pushable objects can be pushed
    bool CanPushChain(GameObject firstObject, Vector3 direction)
    {
        Queue<GameObject> toPush = new Queue<GameObject>();
        toPush.Enqueue(firstObject);

        // Process the objects in the queue
        while (toPush.Count > 0)
        {
            GameObject obj = toPush.Dequeue();
            Vector3 targetPosition = obj.transform.position + direction * pushDistance;

            // Check if the push target position is blocked by a Stop object
            Collider2D pushBlockCheck = Physics2D.OverlapCircle(targetPosition, 0.1f);
            if (pushBlockCheck != null && pushBlockCheck.CompareTag("Stop"))
            {
                // Found a Stop object, return false as we cannot push past it
                return false;
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

        return true; // All checks passed, the chain can be pushed
    }

    // Push the object and handle pushing of adjacent pushable objects
    void PushObject(GameObject obj, Vector3 direction)
    {
        Vector3 pushTargetPosition = obj.transform.position + direction * pushDistance;

        // Move the current pushable object
        obj.transform.position = pushTargetPosition;

        // Check for adjacent pushable objects in the same direction and push them
        Collider2D adjacentCollider = Physics2D.OverlapCircle(pushTargetPosition, 0.1f);
        if (adjacentCollider != null && IsPushable(adjacentCollider.gameObject))
        {
            PushObject(adjacentCollider.gameObject, direction);
        }
    }

    // Check if the object has a child with the tag "Word" (meaning it's pushable)
    bool IsPushable(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Word"))
            {
                return true; // Object is pushable if a child has the tag "Word"
            }
        }
        return false; // Not pushable otherwise
    }
}
