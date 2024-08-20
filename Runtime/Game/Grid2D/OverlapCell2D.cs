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

    public OverlapCell(Grid2DSettings settings, Vector2Int gridKey, Vector3 gridPosition, LayerMask layerMask) : base(settings, gridKey, gridPosition)
    {
        this.layerMask = layerMask;
        this.disabledInitially = base.disabled;
    }

    public void UpdateData()
    {
        // Update the collider data
        this.colliders = Physics2D.OverlapBoxAll(position, dimensions, 0, layerMask);
    }
}