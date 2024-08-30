using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    // -- OverlapCellData Class --
    [System.Serializable]
    public class OverlapCellData : BaseCellData
    {
        [SerializeField, ShowOnly] private List<Collider> _colliders = new List<Collider>();

        public List<Collider> Colliders => _colliders;

        public OverlapCellData() : base() { }
        public OverlapCellData(Vector2Int key) : base(key) { }

        public void SetColliders(List<Collider> colliders)
        {
            _colliders = colliders;
        }
    }

    // -- OverlapCell Class --
    [System.Serializable]
    public class OverlapCell : BaseCell<OverlapCellData>
    {
        public new OverlapCellData data
        {
            get => base.data as OverlapCellData;
            set => base.data = value;
        }

        public OverlapCell() : base() { }
        public OverlapCell(Vector2Int key) : base(key) { }

        public override void Update()
        {
            Vector3 cellCenter = data.position;
            Vector3 halfExtents = new Vector3(data.dimensions.x / 2, 1f, data.dimensions.y / 2);

            // Use Physics.OverlapBox to detect colliders within the cell dimensions
            Collider[] colliders = Physics.OverlapBox(cellCenter, halfExtents, Quaternion.identity);

            data.SetColliders(new List<Collider>(colliders));
        }

        public override void DrawGizmos(bool editMode)
        {
            base.DrawGizmos(editMode);

            Handles.color = Color.red;

            if (data.Colliders.Count > 0)
                Handles.color = Color.green;

            Vector3 halfExtents = new Vector3(data.dimensions.x / 2, 1f, data.dimensions.y / 2);
            Handles.DrawWireCube(data.position, halfExtents * 2);
        }
    }

    public class OverlapGridConfig_DataObject : GridConfigDataObject
    {
        [Header("Overlap Grid Data")]
        public bool showColliders = true;
    }

    // -- OverlapGrid Class --
    [ExecuteAlways]
    public class OverlapGrid : GenericGridMonoBehaviour<OverlapCell, OverlapCellData>
    {
        protected override void GenerateConfigObj()
        {
            configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<OverlapGridConfig_DataObject>(CONFIG_PATH, name);
        }
    }
}
