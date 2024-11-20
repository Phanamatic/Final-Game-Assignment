using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class Tag_Checker : MonoBehaviourPunCallbacks
{
    void Update()
    {
        if (PhotonNetwork.IsMasterClient) // Run only on the master client
        {
            // Check sequences for various objects
            CheckObjectSequences("Word_Skull", "Word_Is", "Word_Defeat", "Skull", "Defeat");
            CheckObjectSequences("Word_Star", "Word_Is", "Word_Defeat", "Star", "Defeat");

            CheckRockSequences();
            CheckBabaSequences();
            CheckWallSequences();
            CheckLalaSequences();
            CheckFlagSequences();
            CheckPillarSequences();
            CheckDoorSequences();
            CheckKeySequences();
            CheckStarSequences();
        }
    }

    void CheckKeySequences()
    {
        bool KeyIsOpen = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Open", "Key", "Open");
        bool KeyIsPush = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Push", "Key", "Push");
        bool KeyIsWin = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Win", "Key", "Win");

        if (!KeyIsOpen && !KeyIsPush && !KeyIsWin)
        {
            SetParentTagsForAll(false, "Key", "Untagged");
        }
        else
        {
            // Check combined conditions first
            if (KeyIsPush && KeyIsOpen)
            {
                SetParentTagsForAll(true, "Key", "OpenAndPush");
            }
            else if (KeyIsOpen)
            {
                SetParentTagsForAll(true, "Key", "Open");
            }
            else if (KeyIsPush)
            {
                SetParentTagsForAll(true, "Key", "Push");
            }
            else if (KeyIsWin)
            {
                SetParentTagsForAll(true, "Key", "Win");
            }
        }
    }

    void CheckStarSequences()
    {
        bool StarIsOpen = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Open", "Star", "Open");
        bool StarIsPush = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Push", "Star", "Push");
        bool StarIsWin = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Win", "Star", "Win");

        if (!StarIsOpen && !StarIsPush && !StarIsWin)
        {
            SetParentTagsForAll(false, "Star", "Untagged");
        }
        else
        {
            // Check combined conditions first
            if (StarIsPush && StarIsOpen)
            {
                SetParentTagsForAll(true, "Star", "OpenAndPush");
            }
            else if (StarIsOpen)
            {
                SetParentTagsForAll(true, "Star", "Open");
            }
            else if (StarIsPush)
            {
                SetParentTagsForAll(true, "Star", "Push");
            }
            else if (StarIsWin)
            {
                SetParentTagsForAll(true, "Star", "Win");
            }
        }
    }

    void CheckDoorSequences()
    {
        bool DoorIsShut = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Shut", "Door", "Shut");
        bool DoorIsWin = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Win", "Door", "Win");
        bool DoorIsPush = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Push", "Door", "Push");
        bool DoorIsStop = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Stop", "Door", "Stop");

        if (!DoorIsPush && !DoorIsShut && !DoorIsWin && !DoorIsStop)
        {
            SetParentTagsForAll(false, "Door", "Untagged");
        }
        else
        {
            if (DoorIsShut && DoorIsStop)
            {
                SetParentTagsForAll(true, "Door", "Shut");
            }
            else if (DoorIsWin)
            {
                SetParentTagsForAll(true, "Door", "Win");
            }
            else if (DoorIsShut)
            {
                SetParentTagsForAll(true, "Door", "Shut");
            }
            else if (DoorIsPush)
            {
                SetParentTagsForAll(true, "Door", "Push");
            }
            else if (DoorIsStop)
            {
                SetParentTagsForAll(true, "Door", "Stop");
            }
        }
    }

    void CheckPillarSequences()
    {
        bool PillarIsDefeat = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Defeat", "Pillar", "Defeat");
        bool PillarIsPush = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Push", "Pillar", "Push");
        bool PillarIsThem1 = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Them1", "Pillar", "You1");
        bool PillarIsYou1 = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_You1", "Pillar", "You1");
        bool PillarIsOpen = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Open", "Pillar", "Open");

        if (!PillarIsDefeat && !PillarIsPush && !PillarIsThem1 && !PillarIsYou1 && !PillarIsOpen)
        {
            SetParentTagsForAll(false, "Pillar", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (PillarIsPush && PillarIsOpen)
            {
                SetParentTagsForAll(true, "Pillar", "OpenAndPush");
            }
            else if (PillarIsYou1 || PillarIsThem1)
            {
                SetParentTagsForAll(true, "Pillar", "You1");
            }
            else if (PillarIsOpen)
            {
                SetParentTagsForAll(true, "Pillar", "Open");
            }
            else if (PillarIsDefeat)
            {
                SetParentTagsForAll(true, "Pillar", "Defeat");
            }
            else if (PillarIsPush)
            {
                SetParentTagsForAll(true, "Pillar", "Push");
            }
        }
    }

    void CheckFlagSequences()
    {
        bool FlagIsStop = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Stop", "Flag", "Stop");
        bool FlagIsWin = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");

        if (!FlagIsStop && !FlagIsWin)
        {
            SetParentTagsForAll(false, "Flag", "Untagged");
        }
        else
        {
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
        bool RockIsPush = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Push", "Rock", "Push");
        bool RockIsOpen = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Open", "Rock", "Open");

        if (!RockIsStop && !RockIsThem1 && !RockIsOpen && !RockIsPush)
        {
            SetParentTagsForAll(false, "Rock", "Untagged");
        }
        else
        {
            if (RockIsPush && RockIsOpen)
            {
                SetParentTagsForAll(true, "Rock", "OpenAndPush");
            }
            else if (RockIsOpen)
            {
                SetParentTagsForAll(true, "Rock", "Open");
            }
            else if (RockIsThem1)
            {
                SetParentTagsForAll(true, "Rock", "You1");
            }
            else if (RockIsPush)
            {
                SetParentTagsForAll(true, "Rock", "Push");
            }
            else if (RockIsStop)
            {
                SetParentTagsForAll(true, "Rock", "Stop");
            }
        }
    }

    void CheckLalaSequences()
    {
        bool LalaIsYou2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_You2", "Lala", "You2");
        bool LalaIsThem2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_Them2", "Lala", "You2");

        if (!LalaIsYou2 && !LalaIsThem2)
        {
            SetParentTagsForAll(false, "Lala", "Untagged");
        }
        else
        {
            SetParentTagsForAll(true, "Lala", "You2");
        }
    }

    void CheckBabaSequences()
    {
        bool BabaIsYou1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_You1", "Baba", "You1");
        bool BabaIsThem1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_Them1", "Baba", "You1");

        if (!BabaIsYou1 && !BabaIsThem1)
        {
            SetParentTagsForAll(false, "Baba", "Untagged");
        }
        else
        {
            SetParentTagsForAll(true, "Baba", "You1");
        }
    }

    void CheckWallSequences()
    {
        bool wallIsStopFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool wallIsYouFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Them2", "Wall", "You2");
        bool wallIsWinFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Win", "Wall", "Win");

        if (!wallIsStopFound && !wallIsYouFound && !wallIsWinFound)
        {
            SetParentTagsForAll(false, "Wall", "Untagged");
        }
        else
        {
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
        }
    }

    void CheckObjectSequences(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        bool sequenceFound = CheckSpecificSequence(firstTag, middleTag, lastTag, targetTag, newParentTag);

        if (!sequenceFound)
        {
            SetParentTagsForAll(false, targetTag, "Untagged");
        }
        else
        {
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

    // This method finds all objects with the given tag and changes their parent's tag, synchronized over the network
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
                    PhotonView parentPhotonView = parentObject.GetComponent<PhotonView>();

                    if (parentPhotonView != null)
                    {
                        // Synchronize tag change across all clients
                        photonView.RPC("RPC_SetParentTag", RpcTarget.AllBuffered, parentPhotonView.ViewID, isSequence ? newTag : "Untagged");
                    }
                    else
                    {
                        // If the parent doesn't have a PhotonView, we change the tag locally (not synchronized)
                        parentObject.tag = isSequence ? newTag : "Untagged";
                    }

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

    [PunRPC]
    void RPC_SetParentTag(int parentViewID, string newTag)
    {
        PhotonView parentPhotonView = PhotonView.Find(parentViewID);
        if (parentPhotonView != null)
        {
            parentPhotonView.gameObject.tag = newTag;
        }
    }
}
