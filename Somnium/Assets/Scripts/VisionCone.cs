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
        if (rb && rb.velocity.sqrMagnitude > 0.01f)
            return rb.velocity.normalized;

        if (agent && agent.velocity.sqrMagnitude > 0.01f)
            return agent.velocity.normalized;

        return transform.right; // fallback to sprite facing
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
