using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [CreateAssetMenu(menuName = "Darklight/Grid2D/DataObject")]
    public class GridMapConfig_DataObject : ScriptableObject
    {
        #region ---- ( CUSTOM EDITOR DATA ) --------- >>
        DropdownList<Vector3> editor_directions = new DropdownList<Vector3>()
        {
            { "Up", Vector3.up },
            { "Down", Vector3.down },
            { "Left", Vector3.left },
            { "Right", Vector3.right },
            { "Forward", Vector3.forward },
            { "Back", Vector3.back }
        };
        bool editor_showTransform => !_lockToTransform;
        #endregion

        [SerializeField] bool _lockToTransform = true;
        [SerializeField] Alignment _gridAlignment = Alignment.Center;
        [SerializeField, Dropdown("editor_directions")] Vector3 _gridNormal = Vector3.up;

        // (( Grid Dimensions )) ------------------------------ >>
        [Header("-- Grid Dimensions ---- >>")]
        [SerializeField, Range(1, 10)] int _numColumns = 3;
        [SerializeField, Range(1, 10)] int _numRows = 3;

        [Header("-- Cell Bonding ---- >>")]
        [SerializeField, Range(-1, 1)] float _cellBondingX = 0;
        [SerializeField, Range(-1, 1)] float _cellBondingY = 0;

        // (( Cell Dimensions )) ------------------------------ >>
        [Header("-- Cell Dimensions ---- >>")]
        [SerializeField, Range(0.1f, 10f)] float _cellWidth = 1;
        [SerializeField, Range(0.1f, 10f)] float _cellHeight = 1;

        [Header("-- Cell Spacing ---- >>")]
        [SerializeField, Range(0, 10)] float _cellHorzSpacing = 0;
        [SerializeField, Range(0, 10)] float _cellVertSpacing = 0;

        // (( Gizmos )) ------------------------------ >>
        [Header("-- Gizmos ---- >>")]
        [SerializeField] bool _showGizmos = true;
        [SerializeField] bool _showEditorGizmos = true;


        public virtual GridMapConfig ToConfig()
        {
            GridMapConfig config = new GridMapConfig();

            config.SetGizmos(_showGizmos, _showEditorGizmos);

            config.SetLockToTransform(_lockToTransform);
            config.SetGridAlignment(_gridAlignment);
            config.SetGridNormal(_gridNormal);
            config.SetGridDimensions(new Vector2Int(_numColumns, _numRows));

            config.SetCellDimensions(new Vector2(_cellWidth, _cellHeight));
            config.SetCellSpacing(new Vector2(_cellHorzSpacing, _cellVertSpacing));
            config.SetCellBonding(new Vector2(_cellBondingX, _cellBondingY));

            return config;
        }

        public virtual void ResetToDefaults()
        {
            _lockToTransform = true;
            _gridAlignment = Alignment.Center;
            _gridNormal = Vector3.up;

            _numColumns = 3;
            _numRows = 3;

            _cellBondingX = 0;
            _cellBondingY = 0;

            _cellWidth = 1;
            _cellHeight = 1;

            _cellHorzSpacing = 0;
            _cellVertSpacing = 0;

            _showGizmos = true;
            _showEditorGizmos = true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GridMapConfig_DataObject))]
    public class GridMapConfig_DataObjectCustomEditor : UnityEditor.Editor
    {
        GridMapConfig_DataObject _script;

        public void OnEnable()
        {
            _script = target as GridMapConfig_DataObject;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Reset to Defaults"))
            {
                _script.ResetToDefaults();
            }

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif

}