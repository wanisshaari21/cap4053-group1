using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProximityBoostSustainZone : MonoBehaviour
{
    [SerializeField] string triggerTag = "Player";
    [SerializeField] float minRemainingSeconds = 0.30f; // how much time to keep on the clock

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(triggerTag)) return;
        var boost = other.GetComponent<PlayerSpeedBoost>();
        if (boost != null) boost.SustainBoost(minRemainingSeconds);
        // result: boost starts immediately on entry and never drops while inside
    }
}