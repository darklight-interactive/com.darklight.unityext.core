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
    public partial class Matrix
    {
        [System.Serializable]
        public class Node : IVisitable<Node>
        {
            Context _ctx;
            [SerializeField, ShowOnly] Vector2Int _key = Vector2Int.zero;
            [SerializeField, ShowOnly] Vector2Int _coordinate = Vector2Int.zero;

            [Header("World Space Values")]
            [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
            [SerializeField, ShowOnly] Quaternion _rotation;
            [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
            [SerializeField, ShowOnly] Vector2 _dimensions = Vector2.one;

            public delegate bool VisitNodeEvent(Node node);

            public Vector2Int Key => _key;
            public Vector2Int Coordinate => _coordinate;
            public Vector3 Position => _position;
            public Quaternion Rotation => _rotation;
            public Vector3 Normal => _normal;
            public Vector2 Dimensions => _dimensions;


            #region ---- < PUBLIC_PROPERTIES > ( Span ) --------------------------------- 
            public float DiagonalSpan => Mathf.Sqrt(Mathf.Pow(_dimensions.x, 2) + Mathf.Pow(_dimensions.y, 2));
            public float AverageSpan => (_dimensions.x + _dimensions.y) * 0.5f;
            public float MaxSpan => Mathf.Max(_dimensions.x, _dimensions.y);
            public float MinSpan => Mathf.Min(_dimensions.x, _dimensions.y);
            #endregion

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Node(Context ctx, Vector2Int key)
            {
                _ctx = ctx;
                _key = key;
                Refresh();
            }

            // (( INTERFACE )) : IVisitable -------- ))
            public void Accept(IVisitor<Node> visitor)
            {
                visitor.Visit(this);
            }

            public void UpdateContext(Context context)
            {
                _ctx = context;
                Refresh();
            }

            public void Refresh()
            {
                _coordinate = _ctx.CalculateNodeCoordinateFromKey(_key);

                _ctx.CalculateNodeTransformFromKey(_key, out _position, out _rotation);
                _normal = _ctx.CalculateNormal();
                
                _dimensions = _ctx.NodeDimensions;
            }

            #region < PUBLIC_CLASS > [[ Visitor ]] ================================================================ 

            public class Visitor : IVisitor<Node>
            {
                VisitNodeEvent _visitFunction;
                public Visitor(VisitNodeEvent visitFunction)
                {
                    _visitFunction = visitFunction;
                }

                public virtual void Visit(Node cell)
                {
                    _visitFunction(cell);
                }
            }
            #endregion
        }


    }
}
