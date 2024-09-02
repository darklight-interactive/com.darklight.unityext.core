using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_ConfigComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Grid2D_ConfigComponent(Grid2D grid) : base(grid) { }

        // ======== [[ METHODS ]] ======================================================= >>>>

        #region  -- (( UNITY RUNTIME )) ------------------ >>
        public override void Initialize()
        {
            // Assign the grid's config from the config object
            Grid2D.Config config = _configObj.CreateGridConfig();
            if (config.LockToTransform)
            {
                // Set the grid's position and normal to the transform's position and forward
                config.SetGridPosition(transform.position);
                config.SetGridNormal(transform.forward);
            }

            Base.SetConfig(config);
        }

        public override void Update()
        {

        }
        public override void DrawGizmos() { }
        public override void DrawEditorGizmos() { }

        #endregion

    }
}