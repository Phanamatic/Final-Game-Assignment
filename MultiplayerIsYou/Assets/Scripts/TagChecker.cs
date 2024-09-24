using UnityEngine;

public class TagChecker : MonoBehaviour
{
    void Update()
    {
        // Check sequences for Baba and Flag
        CheckSequence("Word_Baba", "Word_Is", "Word_You", "Baba", "You");
        CheckSequence("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");

        // Check Wall sequences
        CheckWallSequences();
    }

    void CheckWallSequences()
    {
        bool wallIsStopFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool wallIsYouFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_You", "Wall", "You");

        // If neither sequence is found, set to untagged
        if (!wallIsStopFound && !wallIsYouFound)
        {
            SetParentTagsForAll(false, "Wall", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (wallIsYouFound)
            {
                SetParentTagsForAll(true, "Wall", "You");
            }
            if (wallIsStopFound)
            {
                SetParentTagsForAll(true, "Wall", "Stop");
            }
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

    void CheckSequence(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        GameObject[] firstObjects = GameObject.FindGameObjectsWithTag(firstTag);
        GameObject[] middleObjects = GameObject.FindGameObjectsWithTag(middleTag);
        GameObject[] lastObjects = GameObject.FindGameObjectsWithTag(lastTag);

        if (firstObjects.Length > 0 && middleObjects.Length > 0 && lastObjects.Length > 0)
        {
            bool sequenceFound = false;

            foreach (GameObject first in firstObjects)
            {
                foreach (GameObject middle in middleObjects)
                {
                    foreach (GameObject last in lastObjects)
                    {
                        // Check horizontal or vertical sequence
                        if ((IsHorizontallyAdjacent(first.transform.position, middle.transform.position) &&
                             IsHorizontallyAdjacent(middle.transform.position, last.transform.position)) ||
                            (IsVerticallyAdjacent(first.transform.position, middle.transform.position) &&
                             IsVerticallyAdjacent(middle.transform.position, last.transform.position)))
                        {
                            sequenceFound = true;
                            break;
                        }
                    }
                    if (sequenceFound) break;
                }
                if (sequenceFound) break;
            }

            // Update all parents' tags for objects with the targetTag
            SetParentTagsForAll(sequenceFound, targetTag, newParentTag);
        }
        else
        {
            SetParentTagsForAll(false, targetTag, newParentTag);
        }
    }
}
