using System;
using System.Collections.Generic;
using System.Linq;
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
        public struct Map
        {
            Matrix _matrix;
            Dictionary<Vector2Int, Node> _nodeLookup;
            Dictionary<int, Partition> _partitionLookup;
            List<Node> _cachedNodes;
            List<Partition> _cachedPartitions;
            bool _isDirty;

            public bool IsValid => _matrix != null;
            public int NodeCount => _nodeLookup.Count;
            public int PartitionCount => _partitionLookup.Count;

            public Map(Matrix matrix)
            {
                _matrix = matrix;
                _nodeLookup = new Dictionary<Vector2Int, Node>();
                _partitionLookup = new Dictionary<int, Partition>();
                _cachedNodes = new List<Node>();
                _cachedPartitions = new List<Partition>();
                _isDirty = true;

                Refresh();
            }

            #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================
            void TryCreateNode(Vector2Int key)
            {
                // << RETURN IF NODE EXISTS >>
                if (_nodeLookup.ContainsKey(key))
                    return;

                // << CREATE NEW NODE >>
                Node newNode = new Node(_matrix, key);
                _nodeLookup.Add(key, newNode);

                // << ADD KEY TO PARTITION >>
                if (_partitionLookup.TryGetValue(newNode.PartitionKey, out var partition))
                    partition.ChildNodes.Add(newNode);
                else
                    _partitionLookup[newNode.PartitionKey] = new Partition(
                        _matrix,
                        newNode.PartitionKey
                    );

                // << MARK CACHE AS DIRTY >>
                _isDirty = true;
            }

            void RemoveNode(Vector2Int key)
            {
                if (!_nodeLookup.ContainsKey(key))
                    return;

                _nodeLookup.Remove(key);
                _isDirty = true;
            }

            public void Refresh()
            {
                if (!IsValid)
                    return;

                // << CONFIRM LOOKUPS ARE INITIALIZED >>
                if (_nodeLookup == null)
                    _nodeLookup = new Dictionary<Vector2Int, Node>();
                if (_partitionLookup == null)
                    _partitionLookup = new Dictionary<int, Partition>();

                // << REMOVE OUT OF BOUNDS NODES >>
                foreach (Vector2Int key in new List<Vector2Int>(_nodeLookup.Keys))
                {
                    if (!IsKeyInBounds(key))
                    {
                        RemoveNode(key);
                        //Debug.Log($"Removed Node (OutOfBounds): {key}");
                    }
                }

                // << REMOVE EMPTY PARTITIONS >>
                foreach (Partition partition in new List<Partition>(_partitionLookup.Values))
                {
                    if (partition.ChildNodes.Count == 0)
                        _partitionLookup.Remove(partition.Key);
                }

                // << ADD NEW NODES >>
                for (int x = 0; x < _matrix.GetInfo().ColumnCount; x++)
                {
                    for (int y = 0; y < _matrix.GetInfo().RowCount; y++)
                    {
                        TryCreateNode(new Vector2Int(x, y));
                    }
                }

                // << UPDATE CACHE >>
                if (_isDirty || _cachedNodes == null || _cachedPartitions == null)
                {
                    _cachedNodes = new List<Node>(_nodeLookup.Values);
                    _cachedPartitions = new List<Partition>(_partitionLookup.Values);
                    _isDirty = false;
                }
                Debug.Log($"Map Refreshed: {NodeCount} nodes, {PartitionCount} partitions");
            }
            #endregion

            #region < PUBLIC_METHODS > [[ Get Nodes ]] ==================================================================================

            public Node GetNodeByKey(Vector2Int key)
            {
                _nodeLookup.TryGetValue(key, out var node);
                return node;
            }

            public Node GetNodeByCoordinate(Vector2Int coordinate)
            {
                Node.ConvertCoordinateToKey(_matrix.GetInfo(), coordinate, out Vector2Int key);
                return GetNodeByKey(key);
            }

            public List<Node> GetNodes(List<Vector2Int> keys)
            {
                List<Node> nodes = new List<Node>(keys.Count);
                foreach (Vector2Int key in keys)
                {
                    if (_nodeLookup.TryGetValue(key, out var node))
                    {
                        nodes.Add(node);
                    }
                }
                return nodes;
            }

            public List<Node> GetAllNodes() => _cachedNodes;

            public List<Node> GetEnabledNodes() =>
                _cachedNodes.Where(node => node.IsEnabled).ToList();

            public List<Node> GetDisabledNodes() =>
                _cachedNodes.Where(node => !node.IsEnabled).ToList();

            /// <summary>
            /// Gets all valid neighboring nodes for a given node key.
            /// </summary>
            /// <param name="nodeKey">The key of the node to get neighbors for</param>
            /// <returns>List of neighboring nodes</returns>
            public List<Node> GetNodeNeighbors(Vector2Int nodeKey)
            {
                List<Node> neighbors = new List<Node>();

                // Check all 8 surrounding positions
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        // Skip the center node
                        if (x == 0 && y == 0)
                            continue;

                        Vector2Int neighborKey = nodeKey + new Vector2Int(x, y);
                        Node neighbor = GetNodeByKey(neighborKey);

                        if (neighbor.IsValid && neighbor.IsEnabled)
                        {
                            neighbors.Add(neighbor);
                        }
                    }
                }

                return neighbors;
            }

            /// <summary>
            /// Finds the closest node to a given world position.
            /// </summary>
            /// <param name="position">The world position to check against</param>
            /// <returns>The closest node, or null if no nodes are found</returns>
            public bool GetClosestNodeToPosition(Vector3 position, out Node? closestNode)
            {
                closestNode = null;

                if (_cachedNodes == null || _cachedNodes.Count == 0)
                {
                    closestNode = null;
                    return false;
                }

                // Find the closest partition to our adjusted position
                Partition closestPartition = GetClosestPartitionToPosition(position);
                if (closestPartition == null)
                {
                    closestNode = null;
                    return false;
                }

                float closestDistance = float.MaxValue;
                HashSet<Node> checkedNodes = new HashSet<Node>();

                // Check nodes in the closest partition
                if (closestPartition != null)
                {
                    foreach (Node nextNode in closestPartition.ChildNodes)
                    {
                        if (!nextNode.IsValid || checkedNodes.Contains(nextNode))
                            continue;

                        checkedNodes.Add(nextNode);
                        float distance = Vector3.Distance(position, nextNode.Center);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestNode = nextNode;
                        }
                    }
                }

                // If we found a node, check its neighbors
                if (closestNode != null)
                {
                    List<Node> neighbors = GetNodeNeighbors(closestNode.Value.Key);
                    foreach (Node neighbor in neighbors)
                    {
                        if (checkedNodes.Contains(neighbor))
                            continue;

                        checkedNodes.Add(neighbor);
                        float distance = Vector3.Distance(position, neighbor.Center);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestNode = neighbor;
                        }
                    }
                }

                return closestNode != null;
            }
            #endregion


            #region < PUBLIC_METHODS > [[ Check Nodes ]] =====================================================================================
            public bool IsKeyInBounds(Vector2Int key)
            {
                return key.x >= 0
                    && key.x <= _matrix.GetInfo().TerminalKey.x
                    && key.y >= 0
                    && key.y <= _matrix.GetInfo().TerminalKey.y;
            }

            #endregion

            #region < PUBLIC_METHODS > [[ Get Partitions ]] ==================================================================================
            public Partition GetPartition(int key)
            {
                return _partitionLookup.TryGetValue(key, out var partition) ? partition : null;
            }

            public List<Partition> GetAllPartitions() => _cachedPartitions;

            /// <summary>
            /// Finds the closest partition to a given world position.
            /// </summary>
            /// <param name="position">The world position to check against</param>
            /// <returns>The key of the closest partition, or -1 if no partitions exist</returns>

            public Partition GetClosestPartitionToPosition(Vector3 position)
            {
                float closestDistance = float.MaxValue;
                Partition closestPartition = null;

                foreach (Partition partition in _cachedPartitions)
                {
                    if (partition.CenterWorldPosition == Vector3.zero)
                        continue;

                    float distance = Vector3.Distance(position, partition.CenterWorldPosition);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPartition = partition;
                    }
                }

                return closestPartition;
            }
            #endregion
        }
    }
}
