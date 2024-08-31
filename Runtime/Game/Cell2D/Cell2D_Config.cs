using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public class Cell2D_Config
    {
        // ======== [[ SERIALIZED FIELDS ]] ============================================================ >>>>
        [SerializeField, ShowOnly] Vector2 _cellDimensions = new Vector2(1, 1);
        [SerializeField, ShowOnly] Vector2 _cellSpacing = new Vector2(1, 1);
        [SerializeField, ShowOnly] Vector2 _cellBonding = new Vector2(0, 0);
        [SerializeField, EnumFlags, ShowOnly] ICell2DComponent.TypeKey _componentFlags = 0;

        LayerMask _layerMask = 0;
        int _segments = 8;
        int _weight = 1;

        // ======== [[ PROPERTIES ]] ============================================================ >>>>
        public Vector2 CellDimensions => _cellDimensions;
        public Vector2 CellSpacing => _cellSpacing;
        public Vector2 CellBonding => _cellBonding;
        public ICell2DComponent.TypeKey ComponentFlags => _componentFlags;
        public LayerMask LayerMask => _layerMask;
        public int Segments => _segments;
        public int Weight => _weight;

        // ======== [[ CONSTRUCTORS ]] ============================================================ >>>>
        public Cell2D_Config() { }
        public Cell2D_Config(Cell2D_Config originConfig)
        {
            _cellDimensions = originConfig._cellDimensions;
            _cellSpacing = originConfig._cellSpacing;
            _cellBonding = originConfig._cellBonding;
            _componentFlags = originConfig._componentFlags;
            _layerMask = originConfig._layerMask;
            _segments = originConfig._segments;
            _weight = originConfig._weight;
        }

        // ======== [[ METHODS ]] ============================================================ >>>>
        public void SetCellDimensions(Vector2 cellDimensions) => _cellDimensions = cellDimensions;
        public void SetCellSpacing(Vector2 cellSpacing) => _cellSpacing = cellSpacing;
        public void SetCellBonding(Vector2 cellBonding) => _cellBonding = cellBonding;
        public void SetComponentFlags(ICell2DComponent.TypeKey componentFlags) => _componentFlags = componentFlags;
        public void SetLayerMask(LayerMask layerMask) => _layerMask = layerMask;
        public void SetSegments(int segments) => _segments = segments;
        public void SetWeight(int weight) => _weight = weight;
    }
}