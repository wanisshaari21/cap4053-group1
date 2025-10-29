using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public bool useButtonInstead = false;

    private bool playerInside = false;
    private Transform playerTr;

    void Update()
    {
        if (!useButtonInstead || !playerInside) return;

        // Legacy + new input system compatible F key check
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool pressF = UnityEngine.InputSystem.Keyboard.current != null &&
                      UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame;
#else
        bool pressF = Input.GetKeyDown(KeyCode.F);
#endif
        if (!pressF) return;

        if (Time.realtimeSinceStartup < PlayerMemory.ignoreTriggersUntil) return;

        if (playerTr != null)
        {
            PlayerMemory.savedPosition = playerTr.position;
            PlayerMemory.hasSaved = true;
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (useButtonInstead)
        {
            playerInside = true;
            playerTr = other.transform;   // wait for F
        }
        else
        {
            if (Time.realtimeSinceStartup < PlayerMemory.ignoreTriggersUntil) return;

            PlayerMemory.savedPosition = other.transform.position;
            PlayerMemory.hasSaved = true;

            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerTr = null;
        }
    }

    public void OnContinue()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}