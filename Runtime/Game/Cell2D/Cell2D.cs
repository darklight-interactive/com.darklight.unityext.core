using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
namespace Darklight.UnityExt.Game.Grid
{
    interface ICell
    {
        string Name { get; }
        Cell2D_SerializedData Data { get; }
        List<ICell2DComponent> Components { get; }

        void Accept(ICell2DVisitor visitor);
        void SetData(Cell2D_SerializedData data);

        void AssignComponent(ICell2DComponent component);
        void RemoveComponent(ICell2DComponent component);
    }

    [System.Serializable]
    public class Cell2D : ICell
    {
        [SerializeField, ShowOnly] string _name;
        [SerializeField] Cell2D_SerializedData _data;


        public string Name => Data.Name;
        public Cell2D_SerializedData Data { get => _data; private set => _data = value; }
        public List<ICell2DComponent> Components { get => _data.Components; }


        public Cell2D(Vector2Int key)
        {
            Data = new Cell2D_SerializedData(key);
            Data.Initialize(key);

            _name = Data.Name;
        }

        public void Accept(ICell2DVisitor visitor)
        {
            visitor.VisitCell(this);
        }

        public virtual void SetData(Cell2D_SerializedData data)
        {
            Data = data;
        }

        public void AssignComponent(ICell2DComponent component)
        {
            Data.AddComponent(component);
        }

        public void RemoveComponent(ICell2DComponent component)
        {
            Data.RemoveComponent(component);
        }

        public bool HasComponent(ICell2DComponent.TypeKey type)
        {
            return Data.HasComponent(type);
        }

        public float GetMinDimension() => Mathf.Min(Data.Dimensions.x, Data.Dimensions.y);

        public void GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions)
        {
            position = Data.Position;
            normal = Data.Normal;
            dimensions = Data.Dimensions;
        }

        public void GetTransformData(out Vector3 position, out Vector3 normal, out float radius)
        {
            position = Data.Position;
            normal = Data.Normal;
            radius = GetMinDimension() / 2;
        }
    }
}
