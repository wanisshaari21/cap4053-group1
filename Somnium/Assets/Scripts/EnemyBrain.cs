using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Patrol, Chase, Search }

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBrain : MonoBehaviour
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
            case EnemyState.Chase: ChaseTick(canSeePlayer); break;
            case EnemyState.Search: SearchTick(); break;
        }

        UpdateVisionAim(canSeePlayer);
    }

    // ---- States ----
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
        searchTimer = searchDuration;

        if (agent && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
        }
    }

    void ChaseTick(bool canSeePlayer)
    {
        if (!player) return;

        Vector2 targetPos = lastSeenPos;
        if (canSeePlayer)
        {
            targetPos = player.position;
            lastSeenPos = targetPos;
            lostTimer = loseSightToSearchTime;
        }
        else
        {
            lostTimer -= Time.deltaTime;
            if (lostTimer <= 0f) EnterSearch();
        }

        MoveToTarget(targetPos, chaseSpeed);
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
            // 2) Not chasing (Patrol/Search): aim in movement direction; if nearly idle, aim toward patrol waypoint
            if (agent.velocity.sqrMagnitude > 1e-4f)
            {
                aimDir = agent.velocity.normalized;
            }
            else
            {
                // If we're waiting at a point, face where we intend to go next (patrol target)
                if (patrol != null && patrol.CurrentTarget.HasValue)
                {
                    Vector2 toNext = patrol.CurrentTarget.Value - (Vector2)visionPivot.position;
                    aimDir = toNext.sqrMagnitude > 1e-8f ? toNext.normalized : (Vector2)visionPivot.right;
                }
                else
                {
                    aimDir = (Vector2)visionPivot.right; // default
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