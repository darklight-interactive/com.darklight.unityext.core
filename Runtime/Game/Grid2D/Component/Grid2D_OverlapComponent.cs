using Darklight.UnityExt.Editor;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_OverlapComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] =========================== >>>>
        [SerializeField] LayerMask _layerMask;
        [SerializeField] bool _showGizmos;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        protected Cell2D.EventRegistry.VisitCellComponentEvent InitEvent =
            (Cell2D cell, Cell2D.ComponentTypeKey type) =>
            {
                // Only initialize if the component type is OVERLAP
                if (type != Cell2D.ComponentTypeKey.OVERLAP) return;
                Cell2D_OverlapComponent overlapComponent =
                    cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
                if (overlapComponent == null) return;

                // << INITIALIZATION >> 
                overlapComponent.LayerMask = overlapComponent.LayerMask;
                overlapComponent.OnColliderEnter += overlapComponent.OnColliderEnter;
                overlapComponent.OnColliderExit += overlapComponent.OnColliderExit;
                overlapComponent.Initialize(cell);
            };

        protected Cell2D.EventRegistry.VisitCellComponentEvent UpdateEvent =
            (Cell2D cell, Cell2D.ComponentTypeKey type) =>
            {
                if (type != Cell2D.ComponentTypeKey.OVERLAP) return;
                Cell2D_OverlapComponent overlapComponent =
                    cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
                if (overlapComponent == null) return;

                // << UPDATE >>
                overlapComponent.Updater();
            };

        protected override Cell2D.ComponentVisitor CellComponentVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.OVERLAP);

        // ======== [[ EVENTS ]] ================================== >>>>
        public UltEvent<Cell2D> HandleCollisionEnter;
        public UltEvent<Cell2D> HandleCollisionExit;

        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( INTERFACE METHODS )) -------- ))
        public override void Initialize(Grid2D baseObj)
        {
            base.Initialize(baseObj);
        }

        public override void Updater()
        {
            BaseGrid.SendVisitorToAllCells(CellComponentVisitor);
        }

        public override void DrawGizmos()
        {
            if (!_showGizmos) return;
            BaseGrid.SendVisitorToAllCells(new Cell2D.Visitor((Cell2D cell) =>
            {
                Cell2D_OverlapComponent overlapComponent = cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
                if (overlapComponent != null)
                {
                    overlapComponent.DrawGizmos();
                }
            }));
        }

        // -- (( EVENT HANDLERS )) -------- ))
        void OnColliderEnter(Cell2D cell, Collider2D collider)
        {
            HandleCollisionEnter?.Invoke(cell);
            //Debug.Log($"Collider entered cell {cell.Key} :: {collider}");
        }

        void OnColliderExit(Cell2D cell, Collider2D collider)
        {
            HandleCollisionExit?.Invoke(cell);
            //Debug.Log($"Collider exited cell {cell.Key} :: {collider}");
        }
    }
}

