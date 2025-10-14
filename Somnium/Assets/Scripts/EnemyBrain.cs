using UnityEngine;

public enum EnemyState { Patrol, Chase, Search }

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBrain : MonoBehaviour
{
    public EnemyPatrol patrol;
    public VisionCone fov;
    public Transform player;

    [Header("Speeds")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6.0f;

    [Header("State Timers")]
    public float loseSightToSearchTime = 3f;
    public float searchDuration = 8f;

    [Header("Collision")]
    public LayerMask obstacleMask;

    [Header("Vision Tracking")]
    public Transform visionPivot;            // assign fov.transform or a child “head”
    public float maxTurnDegPerSec = 360f;    // how fast the cone can turn

    private Rigidbody2D rb;
    private Vector2 lastSeenPos;
    private float lostTimer = 0f;
    private float searchTimer = 0f;
    private Vector2 desiredVelocity;

    public EnemyState state = EnemyState.Patrol;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!patrol) patrol = GetComponent<EnemyPatrol>();
        if (!fov) fov = GetComponent<VisionCone>();
        if (!visionPivot) visionPivot = fov ? fov.transform : transform;

        EnterPatrol();
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

    void FixedUpdate()
    {
        if (desiredVelocity.sqrMagnitude > 1e-6f)
            TryMove(desiredVelocity);
        else
            rb.velocity = Vector2.zero;

        desiredVelocity = Vector2.zero;
    }

    // ---- States ----
    void EnterPatrol()
    {
        state = EnemyState.Patrol;
        if (patrol)
        {
            patrol.enabled = true;
            patrol.SetExternalMoveCallback(RequestMoveTowards, patrolSpeed); // route movement via physics
        }
        rb.velocity = Vector2.zero;
    }

    void EnterChase()
    {
        state = EnemyState.Chase;
        if (patrol) patrol.enabled = false;
        lostTimer = loseSightToSearchTime;
    }

    void EnterSearch()
    {
        state = EnemyState.Search;
        searchTimer = searchDuration;
        rb.velocity = Vector2.zero;
    }

    void PatrolTick()
    {
        // Patrol script calls RequestMoveTowards each frame via the callback.
    }

    void ChaseTick(bool canSeePlayer)
    {
        if (!player) return;

        // move directly toward player (no prediction/boost logic)
        RequestMoveTowards(player.position, chaseSpeed);

        if (canSeePlayer)
        {
            lostTimer = loseSightToSearchTime;
            lastSeenPos = player.position;
        }
        else
        {
            // move to last seen while timer runs
            RequestMoveTowards(lastSeenPos, chaseSpeed);
            lostTimer -= Time.deltaTime;
            if (lostTimer <= 0f) EnterSearch();
        }
    }

    void SearchTick()
    {
        RequestMoveTowards(lastSeenPos, patrolSpeed);
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f) EnterPatrol();
    }

    // ---- Helpers ----
    void RequestMoveTowards(Vector2 target, float speed)
    {
        Vector2 dir = target - rb.position;
        if (dir.sqrMagnitude > 1e-6f)
            desiredVelocity = dir.normalized * speed;

        if (Mathf.Abs(dir.x) > 0.01f)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(dir.x) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y, transform.localScale.z);
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
            if (rb.velocity.sqrMagnitude > 1e-4f)
            {
                aimDir = rb.velocity.normalized;
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


    void TryMove(Vector2 velocity)
    {
        float dt = Time.fixedDeltaTime;
        Vector2 remaining = velocity * dt;
        int iterations = 2;
        float skin = 0.01f;

        for (int i = 0; i < iterations; i++)
        {
            if (remaining.sqrMagnitude < 1e-10f) break;

            Vector2 dir = remaining.normalized;
            float len = remaining.magnitude;

            var hits = new RaycastHit2D[6];
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(obstacleMask);

            int count = rb.Cast(dir, filter, hits, len + skin);
            if (count == 0)
            {
                rb.MovePosition(rb.position + remaining);
                remaining = Vector2.zero;
                break;
            }

            // closest hit  (BUGFIX: use j, not i)
            float minDist = Mathf.Infinity;
            int minIdx = -1;
            for (int j = 0; j < count; j++)
            {
                if (hits[j].distance < minDist)
                {
                    minDist = hits[j].distance;
                    minIdx = j;
                }
            }

            float allowed = Mathf.Max(0f, minDist - skin);
            if (allowed > 0f) rb.MovePosition(rb.position + dir * allowed);

            if (minIdx >= 0)
            {
                Vector2 n = hits[minIdx].normal;
                remaining = dir * (len - allowed);
                remaining -= Vector2.Dot(remaining, n) * n; // slide along tangent
            }
            else
            {
                remaining = Vector2.zero;
                break;
            }
        }

        rb.velocity = velocity; // good for anim/knockback blends
    }
}
