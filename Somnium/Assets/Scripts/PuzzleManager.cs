using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public int totalPuzzles = 2; // total number of puzzles to complete
    private int puzzlesCompleted = 0;

    [Header("Gate Settings")]
    public Tilemap tilemap;                  // assign the Tilemap with your gate
    public TileBase[] openGateTiles;         // array of open-gate tiles, in order
    public GateTileFinder gateTileFinder;    // reference to the script that detects positions
    public GameObject swapper;

    /// <summary>
    /// Call this from each puzzle when completed.
    /// </summary>
    public void PuzzleCompleted()
    {
        puzzlesCompleted++;

        Debug.Log($"Puzzles completed: {puzzlesCompleted}/{totalPuzzles}");

        if (puzzlesCompleted >= totalPuzzles)
        {
            OpenGate();
        }
    }

    private void OpenGate()
    {
        if (tilemap == null || gateTileFinder == null)
        {
            Debug.LogError("Tilemap or GateTileFinder not assigned!");
            return;
        }

        // get positions from GateTileFinder
        Vector3Int[] positions = gateTileFinder.gateTilePositions.ToArray();

        if (positions.Length != openGateTiles.Length)
        {
            Debug.LogError("Mismatch: positions array and openGateTiles array must have the same length!");
            return;
        }

        // replace each tile with its corresponding open-tile
        for (int i = 0; i < positions.Length; i++)
        {
            tilemap.SetTile(positions[i], openGateTiles[i]);
        }

        swapper.SetActive(true);

        Debug.Log("Gate opened successfully!");
    }
}