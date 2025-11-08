using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
public class SpiderBrain : MonoBehaviour
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

    [Header("Spider Grapple Settings")]
    public LayerMask wallMask;
    public float grappleTriggerDistance = 10f;   // how far player must be to trigger grapple
    public float grappleSearchRadius = 5f;       // how far around player to look for a wall
    public float grappleSpeed = 20f;             // movement speed during grapple
    public float grappleCooldown = 5f;           // time before next grapple
    private bool isGrappling = false;
    private float grappleCooldownTimer = 0f;

    public LayerMask obstacleMask; // for line-of-sight blocking

    public EnemyState state = EnemyState.Patrol;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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

        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase:
                if (grappleCooldownTimer > 0f)
                    grappleCooldownTimer -= Time.deltaTime;
                ChaseTick(canSeePlayer);
                break;
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

    void TryWebGrapple()
    {
        if (!player)
        {
            Debug.LogWarning($"{name}: No player assigned — cannot grapple!");
            return;
        }

        Debug.Log($"[Grapple] Attempting grapple. WallMask value: {wallMask.value}, SearchRadius: {grappleSearchRadius}");

        // find a wall near the player within radius
        Collider2D[] walls = Physics2D.OverlapCircleAll(player.position, grappleSearchRadius, wallMask);
        Debug.Log($"[Grapple] Found {walls.Length} possible walls near player at {player.position}");

        if (walls.Length == 0)
        {
            Debug.LogWarning("[Grapple] No walls detected near player. Check wallMask layer settings!");
            return;
        }

        Transform bestWall = null;
        float bestDot = -1f;
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        foreach (var w in walls)
        {
            if (w == null) continue;

            Vector2 wallPoint = w.ClosestPoint(player.position);
            Vector2 toWall = (wallPoint - (Vector2)transform.position).normalized;
            float dot = Vector2.Dot(dirToPlayer, toWall);
            bool blocked = Physics2D.Linecast(transform.position, wallPoint, obstacleMask);

            Debug.Log($"[Grapple] Checking wall '{w.name}' at {wallPoint}: dot={dot:F2}, blocked={blocked}");
            Debug.DrawLine(transform.position, wallPoint, Color.cyan, 1.0f);

            if (dot > bestDot && !blocked)
            {
                bestDot = dot;
                bestWall = w.transform;
            }
        }

        if (bestWall != null)
        {
            Vector2 bestPoint = bestWall.GetComponent<Collider2D>().ClosestPoint(player.position);
            Debug.Log($"[Grapple] Selected best wall '{bestWall.name}' at {bestPoint}");
            StartCoroutine(WebGrappleRoutine(bestPoint));
        }

        else
        {
            Debug.LogWarning("[Grapple] No valid wall with line of sight found!");
        }
    }


    System.Collections.IEnumerator WebGrappleRoutine(Vector2 wallPoint)
    {
        Debug.Log($"{name} launching grapple toward {wallPoint}");
        isGrappling = true;
        grappleCooldownTimer = grappleCooldown;

        if (agent)
        {
            Debug.Log("[Grapple] Disabling NavMeshAgent temporarily.");
            agent.enabled = false;
            agent.ResetPath(); // <-- important: clears target
        }

        Vector2 startPos = transform.position;
        float startDist = Vector2.Distance(startPos, wallPoint);
        Debug.Log($"[Grapple] Start distance to wall: {startDist:F2}");

        // simulate fast movement to wall
        while (Vector2.Distance(transform.position, wallPoint) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, wallPoint, grappleSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log($"{name} reached wall point ({wallPoint}), dropping back into chase.");

        yield return new WaitForSeconds(0.3f);

        if (agent)
        {
            Debug.Log("[Grapple] Re-enabling NavMeshAgent.");
            yield return null; // one frame delay to ensure transform has settled
            agent.enabled = true;
            if (agent.isOnNavMesh)
                Debug.Log("[Grapple] Agent successfully reattached to NavMesh.");
            else
                Debug.LogWarning("[Grapple] Agent NOT on NavMesh after grapple!");
        }

        isGrappling = false;
    }

    void OnDrawGizmosSelected()
    {
        if (player)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, grappleSearchRadius);
        }
    }

    void ChaseTick(bool canSeePlayer)
    {
        if (!player) return;

        Vector2 targetPos = player.position; // always chase the player’s current position
        float dist = Vector2.Distance(transform.position, targetPos);

        if (canSeePlayer)
        {
            lastSeenPos = targetPos;
            lostTimer = loseSightToSearchTime; // reset timer while player visible
            //Debug.Log($"[Chase] Player visible. Reset lostTimer to {lostTimer}");
        }
        else
        {
            lostTimer -= Time.deltaTime;
            //Debug.Log($"[Chase] Player not visible. Countdown: {lostTimer:F2} seconds");

            if (lostTimer <= 0f)
            {
                Debug.Log("[Chase] Lost timer expired. Entering Search state.");
                EnterSearch();
                return; // stop chasing after countdown ends
            }
        }

        // 🕸️ Try grapple if player is far and visible
        bool recentlySawPlayer = (lostTimer > 0f);

        if (!isGrappling && recentlySawPlayer && dist > grappleTriggerDistance && grappleCooldownTimer <= 0f)
        {
            TryWebGrapple();

        }

        // Only use NavMesh when not grappling
        if (!isGrappling)
        {
            MoveToTarget(targetPos, chaseSpeed);
        }
    }


    void PatrolTick()
    {
        // Patrol sets agent destination via MoveToTarget callback
        if (patrol && patrol.CurrentTarget.HasValue)
            MoveToTarget(patrol.CurrentTarget.Value, patrolSpeed);
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