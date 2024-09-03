using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_SpawnerComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField] Sprite _sprite;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        // -- (( BASE VISITORS )) -------- ))
        protected override Cell2D.ComponentVisitor InitVisitor =>
            Cell2D.VisitorFactory.CreateInitVisitor(Cell2D.ComponentTypeKey.SPAWNER);
        protected override Cell2D.ComponentVisitor UpdateVisitor =>
            Cell2D.VisitorFactory.CreateBaseUpdateVisitor(Cell2D.ComponentTypeKey.SPAWNER);
        protected override Cell2D.ComponentVisitor GizmosVisitor =>
            Cell2D.VisitorFactory.CreateBaseGizmosVisitor(Cell2D.ComponentTypeKey.SPAWNER);
        protected override Cell2D.ComponentVisitor EditorGizmosVisitor =>
            Cell2D.VisitorFactory.CreateBaseEditorGizmosVisitor(Cell2D.ComponentTypeKey.SPAWNER);

        // -- (( CUSTOM VISITORS )) -------- ))
        private Cell2D.ComponentVisitor _spawnVisitor => Cell2D.VisitorFactory.CreateComponentVisitor
            (Cell2D.ComponentTypeKey.SPAWNER, (Cell2D cell, Cell2D.ComponentTypeKey type) =>
            {
                Cell2D_SpawnerComponent spawnerComponent = cell.ComponentReg.GetComponent<Cell2D_SpawnerComponent>();


                return true;
            });

    }
}