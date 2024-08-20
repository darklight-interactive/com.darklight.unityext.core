using UnityEngine;
using Darklight.UnityExt.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    /// <summary>
    /// A 2D Grid that stores Overlap_Grid2DData objects. 
    /// </summary>
    [ExecuteAlways]
    public class OverlapGrid2D : Grid2D<OverlapCell>
    {
        [SerializeField,
        Tooltip("OverlapGrid2D uses OverlapBoxAll to detect colliders in the grid. This is the layer mask used to filter which colliders are detected.")]
        private LayerMask layerMask;

        /*
        public OverlapCell GetBestOverlapGridData()
        {
            OverlapCell bestData = cellDataMap.Values.GetEnumerator().Current;

            foreach (OverlapCell data in cellDataMap.Values)
            {
                if (bestData == null) { bestData = data; }

                if (data._disabled) continue; // Skip disabled data
                if (data.colliders.Length > 0) continue; // Skip data with colliders

                // If the data has a higher or equal weight and less colliders, set it as the best data
                if (data._weight >= bestData._weight)
                {
                    bestData = data;
                }
            }
            //Debug.Log($"{this.name} OverlapGrid2D Best Data: {bestData.positionKey} - {bestData.worldPosition}");
            return bestData;
        }
        */
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OverlapGrid2D), true)]
    public class OverlapGrid2DEditor : UnityEditor.Editor
    {
        private OverlapGrid2D grid2D;
        private SerializedProperty presetProperty;
        private void OnEnable()
        {
            grid2D = (OverlapGrid2D)target;
            grid2D.Awake();
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();


            base.OnInspectorGUI();


            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI()
        {
            grid2D = (OverlapGrid2D)target;
            //DrawOverlapGrid(grid2D, grid2D.editMode);
        }

        /*
        public static void DrawOverlapGrid(OverlapGrid2D grid2D, bool editMode = false)
        {
            Grid2D_Settings preset = grid2D.Settings;
            float cellSize = preset._grid_cellSize;

            for (int x = 0; x < preset._gridWidth; x++)
            {
                for (int y = 0; y < preset.gridSizeY; y++)
                {
                    Vector2Int positionKey = new Vector2Int(x, y);
                    Grid2D_CellData data = grid2D.GetData(positionKey);
                    if (data == null || data._initialized == false) continue; // Skip uninitialized data

                    Vector3 cellPos = grid2D.GetWorldPositionOfCell(positionKey);

                    CustomGizmos.DrawWireSquare(cellPos, preset._grid_cellSize, Vector3.forward, data.GetColor());
                    CustomGizmos.DrawLabel($"{positionKey}", cellPos, CustomGUIStyles.CenteredStyle);

                    if (editMode)
                    {
                        // Draw the button handle only if the grid is in edit mode
                        CustomGizmos.DrawButtonHandle(cellPos, cellSize * 0.75f, Vector3.forward, data.GetColor(), () =>
                        {
                            data.CycleDataState();
                        }, Handles.RectangleHandleCap);
                    }
                }
            }
        }*/
    }
#endif

}
