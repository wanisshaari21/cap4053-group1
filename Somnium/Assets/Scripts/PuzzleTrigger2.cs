using UnityEngine;

public class PuzzleTrigger2 : MonoBehaviour
{
    public LaserPuzzle laserPuzzle;  // Drag your UI Panel here in the Inspector
    private bool isPlayerNear = false;
    public static bool isPuzzleActive = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Puzzle Opened!");
            laserPuzzle.puzzleUI.SetActive(true);

            Debug.Log($"activeSelf: {laserPuzzle.puzzleUI.activeSelf}, activeInHierarchy: {laserPuzzle.puzzleUI.activeInHierarchy}");


            laserPuzzle.StartPuzzle();
            isPuzzleActive = true;
        }
        else if (isPlayerNear && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Puzzle Closed!");
            laserPuzzle.EndPuzzle();
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
                laserPuzzle.puzzleUI.SetActive(false);
                isPuzzleActive = false;
            }
        }
    }
}