using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LaserPuzzle : MonoBehaviour
{
    [Header("UI Elements")]
    public Image[] tiles;              // Assign Tile_0_0, Tile_0_1, ..., Tile_2_2
    public TextMeshProUGUI feedbackText;
    public GameObject puzzleUI;        // reference to LaserPuzzlePanel
    public GameObject instructionsPanel; // assign in Inspector

    [Header("Settings")]
    public Color defaultColor = Color.white;
    public Color safeColor = Color.green;
    public Color laserColor = Color.red;
    public float highlightDuration = 2f;
    public float laserStepDuration = 0.5f;

    private int safeTileIndex;
    private bool puzzleActive = false;

    void Start()
    {
        puzzleUI.SetActive(false);

        // Initialize tiles
        foreach (var tile in tiles)
            tile.color = defaultColor;
    }
    public void StartPuzzle()
    {
        puzzleUI.SetActive(true);
        puzzleActive = true;
        feedbackText.text = "";

        // Show instructions overlay
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);

        // Reset tiles
        foreach (var tile in tiles)
            tile.color = defaultColor;

        // Pick random safe tile
        safeTileIndex = Random.Range(0, tiles.Length);

        // Delay input enable for 1 frame
        StartCoroutine(StartPuzzleRoutine());
    }

    private System.Collections.IEnumerator StartPuzzleRoutine()
    {
        // Wait one frame to let the UI system refresh
        yield return null;

        // Now show safe tile and start logic
        StartCoroutine(ShowSafeTile());
    }

    System.Collections.IEnumerator ShowSafeTile()
    {
        tiles[safeTileIndex].color = safeColor;
        yield return new WaitForSeconds(highlightDuration);
        tiles[safeTileIndex].color = defaultColor;

        // Start laser animation
        StartCoroutine(FireLaser());
    }

    System.Collections.IEnumerator FireLaser()
    {
        int[] indices = new int[tiles.Length];
        for (int i = 0; i < tiles.Length; i++) indices[i] = i;

        // Shuffle indices
        for (int i = 0; i < indices.Length; i++)
        {
            int temp = indices[i];
            int rand = Random.Range(i, indices.Length);
            indices[i] = indices[rand];
            indices[rand] = temp;
        }

        foreach (int i in indices)
        {
            if (i == safeTileIndex) continue;

            tiles[i].color = laserColor;
            yield return new WaitForSeconds(laserStepDuration);
            tiles[i].color = defaultColor;
        }

        // After animation, allow mouse input
        Debug.Log("Select the tile by clicking on it.");
    }

    void Update()
    {
        if (!puzzleActive) return;

        if (Input.GetMouseButtonDown(0)) // left click
        {
            // Check which tile was clicked
            for (int i = 0; i < tiles.Length; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                        tiles[i].rectTransform,
                        Input.mousePosition,
                        null)) // camera null for Overlay canvas
                {
                    Debug.Log($"Clicked tile {i}");
                    CheckTile(i);
                    break;
                }
            }
        }
    }

    void CheckTile(int tileIndex)
    {
        if (tileIndex == safeTileIndex)
        {
            Debug.Log("Puzzle Win!");
            feedbackText.text = "Success!";
        }
        else
        {
            Debug.Log("Puzzle Fail!");
            feedbackText.text = "Failed!";
        }

        puzzleActive = false;
    }

    public void EndPuzzle()
    {
        puzzleUI.SetActive(false);
        puzzleActive = false;

        foreach (var tile in tiles)
            tile.color = defaultColor;
    }
}