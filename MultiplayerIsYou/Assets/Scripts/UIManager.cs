using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuCanvas;
    [SerializeField] GameObject playModeUI;
    bool onPause = false;

    private void Start()
    {
        pauseMenuCanvas.SetActive(false);
        playModeUI.SetActive(true);
    }
    public void returnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void restartLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void openSetting()
    {
        ;
    }

    public void returnToMap()
    {
        ;
    }

    public void levelOne()
    {
        SceneManager.LoadScene(1);
    }


    public void resumePlay()
    {
        Time.timeScale = 1;
        pauseMenuCanvas.SetActive(false);
        playModeUI.SetActive(true);
    }

    public void pauseGame()
    {
        pauseMenuCanvas.SetActive(true);
        playModeUI.SetActive(false);
        Time.timeScale = 0;
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void pauseController()
    {

        onPause = !onPause;
        if (onPause == true)
        {
            resumePlay();
        }
        else
        {
            pauseGame();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            pauseController();
    }
}
