using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


namespace Darklight.UnityExt.Game.Grid
{
    using ComponentType = ICell2DComponent.TypeKey;

    [Serializable]
    public class Cell2D_SerializedData
    {
        // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
        [SerializeField, ShowOnly] Vector2Int _key = Vector2Int.zero;
        [SerializeField, ShowOnly] Vector2Int _coordinate = Vector2Int.zero;
        [SerializeField, ShowOnly] Vector2 _dimensions = Vector2.one;
        [SerializeField, ShowOnly] Vector3 _position = Vector3.zero;
        [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;
        [SerializeField, ShowOnly] bool _isDisabled = false;
        [SerializeReference, NonReorderable] List<ICell2DComponent> _components = new();

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Vector2Int Key { get => _key; }
        public Vector2Int Coordinate { get => _coordinate; }
        public Vector2 Dimensions { get => _dimensions; }
        public Vector3 Position { get => _position; }
        public Vector3 Normal { get => _normal; }
        public bool Disabled { get => _isDisabled; }
        public List<ICell2DComponent> Components { get => _components; }

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Cell2D_SerializedData()
        {
            _key = Vector2Int.zero;
            _coordinate = Vector2Int.zero;
            _dimensions = Vector2.one;
            _position = Vector3.zero;
            _normal = Vector3.up;
            _isDisabled = false;
        }

        public Cell2D_SerializedData(Vector2Int key)
        {
            _key = key;
            _coordinate = key;
            _dimensions = Vector2.one;
            _position = Vector3.zero;
            _normal = Vector3.up;
            _isDisabled = false;
        }

        // ======== [[ METHODS ]] ============================================================ >>>>
        public void SetCoordinate(Vector2Int coordinate) => _coordinate = coordinate;
        public void SetPosition(Vector3 position) => _position = position;
        public void SetNormal(Vector3 normal) => _normal = normal;
        public void SetDimensions(Vector2 dimensions) => _dimensions = dimensions;
        public void SetDisabled(bool disabled) => _isDisabled = disabled;
        public virtual void CopyFrom(Cell2D_SerializedData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot copy data from null object.");
                return;
            }

            _key = data.Key;
            _coordinate = data.Coordinate;
            _dimensions = data.Dimensions;
            _position = data.Position;
            _normal = data.Normal;
            _isDisabled = data.Disabled;

            _componentMap.Clear();
            foreach (ICell2DComponent component in data.Components)
            {
                _componentMap.Add(component.Type, component);
            }
            Refresh();
        }

        #region ((Component Management)) ------------------ >>
        Dictionary<ComponentType, ICell2DComponent> _componentMap = new();
        public void AddComponent(ICell2DComponent component)
        {
            // If the component is not already in the dictionary, add it.
            if (!_componentMap.ContainsKey(component.Type))
            {
                _componentMap.Add(component.Type, component);
            }
            Refresh();
        }

        public void UpdateComponent(ICell2DComponent component)
        {
            // If the component is in the dictionary, update it.
            if (_componentMap.ContainsKey(component.Type))
            {
                _componentMap[component.Type] = component;
            }
            Refresh();
        }

        public void RemoveComponent(ICell2DComponent component)
        {
            // If the component is in the dictionary, remove it.
            if (_componentMap.ContainsKey(component.Type))
            {
                _componentMap.Remove(component.Type);
            }
            Refresh();
        }

        public bool HasComponent(ComponentType type)
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
        }
        #endregion


    }
}