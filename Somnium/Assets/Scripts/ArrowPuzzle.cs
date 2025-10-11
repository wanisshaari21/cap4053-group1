using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArrowPuzzle : MonoBehaviour
{
    public GameObject puzzleUI; // Panel
    public Image[] arrowSlots; // 4 arrow images
    public Sprite upArrow, downArrow, leftArrow, rightArrow; // sprites
    public TextMeshProUGUI feedbackText;

    private KeyCode[] correctSequence;
    private int currentIndex = 0;
    private bool puzzleActive = false;
    public PuzzleManager puzzleManager; // assign in Inspector
    public PuzzleTrigger puzzleTrigger;

    void Start()
    {
        puzzleUI.SetActive(false);
    }

    public void StartPuzzle()
    {
        currentIndex = 0;
        feedbackText.text = "New text";
        puzzleUI.SetActive(true);
        puzzleActive = true;
        GenerateSequence();
        ShowArrows();
        Invoke("HideArrows", 2f); // Hide after 2 seconds for memory challenge
    }

    public void EndPuzzle()
    {
        puzzleUI.SetActive(false);
        puzzleActive = false;
    }

    void Update()
    {
        if (!puzzleActive) return;

        KeyCode pressedKey = KeyCode.None;

        // Detect only WASD keys
        if (Input.GetKeyDown(KeyCode.W)) pressedKey = KeyCode.W;
        else if (Input.GetKeyDown(KeyCode.A)) pressedKey = KeyCode.A;
        else if (Input.GetKeyDown(KeyCode.S)) pressedKey = KeyCode.S;
        else if (Input.GetKeyDown(KeyCode.D)) pressedKey = KeyCode.D;

        // If a WASD key was pressed, check it
        if (pressedKey != KeyCode.None)
        {
            Debug.Log($"Key Pressed: {pressedKey}");

            if (pressedKey == correctSequence[currentIndex])
            {
                Debug.Log($"Correct! Expected {correctSequence[currentIndex]}, you pressed it.");
                currentIndex++;
                if (currentIndex >= correctSequence.Length)
                {
                    PuzzleWin();
                }
            }
            else
            {
                Debug.Log($"Wrong! Expected {correctSequence[currentIndex]}.");
                PuzzleFail();
            }
        }
    }

    void GenerateSequence()
    {
        // Example: pick random 4 arrows
        correctSequence = new KeyCode[4];

        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    arrowSlots[i].sprite = upArrow;
                    correctSequence[i] = KeyCode.S; // inverse ↓
                    Debug.Log($"Arrow {i}: UP → Expect S");
                    break;
                case 1:
                    arrowSlots[i].sprite = downArrow;
                    correctSequence[i] = KeyCode.W; // inverse ↑
                    Debug.Log($"Arrow {i}: DOWN → Expect W");
                    break;
                case 2:
                    arrowSlots[i].sprite = leftArrow;
                    correctSequence[i] = KeyCode.D; // inverse →
                    Debug.Log($"Arrow {i}: LEFT → Expect D");
                    break;
                case 3:
                    arrowSlots[i].sprite = rightArrow;
                    correctSequence[i] = KeyCode.A; // inverse ←
                    Debug.Log($"Arrow {i}: RIGHT → Expect A");
                    break;
            }
        }
    }


    void ShowArrows()
    {
        foreach (var arrow in arrowSlots)
            arrow.enabled = true;
    }

    void HideArrows()
    {
        foreach (var arrow in arrowSlots)
            arrow.enabled = false;
    }

    void PuzzleWin()
    {
        feedbackText.text = "Success!";
        puzzleActive = false;

        if (puzzleManager != null)
            puzzleManager.PuzzleCompleted(); // notify manager
                                             // TODO: call back to PuzzleTrigger to close puzzle
        if (puzzleTrigger != null)
            puzzleTrigger.PuzzleCompleted();
    }

    void PuzzleFail()
    {
        feedbackText.text = "Failed!";
        puzzleActive = false;
        // TODO: subtract time / penalty
    }
}