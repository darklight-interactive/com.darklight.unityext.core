namespace Darklight.UnityExt.Game.Grid
{
    public class Cell2D_BaseComponent : Cell2D_Component
    {
        public Cell2D_BaseComponent(Cell2D cell) : base(cell) { }

        public override void InitializeComponent(Cell2D baseObj)
        {
            base.InitializeComponent(baseObj);
        }

        public override void UpdateComponent() { }
        public override void DrawGizmos() { }
        public override void DrawEditorGizmos() { }
        public override Type GetTypeTag() => Type.BASE;
    }
}