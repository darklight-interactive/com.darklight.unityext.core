
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICellVisitor
    {
        void VisitCell(BaseCell cell);
    }

    public class CellUpdater : ICellVisitor
    {
        Grid.Config config;
        public CellUpdater(Grid.Config config)
        {
            this.config = config;
        }

        public void VisitCell(BaseCell cell)
        {
            Vector3 position = config.CalculatePositionFromKey(cell.Data.Key);
            Vector2Int coordinate = config.CalculateCoordinateFromKey(cell.Data.Key);
            Vector3 normal = config.gridNormal;
            Vector3 dimensions = config.cellDimensions;

            cell.Data.SetPosition(position);
            cell.Data.SetCoordinate(coordinate);
            cell.Data.SetNormal(normal);
            cell.Data.SetDimensions(dimensions);

            foreach (ICellComponent component in cell.Components)
            {
                component.Update();
            }
        }
    }

    public class CellGizmoRenderer : ICellVisitor
    {
        public void VisitCell(BaseCell cell)
        {
            foreach (ICellComponent component in cell.Components)
            {
                component.DrawGizmos();
            }
        }
    }

    public class CellEditorGizmoRenderer : ICellVisitor
    {
        public void VisitCell(BaseCell cell)
        {
            foreach (ICellComponent component in cell.Components)
            {
                component.DrawEditorGizmos();
            }
        }
    }
}
