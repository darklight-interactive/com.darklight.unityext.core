using System.Collections.Generic;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    [RequireComponent(typeof(Matrix))]
    public class MatrixOverlapComponent : Matrix.BaseComponent
    {
        // ======== [[ FIELDS ]] =========================== >>>>
        [SerializeField] LayerMask _layerMask;
        Dictionary<MatrixNode, int> _colliderWeightMap = new Dictionary<MatrixNode, int>();

        #region ======== [[ PROPERTIES ]] ================================== >>>>
        // -- (( INITIALIZATION EVENT )) -------- ))
        protected MatrixNode.EventRegistry.VisitCellComponentEvent InitEvent =>
            (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.OverlapComponent overlapComponent =
                    cell.ComponentReg.GetComponent(type) as MatrixNode.OverlapComponent;
                if (overlapComponent == null) return false;

                // << INITIALIZATION >> 
                overlapComponent.LayerMask = _layerMask;
                _colliderWeightMap[cell] = overlapComponent.GetColliderCount();

                overlapComponent.OnInitialize(cell);
                return true;
            };

        // -- (( UPDATE EVENT )) -------- ))
        protected MatrixNode.EventRegistry.VisitCellComponentEvent UpdateEvent =>
            (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.OverlapComponent overlapComponent =
                    cell.ComponentReg.GetComponent<MatrixNode.OverlapComponent>();
                if (overlapComponent == null) return false;

                // << UPDATE >>
                overlapComponent.LayerMask = _layerMask;
                _colliderWeightMap[cell] = overlapComponent.GetColliderCount();

                overlapComponent.OnUpdate();
                return true;
            };

        // -- (( VISITORS )) -------- ))
        protected override MatrixNode.ComponentVisitor CellComponent_InitVisitor =>
            MatrixNode.VisitorFactory.CreateComponentVisitor(this, InitEvent);
        protected override MatrixNode.ComponentVisitor CellComponent_UpdateVisitor =>
            MatrixNode.VisitorFactory.CreateComponentVisitor(this, UpdateEvent);
        #endregion

        // ======== [[ METHODS ]] ================================== >>>>
        #region -- (( INTERFACE )) : IComponent -------- ))
        public override void OnInitialize(Matrix baseObj)
        {
            _colliderWeightMap.Clear();

            base.OnInitialize(baseObj);
        }
        #endregion

        // -- (( GETTERS )) -------- ))
        public Dictionary<int, List<MatrixNode>> GetCellsByOverlap()
        {
            Dictionary<int, List<MatrixNode>> overlapMap = new Dictionary<int, List<MatrixNode>>();
            foreach (KeyValuePair<MatrixNode, int> pair in _colliderWeightMap)
            {
                if (!overlapMap.ContainsKey(pair.Value))
                    overlapMap[pair.Value] = new List<MatrixNode>();
                overlapMap[pair.Value].Add(pair.Key);
            }
            return overlapMap;
        }

        public List<MatrixNode> GetCellsWithColliderCount(int count)
        {
            List<MatrixNode> cells = new List<MatrixNode>();
            foreach (KeyValuePair<MatrixNode, int> pair in _colliderWeightMap)
            {
                if (pair.Value == count)
                    cells.Add(pair.Key);
            }
            return cells;
        }

        public MatrixNode GetClosestCellWithColliderCount(int count, Vector2 position)
        {
            MatrixNode closestCell = null;
            float closestDistance = float.MaxValue;
            foreach (KeyValuePair<MatrixNode, int> pair in _colliderWeightMap)
            {
                if (pair.Value == count)
                {
                    float distance = Vector2.Distance(pair.Key.Position, position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCell = pair.Key;
                    }
                }
            }
            return closestCell;
        }
    }
}

