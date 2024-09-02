using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_ConfigComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        protected override Cell2D.ComponentVisitor GizmosVisitor =>
            Cell2D.VisitorFactory.CreateGizmosVisitor(Cell2D.ComponentTypeKey.BASE);
        protected override Cell2D.ComponentVisitor EditorGizmosVisitor =>
            Cell2D.VisitorFactory.CreateEditorGizmosVisitor(Cell2D.ComponentTypeKey.BASE);

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