using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    #region -- << CLASS >> : WEIGHTEDDATA ------------------------------------ >>
    [System.Serializable]
    public class WeightedData : BaseCellData, IWeightedData
    {
        [SerializeField, ShowOnly] int _weight;
        public int weight { get => _weight; set => _weight = value; }

        public WeightedData() : base() { }
        public WeightedData(Vector2Int key) : base(key) { }

        public override void CopyFrom(BaseCellData data)
        {
            base.CopyFrom(data);
            if (data is WeightedData)
            {
                _weight = (data as WeightedData).weight;
            }
        }
    }
    #endregion

    #region -- << CLASS >> : WEIGHTEDCELL ---------------------------------
    [System.Serializable]
    public class WeightedCell : BaseCell<WeightedData>
    {
        public new WeightedData data
        {
            get => base.data as WeightedData;
            set => base.data = value;
        }

        // =========================== [[ CONSTRUCTORS ]] =========================== >>
        public WeightedCell() : base() { }
        public WeightedCell(Vector2Int key) : base(key) { }

        public override void Update() { }

        protected override void GetGizmoColor(out Color color)
        {
            color = Color.Lerp(Color.black, Color.white, data.weight / 100f);
        }

        protected override void OnEditToggle()
        {
            ToggleWeight();
        }

        void ToggleWeight()
        {
            if (data.weight >= 100)
            {
                data.weight = 0;
                data.SetDisabled(true);
            }
            else
            {
                data.weight += 10;
                data.SetDisabled(false);
            }
        }

        public override void DrawGizmos(bool editMode)
        {
            if (data == null) return;

            base.DrawGizmos(editMode);
            DrawLabel($"WeightedCell\n{data.coordinate}\n{data.weight}");
        }

        protected override void DrawLabel(string label)
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            Handles.Label(position, label, new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = color },
                alignment = TextAnchor.MiddleCenter
            });
        }
    }
    #endregion

    #region -- << SCRIPTABLE OBJECT >> : WEIGHTEDGRIDDATAOBJECT ------------    
    public class WeightedGridDataObject : BaseGridDataObject<WeightedData> { }
    #endregion

    #region -- << CLASS >> : WEIGHTEDGRID ------------------------------------
    [ExecuteAlways]
    public class WeightedGrid : GenericGridMonoBehaviour<WeightedCell, WeightedData>
    {
        protected override void GenerateDataObj()
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<WeightedGridDataObject>(DATA_PATH, name);
        }

        public WeightedData GetRandomDataByWeight()
        {
            if (grid == null) return null;

            List<WeightedData> dataList = grid.GetData<WeightedData>();
            WeightedData randData = WeightedDataSelector.SelectRandomWeightedItem(dataList, data => data);
            return randData;
        }
    }
    #endregion

#if UNITY_EDITOR
    [CustomEditor(typeof(WeightedGrid))]
    public class WeightedGridCustomEditor : GridMonoBehaviourEditor
    {
        WeightedGrid _weightedGridScript;
        protected override void OnEnable()
        {
            base.OnEnable();
            _weightedGridScript = (WeightedGrid)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustomInspectorGUI.DrawHorizontalLine(Color.gray, 4, 10);
            if (GUILayout.Button("Print Random Data By Weight"))
            {
                WeightedData data = _weightedGridScript.GetRandomDataByWeight();
                Debug.Log($"[WEIGHTED GRID] Random Data: Coord {data.coordinate} - Weight {data.weight}");
            }
        }

        protected override void OnSceneGUI()
        {
            if (_weightedGridScript)
                _weightedGridScript.DrawGizmos();
        }
    }
#endif

}