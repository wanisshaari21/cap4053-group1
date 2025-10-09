using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Transform visionOrigin;
    public float visionRadius = 6f;
    public float visionAngle = 60f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;


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

        Vector2 forward = transform.right; // right is forward in top-down
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

    void OnDrawGizmosSelected()
    {
        if (!visionOrigin) visionOrigin = transform;
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        Vector3 origin = visionOrigin.position;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2) * transform.right * visionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2) * transform.right * visionRadius;

        Gizmos.DrawLine(origin, origin + leftBoundary);
        Gizmos.DrawLine(origin, origin + rightBoundary);
    }

}
