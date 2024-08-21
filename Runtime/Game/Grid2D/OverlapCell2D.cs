using Darklight.UnityExt.Game;
using UnityEngine;

/// <summary>
/// Create and stores the data from a Physics2D.OverlapBoxAll call at the world position of the Grid2DData. 
/// </summary>
public class OverlapCell : WeightedCell2D
{
    private bool disabledInitially = false;
    public LayerMask layerMask; // The layer mask to use for the OverlapBoxAll called
    public Collider2D[] colliders = new Collider2D[0]; /// The colliders found by the OverlapBoxAll call

    public OverlapCell(Grid2D grid, Vector2Int key) : base(grid, key)
    {
        // Initialize the colliders array
        colliders = new Collider2D[0];
    }

}