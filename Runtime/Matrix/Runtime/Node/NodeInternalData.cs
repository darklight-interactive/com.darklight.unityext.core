using System;

using Darklight.UnityExt.Editor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        public partial class Node
        {
            [Serializable]
            public class InternalData
            {
                Matrix _matrix;
                [SerializeField, ShowOnly] int _guid = Guid.NewGuid().GetHashCode();
                [SerializeField, ShowOnly] Vector2Int _key = Vector2Int.zero;
                [SerializeField, ShowOnly] Vector2Int _coordinate = Vector2Int.zero;

                [Header("World Space Values")]
                [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
                [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
                [SerializeField, ShowOnly] Vector2 _dimensions = Vector2.one;

                [Header("Flags")]
                [SerializeField, ShowOnly] bool _isDisabled = false;

                // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
                public InternalData(Node node, Vector2Int key, Matrix matrix)
                {
                    _matrix = matrix;
                    _key = key;
                    Refresh();
                }

                public void Refresh()
                {
                    _matrix.CalculateNodeWorldSpaceFromKey(_key, out _position, out _coordinate, out _normal, out _dimensions);
                }


                // ======== [[ PROPERTIES ]] ======================================================= >>>>
                public Vector2Int Key { get => _key; }
                public Vector2Int Coordinate { get => _coordinate; }
                public Vector2 Dimensions { get => _dimensions; }
                public float SizeAvg { get => (_dimensions.x + _dimensions.y) / 2f; }
                public Vector3 Position { get => _position; }
                public Vector3 Normal { get => _normal; }
                public bool Disabled { get => _isDisabled; }

                // ======== [[ METHODS ]] ============================================================ >>>>

            }
        }
    }
}