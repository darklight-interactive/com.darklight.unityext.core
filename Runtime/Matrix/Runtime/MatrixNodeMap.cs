using System;
using System.Collections.Generic;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

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
            Dictionary<Vector2Int, Node> _map = new Dictionary<Vector2Int, Node>();
            bool _cacheIsDirty = true;

            [SerializeField] List<Vector2Int> _cachedKeys;
            [SerializeField] List<Node> _cachedNodes;

            Info _info => _matrix._info;

            public List<Vector2Int> Keys
            {
                get
                {
                    if (_cacheIsDirty || _cachedKeys == null)
                    {
                        _cachedKeys = new List<Vector2Int>(_map.Keys);
                        _cacheIsDirty = false;
                    }
                    return _cachedKeys;
                }
            }
            public List<Node> Nodes => _cachedNodes;

            public NodeMap(Matrix matrix)
            {
                _matrix = matrix;
                Refresh();
            }

            #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================ 
            void AddNode(Vector2Int key)
            {
                if (_map.ContainsKey(key)) return;
                _map[key] = new Node(_matrix._info, key);
                _cacheIsDirty = true;
            }

            void RemoveNode(Vector2Int key)
            {
                if (!_map.ContainsKey(key)) return;
                _map.Remove(key);
                _cacheIsDirty = true;
            }

            void Clean()
            {
                if (_map == null)
                    _map = new Dictionary<Vector2Int, Node>();

                // << REMOVE OUT OF BOUNDS NODES >>
                foreach (var key in new List<Vector2Int>(_map.Keys))
                {
                    if (!_info.IsKeyInBounds(key))
                    {
                        RemoveNode(key);
                    }
                }

                // << ADD NEW NODES >>
                for (int x = 0; x < _info.MatrixColumnCount; x++)
                {
                    for (int y = 0; y < _info.MatrixRowCount; y++)
                    {
                        AddNode(new Vector2Int(x, y));
                    }
                }
            }

            void UpdateCache()
            {
                if (_cacheIsDirty || _cachedKeys == null || _cachedNodes == null)
                {
                    _cachedKeys = new List<Vector2Int>(_map.Keys);
                    _cachedNodes = new List<Node>(_map.Values);
                    _cacheIsDirty = false;
                }
            }
            #endregion

            public Node GetNode(Vector2Int key)
            {
                _map.TryGetValue(key, out var node);
                return node;
            }

            public List<Node> GetNodes(List<Vector2Int> keys)
            {
                List<Node> nodes = new List<Node>(keys.Count);
                foreach (Vector2Int key in keys)
                {
                    if (_map.TryGetValue(key, out var node))
                    {
                        nodes.Add(node);
                    }
                }
                return nodes;
            }

            public void Refresh()
            {
                _info.Validate();

                Clean();
                UpdateCache();
            }

            #region < PUBLIC_CLASS > [[ Context Preset ]] ================================================================ 
            public class MatrixContextPreset : ScriptableData<Info>
            {
                public override void SetData(Info data)
                {
                    base.SetData(data);
                }

                public override Info ToData()
                {
                    return data;
                }
            }
            #endregion

        }
    }
}