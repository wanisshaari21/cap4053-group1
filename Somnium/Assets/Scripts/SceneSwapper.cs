using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //dont forget me!
public class SceneSwapper : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ⬇ NEW: block scene changes while puzzle UI is up
        if (PuzzleTrigger.isPuzzleActive) return;

        var player = collision.GetComponent<PlayerController>();
        if (player)
            SceneManager.LoadScene(sceneName);
    }
}
