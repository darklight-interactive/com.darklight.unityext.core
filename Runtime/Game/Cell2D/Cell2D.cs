using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    public interface ICell2D
    {
        /// <summary>
        /// Initializes the cell with the given grid and key.
        /// </summary>
        void Initialize(AbstractGrid2D grid, Vector2Int key);

        /// <summary>
        /// Refresh the cell data to match the current grid configuration.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Draws the gizmos for the cell.
        /// </summary>
        void DrawGizmos();
    }

    /// <summary>
    /// Definition of the Grid2D_CellData class. This class is used by the Grid2D class to store the data for each grid cell.
    /// </summary>
    [System.Serializable]
    public class Cell2D : ICell2D
    {
        #region -- << CLASS >> : DATA ------------------------------------ >>
        [System.Serializable]
        public class Data
        {
            [SerializeField, ShowOnly] string _name = "DefaultCell"; // Name of the cell
            [SerializeField, ShowOnly] Vector2Int _key; // The position key of the cell in the grid
            public string name => _name;
            public Vector2Int key => _key;

            // -- Dimensions ---- >>
            [SerializeField] Vector2 _dimensions = Vector2.one; // Dimensions of the cell
            public Vector2 dimensions { get => _dimensions; set => _dimensions = value; }

            // -- Transform ---- >>
            [SerializeField] Vector3 _position = Vector3.zero; // World position of the cell
            [SerializeField] Vector3 _normal = Vector3.up; // Normal direction of the cell
            public Vector3 position { get => _position; set => _position = value; }
            public Vector3 normal { get => _normal; set => _normal = value; }

            public Data() { }
            public Data(Vector2Int key)
            {
                _key = key;
                _name = $"Cell2D {key}";
            }

            public Tuple<Vector3, Vector3, Vector3, Color> GetGizmoData()
            {
                return new Tuple<Vector3, Vector3, Vector3, Color>(_position, _dimensions, _normal, Color.white);
            }
        }
        #endregion

        // -- References ---- >>
        AbstractGrid2D _gridParent; // Reference to the parent grid
        Data _data; // Data object for the cell

        // -- States ---- >>
        [SerializeField, ShowOnly] bool _disabled = false; // Is the cell active or not
        public bool disabled { get => _disabled; set => _disabled = value; }

        // ===================== [[ CONSTRUCTORS ]] ===================== //
        public Cell2D() { }
        public Cell2D(AbstractGrid2D grid, Vector2Int key) => Initialize(grid, key);
        public virtual void Initialize(AbstractGrid2D grid, Vector2Int key)
        {
            _gridParent = grid;
            _data = new Data(key);
            _data.position = CalculateWorldPosition();
            _data.normal = GetNormal();
            _data.dimensions = _gridParent.config.cellDimensions;
        }

        public virtual void Refresh() { }

        /// <summary>
        /// Internal method to calculate the world position of the cell based on its key and the grid configuration.
        /// </summary>
        /// <returns></returns>
        Vector3 CalculateWorldPosition()
        {
            Vector2Int key = _data.key;
            AbstractGrid2D.Config gridConfig = _gridParent.config;

            // Start with the grid's origin position in world space
            Vector3 basePosition = gridConfig.position;

            // Calculate the offset for the grid origin key
            Vector2 originOffset = (Vector2)gridConfig.originOffset * gridConfig.cellDimensions * -1;

            // Calculate the offset for the current key position
            Vector2 keyOffset = (Vector2)key * gridConfig.cellDimensions;

            // Calculate the spacing offset && clamp it to avoid overlapping cells
            Vector2 spacingOffset = gridConfig.cellSpacing;
            spacingOffset.x = Mathf.Clamp(spacingOffset.x, 1, float.MaxValue);
            spacingOffset.y = Mathf.Clamp(spacingOffset.y, 1, float.MaxValue);

            // Combine origin offset and key offset, then apply spacing
            Vector2 gridOffset = (originOffset + keyOffset) * spacingOffset;

            // Create a rotation matrix based on the grid's direction
            Quaternion rotation = Quaternion.LookRotation(gridConfig.normal, Vector3.up);

            // Apply the rotation to the grid offset to get the final world offset
            Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

            // Combine the base position with the calculated world offset
            return basePosition + worldOffset;
        }

        /// <summary>
        /// Internal method to get the normal direction of the grid.
        /// </summary>
        /// <returns></returns>
        Vector3 GetNormal() => _gridParent.config.normal;

#if UNITY_EDITOR
        public virtual void DrawGizmos()
        {
            // Deconstruct tuple to get the gizmo data
            var (position, dimensions, normal, color) = _data.GetGizmoData();

            // Draw the cell square
            CustomGizmos.DrawWireRect(position, dimensions, normal, color);

            string label = $"{_data.key}\n{(_disabled ? "Disabled" : "Enabled")}";
            CustomGizmos.DrawLabel(label, position, CustomGUIStyles.CenteredStyle);

            /*
            if (editMode)
            {
                float size = Mathf.Min(dimensions.x, dimensions.y);
                size *= 0.75f;

                // Draw the button handle only if the grid is in edit mode
                CustomGizmos.DrawButtonHandle(position, size, normal, color, () =>
                {
                    disabled = !disabled;
                }, Handles.RectangleHandleCap);
            }
            */
        }
#endif
    }
}