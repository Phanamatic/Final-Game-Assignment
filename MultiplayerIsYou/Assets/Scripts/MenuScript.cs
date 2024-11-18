using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
   public void LoadLevel1()
    {
        SceneManager.LoadScene("Eden_Test_1");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Eden_Test_2");
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene("Eden_Test_3");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("TestMainMenu");
    }
}
