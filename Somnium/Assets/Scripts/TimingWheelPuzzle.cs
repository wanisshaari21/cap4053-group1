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
    public EnemyBrain enemy;
    public Transform puzzleMarker;
    public WheelPuzzleTrigger puzzleTrigger;

    public Transform cursorPivot;
    public float rotationSpeed = 60f; // degrees per second

    public Phase2TimingWheel Phase2Wheel;

    void Start()
    {
        puzzleUI.SetActive(false);
    }

    public void StartPuzzle()
    {
        puzzleUI.SetActive(true);
        feedbackText.text = "Timing Puzzle Start!";
        currentRound = 0;
        totalScore = 0f;
        puzzleActive = true;

        // Randomize zones immediately
        RandomizeZones();

        StartCoroutine(RunRounds());
    }

    void Update()
    {
        if (!puzzleActive) return;

        //// rotate pointer
        //pointerAngle += rotationSpeed * Time.deltaTime;
        //pointerImage.rectTransform.rotation = Quaternion.Euler(0, 0, -pointerAngle); // clockwise

        cursorPivot.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

        // check input
        if (Input.GetKeyDown(KeyCode.Space))
            CheckTiming();
    }

    void CheckTiming()
    {
        // normalize angle [0,360)
        //float normalizedAngle = cursorPivot.eulerAngles.z % 360f;
        float relativeAngle = (cursorPivot.eulerAngles.z - hitZonePivot.eulerAngles.z + 360f) % 360f;

        float hitStart = 0f;
        float hitEnd = hitZoneAngle;
        float perfectStart = (hitZoneAngle - perfectZoneAngle) / 2f;
        float perfectEnd = perfectStart + perfectZoneAngle;

        if (relativeAngle >= perfectStart && relativeAngle <= perfectEnd)
        {
            feedbackText.text = "Perfect!";
            totalScore += perfectScore;
        }
        else if (relativeAngle <= hitEnd)
        {
            feedbackText.text = "Good!";
            totalScore += goodScore;
        }
        else
        {
            feedbackText.text = "Miss!";
        }

        currentRound++;

        if (currentRound < totalRounds)
            StartCoroutine(NextRoundDelay());
        else
            EndPuzzle();
    }

    IEnumerator NextRoundDelay()
    {
        puzzleActive = false;
        yield return new WaitForSeconds(1f);
        RandomizeZones();
        puzzleActive = true;
    }

    void RandomizeZones()
    {
        float randomAngle = Random.Range(0f, 360f);
        hitZonePivot.rotation = Quaternion.Euler(0, 0, -randomAngle);

    }

    public float telegraphAngle; // angle of AOE based on Phase 1

    public void EndPuzzle()
    {
        puzzleActive = false;
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

        if (puzzleTrigger != null)
            puzzleTrigger.PuzzleCompleted();

        Debug.Log("Starting Phase 2 Timing Wheel with telegraphAngle: " + telegraphAngle);

        // Launch Phase 2 Timing Wheel
        StartPhase2TimingWheel();
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
