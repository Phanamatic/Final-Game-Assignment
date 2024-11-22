using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tag_Checker : MonoBehaviourPun
{
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        CheckObjectSequences("Word_Skull", "Word_Is", "Word_Defeat", "Skull", "Defeat");
        // CheckObjectSequences("Word_Star", "Word_Is", "Word_Defeat", "Star", "Defeat");

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

    void CheckKeySequences()
    {
        bool KeyIsOpen = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Open", "Key", "Open");
        bool KeyIsPush = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Push", "Key", "Push");
        bool KeyIsWin = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Win", "Key", "Win");

        if (!KeyIsOpen && !KeyIsPush && !KeyIsWin)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Key", "Untagged");
        }
        else
        {
            if (KeyIsPush && KeyIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Key", "OpenAndPush");
            }
            else if (KeyIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Key", "Open");
            }
            else if (KeyIsPush)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Key", "Push");
            }
            else if (KeyIsWin)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Key", "Win");
            }
        }
    }

    void CheckStarSequences()
    {
        bool StarIsOpen = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Open", "Star", "Open");
        bool StarIsPush = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Push", "Star", "Push");
        bool StarIsWin = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Win", "Star", "Win");
        bool StarIsDefeat = CheckSpecificSequence("Word_Star", "Word_Is", "Word_Defeat", "Star", "Defeat");

        if (!StarIsOpen && !StarIsPush && !StarIsWin && !StarIsDefeat)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Star", "Untagged");
        }
        else
        {
            if (StarIsPush && StarIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Star", "OpenAndPush");
            }
            else if (StarIsDefeat)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Star", "Defeat");
            }
            else if (StarIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Star", "Open");
            }
            else if (StarIsPush)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Star", "Push");
            }
            else if (StarIsWin)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Star", "Win");
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
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Door", "Untagged");
        }
        else
        {
            if (DoorIsShut && DoorIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Shut");
            }
            else if (DoorIsWin)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Win");
            }
            else if (DoorIsShut)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Shut");
            }
            else if (DoorIsPush)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Push");
            }
            else if (DoorIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Stop");
            }
            else if (DoorIsWin && DoorIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Stop");
            }
            else if (DoorIsPush && DoorIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Door", "Push");
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
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Pillar", "Untagged");
        }
        else
        {
            if (PillarIsPush && PillarIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "OpenAndPush");
            }
            else if (PillarIsYou1)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "You1");
            }
            else if (PillarIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "Open");
            }
            else if (PillarIsDefeat)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "Defeat");
            }
            else if (PillarIsPush)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "Push");
            }
            else if (PillarIsThem1)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "You1");
            }
            else if (PillarIsThem1 && PillarIsPush)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Pillar", "You1");
            }
        }
    }

    void CheckFlagSequences()
    {
        bool FlagIsStop = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Stop", "Flag", "Stop");
        bool FlagIsWin = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");

        if (!FlagIsStop && !FlagIsWin)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Flag", "Untagged");
        }
        else
        {
            if (FlagIsWin)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Flag", "Win");
            }
            else if (FlagIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Flag", "Stop");
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
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Rock", "Untagged");
        }
        else
        {
            if (RockIsPush && RockIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Rock", "OpenAndPush");
            }
            else if (RockIsOpen)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Rock", "Open");
            }
            else if (RockIsThem1)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Rock", "You1");
            }
            else if (RockIsPush)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Rock", "Push");
            }
            else if (RockIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Rock", "Stop");
            }
            else if (RockIsThem1 && RockIsStop)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Rock", "Stop");
            }
        }
    }

    void CheckLalaSequences()
    {
        bool LalaIsYou2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_You2", "Lala", "You2");
        bool LalaIsThem2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_Them2", "Lala", "You2");

        if (!LalaIsYou2 && !LalaIsThem2)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Lala", "Untagged");
        }
        else
        {
            if (LalaIsThem2 || LalaIsYou2)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Lala", "You2");
            }
        }
    }

    void CheckBabaSequences()
    {
        bool BabaIsYou1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_You1", "Baba", "You1");
        bool BabaIsThem1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_Them1", "Baba", "You1");

        if (!BabaIsYou1 && !BabaIsThem1)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Baba", "Untagged");
        }
        else
        {
            if (BabaIsThem1 || BabaIsYou1)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Baba", "You1");
            }
        }
    }

    void CheckWallSequences()
    {
        bool wallIsStopFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool wallIsYouFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Them2", "Wall", "You2");
        bool wallIsWinFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Win", "Wall", "Win");

        if (!wallIsStopFound && !wallIsYouFound && !wallIsWinFound)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, "Wall", "Untagged");
        }
        else
        {
            if (wallIsYouFound)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Wall", "You2");
            }
            else if (wallIsStopFound)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Wall", "Stop");
            }
            else if (wallIsWinFound)
            {
                photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, "Wall", "Win");
            }
        }
    }

    void CheckObjectSequences(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        bool sequenceFound = CheckSpecificSequence(firstTag, middleTag, lastTag, targetTag, newParentTag);

        if (!sequenceFound)
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, false, targetTag, "Untagged");
        }
        else
        {
            photonView.RPC("SyncSetParentTagsForAll", RpcTarget.All, true, targetTag, newParentTag);
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
                            return true;
                        }
                    }
                }
            }
        }
        return false; 
    }

    bool IsHorizontallyAdjacent(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) < 1.1f && Mathf.Abs(pos1.y - pos2.y) < 0.01f;
    }

    bool IsVerticallyAdjacent(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.y - pos2.y) < 1.1f && Mathf.Abs(pos1.x - pos2.x) < 0.01f;
    }

    [PunRPC]
    void SyncSetParentTagsForAll(bool isSequence, string childTag, string newTag)
    {
        SetParentTagsForAll(isSequence, childTag, newTag);
    }

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
