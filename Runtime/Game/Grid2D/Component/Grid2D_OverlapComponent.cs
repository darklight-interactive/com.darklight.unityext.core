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
        // -- (( VISITORS )) -------- ))
        Cell2D.Visitor _registrationVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.RegisterComponent(Cell2D_Component.Type.OVERLAP);
            Cell2D_OverlapComponent overlapComponent = cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
            if (overlapComponent == null) return;

            overlapComponent.LayerMask = _layerMask;

            overlapComponent.OnColliderEnter = OnColliderEnter;
            overlapComponent.OnColliderExit = OnColliderExit;
        });

        Cell2D.Visitor _updateVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_OverlapComponent overlapComponent = cell.ComponentReg.GetComponent<Cell2D_OverlapComponent>();
            if (overlapComponent == null)
            {
                cell.Accept(_registrationVisitor);
                return;
            }

            overlapComponent.LayerMask = _layerMask;
            overlapComponent.UpdateComponent();
        });

        // ======== [[ EVENTS ]] ================================== >>>>
        public UltEvent<Cell2D> HandleCollisionEnter;
        public UltEvent<Cell2D> HandleCollisionExit;

        // ======== [[ METHODS ]] ================================== >>>>
        public override void InitializeComponent(Grid2D baseObj)
        {
            base.InitializeComponent(baseObj);
            baseObj.SendVisitorToAllCells(_registrationVisitor);
        }

        public override void UpdateComponent()
        {
            baseGrid.SendVisitorToAllCells(_updateVisitor);
        }

        public override Type GetTypeTag() => Type.OVERLAP;

        public override void DrawGizmos()
        {
            if (!_showGizmos) return;
            baseGrid.SendVisitorToAllCells(new Cell2D.Visitor((Cell2D cell) =>
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

