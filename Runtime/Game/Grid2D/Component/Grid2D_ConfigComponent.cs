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

            // Assign the grid's config from the config object
            Grid2D.Config config = _configObj.CreateGridConfig();
            if (config.LockToTransform)
            {
                // Set the grid's position and normal to the transform's position and forward
                config.SetGridPosition(transform.position);
                config.SetGridNormal(transform.forward);
            }
            baseObj.SetConfig(config);
        }

        public override void UpdateComponent() { }
        public override void DrawGizmos() { }
        public override void DrawEditorGizmos() { }
        public override TypeTag GetTypeTag() => TypeTag.CONFIG;

        #endregion

    }
}