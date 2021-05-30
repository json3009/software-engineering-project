using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    public bool GameIsPaused = false;

    
    public GameObject pauseMenuUI;
    public GameObject MainMenuPrefab;

    private void Start()
    {
        GlobalEventSystem.Instance.InputActions.UI.Escape.Enable();

        GlobalEventSystem.Instance.InputActions.UI.Escape.performed += Escape_performed;

        GlobalEventSystem.Instance.OnGameMenuOpened += Instance_OnGameMenuOpened;
        GlobalEventSystem.Instance.OnGameMenuClosed += Instance_OnGameMenuClosed;

    }

    private void OnDestroy()
    {
        GlobalEventSystem.Instance.InputActions.UI.Escape.Disable();
        GlobalEventSystem.Instance.InputActions.UI.Escape.performed -= Escape_performed;

        GlobalEventSystem.Instance.OnGameMenuOpened -= Instance_OnGameMenuOpened;
        GlobalEventSystem.Instance.OnGameMenuClosed -= Instance_OnGameMenuClosed;
    }

    private void Instance_OnGameMenuClosed()
    {
        for (int i = 0; i < pauseMenuUI.transform.childCount; i++)
        {
            var obj = pauseMenuUI.transform.GetChild(i);

            if (obj.gameObject != MainMenuPrefab)
                obj.gameObject.SetActive(false);
        }

        Resume();
    }

    private void Instance_OnGameMenuOpened()
    {
        Pause();
    }

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (GameIsPaused)
        {
            GlobalEventSystem.Instance.CloseMenu();
            MainMenuPrefab.SetActive(false);

        }
        else
        {
            GlobalEventSystem.Instance.OpenMenu();
            MainMenuPrefab.SetActive(true);
        }
    }

    void Resume()
    {
        //disable PauseMenu
        pauseMenuUI.SetActive(false);
        //unfreeze time
        Time.timeScale = 1;

        GameIsPaused = false;
    }

    void Pause()
    {
        //enable PauseMenu
        pauseMenuUI.SetActive(true);
        //freeze time
        Time.timeScale = 0;

        GameIsPaused = true;

    }
}
