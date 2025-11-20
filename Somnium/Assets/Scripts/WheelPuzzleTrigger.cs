using UnityEngine;

public class WheelPuzzleTrigger : MonoBehaviour
{
    public TimingWheelPuzzle wheelPuzzle;  // Drag your UI Panel here in the Inspector
    private bool isPlayerNear = false;
    public static bool isPuzzleActive = false;
    private bool puzzleEnterable = true;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && puzzleEnterable == true)
        {
            Debug.Log("Puzzle Opened!");
            wheelPuzzle.StartPuzzle();
            isPuzzleActive = true;
        }
        else if (isPlayerNear && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Puzzle Closed!");
            wheelPuzzle.EscapePuzzle();
            isPuzzleActive = false;
        }
    }

    public void PuzzleCompleted()
    {
        puzzleEnterable = false;
        Debug.Log("Puzzle enterable is now false");
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
                wheelPuzzle.puzzleUI.SetActive(false);
                isPuzzleActive = false;
            }
        }
    }
}