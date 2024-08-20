using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    /// <summary>
    /// Definition of the Grid2D_CellData class. This class is used by the Grid2D class to store the data for each grid cell.
    /// </summary>
    [System.Serializable]
    public class Cell2D
    {
        // -- DATA -- >>
        Grid2D _grid2D; // The parent grid object

        // -- STATES -- >>
        [SerializeField, ShowOnly] bool _initialized; // Whether or not the data has been initialized
        [SerializeField, ShowOnly] bool _disabled; // Whether or not the data should be used

        // -- IDENTIFIERS -- >>
        [SerializeField, ShowOnly] Vector2Int _key; // The position key of the data in the grid

        // -- DIMENSIONS -- >>
        public Vector2 dimensions => new Vector2(_grid2D.settings.cellWidth, _grid2D.settings.cellHeight);

        // -- REFERENCES -- >>
        public Vector3 position => CalculateWorldPosition();
        public Vector3 direction => _grid2D.settings.gridDirection;

        /// <summary>
        /// Calculates the world space position of the specified position key in the grid.
        /// </summary>
        /// <param name="key">The position key in the grid.</param>
        /// <returns>The world space position of the specified position key.</returns>
        Vector3 CalculateWorldPosition()
        {
            // Start with the grid's origin position in world space
            Vector3 basePosition = _grid2D.transform.position;

            // Calculate the offset for the grid origin key
            Vector2 originKeyOffset = (Vector2)_grid2D.settings.gridOriginKey * dimensions * -1;

            // Calculate the offset for the current key position
            Vector2 keyOffset = (Vector2)_key * dimensions;

            // Combine origin offset and key offset
            Vector2 gridOffset = (originKeyOffset + keyOffset) * _grid2D.settings.gridSpacing;

            // Create a rotation matrix based on the grid's direction
            Quaternion rotation = Quaternion.LookRotation(_grid2D.settings.gridDirection, Vector3.up);

            // Apply the rotation to the grid offset to get the final world offset
            Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

            // Combine the base position with the calculated world offset
            return basePosition + worldOffset;
        }



        /// <summary>
        /// Event to notify listeners of a data state change.
        /// </summary>
        public event Action<Cell2D> OnDataStateChanged;

        public Cell2D(Grid2D grid2D, Vector2Int gridKey)
        {
            _grid2D = grid2D;
            _key = gridKey;

            Grid2DSettings settings = _grid2D.settings;

            _initialized = true;
            _disabled = false;
        }


#if UNITY_EDITOR
        public virtual void DrawGizmos()
        {
            Color color = Color.white;
            Vector3 direction = _grid2D.settings.gridDirection;

            // Draw the cell square
            CustomGizmos.DrawWireRect(position, dimensions, direction, color);

            string label = $"{_key}";
            CustomGizmos.DrawLabel(label, position, new GUIStyle() { normal = new GUIStyleState() { textColor = color } });
        }
#endif

    }
}
