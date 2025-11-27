using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "Level1";   
    public string tutorialSceneName = "Tutorial";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void ToggleAudio()
    {
        if (AudioManager.I != null)
        {
            AudioManager.I.ToggleMute();
        }
        else
        {
            Debug.LogWarning("AudioManager.I is null – is there an AudioManager in the first scene?");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game requested");
    }
}
