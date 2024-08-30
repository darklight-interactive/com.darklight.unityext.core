using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICellVisitor
    {
        void Visit(AbstractCell cell);
    }

    public class CellVisitor
    {
    }
}
