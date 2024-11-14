using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tag_Checker : MonoBehaviour
{
    void Update()
    {
        CheckObjectSequences("Word_Skull", "Word_Is", "Word_Defeat", "Skull", "Defeat");
        CheckObjectSequences("Word_Star", "Word_Is", "Word_Defeat", "Star", "Defeat");


        CheckRockSequences();
        CheckBabaSequences();
        CheckWallSequences();
        CheckLalaSequences();
        CheckFlagSequences();
        CheckPillarSequences();
    }

    void CheckPillarSequences()
    {
        bool PillarIsDefeat = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Defeat", "Pillar", "Defeat");
        bool PillarIsPush = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Push", "Pillar", "Push");
        bool PillarIsThem1 = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Them1", "Pillar", "You1");

        if (!PillarIsDefeat && !PillarIsPush && !PillarIsThem1)
        {
            SetParentTagsForAll(false, "Pillar", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (PillarIsDefeat)
            {
                SetParentTagsForAll(true, "Pillar", "Defeat");
            }
            else if (PillarIsPush)
            {
                SetParentTagsForAll(true, "Pillar", "Push");
            }
            else if (PillarIsThem1)
            {
                SetParentTagsForAll(true, "Pillar", "You1");
            }
            else if (PillarIsThem1 && PillarIsPush)
            {
                // Set the tag of Wall to "Stop"
                SetParentTagsForAll(true, "Pillar", "You1");
            }
        }
    }

    void CheckFlagSequences()
    {
        bool FlagIsStop = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Stop", "Flag", "Stop");
        bool FlagIsWin = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");

        // If neither sequence is found, set Wall objects to "Untagged"
        if (!FlagIsStop && !FlagIsWin)
        {
            SetParentTagsForAll(false, "Flag", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (FlagIsWin)
            {
                SetParentTagsForAll(true, "Flag", "Win");
            }
            else if (FlagIsStop)
            {
                SetParentTagsForAll(true, "Flag", "Stop");
            }
        }
    }
    void CheckRockSequences()
    {
        bool RockIsStop = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Stop", "Rock", "Stop");
        bool RockIsThem1 = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Them1", "Rock", "You1");

        // If neither sequence is found, set Wall objects to "Untagged"
        if (!RockIsStop && !RockIsThem1)
        {
            SetParentTagsForAll(false, "Rock", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (RockIsThem1)
            {
                SetParentTagsForAll(true, "Rock", "You1");
            }
            else if (RockIsStop)
            {
                SetParentTagsForAll(true, "Rock", "Stop");
            }
            else if (RockIsThem1 && RockIsStop)
            {
                // Set the tag of Wall to "Stop"
                SetParentTagsForAll(true, "Rock", "Stop");

            }
        }
    }

    void CheckLalaSequences()
    {
        bool LalaIsYou2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_You2", "Lala", "You2");
        bool LalaIsThem2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_Them2", "Lala", "You2");


        // If neither sequence is found, set Wall objects to "Untagged"
        if (!LalaIsYou2 && !LalaIsThem2)
        {
            SetParentTagsForAll(false, "Lala", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (LalaIsThem2)
            {
                SetParentTagsForAll(true, "Lala", "You2");
            }
            else if (LalaIsYou2)
            {
                SetParentTagsForAll(true, "Lala", "You2");
            }
            else if (LalaIsThem2 && LalaIsYou2)
            {
                // Set the tag of Wall to "Stop"
                SetParentTagsForAll(true, "Lala", "You2");
            }
        }
    }
    void CheckBabaSequences()
    {
        bool BabaIsYou1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_You1", "Baba", "You1");
        bool BabaIsThem1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_Them1", "Baba", "You1");


        // If neither sequence is found, set Wall objects to "Untagged"
        if (!BabaIsYou1 && !BabaIsThem1)
        {
            SetParentTagsForAll(false, "Baba", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (BabaIsThem1)
            {
                SetParentTagsForAll(true, "Baba", "You1");
            }
            else if (BabaIsYou1)
            {
                SetParentTagsForAll(true, "Baba", "You1");
            }
            else if (BabaIsThem1 && BabaIsYou1)
            {
                // Set the tag of Wall to "Stop"
                SetParentTagsForAll(true, "Baba", "You1");
            }
        }
    }
    void CheckWallSequences()
    {
        bool wallIsStopFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool wallIsYouFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Them2", "Wall", "You2");
        bool wallIsWinFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Win", "Wall", "Win");

        // If neither sequence is found, set Wall objects to "Untagged"
        if (!wallIsStopFound && !wallIsYouFound && !wallIsWinFound)
        {
            SetParentTagsForAll(false, "Wall", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (wallIsYouFound)
            {
                SetParentTagsForAll(true, "Wall", "You2");
            }
            else if (wallIsStopFound)
            {
                SetParentTagsForAll(true, "Wall", "Stop");
            }
            else if (wallIsWinFound)
            {
                SetParentTagsForAll(true, "Wall", "Win");
            }
            else if (wallIsYouFound && wallIsStopFound)
            {
                // Set the tag of Wall to "Stop"
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
