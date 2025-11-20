using UnityEngine;

[ExecuteAlways]
public class TimingWheelDebugGizmo : MonoBehaviour
{
    public Transform cursorPivot;
    public Transform hitZonePivot;
    public float hitZoneAngle = 60f;
    public float perfectZoneAngle = 15f;

    void OnDrawGizmos()
    {
        if (cursorPivot == null || hitZonePivot == null)
            return;

        // Draw cursor direction
        Gizmos.color = Color.green;
        Vector3 cursorDir = cursorPivot.right;
        Gizmos.DrawLine(cursorPivot.position, cursorPivot.position + cursorDir * 2f);

        // Hit zone center direction
        Gizmos.color = Color.yellow;
        Vector3 hitCenterDir = hitZonePivot.right;
        Gizmos.DrawLine(hitZonePivot.position, hitZonePivot.position + hitCenterDir * 2f);

        // Draw hit zone arc centered
        DrawArc(hitZonePivot.position, hitCenterDir, hitZoneAngle, Color.white, -hitZoneAngle / 2f);

        // Draw perfect zone arc centered
        float perfectOffset = -perfectZoneAngle / 2f;
        DrawArc(hitZonePivot.position, hitCenterDir, perfectZoneAngle, Color.magenta, perfectOffset);
    }

    void DrawArc(Vector3 pos, Vector3 dir, float angle, Color color, float startOffset = 0f)
    {
        Gizmos.color = color;
        int steps = 60;
        float radius = 1.5f;
        Vector3 prev = pos + (Quaternion.Euler(0, 0, startOffset) * dir) * radius;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            float a = startOffset + angle * t;
            Vector3 v = Quaternion.Euler(0, 0, a) * dir;
            Vector3 p = pos + v * radius;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }


}
