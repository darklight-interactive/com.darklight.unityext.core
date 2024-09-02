using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_WeightComponent : Grid2D_Component
    {
        // ======== [[ FIELDS ]] ================================== >>>>
        [SerializeField] bool _showGizmos;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        // -- (( VISITORS )) -------- ))
        protected override Cell2D.ComponentVisitor GizmosVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        protected override Cell2D.ComponentVisitor EditorGizmosVisitor =>
            new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        Cell2D.ComponentVisitor _updateVisitor => new Cell2D.ComponentVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        Cell2D.Visitor _randomizeVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent == null) return;

            weightComponent.SetRandomWeight();
        });



        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( INTERFACE METHODS )) -------- ))
        public override void Updater()
        {
            BaseGrid.SendVisitorToAllCells(_updateVisitor);
        }

        public override void DrawGizmos()
        {
            if (!_showGizmos) return;
            BaseGrid.SendVisitorToAllCells(GizmosVisitor);
        }

        // -- (( HANDLER METHODS )) -------- ))
        public void RandomizeWeights()
        {
            BaseGrid.SendVisitorToAllCells(_randomizeVisitor);
        }

        public void AddWeightToCell(Cell2D cell, int weight)
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            weightComponent.AddWeight(weight);
        }

        public void RemoveWeightFromCell(Cell2D cell, int weight)
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            weightComponent.SubtractWeight(weight);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Grid2D_WeightComponent))]
        public class Grid2D_WeightComponentCustomEditor : UnityEditor.Editor
        {
            SerializedObject _serializedObject;
            Grid2D_WeightComponent _script;
            private void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (Grid2D_WeightComponent)target;
                _script.Awake();
            }

            public override void OnInspectorGUI()
            {
                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                base.OnInspectorGUI();

                if (GUILayout.Button("Randomize Weights"))
                {
                    _script.RandomizeWeights();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                }
            }
        }
#endif
    }
}

