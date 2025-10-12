using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BoardSwapCursor : MonoBehaviour
{
    [Header("Grid")]
    public int width = 3;
    public int height = 3;
    public float cellSize = 1f;               // auto set from prefab sprite

    [Header("Tile Prefabs")]
    public GameObject redPrefab;              // SpriteRenderer (and ideally a BoxCollider2D)
    public GameObject greyPrefab;             // SpriteRenderer

    [Header("Start Layout (no blanks)")]
    [Tooltip("Left→right, top→bottom. 1 = red, 0 = grey. Size must be width*height.")]
    public bool usePreset = true;
    public List<int> layout;                  // size = width*height

    [Header("Controls")]
    public float swapSpeed = 12f;             // animation speed
    public KeyCode nextKey = KeyCode.Tab;     // cycle active red forward
    public KeyCode prevKey = KeyCode.Q;       // cycle active red back (also E cycles forward)

    [Header("Selection Visual")]
    public Color selectedColor = new Color(1f, 0.95f, 0.2f, 1f);   // yellow tint
    public Color normalColor = Color.white;

    [Header("UI References")]
    public GameObject winPanel;          // Your WinPanel
    public GameObject instructionsPanel; // Drag your InstructionsCanvas (or parent) here
    public string nextSceneName;

    public delegate void WinAction();
    public static event WinAction onWin;

    [Header("Timeout Exit")]
    public PuzzleTimer timer;        // drag your PuzzleTimer here
    public string exitSceneName;     // type your hub scene name here in Inspector


    // Internals
    private GameObject[,] tiles;              // every cell has a tile
    private List<Vector2Int> redCells = new List<Vector2Int>();
    private List<GameObject> redTiles = new List<GameObject>();
    private int activeRedIndex = 0;
    private bool animating;
    private Vector3 origin;

    void Start()
    {
        // Hide win panel on start
        if (winPanel) winPanel.SetActive(false);
        Time.timeScale = 1f;
        // Show instructions overlay
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);

        // Auto cell size from a prefab sprite
        var sr = (redPrefab ? redPrefab.GetComponent<SpriteRenderer>() : null);
        if ((!sr || !sr.sprite) && greyPrefab) sr = greyPrefab.GetComponent<SpriteRenderer>();
        if (sr && sr.sprite) cellSize = sr.sprite.bounds.size.x * 1.02f;

        origin = new Vector3(-(width - 1) * 0.5f * cellSize, -(height - 1) * 0.5f * cellSize, 0f);
        tiles = new GameObject[width, height];

        if (usePreset && layout != null && layout.Count == width * height)
            BuildFromPreset();
        else
            BuildDefault();

        FitCamera();
        ApplySelectionVisual();
    }

    void BuildFromPreset()
    {
        redCells.Clear();
        redTiles.Clear();

        int i = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++, i++)
            {
                int code = layout[i]; // 1=red, 0=grey
                GameObject prefab = (code == 1) ? redPrefab : greyPrefab;
                var go = Instantiate(prefab, CellToWorld(x, y), Quaternion.identity, transform);
                go.name = (code == 1) ? "Red" : "Grey";
                FitToCell(go);
                tiles[x, y] = go;

                if (code == 1)
                {
                    redCells.Add(new Vector2Int(x, y));
                    redTiles.Add(go);
                }
            }
        }

        if (redTiles.Count > 0) activeRedIndex = 0;
    }

    // Default tutorial layout:
    // Top row:    Red, Red, Grey
    // Middle row: Grey, Grey, Grey
    // Bottom row: Grey, Red, Grey
    void BuildDefault()
    {
        usePreset = true;
        layout = new List<int>()
        {
            1, 1, 0,
            0, 0, 0,
            0, 1, 0
        };
        BuildFromPreset();
    }

    void FitCamera()
    {
        var cam = Camera.main;
        if (!cam) return;
        cam.orthographic = true;
        cam.transform.position = new Vector3(0, 0, -10);
        float padding = cellSize * 0.6f;
        cam.orthographicSize = (height * cellSize) / 2f + padding;
    }

    void FitToCell(GameObject go)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        if (!sr || !sr.sprite) return;
        float w = sr.sprite.bounds.size.x;
        if (w <= 0f) return;
        float s = cellSize / w;
        go.transform.localScale = new Vector3(s, s, 1f);
    }

    void Update()
    {
        if (animating) return;

        // Guard if no reds (shouldn't happen, but safe)
        if (redTiles.Count == 0) return;

        // Cycle selected red
        if (Input.GetKeyDown(nextKey) || Input.GetKeyDown(KeyCode.E))
        {
            activeRedIndex = (activeRedIndex + 1) % redTiles.Count;
            ApplySelectionVisual();
        }
        else if (Input.GetKeyDown(prevKey))
        {
            activeRedIndex = (activeRedIndex - 1 + redTiles.Count) % redTiles.Count;
            ApplySelectionVisual();
        }

        // Move active red by swapping with an adjacent GREY
        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dir = Vector2Int.right;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) dir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) dir = Vector2Int.down;

        if (dir != Vector2Int.zero) TrySwap(dir);
    }

    void ApplySelectionVisual()
    {
        for (int i = 0; i < redTiles.Count; i++)
        {
            var sr = redTiles[i].GetComponent<SpriteRenderer>();
            if (!sr) continue;
            sr.color = (i == activeRedIndex) ? selectedColor : normalColor;
        }
    }

    void TrySwap(Vector2Int dir)
    {
        if (redTiles.Count == 0) return;

        Vector2Int redPos = redCells[activeRedIndex];
        Vector2Int target = redPos + dir;
        if (!InBounds(target)) return;

        // Only swap with GREY
        var other = tiles[target.x, target.y];
        if (!other || other.name != "Grey") return;

        var redGO = tiles[redPos.x, redPos.y];

        // Swap refs in grid
        tiles[target.x, target.y] = redGO;
        tiles[redPos.x, redPos.y] = other;

        // Update stored red position
        redCells[activeRedIndex] = target;

        // Animate both to their new cells
        StartCoroutine(AnimateSwap(
            redGO, CellToWorld(target.x, target.y),
            other, CellToWorld(redPos.x, redPos.y),
            () =>
            {
                ApplySelectionVisual();
                if (RedsConnectedCount() == redTiles.Count)
                {
                    HandleWin();
                }
            }));
    }

    IEnumerator AnimateSwap(GameObject a, Vector3 aTarget, GameObject b, Vector3 bTarget, System.Action after)
    {
        animating = true;
        while (true)
        {
            bool aDone = !a || Vector3.Distance(a.transform.position, aTarget) <= 0.01f;
            bool bDone = !b || Vector3.Distance(b.transform.position, bTarget) <= 0.01f;

            if (!aDone && a) a.transform.position = Vector3.MoveTowards(a.transform.position, aTarget, swapSpeed * Time.deltaTime);
            if (!bDone && b) b.transform.position = Vector3.MoveTowards(b.transform.position, bTarget, swapSpeed * Time.deltaTime);

            if (aDone && bDone) break;
            yield return null;
        }
        animating = false;
        after?.Invoke();
    }

    int RedsConnectedCount()
    {
        if (redTiles.Count == 0) return 0;

        var redSet = new HashSet<Vector2Int>(redCells);
        var seen = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(redCells[0]);
        seen.Add(redCells[0]);

        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            foreach (var d in dirs)
            {
                var n = p + d;
                if (!InBounds(n)) continue;
                if (!redSet.Contains(n)) continue;
                if (seen.Add(n)) q.Enqueue(n);
            }
        }
        return seen.Count;
    }

    bool InBounds(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    Vector3 CellToWorld(int x, int y) => origin + new Vector3(x * cellSize, y * cellSize, 0f);

    void HandleWin()
    {
        onWin?.Invoke();

        // Show win panel
        if (winPanel) winPanel.SetActive(true);

        // Hide instructions
        if (instructionsPanel) instructionsPanel.SetActive(false);

        // Pause gameplay
        Time.timeScale = 0f;
    }

    public void OnWinClose()
    {
        if (winPanel) winPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnWinGoToNextScene()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // called by PuzzleTimer.OnExpired
    public void FailDueToTimeout()
    {
        if (timer != null) timer.StopTimer();

        // Save player position before leaving
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMemory.savedPosition = player.transform.position;
            PlayerMemory.hasSaved = true;
        }

        Time.timeScale = 1f;
        PlayerMemory.ignoreTriggersUntil = Time.realtimeSinceStartup + 0.75f;

        if (!string.IsNullOrEmpty(exitSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(exitSceneName);
        }
        else
        {
            Debug.LogWarning("BoardSwapCursor: exitSceneName is empty; can't exit on timeout.");
        }
    }

}