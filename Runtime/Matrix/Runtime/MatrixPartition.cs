using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Matrix;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        public class Partition : IVisitable<Partition>
        {
            Matrix _matrix = null;
            int _key = -1;

            public int Key => _key;
            public Vector3 CenterWorldPosition => CalculateCenterPosition();
            public Vector2Int CenterKey => CalculateCenterKey();
            public Vector2 Dimensions => CalculateDimensions();
            public HashSet<Node> ChildNodes { get; private set; } = new HashSet<Node>();

            public bool IsValid => _matrix != null && _key >= 0;
            public bool IsEmpty => ChildNodes.Count == 0;

            public Partition(Matrix matrix, int key)
            {
                _matrix = matrix;
                _key = key;
            }

            /// <summary>
            /// Calculates the center position of a partition based on its nodes.
            /// </summary>
            /// <param name="partitionKey">The key of the partition to calculate the center for</param>
            /// <returns>The center position of the partition in world space</returns>
            private Vector3 CalculateCenterPosition()
            {
                // Get bounds of partition
                Vector3 min = Vector3.one * float.MaxValue;
                Vector3 max = Vector3.one * float.MinValue;

                foreach (Node node in ChildNodes)
                {
                    min.x = Mathf.Min(min.x, node.Position.x);
                    min.y = Mathf.Min(min.y, node.Position.y);
                    max.x = Mathf.Max(max.x, node.Position.x);
                    max.y = Mathf.Max(max.y, node.Position.y);
                }

                return (min + max) * 0.5f;
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

            private Vector2Int CalculateCenterKey()
            {
                Vector2Int estimatedCenter = new Vector2Int(
                    Mathf.RoundToInt(CenterWorldPosition.x),
                    Mathf.RoundToInt(CenterWorldPosition.y)
                );

                Vector2Int closestKey = estimatedCenter;
                float closestDistance = int.MaxValue;

                foreach (Node node in ChildNodes)
                {
                    if (node.Key == estimatedCenter)
                    {
                        return node.Key;
                    }
                    else
                    {
                        float distanceToEstimatedCenter = Vector2Int.Distance(
                            node.Key,
                            estimatedCenter
                        );
                        if (distanceToEstimatedCenter < closestDistance)
                        {
                            closestDistance = distanceToEstimatedCenter;
                            closestKey = node.Key;
                        }
                    }
                }

                return closestKey;
            }

            private Vector2 CalculateDimensions()
            {
                Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
                Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

                foreach (Node node in ChildNodes)
                {
                    min.x = Mathf.Min(min.x, node.Key.x);
                    min.y = Mathf.Min(min.y, node.Key.y);
                    max.x = Mathf.Max(max.x, node.Key.x);
                    max.y = Mathf.Max(max.y, node.Key.y);
                }

                return new Vector2(max.x - min.x, max.y - min.y);
            }

            #region < PRIVATE_METHOD > : IVisitable ========================================================================================
            public void AcceptVisitor(IVisitor<Partition> visitor)
            {
                visitor.Visit(this);
            }
            #endregion

#if UNITY_EDITOR
            public static class GUI
            {
                public static void DrawSceneGUI(Matrix matrix, Partition partition)
                {
                    if (partition == null)
                        return;

                    Color partitionColor = GenerateColorFromPartitionKey(partition.Key);
                    CustomGizmos.DrawWireRect(
                        partition.CenterWorldPosition,
                        partition.Dimensions,
                        matrix.GetInfo().Rotation,
                        partitionColor
                    );
                }

                public static Color GenerateColorFromPartitionKey(int partitionKey)
                {
                    // MurmurHash3-inspired mixing for better distribution
                    uint h = (uint)partitionKey;
                    h ^= h >> 16;
                    h *= 0x85ebca6b;
                    h ^= h >> 13;
                    h *= 0xc2b2ae35;
                    h ^= h >> 16;

                    // Use different prime multipliers for each component
                    float hue = (h * 0xcc9e2d51) % 360f / 360f;
                    float saturation = 0.6f + (((h * 0x1b873593) % 40f) / 100f); // Range 0.6-1.0
                    float value = 0.8f + (((h * 0xe6546b64) % 20f) / 100f); // Range 0.8-1.0

                    return Color.HSVToRGB(hue, saturation, value);
                }

                /// <summary>
                /// Draws the partition list in the inspector
                /// </summary>
                public static void DrawPartitionList(
                    Matrix matrix,
                    ref Vector2 partitionScrollPosition,
                    Dictionary<int, bool> partitionFoldouts,
                    System.Action<Matrix.Node> onNodeSelected,
                    System.Action<Matrix.Node> onSceneViewFocus
                )
                {
                    if (matrix == null)
                        return;

                    using (
                        var scrollView = new EditorGUILayout.ScrollViewScope(
                            partitionScrollPosition
                        )
                    )
                    {
                        partitionScrollPosition = scrollView.scrollPosition;

                        var partitions = matrix.GetMap().GetAllPartitions();
                        if (partitions.Count == 0)
                        {
                            EditorGUILayout.HelpBox("No partitions found.", MessageType.Info);
                            return;
                        }

                        foreach (var partition in partitions)
                        {
                            // Ensure the partition has a foldout state
                            if (!partitionFoldouts.ContainsKey(partition.Key))
                            {
                                partitionFoldouts[partition.Key] = false;
                            }

                            // Draw partition header with node count
                            EditorGUILayout.BeginHorizontal("box");
                            {
                                Color partitionColor = GenerateColorFromPartitionKey(partition.Key);
                                EditorGUI.DrawRect(
                                    GUILayoutUtility.GetRect(16, 16),
                                    partitionColor
                                );

                                partitionFoldouts[partition.Key] = EditorGUILayout.Foldout(
                                    partitionFoldouts[partition.Key],
                                    $"Partition {partition.Key} ({partition.ChildNodes.Count} nodes)",
                                    true
                                );
                            }
                            EditorGUILayout.EndHorizontal();

                            // Draw partition contents if expanded
                            if (partitionFoldouts[partition.Key])
                            {
                                /*
                                EditorGUI.indentLevel++;
                                foreach (var node in partition.ChildNodes)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        // Indent the content
        
                                        GUILayout.Space(20);
        
                                        var node = matrix.GetMap().GetNodeByKey(node.Key);
                                        if (node != null)
                                        {
                                            // Draw node info
                                            EditorGUILayout.LabelField($"Key: {nodeKey}");
        
                                            // Add a select button
                                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                                            {
                                                onNodeSelected?.Invoke(node);
                                                onSceneViewFocus?.Invoke(node);
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUI.indentLevel--;
                                */
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}
