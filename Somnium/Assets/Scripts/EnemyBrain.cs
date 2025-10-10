using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Possible enemy states
public enum EnemyState { Patrol, Chase, Search }

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBrain : MonoBehaviour {
    [Header("Refs")]
    public EnemyPatrol patrol; // The patrol script
    public VisionCone fov; // The vision cone script
    public Transform player; // Player transform

    // Movement Speeds:
    [Header("Speeds")]
    public float patrolSpeed = 3.5f; // Normal walking speed
    public float chaseSpeed = 6.0f; // Faster chase speed

    // Times controlling state transitions:
    [Header("Timers")]
    public float loseSightToSearchTime = 3f; // How long the enemy waits after losing sight before searching
    public float searchDuration = 8f; // How long enemy searches before returning to patrol

    // Debug:
    [Header("Debug")]
    public EnemyState state = EnemyState.Patrol;

    // Internal variables
    private Rigidbody2D rb;
    private Vector2 lastSeenPos; // Where player was last seen
    private float lostTimer = 0f; // Counts down after losing sight
    private float searchTimer = 0f; // Counts down while searching

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!patrol) patrol = GetComponent<EnemyPatrol>();
        if (!fov) fov = GetComponent<VisionCone>();

        EnterPatrol();
    }

    void Update()
    {
        // Check if enemy currently sees the player
        bool canSeePlayer = (fov && player && fov.Detect(player));

        // If enemy sees the player, switch to chase
        if (canSeePlayer)
        {
            lastSeenPos = player.position;
            if (state != EnemyState.Chase)
                EnterChase();
        }

        // Run logic depending on current state
        switch (state)
        {
            case EnemyState.Patrol: PatrolTick(); break;
            case EnemyState.Chase: ChaseTick(canSeePlayer); break;
            case EnemyState.Search: SearchTick(); break;
        }
    }

    // State Enter Functions:

    // Start patrolling
    void EnterPatrol()
    {
        state = EnemyState.Patrol;
        if (patrol)
        {
            patrol.enabled = true; // Re-enable patrol script
            patrol.speed = patrolSpeed; // Make sure it moves at patrol speed 

        }
    }

    // Start chasing
    void EnterChase()
    {
        state = EnemyState.Chase;
        if (patrol) patrol.enabled = false; // Disable patrol movements
        lostTimer = loseSightToSearchTime; // Set timer for losing sight
    }

    // Start searching 
    void EnterSearch()
    {
        state = EnemyState.Search;
        searchTimer = searchDuration;
        // Patrols stays disbabled until we return to patrol state  
    }

    // State Update Functions:

    // Called every frame while patrolling
    void PatrolTick()
    {
        // EnemyPatrol Script handles movement automatically
        // Nothing happens until enemmy sees player
    }

    // Called every frame while chasing 
    void ChaseTick(bool canSeePlayer)
    {
        if (player)
        {
            MoveTowards(player.position, chaseSpeed); // Move towards the player
        }
        if (canSeePlayer)
        {
            // Refresh timer if player still visible
            lostTimer = loseSightToSearchTime;
        }
        else
        {
            // Move to last seen position while timer counts down
            MoveTowards(lastSeenPos, chaseSpeed);
            lostTimer -= Time.deltaTime;
            if (lostTimer <= 0f)
            {
                EnterSearch(); // Switch to Search when timer runs out
            }
        }
    }

    // Called every frame while searching 
    void SearchTick()
    {
        MoveTowards(lastSeenPos, patrolSpeed); // Hover near last seen spot
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            EnterPatrol(); // Switch to Patrol when timer runs out
        }
    }

    // Helper Functions:

    // Handles Rigidbody2D movement toward a target position
    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 pos = rb.position;
        Vector2 dir = (target - pos).normalized;

        rb.MovePosition(pos + dir * speed * Time.fixedDeltaTime);

        // Flip sprite to face direction of motion
        if (Mathf.Abs(dir.x) > 0.01f)
        {
            transform.localScale = new Vector3(MathF.Sign(dir.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    
}
