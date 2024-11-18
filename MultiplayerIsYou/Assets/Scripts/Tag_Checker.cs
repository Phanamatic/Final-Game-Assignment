using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class Tag_Checker : MonoBehaviourPunCallbacks
{
    void Update()
    {
        if (photonView.IsMine) // Only run tag checks on the master client
        {
            // Check sequences for Baba and Flag using the same logic as the Wall sequences
            photonView.RPC("RPC_CheckObjectSequences", RpcTarget.All, "Word_Flag", "Word_Is", "Word_Win", "Flag", "Win");
            photonView.RPC("RPC_CheckObjectSequences", RpcTarget.All, "Word_Skull", "Word_Is", "Word_Defeat", "Skull", "Defeat");

            photonView.RPC("RPC_CheckRockSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckBabaSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckWallSequences", RpcTarget.All);
            photonView.RPC("RPC_CheckLalaSequences", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_CheckRockSequences()
    {
        bool RockIsStop = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Stop", "Rock", "Stop");
        bool RockIsThem1 = CheckSpecificSequence("Word_Rock", "Word_Is", "Word_Them1", "Rock", "You1");

        if (!RockIsStop && !RockIsThem1)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Wall", "Untagged");
        }
        else
        {
            if (RockIsThem1)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", "You2");
            }
            else if (RockIsStop)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", "Stop");
            }
            else if (RockIsThem1 && RockIsStop)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", "Stop");
            }
        }
    }

    [PunRPC]
    void RPC_CheckLalaSequences()
    {
        bool LalaIsYou2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_You2", "Lala", "You2");
        bool LalaIsThem2 = CheckSpecificSequence("Word_Lala", "Word_Is", "Word_Them2", "Lala", "You2");

        // If neither sequence is found, set Lala objects to "Untagged"
        if (!LalaIsYou2 && !LalaIsThem2)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Lala", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (LalaIsThem2 || LalaIsYou2)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Lala", "You2");
            }
        }
    }

    [PunRPC]
    void RPC_CheckBabaSequences()
    {
        bool BabaIsYou1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_You1", "Baba", "You1");
        bool BabaIsThem1 = CheckSpecificSequence("Word_Baba", "Word_Is", "Word_Them1", "Baba", "You1");

        // If neither sequence is found, set Baba objects to "Untagged"
        if (!BabaIsYou1 && !BabaIsThem1)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Baba", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (BabaIsThem1 || BabaIsYou1)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Baba", "You1");
            }
        }
    }

    [PunRPC]
    void RPC_CheckWallSequences()
    {
        bool wallIsStopFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Stop", "Wall", "Stop");
        bool wallIsYouFound = CheckSpecificSequence("Word_Wall", "Word_Is", "Word_Them2", "Wall", "You2");

        // If neither sequence is found, set Wall objects to "Untagged"
        if (!wallIsStopFound && !wallIsYouFound)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, "Wall", "Untagged");
        }
        else
        {
            // At least one sequence is found; set to the appropriate tag
            if (wallIsYouFound)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", "You2");
            }
            else if (wallIsStopFound)
            {
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", "Stop");
            }
            else if (wallIsYouFound && wallIsStopFound)
            {
                // Set the tag of Wall to "Stop"
                photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, "Wall", "Stop");
            }
        }
    }

    [PunRPC]
    void RPC_CheckObjectSequences(string firstTag, string middleTag, string lastTag, string targetTag, string newParentTag)
    {
        bool sequenceFound = CheckSpecificSequence(firstTag, middleTag, lastTag, targetTag, newParentTag);

        // If no sequence is found, set the parent tags to untagged
        if (!sequenceFound)
        {
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, false, targetTag, "Untagged");
        }
        else
        {
            // Sequence found, set the parent tag to the newParentTag
            photonView.RPC("SetParentTagsForAll", RpcTarget.All, true, targetTag, newParentTag);
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

    [PunRPC]
    void SetParentTagsForAll(bool isSequence, string childTag, string newTag)
    {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(childTag);
        foreach (GameObject targetObject in targetObjects)
        {
            if (targetObject != null)
            {
                GameObject parentObject = targetObject.transform.parent?.gameObject;

                if (parentObject != null && parentObject.tag != "You1" && parentObject.tag != "You2")
                {
                    parentObject.tag = isSequence ? newTag : "Untagged";
                }
            }
        }
    }
}
