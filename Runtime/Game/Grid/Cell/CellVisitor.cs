
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICellVisitor
    {
        void Visit(BaseCell cell);
    }

    public class CellUpdater : ICellVisitor
    {
        Grid.Config config;
        public CellUpdater(Grid.Config config)
        {
            this.config = config;
        }

        public void Visit(BaseCell cell)
        {
            Vector3 position = config.CalculatePositionFromKey(cell.Data.key);
            Vector2Int coordinate = config.CalculateCoordinateFromKey(cell.Data.key);
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
        public void Visit(BaseCell cell)
        {
            BaseCellData data = cell.Data;
            CustomGizmos.DrawWireRect(data.position, data.dimensions, data.normal, Color.grey);

            foreach (ICellComponent component in cell.Components)
            {
                component.DrawGizmos();
            }
        }
    }
}
