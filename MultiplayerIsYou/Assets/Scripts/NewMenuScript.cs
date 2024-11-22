using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using Photon.Pun;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NewMenuScript : MonoBehaviourPunCallbacks
{
    private bool isLoadingScene = false; 

    void Start()
    {
        if (GameState.IsResettingLevel && !string.IsNullOrEmpty(GameState.LevelToLoad))
        {
            Debug.Log($"Resetting level: {GameState.LevelToLoad}");


            GameState.IsResettingLevel = false;


            LoadLevel(GameState.LevelToLoad);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingScene = false;
    }

    void Update()
    {
        CheckMenuSequences();
    }

    void CheckMenuSequences()
    {
        if (isLoadingScene)
            return;

        for (int level = 1; level <= 7; level++)
        {
            string levelTag = $"Word_Level";
            string isTag = "Word_Is";
            string levelNumberTag = $"Word_{level}";

            if (CheckSpecificSequence(levelTag, isTag, levelNumberTag))
            {
                Debug.Log($"Level {level} sequence found! Loading scene...");

                string sceneName = $"Level_{level}";

                LoadLevel(sceneName);

                return;
            }
        }
    }

    void LoadLevel(string sceneName)
    {
        isLoadingScene = true;

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {
                    { "CurrentLevel", sceneName }
                });

                PhotonNetwork.LoadLevel(sceneName);
            }
            else
            {
                Debug.Log("Waiting for MasterClient to load the scene.");
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    bool CheckSpecificSequence(string firstTag, string middleTag, string lastTag)
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
