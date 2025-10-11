using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostDebug : MonoBehaviour
{
    PlayerSpeedBoost boost;

    void Awake()
    {
        boost = GetComponent<PlayerSpeedBoost>();
        Debug.Log("[BoostDebug] Awake - boost component " + (boost ? "FOUND" : "MISSING"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[BoostDebug] B pressed");
            if (boost != null)
            {
                bool started = boost.TryTriggerBoost();
                Debug.Log("[BoostDebug] TryTriggerBoost() => " + started);
            }
        }

        if (boost != null && boost.IsBoosting)
            Debug.Log("[BoostDebug] BOOST ON");
    }
}
