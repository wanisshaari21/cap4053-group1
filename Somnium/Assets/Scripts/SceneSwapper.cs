using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeToScene(sceneName);
            }
            else
            {
                Debug.LogWarning("No SceneFader found — loading directly!");
                SceneManager.LoadScene(sceneName);
            }

        }
    }
}