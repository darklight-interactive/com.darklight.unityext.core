
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
        Grid2D_Config config;
        public Cell2DUpdater(Grid2D_Config config)
        {
            this.config = config;
        }

        public void VisitCell(Cell2D cell)
        {
            Vector3 position = config.CalculatePositionFromKey(cell.Data.Key);
            Vector2Int coordinate = config.CalculateCoordinateFromKey(cell.Data.Key);
            Vector3 normal = config.gridNormal;
            Vector3 dimensions = config.cellDimensions;

            cell.Data.SetPosition(position);
            cell.Data.SetCoordinate(coordinate);
            cell.Data.SetNormal(normal);
            cell.Data.SetDimensions(dimensions);

            foreach (ICell2DComponent component in cell.Components)
            {
                component.Update();
            }
        }
    }

    public class Cell2D_GizmoRenderer : ICell2DVisitor
    {
        public void VisitCell(Cell2D cell)
        {
            foreach (ICell2DComponent component in cell.Components)
            {
                component.DrawGizmos();
            }
        }
    }

    public class Cell2D_EditorGizmoRenderer : ICell2DVisitor
    {
        public void VisitCell(Cell2D cell)
        {
            foreach (ICell2DComponent component in cell.Components)
            {
                component.DrawEditorGizmos();
            }
        }
    }
}
