namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_BaseComponent : Grid2D_Component
    {
        protected override Cell2D.ComponentVisitor GizmosVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.BASE);
        protected override Cell2D.ComponentVisitor EditorGizmosVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.BASE);
    }
}