using UnityEngine;
using TMPro;
using System.Collections;

public class PuzzleBoxActivator : MonoBehaviour
{
    [Header("Puzzle UI")]
    public GameObject puzzleUI;              // Panel or Canvas to show
    public MonoBehaviour playerMovement;     // Your movement script

    [Header("Optional gate")]
    public bool requiresFirstPuzzle = false;     // ✔ check this ONLY on puzzle 2 box
    public TMP_Text gateMessageText;             // message text in the main HUD
    [TextArea]
    public string gateMessage = "You must complete the first puzzle before opening this one.";
    public float gateMessageDuration = 2f;

    bool playerInRange = false;
    bool puzzleActive = false;
    Coroutine gateRoutine;

    void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(KeyCode.F))
            return;

        if (puzzleActive)
            return;

        // Gate logic for second puzzle
        if (requiresFirstPuzzle && !TombstoneMatchingPuzzle.firstPuzzleCompleted)
        {
            ShowGateMessage();
            return;
        }

        OpenPuzzle();
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
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void ShowGateMessage()
    {
        if (gateMessageText == null)
            return;

        if (gateRoutine != null)
            StopCoroutine(gateRoutine);

        gateRoutine = StartCoroutine(GateMessageCoroutine());
    }

    IEnumerator GateMessageCoroutine()
    {
        gateMessageText.gameObject.SetActive(true);
        gateMessageText.text = gateMessage;

        yield return new WaitForSeconds(gateMessageDuration);

        gateMessageText.gameObject.SetActive(false);
    }
}
