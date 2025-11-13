// RedStoneLoopManager.cs
using UnityEngine;

public class RedStoneLoopManager : MonoBehaviour
{
    // --- Internal State (our "checklist") ---
    private bool isPlayerInZone = false;
    private bool hitCheckpointA = false;
    private bool hitCheckpointB = false;
    private bool hitCheckpointC = false;
    private bool hitCheckpointD = false; // Added D

    // This is called by LoopTrigger.cs
    public void PlayerEnteredTrigger(TriggerType type, PlayerSpeedBoost player)
    {
        if (player == null) return;

        // --- Zone Check ---
        if (type == TriggerType.Zone)
        {
            isPlayerInZone = true;
            return;
        }

        // --- Checkpoint Logic (only runs if player is in the zone) ---
        if (!isPlayerInZone) return;

        // Set the flag for whichever checkpoint was hit
        switch (type)
        {
            case TriggerType.CheckpointA:
                hitCheckpointA = true;
                break;
            case TriggerType.CheckpointB:
                hitCheckpointB = true;
                break;
            case TriggerType.CheckpointC:
                hitCheckpointC = true;
                break;
            case TriggerType.CheckpointD:
                hitCheckpointD = true;
                break;
        }

        // After hitting any checkpoint, check if the loop is complete
        CheckForLoopCompletion(player);
    }

    private void CheckForLoopCompletion(PlayerSpeedBoost player)
    {
        // Check if ALL four points have been visited
        if (hitCheckpointA && hitCheckpointB && hitCheckpointC && hitCheckpointD)
        {
            // SUCCESS!
            if (player.TryTriggerBoost())
            {
                Debug.Log("Loop Complete (All 4 checkpoints)! Boost applied.");
            }
            
            // Reset the checklist for the next lap
            ResetCheckpoints();
        }
    }

    // This is called by LoopTrigger.cs
    public void PlayerExitedTrigger(TriggerType type, PlayerSpeedBoost player)
    {
        if (type == TriggerType.Zone)
        {
            // If player leaves the main zone, reset everything.
            isPlayerInZone = false;
            ResetCheckpoints();
        }
    }

    // Helper function to reset the lap's checklist
    private void ResetCheckpoints()
    {
        hitCheckpointA = false;
        hitCheckpointB = false;
        hitCheckpointC = false;
        hitCheckpointD = false; // Make sure to reset D
    }
}