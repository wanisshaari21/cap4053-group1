using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    public ArrowPuzzle arrowPuzzle;  // Drag your UI Panel here in the Inspector
    private bool isPlayerNear = false;
    public static bool isPuzzleActive = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Puzzle Opened!");
            arrowPuzzle.StartPuzzle();
            isPuzzleActive = true;
        }
        else if (isPlayerNear && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Puzzle Closed!");
            arrowPuzzle.EndPuzzle();
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

            // Optional: hide puzzle if player walks away
            if (isPuzzleActive)
            {
                arrowPuzzle.puzzleUI.SetActive(false);
                isPuzzleActive = false;
            }
        }
    }
}