using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public void Level_1()
    {
        SceneManager.LoadScene("Level1");
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
    }

    public void Level_2()
    {
        SceneManager.LoadScene("Level2Trial");
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
    }

    public void Level_3()
    {
        SceneManager.LoadScene("Level3");
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
    }
}
