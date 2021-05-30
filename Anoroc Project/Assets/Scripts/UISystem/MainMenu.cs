using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    //public PauseMenu pauseMenuVar;


    public void PlayGame() {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void Resume()
    {
        //disable PauseMenu
        pauseMenuUI.SetActive(false);
        //unfreeze time
        Time.timeScale = 1f;

        //pauseMenuVar.GameIsPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }
}
