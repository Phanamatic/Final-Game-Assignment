using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tag_Checker : MonoBehaviour
{
    void Update()
    {
        // Check sequences for Baba and Flag using the same logic as the Wall sequences
        CheckObjectSequences("Word_Baba", "Word_Is", "Word_You", "Baba", "You1");
        CheckObjectSequences("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");
        CheckObjectSequences("Word_Lala", "Word_Is", "Word_You", "Lala", "You2");
        CheckObjectSequences("Word_Rock", "Word_Is", "Word_Stop", "Rock", "Stop");

        CheckWallSequences(); // Wall-specific logic
    }

    void CheckWallSequences()
    {
        bool wallIsStopFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool wallIsYouFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_You", "Wall", "You1");

        // If neither sequence is found, set Wall objects to "Untagged"
        if (!wallIsStopFound && !wallIsYouFound)
        {
            SetParentTagsForAll(false, "Wall", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (wallIsYouFound)
            {
                SetParentTagsForAll(true, "Wall", "You1");
            }
            if (wallIsStopFound)
            {
                SetParentTagsForAll(true, "Wall", "Stop");
            }
        }
    }

    // Check and apply sequence logic for other objects (Baba, Flag, etc.)
    void CheckObjectSequences(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        bool sequenceFound = CheckSpecificSequence(firstTag, middleTag, lastTag, targetTag, newParentTag);

        // If no sequence is found, set the parent tags to untagged
        if (!sequenceFound)
        {
            SetParentTagsForAll(false, targetTag, "Untagged");
        }
        else
        {
            // Sequence found, set the parent tag to the newParentTag
            SetParentTagsForAll(true, targetTag, newParentTag);
        }
    }

    bool CheckSpecificSequence(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        GameObject[] firstObjects = GameObject.FindGameObjectsWithTag(firstTag);
        GameObject[] middleObjects = GameObject.FindGameObjectsWithTag(middleTag);
        GameObject[] lastObjects = GameObject.FindGameObjectsWithTag(lastTag);

        if (firstObjects.Length > 0 && middleObjects.Length > 0 && lastObjects.Length > 0)
        {
            foreach (GameObject first in firstObjects)
            {
                foreach (GameObject middle in middleObjects)
                {
                    foreach (GameObject last in lastObjects)
                    {
                        // Check for adjacency
                        if ((IsHorizontallyAdjacent(first.transform.position, middle.transform.position) &&
                             IsHorizontallyAdjacent(middle.transform.position, last.transform.position)) ||
                            (IsVerticallyAdjacent(first.transform.position, middle.transform.position) &&
                             IsVerticallyAdjacent(middle.transform.position, last.transform.position)))
                        {
                            Debug.Log($"Sequence found: {first.tag} - {middle.tag} - {last.tag}");
                            return true; // Sequence found
                        }
                    }
                }
            }
        }
        return false; // No sequence found
    }

    bool IsHorizontallyAdjacent(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) < 1.1f && Mathf.Abs(pos1.y - pos2.y) < 0.01f;
    }

    bool IsVerticallyAdjacent(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.y - pos2.y) < 1.1f && Mathf.Abs(pos1.x - pos2.x) < 0.01f;
    }

    // This method finds all objects with the given tag and changes their parent's tag
    void SetParentTagsForAll(bool isSequence, string childTag, string newTag)
    {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(childTag);

        foreach (GameObject targetObject in targetObjects)
        {
            if (targetObject != null)
            {
                GameObject parentObject = targetObject.transform.parent?.gameObject;

                if (parentObject != null)
                {
                    parentObject.tag = isSequence ? newTag : "Untagged";
                    Debug.Log(isSequence
                        ? $"Sequence found: Parent's tag set to '{newTag}'"
                        : "Broken sequence: Parent's tag set to 'Untagged'");
                }
                else
                {
                    Debug.LogWarning($"The '{childTag}' object does not have a parent.");
                }
            }
        }
    }
}
