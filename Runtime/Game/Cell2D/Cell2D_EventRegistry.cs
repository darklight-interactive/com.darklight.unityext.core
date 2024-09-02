
namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        public static class EventRegistry
        {
            public delegate void VisitCellEvent(Cell2D cell);
            public delegate void VisitCellComponentEvent(Cell2D cell, ComponentTypeKey componentTypeKey);

        }
    }
}