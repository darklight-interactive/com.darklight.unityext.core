using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Behaviour;
using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    /// <summary>
    /// Implements A* pathfinding for Matrix nodes using the base AStarPathfinder implementation.
    /// </summary>
    [RequireComponent(typeof(Matrix))]
    public class MatrixPathfinder : MonoBehaviour
    {
        Matrix _matrix;
        AStarPathfinder _pathfinder;

        public List<Matrix.Node> FindPath(
            List<Matrix.Node> nodes,
            Matrix.Node startNode,
            Matrix.Node endNode
        )
        {
            _matrix = GetComponent<Matrix>();
            List<Vector2Int> validKeys = nodes.Select(node => node.Key).ToList();
            _pathfinder = new AStarPathfinder(validKeys);

            List<Vector2Int> path = _pathfinder.FindPath(startNode.Key, endNode.Key);
            return _matrix.Map.GetNodes(path);
        }
    }
}
