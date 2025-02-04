using System;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    [System.Serializable]
    public class MatrixNode : IVisitable<MatrixNode>
    {
        MatrixInfo _matrixInfo;

        [SerializeField, ShowOnly]
        Vector2Int _key = Vector2Int.zero;

        [SerializeField, ShowOnly]
        Vector2Int _coordinate = Vector2Int.zero;

        [Header("World Space Values")]
        [SerializeField, ShowOnly]
        Vector3 _position = Vector3.zero;

        [SerializeField, ShowOnly]
        Quaternion _rotation;

        [SerializeField, ShowOnly]
        Vector3 _normal = Vector3.up;

        [SerializeField, ShowOnly]
        Vector2 _dimensions = Vector2.one;

        [SerializeField, ShowOnly]
        int _partition;

        public delegate bool VisitNodeEvent(MatrixNode node);

        public MatrixInfo MatrixInfo => _matrixInfo;
        public Vector2Int Key => _key;
        public Vector2Int Coordinate => _coordinate;
        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;
        public Vector3 Normal => _normal;
        public Vector2 Dimensions => _dimensions;
        public int Partition => _partition;

        #region ---- < PUBLIC_PROPERTIES > ( Span ) ---------------------------------
        public float DiagonalSpan =>
            Mathf.Sqrt(Mathf.Pow(_dimensions.x, 2) + Mathf.Pow(_dimensions.y, 2));
        public float AverageSpan => (_dimensions.x + _dimensions.y) * 0.5f;
        public float MaxSpan => Mathf.Max(_dimensions.x, _dimensions.y);
        public float MinSpan => Mathf.Min(_dimensions.x, _dimensions.y);
        #endregion

        // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
        public MatrixNode(MatrixInfo info, Vector2Int key)
        {
            _matrixInfo = info;
            _key = key;
            Refresh();
        }

        // (( INTERFACE )) : IVisitable -------- ))
        public void Accept(IVisitor<MatrixNode> visitor)
        {
            visitor.Visit(this);
        }

        public void Refresh()
        {
            _coordinate = _matrixInfo.CalculateNodeCoordinateFromKey(_key);
            _matrixInfo.CalculateNodeTransformFromKey(_key, out _position, out _rotation);
            //_normal = _ctx.CalculateNormal();
            _partition = _matrixInfo.CalculateCellPartition(_key);

            _dimensions = _matrixInfo.NodeDimensions;
        }

        #region < PUBLIC_CLASS > [[ Visitor ]] ================================================================

        public class Visitor : IVisitor<MatrixNode>
        {
            VisitNodeEvent _visitFunction;

            public Visitor(VisitNodeEvent visitFunction)
            {
                _visitFunction = visitFunction;
            }

            public virtual void Visit(MatrixNode cell)
            {
                _visitFunction(cell);
            }
        }
        #endregion
    }
}
