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
        Cell2D.Visitor _registrationVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.RegisterComponent(Cell2D_Component.Type.WEIGHT);
        });

        Cell2D.Visitor _updateVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent == null)
            {
                cell.Accept(_registrationVisitor);
                return;
            }

            weightComponent.UpdateComponent();
        });

        Cell2D.Visitor _randomizeVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent == null) return;

            weightComponent.SetRandomWeight();
        });

        Cell2D.Visitor _gizmosVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent == null) return;

            weightComponent.DrawGizmos();
        });

        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( INTERFACE METHODS )) -------- ))
        public override void InitializeComponent(Grid2D baseObj)
        {
            base.InitializeComponent(baseObj);
            baseObj.SendVisitorToAllCells(_registrationVisitor);
        }
        public override void UpdateComponent()
        {
            baseGrid.SendVisitorToAllCells(_updateVisitor);
        }

        public override Type GetTypeTag() => Type.WEIGHT;

        public override void DrawGizmos()
        {
            if (!_showGizmos) return;
            baseGrid.SendVisitorToAllCells(_gizmosVisitor);
        }

        // -- (( HANDLER METHODS )) -------- ))
        public void RandomizeWeights()
        {
            baseGrid.SendVisitorToAllCells(_randomizeVisitor);
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

