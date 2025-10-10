using UnityEngine;

public class PlayerSpawnAtMemory : MonoBehaviour
{
    public Transform defaultSpawnPoint;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (PlayerMemory.hasSaved)
            player.transform.position = PlayerMemory.savedPosition;
        else if (defaultSpawnPoint != null)
            player.transform.position = defaultSpawnPoint.position;

        // Arm a short grace period to ignore triggers right after spawn
        PlayerMemory.ignoreTriggersUntil = Time.realtimeSinceStartup + 0.95f;
    }
}
