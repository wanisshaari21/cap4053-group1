using UnityEngine;

public class PuzzleTrigger2 : MonoBehaviour
{
    public LaserPuzzle laserPuzzle;   // Drag your puzzle manager here
    private bool isPlayerNear = false;
    public static bool isPuzzleActive = false;

    void Update()
    {
        // Open puzzle when near + F
        if (isPlayerNear && !isPuzzleActive && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Puzzle Opened!");
            laserPuzzle.puzzleUI.SetActive(true);
            laserPuzzle.StartPuzzle();
            isPuzzleActive = true;
        }

        // Close puzzle with Escape if active
        if (isPuzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Puzzle Closed!");
            laserPuzzle.EndPuzzle();
            laserPuzzle.puzzleUI.SetActive(false);
            isPuzzleActive = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            // Auto-close puzzle if player walks away while it’s open
            if (isPuzzleActive)
            {
                Debug.Log("Puzzle closed because player left trigger.");
                laserPuzzle.EndPuzzle();
                laserPuzzle.puzzleUI.SetActive(false);
                isPuzzleActive = false;
            }
        }
    }
}
