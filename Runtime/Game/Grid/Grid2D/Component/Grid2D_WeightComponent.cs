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
        // -- (( BASE VISITORS )) -------- ))
        protected override Cell2D.ComponentVisitor InitVisitor =>
            Cell2D.VisitorFactory.CreateInitVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        protected override Cell2D.ComponentVisitor UpdateVisitor =>
            Cell2D.VisitorFactory.CreateBaseUpdateVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        protected override Cell2D.ComponentVisitor GizmosVisitor =>
            Cell2D.VisitorFactory.CreateBaseGizmosVisitor(Cell2D.ComponentTypeKey.WEIGHT);
        protected override Cell2D.ComponentVisitor EditorGizmosVisitor =>
            Cell2D.VisitorFactory.CreateBaseEditorGizmosVisitor(Cell2D.ComponentTypeKey.WEIGHT);

        // -- (( CUSTOM VISITORS )) -------- ))
        private Cell2D.ComponentVisitor _randomizeVisitor => Cell2D.VisitorFactory.CreateComponentVisitor
            (Cell2D.ComponentTypeKey.WEIGHT, (Cell2D cell, Cell2D.ComponentTypeKey type) =>
            {
                Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
                weightComponent.SetRandomWeight();
                return true;
            });

        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( INTERFACE )) : IComponent -------- ))
        public override void DrawGizmos()
        {
            if (!_showGizmos) return;
            base.DrawGizmos();
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

