using Darklight.UnityExt.Editor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Weighted_Cell2DComponent : Abstract_Cell2DComponent, ICell2DComponent
    {
        [SerializeField, Range(0, 100)] int _weight;
        public int Weight { get => _weight; }

        public Weighted_Cell2DComponent() { }
        public Weighted_Cell2DComponent(Cell2D cell)
        {
            _weight = 0;
            Initialize(cell);
        }
        public Weighted_Cell2DComponent(Cell2D cell, Weighted_Cell2DComponent template)
        {
            _weight = template.Weight;
            Initialize(cell);
        }

        public override void Initialize(Cell2D cell)
        {
            base.Initialize(cell);
            Name = "WeightComponent";
            Type = ICell2DComponent.TypeKey.Weight;
        }

        public void Update() { }

        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        public void DrawGizmos()
        {
            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawLabel($"Weight: {_weight}", position, CustomGUIStyles.BoldCenteredStyle);
        }

#if UNITY_EDITOR
        public void DrawEditorGizmos()
        {
            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawButtonHandle(position, radius, normal, Color.yellow, () =>
            {
                _weight += 5;
                if (_weight > 100) _weight = 0;
                if (_weight < 0) _weight = 0;

            }, Handles.RectangleHandleCap);
        }
#endif
    }
}