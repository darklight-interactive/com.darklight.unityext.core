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
            Matrix _matrix;
            [SerializeField] InternalData _data;

            public Matrix Matrix { get => _matrix; }
            public InternalData Data { get => _data; }
            public Vector2Int Key { get => _data.Key; }
            public Vector3 Position { get => _data.Position; }

            public delegate bool VisitNodeEvent(Node node);


            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Node(Matrix matrix, Vector2Int key)
            {
                _matrix = matrix;
                _data = new InternalData(this, key, matrix);
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            public void Refresh()
            {
                Data.Refresh();
            }

            // (( INTERFACE )) : IVisitable -------- ))
            public void Accept(IVisitor<Node> visitor)
            {
                visitor.Visit(this);
            }

            // (( GETTERS )) -------- ))
            public void GetWorldSpaceValues(out Vector3 position, out Vector2 dimensions, out Vector3 normal)
            {
                Data.Refresh();
                position = Data.Position;
                dimensions = Data.Dimensions;
                normal = Data.Normal;
            }

            public float GetMinDimension() => Mathf.Min(Data.Dimensions.x, Data.Dimensions.y);

            #region < PUBLIC_CLASS > [[ InternalData ]] ================================================================ 

            [Serializable]
            public class InternalData
            {
                Matrix _matrix;
                [SerializeField, ShowOnly] int _guid = Guid.NewGuid().GetHashCode();
                [SerializeField, ShowOnly] Vector2Int _key = Vector2Int.zero;
                [SerializeField, ShowOnly] Vector2Int _coordinate = Vector2Int.zero;

                [Header("World Space Values")]
                [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
                [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
                [SerializeField, ShowOnly] Vector2 _dimensions = Vector2.one;

                [Header("Flags")]
                [SerializeField, ShowOnly] bool _isDisabled = false;

                // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
                public InternalData(Node node, Vector2Int key, Matrix matrix)
                {
                    _matrix = matrix;
                    _key = key;
                    Refresh();
                }

                public void Refresh()
                {
                    _matrix.CalculateNodeWorldSpaceFromKey(_key, out _position, out _coordinate, out _normal, out _dimensions);
                }


                // ======== [[ PROPERTIES ]] ======================================================= >>>>
                public Vector2Int Key { get => _key; }
                public Vector2Int Coordinate { get => _coordinate; }
                public Vector2 Dimensions { get => _dimensions; }
                public float SizeAvg { get => (_dimensions.x + _dimensions.y) / 2f; }
                public Vector3 Position { get => _position; }
                public Vector3 Normal { get => _normal; }
                public bool Disabled { get => _isDisabled; }
            }
            #endregion

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
