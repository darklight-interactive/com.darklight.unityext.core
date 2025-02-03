using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// Implements A* pathfinding algorithm for grid-based movement using integer coordinates.
    /// </summary>
    public class AStarPathfinder
    {
        private class PathNode : IComparable<PathNode>
        {
            public Vector2Int Coordinate { get; private set; }
            public float GCost { get; set; } // Cost from start to current node
            public float HCost { get; set; } // Estimated cost from current node to end
            public float FCost => GCost + HCost;
            public PathNode Parent { get; set; }

            public PathNode(Vector2Int coordinate)
            {
                Coordinate = coordinate;
            }

            public int CompareTo(PathNode other)
            {
                int compare = FCost.CompareTo(other.FCost);
                if (compare == 0)
                {
                    compare = HCost.CompareTo(other.HCost);
                }
                return compare;
            }
        }

        private List<Vector2Int> _grid;
        private Vector2Int _gridMin;
        private Vector2Int _gridMax;

        public AStarPathfinder(List<Vector2Int> grid)
        {
            _grid = grid;
            CalculateGridBounds();
        }

        private void CalculateGridBounds()
        {
            if (_grid == null || _grid.Count == 0)
            {
                _gridMin = _gridMax = Vector2Int.zero;
                return;
            }

            _gridMin = _gridMax = _grid[0];
            foreach (var pos in _grid)
            {
                _gridMin = Vector2Int.Min(_gridMin, pos);
                _gridMax = Vector2Int.Max(_gridMax, pos);
            }
        }

        private bool IsCoordinateValid(Vector2Int pos)
        {
            if (
                pos.x < _gridMin.x
                || pos.x > _gridMax.x
                || pos.y < _gridMin.y
                || pos.y > _gridMax.y
            )
            {
                return false;
            }

            return _grid.Contains(pos);
        }

        /// <summary>
        /// Finds the shortest path between start and end coordinates using A* algorithm.
        /// </summary>
        /// <param name="startPos">Starting grid position</param>
        /// <param name="endPos">Target grid position</param>
        /// <returns>List of grid positions representing the path, or null if no path exists</returns>
        public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
        {
            var openSet = new SortedSet<PathNode>();
            var closedSet = new HashSet<Vector2Int>();
            var nodeDict = new Dictionary<Vector2Int, PathNode>();

            var startNode = new PathNode(startPos);
            startNode.GCost = 0;
            startNode.HCost = CalculateHeuristic(startPos, endPos);
            openSet.Add(startNode);
            nodeDict[startPos] = startNode;

            while (openSet.Count > 0)
            {
                var currentNode = openSet.Min;
                openSet.Remove(currentNode);

                if (currentNode.Coordinate == endPos)
                {
                    return ReconstructPath(currentNode);
                }

                closedSet.Add(currentNode.Coordinate);

                foreach (var neighborPos in GetNeighbors(currentNode.Coordinate))
                {
                    if (closedSet.Contains(neighborPos))
                        continue;

                    float tentativeGCost =
                        currentNode.GCost + CalculateDistance(currentNode.Coordinate, neighborPos);

                    PathNode neighborNode;
                    if (!nodeDict.TryGetValue(neighborPos, out neighborNode))
                    {
                        neighborNode = new PathNode(neighborPos);
                        nodeDict[neighborPos] = neighborNode;
                    }
                    else if (tentativeGCost >= neighborNode.GCost)
                    {
                        continue;
                    }

                    neighborNode.Parent = currentNode;
                    neighborNode.GCost = tentativeGCost;
                    neighborNode.HCost = CalculateHeuristic(neighborPos, endPos);

                    if (!openSet.Contains(neighborNode))
                    {
                        openSet.Add(neighborNode);
                    }
                }
            }

            return null; // No path found
        }

        private float CalculateHeuristic(Vector2Int from, Vector2Int to)
        {
            // Manhattan distance for grid-based movement
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        private float CalculateDistance(Vector2Int from, Vector2Int to)
        {
            // Use 1 for orthogonal movement in grid
            return 1f;
        }

        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
        {
            var directions = new Vector2Int[]
            {
                new Vector2Int(0, 1), // Up
                new Vector2Int(1, 0), // Right
                new Vector2Int(0, -1), // Down
                new Vector2Int(-1, 0) // Left
            };

            foreach (var dir in directions)
            {
                var neighborPos = position + dir;
                if (IsCoordinateValid(neighborPos))
                {
                    yield return neighborPos;
                }
            }
        }

        private List<Vector2Int> ReconstructPath(PathNode endNode)
        {
            var path = new List<Vector2Int>();
            var currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode.Coordinate);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Converts a world position to a grid position.
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x),
                Mathf.RoundToInt(worldPosition.z)
            );
        }

        /// <summary>
        /// Converts a grid position to a world position.
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return new Vector3(gridPosition.x, 0, gridPosition.y);
        }
    }
}
