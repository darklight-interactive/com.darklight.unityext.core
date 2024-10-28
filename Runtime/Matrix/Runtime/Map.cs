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
        Dictionary<Vector2Int, Node> _map = new Dictionary<Vector2Int, Node>();
        bool _keysCacheDirty = true;
        bool _nodesCacheDirty = true;

        [SerializeField] MapInfo _info;
        [SerializeField] List<Vector2Int> _cachedKeys;
        [SerializeField] List<Node> _cachedNodes;


        public MapInfo Info => _info;
        public List<Vector2Int> Keys
        {
            get
            {
                if (_keysCacheDirty || _cachedKeys == null)
                {
                    _cachedKeys = new List<Vector2Int>(_map.Keys);
                    _keysCacheDirty = false;
                }
                return _cachedKeys;
            }
        }
        public List<Node> Nodes
        {
            get
            {
                if (_nodesCacheDirty || _cachedNodes == null)
                {
                    _cachedNodes = new List<Node>(_map.Values);
                    _nodesCacheDirty = false;
                }
                return _cachedNodes;
            }
        }

        public Action OnUpdate;

        public Map()
        {
            _info = MapInfo.GetDefault();
            Refresh();
        }

        #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================ 
        void AddNode(Vector2Int key)
        {
            if (_map.ContainsKey(key)) return;
            _map[key] = new Node(_info, key);
            _keysCacheDirty = true;
        }

        void RemoveNode(Vector2Int key)
        {
            if (!_map.ContainsKey(key)) return;
            _map.Remove(key);
            _keysCacheDirty = true;
        }

        void Clean()
        {
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
            Clean();
            OnUpdate?.Invoke();
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