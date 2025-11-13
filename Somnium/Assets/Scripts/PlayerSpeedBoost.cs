using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerSpeedBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] float boostMultiplier = 1.6f;
    [SerializeField] float boostDuration = 1.25f;
    [SerializeField] float cooldown = 2.0f;

    // --- 1. ADD THIS NEW LINE ---
    [SerializeField] float boostWarningTime = 0.5f; // How many seconds before ending to start warning

    float boostUntil = -999f;
    float nextAllowed = 0f;

    public bool IsBoosting => Time.time < boostUntil;

    // --- 2. ADD THIS NEW PROPERTY ---
    /// <summary>
    /// True if the boost is active AND in its final warning period.
    /// </summary>
    public bool IsBoostEndingSoon => IsBoosting && (boostUntil - Time.time < boostWarningTime);
    
    public float GetSpeedMultiplier() => IsBoosting ? boostMultiplier : 1f;

    public bool TryTriggerBoost()
    {
        if (Time.time < nextAllowed) return false;
        boostUntil  = Time.time + boostDuration;
        nextAllowed = Time.time + Mathf.Max(boostDuration, cooldown);
        return true;
    }

    // Keep-alive while inside a zone
    public void SustainBoost(float minRemainingSeconds = 0.25f)
    {
        if (minRemainingSeconds <= 0f) minRemainingSeconds = 0.1f;
        float targetEnd = Time.time + minRemainingSeconds;
        if (boostUntil < targetEnd) boostUntil = targetEnd;
        nextAllowed = Time.time; // no “cast delay” while sustained
    }
}