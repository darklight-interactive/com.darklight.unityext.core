using System.Collections.Generic;
using Darklight.UnityExt.Behaviour.Interface;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [RequireComponent(typeof(Grid2D_OverlapComponent), typeof(Grid2D_WeightComponent))]
    public class Grid2D_WeightedSpawnerComponent : Grid2D_Component
    {
        Grid2D_OverlapComponent _overlapComponent;
        Grid2D_WeightComponent _weightComponent;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        Cell2D.ComponentVisitor _overlapVisitor => new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.OVERLAP);
        Cell2D.ComponentVisitor _weightVisitor => new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        Cell2D.ComponentVisitor _overlapGizmosVisitor => Cell2D.VisitorFactory.CreateGizmosVisitor(Cell2D.ComponentTypeKey.OVERLAP);
        Cell2D.ComponentVisitor _weightGizmosVisitor => Cell2D.VisitorFactory.CreateGizmosVisitor(Cell2D.ComponentTypeKey.WEIGHT);

        // ======== [[ METHODS ]] ================================== >>>>
        public override void Initialize(Grid2D baseObj)
        {
            base.Initialize(baseObj);

            _overlapComponent = GetComponent<Grid2D_OverlapComponent>();
            if (_overlapComponent == null)
                _overlapComponent = gameObject.AddComponent<Grid2D_OverlapComponent>();

            _weightComponent = GetComponent<Grid2D_WeightComponent>();
            if (_weightComponent == null)
                _weightComponent = gameObject.AddComponent<Grid2D_WeightComponent>();
        }

        public override void DrawGizmos()
        {
            BaseGrid.SendVisitorToAllCells(_overlapGizmosVisitor);
            BaseGrid.SendVisitorToAllCells(_weightGizmosVisitor);
        }

    }
}