using UnityEngine;
using TMPro;
using System.Collections;

public class TombstoneMatchingPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class TombstoneSlot
    {
        public TMP_Text numberText;   // Text on each tombstone
    }

    [Header("Tombstones")]
    public TombstoneSlot[] tombstones;   // size = 4

    [Header("UI")]
    public TMP_Text infoText;            // top text ("Matched!", "Try again")
    public TMP_Text triesText;           // bottom text ("Tries left: X")

    [Header("Links")]
    public PuzzleBoxActivator activator; // your box trigger script

    [Header("Settings")]
    public float flipBackDelay = 0.8f;   // how long before wrong pair flips back
    public float exitDelay = 1.5f;       // delay before leaving puzzle on win/fail
    public int maxTries = 2;             // how many wrong tries before fail

    private int[] values = new int[4];   // hidden numbers
    private int firstIndex = -1;         // first clicked index
    private bool inputLocked = false;
    private int pairsFound = 0;
    private int triesLeft;

    // make sure we don't exit multiple times
    private bool isExiting = false;

    void OnEnable()
    {
        SetupRound();
    }

    void SetupRound()
    {
        // two pairs: 1,1,2,2
        values[0] = 1;
        values[1] = 1;
        values[2] = 2;
        values[3] = 2;

        // shuffle
        for (int i = 0; i < values.Length; i++)
        {
            int r = Random.Range(i, values.Length);
            int temp = values[i];
            values[i] = values[r];
            values[r] = temp;
        }

        firstIndex = -1;
        inputLocked = false;
        pairsFound = 0;
        triesLeft = maxTries;
        isExiting = false;

        // hide all numbers
        for (int i = 0; i < tombstones.Length; i++)
        {
            if (tombstones[i].numberText != null)
                tombstones[i].numberText.text = "?";
        }

        if (infoText != null)
            infoText.text = "Find all matching pairs";

        UpdateTriesText();
    }

    void UpdateTriesText()
    {
        if (triesText != null)
            triesText.text = "Tries left: " + triesLeft;
    }

    // ---- Button wrapper methods for Inspector ----
    public void ClickTomb0() { ClickTomb(0); }
    public void ClickTomb1() { ClickTomb(1); }
    public void ClickTomb2() { ClickTomb(2); }
    public void ClickTomb3() { ClickTomb(3); }
    // ---------------------------------------------

    void ClickTomb(int index)
    {
        if (inputLocked || isExiting || index < 0 || index >= tombstones.Length)
            return;

        if (tombstones[index].numberText == null)
            return;

        // reveal
        tombstones[index].numberText.text = values[index].ToString();

        // first click
        if (firstIndex == -1)
        {
            firstIndex = index;
            return;
        }

        // ignore double-click same stone
        if (index == firstIndex)
            return;

        inputLocked = true;
        StartCoroutine(CheckPairCoroutine(firstIndex, index));
    }

    IEnumerator CheckPairCoroutine(int a, int b)
    {
        // short delay so they see the second number flip
        yield return new WaitForSeconds(0.25f);

        if (values[a] == values[b])
        {
            // ✅ correct pair
            pairsFound++;

            if (infoText != null)
                infoText.text = "Matched!";

            if (pairsFound >= 2)
            {
                // all pairs found -> win
                if (infoText != null)
                    infoText.text = "All pairs found!";

                if (!isExiting)
                    StartCoroutine(ExitPuzzleAfterDelay(true));

                yield break;
            }
        }
        else
        {
            // ❌ wrong pair → consume a try
            triesLeft--;
            UpdateTriesText();

            if (infoText != null)
                infoText.text = "No match...";

            // if out of tries -> lose and exit (no flip-back needed)
            if (triesLeft <= 0)
            {
                if (infoText != null)
                    infoText.text = "You feel a chill...";

                if (!isExiting)
                    StartCoroutine(ExitPuzzleAfterDelay(false));

                yield break;
            }

            // still have tries left -> flip back after delay
            yield return new WaitForSeconds(flipBackDelay);

            if (tombstones[a].numberText != null)
                tombstones[a].numberText.text = "?";
            if (tombstones[b].numberText != null)
                tombstones[b].numberText.text = "?";

            if (infoText != null)
                infoText.text = "Try again";
        }

        firstIndex = -1;
        inputLocked = false;
    }

    IEnumerator ExitPuzzleAfterDelay(bool success)
    {
        isExiting = true;

        // pause so player can read the final text
        yield return new WaitForSeconds(exitDelay);

        if (activator != null)
            activator.ClosePuzzle();
    }
}
