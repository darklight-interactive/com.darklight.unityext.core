using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface IGrid2D_Component
    {
        Grid2D Grid { get; }
    }

    [System.Serializable]
    public abstract class Grid2D_AbstractComponent : MonoBehaviour
    {
        // (( CELL2D COMPONENTS )) ---- >>
        [SerializeField, EnumFlags] Cell2D.ComponentFlags _cellComponentFlags = 0;
    }

    public class Grid2D_Component : Grid2D_AbstractComponent
    {

    }
}
