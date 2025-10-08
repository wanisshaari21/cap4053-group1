using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [Header("Scene to load when triggered")]
    public string sceneToLoad;

    [Header("Optional: Use for Continue button")]
    public bool useButtonInstead = false;   // if true, load when OnContinue() is called

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useButtonInstead && other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // 👇 Hook this up to your Continue button
    public void OnContinue()
    {
        Time.timeScale = 1f; // unpause if needed
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
