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
    public class MatrixNodeMap
    {
        Matrix _matrix;
        Dictionary<Vector2Int, MatrixNode> _nodeLookup = new Dictionary<Vector2Int, MatrixNode>();
        Dictionary<int, HashSet<Vector2Int>> _spatialPartitions =
            new Dictionary<int, HashSet<Vector2Int>>();

        bool _cacheIsDirty = true;

        [SerializeField]
        List<Vector2Int> _cachedKeys;

        [SerializeField]
        List<MatrixNode> _cachedNodes;

        MatrixInfo _info => _matrix.Info;

        public List<MatrixNode> Nodes => _cachedNodes;

        public MatrixNodeMap(Matrix matrix)
        {
            _matrix = matrix;
            Refresh();
        }

        #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================
        void AddNode(Vector2Int key)
        {
            if (_nodeLookup.ContainsKey(key))
                return;
            _nodeLookup[key] = new MatrixNode(_matrix.Info, key);
            MatrixNode node = _nodeLookup[key];
            AddKeyToPartition(key, node.Partition);
            _cacheIsDirty = true;
        }

        void AddKeyToPartition(Vector2Int key, int partitionKey)
        {
            // Add the partition if it doesn't exist
            if (_spatialPartitions.ContainsKey(partitionKey) == false)
                _spatialPartitions[partitionKey] = new HashSet<Vector2Int>();
            // Return if the key already exists in the partition
            else if (_spatialPartitions[partitionKey].Contains(key))
                return;

            // Add the key to the partition
            _spatialPartitions[partitionKey].Add(key);
            _cacheIsDirty = true;
        }

        void RemoveNode(Vector2Int key)
        {
            if (!_nodeLookup.ContainsKey(key))
                return;
            _nodeLookup.Remove(key);
            _cacheIsDirty = true;
        }

        void Clean()
        {
            if (_nodeLookup == null)
                _nodeLookup = new Dictionary<Vector2Int, MatrixNode>();

            // << REMOVE OUT OF BOUNDS NODES >>
            foreach (var key in new List<Vector2Int>(_nodeLookup.Keys))
            {
                if (!_info.IsKeyInBounds(key))
                {
                    RemoveNode(key);
                }
            }

            // << ADD NEW NODES >>
            for (int x = 0; x < _info.ColumnCount; x++)
            {
                for (int y = 0; y < _info.RowCount; y++)
                {
                    AddNode(new Vector2Int(x, y));
                }
            }
        }

        void UpdateCache()
        {
            if (_cacheIsDirty || _cachedKeys == null || _cachedNodes == null)
            {
                _cachedKeys = new List<Vector2Int>(_nodeLookup.Keys);
                _cachedNodes = new List<MatrixNode>(_nodeLookup.Values);
                _cacheIsDirty = false;
            }
        }
        #endregion

        public MatrixNode GetNode(Vector2Int key)
        {
            _nodeLookup.TryGetValue(key, out var node);
            return node;
        }

        public List<MatrixNode> GetNodes(List<Vector2Int> keys)
        {
            List<MatrixNode> nodes = new List<MatrixNode>(keys.Count);
            foreach (Vector2Int key in keys)
            {
                if (_nodeLookup.TryGetValue(key, out var node))
                {
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        public Dictionary<int, HashSet<Vector2Int>> GetPartitions()
        {
            return _spatialPartitions;
        }

        public void Refresh()
        {
            _info.Validate();

            Clean();
            UpdateCache();
        }

        #region < PUBLIC_CLASS > [[ Context Preset ]] ================================================================
        public class MatrixContextPreset : ScriptableData<MatrixInfo>
        {
            public override void SetData(MatrixInfo data)
            {
                base.SetData(data);
            }

            public override MatrixInfo ToData()
            {
                return data;
            }
        }
        #endregion
    }
}
