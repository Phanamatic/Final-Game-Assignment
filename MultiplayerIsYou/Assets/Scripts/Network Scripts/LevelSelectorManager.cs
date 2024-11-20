using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

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
        // Assign OnClick listeners to each button
        level1Button.onClick.AddListener(() => OnLevelButtonClicked("Eden_Test_1"));
        level2Button.onClick.AddListener(() => OnLevelButtonClicked("Eden_Test_2"));
        level3Button.onClick.AddListener(() => OnLevelButtonClicked("Eden_Test_3"));
        level4Button.onClick.AddListener(() => OnLevelButtonClicked("Level_4"));
        level5Button.onClick.AddListener(() => OnLevelButtonClicked("Level_5"));
        level6Button.onClick.AddListener(() => OnLevelButtonClicked("Level_6"));
        level7Button.onClick.AddListener(() => OnLevelButtonClicked("Level_7"));
    }

    private void OnLevelButtonClicked(string sceneName)
    {
        if (PhotonNetwork.InRoom)
        {
            // Load the scene for all players in the room
            PhotonNetwork.LoadLevel(sceneName);
        }
        else
        {
            // If not connected to Photon, load the scene locally
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
