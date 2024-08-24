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
        /// <summary>
        /// The name of the cell.
        /// </summary>
        string name { get; }

        /// <summary>
        /// The key value of the cell.
        /// This is the primary identifier for the cell and does not change after initialization.
        /// </summary>
        Vector2Int key { get; }

        /// <summary>
        /// The coordinate of the cell.
        /// This is the position of the cell in the grid in relation to the origin.
        /// This value can change if the grid is resized or realigned.
        /// </summary>
        Vector2Int coordinate { get; }

        /// <summary>
        /// The dimensions of the cell in 2D space.
        /// </summary>
        Vector2 dimensions { get; }

        /// <summary>
        /// The position of the cell in 3D world space.
        /// </summary>
        Vector3 position { get; }

        /// <summary>
        /// The normal of the cell in 3D world space.
        /// </summary>
        Vector3 normal { get; }

        /// <summary>
        /// Whether the cell is disabled.
        /// </summary>
        bool disabled { get; }

        void Initialize(Vector2Int key);
    }
    #endregion

    #region -- << ABSTRACT CLASS >> : BaseCellData ------------------------------------ >>
    [System.Serializable]
    public abstract class BaseCellData : ICellData
    {
        [SerializeField, ShowOnly] private string _name = "BaseCell";
        [SerializeField, ShowOnly] private Vector2Int _key = Vector2Int.zero;
        [SerializeField, ShowOnly] private Vector2Int _coordinate = Vector2Int.zero;
        [SerializeField, ShowOnly] private Vector2 _dimensions = Vector2.one;
        [SerializeField, ShowOnly] private Vector3 _position = Vector3.zero;
        [SerializeField, ShowOnly] private Vector3 _normal = Vector3.up;
        [SerializeField, ShowOnly] private bool _isDisabled = false;

        public string name { get => _name; protected set => _name = value; }
        public Vector2Int key { get => _key; protected set => _key = value; }
        public Vector2Int coordinate { get => _coordinate; protected set => _coordinate = value; }
        public Vector2 dimensions { get => _dimensions; protected set => _dimensions = value; }
        public Vector3 position { get => _position; protected set => _position = value; }
        public Vector3 normal { get => _normal; protected set => _normal = value; }
        public bool disabled { get => _isDisabled; protected set => _isDisabled = value; }

        public BaseCellData() { }
        public BaseCellData(Vector2Int key) => Initialize(key);
        public virtual void Initialize(Vector2Int key)
        {
            _key = key;
            _name = $"Cell2D {key}";
        }

        public void SetName(string name) => _name = name;
        public void SetKey(Vector2Int key) => _key = key;
        public void SetCoordinate(Vector2Int coordinate) => _coordinate = coordinate;
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

            _name = data.name;
            _key = data.key;
            _dimensions = data.dimensions;
            _position = data.position;
            _normal = data.normal;
            _isDisabled = data.disabled;
        }
    }
    #endregion

    #region -- << CLASS >> : CellData ------------------------------------ >>
    [System.Serializable]
    public class CellData : BaseCellData
    {
        public CellData() : base(Vector2Int.zero) { }
        public CellData(Vector2Int key, GridMapConfig config) : base(key) { }
    }
    #endregion
}