using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerSpeedBoost))]
public class PlayerVisionRadius : MonoBehaviour
{
    [Header("Assign your child Light2D (Point)")]
    public Light2D visionLight;

    [Header("Radii")]
    public float normalOuterRadius = 8f;
    public float boostedOuterRadius = 11f;
    public float lerpSpeed = 10f;

    // --- 1. ADD THIS NEW VARIABLE ---
    [Header("Flash Effect")]
    [SerializeField] float flashSpeed = 20f; // How fast the light will pulse

    PlayerSpeedBoost boost;

    void Awake()
    {
        boost = GetComponent<PlayerSpeedBoost>();
        if (visionLight == null)
            visionLight = GetComponentInChildren<Light2D>();
        if (visionLight != null && visionLight.pointLightOuterRadius <= 0f)
            visionLight.pointLightOuterRadius = normalOuterRadius;
    }

    void Update()
    {
        if (visionLight == null || boost == null) return;

        float target;

        // Check boost status
        if (boost.IsBoosting)
        {
            // --- 2. THIS IS THE NEW LOGIC ---
            if (boost.IsBoostEndingSoon)
            {
                // Create a flashing effect
                // A sine wave between 0 and 1
                float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f; 
                // Lerp between normal and boosted radius
                target = Mathf.Lerp(normalOuterRadius, boostedOuterRadius, alpha);
            }
            else
            {
                // Just stay at the max boosted radius
                target = boostedOuterRadius;
            }
        }
        else
        {
            // Not boosting, go back to normal
            target = normalOuterRadius;
        }

        // Apply the new target
        visionLight.pointLightOuterRadius = Mathf.Lerp(
            visionLight.pointLightOuterRadius, target, Time.deltaTime * lerpSpeed);
    }
}