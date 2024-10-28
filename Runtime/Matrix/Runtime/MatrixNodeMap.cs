using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using NaughtyAttributes;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public class NodeMap
        {
            Matrix _matrix;
            Context _ctx;
            Dictionary<Vector2Int, Node> _map = new Dictionary<Vector2Int, Node>();

            [SerializeField, ShowOnly] int _nodeCount;
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
                    if (_map == null || _map.Count == 0)
                        _nodes = new List<Node>();
                    else
                        _nodes = new List<Node>(_map.Values);
                    return _nodes;
                }
            }

            public Action OnUpdate;

            public NodeMap(Matrix matrix)
            {
                _matrix = matrix;
                _ctx = _matrix.GetContext();
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

            public void Refresh()
            {
                if (_matrix == null) return;

                if (_map == null)
                    _map = new Dictionary<Vector2Int, Node>();

                // << CHECK FOR CHANGES >>
                if (_matrix.GetContext().Equals(_ctx) == false)
                {
                    _ctx = _matrix.GetContext();
                    Matrix.SendVisitorToNodes(_nodes, _matrix.UpdateNodeContextVisitor);
                }

                if (IsDirty())
                    Clean();
                
                _nodeCount = _map.Count;
                _nodes = new List<Node>(_map.Values);
            }
        }
    }

}