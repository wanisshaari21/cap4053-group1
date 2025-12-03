using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer; 
    private Vector3 lastPosition;

    void Awake()
    {
        // LOOK IN CHILDREN: The script is on Parent, components are on Visuals
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (animator == null) return;

        // 1. Calculate actual movement (Works for NavMesh AND Grapple!)
        Vector3 movement = transform.position - lastPosition;
        
        // 2. Check if moving (threshold prevents micro-jitter)
        bool isMoving = movement.magnitude > 0.001f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            Vector3 direction = movement.normalized;

            // 3. Feed the Blend Tree
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);

            // 4. Handle Flipping
            // Since your Side sprites face Right:
            // Moving Left (negative X) -> Flip True
            // Moving Right (positive X) -> Flip False
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        // 5. Update for next frame
        lastPosition = transform.position;
    }
}