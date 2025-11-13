using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Phase2TimingWheel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject puzzleUI;
    public Image telegraphImage; // the AOE graphic
    public Transform cursorPivot;
    public Image cursorImage;
    public TextMeshProUGUI feedbackText;

    [Header("Settings")]
    public float rotationSpeed = 90f; // degrees per second
    private float safeSpotAngle;      // start of safe spot in degrees
    private float telegraphAngle;     // size of the AOE from Phase 1
    private bool puzzleActive = false;

    [Header("External")]
    public EnemyBrain enemy;
    public Transform puzzleMarker;

    public void Setup(float phase1TelegraphAngle)
    {
        telegraphAngle = phase1TelegraphAngle;

        // Determine a random starting angle for telegraph
        float telegraphStartAngle = Random.Range(0f, 360f - telegraphAngle);
        safeSpotAngle = (telegraphStartAngle + telegraphAngle) % 360f; // safe spot is the gap

        // Rotate the telegraph to match Phase 1 result
        telegraphImage.rectTransform.rotation = Quaternion.Euler(0, 0, -telegraphStartAngle);
        telegraphImage.fillAmount = telegraphAngle / 360f; // assuming Image Type = Filled, Radial360
    }

    void Start()
    {
        puzzleUI.SetActive(false);
    }

    public void StartPuzzle()
    {
        puzzleUI.SetActive(true);
        feedbackText.text = "Phase 2: Find the safe spot!";
        puzzleActive = true;
    }

    void Update()
    {
        if (!puzzleActive) return;

        // Rotate cursor
        cursorPivot.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

        // Check player input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckSafeSpot();
        }
    }

    void CheckSafeSpot()
    {
        float cursorAngle = cursorPivot.eulerAngles.z % 360f;

        // Determine safe range (safe spot is opposite of telegraph)
        float safeStart = (safeSpotAngle + telegraphAngle) % 360f;
        float safeEnd = (safeStart + (360f - telegraphAngle)) % 360f;

        bool isSafe = false;

        // Handle angle wrapping
        if (safeEnd > safeStart)
        {
            if (cursorAngle >= safeStart && cursorAngle <= safeEnd) isSafe = true;
        }
        else
        {
            // Wrap around 0
            if (cursorAngle >= safeStart || cursorAngle <= safeEnd) isSafe = true;
        }

        if (isSafe)
        {
            feedbackText.text = "Success! Safe spot chosen!";
            Debug.Log("Phase 2 success! Player avoided the AOE.");
        }
        else
        {
            feedbackText.text = "Hit! You got caught!";
            Debug.Log("Phase 2 fail! Player hit by telegraph.");
            if (enemy != null) enemy.EnterGoToPuzzle(puzzleMarker);
        }

        puzzleActive = false;
        StartCoroutine(EndPuzzleAfterDelay(1f));
    }

    IEnumerator EndPuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        puzzleUI.SetActive(false);
    }
}