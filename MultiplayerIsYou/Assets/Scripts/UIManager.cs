using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        ;
    }

    public void restartLevel()
    {
        ;
    }

    public void openSetting()
    {
        ;
    }

    public void returnToMap()
    {
        ;
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
