#if UNITY_EDITOR
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
    private const double UpdateDelay = 0.05; // 50ms delay for smoother updates

    private Vector3? cachedRandomCenter;
    private Color headerColor = new Color(0.2f, 0.2f, 0.2f);

    private GUIStyle headerStyle;

    private Vector3? lastCenterPosition;
    private DistributionPlane lastDistributionPlane;
    private Vector3Int lastGridSize;

    private Vector3 lastHitNormal;

    private GameObject[] lastSelectedObjects;
    private Vector3 lastSpacing;
    private double lastUpdateTime;
    private bool previewEnabled = true;
    private List<Vector3> previewPositions = new List<Vector3>();
    private Vector2 scrollPosition;
    private Color sectionColor = new Color(0.18f, 0.18f, 0.18f);
    private GUIStyle sectionStyle;

    private GridArrangerSettings settings;
    private bool showAdvancedSettings = false;
    private bool showDistributionSettings = true;
    private bool showPositionSettings = true;
    private bool showRotationSettings = true;

    private bool showStatistics = true;
    private GUIStyle toggleStyle;

    private bool updatePending = false;

    public enum DistributionPlane
    {
        XZ, // Ground plane (default)
        XY, // Front plane
        YZ // Side plane
    }

    /// <summary>
    /// Opens the Grid Arranger window
    /// </summary>
    [MenuItem("Tools/Darklight/GridArranger")]
    public static void ShowWindow()
    {
        var window = GetWindow<GridArrangerWindow>("Darklight Grid Arranger");
        window.minSize = new Vector2(400, 500); // Minimum size that works with vertical layout
        window.maxSize = new Vector2(800, 600); // Maximum size for readability
        window.position = new Rect(window.position.x, window.position.y, 725, 550); // Default/preferred size
        window.LoadOrCreateSettings();
    }

    #region < PRIVATE_METHODS > [[ Internal Calculations ]] ================================================================

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

        if (settings.positionType == GridArrangerSettings.PositionType.Randomize)
        {
            Vector3 randomCenter = CalculateRandomizeCenter();
            float range = settings.randomRange.y;

            foreach (GameObject obj in selectedObjects)
            {
                Vector3 randomPos = Vector3.zero;
                switch (settings.distributionPlane)
                {
                    case DistributionPlane.XZ:
                        randomPos = new Vector3(
                            Random.Range(-range, range),
                            0,
                            Random.Range(-range, range)
                        );
                        break;
                    case DistributionPlane.XY:
                        randomPos = new Vector3(
                            Random.Range(-range, range),
                            Random.Range(-range, range),
                            0
                        );
                        break;
                    case DistributionPlane.YZ:
                        randomPos = new Vector3(
                            0,
                            Random.Range(-range, range),
                            Random.Range(-range, range)
                        );
                        break;
                }

                if (settings.snapToGrid)
                {
                    float gridSize = EditorSnapSettings.move.x;
                    randomPos = new Vector3(
                        Mathf.Round(randomPos.x / gridSize) * gridSize,
                        Mathf.Round(randomPos.y / gridSize) * gridSize,
                        Mathf.Round(randomPos.z / gridSize) * gridSize
                    );
                }

                obj.transform.position = randomCenter + randomPos;
            }
        }
        else
        {
            CalculateGridPositions(selectedObjects.Length, out var positions);
            for (int i = 0; i < selectedObjects.Length && i < positions.Count; i++)
            {
                Transform transform = selectedObjects[i].transform;
                transform.position = positions[i];
            }
        }

        // Handle rotation based on rotation type
        foreach (GameObject obj in selectedObjects)
        {
            switch (settings.rotationType)
            {
                case GridArrangerSettings.RotationType.Align:
                    obj.transform.rotation = Quaternion.Euler(settings.targetRotation);
                    break;

                case GridArrangerSettings.RotationType.Random:
                    float randomRotation = Random.Range(
                        settings.randomRotationRange.x,
                        settings.randomRotationRange.y
                    );
                    obj.transform.rotation = Quaternion.Euler(0, randomRotation, 0);
                    break;
            }
        }
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

        // Cache component queries
        var bounds = new Dictionary<GameObject, Bounds?>();

        foreach (GameObject obj in selectedObjects)
        {
            if (!bounds.TryGetValue(obj, out var objBounds))
            {
                objBounds = null;
                var renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (objBounds.HasValue)
                        objBounds.Value.Encapsulate(renderer.bounds);
                    else
                        objBounds = renderer.bounds;
                }

                if (!objBounds.HasValue)
                {
                    var colliders = obj.GetComponentsInChildren<Collider>();
                    foreach (var collider in colliders)
                    {
                        if (objBounds.HasValue)
                            objBounds.Value.Encapsulate(collider.bounds);
                        else
                            objBounds = collider.bounds;
                    }
                }
                bounds[obj] = objBounds;
            }

            Vector3 objSize = objBounds?.size ?? obj.transform.lossyScale;
            if (objSize != Vector3.zero)
            {
                totalSize += objSize;
                validObjects++;
            }
        }

        return validObjects == 0
            ? Vector3.one
            : (totalSize / validObjects) * settings.spacingMultiplier;
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

    private Vector3 CalculateCenterPosition()
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
            return Vector3.zero;

        Vector3 center =
            selectedObjects.Aggregate(Vector3.zero, (sum, obj) => sum + obj.transform.position)
            / selectedObjects.Length;

        // Snap to Unity's grid settings
        float gridSize = EditorSnapSettings.move.x;
        center.x = Mathf.Round(center.x / gridSize) * gridSize;
        center.y = Mathf.Round(center.y / gridSize) * gridSize;
        center.z = Mathf.Round(center.z / gridSize) * gridSize;

        // Zero out the non-distributed axis based on plane
        switch (settings.distributionPlane)
        {
            case DistributionPlane.XZ:
                center.y = 0;
                break;
            case DistributionPlane.XY:
                center.z = 0;
                break;
            case DistributionPlane.YZ:
                center.x = 0;
                break;
        }

        return center;
    }

    /// <summary>
    /// Calculates grid positions based on current settings
    /// </summary>
    private void CalculateGridPositions(int objectCount, out List<Vector3> positions)
    {
        positions = new List<Vector3>();
        if (objectCount == 0)
            return;

        switch (settings.positionType)
        {
            case GridArrangerSettings.PositionType.Randomize:
                // For preview, show current positions of objects
                positions.AddRange(Selection.gameObjects.Select(obj => obj.transform.position));
                break;

            default:
                // Get all possible grid positions
                var indices = GetArrangementIndices().ToList();

                // For preview, show all positions in the grid
                if (previewEnabled)
                {
                    foreach (var index in indices)
                    {
                        positions.Add(GetPositionForAxis(index));
                    }
                }
                // For actual arrangement, only use needed positions
                else
                {
                    for (int i = 0; i < Mathf.Min(objectCount, indices.Count); i++)
                    {
                        positions.Add(GetPositionForAxis(indices[i]));
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Calculates the minimum grid size needed to accommodate all selected objects
    /// </summary>
    private Vector3Int CalculateMinimumGridSize(int objectCount)
    {
        if (objectCount <= 0)
            return Vector3Int.one;

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

    private Vector3 CalculateRandomizeCenter()
    {
        // Return cached center if it exists
        if (cachedRandomCenter.HasValue)
            return cachedRandomCenter.Value;

        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
            return Vector3.zero;

        Vector3 roughCenter =
            selectedObjects.Aggregate(Vector3.zero, (sum, obj) => sum + obj.transform.position)
            / selectedObjects.Length;

        float gridSize = EditorSnapSettings.move.x;
        Vector3 snappedCenter = new Vector3(
            Mathf.Round(roughCenter.x / gridSize) * gridSize,
            Mathf.Round(roughCenter.y / gridSize) * gridSize,
            Mathf.Round(roughCenter.z / gridSize) * gridSize
        );

        Vector3 center = settings.distributionPlane switch
        {
            DistributionPlane.XZ => new Vector3(snappedCenter.x, 0, snappedCenter.z),
            DistributionPlane.XY => new Vector3(snappedCenter.x, snappedCenter.y, 0),
            DistributionPlane.YZ => new Vector3(0, snappedCenter.y, snappedCenter.z),
            _ => new Vector3(snappedCenter.x, 0, snappedCenter.z)
        };

        cachedRandomCenter = center;
        return center;
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
    #endregion

    #region < PRIVATE_METHODS > [[ Draw UI ]] ==================================================================
    private void DrawActionButtons()
    {
        previewEnabled = EditorGUILayout.ToggleLeft(
            new GUIContent("Show Preview", "Show preview of object positions in Scene view"),
            previewEnabled
        );

        EditorGUILayout.Space(5);

        if (Selection.gameObjects.Length <= 1)
        {
            EditorGUILayout.HelpBox(
                "Select multiple objects in the scene to arrange them",
                MessageType.Warning
            );
        }
        else if (
            GUILayout.Button(
                settings.positionType == GridArrangerSettings.PositionType.Randomize
                    ? "Randomize Objects"
                    : "Arrange Objects",
                GUILayout.Height(30)
            )
        )
        {
            ArrangeSelectedObjects();
        }

        if (GUILayout.Button("Reset Settings", EditorStyles.miniButton))
        {
            if (
                EditorUtility.DisplayDialog(
                    "Reset Settings",
                    "Are you sure you want to reset all settings to their default values?",
                    "Reset",
                    "Cancel"
                )
            )
            {
                ResetSettings();
                UpdatePreview();
            }
        }

        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.ObjectField(
                new GUIContent("Settings Asset", "Location of the settings asset"),
                settings,
                typeof(GridArrangerSettings),
                false
            );
        }
    }

    private void DrawDistributionPlaneSettings()
    {
        EditorGUILayout.LabelField("Distribution Plane", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        settings.distributionPlane = (DistributionPlane)
            EditorGUILayout.EnumPopup(
                new GUIContent("Distribution Plane", "Choose which plane to arrange objects on"),
                settings.distributionPlane
            );

        settings.snapToGrid = EditorGUILayout.Toggle(
            new GUIContent("Snap to Grid", "Snap object positions to Unity's grid settings"),
            settings.snapToGrid
        );

        EditorGUI.indentLevel--;
    }

    private void DrawGridSizeControls()
    {
        int objectCount = Selection.gameObjects.Length;
        Vector2Int gridSize2D = GetGridSize2D();
        Vector2Int newGridSize = gridSize2D;

        // Calculate min/max values for rows and columns
        int minDimension = Mathf.CeilToInt(Mathf.Sqrt(objectCount));
        int maxDimension = objectCount;

        // Validate current values before displaying
        if (
            gridSize2D.x < minDimension
            || gridSize2D.x > maxDimension
            || gridSize2D.y < Mathf.CeilToInt((float)objectCount / gridSize2D.x)
            || gridSize2D.y > maxDimension
        )
        {
            ValidateAndUpdateGridSize();
            gridSize2D = GetGridSize2D();
            newGridSize = gridSize2D;
        }

        EditorGUI.BeginChangeCheck();
        switch (settings.distributionPlane)
        {
            case DistributionPlane.XZ:
                // Columns
                EditorGUILayout.LabelField(
                    $"Columns (X) - Min: {minDimension}, Max: {maxDimension}"
                );
                newGridSize.x = EditorGUILayout.IntSlider(gridSize2D.x, minDimension, maxDimension);

                // Calculate row constraints based on column selection
                int minRows = Mathf.CeilToInt((float)objectCount / newGridSize.x);
                int maxRows = Mathf.Min(maxDimension, minRows * 2);

                EditorGUILayout.LabelField($"Rows (Z) - Min: {minRows}, Max: {maxRows}");
                newGridSize.y = EditorGUILayout.IntSlider(
                    Mathf.Clamp(gridSize2D.y, minRows, maxRows),
                    minRows,
                    maxRows
                );
                settings.gridSize = new Vector3Int(newGridSize.x, 1, newGridSize.y);
                break;

            case DistributionPlane.XY:
                // Columns
                EditorGUILayout.LabelField(
                    $"Columns (X) - Min: {minDimension}, Max: {maxDimension}"
                );
                newGridSize.x = EditorGUILayout.IntSlider(gridSize2D.x, minDimension, maxDimension);

                // Calculate row constraints based on column selection
                minRows = Mathf.CeilToInt((float)objectCount / newGridSize.x);
                maxRows = Mathf.Min(maxDimension, minRows * 2);

                EditorGUILayout.LabelField($"Rows (Y) - Min: {minRows}, Max: {maxRows}");
                newGridSize.y = EditorGUILayout.IntSlider(
                    Mathf.Clamp(gridSize2D.y, minRows, maxRows),
                    minRows,
                    maxRows
                );
                settings.gridSize = new Vector3Int(newGridSize.x, newGridSize.y, 1);
                break;

            case DistributionPlane.YZ:
                // Columns
                EditorGUILayout.LabelField(
                    $"Columns (Y) - Min: {minDimension}, Max: {maxDimension}"
                );
                newGridSize.x = EditorGUILayout.IntSlider(gridSize2D.x, minDimension, maxDimension);

                // Calculate row constraints based on column selection
                minRows = Mathf.CeilToInt((float)objectCount / newGridSize.x);
                maxRows = Mathf.Min(maxDimension, minRows * 2);

                EditorGUILayout.LabelField($"Rows (Z) - Min: {minRows}, Max: {maxRows}");
                newGridSize.y = EditorGUILayout.IntSlider(
                    Mathf.Clamp(gridSize2D.y, minRows, maxRows),
                    minRows,
                    maxRows
                );
                settings.gridSize = new Vector3Int(1, newGridSize.x, newGridSize.y);
                break;
        }

        if (EditorGUI.EndChangeCheck())
        {
            // Only update when the slider drag is complete
            if (Event.current.type == EventType.Used)
            {
                ValidateAndUpdateGridSize();
                UpdatePreview();
                SceneView.RepaintAll();
            }
        }
    }

    private void DrawHeader(string title)
    {
        EditorGUILayout.LabelField(title, headerStyle, GUILayout.ExpandWidth(true));
    }

    private void DrawHorizontalLayout()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);

        // Left side - Settings
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.60f)))
        {
            using (new EditorGUILayout.VerticalScope(sectionStyle))
            {
                DrawDistributionPlaneSettings();
            }

            EditorGUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(sectionStyle))
            {
                DrawPositionSettings();
            }

            EditorGUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(sectionStyle))
            {
                DrawRotationSettings();
            }
        }

        GUILayout.Space(15);

        // Right side - Preview and Actions
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.27f));

        using (new EditorGUILayout.VerticalScope(sectionStyle))
        {
            DrawActionButtons();
        }

        EditorGUILayout.Space(10);

        // Statistics Section
        using (new EditorGUILayout.VerticalScope(sectionStyle))
        {
            showStatistics = EditorGUILayout.Foldout(showStatistics, "Statistics", true);
            if (showStatistics)
            {
                DrawStatsBox();
            }
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(15);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPositionSettings()
    {
        showPositionSettings = EditorGUILayout.Foldout(
            showPositionSettings,
            "Position Settings",
            true
        );
        if (!showPositionSettings)
            return;

        EditorGUI.indentLevel++;
        settings.positionType = (GridArrangerSettings.PositionType)
            EditorGUILayout.EnumPopup(
                new GUIContent("Position Type", "How to arrange object positions"),
                settings.positionType
            );

        EditorGUI.indentLevel++;
        switch (settings.positionType)
        {
            case GridArrangerSettings.PositionType.Auto:
                settings.spacingMultiplier = EditorGUILayout.Slider(
                    new GUIContent(
                        "Spacing Multiplier",
                        "Multiplier for automatically calculated spacing"
                    ),
                    settings.spacingMultiplier,
                    1f,
                    10f
                );
                break;

            case GridArrangerSettings.PositionType.ManualGrid:
            case GridArrangerSettings.PositionType.ManualGridAndSpacing:
                DrawGridSizeControls();

                if (settings.positionType == GridArrangerSettings.PositionType.ManualGrid)
                {
                    // Calculate and show best spacing
                    Vector3 averageSize = CalculateAverageObjectSize();
                    float bestSpacing = settings.distributionPlane switch
                    {
                        DistributionPlane.XZ => Mathf.Max(averageSize.x, averageSize.z),
                        DistributionPlane.XY => Mathf.Max(averageSize.x, averageSize.y),
                        DistributionPlane.YZ => Mathf.Max(averageSize.y, averageSize.z),
                        _ => averageSize.x
                    };

                    settings.spacing = settings.distributionPlane switch
                    {
                        DistributionPlane.XZ => new Vector3(bestSpacing, 0, bestSpacing),
                        DistributionPlane.XY => new Vector3(bestSpacing, bestSpacing, 0),
                        DistributionPlane.YZ => new Vector3(0, bestSpacing, bestSpacing),
                        _ => new Vector3(bestSpacing, 0, bestSpacing)
                    };

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.FloatField(
                            new GUIContent(
                                "Calculated Spacing",
                                "Automatically calculated based on object sizes"
                            ),
                            bestSpacing
                        );
                    }
                }
                else // ManualGridAndSpacing
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Spacing", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    float spacing = EditorGUILayout.Slider(
                        new GUIContent("Grid Spacing", "Distance between objects in the grid"),
                        settings.spacing.x,
                        0.1f,
                        10f
                    );

                    if (EditorGUI.EndChangeCheck())
                    {
                        // Only update when the slider drag is complete
                        if (Event.current.type == EventType.Used)
                        {
                            settings.spacing = settings.distributionPlane switch
                            {
                                DistributionPlane.XZ => new Vector3(spacing, 0, spacing),
                                DistributionPlane.XY => new Vector3(spacing, spacing, 0),
                                DistributionPlane.YZ => new Vector3(0, spacing, spacing),
                                _ => new Vector3(spacing, 0, spacing)
                            };
                            UpdatePreview();
                            SceneView.RepaintAll();
                        }
                    }
                }
                break;

            case GridArrangerSettings.PositionType.Randomize:
                EditorGUI.BeginChangeCheck();
                float range = EditorGUILayout.Slider(
                    new GUIContent(
                        "Random Range",
                        "Maximum distance from center for random distribution"
                    ),
                    settings.randomRange.y,
                    0.1f,
                    100f
                );

                if (EditorGUI.EndChangeCheck())
                {
                    if (Event.current.type == EventType.Used)
                    {
                        settings.randomRange = new Vector2(-range, range);
                        UpdatePreview();
                        SceneView.RepaintAll();
                    }
                }
                break;
        }
        EditorGUI.indentLevel -= 2;
    }

    private void DrawPreviewPoints()
    {
        if (previewPositions.Count == 0)
            return;

        // Draw thicker outline first
        Handles.color = new Color(0.2f, 0.5f, 1f, 1f);
        float size = HandleUtility.GetHandleSize(previewPositions[0]) * 0.15f;

        foreach (var position in previewPositions)
        {
            Handles.SphereHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
        }

        // Draw inner sphere
        Handles.color = new Color(0.2f, 0.5f, 1f, 0.8f);
        size = HandleUtility.GetHandleSize(previewPositions[0]) * 0.1f;

        foreach (var position in previewPositions)
        {
            Handles.SphereHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
        }
    }

    private void DrawRotationSettings()
    {
        showRotationSettings = EditorGUILayout.Foldout(
            showRotationSettings,
            "Rotation Settings",
            true
        );
        if (!showRotationSettings)
            return;

        EditorGUI.indentLevel++;

        settings.rotationType = (GridArrangerSettings.RotationType)
            EditorGUILayout.EnumPopup(
                new GUIContent("Rotation Type", "How to handle object rotation"),
                settings.rotationType
            );

        EditorGUI.indentLevel++;
        switch (settings.rotationType)
        {
            case GridArrangerSettings.RotationType.Random:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Rotation Range");

                float roundedMin = Mathf.Round(settings.randomRotationRange.x * 100f) / 100f;
                settings.randomRotationRange.x = EditorGUILayout.FloatField(
                    roundedMin,
                    GUILayout.Width(75)
                );
                GUILayout.Space(-5);

                EditorGUILayout.MinMaxSlider(
                    ref settings.randomRotationRange.x,
                    ref settings.randomRotationRange.y,
                    0f,
                    360f,
                    GUILayout.ExpandWidth(true)
                );
                GUILayout.Space(-5);

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
        EditorGUI.indentLevel -= 2;
    }

    private void DrawStatsBox()
    {
        var selectedObjects = Selection.gameObjects;
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField($"Selected Objects:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField($"{selectedObjects.Length}");
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField($"Grid Dimensions:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField(
            $"{settings.gridSize.x}x{settings.gridSize.y}x{settings.gridSize.z}"
        );
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Current Spacing:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField($"X: {settings.spacing.x:F2}");
        EditorGUILayout.LabelField($"Y: {settings.spacing.y:F2}");
        EditorGUILayout.LabelField($"Z: {settings.spacing.z:F2}");
        EditorGUI.indentLevel--;

        if (settings.autoCalculate)
        {
            EditorGUILayout.LabelField("Spacing Multiplier:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"{settings.spacingMultiplier:F2}x");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("Center Offset:", EditorStyles.boldLabel);
        Vector3 offset = CalculateCenterOffset(selectedObjects.Length);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField($"X: {offset.x:F1}");
        EditorGUILayout.LabelField($"Y: {offset.y:F1}");
        EditorGUILayout.LabelField($"Z: {offset.z:F1}");
        EditorGUI.indentLevel--;
    }

    private void DrawVerticalLayout()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(15);

        // Distribution Plane Section
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(15); // Left margin
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.ExpandWidth(true)))
            {
                DrawDistributionPlaneSettings();
            }
            GUILayout.Space(15); // Right margin
        }

        EditorGUILayout.Space(5);

        // Position Settings Section
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(15);
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.ExpandWidth(true)))
            {
                DrawPositionSettings();
            }
            GUILayout.Space(15);
        }

        EditorGUILayout.Space(5);

        // Rotation Settings Section
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(15);
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.ExpandWidth(true)))
            {
                DrawRotationSettings();
            }
            GUILayout.Space(15);
        }

        EditorGUILayout.Space(10);

        // Preview and Actions Section
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(15);
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.ExpandWidth(true)))
            {
                DrawActionButtons();
            }
            GUILayout.Space(15);
        }

        EditorGUILayout.Space(10);

        // Statistics Section
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(15);
            using (new EditorGUILayout.VerticalScope(sectionStyle, GUILayout.ExpandWidth(true)))
            {
                showStatistics = EditorGUILayout.Foldout(showStatistics, "Statistics", true);
                if (showStatistics)
                {
                    DrawStatsBox();
                }
            }
            GUILayout.Space(15);
        }

        GUILayout.Space(15);
        EditorGUILayout.EndVertical();
    }

    #endregion

    #region < PRIVATE_METHODS > [[ Grid Calculations ]] ================================================================
    /// <summary>
    /// Gets the indices for grid positions, including empty spaces for uniform grid
    /// </summary>
    private IEnumerable<Vector3Int> GetArrangementIndices()
    {
        List<Vector3Int> indices = new List<Vector3Int>();
        int totalObjects = Selection.gameObjects.Length;
        if (totalObjects == 0)
            return indices;

        switch (settings.distributionPlane)
        {
            case DistributionPlane.XZ:
                for (int z = 0; z < settings.gridSize.z; z++)
                for (int x = 0; x < settings.gridSize.x; x++)
                    indices.Add(new Vector3Int(x, 0, z));
                break;

            case DistributionPlane.XY:
                for (int y = 0; y < settings.gridSize.y; y++)
                for (int x = 0; x < settings.gridSize.x; x++)
                    indices.Add(new Vector3Int(x, y, 0));
                break;

            case DistributionPlane.YZ:
                for (int z = 0; z < settings.gridSize.z; z++)
                for (int y = 0; y < settings.gridSize.y; y++)
                    indices.Add(new Vector3Int(0, y, z));
                break;
        }

        return indices;
    }

    private Vector2Int GetGridSize2D()
    {
        return settings.distributionPlane switch
        {
            DistributionPlane.XZ => new Vector2Int(settings.gridSize.x, settings.gridSize.z),
            DistributionPlane.XY => new Vector2Int(settings.gridSize.x, settings.gridSize.y),
            DistributionPlane.YZ => new Vector2Int(settings.gridSize.y, settings.gridSize.z),
            _ => new Vector2Int(settings.gridSize.x, settings.gridSize.z)
        };
    }

    private Vector3 GetPositionForAxis(Vector3Int index)
    {
        Vector3 position = new Vector3(
            index.x * settings.spacing.x,
            index.y * settings.spacing.y,
            index.z * settings.spacing.z
        );

        Vector3 centerOffset = CalculateCenterOffset(Selection.gameObjects.Length);
        Vector3 centerPosition = CalculateCenterPosition();
        Vector3 finalPosition = centerPosition + position + centerOffset;

        // Only snap if enabled
        if (settings.snapToGrid)
        {
            float gridSize = EditorSnapSettings.move.x;
            finalPosition.x = Mathf.Round(finalPosition.x / gridSize) * gridSize;
            finalPosition.y = Mathf.Round(finalPosition.y / gridSize) * gridSize;
            finalPosition.z = Mathf.Round(finalPosition.z / gridSize) * gridSize;
        }

        return finalPosition;
    }

    // Call ResetRandomizeCenter() when position type or distribution plane changes
    #endregion

    #region < PRIVATE_METHODS > [[ Setup UI ]] ======================================================================
    private void InitializeStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 10, 10)
        };

        sectionStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(0, 0, 5, 5)
        };

        toggleStyle = new GUIStyle(EditorStyles.toggle) { padding = new RectOffset(5, 5, 5, 5) };
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
    #endregion

    #region < PRIVATE_METHODS > [[ Unity Editor Events ]] ======================================================================
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChange;
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (!previewEnabled || Selection.gameObjects.Length == 0)
            return;

        // Track position changes
        Vector3 currentCenter =
            Selection.gameObjects.Aggregate(
                Vector3.zero,
                (sum, obj) => sum + obj.transform.position
            ) / Selection.gameObjects.Length;

        if (
            !lastCenterPosition.HasValue
            || Vector3.Distance(currentCenter, lastCenterPosition.Value) > 0.001f
        )
        {
            RequestUpdate();
            lastCenterPosition = currentCenter;
        }

        if (updatePending && EditorApplication.timeSinceStartup - lastUpdateTime >= UpdateDelay)
        {
            //Debug.Log($"[GridArranger] Executing delayed update after {EditorApplication.timeSinceStartup - lastUpdateTime:F3}s");
            updatePending = false;
            if (settings.positionType == GridArrangerSettings.PositionType.Randomize)
            {
                ResetRandomizeCenter();
            }
            UpdatePreview();
            SceneView.RepaintAll();
        }
    }

    private void OnEnable()
    {
        LoadOrCreateSettings();
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChange;
        EditorApplication.update += OnEditorUpdate;
        RequestUpdate();
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

        if (headerStyle == null)
            InitializeStyles();

        EditorGUI.BeginChangeCheck();

        DrawHeader("Grid Arranger");
        EditorGUILayout.Space(10);

        // Determine layout based on window width
        bool useVerticalLayout = position.width < 650; // Threshold for switching layouts

        if (useVerticalLayout)
        {
            DrawVerticalLayout();
        }
        else
        {
            DrawHorizontalLayout();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(settings);
            RequestUpdate();
        }
    }

    /// <summary>
    /// Draws preview gizmos in the scene view
    /// </summary>
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!previewEnabled)
            return;

        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
            return;

        // Draw preview points and connections
        DrawPreviewPoints();

        // Draw random distribution range visualization
        if (settings.positionType == GridArrangerSettings.PositionType.Randomize)
        {
            Vector3 center = CalculateRandomizeCenter();
            float range = settings.randomRange.y;

            Color fillColor = new Color(0.2f, 0.5f, 1f, 0.1f);
            Color outlineColor = new Color(0.2f, 0.5f, 1f, 0.3f);

            Vector3[] squareCorners = new Vector3[4];
            switch (settings.distributionPlane)
            {
                case DistributionPlane.XZ:
                    squareCorners[0] = center + new Vector3(-range, 0, -range);
                    squareCorners[1] = center + new Vector3(range, 0, -range);
                    squareCorners[2] = center + new Vector3(range, 0, range);
                    squareCorners[3] = center + new Vector3(-range, 0, range);
                    break;

                case DistributionPlane.XY:
                    squareCorners[0] = center + new Vector3(-range, -range, 0);
                    squareCorners[1] = center + new Vector3(range, -range, 0);
                    squareCorners[2] = center + new Vector3(range, range, 0);
                    squareCorners[3] = center + new Vector3(-range, range, 0);
                    break;

                case DistributionPlane.YZ:
                    squareCorners[0] = center + new Vector3(0, -range, -range);
                    squareCorners[1] = center + new Vector3(0, range, -range);
                    squareCorners[2] = center + new Vector3(0, range, range);
                    squareCorners[3] = center + new Vector3(0, -range, range);
                    break;
            }

            Handles.color = fillColor;
            Handles.DrawSolidRectangleWithOutline(squareCorners, fillColor, outlineColor);

            // Draw thicker outline
            Handles.color = outlineColor;
            float thickness = 3f;
            for (int i = 0; i < thickness; i++)
            {
                Handles.DrawPolyLine(
                    squareCorners[0],
                    squareCorners[1],
                    squareCorners[2],
                    squareCorners[3],
                    squareCorners[0]
                );
            }
        }
    }

    // Update the preview when selection changes
    private void OnSelectionChange()
    {
        if (!previewEnabled)
            return;

        if (settings.positionType != GridArrangerSettings.PositionType.Auto)
        {
            ValidateAndUpdateGridSize();
        }

        UpdateAutoCalculations();
        RequestUpdate();
    }
    #endregion

    #region < PRIVATE_METHODS > [[ Update Request ]] ======================================================================
    private void RequestUpdate()
    {
        if (!updatePending)
        {
            Debug.Log("[GridArranger] Requesting update");
            updatePending = true;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }
        else
        {
            //Debug.Log("[GridArranger] Update already pending");
        }
    }

    /// <summary>
    /// Updates spacing and grid size based on selected objects
    /// </summary>
    private void UpdateAutoCalculations()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0 || !settings.autoCalculate)
            return;

        // Calculate spacing based on object bounds
        Vector3 averageSize = CalculateAverageObjectSize();
        float maxPlanarSize = settings.distributionPlane switch
        {
            DistributionPlane.XZ => Mathf.Max(averageSize.x, averageSize.z),
            DistributionPlane.XY => Mathf.Max(averageSize.x, averageSize.y),
            DistributionPlane.YZ => Mathf.Max(averageSize.y, averageSize.z),
            _ => averageSize.x
        };

        // Set uniform spacing for the distribution plane, zero for the unused axis
        settings.spacing = settings.distributionPlane switch
        {
            DistributionPlane.XZ
                => new Vector3(maxPlanarSize, 0, maxPlanarSize) * settings.spacingMultiplier,
            DistributionPlane.XY
                => new Vector3(maxPlanarSize, maxPlanarSize, 0) * settings.spacingMultiplier,
            DistributionPlane.YZ
                => new Vector3(0, maxPlanarSize, maxPlanarSize) * settings.spacingMultiplier,
            _ => new Vector3(maxPlanarSize, 0, maxPlanarSize) * settings.spacingMultiplier
        };

        // Only update grid size in Auto mode
        if (settings.positionType == GridArrangerSettings.PositionType.Auto)
        {
            settings.gridSize = CalculateMinimumGridSize(selectedObjects.Length);
        }
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
            return;

        // Calculate current center for position change detection
        Vector3 currentCenter =
            selectedObjects.Aggregate(Vector3.zero, (sum, obj) => sum + obj.transform.position)
            / selectedObjects.Length;

        lastSelectedObjects = selectedObjects;
        lastGridSize = settings.gridSize;
        lastSpacing = settings.spacing;
        lastDistributionPlane = settings.distributionPlane;
        lastCenterPosition = currentCenter;

        CalculateGridPositions(selectedObjects.Length, out previewPositions);
        SceneView.RepaintAll();
    }

    private void ValidateAndUpdateGridSize()
    {
        int objectCount = Selection.gameObjects.Length;
        if (objectCount == 0)
            return;

        Vector2Int currentSize = GetGridSize2D();

        // Calculate base constraints
        int minDimension = Mathf.CeilToInt(Mathf.Sqrt(objectCount));

        // Clamp columns first
        int newColumns = Mathf.Clamp(currentSize.x, minDimension, objectCount);

        // Calculate row constraints based on column count
        int minRows = Mathf.CeilToInt((float)objectCount / newColumns);
        int maxRows = Mathf.Min(objectCount, minRows * 2); // Limit max rows to double the minimum

        // Clamp rows to valid range
        int newRows = Mathf.Clamp(currentSize.y, minRows, maxRows);

        // Check if total grid size is too large
        int totalGridSpaces = newColumns * newRows;
        if (totalGridSpaces > objectCount * 2)
        {
            // Recalculate to get closer to a square grid
            newColumns = Mathf.CeilToInt(Mathf.Sqrt(objectCount));
            minRows = Mathf.CeilToInt((float)objectCount / newColumns);
            newRows = Mathf.Min(maxRows, minRows * 2);
        }

        // Update grid size based on distribution plane
        settings.gridSize = settings.distributionPlane switch
        {
            DistributionPlane.XZ => new Vector3Int(newColumns, 1, newRows),
            DistributionPlane.XY => new Vector3Int(newColumns, newRows, 1),
            DistributionPlane.YZ => new Vector3Int(1, newColumns, newRows),
            _ => new Vector3Int(newColumns, 1, newRows)
        };
    }
    #endregion

    #region < PRIVATE_METHODS > [[ Reset ]] ======================================================================
    // Add this to reset the cached center when needed
    private void ResetRandomizeCenter()
    {
        cachedRandomCenter = null;
    }

    private void ResetSettings()
    {
        settings = CreateInstance<GridArrangerSettings>();
        EditorUtility.SetDirty(settings);
    }
    #endregion

    #region < NESTED_TYPE > [[ Settings ]] ======================================================================
    [CreateAssetMenu(fileName = "GridArrangerSettings", menuName = "Grid Arranger/Settings")]
    public class GridArrangerSettings : ScriptableObject
    {
        [SerializeField]
        public bool autoCalculate = true;

        [SerializeField]
        public DistributionPlane distributionPlane = DistributionPlane.XZ;

        [SerializeField]
        public Vector3Int gridSize = new Vector3Int(3, 3, 3);

        [SerializeField]
        public PositionType positionType = PositionType.Auto;

        [SerializeField]
        public Vector2 randomRange = new Vector2(0, 10);

        [SerializeField]
        public Vector2 randomRotationRange = new Vector2(0, 360);

        [SerializeField]
        public RotationType rotationType = RotationType.None;

        [SerializeField]
        public bool snapToGrid = true;

        [SerializeField]
        public Vector3 spacing = new Vector3(2f, 2f, 2f);

        [SerializeField]
        public float spacingMultiplier = 1.1f;

        [SerializeField]
        public Vector3 targetRotation = Vector3.zero;

        public enum PositionType
        {
            Auto,
            ManualGrid,
            ManualGridAndSpacing,
            Randomize
        }

        public enum RotationType
        {
            None,
            Align,
            Random
        }
    }
    #endregion
}
#endif
