using UnityEngine;

public static class PlayerMemory
{
    public static Vector3 savedPosition;
    public static bool hasSaved = false;

    // Ignore triggers until this realtime (prevents instant re-trigger loops)
    public static float ignoreTriggersUntil = 0f;
}
