using UnityEngine.AI;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Transform visionOrigin;
    public float visionRadius = 6f;
    public float visionAngle = 60f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Optional: for movement-based facing")]
    public Rigidbody2D rb;
    public NavMeshAgent agent;

    void Reset()
    {
        if (!visionOrigin)
            visionOrigin = transform;
    }

    public bool Detect(Transform target)
    {
        if (!target)
            return false;

        Vector2 origin = visionOrigin ? (Vector2)visionOrigin.position : (Vector2)transform.position;
        Vector2 toTarget = (Vector2)target.position - origin;

        if (toTarget.sqrMagnitude > visionRadius * visionRadius)
            return false;

        // --- CHANGED PART: use movement direction if available ---
        Vector2 forward = GetForward();
        if (Vector2.Angle(forward, toTarget) > visionAngle * 0.5f)
            return false;

        float dist = Mathf.Sqrt(toTarget.sqrMagnitude);
        if (Physics2D.Raycast(origin, toTarget.normalized, dist, obstacleMask))
            return false;

        int targetLayerBit = 1 << target.gameObject.layer;
        if ((targetLayerBit & playerMask) == 0)
            return false;

        return true;
    }

    Vector2 GetForward()
    {
        // 1) If using Rigidbody, use its velocity if moving
        if (rb && rb.velocity.sqrMagnitude > 0.01f)
            return rb.velocity.normalized;

        // 2) If using NavMeshAgent, use its velocity if moving
        if (agent && agent.velocity.sqrMagnitude > 0.01f)
            return agent.velocity.normalized;

        // 3) PHASE MODE: fallback to player direction if assigned
        GhostBrain ghost = GetComponent<GhostBrain>();
        if (ghost && ghost.player)
        {
            Vector2 dirToPlayer = ((Vector2)ghost.player.position - (Vector2)transform.position).normalized;
            if (dirToPlayer.sqrMagnitude > 0.001f)
                return dirToPlayer;
        }

        // 4) Spider jump/grapple case: use direction to player if assigned
        SpiderBrain spider = GetComponent<SpiderBrain>();
        if (spider && spider.player && spider.isGrappling)
        {
            Vector2 dirToPlayer = ((Vector2)spider.player.position - (Vector2)transform.position).normalized;
            if (dirToPlayer.sqrMagnitude > 0.001f)
                return dirToPlayer;
        }

        // 5) Last fallback: sprite right
        return transform.right;
    }

    void OnDrawGizmos()
    {
        if (!visionOrigin) visionOrigin = transform;
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        Vector3 origin = visionOrigin.position;
        Vector2 forward = GetForward();
        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2) * (Vector3)forward * visionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2) * (Vector3)forward * visionRadius;

        Gizmos.DrawLine(origin, origin + leftBoundary);
        Gizmos.DrawLine(origin, origin + rightBoundary);
    }
}
