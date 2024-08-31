using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class BaseCellData
    {
        public string Name { get => _name; protected set => _name = value; }
        public Vector2Int Key { get => _key; protected set => _key = value; }
        public Vector2Int Coordinate { get => _coordinate; protected set => _coordinate = value; }
        public Vector2 Dimensions { get => _dimensions; protected set => _dimensions = value; }
        public Vector3 Position { get => _position; protected set => _position = value; }
        public Vector3 Normal { get => _normal; protected set => _normal = value; }
        public bool Disabled { get => _isDisabled; protected set => _isDisabled = value; }
        public List<ICellComponent> Components { get => _components; }



        string _name = "BaseCell";
        [SerializeField, ShowOnly] private Vector2Int _key = Vector2Int.zero;
        [SerializeField, ShowOnly] private Vector2Int _coordinate = Vector2Int.zero;
        [SerializeField, ShowOnly] private Vector2 _dimensions = Vector2.one;
        [SerializeField, ShowOnly] private Vector3 _position = Vector3.zero;
        [SerializeField, ShowOnly] private Vector3 _normal = Vector3.up;
        [SerializeField, ShowOnly] private bool _isDisabled = false;

        // (( COMPONENTS )) ------------------ >>
        Dictionary<CellComponentType, ICellComponent> _componentMap = new();
        [SerializeField] List<ICellComponent> _components = new();

        [NonReorderable]
        [SerializeField, ShowOnly] List<CellComponentType> _componentTypes = new();


        // ============== (( CONSTRUCTORS )) ============== >>

        public BaseCellData() => Initialize(Vector2Int.zero);
        public BaseCellData(Vector2Int key) => Initialize(key);
        public virtual void Initialize(Vector2Int key)
        {
            _key = key;
            _name = $"BaseCell {key}";
        }


        // ============== (( METHODS )) ============== >>
        public virtual void CopyFrom(BaseCellData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot copy data from null object.");
                return;
            }

            _name = data.Name;
            _key = data.Key;
            _dimensions = data.Dimensions;
            _position = data.Position;
            _normal = data.Normal;
            _isDisabled = data.Disabled;

            _componentMap.Clear();
            foreach (ICellComponent component in data.Components)
            {
                _componentMap.Add(component.Type, component);
            }
            Refresh();
        }

        public void SetCoordinate(Vector2Int coordinate) => _coordinate = coordinate;
        public void SetPosition(Vector3 position) => _position = position;
        public void SetNormal(Vector3 normal) => _normal = normal;
        public void SetDimensions(Vector2 dimensions) => _dimensions = dimensions;
        public void SetDisabled(bool disabled) => _isDisabled = disabled;

        #region ((Component Management)) ------------------ >>
        public void AddComponent(ICellComponent component)
        {
            // If the component is not already in the dictionary, add it.
            if (!_componentMap.ContainsKey(component.Type))
            {
                _componentMap.Add(component.Type, component);
            }
            Refresh();
        }

        public void UpdateComponent(ICellComponent component)
        {
            // If the component is in the dictionary, update it.
            if (_componentMap.ContainsKey(component.Type))
            {
                _componentMap[component.Type] = component;
            }
            Refresh();
        }

        public void RemoveComponent(ICellComponent component)
        {
            // If the component is in the dictionary, remove it.
            if (_componentMap.ContainsKey(component.Type))
            {
                _componentMap.Remove(component.Type);
            }
            Refresh();
        }

        public bool HasComponent(CellComponentType type)
        {
            if (_componentMap.ContainsKey(type))
            {
                if (_componentMap[type] != null)
                {
                    return true;
                }
            }
            return false;
        }

        void Refresh()
        {
            _components = _componentMap.Values.ToList();
            _componentTypes = _componentMap.Keys.ToList();
        }
        #endregion


    }
}