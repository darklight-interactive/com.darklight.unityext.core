using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICellVisitor
    {
        void Visit(BaseCell cell);
    }

    public class CellVisitor
    {
    }
}
