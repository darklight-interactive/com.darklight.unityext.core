using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [ExecuteAlways]
    [RequireComponent(typeof(Grid2D))]
    public class Grid2D_Component : MonoBehaviour
    {


        [SerializeField, EnumFlags] Cell2D.ComponentFlags _cellComponentFlags = Cell2D.ComponentFlags.Shape;

        Grid2D _grid => GetComponent<Grid2D>();
        Cell2D.Visitor _componentInitializer => new Cell2D.Visitor(cell =>
        {
            cell.InitializeComponents(_cellComponentFlags);
        });

        void Awake()
        {
            _grid.OnGridInitialized += () =>
            {
                _grid.SendVisitorToAllCells(_componentInitializer);
            };
        }


    }
}
