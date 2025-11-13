// LoopTrigger.cs
using UnityEngine;

// Define the different types of triggers we can be
public enum TriggerType { Zone, CheckpointA, CheckpointB, CheckpointC, CheckpointD }

[RequireComponent(typeof(Collider2D))]
public class LoopTrigger : MonoBehaviour
{
    public TriggerType type;
    
    private RedStoneLoopManager manager;

    void Awake()
    {
        // Find the "brains" in our parent object
        manager = GetComponentInParent<RedStoneLoopManager>();
        
        // Safety check
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (manager == null) return;

        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // Tell the manager we were entered
            manager.PlayerEnteredTrigger(type, other.GetComponent<PlayerSpeedBoost>());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (manager == null) return;
        
        if (other.CompareTag("Player"))
        {
            // Tell the manager we were exited
            manager.PlayerExitedTrigger(type, other.GetComponent<PlayerSpeedBoost>());
        }
    }
}