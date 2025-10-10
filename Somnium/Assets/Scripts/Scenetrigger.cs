using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public bool useButtonInstead = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useButtonInstead) return;
        if (!other.CompareTag("Player")) return;

        // Prevent immediate re-trigger right after returning to the scene
        if (Time.realtimeSinceStartup < PlayerMemory.ignoreTriggersUntil) return;

        // Save where we are in Tutorial before leaving
        PlayerMemory.savedPosition = other.transform.position;
        PlayerMemory.hasSaved = true;

        SceneManager.LoadScene(sceneToLoad);
    }

    public void OnContinue()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}
