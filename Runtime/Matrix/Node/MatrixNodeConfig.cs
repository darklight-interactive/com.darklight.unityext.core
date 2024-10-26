using Darklight.UnityExt.Editor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class MatrixNode
    {
        [System.Serializable]
        public class NodeConfig
        {
            // ======== [[ SERIALIZED FIELDS ]] ============================================================ >>>>
            [SerializeField, ShowOnly] Vector2 _cellDimensions = new Vector2(1, 1);
            [SerializeField, ShowOnly] Vector2 _cellSpacing = new Vector2(1, 1);
            [SerializeField, ShowOnly] Vector2 _cellBonding = new Vector2(0, 0);

            // ======== [[ PROPERTIES ]] ============================================================ >>>>
            public Vector2 CellDimensions => _cellDimensions;
            public Vector2 CellSpacing => _cellSpacing;
            public Vector2 CellBonding => _cellBonding;

            // ======== [[ CONSTRUCTORS ]] ============================================================ >>>>
            public NodeConfig() { }
            public NodeConfig(NodeConfig originConfig)
            {
                _cellDimensions = originConfig._cellDimensions;
                _cellSpacing = originConfig._cellSpacing;
                _cellBonding = originConfig._cellBonding;
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            public void SetCellDimensions(Vector2 cellDimensions) => _cellDimensions = cellDimensions;
            public void SetCellSpacing(Vector2 cellSpacing) => _cellSpacing = cellSpacing;
            public void SetCellBonding(Vector2 cellBonding) => _cellBonding = cellBonding;
        }
    }
}