using System;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;

using NaughtyAttributes;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    [Serializable]
    public struct Context
    {
        const float MIN_NODE_DIMENSION = 0.125f;
        const float MIN_NODE_SPACING = -0.5f;
        readonly DropdownList<Vector3> _vec3directions => new DropdownList<Vector3>()
        {
            { "Up", Vector3.up },
            { "Down", Vector3.down },
            { "Left", Vector3.left },
            { "Right", Vector3.right },
            { "Forward", Vector3.forward },
            { "Back", Vector3.back },
        };

        static Context DefaultContext = new Context()
        {
            MatrixParent = null,
            MatrixPosition = Vector3.zero,
            MatrixRotation = Vector3.zero,
            MatrixNormal = Vector3.up,

            MatrixAlignment = Alignment.MiddleCenter,
            MatrixColumns = 3,
            MatrixRows = 3,

            NodeDimensions = new Vector2(1, 1),
            NodeSpacing = new Vector2(0, 0),
            NodeBonding = new Vector2(0, 0),
        };


        [SerializeField, ShowIf("HasParent"), DisableIf("HasParent"), AllowNesting] public Transform MatrixParent;

        [Space(5)]
        [SerializeField, HideIf("HasParent"), AllowNesting] public Vector3 MatrixPosition;
        [SerializeField, HideIf("HasParent"), AllowNesting] public Vector3 MatrixRotation;
        [SerializeField] public Vector3 MatrixNormal;

        [Space(5)]
        public Alignment MatrixAlignment;
        [Range(1, 25)] public int MatrixRows;
        [Range(1, 25)] public int MatrixColumns;

        [Space(5)]
        public Vector2 NodeDimensions;
        public Vector2 NodeSpacing;
        public Vector2 NodeBonding;

        public bool HasParent => MatrixParent != null;

        public Context(Transform parent = null)
        {
            this = DefaultContext;
            MatrixParent = parent;

            if (MatrixParent == null)
            {
                MatrixPosition = parent.position;
                MatrixRotation = parent.rotation.eulerAngles;
            }
        }

        public Context(Context context, Transform parent = null) : this(parent)
        {
            this = context;
            Validate();
        }

        public Context(int rows, int columns, Transform parent = null) : this(parent)
        {
            MatrixRows = rows > 0 ? rows : 1;
            MatrixColumns = columns > 0 ? rows : 1;
        }
        public Context(int rows, int columns, Alignment alignment, Transform parent = null) : this(rows, columns, parent) => MatrixAlignment = alignment;

        public bool Equals(Context other)
        {
            bool transformEquals = MatrixPosition == other.MatrixPosition
                && MatrixRotation == other.MatrixRotation;
            bool matrixEquals = MatrixAlignment == other.MatrixAlignment
                && MatrixRows == other.MatrixRows
                && MatrixColumns == other.MatrixColumns;
            bool nodeEquals = NodeDimensions == other.NodeDimensions
                && NodeSpacing == other.NodeSpacing
                && NodeBonding == other.NodeBonding;
            return transformEquals && matrixEquals && nodeEquals;
        }

        public void Validate()
        {
            if (HasParent)
            {
                MatrixPosition = MatrixParent.position;
                MatrixRotation = MatrixParent.rotation.eulerAngles;
            }

            MatrixRows = MatrixRows > 0 ? MatrixRows : 1;
            MatrixColumns = MatrixColumns > 0 ? MatrixColumns : 1;

            NodeDimensions.x = Mathf.Max(NodeDimensions.x, MIN_NODE_DIMENSION);
            NodeDimensions.y = Mathf.Max(NodeDimensions.y, MIN_NODE_DIMENSION);

            NodeSpacing.x = Mathf.Max(NodeSpacing.x, MIN_NODE_SPACING);
            NodeSpacing.y = Mathf.Max(NodeSpacing.y, MIN_NODE_SPACING);
        }

        #region ---- < PUBLIC_METHODS > ( Calculations ) --------------------------------- 

        /// <summary>
        /// Calculates the dimensions of the matrix in world space.
        /// </summary>
        /// <returns></returns>
        public Vector2 CalculateMatrixDimensions()
        {
            return new Vector2(
                MatrixColumns * NodeDimensions.x,
                MatrixRows * NodeDimensions.y
            );
        }

        /// <summary>
        /// Calculates the alignment offset of the matrix in world space,
        /// based on the alignment setting and the dimensions of the matrix.
        /// </summary>
        public Vector2 CalculateMatrixAlignmentOffset()
        {
            Vector2 matrixDimensions = CalculateMatrixDimensions() - NodeDimensions;
            Vector2 alignmentOffset = Vector2.zero;

            switch (MatrixAlignment)
            {
                case Alignment.BottomLeft: alignmentOffset = Vector2.zero; break;
                case Alignment.BottomCenter: alignmentOffset = new Vector2(-matrixDimensions.x / 2, 0); break;
                case Alignment.BottomRight: alignmentOffset = new Vector2(-matrixDimensions.x, 0); break;
                case Alignment.MiddleLeft: alignmentOffset = new Vector2(0, -matrixDimensions.y / 2); break;
                case Alignment.MiddleCenter: alignmentOffset = new Vector2(-matrixDimensions.x / 2, -matrixDimensions.y / 2); break;
                case Alignment.MiddleRight: alignmentOffset = new Vector2(-matrixDimensions.x, -matrixDimensions.y / 2); break;
                case Alignment.TopLeft: alignmentOffset = new Vector2(0, -matrixDimensions.y); break;
                case Alignment.TopCenter: alignmentOffset = new Vector2(-matrixDimensions.x / 2, -matrixDimensions.y); break;
                case Alignment.TopRight: alignmentOffset = new Vector2(-matrixDimensions.x, -matrixDimensions.y); break;
            }
            return alignmentOffset;
        }

        /// <summary>
        /// Calculates the origin node key of the matrix, based on the alignment setting.
        /// </summary>
        public Vector2Int CalculateMatrixOriginKey()
        {
            int maxRowIndex = MatrixRows - 1;
            int maxColumnIndex = MatrixColumns - 1;
            Vector2Int originKey = Vector2Int.zero;
            switch (MatrixAlignment)
            {
                case Alignment.BottomLeft:
                    originKey = new Vector2Int(0, 0);
                    break;
                case Alignment.BottomCenter:
                    originKey = new Vector2Int(Mathf.FloorToInt(maxColumnIndex / 2), 0);
                    break;
                case Alignment.BottomRight:
                    originKey = new Vector2Int(Mathf.FloorToInt(maxColumnIndex), 0);
                    break;
                case Alignment.MiddleLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(maxRowIndex / 2));
                    break;
                case Alignment.MiddleCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(maxColumnIndex / 2),
                        Mathf.FloorToInt(maxRowIndex / 2)
                    );
                    break;
                case Alignment.MiddleRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(maxColumnIndex),
                        Mathf.FloorToInt(maxRowIndex / 2)
                    );
                    break;
                case Alignment.TopLeft:
                    originKey = new Vector2Int(0, Mathf.FloorToInt(maxRowIndex));
                    break;
                case Alignment.TopCenter:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(maxColumnIndex / 2),
                        Mathf.FloorToInt(maxRowIndex)
                    );
                    break;
                case Alignment.TopRight:
                    originKey = new Vector2Int(
                        Mathf.FloorToInt(maxColumnIndex),
                        Mathf.FloorToInt(maxRowIndex)
                    );
                    break;
            }
            return originKey;
        }

        /// <summary>
        /// Calculates the world position and rotation of a node based on its key.
        /// </summary>
        public void CalculateNodeTransformFromKey(Vector2Int key, out Vector3 position, out Quaternion rotation)
        {
            // Calculate the node position offset in world space based on dimensions
            Vector2 keyOffsetPos = key * NodeDimensions;

            // Calculate the origin position offset in world space based on alignment
            Vector2 originOffset = CalculateMatrixAlignmentOffset();

            // Calculate the spacing offset and clamp to avoid overlapping cells
            Vector2 spacingOffsetPos = NodeSpacing + Vector2.one;
            spacingOffsetPos.x = Mathf.Max(spacingOffsetPos.x, 0.5f);
            spacingOffsetPos.y = Mathf.Max(spacingOffsetPos.y, 0.5f);

            // Calculate bonding offsets
            Vector2 bondingOffset = Vector2.zero;
            if (key.y % 2 == 0)
                bondingOffset.x = NodeBonding.x;
            if (key.x % 2 == 0)
                bondingOffset.y = NodeBonding.y;

            // Combine offsets and apply spacing
            Vector2 localPosition2D = keyOffsetPos + originOffset;
            localPosition2D *= spacingOffsetPos;
            localPosition2D += bondingOffset;

            // Convert the 2D local position to 3D and apply matrix rotation
            Vector3 localPosition = new Vector3(localPosition2D.x, 0, localPosition2D.y);
            Quaternion matrixRotation = Quaternion.Euler(MatrixRotation);
            Vector3 rotatedPosition = matrixRotation * localPosition;

            // Final world position by adding rotated local position to MatrixPosition
            position = MatrixPosition + rotatedPosition;

            // Apply the same rotation to each node
            rotation = matrixRotation;
        }

        /// <summary>
        /// Calculates the normal vector from the current MatrixRotation.
        /// </summary>
        public Vector3 CalculateNormal()
        {
            // Convert the Euler rotation to a Quaternion
            Quaternion rotation = Quaternion.Euler(MatrixRotation);

            // Use the rotation to transform the default "up" direction
            Vector3 normal = rotation * Vector3.up;

            return normal;
        }


        /// <summary>
        /// Calculates the node coordinate of a node based on its key.
        /// </summary>
        public Vector2Int CalculateNodeCoordinateFromKey(Vector2Int key)
        {
            Vector2Int originKey = CalculateMatrixOriginKey();
            return key - originKey;
        }
        #endregion

    }
}