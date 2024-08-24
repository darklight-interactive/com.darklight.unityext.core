using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{

    [System.Serializable]
    public class WeightedData : BaseCellData
    {
        [SerializeField, ShowOnly] int _weight;
        public int Weight { get => _weight; set => _weight = value; }

        public WeightedData() : base() { }
        public WeightedData(Vector2Int key) : base(key) { }

        public override void CopyFrom(BaseCellData data)
        {
            base.CopyFrom(data);
            if (data is WeightedData)
            {
                _weight = (data as WeightedData).Weight;
            }
        }
    }

    [System.Serializable]
    public class WeightedCell : GenericCell<WeightedData>
    {
        public new WeightedData data
        {
            get => base.data as WeightedData;
            set => base.data = value;
        }

        // =========================== [[ CONSTRUCTORS ]] =========================== >>
        public WeightedCell() : base() { }
        public WeightedCell(Vector2Int key) : base(key) { }

        protected override void GetGizmoColor(out Color color)
        {
            color = Color.Lerp(Color.black, Color.white, data.Weight / 100f);
        }

        protected override void OnEditToggle()
        {
            ToggleWeight();
        }

        void ToggleWeight()
        {
            if (data.Weight >= 100)
            {
                data.Weight = 0;
                data.SetDisabled(true);
            }
            else
            {
                data.Weight += 10;
                data.SetDisabled(false);
            }
        }

        public override void DrawGizmos(bool editMode)
        {
            if (data == null) return;

            base.DrawGizmos(editMode);
            DrawLabel($"WeightedCell {data.key}\n{data.Weight}");
        }

        protected override void DrawLabel(string label)
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            Handles.Label(position, $"{data.key}\n{data.Weight}", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = color },
                alignment = TextAnchor.MiddleCenter
            });
        }
    }

    public class WeightedGrid2D_DataObject : GenericGrid_DataObject<WeightedCell, WeightedData> { }




    [ExecuteAlways]
    public class WeightedGrid : GenericMonoBehaviourGrid<WeightedCell, WeightedData>
    {
        protected override void GenerateDataObj()
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<WeightedGrid2D_DataObject>(DATA_PATH, name);
        }
    }

}