using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;  
    public GameObject audioPanel;   

    private bool isPaused = false;

    void Start()
    {
        // make sure everything starts hidden
        if (pausePanel) pausePanel.SetActive(false);
        if (audioPanel) audioPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                OpenPause();
            }
            else
            {
                // If audio panel is open, go back to pause panel
                if (audioPanel && audioPanel.activeSelf)
                {
                    ShowPauseFromAudio();
                }
                else
                {
                    Resume();
                }
            }
        }
    }

    public void OpenPause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel) pausePanel.SetActive(true);
        if (audioPanel) audioPanel.SetActive(false);  
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel) pausePanel.SetActive(false);
        if (audioPanel) audioPanel.SetActive(false);   
    }

    public void OpenAudioPanel()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (audioPanel) audioPanel.SetActive(true);
    }

    public void ShowPauseFromAudio()
    {
        if (audioPanel) audioPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit from pause menu");
        Application.Quit();
    }
}
