using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;


[ExecuteAlways]
public class WeightedGrid2D : GenericMonoBehaviourGrid2D<
    WeightedGrid2D.WeightedCell, WeightedGrid2D.WeightedCell.WeightedData>
{
    #region -- << INTERNAL CLASS >> : WEIGHTEDCELL ------------------------------------ >>
    [System.Serializable]
    public class WeightedCell : Cell<WeightedCell.WeightedData>
    {
        [System.Serializable]
        public class WeightedData : Cell2D.Data
        {
            [SerializeField, ShowOnly] int _weight;
            public int weight { get => _weight; set => _weight = value; }

            public WeightedData() : base() { }
            public WeightedData(Vector2Int key) : base(key) { }
            public WeightedData(AbstractCellData data)
            {
                CopyFrom(data);
            }

            public override void CopyFrom(AbstractCellData data)
            {
                base.CopyFrom(data);
                if (data is WeightedData)
                {
                    _weight = (data as WeightedData).weight;
                }
            }
        }

        // =========================== [[ CONSTRUCTORS ]] =========================== >>
        public WeightedCell() : base() { }
        public WeightedCell(Vector2Int key, AbstractGrid2D.Config config) : base(key, config) { }

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
            DrawLabel($"WeightedCell {data.key}\n{data.weight}");
        }

        protected override void DrawLabel(string label)
        {
            GetGizmoData(out Vector3 position, out Vector2 dimensions, out Vector3 normal, out Color color);

            Handles.Label(position, $"{data.key}\n{data.weight}", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = color },
                alignment = TextAnchor.MiddleCenter
            });
        }
    }
    #endregion

    #region -- << INTERNAL CLASS >> : WEIGHTEDGRID2D_DATAOBJECT ------------------------------------ >>
    public class WeightedGrid2D_DataObject : Grid2D_GenericDataObject<WeightedCell, WeightedCell.WeightedData> { }
    #endregion

    protected new WeightedGrid2D_DataObject dataObj
    {
        get => base.dataObj as WeightedGrid2D_DataObject;
        set => base.dataObj = value;
    }
    protected override void GenerateDataObj()
    {
        dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<WeightedGrid2D_DataObject>(DATA_PATH, name);
    }
}

