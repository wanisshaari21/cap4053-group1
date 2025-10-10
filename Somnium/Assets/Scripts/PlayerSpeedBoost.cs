using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpeedBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostMultiplier = 1.6f; // how much faster
    [SerializeField] private float boostDuration = 1.25f;  // seconds active
    [SerializeField] private float cooldown = 2.0f;        // seconds before next boost

    private float boostUntil = -999f;
    private float nextAllowed = 0f;

    /// <summary>True while boost is active.</summary>
    public bool IsBoosting => Time.time < boostUntil;

    /// <summary>Multiply your move speed/velocity by this value.</summary>
    public float GetSpeedMultiplier() => IsBoosting ? boostMultiplier : 1f;

    /// <summary>Try to start a boost; returns true if started.</summary>
    public bool TryTriggerBoost()
    {
        if (Time.time < nextAllowed) return false;
        boostUntil = Time.time + boostDuration;
        nextAllowed = Time.time + Mathf.Max(boostDuration, cooldown);
        return true;
    }
    
    // Keeps the boost active by ensuring at least this much time remains.
    // Also clears cooldown while sustained (so no cast delay).
    public void SustainBoost(float minRemainingSeconds = 0.25f)
    {
        if (minRemainingSeconds <= 0f) minRemainingSeconds = 0.1f;

        // If boost would end sooner than desired, extend it.
        float targetEnd = Time.time + minRemainingSeconds;
        if (boostUntil < targetEnd)
            boostUntil = targetEnd;

        // While a zone is sustaining us, don't enforce cooldown.
        nextAllowed = Time.time;
    }

}
