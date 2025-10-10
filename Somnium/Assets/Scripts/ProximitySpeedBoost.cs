using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProximityBoostZone : MonoBehaviour
{
    [Header("Who can trigger")]
    [SerializeField] string triggerTag = "Player";

    [Header("Stay-near requirements")]
    [SerializeField] float requiredStaySeconds = 1.0f;  // time inside to earn boost
    [SerializeField] float minMoveSpeed = 0.2f;         // must be moving a bit (units/sec)
    [SerializeField] bool fireOncePerEnter = true;      // only once until they leave & re-enter

    float enterTime = -1f;
    Vector3 lastPos;
    bool firedThisEnter;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(triggerTag)) return;
        enterTime = Time.time;
        lastPos = other.transform.position;
        firedThisEnter = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(triggerTag)) return;

        // movement speed (units/sec)
        float speed = (other.transform.position - lastPos).magnitude / Time.deltaTime;
        lastPos = other.transform.position;

        if (fireOncePerEnter && firedThisEnter) return;
        if (enterTime < 0f) return;

        bool longEnough = (Time.time - enterTime) >= requiredStaySeconds;
        bool movingEnough = speed >= minMoveSpeed;

        if (longEnough && movingEnough)
        {
            var boost = other.GetComponent<PlayerSpeedBoost>();
            if (boost != null && boost.TryTriggerBoost())
            {
                firedThisEnter = true;
                if (!fireOncePerEnter) enterTime = Time.time; // allow repeats
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(triggerTag)) return;
        enterTime = -1f;
        firedThisEnter = false;
    }
}