// LevelSelectorManager.cs
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class LevelSelectorManager : MonoBehaviour
{
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;
    public Button level5Button;
    public Button level6Button;
    public Button level7Button;

    private void Start()
    {
        // If we are resetting the level, immediately load the level
        if (GameState.IsResettingLevel && !string.IsNullOrEmpty(GameState.LevelToLoad))
        {
            Debug.Log($"Resetting level: {GameState.LevelToLoad}");

            // Reset the flag
            GameState.IsResettingLevel = false;

            // Load the level
            OnLevelButtonClicked(GameState.LevelToLoad);
        }
        else
        {
            level1Button.onClick.AddListener(() => OnLevelButtonClicked("Eden_Test_1"));
            level2Button.onClick.AddListener(() => OnLevelButtonClicked("Eden_Test_2"));
            level3Button.onClick.AddListener(() => OnLevelButtonClicked("Eden_Test_3"));
            level4Button.onClick.AddListener(() => OnLevelButtonClicked("Level_4"));
            level5Button.onClick.AddListener(() => OnLevelButtonClicked("Level_5"));
            level6Button.onClick.AddListener(() => OnLevelButtonClicked("Level_6"));
            level7Button.onClick.AddListener(() => OnLevelButtonClicked("Level_7"));
        }
    }

    private void OnLevelButtonClicked(string sceneName)
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Store the current level in room properties
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {
                    { "CurrentLevel", sceneName }
                });

                // Load the scene for all players in the room
                PhotonNetwork.LoadLevel(sceneName);
            }
            else
            {
                // Do nothing; other clients will sync automatically
            }
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
}
