
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICell2DVisitor
    {
        void VisitCell(Cell2D cell);
    }

    public class Cell2DUpdater : ICell2DVisitor
    {
        Grid2D _grid;
        public Cell2DUpdater(Grid2D grid)
        {
            _grid = grid;
        }

        public void VisitCell(Cell2D cell)
        {
            // Calculate the cell's transform
            Grid2D_SpatialUtility.CalculateCellTransform(
                out Vector3 position, out Vector2Int coordinate,
                out Vector3 normal, out Vector2 dimensions,
                cell, _grid.Config);

            // Assign the calculated values to the cell
            cell.Data.SetPosition(position);
            cell.Data.SetCoordinate(coordinate);
            cell.Data.SetNormal(normal);
            cell.Data.SetDimensions(dimensions);

            cell.SetConfig(_grid.Config.CellConfig);

            // Update the cell
            cell.Update();
        }
    }

    public class Cell2D_GizmoRenderer : ICell2DVisitor
    {
        public void VisitCell(Cell2D cell)
        {
            cell.DrawGizmos();
        }
    }

    public class Cell2D_EditorGizmoRenderer : ICell2DVisitor
    {
        public void VisitCell(Cell2D cell)
        {
            cell.DrawEditorGizmos();
        }
    }
}
