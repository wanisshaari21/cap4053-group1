using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PlayerSpeedBoost boost;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boost = GetComponent<PlayerSpeedBoost>();

        // Top-down convenience (safe to keep; remove if you want gravity)
        if (rb != null) rb.gravityScale = 0f;
    }

    void Update()
    {
        // Block input while puzzle UI is active
        if (PuzzleTrigger.isPuzzleActive)
        {
            movement = Vector2.zero;
            return;
        }

        // WASD / Arrow keys
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized; // keep diagonal speed consistent
    }

    void FixedUpdate()
    {
        // Apply movement (boost-aware)
        float mul = (boost != null) ? boost.GetSpeedMultiplier() : 1f;
        float currentSpeed = moveSpeed * mul;

        // Use MovePosition for smooth kinematic-style motion with collisions
        Vector2 targetPos = rb.position + movement * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }
}
