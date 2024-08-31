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
        BaseCellData Data { get; }
        List<ICellComponent> Components { get; }

        void Accept(ICellVisitor visitor);
        void SetData(BaseCellData data);

        void AssignComponent(ICellComponent component);
        void RemoveComponent(ICellComponent component);
    }

    [System.Serializable]
    public class BaseCell : ICell
    {


        [SerializeField, ShowOnly] string _name;
        [SerializeField] BaseCellData _data;


        public string Name => Data.Name;
        public BaseCellData Data { get => _data; private set => _data = value; }
        public List<ICellComponent> Components { get => _data.Components; }


        public BaseCell(Vector2Int key)
        {
            Data = new BaseCellData(key);
            Data.Initialize(key);

            _name = Data.Name;
        }

        public void Accept(ICellVisitor visitor)
        {
            visitor.VisitCell(this);
        }

        public virtual void SetData(BaseCellData data)
        {
            Data = data;
        }

        public void AssignComponent(ICellComponent component)
        {
            Data.AddComponent(component);
        }

        public void RemoveComponent(ICellComponent component)
        {
            Data.RemoveComponent(component);
        }

        public bool HasComponent(CellComponentType type)
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
