
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
            cell.RecalculateDataFromGrid(_grid);

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
