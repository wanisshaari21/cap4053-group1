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

    private Rigidbody2D rb;
    private List<Vector2> points = new();
    private int index = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isPatrolling = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        BuildPath();

    }

    void OnValidate()
    {
        if (path != null)
            BuildPath();
    }

    void BuildPath()
    {
        points.Clear();
        if (path == null) return;

        foreach (Transform child in path)
        {
            points.Add(child.position);
        }
    }

    void Update()
    {
        if (fov && player && fov.Detect(player))
        {
            Debug.Log($"{name} sees the player!");
        }
        else
        {
            isPatrolling = true;
        }

        if (!isPatrolling)
            return;

        Patrol();
    }

    void Patrol()
    {
        if (points.Count == 0)
            return;
        if (points.Count == 1)
        {
            MoveTo(points[0]);
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector2 target = points[index];
        bool reached = MoveTo(target);

        if (reached)
        {
            if (wait > 0f)
                waitTimer = wait;

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
    }

    bool MoveTo(Vector2 target)
    {
        Vector2 pos = rb.position;
        Vector2 delta = target - pos;

        if (delta.sqrMagnitude <= arriveDistance * arriveDistance)
            return true;

        Vector2 step = delta.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        if (step.x > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (step.x < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        float angle = Mathf.Atan2(step.y, step.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        return false;
    }


    void OnDrawGizmosSelected()
    {
        if (path == null) return;

        Gizmos.color = Color.cyan;
        Transform prev = null;

        foreach (Transform t in path)
        {
            Gizmos.DrawSphere(t.position, 0.05f);
            if (prev != null)
                Gizmos.DrawLine(prev.position, t.position);
            prev = t;
        }

        if (loop && path.childCount > 1)
        {
            Gizmos.DrawLine(path.GetChild(0).position,
                            path.GetChild(path.childCount - 1).position);
        }
    }
}
