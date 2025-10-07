using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public int x, y;     // grid position in the board
    public int type;     // which sprite index (color, shape, etc.)

    // If you still want to track merges or special states, you can keep this
    [HideInInspector] public bool mergedThisMove;

    // Updates the tile’s look
    public void SetSprite(Sprite s)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = s;
        }
    }
}
