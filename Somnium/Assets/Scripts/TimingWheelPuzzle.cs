using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TimingWheelPuzzle : MonoBehaviour
{
    [Header("UI References")]
    public GameObject puzzleUI;
    public Image wheelImage;
    public Image pointerImage;
    public Image hitZone;
    public Image perfectZone;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI feedbackText2;
    public Transform hitZonePivot;   // parent of hitZone and perfectZone

    [Header("Settings")]
    public int totalRounds = 4;
    //public float rotationSpeed = 180f; // degrees per second
    public float hitZoneAngle = 60f;   // how wide white area is
    public float perfectZoneAngle = 15f; // small inner region
    public float perfectScore = 25f;
    public float goodScore = 10f;

    private int currentRound = 0;
    private float totalScore = 0f;
    private bool puzzleActive = false;
    private float pointerAngle = 0f;

    [Header("External")]
    public SpiderBrain enemy;
    public Transform puzzleMarker;
    public PuzzleManager puzzleManager; // assign in Inspector
    public WheelPuzzleTrigger puzzleTrigger;

    public Transform cursorPivot;
    public float rotationSpeed = 60f; // degrees per second

    public Phase2TimingWheel Phase2Wheel;

    private bool roundNotEnded = true;

    [Header("Timer")]
    public float roundTime = 10f; // seconds allowed per round
    private float timeRemaining;
    private bool timerRunning = false;

    [Header("Score Requirement")]
    public float requiredScore = 60f;

    void Start()
    {
        puzzleUI.SetActive(false);
    }

    public void StartPuzzle()
    {
        puzzleUI.SetActive(true);
        currentRound = 0;
        totalScore = 0f;
        puzzleActive = true;

        // Randomize zones immediately
        RandomizeZones();
        timeRemaining = roundTime;
        timerRunning = true;

        StartCoroutine(RunRounds());
    }

    void Update()
    { 
        if (!puzzleActive) return;

        if (roundNotEnded)
        {
            feedbackText.text = $"{currentRound + 1}/{totalRounds}\n";
        }

        //// rotate pointer
        //pointerAngle += rotationSpeed * Time.deltaTime;
        //pointerImage.rectTransform.rotation = Quaternion.Euler(0, 0, -pointerAngle); // clockwise

        // Countdown timer
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            feedbackText.text = $"{currentRound + 1}/{totalRounds}\nTime: {Mathf.Ceil(timeRemaining)}\n";

            if (timeRemaining <= 0f)
            {
                timerRunning = false;
                TimerFail();
                return;
            }
        }

        cursorPivot.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

        // check input
        if (Input.GetKeyDown(KeyCode.Space))
            CheckTiming();

    }

    void TimerFail()
    {
        feedbackText.text = "Failed!";
        puzzleActive = false;

        if (enemy != null)
        {
            enemy.EnterGoToPuzzle(puzzleMarker);
            Debug.Log("Enemy not null");
        }
        if (enemy == null)
        {
            Debug.Log("Enemy null");
        }
        // Hide puzzle UI automatically after 2 seconds
        StartCoroutine(EndPuzzleAfterDelay(2f));
    }


    void CheckTiming()
    {
        timerRunning = false;

        roundNotEnded = false;
        // normalize angle [0,360)
        //float normalizedAngle = cursorPivot.eulerAngles.z % 360f;
        float relativeAngle = (cursorPivot.eulerAngles.z - hitZonePivot.eulerAngles.z + 360f) % 360f;

        float halfHit = hitZoneAngle / 2f;
        float halfPerfect = perfectZoneAngle / 2f;

        // Normalize (-180, +180)
        float centerAngle = (relativeAngle + 360f) % 360f;
        if (centerAngle > 180f) centerAngle -= 360f;

        // PERFECT: within tiny symmetric window
        if (Mathf.Abs(centerAngle) <= halfPerfect)
        {
            totalScore += perfectScore;
            feedbackText.text = "Perfect! (+25)";
            feedbackText2.text = "Hit spacebar at the correct timing!\n" + $"Score: {totalScore} / {requiredScore}";
            currentRound++;

            if (currentRound < totalRounds)
                StartCoroutine(NextRoundDelay());
            else
                EndPuzzle();
        }
        // GOOD: within larger symmetric window
        else if (Mathf.Abs(centerAngle) <= halfHit)
        {
            totalScore += goodScore;
            feedbackText.text = "Good! (+10)";
            feedbackText2.text = "Hit spacebar at the correct timing!\n" + $"Score: {totalScore} / {requiredScore}";
            currentRound++;

            if (currentRound < totalRounds)
                StartCoroutine(NextRoundDelay());
            else
                EndPuzzle();
        }
        else
        {
            feedbackText.text = "Failed!";
            puzzleActive = false;
            if (enemy != null)
            {
                enemy.EnterGoToPuzzle(puzzleMarker);
                Debug.Log("Enemy not null");
            }
            if (enemy == null)
            {
                Debug.Log("Enemy null");
            }
            // Hide puzzle UI automatically after 2 seconds
            StartCoroutine(EndPuzzleAfterDelay(2f));
        }
    }

    void EndPuzzle()
    {
        if (totalScore >= requiredScore) // At the moment 60
        {
            feedbackText.text = "Success!";
            puzzleActive = false;
            if (puzzleManager != null)
                puzzleManager.PuzzleCompleted();
            if (puzzleTrigger != null)
                puzzleTrigger.PuzzleCompleted();
            StartCoroutine(EndPuzzleDelay(2f));
        }

        else
        {
            feedbackText.text = "Failed! Score not high enough.";
            puzzleActive = false;
            if (enemy != null)
            {
                enemy.EnterGoToPuzzle(puzzleMarker);
            }
            // Hide puzzle UI automatically after 2 seconds
            StartCoroutine(EndPuzzleAfterDelay(2f));
        }
    }

    IEnumerator NextRoundDelay()
    {
        puzzleActive = false;
        yield return new WaitForSeconds(1f);

        RandomizeZones();
        roundNotEnded = true;

        timeRemaining = roundTime;
        timerRunning = true;

        puzzleActive = true;
    }

    void RandomizeZones()
    {
        float randomAngle;

        // repeat until outside forbidden region
        do
        {
            randomAngle = Random.Range(0f, 360f);
        }
        while (randomAngle >= 195f && randomAngle <= 345f); // forbidden North region

        hitZonePivot.rotation = Quaternion.Euler(0, 0, -randomAngle);

    }

    public float telegraphAngle; // angle of AOE based on Phase 1

    public void EscapePuzzle()
    {
        puzzleActive = false;
        puzzleUI.SetActive(false);
    }

    private IEnumerator EndPuzzleDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        puzzleUI.SetActive(false);

        if (totalScore >= perfectScore * totalRounds)
        {
            feedbackText.text = "Perfect Timing!";
            telegraphAngle = 90f; // only 1 cardinal is safe
            Debug.Log("Phase 2: 90° telegraph (1 safe spot)");
        }
        else if (totalScore >= goodScore * totalRounds)
        {
            feedbackText.text = "Good Timing!";
            telegraphAngle = 270f; // 3 cardinals hit
            Debug.Log("Phase 2: 270° telegraph (1 safe spot)");
        }
        else
        {
            feedbackText.text = "Failed!";
            telegraphAngle = 360f; // unavoidable
            Debug.Log("Phase 2: 360° telegraph (guaranteed hit)");
            if (enemy != null) enemy.EnterGoToPuzzle(puzzleMarker);
        }

        //// Launch Phase 2 Timing Wheel
        //StartPhase2TimingWheel();
    }

    private IEnumerator EndPuzzleAfterDelay(float delay)
    {
        // Keep blocking movement during the delay
        yield return new WaitForSeconds(delay);

        // Now hide the UI and unblock the player
        puzzleUI.SetActive(false);

        // Tell PuzzleTrigger the puzzle is over
        WheelPuzzleTrigger.isPuzzleActive = false;
    }

    void StartPhase2TimingWheel()
    {
        // Phase 2 logic: show telegraph in middle, spawn cursor/timing wheel
        // Set the safe spot based on telegraphAngle
        if (Phase2Wheel == null)
        {
            Debug.LogError("Phase2Wheel not assigned in inspector!");
            return;
        }

        Debug.Log("Starting Phase 2 Timing Wheel with telegraphAngle: " + telegraphAngle);
        Phase2Wheel.Setup(telegraphAngle);
        Phase2Wheel.StartPuzzle();
    }

    IEnumerator RunRounds()
    {
        yield return new WaitForSeconds(1f);
    }
}
