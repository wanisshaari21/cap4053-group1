using UnityEngine;

public class PuzzleBoxActivator : MonoBehaviour
{
    [Header("Puzzle UI")]
    public GameObject puzzleUI;              // Panel or Canvas to show
    public MonoBehaviour playerMovement;     // Your movement script

    bool playerInRange = false;
    bool puzzleActive = false;

    void Update()
    {
        if (playerInRange && !puzzleActive && Input.GetKeyDown(KeyCode.F))
        {
            OpenPuzzle();
        }
    }

    void OpenPuzzle()
    {
        puzzleActive = true;
        if (puzzleUI != null) puzzleUI.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;
    }

    public void ClosePuzzle()
    {
        puzzleActive = false;
        if (puzzleUI != null) puzzleUI.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
