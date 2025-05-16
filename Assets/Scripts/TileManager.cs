using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap interactible;
    [SerializeField] private Tile hiddenInteractableTile;
    void start()
    {
        foreach(var position in interactible.cellBounds.allPositionsWithin)
        {
            interactible.SetTile(position, hiddenInteractableTile);
        }
    }
}
