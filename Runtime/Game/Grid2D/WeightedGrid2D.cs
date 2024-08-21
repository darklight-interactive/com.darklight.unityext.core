using Darklight.UnityExt.Game;
using UnityEngine;

[ExecuteAlways]
public class WeightedGrid2D : MonoBehaviour
{

    [System.Serializable]
    public class WeightedData : Grid2D.Cell.Data
    {
        public float weight;
        public WeightedData(Grid2D grid, Vector2Int key) : base(grid, key) { }
    }

    [System.Serializable]
    public class WeightedCell : Grid2D.Cell<WeightedData>
    {
        public WeightedCell(Grid2D grid, Vector2Int key) : base(grid, key) { }
        public override void Initialize() { }
        public override void DrawGizmos(bool editMode = false)
        {
            //throw new System.NotImplementedException();
        }
    }

    public Grid2D<WeightedCell> _grid = new Grid2D<WeightedCell>();

}