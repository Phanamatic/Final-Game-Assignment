using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentenceChecker : MonoBehaviour
{
    public float checkDistance = 1f; // Distance to check for adjacent pushable objects
    private float detectionRadius = 0.1f; // Radius for overlap checks

    private void Start()
    {
        StartCoroutine(CheckForAlignedPushables());
    }

    private IEnumerator CheckForAlignedPushables()
    {
        while (true)
        {
            CheckForThreeInRow();
            yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds
        }
    }

    private void CheckForThreeInRow()
    {
        // Check horizontally
        CheckDirection(Vector3.right, "horizontal");

        // Check vertically
        CheckDirection(Vector3.down, "vertical");
    }

    private void CheckDirection(Vector3 direction, string alignment)
    {
        Vector3 position1 = transform.position + direction * checkDistance;
        Vector3 position2 = position1 + direction * checkDistance;

        string firstTag = GetPushableTagAtPosition(transform.position);
        string secondTag = GetPushableTagAtPosition(position1);
        string thirdTag = GetPushableTagAtPosition(position2);

        // Check if the tags are in the correct order: "Word_Baba", "Word_Is", "Word_You"
        if (firstTag == "Word_Baba" && secondTag == "Word_Is" && thirdTag == "Word_You")
        {
            Debug.Log("BABA IS YOU");

            Transform babaChild = FindChildByTag("Baba");
            if (babaChild != null)
            {
                Transform babaParent = babaChild.parent;
                if (babaParent != null)
                {
                    babaParent.tag = "You"; // Change the parent's tag to "You"
                }
            }
        }
        // Check for "Word_Flag", "Word_Is", "Word_Win" sequence
        if (firstTag == "Word_Flag" && secondTag == "Word_Is" && thirdTag == "Word_Win")
        {
            Debug.Log("FLAG IS WIN");

            Transform flagChild = FindChildByTag("Flag");
            if (flagChild != null)
            {
                Transform flagParent = flagChild.parent;
                if (flagParent != null)
                {
                    flagParent.tag = "Win"; // Change the parent's tag to "Win"
                }
            }
        }
    }

    private string GetPushableTagAtPosition(Vector3 position)
    {
        Collider2D hitCollider = Physics2D.OverlapCircle(position, detectionRadius);

        if (hitCollider != null && IsPushable(hitCollider.gameObject))
        {
            return hitCollider.gameObject.tag;
        }
        return "None";
    }

    private bool IsPushable(GameObject obj)
    {
        // Check if the object has a child with the tag "Word" (indicating it's pushable)
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Word"))
            {
                return true; // Object is pushable if a child has the tag "Word"
            }
        }
        return false; // Not pushable otherwise
    }

    private Transform FindChildByTag(string tag)
    {
        // Search through all objects in the scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            foreach (Transform child in obj.transform)
            {
                if (child.CompareTag(tag))
                {
                    return child; // Return the child with the specified tag
                }
            }
        }
        return null; // Return null if no child with the specified tag is found
    }
}
