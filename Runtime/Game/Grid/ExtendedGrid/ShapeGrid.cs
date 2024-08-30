using System.Collections.Generic;
using Darklight.UnityExt.Game.Grid;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class ShapeData : BaseCellData
    {
        public Shape2D shape;
        public int segments = 6;
        public ShapeData() : base() { }
        public ShapeData(Vector2Int key) : base(key) { }

    }

    [System.Serializable]
    public class ShapeCell : BaseCell<ShapeData>
    {
        public new ShapeData data
        {
            get => base.data as ShapeData;
            set => base.data = value;
        }

        public ShapeCell() : base() { }
        public ShapeCell(Vector2Int key) : base(key) { }

        public override void Update()
        {
            GetGizmoColor(out Color color);

            if (data.shape == null)
                data.shape = new Shape2D(data.position, data.GetMinDimension() / 2, data.segments, data.normal, color);
            data.shape.radius = data.GetMinDimension() / 2;
        }

        public override void DrawGizmos(bool editMode)
        {
            if (data.shape == null) return;
            data.shape.DrawGizmos();
            DrawLabel($"Shape Cell\n{data.coordinate}");
        }
    }

    public class ShapeGrid_DataObject : BaseGridDataObject<ShapeData>
    {
        [Header("Shape Grid Data")]
        [Range(3, 12)] public int segments = 6;
    }

    public class ShapeGrid : GenericGridMonoBehaviour<ShapeCell, ShapeData>
    {
        protected override void GenerateDataObj()
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<ShapeGrid_DataObject>(DATA_PATH, name);
        }

        public override void UpdateGrid()
        {
            base.UpdateGrid();
            List<ShapeData> dataList = grid.GetData<ShapeData>();
            if (dataList == null || dataList.Count == 0) return;

            foreach (ShapeData data in dataList)
            {
                data.segments = (dataObj as ShapeGrid_DataObject).segments;
            }

        }
    }
}

