using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NormalArrowPuzzle : MonoBehaviour
{
    public GameObject puzzleUI; // Panel
    public Image[] arrowSlots; // 4 arrow images
    public Sprite upArrow, downArrow, leftArrow, rightArrow; // sprites
    public TextMeshProUGUI feedbackText;

    private KeyCode[] correctSequence;
    private int currentIndex = 0;
    private bool puzzleActive = false;
    public PuzzleManager puzzleManager; // assign in Inspector
    public ArrowPuzzleTrigger1 puzzleTrigger;

    [Header("Key Feedback")]
    public Transform upSpawn, downSpawn, leftSpawn, rightSpawn;  // UI empty objects in Canvas
    public GameObject arrowPrefab; // small arrow image prefab
    private GameObject currentArrow; // new

    [Header("Timer Settings")]
    public float puzzleTime = 10f; // seconds to solve puzzle
    private float timeRemaining;
    private bool timerRunning = false;

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
        Invoke("HideArrows", 5f); // Hide after 5 seconds for memory challenge, consider preventing reentering puzzle for 5 seconds with visual cues
        StartTimer();
    }

    void StartTimer()
    {
        timeRemaining = puzzleTime;
        timerRunning = true;
    }

    public void EndPuzzle()
    {
        puzzleUI.SetActive(false);
        puzzleActive = false;
    }

    void Update()
    {
        if (!puzzleActive) return;

        // Countdown timer
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            feedbackText.text = $"Time: {Mathf.Ceil(timeRemaining)}";

            if (timeRemaining <= 0f)
            {
                timerRunning = false;
                PuzzleFail();
                return;
            }
        }

        KeyCode pressedKey = KeyCode.None;

        // Detect only WASD keys
        if (Input.GetKeyDown(KeyCode.W)) pressedKey = KeyCode.W;
        else if (Input.GetKeyDown(KeyCode.A)) pressedKey = KeyCode.A;
        else if (Input.GetKeyDown(KeyCode.S)) pressedKey = KeyCode.S;
        else if (Input.GetKeyDown(KeyCode.D)) pressedKey = KeyCode.D;

        // If a WASD key was pressed, check it
        if (pressedKey != KeyCode.None)
        {
            // Hide the puzzle arrows immediately
            HideArrows();

            // Spawn a visual arrow for pressed key
            SpawnPressedArrow(pressedKey);

            Debug.Log($"Key Pressed: {pressedKey}");

            if (pressedKey == correctSequence[currentIndex])
            {
                Debug.Log($"Correct! Expected {correctSequence[currentIndex]}, you pressed it.");
                currentIndex++;
                if (currentIndex >= correctSequence.Length)
                {
                    timerRunning = false; // stop timer
                    PuzzleWin();
                }
            }
            else
            {
                timerRunning = false; // stop timer
                Debug.Log($"Wrong! Expected {correctSequence[currentIndex]}.");
                PuzzleFail();
            }
        }
    }

    void SpawnPressedArrow(KeyCode key)
    {
        Transform spawnPoint = null;
        Sprite sprite = null;

        switch (key)
        {
            case KeyCode.W:
                spawnPoint = upSpawn;
                sprite = upArrow;
                break;
            case KeyCode.S:
                spawnPoint = downSpawn;
                sprite = downArrow;
                break;
            case KeyCode.A:
                spawnPoint = leftSpawn;
                sprite = leftArrow;
                break;
            case KeyCode.D:
                spawnPoint = rightSpawn;
                sprite = rightArrow;
                break;
        }

        if (spawnPoint != null && arrowPrefab != null)
        {
            // Destroy previous arrow if it exists
            if (currentArrow != null)
            {
                Destroy(currentArrow);
            }

            currentArrow = Instantiate(arrowPrefab, spawnPoint);
            currentArrow.GetComponent<Image>().sprite = sprite;

            // Offset from parent (in local pixels)
            RectTransform rt = currentArrow.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(750f, 0f); // adjust as needed

            // Optional: destroy automatically after short time
            Destroy(currentArrow, 0.5f);
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
                case 0: // Up arrow
                    arrowSlots[i].sprite = upArrow;
                    correctSequence[i] = KeyCode.W; // press W to match ↑
                    Debug.Log($"Arrow {i}: UP → Expect W");
                    break;
                case 1: // Down arrow
                    arrowSlots[i].sprite = downArrow;
                    correctSequence[i] = KeyCode.S; // press S to match ↓
                    Debug.Log($"Arrow {i}: DOWN → Expect S");
                    break;
                case 2: // Left arrow
                    arrowSlots[i].sprite = leftArrow;
                    correctSequence[i] = KeyCode.A; // press A to match ←
                    Debug.Log($"Arrow {i}: LEFT → Expect A");
                    break;
                case 3: // Right arrow
                    arrowSlots[i].sprite = rightArrow;
                    correctSequence[i] = KeyCode.D; // press D to match →
                    Debug.Log($"Arrow {i}: RIGHT → Expect D");
                    break;
            }

        }
    }


    void ShowArrows()
    {
        foreach (var arrow in arrowSlots)
        {
            arrow.enabled = true;
            arrow.color = Color.yellow; // tint to yellow
        }
    }

    void HideArrows()
    {
        foreach (var arrow in arrowSlots)
            arrow.enabled = false;
    }

    private IEnumerator EndPuzzleAfterDelay(float delay)
    {
        // Keep blocking movement during the delay
        yield return new WaitForSeconds(delay);

        // Now hide the UI and unblock the player
        puzzleUI.SetActive(false);

        // Tell PuzzleTrigger the puzzle is over
        ArrowPuzzleTrigger1.isPuzzleActive = false;
    }

    void PuzzleWin()
    {
        feedbackText.text = "Success!";
        puzzleActive = false;

        if (puzzleManager != null)
            puzzleManager.PuzzleCompleted();
        if (puzzleTrigger != null)
            puzzleTrigger.PuzzleCompleted();

        StartCoroutine(EndPuzzleAfterDelay(2f));
    }

    void PuzzleFail()
    {
        feedbackText.text = "Failed!";
        puzzleActive = false;
        // TODO: subtract time / penalty
        // Hide puzzle UI automatically after 2 seconds
        StartCoroutine(EndPuzzleAfterDelay(2f));
    }
}