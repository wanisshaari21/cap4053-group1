using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;  

    bool isOpen = false;

    void Start()
    {
        CloseMenu();   // hidden on start
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        isOpen = true;
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f;   // pause game
    }

    public void CloseMenu()
    {
        isOpen = false;
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;   // unpause
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleAudio()
    {
        if (AudioManager.I != null)
            AudioManager.I.ToggleMute();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit from pause menu");
    }
}
