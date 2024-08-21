using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class WeightedGrid2D : MonoBehaviourGrid2D<WeightedGrid2D.WeightedCell>
{
    [System.Serializable]
    public class WeightedCell : Grid2D.Cell<WeightedCell.Data>
    {
        [System.Serializable]
        public new class Data : Grid2D.Cell.Data, IWeightedData
        {
            [SerializeField, ShowOnly] int _weight;
            public int weight { get => _weight; set => _weight = value; }

            public Data(Grid2D grid, Vector2Int key) : base(grid, key)
            {
                weight = 0;
            }

            public override void Refresh()
            {
                base.Refresh();
                SetColor();
            }

            protected override void SetColor()
            {
                base.SetColor();

                if (disabled) return;
                color = Color.Lerp(Color.red, Color.green, (float)_weight / 100);
            }
        }

        public new Data data => (Data)base.data;

        public WeightedCell() { }
        public WeightedCell(Grid2D grid, Vector2Int key) : base(grid, key) { }

        void ToggleWeight()
        {
            if (data.weight >= 100)
            {
                data.weight = 0;
                data.disabled = true;
            }
            else
            {
                data.weight += 10;
                data.disabled = false;
            }
        }

        public override void DrawGizmos(bool editMode = false)
        {
            // Draw the cell square
            CustomGizmos.DrawWireRect(data.position, data.dimensions, data.normal, data.color);

            string label = $"{data.key}\n{data.weight}";
            CustomGizmos.DrawLabel(label, data.position, CustomGUIStyles.CenteredStyle);

            if (editMode)
            {
                float size = Mathf.Min(data.dimensions.x, data.dimensions.y);
                size *= 0.75f;

                // Draw the button handle only if the grid is in edit mode
                CustomGizmos.DrawButtonHandle(data.position, size, data.normal, data.color, () =>
                {
                    ToggleWeight();
                    data.Refresh();
                }, Handles.RectangleHandleCap);
            }
        }
    }

    public class WeightedGridDataObject : Grid2D_DataObject<WeightedCell> { }
    public override void InitializeGrid()
    {
        base.InitializeGrid();

        if (dataObj == null)
        {
            dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<WeightedGridDataObject>(DATA_PATH, "WeightedGridDataObject");
            dataObj.Initialize(grid);
        }

        // Apply random weights to each cell
        grid.cellMap.ModifyCells((cell) =>
        {
            if (cell is WeightedCell weightedCell)
            {
                //weightedCell.data = new WeightedCell.Data(grid, weightedCell.data.key);
                weightedCell.data.weight = Random.Range(0, 100);
                weightedCell.data.Refresh();
            }
            return cell;
        });

    }
}

