using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tag_Checker : MonoBehaviourPunCallbacks
{
    void Update()
    {
        if (photonView.IsMine) // Run only on the master client
        {
            // Checking sequences for various objects
            photonView.RPC("RPC_CheckObjectSequences", RpcTarget.All, "Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");
            photonView.RPC("RPC_CheckObjectSequences", RpcTarget.All, "Word_Skull", "Word_Is", "Word_Defeat", "Skull", "Defeat");
            photonView.RPC("RPC_CheckObjectSequences", RpcTarget.All, "Word_Star", "Word_Is", "Word_Defeat", "Star", "Defeat");

            // Checking specific object sequences
            photonView.RPC("RPC_CheckRockSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckBabaSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckWallSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckLalaSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckFlagSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckPillarSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckDoorSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckKeySequences", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_CheckObjectSequences(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        bool sequenceFound = CheckSpecificSequence(firstTag, middleTag, lastTag, targetTag, newParentTag);

        if (!sequenceFound)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, targetTag, "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, targetTag, newParentTag);
        }
    }

    [PunRPC]
    void RPC_CheckRockSequences()
    {
        bool RockIsStop = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Stop", "Rock", "Stop");
        bool RockIsThem1 = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Them1", "Rock", "You1");

        if (!RockIsStop && !RockIsThem1)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Rock", "Untagged");
        }
        else if (RockIsStop || RockIsThem1)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Rock", RockIsStop ? "Stop" : "You1");
        }
    }

    [PunRPC]
    void RPC_CheckLalaSequences()
    {
        bool LalaIsYou2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_You2", "Lala", "You2");
        bool LalaIsThem2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_Them2", "Lala", "You2");

        if (!LalaIsYou2 && !LalaIsThem2)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Lala", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Lala", "You2");
        }
    }

    [PunRPC]
    void RPC_CheckBabaSequences()
    {
        bool BabaIsYou1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_You1", "Baba", "You1");
        bool BabaIsThem1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_Them1", "Baba", "You1");

        if (!BabaIsYou1 && !BabaIsThem1)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Baba", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Baba", "You1");
        }
    }

    [PunRPC]
    void RPC_CheckWallSequences()
    {
        bool WallIsStop = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool WallIsYou = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Them2", "Wall", "You2");

        if (!WallIsStop && !WallIsYou)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Wall", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", WallIsStop ? "Stop" : "You2");
        }
    }

    [PunRPC]
    void RPC_CheckFlagSequences()
    {
        bool FlagIsStop = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Stop", "Flag", "Stop");
        bool FlagIsWin = CheckSpecificSequence("Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");

        if (!FlagIsStop && !FlagIsWin)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Flag", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Flag", FlagIsStop ? "Stop" : "Win");
        }
    }

    [PunRPC]
    void RPC_CheckPillarSequences()
    {
        bool PillarIsDefeat = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Defeat", "Pillar", "Defeat");
        bool PillarIsPush = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_Push", "Pillar", "Push");
        bool PillarIsYou1 = CheckSpecificSequence("Word_Pillar", "Word_Is", "Word_You1", "Pillar", "You1");

        if (!PillarIsDefeat && !PillarIsPush && !PillarIsYou1)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Pillar", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Pillar", PillarIsDefeat ? "Defeat" : PillarIsPush ? "Push" : "You1");
        }
    }

    [PunRPC]
    void RPC_CheckDoorSequences()
    {
        bool DoorIsShut = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Shut", "Door", "Shut");
        bool DoorIsWin = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Win", "Door", "Win");
        bool DoorIsPush = CheckSpecificSequence("Word_Door", "Word_Is", "Word_Push", "Door", "Push");

        if (!DoorIsShut && !DoorIsWin && !DoorIsPush)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Door", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Door", DoorIsShut ? "Shut" : DoorIsWin ? "Win" : "Push");
        }
    }

    [PunRPC]
    void RPC_CheckKeySequences()
    {
        bool KeyIsOpen = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Open", "Key", "Open");
        bool KeyIsPush = CheckSpecificSequence("Word_Key", "Word_Is", "Word_Push", "Key", "Push");

        if (!KeyIsOpen && !KeyIsPush)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Key", "Untagged");
        }
        else
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Key", KeyIsOpen ? "Open" : "Push");
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
                        if ((IsHorizontallyAdjacent(first.transform.position, middle.transform.position) &&
                             IsHorizontallyAdjacent(middle.transform.position, last.transform.position)) ||
                            (IsVerticallyAdjacent(first.transform.position, middle.transform.position) &&
                             IsVerticallyAdjacent(middle.transform.position, last.transform.position)))
                        {
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
                }
            }
        }
    }
}
