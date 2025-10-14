using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TileSwapSlot
{
    public Vector3Int tilePosition; // position in the tilemap
    public TileBase newTile;        // the tile to set
}

public class SceneSwapperNoDark : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;
    public bool useButtonInstead = false;

    [Header("Door collider to disable before swapping")]
    [SerializeField] private Collider2D doorColliderToDisable;

    [Header("Unique Door ID (must be unique across all scenes)")]
    [SerializeField] private string doorID;

    [Header("Tilemap Swap Settings")]
    [SerializeField] private Tilemap tilemapToSwap;
    [SerializeField] private TileSwapSlot[] tileSwapSlots;
    [SerializeField] private string tileSwapID; // unique ID for remembering swaps

    private bool playerInside = false;
    private Transform playerTr;

    // Static memory
    private static HashSet<string> usedDoors = new HashSet<string>();
    private static HashSet<string> swappedTiles = new HashSet<string>();

    private void Start()
    {
        // Restore door state
        if (!string.IsNullOrEmpty(doorID) && usedDoors.Contains(doorID))
        {
            if (doorColliderToDisable != null)
                doorColliderToDisable.enabled = false;
        }

        // Restore tile swaps if previously applied
        if (!string.IsNullOrEmpty(tileSwapID) && swappedTiles.Contains(tileSwapID))
        {
            ApplyTileSwaps();
            Debug.Log($"[SceneSwapperNoDark] Restored tile swap '{tileSwapID}'.");
        }
    }

    void Update()
    {
        if (!useButtonInstead || !playerInside) return;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool pressF = UnityEngine.InputSystem.Keyboard.current != null &&
                      UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame;
#else
        bool pressF = Input.GetKeyDown(KeyCode.F);
#endif
        if (!pressF) return;

        if (Time.realtimeSinceStartup < PlayerMemory.ignoreTriggersUntil)
            return;

        if (playerTr != null)
        {
            PlayerMemory.savedPosition = playerTr.position;
            PlayerMemory.hasSaved = true;
        }

        DisableDoorAndRecord();
        ApplyTileSwaps();

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (useButtonInstead)
        {
            playerInside = true;
            playerTr = other.transform;
        }
        else
        {
            if (Time.realtimeSinceStartup < PlayerMemory.ignoreTriggersUntil) return;

            PlayerMemory.savedPosition = other.transform.position;
            PlayerMemory.hasSaved = true;

            DisableDoorAndRecord();
            ApplyTileSwaps();

            if (!string.IsNullOrEmpty(sceneToLoad))
                SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerTr = null;
        }
    }

    private void DisableDoorAndRecord()
    {
        if (doorColliderToDisable != null)
            doorColliderToDisable.enabled = false;

        if (!string.IsNullOrEmpty(doorID))
            usedDoors.Add(doorID);
    }

    private void ApplyTileSwaps()
    {
        if (tilemapToSwap == null || tileSwapSlots == null || tileSwapSlots.Length == 0)
            return;

        foreach (var slot in tileSwapSlots)
        {
            if (slot != null && slot.newTile != null)
                tilemapToSwap.SetTile(slot.tilePosition, slot.newTile);
        }

        if (!string.IsNullOrEmpty(tileSwapID))
            swappedTiles.Add(tileSwapID);
    }
}
