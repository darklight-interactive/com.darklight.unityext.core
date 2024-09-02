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
        protected Cell2D.EventRegistry.VisitCellComponentEvent InitEvent =>
            (Cell2D cell, Cell2D.ComponentTypeKey type) =>
            {
                Cell2D_OverlapComponent overlapComponent =
                    cell.ComponentReg.GetComponent(type) as Cell2D_OverlapComponent;
                if (overlapComponent == null) return;

                // << INITIALIZATION >> 
                overlapComponent.LayerMask = _layerMask;
                overlapComponent.OnColliderEnter += OnColliderEnter;
                overlapComponent.OnColliderExit += OnColliderExit;
                overlapComponent.Initialize(cell);
            };

        protected Cell2D.EventRegistry.VisitCellComponentEvent UpdateEvent =>
            (Cell2D cell, Cell2D.ComponentTypeKey type) =>
            {
                // Only update if the component type is OVERLAP
                if (type != Cell2D.ComponentTypeKey.OVERLAP) return;
                Cell2D_OverlapComponent overlapComponent =
                    cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
                if (overlapComponent == null) return;

                // << UPDATE >>
                overlapComponent.LayerMask = _layerMask;
                overlapComponent.Updater();
            };

        protected Cell2D.ComponentVisitor CellComponentVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.OVERLAP, InitEvent, UpdateEvent);
        protected override Cell2D.ComponentVisitor GizmosVisitor =>
            Cell2D.VisitorFactory.CreateGizmosVisitor(Cell2D.ComponentTypeKey.OVERLAP);
        protected override Cell2D.ComponentVisitor EditorGizmosVisitor =>
            Cell2D.VisitorFactory.CreateEditorGizmosVisitor(Cell2D.ComponentTypeKey.OVERLAP);

        // ======== [[ EVENTS ]] ================================== >>>>
        public UltEvent<Cell2D> HandleCollisionEnter;
        public UltEvent<Cell2D> HandleCollisionExit;

        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( INTERFACE METHODS )) -------- ))

        public override void Updater()
        {
            BaseGrid.SendVisitorToAllCells(CellComponentVisitor);
        }

        public override void DrawGizmos()
        {
            if (!_showGizmos) return;
            BaseGrid.SendVisitorToAllCells(GizmosVisitor);
        }

        public override void DrawEditorGizmos()
        {
            if (!_showGizmos) return;
            BaseGrid.SendVisitorToAllCells(EditorGizmosVisitor);
        }

        // -- (( EVENT HANDLERS )) -------- ))
        void OnColliderEnter(Cell2D cell, Collider2D collider)
        {
            HandleCollisionEnter?.Invoke(cell);
        }

        void OnColliderExit(Cell2D cell, Collider2D collider)
        {
            HandleCollisionExit?.Invoke(cell);
        }
    }
}

