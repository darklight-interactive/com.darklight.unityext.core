using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    // -- OverlapCell Class --
    [System.Serializable]
    public class OverlapCell : BaseCell<BaseCellData>, IOverlap
    {
        [SerializeField] LayerMask _layerMask;
        [SerializeField, ShowOnly] Collider2D[] _colliders;


        public List<Collider2D> Colliders { get => _colliders.ToList(); set => _colliders = value.ToArray(); }
        public LayerMask LayerMask { get => _layerMask; set => _layerMask = value; }

        public OverlapCell() : base() { }
        public OverlapCell(Vector2Int key) : base(key) { }

        public override void Update()
        {
            UpdateColliders();
        }

        public void UpdateColliders()
        {
            Vector3 cellCenter = Data.position;
            Vector3 halfExtents = new Vector3(Data.dimensions.x / 2, 1f, Data.dimensions.y / 2);

            // Use Physics.OverlapBox to detect colliders within the cell dimensions
            _colliders = Physics2D.OverlapBoxAll(cellCenter, halfExtents, 0, LayerMask);
        }

        public override void DrawGizmos(bool editMode)
        {
            base.DrawGizmos(editMode);

            Handles.color = Color.red;

            if (Colliders.Count > 0)
                Handles.color = Color.green;

            Vector3 halfExtents = new Vector3(Data.dimensions.x / 2, Data.GetMinDimension() / 2, Data.dimensions.y / 2);
            Handles.DrawWireCube(Data.position, halfExtents * 2);
        }
    }

    public class OverlapGridConfig_DataObject : GridConfigDataObject
    {
        [Header("Overlap Grid Data")]
        public bool showColliders = true;
    }

    // -- OverlapGrid Class --
    [ExecuteAlways]
    public class OverlapGrid : GenericGridMonoBehaviour<OverlapCell>
    {
        protected override void GenerateConfigObj()
        {
            configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<OverlapGridConfig_DataObject>(CONFIG_PATH, name);
        }
    }
}
