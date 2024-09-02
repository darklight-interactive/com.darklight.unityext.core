namespace Darklight.UnityExt.Game.Grid
{
    public class Cell2D_BaseComponent : Cell2D.Component
    {
        public Cell2D_BaseComponent(Cell2D cell) : base(cell) { }

        public override void Initialize()
        {
            Tag = Type.BASE;
        }

        public override void Update()
        {
        }

        public override void DrawGizmos()
        {
        }

        public override void DrawEditorGizmos()
        {
        }
    }
}