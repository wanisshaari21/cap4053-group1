using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostBrain : MonoBehaviour
{
    public EnemyPatrol patrol;
    public VisionCone fov;
    public Transform player;
    public NavMeshAgent agent;

    [Header("Speeds")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6.0f;

    [Header("State Timers")]
    public float loseSightToSearchTime = 3f;
    public float searchDuration = 8f;

    [Header("Vision Tracking")]
    public Transform visionPivot; // assign fov.transform or a child “head”
    public float maxTurnDegPerSec = 360f; // how fast the cone can turn

    private Rigidbody2D rb;
    private Vector2 lastSeenPos;
    private float lostTimer = 0f;
    private float searchTimer = 0f;

    public Transform puzzlePosition; // assign the puzzle GameObject in Inspector
    private float puzzleTimer = 0f;
    public float maxPuzzleTime = 10f; // seconds

    public EnemyState state = EnemyState.Patrol;

    [Header("Ghost Phase Settings")]
    public float phaseDistance = 6f;       // start phasing if player this far away
    public float phaseSpeed = 8f;           // how fast ghost moves when phasing
    public float unphaseBuffer = 1.5f;      // distance threshold to safely unphase

    private bool isPhasing = false;
    private bool isInsideObstacle = false;  // detected using trigger checks

    private float lastDebugDist = -1f;
    private bool lastPhaseState = false;
    private Collider2D col;

    [Header("Teleportation")]
    public float teleportDistance = 2f;
    public float teleportCooldown = 5f;
    public float teleportDelayAtChaseStart = 1f;

    private float teleportTimer = 0f;       // counts down between teleports
    private bool teleportFirstDelay = true; // wait 1s before first teleport

    [Header("Teleport Settings")]
    private bool isTeleporting = false;

    [Header("Teleportation")]
    private float teleportChaseTimer = 0f;

    [Header("Hallucination Settings")]
    public GameObject hallucinationPrefab;  // red ghost prefab
    public float hallucinationInterval = 10f;
    public float hallucinationSpeed = 12f;  // faster than normal
    private float chaseTimer = 0f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!patrol) patrol = GetComponent<EnemyPatrol>();
        if (!fov) fov = GetComponent<VisionCone>();

        if (patrol != null)
            patrol.SetExternalMoveCallback(MoveToTarget, patrolSpeed);

        // Prevent NavMeshAgent from auto-rotating in 2D
        if (agent)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        EnterPatrol();
    }

    void Start()
    {
        // Only enter patrol if agent is on NavMesh
        if (agent && agent.isOnNavMesh)
            EnterPatrol();
        else
            Debug.LogWarning($"{name} NavMeshAgent is not on NavMesh at start!");
    }


    void Update()
    {
        bool canSeePlayer = (fov && player && fov.Detect(player));
        if (canSeePlayer)
        {
            lastSeenPos = player.position;
            if (state != EnemyState.Chase) EnterChase();
        }
        else if (!canSeePlayer && isPhasing)
        {
            
            lostTimer -= Time.deltaTime;
            if (lostTimer > 0)
            {
                PhaseMoveTowardPlayer();
            }
            if (lostTimer <= 0f)
            {
                StopPhasing();
                EnterPatrol();
                return;
            }
            
        }

        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(canSeePlayer); break;
            case EnemyState.Search: SearchTick(); break;
            case EnemyState.GoToPuzzle: GoToPuzzleTick(); break;
        }

        UpdateVisionAim(canSeePlayer);
    }

    void OnDrawGizmos()
    {
        if (agent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(agent.destination, 0.2f);
        }

        if (puzzlePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(puzzlePosition.position, 0.2f);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Wall"))
            isInsideObstacle = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Wall"))
            isInsideObstacle = false;
    }

    void GoToPuzzleTick()
    {
        if (!puzzlePosition) return;

        agent.SetDestination(puzzlePosition.position);
        puzzleTimer += Time.deltaTime;

        // Only leave this state when actually close enough
        float distance = Vector3.Distance(transform.position, puzzlePosition.position);
        if (distance < 0.5f) // adjust threshold as needed
        {
            Debug.Log("[Enemy] Reached puzzle. Switching to Patrol.");
            EnterPatrol(); // resume normal FSM
        }
        else if (puzzleTimer >= maxPuzzleTime)
        {
            Debug.Log("[Enemy] Puzzle failsafe triggered. Returning to Patrol.");
            EnterPatrol();
        }
    }

    // ---- States ----
    public void EnterGoToPuzzle(Transform puzzle)
    {
        if (puzzlePosition == null)
        {
            Debug.LogWarning($"{name}: Puzzle position not assigned!");
            return;
        }
        puzzlePosition = puzzle;
        state = EnemyState.GoToPuzzle;
        if (patrol) patrol.enabled = false; // ❌ Disable patrol updates
        agent.SetDestination(puzzlePosition.position);
        agent.speed = chaseSpeed;
        puzzleTimer = 0f; // reset failsafe timer
        Debug.Log($"{name}: Moving to puzzle at {puzzlePosition.position}");
    }

    void EnterPatrol()
    {
        state = EnemyState.Patrol;
        if (patrol) patrol.enabled = true;
        if (agent && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
        }
    }


    void EnterChase()
    {
        state = EnemyState.Chase;
        if (patrol) patrol.enabled = false;
        lostTimer = loseSightToSearchTime;

        if (agent && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }

        // Start teleport delay timer
        teleportTimer = teleportDelayAtChaseStart; // start with 1s delay
        teleportFirstDelay = true;
    }

    void EnterSearch()
    {
        state = EnemyState.Search;
        if (patrol) patrol.enabled = false;
        searchTimer = searchDuration;

        if (agent && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
        }
    }

    void StartPhasing()
    {
        if (isPhasing) return;
        isPhasing = true;
        if (agent) agent.enabled = false;  // disable NavMeshAgent control
        rb.isKinematic = false;            // allow manual movement
        col.enabled = false;
        Debug.Log($"{name} started phasing!");
    }

    void StopPhasing()
    {
        if (!isPhasing) return;

        isPhasing = false;
        Debug.Log($"[Ghost Debug] >>> STOPPING PHASE | Position: {transform.position} | Distance to player: {Vector2.Distance(transform.position, player.position):F2}");

        // Re-enable collider
        if (col)
        {
            col.enabled = true;
            Debug.Log("[Ghost Debug] Collider re-enabled.");
        }

        // Re-enable NavMeshAgent
        if (agent)
        {
            agent.enabled = true;
            Debug.Log("[Ghost Debug] NavMeshAgent re-enabled.");

            if (agent.isOnNavMesh)
            {
                // Sync agent position with current ghost position to avoid jumps
                agent.Warp(transform.position);
                agent.isStopped = false;
                agent.speed = chaseSpeed;

                // Reset destination to player or last seen position
                Vector3 destination = player ? player.position : lastSeenPos;
                agent.SetDestination(destination);

                Debug.Log($"[Ghost Debug] Agent destination set to {(player ? "player" : "lastSeenPos")}: {destination}");
            }
            else
            {
                Debug.LogWarning("[Ghost Debug] NavMeshAgent is not on NavMesh! Cannot set destination.");
            }
        }

        rb.velocity = Vector2.zero;
        Debug.Log("[Ghost Debug] Ghost fully unphased and ready to chase.");
    }


    void PhaseMoveTowardPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * phaseSpeed * Time.deltaTime);
    }


    void ChaseTick(bool canSeePlayer)
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // --- Debug every 1 meter change (to avoid spam)
        if (Mathf.Abs(dist - lastDebugDist) > 1f)
        {
            Debug.Log($"[Ghost Debug] Distance to player: {dist:F1} | Phasing: {isPhasing} | CanSeePlayer: {canSeePlayer}");
            lastDebugDist = dist;
        }


        // === Handle Phase Mode ===
        if (!isPhasing && dist > phaseDistance)
        {
            // Begin phasing
            StartPhasing();
            Debug.Log($"[Ghost Debug] >>> Started phasing (distance {dist:F1} > {phaseDistance})");
        }
        else if (isPhasing)
        {
            // Check if we can stop phasing
            if (!isInsideObstacle && (dist < phaseDistance - unphaseBuffer /*If player is too close when chasing*/ || (!canSeePlayer && lostTimer <= 0) /*If cannot see player for a certain amount of time */)) 
            {
                StopPhasing();
                Debug.Log($"[Ghost Debug] <<< Stopped phasing (distance {dist:F1}) | Inside obstacle: {isInsideObstacle} | CanSee: {canSeePlayer}");
            }
        }

        if (isPhasing)
        {
            PhaseMoveTowardPlayer();
        }
        else
        {
            // Normal NavMesh chase
            Vector2 targetPos = player.position;

            if (canSeePlayer)
            {
                lastSeenPos = targetPos;
                lostTimer = loseSightToSearchTime;
                chaseTimer += Time.deltaTime;
                if (chaseTimer >= hallucinationInterval)
                {
                    //SpawnHallucination();
                    chaseTimer = 0f; // reset
                }
            }
            else
            {
                lostTimer -= Time.deltaTime;
                if (lostTimer <= 0f)
                {
                    EnterSearch();
                    return;
                }
            }

            MoveToTarget(targetPos, chaseSpeed);
        }
        if (isPhasing != lastPhaseState)
        {
            Debug.Log($"[Ghost Debug] Phase state changed: {(isPhasing ? "PHASING" : "NORMAL")} | Dist: {dist:F1}");
            lastPhaseState = isPhasing;
        }

        
        // Countdown timer
        teleportTimer -= Time.deltaTime;

        if (teleportTimer <= 0f)
        {
            // Teleport
            TeleportToPlayer();

            // After first teleport, always use cooldown
            teleportTimer = teleportCooldown;
            teleportFirstDelay = false;
        }

    }

    //void SpawnHallucination()
    //{
    //    if (!hallucinationPrefab || !player) return;

    //    // Spawn near player or ghost
    //    Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle.normalized * 1.5f;

    //    GameObject hallucination = Instantiate(hallucinationPrefab, spawnPos, Quaternion.identity);
    //    hallucination.transform.position = new Vector3(spawnPos.x, spawnPos.y, -1f); // in front of player
    //    var sr = hallucination.GetComponentInChildren<SpriteRenderer>();
    //    if (sr != null)
    //    {
    //        sr.enabled = true;
    //        sr.color = new Color(1, 0, 0, 1); // fully opaque red
    //        Debug.Log($"Hallucination sprite enabled at position {hallucination.transform.position}");
    //    }
    //    else
    //    {
    //        Debug.LogWarning("No SpriteRenderer found in hallucination prefab or children!");
    //    }

    //    // Assign target and speed
    //    HallucinationBrain hb = hallucination.GetComponent<HallucinationBrain>();
    //    if (hb)
    //    {
    //        hb.player = player;
    //        hb.speed = hallucinationSpeed;
    //    }

    //    Debug.Log($"[Ghost] Spawned hallucination at {spawnPos}");

    //    if (hallucinationPrefab == null)
    //    {
    //        Debug.LogWarning("Hallucination prefab not assigned!");
    //    }
    //}

    void TeleportToPlayer()
    {
        if (!player) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized * teleportDistance;
        Vector2 targetPos = (Vector2)player.position + randomDir;

        NavMeshHit hit;
        bool onNavMesh = NavMesh.SamplePosition(targetPos, out hit, 0.5f, NavMesh.AllAreas);

        if (onNavMesh)
        {
            if (agent) agent.Warp(hit.position);
            else transform.position = hit.position;

            Debug.Log($"[Ghost] Teleported to {hit.position}");
        }
        else
        {
            Debug.Log($"[Ghost] Teleport failed, starting phase toward player from {transform.position}");
            StartPhasing();
        }
    }



    void PatrolTick()
    {
        // Patrol sets agent destination via MoveToTarget callback
        if (patrol && patrol.CurrentTarget.HasValue)
        {
            MoveToTarget(patrol.CurrentTarget.Value, patrolSpeed);
        }
    }

    void SearchTick()
    {
        MoveToTarget(lastSeenPos, patrolSpeed); // <-- use NavMeshAgent2D
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f) EnterPatrol();
    }

    // Callback used by Patrol script and FSM for movement
    void MoveToTarget(Vector2 target, float speed)
    {
        if (agent)
        {
            if (isPhasing || !agent || !agent.enabled) return;
            agent.SetDestination(target);
            agent.speed = speed;
        }
    }

    void UpdateVisionAim(bool seeingPlayer)
    {
        Vector2 aimDir;

        // 1) If chasing: aim at player while visible; otherwise aim at last seen
        if (state == EnemyState.Chase && (seeingPlayer || lostTimer > 0f))
        {
            Vector2 lookPos = seeingPlayer && player
                ? (Vector2)player.position
                : lastSeenPos;

            aimDir = (lookPos - (Vector2)visionPivot.position);
            if (aimDir.sqrMagnitude < 1e-8f) aimDir = visionPivot.right; // degenerate safety
            else aimDir.Normalize();
        }
        else
        {
            if (agent.velocity.sqrMagnitude > 1e-4f)
            {
                aimDir = agent.velocity.normalized;
            }
            else
            {
                // Use patrol target if available
                if (patrol != null && patrol.CurrentTarget.HasValue)
                {
                    Vector2 toNext = patrol.CurrentTarget.Value - (Vector2)visionPivot.position;
                    aimDir = toNext.sqrMagnitude > 1e-8f ? toNext.normalized : (Vector2)lastSeenPos - (Vector2)visionPivot.position;
                }
                else
                {
                    // fallback to last seen position if not moving
                    Vector2 toTarget = lastSeenPos - (Vector2)visionPivot.position;
                    aimDir = toTarget.sqrMagnitude > 1e-6f ? toTarget.normalized : visionPivot.right;
                }
            }
        }



        float targetAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        float current = visionPivot.eulerAngles.z;
        float maxStep = maxTurnDegPerSec * Time.deltaTime;
        float next = Mathf.MoveTowardsAngle(current, targetAngle, maxStep);
        visionPivot.rotation = Quaternion.Euler(0, 0, next);
    }
}