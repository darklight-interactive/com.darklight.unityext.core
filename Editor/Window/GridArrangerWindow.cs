using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor window that arranges selected objects in a grid pattern with advanced configuration options
/// </summary>
public class GridArrangerWindow : EditorWindow
{
    private const string SettingsPath = "Assets/Editor/GridArrangerSettings.asset";

    private GridArrangerSettings settings;
    private Vector2 scrollPosition;
    private bool showAdvancedSettings = false;
    private bool previewEnabled = true;
    private List<Vector3> previewPositions = new List<Vector3>();

    private GUIStyle headerStyle;
    private GUIStyle sectionStyle;
    private GUIStyle toggleStyle;
    private Color headerColor = new Color(0.2f, 0.2f, 0.2f);
    private Color sectionColor = new Color(0.18f, 0.18f, 0.18f);
    private bool showPositionSettings = true;
    private bool showRotationSettings = true;

    private Vector3 lastHitNormal;

    private enum DistributionPlane
    {
        XZ, // Ground plane (default)
        XY, // Front plane
        YZ  // Side plane
    }

    /// <summary>
    /// Opens the Grid Arranger window
    /// </summary>
    [MenuItem("Tools/Darklight/GridArranger")]
    public static void ShowWindow()
    {
        var window = GetWindow<GridArrangerWindow>("Darklight Grid Arranger");
        window.minSize = new Vector2(725, 325); // Fixed size
        window.maxSize = new Vector2(750, 400); // Same as minSize to lock dimensions
        window.LoadOrCreateSettings();
    }

    private void LoadOrCreateSettings()
    {
        settings = AssetDatabase.LoadAssetAtPath<GridArrangerSettings>(SettingsPath);
        if (settings == null)
        {
            settings = CreateInstance<GridArrangerSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                AssetDatabase.CreateFolder("Assets", "Editor");
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnEnable()
    {
        LoadOrCreateSettings();
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChange;
        UpdatePreview(); // Initial preview
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChange;
    }

    /// <summary>
    /// Draws the editor window GUI
    /// </summary>
    private void OnGUI()
    {
        if (settings == null)
        {
            LoadOrCreateSettings();
            return;
        }

        InitializeStyles();
        EditorGUI.BeginChangeCheck();

        // Full-width header with more height
        DrawHeader("Grid Arranger");

        EditorGUILayout.Space(10); // Space after header

        // Main content layout with margins
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(15); // Left margin

            // Left side - Settings container (2/3 width)
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.Width(position.width * 0.60f))) // Adjusted width for margins
            {
                // Position Settings
                showPositionSettings = EditorGUILayout.Foldout(
                    showPositionSettings,
                    "Position Settings",
                    true,
                    toggleStyle
                );
                if (showPositionSettings)
                {
                    EditorGUI.indentLevel++;
                    DrawPositionSettings();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(5);

                // Rotation Settings
                showRotationSettings = EditorGUILayout.Foldout(
                    showRotationSettings,
                    "Rotation Settings",
                    true,
                    toggleStyle
                );
                if (showRotationSettings)
                {
                    EditorGUI.indentLevel++;
                    DrawRotationSettings();
                    EditorGUI.indentLevel--;
                }
            }

            GUILayout.Space(15); // Space between columns

            // Right side - Preview and Action (1/3 width)
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.Width(position.width * 0.27f))) // Adjusted width for margins
            {
                previewEnabled = EditorGUILayout.ToggleLeft(
                    new GUIContent("Show Preview", "Show preview of object positions in Scene view"), 
                    previewEnabled
                );

                EditorGUILayout.Space(5);

                // Arrange Objects button
                using (new EditorGUI.DisabledGroupScope(Selection.gameObjects.Length == 0))
                {
                    if (GUILayout.Button("Arrange Objects", GUILayout.Height(30)))
                    {
                        ArrangeSelectedObjects();
                    }
                }

                // Reset Settings button
                if (GUILayout.Button("Reset Settings", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog(
                        "Reset Settings",
                        "Are you sure you want to reset all settings to their default values?",
                        "Reset",
                        "Cancel"))
                    {
                        ResetSettings();
                        UpdatePreview();
                    }
                }

                EditorGUILayout.Space(5);

                // Stats Box
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var selectedObjects = Selection.gameObjects;
                    EditorGUILayout.LabelField("Statistics:", EditorStyles.boldLabel);
                    
                    EditorGUILayout.Space(2);
                    
                    // Selected Objects
                    EditorGUILayout.LabelField($"Selected Objects: {selectedObjects.Length}");
                    
                    // Grid Dimensions
                    EditorGUILayout.LabelField($"Grid Dimensions: {settings.gridSize.x}x{settings.gridSize.y}x{settings.gridSize.z}");
                    
                    // Spacing (multi-line)
                    EditorGUILayout.LabelField("Current Spacing:");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"X: {settings.spacing.x:F2} Y: {settings.spacing.y:F2} Z: {settings.spacing.z:F2}");
                    EditorGUI.indentLevel--;
                    
                    if (settings.autoCalculate)
                    {
                        EditorGUILayout.LabelField($"Spacing Multiplier: {settings.spacingMultiplier:F2}x");
                    }

                    if (settings.centerInGrid)
                    {
                        Vector3 offset = CalculateCenterOffset(selectedObjects.Length);
                        EditorGUILayout.LabelField("Center Offset:");
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"X: {offset.x:F1}");
                        EditorGUILayout.LabelField($"Y: {offset.y:F1}");
                        EditorGUILayout.LabelField($"Z: {offset.z:F1}");
                        EditorGUI.indentLevel--;
                    }
                }
            }

            GUILayout.Space(15); // Right margin
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(settings);
            if (previewEnabled)
            {
                UpdateAutoCalculations();
                UpdatePreview();
                SceneView.RepaintAll();
            }
        }
    }

    private void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(15, 10, 15, 15),
                margin = new RectOffset(0, 0, 5, 5),
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };
        }

        if (sectionStyle == null)
        {
            sectionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(15, 15, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };
        }

        if (toggleStyle == null)
        {
            toggleStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };
        }
    }

    private void DrawHeader(string title)
    {
        EditorGUILayout.Space(5);
        var rect = EditorGUILayout.GetControlRect(false, 50); // Increased from 40 to 50
        EditorGUI.DrawRect(rect, headerColor);
        EditorGUI.LabelField(rect, title, headerStyle);
        EditorGUILayout.Space(5);
    }

    private void DrawPositionSettings()
    {
        settings.distributionPlane = (DistributionPlane)EditorGUILayout.EnumPopup(
            new GUIContent("Distribution Plane", "Choose which plane to arrange objects on"),
            settings.distributionPlane
        );

        settings.autoCalculate = EditorGUILayout.Toggle(
            new GUIContent("Auto Calculate", "Automatically calculate spacing and grid size based on object bounds"),
            settings.autoCalculate
        );

        // Start Position on one line
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(new GUIContent("Start Position", "Starting position for the grid arrangement"), GUILayout.Width(EditorGUIUtility.labelWidth - 15));
            settings.startPosition = EditorGUILayout.Vector3Field("", settings.startPosition);
        }

        settings.centerInGrid = EditorGUILayout.Toggle("Center In Grid", settings.centerInGrid);
    }

    private void DrawRotationSettings()
    {
        settings.rotationType = (GridArrangerSettings.RotationType)EditorGUILayout.EnumPopup(
            new GUIContent("Rotation Type", "How to handle object rotation"),
            settings.rotationType
        );

        EditorGUI.indentLevel++;
        switch (settings.rotationType)
        {
            case GridArrangerSettings.RotationType.Random:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Rotation Range");
                
                // Min value
                float roundedMin = Mathf.Round(settings.randomRotationRange.x * 100f) / 100f;
                settings.randomRotationRange.x = EditorGUILayout.FloatField(
                    roundedMin,
                    GUILayout.Width(75)
                );
                GUILayout.Space(-5); // Reduce space before slider
                
                // Slider
                EditorGUILayout.MinMaxSlider(
                    ref settings.randomRotationRange.x,
                    ref settings.randomRotationRange.y,
                    0f,
                    360f,
                    GUILayout.ExpandWidth(true)
                );
                GUILayout.Space(-5); // Reduce space after slider
                
                // Max value
                float roundedMax = Mathf.Round(settings.randomRotationRange.y * 100f) / 100f;
                settings.randomRotationRange.y = EditorGUILayout.FloatField(
                    roundedMax,
                    GUILayout.Width(75)
                );
                
                EditorGUILayout.EndHorizontal();
                break;

            case GridArrangerSettings.RotationType.Align:
                settings.targetRotation = EditorGUILayout.Vector3Field(
                    "Target Rotation",
                    settings.targetRotation
                );

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset", EditorStyles.miniButton))
                    {
                        settings.targetRotation = Vector3.zero;
                        GUI.changed = true;
                    }
                    if (GUILayout.Button("X Up", EditorStyles.miniButton))
                    {
                        settings.targetRotation = new Vector3(90, 0, 0);
                        GUI.changed = true;
                    }
                    if (GUILayout.Button("Y Up", EditorStyles.miniButton))
                    {
                        settings.targetRotation = Vector3.zero;
                        GUI.changed = true;
                    }
                    if (GUILayout.Button("Z Up", EditorStyles.miniButton))
                    {
                        settings.targetRotation = new Vector3(0, 0, 90);
                        GUI.changed = true;
                    }
                }
                break;
        }
        EditorGUI.indentLevel--;
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.Space(5);
        using (new EditorGUILayout.HorizontalScope())
        {
            previewEnabled = EditorGUILayout.ToggleLeft(
                new GUIContent("Show Preview", "Show preview of object positions in Scene view"), 
                previewEnabled
            );

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledGroupScope(Selection.gameObjects.Length == 0))
            {
                if (GUILayout.Button("Arrange Objects", GUILayout.Height(30)))
                {
                    ArrangeSelectedObjects();
                }
            }
        }
    }

    private void ResetSettings()
    {
        settings = CreateInstance<GridArrangerSettings>();
        EditorUtility.SetDirty(settings);
    }

    /// <summary>
    /// Calculates the minimum grid size needed to accommodate all selected objects
    /// </summary>
    private Vector3Int CalculateMinimumGridSize(int objectCount)
    {
        if (objectCount <= 0) return Vector3Int.one;

        // Calculate the size of the square grid
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(objectCount));
        
        // Set the appropriate dimensions based on the distribution plane
        return settings.distributionPlane switch
        {
            DistributionPlane.XZ => new Vector3Int(gridSize, 1, gridSize),
            DistributionPlane.XY => new Vector3Int(gridSize, gridSize, 1),
            DistributionPlane.YZ => new Vector3Int(1, gridSize, gridSize),
            _ => new Vector3Int(gridSize, 1, gridSize)
        };
    }

    /// <summary>
    /// Calculates the average size of selected objects including their children
    /// </summary>
    private Vector3 CalculateAverageObjectSize()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
            return Vector3.one;

        Vector3 totalSize = Vector3.zero;
        int validObjects = 0;

        foreach (GameObject obj in selectedObjects)
        {
            // Get bounds including all children
            Bounds? bounds = null;

            // Check Renderers (including children)
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (bounds.HasValue)
                    bounds.Value.Encapsulate(renderer.bounds);
                else
                    bounds = renderer.bounds;
            }

            // Check Colliders if no renderers found
            if (!bounds.HasValue)
            {
                var colliders = obj.GetComponentsInChildren<Collider>();
                foreach (var collider in colliders)
                {
                    if (bounds.HasValue)
                        bounds.Value.Encapsulate(collider.bounds);
                    else
                        bounds = collider.bounds;
                }
            }

            // Use transform scale as fallback
            Vector3 objSize = bounds?.size ?? obj.transform.lossyScale;

            if (objSize != Vector3.zero)
            {
                totalSize += objSize;
                validObjects++;
            }
        }

        if (validObjects == 0)
            return Vector3.one;

        // Calculate base spacing
        Vector3 averageSize = totalSize / validObjects;

        // Apply multiplier and handle distribution plane
        Vector3 spacing = averageSize * settings.spacingMultiplier;

        // Ensure minimum spacing
        spacing.x = Mathf.Max(spacing.x, 0.01f);
        spacing.y = Mathf.Max(spacing.y, 0.01f);
        spacing.z = Mathf.Max(spacing.z, 0.01f);

        // Handle 2D distribution planes
        switch (settings.distributionPlane)
        {
            case DistributionPlane.XY:
                spacing.z = 0;
                break;
            case DistributionPlane.XZ:
                spacing.y = 0;
                break;
            case DistributionPlane.YZ:
                spacing.x = 0;
                break;
        }

        return spacing;
    }

    /// <summary>
    /// Updates spacing and grid size based on selected objects
    /// </summary>
    private void UpdateAutoCalculations()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0 || !settings.autoCalculate)
            return;

        // Reset spacing and grid size before recalculating
        settings.spacing = Vector3.one * 2f;
        settings.gridSize = Vector3Int.one;

        // Calculate spacing based on object bounds
        Vector3 averageSize = CalculateAverageObjectSize();
        settings.spacing = averageSize * settings.spacingMultiplier;

        // Calculate grid size based on object count
        settings.gridSize = CalculateMinimumGridSize(selectedObjects.Length);
    }

    /// <summary>
    /// Updates the preview positions for the grid arrangement
    /// </summary>
    private void UpdatePreview()
    {
        if (!previewEnabled)
        {
            previewPositions.Clear();
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            previewPositions.Clear();
            return;
        }

        // Ensure we have current calculations
        UpdateAutoCalculations();
        CalculateGridPositions(selectedObjects.Length, out previewPositions);
    }

    /// <summary>
    /// Draws preview gizmos in the scene view
    /// </summary>
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!previewEnabled || previewPositions == null || previewPositions.Count == 0) 
            return;

        // Draw preview spheres
        Handles.color = new Color(0, 1, 0, 0.5f); // Semi-transparent green
        foreach (var position in previewPositions)
        {
            Handles.SphereHandleCap(
                0,
                position,
                Quaternion.identity,
                0.25f, // Size of preview sphere
                EventType.Repaint
            );
        }

        // Draw connecting lines to show grid structure
        Handles.color = new Color(0, 1, 0, 0.2f); // More transparent green
        for (int i = 0; i < previewPositions.Count; i++)
        {
            for (int j = i + 1; j < previewPositions.Count; j++)
            {
                // Only draw lines between adjacent positions based on the distribution plane
                bool shouldDrawLine = settings.distributionPlane switch
                {
                    DistributionPlane.XZ => 
                        Mathf.Approximately(previewPositions[i].y, previewPositions[j].y) &&
                        (Mathf.Approximately(previewPositions[i].x, previewPositions[j].x) ||
                         Mathf.Approximately(previewPositions[i].z, previewPositions[j].z)),
                    DistributionPlane.XY => 
                        Mathf.Approximately(previewPositions[i].z, previewPositions[j].z) &&
                        (Mathf.Approximately(previewPositions[i].x, previewPositions[j].x) ||
                         Mathf.Approximately(previewPositions[i].y, previewPositions[j].y)),
                    DistributionPlane.YZ => 
                        Mathf.Approximately(previewPositions[i].x, previewPositions[j].x) &&
                        (Mathf.Approximately(previewPositions[i].y, previewPositions[j].y) ||
                         Mathf.Approximately(previewPositions[i].z, previewPositions[j].z)),
                    _ => false
                };

                if (shouldDrawLine)
                {
                    Handles.DrawLine(previewPositions[i], previewPositions[j]);
                }
            }
        }
    }

    /// <summary>
    /// Calculates grid positions based on current settings
    /// </summary>
    private void CalculateGridPositions(int objectCount, out List<Vector3> positions)
    {
        positions = new List<Vector3>();
        if (objectCount == 0)
            return;

        // Get arrangement indices based on current settings
        foreach (Vector3Int index in GetArrangementIndices())
        {
            Vector3 position = GetPositionForAxis(index);
            positions.Add(position);
        }
    }

    private Vector3 CalculateCenterOffset(int objectCount)
    {
        Vector3 totalSize = new Vector3(
            (settings.gridSize.x - 1) * settings.spacing.x,
            (settings.gridSize.y - 1) * settings.spacing.y,
            (settings.gridSize.z - 1) * settings.spacing.z
        );
        return -totalSize * 0.5f;
    }

    private IEnumerable<Vector3Int> GetArrangementIndices()
    {
        List<Vector3Int> indices = new List<Vector3Int>();
        int totalObjects = Selection.gameObjects.Length;
        if (totalObjects == 0) return indices;

        switch (settings.distributionPlane)
        {
            case DistributionPlane.XZ:
                for (int z = 0; z < settings.gridSize.z; z++)
                    for (int x = 0; x < settings.gridSize.x; x++)
                        if (indices.Count < totalObjects)
                            indices.Add(new Vector3Int(x, 0, z));
                break;

            case DistributionPlane.XY:
                for (int y = 0; y < settings.gridSize.y; y++)
                    for (int x = 0; x < settings.gridSize.x; x++)
                        if (indices.Count < totalObjects)
                            indices.Add(new Vector3Int(x, y, 0));
                break;

            case DistributionPlane.YZ:
                for (int z = 0; z < settings.gridSize.z; z++)
                    for (int y = 0; y < settings.gridSize.y; y++)
                        if (indices.Count < totalObjects)
                            indices.Add(new Vector3Int(0, y, z));
                break;
        }

        return indices;
    }

    private Vector3 GetPositionForAxis(Vector3Int index)
    {
        Vector3 position = new Vector3(
            index.x * settings.spacing.x,
            index.y * settings.spacing.y,
            index.z * settings.spacing.z
        );

        if (settings.centerInGrid)
        {
            position += CalculateCenterOffset(Selection.gameObjects.Length);
        }

        return settings.startPosition + position;
    }

    private Bounds CalculateSelectionBounds()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one);

        Bounds bounds = new Bounds(selectedObjects[0].transform.position, Vector3.zero);
        foreach (GameObject obj in selectedObjects)
        {
            bounds.Encapsulate(obj.transform.position);
        }
        return bounds;
    }

    /// <summary>
    /// Arranges the selected objects in a grid pattern based on the specified settings
    /// </summary>
    private void ArrangeSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Grid Arranger",
                "Please select at least one object to arrange.",
                "OK"
            );
            return;
        }

        UpdateAutoCalculations();
        Undo.RecordObjects(
            selectedObjects.Select(obj => obj.transform).ToArray(),
            "Arrange Objects in Grid"
        );

        CalculateGridPositions(selectedObjects.Length, out var positions);

        for (int i = 0; i < selectedObjects.Length && i < positions.Count; i++)
        {
            Transform transform = selectedObjects[i].transform;
            transform.position = positions[i];

            // Handle rotation based on rotation type
            switch (settings.rotationType)
            {
                case GridArrangerSettings.RotationType.Align:
                    transform.rotation = Quaternion.Euler(settings.targetRotation);
                    break;

                case GridArrangerSettings.RotationType.Random:
                    float randomRotation = Random.Range(
                        settings.randomRotationRange.x,
                        settings.randomRotationRange.y
                    );
                    transform.rotation = Quaternion.Euler(0, randomRotation, 0);
                    break;
            }
        }
    }

    [System.Serializable]
    private class GridArrangerSettings : ScriptableObject
    {
        public DistributionPlane distributionPlane = DistributionPlane.XZ;
        public bool autoCalculate = true;
        public float spacingMultiplier = 1.1f;
        public Vector3 spacing = new Vector3(2f, 2f, 2f);
        public Vector3Int gridSize = new Vector3Int(3, 3, 3);
        public Vector3 startPosition = Vector3.zero;
        public bool centerInGrid = false;
        
        // Rotation settings as enum instead of booleans
        public enum RotationType
        {
            None,
            Align,
            Random
        }
        public RotationType rotationType = RotationType.None;
        public Vector3 targetRotation = Vector3.zero;
        public Vector2 randomRotationRange = new Vector2(0, 360);
    }

    // Update the preview when selection changes
    private void OnSelectionChange()
    {
        if (previewEnabled)
        {
            UpdatePreview();
            Repaint();
            SceneView.RepaintAll();
        }
    }
}
