using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game.Grid;
using UnityEngine;


[System.Serializable]
public class MultiCell : BaseCell, IOverlap, IShape, IWeighted
{
    Collider2D[] _colliders;
    [SerializeField] LayerMask _layerMask;
    Shape2D _shape;
    [SerializeField, ShowOnly] int _weight;

    public List<Collider2D> Colliders { get => _colliders.ToList(); set => _colliders = value.ToArray(); }
    public LayerMask LayerMask { get => _layerMask; set => _layerMask = value; }
    public Shape2D Shape { get => _shape; set => _shape = value; }
    public int Weight { get => _weight; set => _weight = value; }

    public MultiCell() : base() { }
    public MultiCell(Vector2Int key) : base(key) { }

    public override void Update()
    {
        UpdateColliders();
        UpdateShape(Data.position, Data.GetMinDimension() / 2, 6, Data.normal, Color.white);
        UpdateWeight();
    }

    public override void ApplyConfig<TConfig>(TConfig config)
    {
        base.ApplyConfig(config);
    }

    public void UpdateColliders()
    {
        Vector3 cellCenter = Data.position;
        Vector3 halfExtents = new Vector3(Data.dimensions.x / 2, 1f, Data.dimensions.y / 2);

        // Use Physics.OverlapBox to detect colliders within the cell dimensions
        _colliders = Physics2D.OverlapBoxAll(cellCenter, halfExtents, 0, LayerMask);
    }

    public void UpdateShape(Vector3 center, float radius, int segments, Vector3 normal, Color gizmoColor)
    {
        _shape = new Shape2D(center, radius, segments, normal, gizmoColor);
    }

    public void UpdateWeight()
    {
        _weight = _colliders.Length;
    }

    protected override void GetGizmoColor(out Color color)
    {
        color = Color.red;
        if (_colliders.Length > 0)
            color = Color.green;
    }


    public override void DrawGizmos(bool editMode)
    {
        GetGizmoColor(out Color color);
        Shape.SetGizmoColor(color);
        Shape.DrawGizmos(false);
        DrawLabel($"MultiCell\n{Data.coordinate}");

    }
}

public class MultiGridConfigDataObject : GridConfigDataObject
{
    [Header("Multi Grid Data")]
    public LayerMask layerMask;
    public int segments = 6;
}


public class MultiGrid : GenericGridMonoBehaviour<MultiCell>
{
    protected override void GenerateConfigObj()
    {
        if (configObj != null)
        {
            if (configObj is MultiGridConfigDataObject multiConfig) return;
            ScriptableObjectUtility.DeleteScriptableObject(CONFIG_PATH, name);
        }

        configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<MultiGridConfigDataObject>(CONFIG_PATH, name);
    }

    public override void UpdateGrid()
    {
        base.UpdateGrid();

        grid.MapFunction(cell =>
        {
            MultiGridConfigDataObject multiGridConfig = configObj as MultiGridConfigDataObject;
            cell.LayerMask = multiGridConfig.layerMask;
            cell.Shape.SetSegments(multiGridConfig.segments);
            return cell;
        });
    }

}