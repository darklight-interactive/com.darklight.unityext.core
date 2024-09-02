

using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{


    public class Cell2D_BaseComponent : Cell2D.Component
    {
        public Cell2D_BaseComponent(Cell2D cell) : base(cell) { }

        public override void Initialize(Cell2D baseObj)
        {
            base.Initialize(baseObj);
        }

        public override void Updater() { }
        public override void DrawGizmos() { }
        public override void DrawEditorGizmos() { }
        public override Cell2D.ComponentTypeKey GetTypeKey() => Cell2D.ComponentTypeKey.BASE;
    }
}