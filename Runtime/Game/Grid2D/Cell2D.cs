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
        Grid2DSettings _gridSettings; // The settings of the grid
        Vector3 _gridPosition; // The position of the grid in world space

        // -- STATES -- >>
        [SerializeField, ShowOnly] bool _initialized; // Whether or not the data has been initialized
        [SerializeField, ShowOnly] bool _disabled; // Whether or not the data should be used

        // -- IDENTIFIERS -- >>
        [SerializeField, ShowOnly] Vector2Int _key; // The position key of the data in the grid

        // -- REFERENCES -- >>
        public bool disabled => _disabled;
        public Vector3 position => CalculateWorldPosition();
        public Vector3 direction => _gridSettings.gridDirection;
        public Vector2 dimensions => new Vector2(_gridSettings.cellWidth, _gridSettings.cellHeight);


        // -- EVENTS -- >>
        public Action OnEditorSimpleToggle;

        public Cell2D(Grid2DSettings settings, Vector2Int gridKey, Vector3 gridPosition)
        {
            _gridSettings = settings;
            _key = gridKey;
            _gridPosition = gridPosition;

            _initialized = true;
            _disabled = false;

            OnEditorSimpleToggle += ToggleDisabled;
        }

        public void ToggleDisabled()
        {
            _disabled = !_disabled;
        }

        public void SetDisabled(bool disabled)
        {
            _disabled = disabled;
        }


        /// <summary>
        /// Calculates the world space position of the specified position key in the grid.
        /// </summary>
        /// <param name="key">The position key in the grid.</param>
        /// <returns>The world space position of the specified position key.</returns>
        Vector3 CalculateWorldPosition()
        {
            // Start with the grid's origin position in world space
            Vector3 basePosition = _gridPosition;

            // Calculate the offset for the grid origin key
            Vector2 originKeyOffset = (Vector2)_gridSettings.gridOriginKey * dimensions * -1;

            // Calculate the offset for the current key position
            Vector2 keyOffset = (Vector2)_key * dimensions;

            // Combine origin offset and key offset
            Vector2 gridOffset = (originKeyOffset + keyOffset) * _gridSettings.gridSpacing;

            // Create a rotation matrix based on the grid's direction
            Quaternion rotation = Quaternion.LookRotation(_gridSettings.gridDirection, Vector3.up);

            // Apply the rotation to the grid offset to get the final world offset
            Vector3 worldOffset = rotation * new Vector3(gridOffset.x, gridOffset.y, 0);

            // Combine the base position with the calculated world offset
            return basePosition + worldOffset;
        }

#if UNITY_EDITOR
        public virtual Color GetColor()
        {
            return _disabled ? Color.white : Color.black;
        }

        public virtual void DrawGizmos(bool editMode = false)
        {
            // Draw the cell square
            CustomGizmos.DrawWireRect(position, dimensions, direction, GetColor());

            string label = $"{_key}";
            CustomGizmos.DrawLabel(label, position, CustomGUIStyles.CenteredStyle);


            if (editMode)
            {
                float size = Mathf.Min(dimensions.x, dimensions.y);
                size *= 0.75f;

                // Draw the button handle only if the grid is in edit mode
                CustomGizmos.DrawButtonHandle(position, size, direction, GetColor(), () =>
                {
                    OnEditorSimpleToggle?.Invoke();
                }, Handles.RectangleHandleCap);
            }
        }
#endif

    }
}
