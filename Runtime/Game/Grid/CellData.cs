using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;


namespace Darklight.UnityExt.Game.Grid
{
    #region -- << INTERFACE >> : ICellData ------------------------------------ >>
    public interface ICellData
    {
        string Name { get; }
        Vector2Int Key { get; }
        Vector2 Dimensions { get; }
        Vector3 Position { get; }
        Vector3 Normal { get; }
        bool IsDisabled { get; }

        void Initialize(Vector2Int key);
    }
    #endregion

    #region -- << ABSTRACT CLASS >> : BaseCellData ------------------------------------ >>
    [System.Serializable]
    public abstract class BaseCellData : ICellData
    {
        [SerializeField, ShowOnly] private string _name = "BaseCell";
        [SerializeField, ShowOnly] private Vector2Int _key = Vector2Int.zero;
        [SerializeField, ShowOnly] private Vector2 _dimensions = Vector2.one;
        [SerializeField, ShowOnly] private Vector3 _position = Vector3.zero;
        [SerializeField, ShowOnly] private Vector3 _normal = Vector3.up;
        [SerializeField, ShowOnly] private bool _isDisabled = false;

        public string Name { get => _name; protected set => _name = value; }
        public Vector2Int Key { get => _key; protected set => _key = value; }
        public Vector2 Dimensions { get => _dimensions; protected set => _dimensions = value; }
        public Vector3 Position { get => _position; protected set => _position = value; }
        public Vector3 Normal { get => _normal; protected set => _normal = value; }
        public bool IsDisabled { get => _isDisabled; protected set => _isDisabled = value; }

        public BaseCellData() { }
        public BaseCellData(Vector2Int key) => Initialize(key);
        public virtual void Initialize(Vector2Int key)
        {
            _key = key;
            _name = $"Cell2D {key}";
        }

        public void SetPosition(Vector3 position) => _position = position;
        public void SetNormal(Vector3 normal) => _normal = normal;
        public void SetDimensions(Vector2 dimensions) => _dimensions = dimensions;
        public void SetDisabled(bool disabled) => _isDisabled = disabled;

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
            _isDisabled = data.IsDisabled;
        }
    }
    #endregion

    #region -- << CLASS >> : CellData ------------------------------------ >>
    [System.Serializable]
    public class CellData : BaseCellData
    {
        public CellData() : base(Vector2Int.zero) { }
        public CellData(Vector2Int key, GridConfig config) : base(key) { }
    }
    #endregion
}