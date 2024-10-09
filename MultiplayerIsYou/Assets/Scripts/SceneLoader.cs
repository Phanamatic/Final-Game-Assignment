using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMultiplayerScene()
    {
        SceneManager.LoadScene("MultiplayerTestScene"); 
    }
}
