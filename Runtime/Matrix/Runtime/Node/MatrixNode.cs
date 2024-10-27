using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public partial class Node : IVisitable<Node>
        {
            Matrix _matrix;
            [SerializeField] InternalData _data;

            public Matrix Matrix { get => _matrix; }
            public InternalData Data { get => _data; }
            public Vector2Int Key { get => _data.Key; }
            public Vector3 Position { get => _data.Position; }

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
        }
    }
}
