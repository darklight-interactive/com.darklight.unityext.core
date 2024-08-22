using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Game;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class WeightedCell : Cell2D
{
    [SerializeField, ShowOnly] int _weight;
    public int weight { get => _weight; set => _weight = value; }

    public WeightedCell() { }
    public WeightedCell(AbstractGrid2D grid, Vector2Int key) : base(grid, key)
    {
        weight = 0;
    }

    public override void Refresh()
    {
        base.Refresh();
    }

    void ToggleWeight()
    {
        if (weight >= 100)
        {
            weight = 0;
            disabled = true;
        }
        else
        {
            weight += 10;
            disabled = false;
        }
    }
}

[ExecuteAlways]
public class WeightedGrid2D : MonoBehaviourGrid2D<WeightedCell>
{

}

