using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EnemyCatchTrigger : MonoBehaviour
{
    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";
    [Tooltip("Delay before reloading the level after being caught")]
    public float reloadDelay = 1f;
    private Animator animator;

    void OnTriggerEnter2D(Collider2D other)
    {
        // This runs automatically when the enemy's trigger overlaps another collider
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Caught the player!");

            animator = other.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.Log("Animator is null");
            }
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
                Debug.Log("Re-enabled animator");
            }
                
            StartCoroutine(RestartLevel());
        }
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
