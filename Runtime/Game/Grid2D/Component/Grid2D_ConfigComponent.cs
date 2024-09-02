using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_ConfigComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        protected override Cell2D.ComponentVisitor CellComponentVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.BASE);

        // ======== [[ METHODS ]] ======================================================= >>>>
        public override void Updater()
        {
            RefreshConfig();
        }

        void RefreshConfig()
        {
            // Assign the grid's config from the config object
            Grid2D.Config newConfig = _configObj.CreateGridConfig();
            BaseGrid.SetConfig(newConfig);
        }

    }
}