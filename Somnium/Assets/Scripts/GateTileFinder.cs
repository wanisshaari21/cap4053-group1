using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GateTileFinder : MonoBehaviour
{
    public Tilemap tilemap;  // assign your gate Tilemap
    public List<Vector3Int> gateTilePositions = new List<Vector3Int>();

    [ContextMenu("Get Gate Tiles")]
    public void GetGateTiles()
    {
        gateTilePositions.Clear();

        // Loop through all positions in the Tilemap's bounds
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null) // only include tiles that exist
                {
                    gateTilePositions.Add(pos);
                }
            }
        }

        Debug.Log("Found " + gateTilePositions.Count + " gate tiles.");
    }
}