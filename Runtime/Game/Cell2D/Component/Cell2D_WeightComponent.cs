using Darklight.UnityExt.Editor;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Cell2D_WeightComponent : Cell2D_Component
    {
        [SerializeField, ShowOnly] int _weight;
        Color _gizmoColor = Color.white;

        public Cell2D_WeightComponent(Cell2D cell) : base(cell)
        {
            _weight = 0;
        }

        public override void UpdateComponent()
        {
        }

        public override Type GetTypeTag() => Type.WEIGHT;

        public override void DrawGizmos()
        {
            baseCell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawFilledSquare(position, radius, normal, _gizmoColor);
            CustomGizmos.DrawLabel($"Weight: {_weight}", position, CustomGUIStyles.CenteredStyle);
        }

        public override void DrawEditorGizmos()
        {
            baseCell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawButtonHandle(position, radius, normal, Color.white, () =>
            {
                _weight += 5;
                if (_weight > 100) _weight = 0;
                if (_weight < 0) _weight = 0;

            }, Handles.RectangleHandleCap);
        }

        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        public int GetWeight()
        {
            return _weight;
        }

        public void SetGizmoColor(Color color)
        {
            _gizmoColor = color;
        }
    }
}