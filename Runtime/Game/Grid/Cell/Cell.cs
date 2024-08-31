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

        void AddComponent<TComponent>(TComponent component) where TComponent : ICellComponent;
        void RemoveComponent<TComponent>(TComponent component) where TComponent : ICellComponent;
    }

    [System.Serializable]
    public class BaseCell : ICell
    {
        Dictionary<CellComponentType, ICellComponent> _components = new();


        [SerializeField, ShowOnly] string _name;
        [SerializeField] BaseCellData _data;
        [SerializeField, ShowOnly, NonReorderable] List<CellComponentType> _componentTypes = new();

        public string Name => Data.name;
        public BaseCellData Data { get => _data; private set => _data = value; }
        public List<ICellComponent> Components { get => _components.Values.ToList(); }


        public BaseCell(Vector2Int key)
        {
            Data = new BaseCellData(key);
            Data.Initialize(key);

            _name = Data.name;
        }

        public void Accept(ICellVisitor visitor)
        {
            visitor.Visit(this);
        }

        public virtual void SetData(BaseCellData data)
        {
            Data = data;
        }

        public void AddComponent<T>(T component) where T : ICellComponent
        {
            if (component == null)
            {
                Debug.LogError("Cannot add null component to cell.");
                return;
            }

            if (!_components.ContainsKey(component.type))
            {
                component.Initialize(this);
                _components.Add(component.type, component);
                _componentTypes.Add(component.type);
            }
        }

        public void RemoveComponent<T>(T component) where T : ICellComponent
        {
            if (component == null)
            {
                Debug.LogError("Cannot remove null component from cell.");
                return;
            }

            if (_components.ContainsKey(component.type))
            {
                _components.Remove(component.type);
                _componentTypes.Remove(component.type);
            }
        }

        public float GetMinDimension() => Mathf.Min(Data.dimensions.x, Data.dimensions.y);

        public void GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions)
        {
            position = Data.position;
            normal = Data.normal;
            dimensions = Data.dimensions;
        }

        public void GetTransformData(out Vector3 position, out Vector3 normal, out float radius)
        {
            position = Data.position;
            normal = Data.normal;
            radius = GetMinDimension() / 2;
        }
    }
}
