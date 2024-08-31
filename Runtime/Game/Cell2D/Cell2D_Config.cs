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

        // ======== [[ PROPERTIES ]] ============================================================ >>>>
        public Vector2 CellDimensions => _cellDimensions;
        public Vector2 CellSpacing => _cellSpacing;
        public Vector2 CellBonding => _cellBonding;
        public ICell2DComponent.TypeKey ComponentFlags => _componentFlags;

        // ======== [[ CONSTRUCTORS ]] ============================================================ >>>>
        public Cell2D_Config() { }

        // ======== [[ METHODS ]] ============================================================ >>>>
        public void SetCellDimensions(Vector2 cellDimensions) => _cellDimensions = cellDimensions;
        public void SetCellSpacing(Vector2 cellSpacing) => _cellSpacing = cellSpacing;
        public void SetCellBonding(Vector2 cellBonding) => _cellBonding = cellBonding;
        public void SetComponentFlags(ICell2DComponent.TypeKey componentFlags) => _componentFlags = componentFlags;
    }
}