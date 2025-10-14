using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // for Light2D (URP 2D)

[RequireComponent(typeof(PlayerSpeedBoost))]
public class PlayerSpeedBoostFX : MonoBehaviour
{
    [Header("Light (optional)")]
    public Light2D visionLight;          // drag your child Light2D here (can be null)
    public float normalOuterRadius = 8f;
    public float boostedOuterRadius = 11f;
    public float lightLerp = 10f;

    [Header("Tint (optional)")]
    public SpriteRenderer sprite;        // auto-fills from same GO if null
    public Color boostedTint = Color.white;
    [Range(1f, 2f)] public float tintMultiplier = 1.15f;

    [Header("Trail (optional)")]
    public TrailRenderer trail;          // drag a TrailRenderer here (can be null)

    [Header("Burst FX (optional)")]
    public ParticleSystem burst;         // small particle burst on boost start
    public AudioSource sfx;              // AudioSource on player
    public AudioClip boostClip;          // one-shot played on boost start

    PlayerSpeedBoost boost;
    Color baseColor = Color.white;
    bool wasBoosting;

    void Awake()
    {
        boost = GetComponent<PlayerSpeedBoost>();
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;

        // start trail off (if assigned)
        if (trail != null) trail.emitting = false;

        // if light assigned but radius is 0, initialize to normal
        if (visionLight != null && visionLight.pointLightOuterRadius <= 0f)
            visionLight.pointLightOuterRadius = normalOuterRadius;
    }

    void Update()
    {
        bool on = boost != null && boost.IsBoosting;

        // edge: when boost turns on/off
        if (on != wasBoosting)
        {
            if (on)
            {
                if (trail != null) trail.emitting = true;
                if (burst != null) burst.Play();
                if (sfx != null && boostClip != null) sfx.PlayOneShot(boostClip);
            }
            else
            {
                if (trail != null) trail.emitting = false;
            }
            wasBoosting = on;
        }

        // tint
        if (sprite != null)
            sprite.color = on ? boostedTint * tintMultiplier : baseColor;

        // light radius lerp
        if (visionLight != null)
        {
            float target = on ? boostedOuterRadius : normalOuterRadius;
            visionLight.pointLightOuterRadius = Mathf.Lerp(
                visionLight.pointLightOuterRadius, target, Time.deltaTime * lightLerp);
        }
    }
}