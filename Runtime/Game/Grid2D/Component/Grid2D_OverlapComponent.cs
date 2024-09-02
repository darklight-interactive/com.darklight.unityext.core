using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_OverlapComponent : Grid2D_Component
    {
        [SerializeField] LayerMask _layerMask;

        Cell2D.Visitor _registrationVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.RegisterComponent(Cell2D.Component.TypeTag.OVERLAP);
        });

        Cell2D.Visitor _updateVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_OverlapComponent overlapComponent = cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
            if (overlapComponent != null)
            {
                overlapComponent.LayerMask = _layerMask;
                overlapComponent.UpdateComponent();
            }
            else
            {
                cell.ComponentReg.RegisterComponent(Cell2D.Component.TypeTag.OVERLAP);
            }
        });

        public override void InitializeComponent(Grid2D baseObj)
        {
            base.InitializeComponent(baseObj);
            baseObj.SendVisitorToAllCells(_registrationVisitor);
        }

        public override void UpdateComponent()
        {
            baseGrid.SendVisitorToAllCells(_updateVisitor);
        }

        public override TypeTag GetTypeTag() => TypeTag.OVERLAP;
    }
}

