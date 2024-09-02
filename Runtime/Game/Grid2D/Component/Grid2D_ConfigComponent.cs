using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_ConfigComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;

        // ======== [[ METHODS ]] ======================================================= >>>>

        #region  -- (( UNITY RUNTIME )) ------------------ >>
        public override void InitializeComponent(Grid2D baseObj)
        {
            base.InitializeComponent(baseObj);
            RefreshConfig();
        }

        public override void UpdateComponent()
        {
            RefreshConfig();
        }

        public override void DrawGizmos() { }
        public override void DrawEditorGizmos() { }
        public override Type GetTypeTag() => Type.CONFIG;

        #endregion

        void RefreshConfig()
        {
            // Assign the grid's config from the config object
            Grid2D.Config newConfig = _configObj.CreateGridConfig();
            baseGrid.SetConfig(newConfig);
        }

    }
}