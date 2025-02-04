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
        Vector2Int[] _validNodes;

        public List<MatrixNode> FindPath(MatrixNode startNode, MatrixNode endNode)
        {
            _matrix = GetComponent<Matrix>();
            _validNodes = _matrix.Map.Nodes.Select(node => node.Key).ToArray();
            _pathfinder = new AStarPathfinder(_validNodes);

            List<Vector2Int> path = _pathfinder.FindPath(startNode.Key, endNode.Key);
            return _matrix.Map.GetNodes(path);
        }
    }
}
