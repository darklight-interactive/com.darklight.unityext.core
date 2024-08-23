using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;


[ExecuteAlways]
public class WeightedGrid2D : GenericMonoBehaviourGrid2D<
    WeightedGrid2D.WeightedCell, WeightedGrid2D.WeightedCell.WeightedData>
{
    public class WeightedGrid : GenericGrid2D<WeightedCell, WeightedCell.WeightedData>
    {
        public WeightedGrid(Config config) : base(config) { }
    }

    [System.Serializable]
    public class WeightedCell : Cell<WeightedCell.WeightedData>
    {
        [System.Serializable]
        public class WeightedData : Cell.Data
        {
            [SerializeField, ShowOnly] int _weight;
            public int weight { get => _weight; set => _weight = value; }

            public WeightedData() { }
            public WeightedData(Vector2Int key) : base(key) { }
        }

        // =========================== [[ CONSTRUCTORS ]] =========================== >>
        public WeightedCell() { }
        public WeightedCell(WeightedData data) : base(data) { }
        public WeightedCell(Vector2Int key) : base(key) { }
        public WeightedCell(Vector2Int key, AbstractGrid2D.Config config) : base(key, config) { }


        // =========================== [[ OVERRIDES ]] =========================== >>
        public WeightedData weightedData { get => data as WeightedData; protected set => data = value; }

        protected override void Initialize(WeightedCell.Data data)
        {
            if (data is WeightedData)
                weightedData = data as WeightedData;
            else
                weightedData = new WeightedData(data.key);
        }

        protected override void GetGizmoColor(out Color color)
        {
            color = Color.Lerp(Color.black, Color.white, weightedData.weight / 100f);
        }

        protected override void OnEditToggle()
        {
            ToggleWeight();
        }

        void ToggleWeight()
        {
            if (weightedData.weight >= 100)
            {
                weightedData.weight = 0;
                weightedData.SetDisabled(true);
            }
            else
            {
                weightedData.weight += 10;
                weightedData.SetDisabled(false);
            }
        }

        public override void DrawGizmos(bool editMode)
        {
            base.DrawGizmos(editMode);
            DrawLabel($"WeightedCell {weightedData.key}\n{weightedData.weight}");
        }

        protected override void DrawLabel(string label)
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            Handles.Label(position, $"{weightedData.key}\n{weightedData.weight}", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = color },
                alignment = TextAnchor.MiddleCenter
            });
        }
    }

    public class DataObject : Grid2D_DataObject<WeightedCell, WeightedCell.WeightedData> { }
    protected override void GenerateDataObj()
    {
        dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<DataObject>(DATA_PATH, name);
    }
}

