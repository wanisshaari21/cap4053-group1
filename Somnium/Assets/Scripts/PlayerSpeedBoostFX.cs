using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // for Light2D (URP 2D)

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpeedBoostFX : MonoBehaviour
{
    [Header("Light (optional)")]
    public Light2D visionLight;          // drag your child Light2D here (can be null)
    public float normalLightRadius = 8f;
    public float boostedLightRadius = 10f;
    public float lightLerp = 8f;

    [Header("Tint")]
    public Color boostedTint = new Color(1f, 1f, 1f, 1f); // slightly brighter/whiter
    public float tintMultiplier = 1.2f;                   // 1.0 = no change

    SpriteRenderer sr;
    PlayerSpeedBoost boost;
    Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boost = GetComponent<PlayerSpeedBoost>();
        baseColor = sr.color;
    }

    void Update()
    {
        bool on = boost != null && boost.IsBoosting;

        // Simple visual tint
        sr.color = on ? boostedTint * tintMultiplier : baseColor;

        // Optional: widen/narrow the Light2D radius
        if (visionLight != null)
        {
            float target = on ? boostedLightRadius : normalLightRadius;
            visionLight.pointLightOuterRadius = Mathf.Lerp(
                visionLight.pointLightOuterRadius, target, Time.deltaTime * lightLerp);
        }
    }
}