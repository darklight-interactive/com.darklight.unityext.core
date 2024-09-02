using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_WeightComponent : Grid2D_Component
    {
        const int MIN_WEIGHT = 0;
        const int MAX_WEIGHT = 100;

        // ======== [[ FIELDS ]] ================================== >>>>
        [SerializeField] bool _showGizmos;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        // -- (( VISITORS )) -------- ))
        Cell2D.Visitor _registrationVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            cell.ComponentReg.RegisterComponent(Cell2D_Component.Type.WEIGHT);

            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
        });

        Cell2D.Visitor _randomizeVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent != null)
            {
                weightComponent.SetWeight(Random.Range(MIN_WEIGHT, MAX_WEIGHT));
            }
        });

        Cell2D.Visitor _gizmosVisitor => new Cell2D.Visitor((Cell2D cell) =>
        {
            Cell2D_WeightComponent weightComponent = cell.ComponentReg.GetComponent<Cell2D_WeightComponent>();
            if (weightComponent != null)
            {
                int weight = weightComponent.GetWeight();
                Color color = GetColorForWeight(weight);
                weightComponent.SetGizmoColor(color);
                weightComponent.DrawGizmos();
            }
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

        // -- (( HELPER METHODS )) -------- ))
        Color GetColorForWeight(int weight)
        {
            return Color.Lerp(Color.red, Color.green, (float)weight / MAX_WEIGHT);
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

