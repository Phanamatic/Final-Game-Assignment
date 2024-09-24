using UnityEngine;

public class TagChecker : MonoBehaviour
{
    void Update()
    {
        CheckSequence("Word_Baba", "Word_Is", "Word_You", "Baba", "You");
        CheckSequence("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");
        CheckSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        CheckSequence("Word_Wall", "Word_Is", "Word_You", "Wall", "You");

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
