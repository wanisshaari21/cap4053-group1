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

    // --- 1. ADD A REFERENCE TO THE ANIMATOR ---
    private Animator animator;

    // --- 2. ADD A VARIABLE TO REMEMBER THE LAST DIRECTION ---
    private Vector2 lastMoveDirection; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boost = GetComponent<PlayerSpeedBoost>();

        // --- 3. GET THE ANIMATOR FROM THE *CHILD* OBJECT ---
        // (This is why we made the "Visuals" child)
        animator = GetComponentInChildren<Animator>(); 

        if (rb != null) rb.gravityScale = 0f;
    }

    void Update()
    {
        // Block input while puzzle UI is active
        if (PuzzleTrigger.isPuzzleActive)
        {
            movement = Vector2.zero;
            // --- 4. TELL THE ANIMATOR WE STOPPED ---
            animator.SetBool("isMoving", false);
            return;
        }
        else if (ArrowPuzzleTrigger1.isPuzzleActive)
        {
            movement = Vector2.zero;
            // --- 5. TELL THE ANIMATOR WE STOPPED ---
            animator.SetBool("isMoving", false);
            return;
        }

        // WASD / Arrow keys
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized; // keep diagonal speed consistent

        // --- 6. THIS IS THE NEW ANIMATION LOGIC ---
        if (movement != Vector2.zero)
        {
            // We are moving
            animator.SetBool("isMoving", true);
            
            // Send the direction to the blend trees
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            
            // Remember this as our last direction
            lastMoveDirection = movement;
        }
        else
        {
            // We are not moving
            animator.SetBool("isMoving", false);
            
            // Set the idle direction to our last known movement
            // This makes sure we are "Idle_Left" if we stopped moving left, etc.
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
        }
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