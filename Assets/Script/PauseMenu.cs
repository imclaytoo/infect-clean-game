using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject Pemain;
    public GameObject Orang;
    public GameObject Orang1;
    public GameObject Orang2;
    public GameObject Orang3;
    public GameObject Orang4;
    public GameObject Orang5;
    public GameObject joycon;

    public void TogglePause()
    {
        Pemain.SetActive(false);
        Orang.SetActive(false);
        Orang1.SetActive(false);
        Orang2.SetActive(false);
        Orang3.SetActive(false);
        Orang4.SetActive(false);
        Orang5.SetActive(false);
        joycon.SetActive(false);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.volume = 0f;
    }

    public void Resume()
    {
        Pemain.SetActive(true);
        Orang.SetActive(true);
        Orang1.SetActive(true);
        Orang2.SetActive(true);
        Orang3.SetActive(true);
        Orang4.SetActive(true);
        Orang5.SetActive(true);
        joycon.SetActive(true);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
    }

    public void Restart()
    {
        SceneManager.LoadScene("Level1");
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
    }

    public void Exit()
    {
        Application.Quit();
        Debug.Log("Quiting game....");
    }

    public void Home()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NextLevel(){
        SceneManager.LoadScene("Level2Trial");
    }
}