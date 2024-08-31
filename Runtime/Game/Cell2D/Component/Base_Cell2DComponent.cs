using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Base_Cell2DComponent : Abstract_Cell2DComponent, ICell2DComponent
    {
        public Base_Cell2DComponent(Cell2D cell) => Initialize(cell);

        public override void Initialize(Cell2D cell)
        {
            base.Initialize(cell);
            this.Name = "Base2DComponent";
            this.Type = ICell2DComponent.TypeKey.Base;
        }

        public void Update()
        {
            if (!initialized) return;
        }

        public void DrawGizmos()
        {
            if (!initialized) return;
        }

        public void DrawEditorGizmos()
        {
            if (!initialized) return;
        }
    }
}