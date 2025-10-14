using UnityEngine;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    public Transform path;
    public bool loop = true;
    public bool backnforth = false;
    public float wait = 0f;
    public float speed = 3f;          
    public float arriveDistance = 0.05f;

    public VisionCone fov;                  
    public Transform player;               

    private readonly List<Vector2> points = new();
    private int index = 0;
    private int direction = 1;
    private float waitTimer = 0f;

    private System.Action<Vector2, float> moveRequest;
    private float reportedSpeed = 3f;

    void Awake() => BuildPath();
    void OnValidate() { if (path) BuildPath(); }

    public Vector2? CurrentTarget { get; private set; }

    public void SetExternalMoveCallback(System.Action<Vector2, float> req, float patrolSpeed)
    {
        moveRequest = req;
        reportedSpeed = patrolSpeed;
    }

    void BuildPath()
    {
        points.Clear();
        if (!path) return;
        foreach (Transform child in path) points.Add(child.position);
    }

    void Update()
    {
        if (!enabled || points.Count == 0) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector2 target = points[Mathf.Clamp(index, 0, points.Count - 1)];
        CurrentTarget = target;
        moveRequest?.Invoke(target, reportedSpeed);

        bool reached = ((Vector2)transform.position - target).sqrMagnitude <= arriveDistance * arriveDistance;
        if (!reached) return;

        if (wait > 0f) waitTimer = wait;

        if (backnforth)
        {
            if (index == points.Count - 1) direction = -1;
            else if (index == 0) direction = 1;
            index += direction;
        }
        else if (loop)
        {
            index = (index + 1) % points.Count;
        }
        else
        {
            index = Mathf.Min(index + 1, points.Count - 1);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!path) return;
        Gizmos.color = Color.cyan;
        Transform prev = null;
        foreach (Transform t in path)
        {
            Gizmos.DrawSphere(t.position, 0.05f);
            if (prev) Gizmos.DrawLine(prev.position, t.position);
            prev = t;
        }
        if (loop && path.childCount > 1)
            Gizmos.DrawLine(path.GetChild(0).position, path.GetChild(path.childCount - 1).position);
    }
}
