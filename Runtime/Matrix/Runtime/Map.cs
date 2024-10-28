using System;
using System.Collections.Generic;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    [System.Serializable]
    public class Map
    {
        Matrix _matrix;
        Dictionary<Vector2Int, Node> _map = new Dictionary<Vector2Int, Node>();
        bool _cacheIsDirty = true;

        [SerializeField, AllowNesting] MapInfo _info;
        [SerializeField] List<Vector2Int> _cachedKeys;
        [SerializeField] List<Node> _cachedNodes;


        public MapInfo Info => _info;
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

        public Map(Matrix matrix)
        {
            _matrix = matrix;
            _info = MapInfo.GetDefault(_matrix.transform);
            Refresh();
        }

        #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================ 
        void AddNode(Vector2Int key)
        {
            if (_map.ContainsKey(key)) return;
            _map[key] = new Node(_info, key);
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
            for (int x = 0; x < _info.NumColumns; x++)
            {
                for (int y = 0; y < _info.NumRows; y++)
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
        public class MatrixContextPreset : ScriptableData<MapInfo>
        {
            public override void SetData(MapInfo data)
            {
                base.SetData(data);
            }

            public override MapInfo ToData()
            {
                return data;
            }
        }
        #endregion

    }
}