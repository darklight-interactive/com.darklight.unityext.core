using UnityEngine;
using Darklight.UnityExt.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    /*
    /// <summary>
    /// A 2D Grid that stores Overlap_Grid2DData objects. 
    /// </summary>
    [ExecuteAlways]
    public class OverlapGrid2D : Grid2D
    {

        /// <summary>
        /// Create and stores the data from a Physics2D.OverlapBoxAll call at the world position of the Grid2DData. 
        /// </summary>
        public class OverlapCell : Grid2D.BasicCell
        {
            private bool disabledInitially = false;
            public LayerMask layerMask; // The layer mask to use for the OverlapBoxAll called
            public Collider2D[] colliders = new Collider2D[0]; /// The colliders found by the OverlapBoxAll call

            public OverlapCell(Grid2D grid, Vector2Int key) : base(grid, key)
            {

            }



            public void Initialize(Vector2Int positionKey, Vector3 worldPosition, float coordinateSize, LayerMask layerMask)
            {
                base.Initialize(positionKey, disabled, weight, worldPosition, coordinateSize);
                this.layerMask = layerMask;
                this.disabledInitially = disabled; // << set equal to initial value
            }

            public override void CycleDataState()
            {
                base.CycleDataState();
                this.disabledInitially = disabled; // << set to match the new state
            }

            public override void UpdateData()
            {
                // Update the collider data
                this.colliders = Physics2D.OverlapBoxAll(worldPosition, Vector2.one * _cellWidth, 0, layerMask);
                if (!disabledInitially)
                {
                    this._disabled = colliders.Length > 0;
                }
            }
        }

        [SerializeField,
        Tooltip("OverlapGrid2D uses OverlapBoxAll to detect colliders in the grid. This is the layer mask used to filter which colliders are detected.")]
        private LayerMask layerMask;
        public bool editMode = false;

        public override void Awake()
        {
            base.Awake();
            InitializeDataMap();
        }

        protected override void InitializeDataMap()
        {
            if (Settings == null) return;

            cellDataMap.Clear();
            for (int x = 0; x < gridArea.x; x++)
            {
                for (int y = 0; y < gridArea.y; y++)
                {
                    Vector2Int positionKey = new Vector2Int(x, y);
                    Vector3 worldPosition = GetWorldPositionOfCell(positionKey);

                    OverlapCell newData = new OverlapCell();
                    Grid2D_SerializedData existingData = Settings.LoadData(positionKey);
                    if (existingData != null)
                    {
                        newData.Initialize(existingData, worldPosition, Settings.cellWidth);
                        newData.layerMask = layerMask;

                    }
                    else
                    {
                        newData.Initialize(positionKey, worldPosition, Settings.cellWidth, layerMask);
                    }

                    // Set the data in the map
                    if (cellDataMap.ContainsKey(positionKey))
                        cellDataMap[positionKey] = newData;
                    else
                        cellDataMap.Add(positionKey, newData);

                    // Notify listeners of the data change
                    newData.OnDataStateChanged += (data) =>
                    {
                        Settings.SaveData(data);
                    };
                }
            }
        }

        public virtual void Update()
        {
            foreach (OverlapCell data in cellDataMap.Values)
            {
                Vector3 worldPosition = GetWorldPositionOfCell(data._key);
                data.worldPosition = worldPosition;

                data.UpdateData();
            }
        }

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
            DrawOverlapGrid(grid2D, grid2D.editMode);
        }

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
        }
    }
#endif
*/

}
