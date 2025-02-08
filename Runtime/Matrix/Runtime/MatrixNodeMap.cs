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
        public class NodeMap
        {
            MatrixInfo _info;

            Dictionary<Vector2Int, Node> _nodeKeyLookup = new Dictionary<Vector2Int, Node>();
            Dictionary<Vector3, Node> _nodeWorldLookup = new Dictionary<Vector3, Node>();
            Dictionary<int, HashSet<Vector2Int>> _spatialPartitions =
                new Dictionary<int, HashSet<Vector2Int>>();
            Dictionary<int, Vector3> _partitionCenters = new Dictionary<int, Vector3>();

            bool _cacheIsDirty = true;

            [SerializeField]
            List<Vector2Int> _cachedKeys;

            [SerializeField]
            List<Node> _cachedNodes;

            public List<Node> Nodes => _cachedNodes;
            public List<Node> ActiveNodes => _cachedNodes.Where(node => node.Enabled).ToList();
            public List<Node> InactiveNodes => _cachedNodes.Where(node => !node.Enabled).ToList();

            public NodeMap(MatrixInfo info)
            {
                _info = info;
                Refresh();
            }

            #region < PRIVATE_METHODS > [[ Internal Handlers ]] ================================================================
            void AddNode(Vector2Int key)
            {
                if (_nodeKeyLookup.ContainsKey(key))
                    return;
                _nodeKeyLookup[key] = new Node(_info, key);
                Node node = _nodeKeyLookup[key];
                _nodeWorldLookup[node.Position] = node;
                AddKeyToPartition(key, node.Partition);
                _cacheIsDirty = true;
            }

            void AddKeyToPartition(Vector2Int key, int partitionKey)
            {
                if (_spatialPartitions == null)
                    _spatialPartitions = new Dictionary<int, HashSet<Vector2Int>>();
                if (_partitionCenters == null)
                    _partitionCenters = new Dictionary<int, Vector3>();

                // Add the partition if it doesn't exist
                if (_spatialPartitions.ContainsKey(partitionKey) == false)
                    _spatialPartitions[partitionKey] = new HashSet<Vector2Int>();
                // Return if the key already exists in the partition
                else if (_spatialPartitions[partitionKey].Contains(key))
                    return;

                // Add the key to the partition
                _spatialPartitions[partitionKey].Add(key);
                _partitionCenters[partitionKey] = CalculatePartitionCenter(partitionKey);
                _cacheIsDirty = true;
            }

            void RemoveNode(Vector2Int key)
            {
                if (!_nodeKeyLookup.ContainsKey(key))
                    return;
                _nodeKeyLookup.Remove(key);
                _cacheIsDirty = true;
            }

            void Clean()
            {
                if (_info == null)
                    return;

                if (_nodeKeyLookup == null)
                    _nodeKeyLookup = new Dictionary<Vector2Int, Node>();

                if (_nodeWorldLookup == null)
                    _nodeWorldLookup = new Dictionary<Vector3, Node>();

                // << REMOVE OUT OF BOUNDS NODES >>
                foreach (var key in new List<Vector2Int>(_nodeKeyLookup.Keys))
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
                    _cachedKeys = new List<Vector2Int>(_nodeKeyLookup.Keys);
                    _cachedNodes = new List<Node>(_nodeKeyLookup.Values);
                    _cacheIsDirty = false;
                }
            }

            public void Refresh()
            {
                Clean();
                UpdateCache();
            }
            #endregion


            public Node GetNodeByKey(Vector2Int key)
            {
                _nodeKeyLookup.TryGetValue(key, out var node);
                return node;
            }

            public Node GetNodeByCoordinate(Vector2Int coordinate)
            {
                Vector2Int key = ConvertCoordinateToKey(coordinate, _info);
                return GetNodeByKey(key);
            }

            public List<Node> GetNodes(List<Vector2Int> keys)
            {
                List<Node> nodes = new List<Node>(keys.Count);
                foreach (Vector2Int key in keys)
                {
                    if (_nodeKeyLookup.TryGetValue(key, out var node))
                    {
                        nodes.Add(node);
                    }
                }
                return nodes;
            }

            /// <summary>
            /// Gets all valid neighboring nodes for a given node key.
            /// </summary>
            /// <param name="nodeKey">The key of the node to get neighbors for</param>
            /// <returns>List of neighboring nodes</returns>
            public List<Node> GetNeighboringNodes(Vector2Int nodeKey)
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

                        if (neighbor != null && neighbor.Enabled)
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
            public Node GetClosestNodeToPosition(Vector3 position)
            {
                if (_cachedNodes == null || _cachedNodes.Count == 0)
                    return null;

                // Find the closest partition to our adjusted position
                int closestPartitionKey = GetClosestPartitionToPosition(position);
                if (closestPartitionKey == -1)
                    return null;

                Node closestNode = null;
                float closestDistance = float.MaxValue;
                HashSet<Node> checkedNodes = new HashSet<Node>();

                // Check nodes in the closest partition
                if (_spatialPartitions.TryGetValue(closestPartitionKey, out var partitionNodes))
                {
                    foreach (var key in partitionNodes)
                    {
                        Node node = GetNodeByKey(key);
                        if (node == null || checkedNodes.Contains(node))
                            continue;

                        checkedNodes.Add(node);
                        float distance = Vector3.Distance(position, node.Position);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestNode = node;
                        }
                    }
                }

                // If we found a node, check its neighbors
                if (closestNode != null)
                {
                    var neighbors = GetNeighboringNodes(closestNode.Key);
                    foreach (var neighbor in neighbors)
                    {
                        if (checkedNodes.Contains(neighbor))
                            continue;

                        checkedNodes.Add(neighbor);
                        float distance = Vector3.Distance(position, neighbor.Position);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestNode = neighbor;
                        }
                    }
                }

                return closestNode;
            }

            /// <summary>
            /// Checks if a node is at the edge of its partition.
            /// </summary>
            private bool IsAtPartitionEdge(Vector2Int key, int partitionSize)
            {
                return key.x % partitionSize == 0
                    || key.y % partitionSize == 0
                    || key.x % partitionSize == partitionSize - 1
                    || key.y % partitionSize == partitionSize - 1;
            }

            /// <summary>
            /// Gets the partition keys for neighboring partitions.
            /// </summary>
            private IEnumerable<int> GetNeighboringPartitions(int partitionKey)
            {
                // Convert partition key to 2D coordinates
                int partitionSize = _info.PartitionSize;
                int x = partitionKey / partitionSize;
                int y = partitionKey % partitionSize;

                // Check all 8 neighboring partitions
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int newX = x + dx;
                        int newY = y + dy;

                        // Calculate new partition key
                        int neighborKey = (newX * partitionSize) + newY;
                        yield return neighborKey;
                    }
                }
            }

            public Dictionary<int, HashSet<Vector2Int>> GetPartitions()
            {
                return _spatialPartitions;
            }

            /// <summary>
            /// Finds the closest partition to a given world position.
            /// </summary>
            /// <param name="position">The world position to check against</param>
            /// <returns>The key of the closest partition, or -1 if no partitions exist</returns>
            public int GetClosestPartitionToPosition(Vector3 position)
            {
                if (_partitionCenters == null || _partitionCenters.Count == 0)
                    return -1;

                float closestDistance = float.MaxValue;
                int closestPartition = -1;

                foreach (var kvp in _partitionCenters)
                {
                    float distance = Vector3.Distance(position, kvp.Value);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPartition = kvp.Key;
                    }
                }
                return closestPartition;
            }

            /// <summary>
            /// Calculates the center position of a partition based on its nodes.
            /// </summary>
            /// <param name="partitionKey">The key of the partition to calculate the center for</param>
            /// <returns>The center position of the partition in world space</returns>
            private Vector3 CalculatePartitionCenter(int partitionKey)
            {
                if (
                    !_spatialPartitions.TryGetValue(partitionKey, out var partitionNodes)
                    || partitionNodes.Count == 0
                )
                {
                    return Vector3.zero;
                }

                // Get bounds of partition
                Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
                Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

                foreach (Vector2Int key in partitionNodes)
                {
                    min.x = Mathf.Min(min.x, key.x);
                    min.y = Mathf.Min(min.y, key.y);
                    max.x = Mathf.Max(max.x, key.x);
                    max.y = Mathf.Max(max.y, key.y);
                }

                // Calculate center key
                Vector2Int centerKey = new Vector2Int(
                    min.x + (max.x - min.x) / 2,
                    min.y + (max.y - min.y) / 2
                );

                // Get the node at center key
                Node centerNode = GetNodeByKey(centerKey);
                if (centerNode != null)
                {
                    return centerNode.Position;
                }

                // If center node doesn't exist, average all node positions
                Vector3 sum = Vector3.zero;
                int count = 0;

                foreach (Vector2Int key in partitionNodes)
                {
                    Node node = GetNodeByKey(key);
                    if (node != null)
                    {
                        sum += node.Position;
                        count++;
                    }
                }

                return count > 0 ? sum / count : Vector3.zero;
            }

            /// <summary>
            /// Gets the center position of a partition.
            /// </summary>
            /// <param name="partitionKey">The key of the partition</param>
            /// <returns>The cached center position of the partition</returns>
            public Vector3 GetPartitionCenter(int partitionKey)
            {
                return _partitionCenters.TryGetValue(partitionKey, out Vector3 center)
                    ? center
                    : Vector3.zero;
            }

#if UNITY_EDITOR
            /// <summary>
            /// Debug method to visualize partition centers.
            /// </summary>
            public void DrawPartitionCenters()
            {
                foreach (var kvp in _partitionCenters)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(kvp.Value, 0.2f);

                    // Draw connections to partition nodes
                    if (_spatialPartitions.TryGetValue(kvp.Key, out var nodes))
                    {
                        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // Semi-transparent yellow
                        foreach (var nodeKey in nodes)
                        {
                            Node node = GetNodeByKey(nodeKey);
                            if (node != null)
                            {
                                Gizmos.DrawLine(kvp.Value, node.Position);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Debug visualization of closest partition search.
            /// </summary>
            public void DrawClosestPartitionToPosition(Vector3 position)
            {
                int closestPartition = GetClosestPartitionToPosition(position);
                if (closestPartition != -1)
                {
                    // Draw line from position to partition center
                    if (_partitionCenters.TryGetValue(closestPartition, out Vector3 center))
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(position, center);
                        Gizmos.DrawWireSphere(center, 0.3f);

                        // Draw the partition bounds
                        if (_spatialPartitions.TryGetValue(closestPartition, out var nodes))
                        {
                            Gizmos.color = new Color(1f, 0f, 1f, 0.2f); // Semi-transparent magenta
                            foreach (var nodeKey in nodes)
                            {
                                Node node = GetNodeByKey(nodeKey);
                                if (node != null)
                                {
                                    Gizmos.DrawLine(center, node.Position);
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Debug visualization of closest node search.
            /// </summary>
            public void DrawClosestNodeSearch(Vector3 position)
            {
                Node closest = GetClosestNodeToPosition(position);
                if (closest != null)
                {
                    // Draw line to closest node
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(position, closest.Position);
                    Gizmos.DrawWireSphere(closest.Position, 0.2f);

                    // Draw partition boundary
                    int partitionKey = CalculateCellPartition(closest.Key, _info.PartitionSize);
                    if (_spatialPartitions.TryGetValue(partitionKey, out var nodes))
                    {
                        Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // Semi-transparent green
                        Vector3 center = GetPartitionCenter(partitionKey);
                        foreach (var nodeKey in nodes)
                        {
                            Node node = GetNodeByKey(nodeKey);
                            if (node != null)
                            {
                                Gizmos.DrawLine(center, node.Position);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Debug visualization of neighboring nodes.
            /// </summary>
            public void DrawNeighboringNodes(Node centerNode)
            {
                if (centerNode == null)
                    return;

                var neighbors = GetNeighboringNodes(centerNode.Key);

                // Draw center node
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(centerNode.Position, 0.2f);

                // Draw neighbors and connections
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                foreach (var neighbor in neighbors)
                {
                    Gizmos.DrawWireSphere(neighbor.Position, 0.15f);
                    Gizmos.DrawLine(centerNode.Position, neighbor.Position);
                }
            }
#endif
        }
    }
}
