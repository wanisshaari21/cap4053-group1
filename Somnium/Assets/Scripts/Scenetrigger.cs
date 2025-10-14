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

        // ⬇ NEW: block scene changes while puzzle UI is up
        if (PuzzleTrigger.isPuzzleActive) return;

        if (Time.realtimeSinceStartup < PlayerMemory.ignoreTriggersUntil) return;

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