using System;
using System.Collections.Generic;

using NaughtyAttributes;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public class NodeMap
        {
            Context _ctx;
            Dictionary<Vector2Int, Node> _map = new Dictionary<Vector2Int, Node>();

            [SerializeField] List<Node> _nodes = new List<Node>();

            public List<Vector2Int> Keys
            {
                get
                {
                    if (_map == null || _map.Count == 0) return new List<Vector2Int>();
                    return new List<Vector2Int>(_map.Keys);
                }
            }
            public List<Node> Nodes
            {
                get
                {
                    if (_map == null || _map.Count == 0) return new List<Node>();
                    return new List<Node>(_map.Values);
                }
            }

            public Action OnUpdate;

            public NodeMap(Context context)
            {
                _ctx = context;
                _map = new Dictionary<Vector2Int, Node>();
                Refresh();
            }

            #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================ 
            void AddNode(Vector2Int key)
            {
                if (_map.ContainsKey(key)) return;

                Node node = new Node(_ctx, key);
                _map[key] = node;
            }

            void RemoveNode(Vector2Int key)
            {
                if (!_map.ContainsKey(key)) return;
                _map.Remove(key);
            }

            bool IsDirty()
            {
                int expectedNodeCount = _ctx.MatrixColumns * _ctx.MatrixRows;
                return _map.Count != expectedNodeCount;
            }

            bool IsKeyInBounds(Vector2Int key)
            {
                return key.x < _ctx.MatrixColumns && key.y < _ctx.MatrixRows;
            }

            void Clean()
            {
                // << REMOVE OUT OF BOUNDS NODES >>
                List<Vector2Int> keys = new List<Vector2Int>(_map.Keys);
                foreach (Vector2Int key in keys)
                    if (!IsKeyInBounds(key)) RemoveNode(key);

                // << ADD NEW NODES >>
                for (int x = 0; x < _ctx.MatrixColumns; x++)
                {
                    for (int y = 0; y < _ctx.MatrixRows; y++)
                    {
                        AddNode(new Vector2Int(x, y));
                    }
                }
            }
            #endregion

            public Node GetNode(Vector2Int key)
            {
                if (_map.ContainsKey(key))
                    return _map[key];
                return null;
            }

            public List<Node> GetNodes(List<Vector2Int> keys)
            {
                List<Node> nodes = new List<Node>(keys.Count);
                foreach (Vector2Int key in keys)
                {
                    Node node = GetNode(key);
                    if (node != null) nodes.Add(node);
                }
                return nodes;
            }

            public void UpdateContext(Context context)
            {
                _ctx = context;
                Refresh();
            }

            public void Refresh()
            {
                if (_map == null)
                    _map = new Dictionary<Vector2Int, Node>();

                if (IsDirty()) Clean();

                _nodes = new List<Node>(_map.Values);
            }


            static Vector2 CalculateAlignmentOffset(Context ctx)
            {
                int rows = ctx.MatrixRows - 1;
                int columns = ctx.MatrixColumns - 1;
                Vector2 originOffset = Vector2.zero;

                switch (ctx.MatrixAlignment)
                {
                    case Matrix.Alignment.BottomLeft:
                        originOffset = Vector2.zero;
                        break;
                    case Matrix.Alignment.BottomCenter:
                        originOffset = new Vector2(
                            -columns * rows / 2,
                            0
                        );
                        break;
                    case Matrix.Alignment.BottomRight:
                        originOffset = new Vector2(
                            -columns * rows,
                            0
                        );
                        break;
                    case Matrix.Alignment.MiddleLeft:
                        originOffset = new Vector2(
                            0,
                            -rows * rows / 2
                        );
                        break;
                    case Matrix.Alignment.MiddleCenter:
                        originOffset = new Vector2(
                            -columns * rows / 2,
                            -rows * rows / 2
                        );
                        break;
                    case Matrix.Alignment.MiddleRight:
                        originOffset = new Vector2(
                            -columns * rows,
                            -rows * rows / 2
                        );
                        break;
                    case Matrix.Alignment.TopLeft:
                        originOffset = new Vector2(
                            0,
                            -rows * rows
                        );
                        break;
                    case Matrix.Alignment.TopCenter:
                        originOffset = new Vector2(
                            -columns * rows / 2,
                            -rows * rows
                        );
                        break;
                    case Matrix.Alignment.TopRight:
                        originOffset = new Vector2(
                            -columns * rows,
                            -rows * rows
                        );
                        break;
                }

                return originOffset;
            }

            static Vector2Int CalculateOriginKey(Context ctx)
            {
                int rows = ctx.MatrixRows - 1;
                int columns = ctx.MatrixColumns - 1;
                Vector2Int originKey = Vector2Int.zero;
                switch (ctx.MatrixAlignment)
                {
                    case Matrix.Alignment.BottomLeft:
                        originKey = new Vector2Int(0, 0);
                        break;
                    case Matrix.Alignment.BottomCenter:
                        originKey = new Vector2Int(Mathf.FloorToInt(columns / 2), 0);
                        break;
                    case Matrix.Alignment.BottomRight:
                        originKey = new Vector2Int(Mathf.FloorToInt(columns), 0);
                        break;
                    case Matrix.Alignment.MiddleLeft:
                        originKey = new Vector2Int(0, Mathf.FloorToInt(rows / 2));
                        break;
                    case Matrix.Alignment.MiddleCenter:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(columns / 2),
                            Mathf.FloorToInt(rows / 2)
                        );
                        break;
                    case Matrix.Alignment.MiddleRight:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(columns),
                            Mathf.FloorToInt(rows / 2)
                        );
                        break;
                    case Matrix.Alignment.TopLeft:
                        originKey = new Vector2Int(0, Mathf.FloorToInt(rows));
                        break;
                    case Matrix.Alignment.TopCenter:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(columns / 2),
                            Mathf.FloorToInt(rows)
                        );
                        break;
                    case Matrix.Alignment.TopRight:
                        originKey = new Vector2Int(
                            Mathf.FloorToInt(columns),
                            Mathf.FloorToInt(rows)
                        );
                        break;
                }
                return originKey;
            }

            public static Vector3 CalculateNodePosition(Context ctx, Vector2Int key)
            {
                // Calculate the node position offset in world space based on dimensions
                Vector2 keyOffsetPos = key * ctx.NodeDimensions;

                // Calculate the origin position offset in world space based on alignment
                Vector2 originOffset = CalculateAlignmentOffset(ctx);

                // Calculate the spacing offset and clamp to avoid overlapping cells
                Vector2 spacingOffsetPos = ctx.NodeSpacing + Vector2.one;
                spacingOffsetPos.x = Mathf.Clamp(spacingOffsetPos.x, 0.5f, float.MaxValue);
                spacingOffsetPos.y = Mathf.Clamp(spacingOffsetPos.y, 0.5f, float.MaxValue);

                // Calculate bonding offsets
                Vector2 bondingOffset = Vector2.zero;
                if (key.y % 2 == 0)
                    bondingOffset.x = ctx.NodeBonding.x;
                if (key.x % 2 == 0)
                    bondingOffset.y = ctx.NodeBonding.y;

                Vector2 cellPosition = keyOffsetPos + originOffset;
                cellPosition *= spacingOffsetPos;
                cellPosition += bondingOffset;

                // Apply a scale transformation that flips the x-axis
                Vector3 scale = new Vector3(-1, 1, 1);  // Inverts the x-axis
                Vector3 transformedPosition = Vector3.Scale(new Vector3(cellPosition.x, cellPosition.y, 0), scale);

                // Apply rotation based on grid's normal and return the final world position
                Quaternion rotation = Quaternion.LookRotation(ctx.MatrixNormal, Vector3.forward);
                return ctx.MatrixPosition + (rotation * transformedPosition);
            }

            public static Vector2Int CalculateNodeCoordinateFromKey(Context ctx, Vector2Int key)
            {
                Vector2Int originKey = CalculateOriginKey(ctx);
                return key - originKey;
            }
        }
    }

}