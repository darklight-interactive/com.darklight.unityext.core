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
        const int MIN_WEIGHT = 0;
        const int MAX_WEIGHT = 100;

        [SerializeField, ShowOnly] int _weight;

        // ======== [[ CONSTRUCTORS ]] =========================== >>>>
        public Cell2D_WeightComponent(Cell2D cell) : base(cell)
        {
            _weight = 0;
        }

        // ======== [[ METHODS ]] ================================== >>>>
        // -- (( INTERFACE METHODS )) -------- ))
        public override void Updater() { }
        public override Cell2D.ComponentTypeKey GetTypeKey() => Cell2D.ComponentTypeKey.WEIGHT;

        public override void DrawGizmos()
        {
            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawFilledSquare(position, radius, normal, GetColor());

            GUIStyle style = new GUIStyle()
            {
                fontSize = 12,
                normal = new GUIStyleState() { textColor = GetInverseColor() }
            };

            CustomGizmos.DrawLabel($"Weight: {_weight}", position, style);
        }

        public override void DrawEditorGizmos()
        {
            Cell.GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawButtonHandle(position, radius, normal, Color.white, () =>
            {
                _weight += 5;
                if (_weight > 100) _weight = 0;
                if (_weight < 0) _weight = 0;

            }, Handles.RectangleHandleCap);
        }

        // -- (( GETTERS )) -------- ))
        public int GetWeight()
        {
            return _weight;
        }

        Color GetColor()
        {
            return Color.Lerp(Color.black, Color.white, (float)_weight / MAX_WEIGHT);
        }

        Color GetInverseColor()
        {
            return Color.Lerp(Color.white, Color.black, (float)_weight / MAX_WEIGHT);
        }

        // -- (( SETTERS )) -------- ))
        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        public void SetRandomWeight()
        {
            _weight = UnityEngine.Random.Range(MIN_WEIGHT, MAX_WEIGHT);
        }

        // -- (( HANDLER METHODS )) -------- ))
        public void AddWeight(int amount)
        {
            _weight += amount;
        }

        public void SubtractWeight(int amount)
        {
            _weight -= amount;
        }
    }
}