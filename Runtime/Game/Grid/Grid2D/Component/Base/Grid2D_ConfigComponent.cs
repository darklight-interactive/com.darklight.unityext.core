using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_ConfigComponent : Grid2D_BaseComponent
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;

        // ======== [[ METHODS ]] ======================================================= >>>>
        public override void OnUpdate()
        {
            base.OnUpdate();
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