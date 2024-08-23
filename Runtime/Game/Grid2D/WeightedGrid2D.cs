using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;


[ExecuteAlways]
public class WeightedGrid2D : MonoBehaviourGrid2D<WeightedGrid2D.WeightedCell>
{
    [System.Serializable]
    public class WeightedCell : Cell2D<WeightedCell.WeightedData>
    {
        [System.Serializable]
        public class WeightedData : Cell2D.Data
        {
            [SerializeField, ShowOnly] int _weight;
            public int weight { get => _weight; set => _weight = value; }

            public WeightedData() { }
            public WeightedData(Vector2Int key) : base(key) { }
        }

        public WeightedData weightedData { get => data as WeightedData; protected set => data = value; }

        public WeightedCell() { }
        public WeightedCell(Grid2D<WeightedCell> grid, Vector2Int key) : base(grid, key) { }
        public override void Initialize(AbstractGrid2D grid, Vector2Int key)
        {
            gridParent = grid;
            weightedData = new WeightedData(key);
        }

        protected override Color DetermineColor()
        {
            return Color.Lerp(Color.black, Color.white, weightedData.weight / 100f);
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
                disabled = true;
            }
            else
            {
                weightedData.weight += 10;
                disabled = false;
            }
        }

        public override void DrawGizmos(bool editMode)
        {
            base.DrawGizmos(editMode);
            DrawLabel();
        }

        protected override void DrawLabel()
        {
            weightedData.GetWorldSpaceData(out Vector3 position, out Vector2 dimensions, out Vector3 normal);
            Color color = DetermineColor();

            Handles.Label(position, $"{weightedData.key}\n{weightedData.weight}", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = color },
                alignment = TextAnchor.MiddleCenter
            });
        }
    }

    public class DataObject : Grid2D_DataObject<WeightedCell, WeightedCell.WeightedData> { }
    public override void GenerateDataObj()
    {
        dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<DataObject>(DATA_PATH, name);
    }
}

