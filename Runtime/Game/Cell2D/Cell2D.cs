using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Cell2D
    {
        // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
        [SerializeField, ShowOnly] string _name = "Cell2D";
        [SerializeField] Cell2D_SerializedData _serializedData;

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Cell2D_SerializedData Data { get => _serializedData; }

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Cell2D(Vector2Int key)
        {
            _serializedData = new Cell2D_SerializedData(key);
            _name = $"Cell2D ({key.x},{key.y})";
        }

        // ======== [[ METHODS ]] ============================================================ >>>>

        // (( RUNTIME )) -------- )))
        public void Update()
        {
            if (_serializedData == null) return;
            if (_serializedData.Components == null) return;

            // Update the components
            foreach (ICell2DComponent component in _serializedData.Components)
                component.Update();
        }

        // (( VISITOR PATTERN )) -------- ))
        public void Accept(ICell2DVisitor visitor)
        {
            visitor.VisitCell(this);
        }

        // (( SERIALIZATION )) -------- ))
        public virtual void SetData(Cell2D_SerializedData data) => _serializedData = data;

        // (( GETTERS )) -------- ))
        public float GetMinDimension() => Mathf.Min(Data.Dimensions.x, Data.Dimensions.y);

        public void GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions)
        {
            position = Data.Position;
            normal = Data.Normal;
            dimensions = Data.Dimensions;
        }

        public void GetTransformData(out Vector3 position, out float radius, out Vector3 normal)
        {
            position = Data.Position;
            radius = GetMinDimension() / 2;
            normal = Data.Normal;
        }

        // (( SETTERS )) -------- ))

        // (( GIZMOS )) -------- ))
        public void DrawGizmos()
        {
            if (_serializedData == null) return;

            GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawWireSquare(position, radius, normal, Color.gray);

            if (_serializedData.Components == null) return;
            foreach (ICell2DComponent component in _serializedData.Components)
                component.DrawGizmos();
        }

        public void DrawEditorGizmos()
        {
            if (_serializedData == null) return;
            if (_serializedData.Components == null) return;

            foreach (ICell2DComponent component in _serializedData.Components)
                component.DrawEditorGizmos();
        }
    }
}
