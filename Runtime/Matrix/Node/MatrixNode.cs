using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public partial class Node : IVisitable<Node>
        {
            Matrix _matrix;

            bool _enabled;
            bool _initialized;

            [SerializeField, ShowOnly]
            string _name = "Cell2D";

            [SerializeField]
            NodeData _data;

            // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
            public Node(Vector2Int key) => Initialize(key);

            // ======== [[ PROPERTIES ]] ======================================================= >>>>
            public string Name => _name;
            public NodeData Data => _data;
            public Vector2Int Key => _data.Key;
            public Vector2Int Coordinate => _data.Coordinate;
            public Vector3 Position => _data.Position;
            public Vector3 Normal => _data.Normal;
            public Vector2 Dimensions => _data.Dimensions;
            public bool IsInitialized => _initialized;

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public Node(Matrix matrix, Vector2Int key)
            {
                _matrix = matrix;
                Initialize(key);
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            #region -- (( RUNTIME )) -------- )))
            public void Initialize(Vector2Int key)
            {
                // Create the data
                _data = new NodeData(key);

                // Set the name
                _name = $"Cell2D ({key.x},{key.y})";

                _initialized = true;
            }

            public void Refresh()
            {
                if (!_initialized)
                    return;
            }

            public void DrawGizmos()
            {
                if (!_initialized) return;

                Gizmos.color = Color.grey;
                Gizmos.DrawWireCube(Position, new Vector3(Dimensions.x, 0, Dimensions.y));
            }
            #endregion

            // -- (( HANDLERS )) -------- )))
            public void RecalculateDataFromGrid(Matrix grid)
            {
                if (!_initialized)
                    return;
                if (grid == null)
                    return;
                if (grid.GetConfig() == null)
                    return;
                if (Data == null)
                    return;

                // Calculate the cell's transform
                Matrix.CalculateCellTransform(
                    out Vector3 position,
                    out Vector2Int coordinate,
                    out Vector3 normal,
                    out Vector2 dimensions,
                    this,
                    grid.GetConfig()
                );

                // Assign the calculated values to the cell
                Data.SetPosition(position);
                Data.SetCoordinate(coordinate);
                Data.SetNormal(normal);
                Data.SetDimensions(dimensions);
            }

            public Node Clone()
            {
                Node clone = new Node(Data.Key);
                NodeData newData = new NodeData(Data);
                clone.SetData(newData);
                return clone;
            }

            // (( INTERFACE )) : IVisitable -------- ))
            public void Accept(IVisitor<Node> visitor)
            {
                visitor.Visit(this);
            }

            // (( GETTERS )) -------- ))
            public bool IsEnabled() => _enabled;

            public float GetMinDimension() => Mathf.Min(Data.Dimensions.x, Data.Dimensions.y);

            public void GetTransformData(
                out Vector3 position,
                out Vector2 dimensions,
                out Vector3 normal
            )
            {
                position = Data.Position;
                dimensions = Data.Dimensions;
                normal = Data.Normal;
            }

            // (( SETTERS )) -------- ))
            protected void SetData(NodeData data) => _data = data;

            protected void SetEnabled(bool enabled) => _enabled = enabled;
        }
    }
}
