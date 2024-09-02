using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_OverlapComponent : Grid2D_Component
    {
        [SerializeField] LayerMask _layerMask;
        Cell2D.Visitor _overlapVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_OverlapComponent overlapComponent;
            overlapComponent = cell.ComponentReg.RegisterComponent(Cell2D.Component.TypeTag.OVERLAP) as Cell2D_OverlapComponent;
            overlapComponent.layerMask = _layerMask;
        });

        public override void InitializeComponent(Grid2D baseObj)
        {
            base.InitializeComponent(baseObj);
            baseObj.SendVisitorToAllCells(_overlapVisitor);
        }

        public override void UpdateComponent()
        {
            baseGrid.SendVisitorToAllCells(_overlapVisitor);
        }

        public override void DrawEditorGizmos()
        {
        }

        public override void DrawGizmos()
        {
        }

        public override TypeTag GetTypeTag() => TypeTag.OVERLAP;
    }
}

