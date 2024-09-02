using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Cell2D_SpawnerComponent : Cell2D.Component
    {
        public Cell2D_SpawnerComponent(Cell2D cell) : base(cell) { }

        public override Cell2D.ComponentTypeKey GetTypeKey()
        {
            throw new System.NotImplementedException();
        }

        public override void Updater()
        {
            throw new System.NotImplementedException();
        }
    }
}