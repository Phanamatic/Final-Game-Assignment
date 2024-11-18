using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject loginMenu;
    public GameObject signInMenu;
    public GameObject createAccountMenu;

    public void ShowPanel(GameObject panel)
    {
        signInMenu.SetActive(false);
        createAccountMenu.SetActive(false);

        panel.SetActive(true);
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
