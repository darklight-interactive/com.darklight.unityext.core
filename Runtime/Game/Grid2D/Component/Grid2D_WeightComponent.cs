using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_WeightComponent : Grid2D_Component
    {
        [SerializeField] int _defaultWeight = 1;

        Cell2D.Visitor _registrationVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.RegisterComponent(Cell2D.Component.TypeTag.WEIGHT);

            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent != null)
            {
                weightComponent.SetWeight(_defaultWeight);
            }
        });

        public override void InitializeComponent(Grid2D baseObj)
        {
            base.InitializeComponent(baseObj);
            baseObj.SendVisitorToAllCells(_registrationVisitor);
        }

        public override void UpdateComponent()
        {
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

