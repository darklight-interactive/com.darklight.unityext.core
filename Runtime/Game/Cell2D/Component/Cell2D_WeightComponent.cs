using Darklight.UnityExt.Editor;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Cell2D_WeightComponent : Cell2D.Component
    {
        [SerializeField, Range(0, 100)] int _weight;
        public int Weight { get => _weight; }

        public Cell2D_WeightComponent(Cell2D cell) : base(cell)
        {
            _weight = 0;
        }

        public override void UpdateComponent()
        {

        }

        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        public override TypeTag GetTypeTag() => TypeTag.WEIGHT;

        public override void DrawGizmos()
        {
            Base.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawLabel($"Weight: {_weight}", position, CustomGUIStyles.CenteredStyle);
        }

        public override void DrawEditorGizmos()
        {
            Base.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawButtonHandle(position, radius, normal, Color.white, () =>
            {
                _weight += 5;
                if (_weight > 100) _weight = 0;
                if (_weight < 0) _weight = 0;

            }, Handles.RectangleHandleCap);
        }
    }
}