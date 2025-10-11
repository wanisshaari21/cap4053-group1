using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LoopDetector : MonoBehaviour
{
    [Header("Loop Geometry")]
    [Tooltip("The center point the player must circle around.")]
    public Transform pivot;

    [Tooltip("Degrees of rotation required to count as a loop.")]
    [SerializeField] private float requiredDegrees = 240f;  // forgiving for testing

    [Tooltip("Reset progress if player barely moves for this long (set high for testing).")]
    [SerializeField] private float resetIfStationarySecs = 99f; // forgiving for testing

    [Tooltip("Set <= 0 to disable time cap during testing.")]
    [SerializeField] private float maxLoopWindowSecs = 0f; // forgiving for testing

    [Header("Target Filtering")]
    [Tooltip("Only objects with this tag can trigger loops (usually 'Player').")]
    [SerializeField] private string triggerTag = "Player";

    // runtime state
    private float accumulated;
    private float lastAngle;
    private float lastMoveTime;
    private float loopStartTime;
    private bool tracking;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // ensure trigger
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Loop] Enter: {other.name} (tag={other.tag})");

        if (!other.CompareTag(triggerTag) || pivot == null) return;

        Vector2 to = (Vector2)other.transform.position - (Vector2)pivot.position;
        lastAngle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg;

        accumulated = 0f;
        tracking = true;
        lastMoveTime = Time.time;
        loopStartTime = Time.time;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[Loop] Exit: {other.name}");

        if (!other.CompareTag(triggerTag)) return;

        tracking = false;
        accumulated = 0f;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // spam a little so we know Stay is firing
        if (Time.frameCount % 10 == 0) Debug.Log($"[Loop] Stay: {other.name}");

        if (!tracking || pivot == null || !other.CompareTag(triggerTag)) return;

        Vector2 to = (Vector2)other.transform.position - (Vector2)pivot.position;
        float angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(lastAngle, angle);

        // ignore tiny jitter
        if (Mathf.Abs(delta) > 0.05f)
        {
            accumulated += delta;
            lastAngle = angle;
            lastMoveTime = Time.time;

            if (Time.frameCount % 10 == 0)
                Debug.Log($"[Loop] Accum = {accumulated:0}°");
        }

        // optional resets (disabled/forgiving while testing)
        if (resetIfStationarySecs > 0f && Time.time - lastMoveTime > resetIfStationarySecs)
        {
            accumulated = 0f;
            loopStartTime = Time.time;
        }
        if (maxLoopWindowSecs > 0f && Time.time - loopStartTime > maxLoopWindowSecs)
        {
            accumulated = 0f;
            loopStartTime = Time.time;
        }

        // loop complete?
        if (Mathf.Abs(accumulated) >= requiredDegrees)
        {
            Debug.Log("[Loop] LOOP COMPLETE — triggering boost");
            var boost = other.GetComponent<PlayerSpeedBoost>();
            if (boost != null) boost.TryTriggerBoost();

            // reset so player can loop again
            accumulated = 0f;
            loopStartTime = Time.time;
        }
    }
}