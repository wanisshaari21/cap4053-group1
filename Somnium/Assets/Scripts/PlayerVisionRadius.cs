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
        if (visionLight == null) return;
        bool on = boost != null && boost.IsBoosting;
        float target = on ? boostedOuterRadius : normalOuterRadius;
        visionLight.pointLightOuterRadius = Mathf.Lerp(
            visionLight.pointLightOuterRadius, target, Time.deltaTime * lerpSpeed);
    }
}