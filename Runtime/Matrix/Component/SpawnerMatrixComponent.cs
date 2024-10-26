using System.Collections.Generic;

using Darklight.UnityExt.Core2D;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using UnityEngine;

using Object = UnityEngine.Object;

#if UNITY_EDITOR
#endif

namespace Darklight.UnityExt.Matrix
{
    public class MatrixSpawnerComponent : Matrix.BaseComponent
    {

        // ======== [[ FIELDS ]] ================================== >>>>
        Dictionary<Vector2Int, MatrixNode.SpawnerComponent.InternalData> _dataMap = new Dictionary<Vector2Int, MatrixNode.SpawnerComponent.InternalData>();

        [SerializeField, Expandable] MatrixSpawnerComponentPreset _dataObject;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        MatrixSpawnerComponentPreset DataObject
        {
            get
            {
                if (_dataObject == null)
                    _dataObject = CreateOrLoadDataObject();
                return _dataObject;
            }
        }

        List<MatrixNode.SpawnerComponent.InternalData> SerializedData { get => DataObject.SerializedSpawnData; set => DataObject.SerializedSpawnData = value; }

        protected override MatrixNode.ComponentVisitor CellComponent_InitVisitor =>
            MatrixNode.VisitorFactory.CreateComponentVisitor(ComponentTypeKey.SPAWNER,
            (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.SpawnerComponent spawnerComponent = cell.GetComponent<MatrixNode.SpawnerComponent>();
                return true;
            });

        protected override MatrixNode.ComponentVisitor CellComponent_UpdateVisitor =>
            MatrixNode.VisitorFactory.CreateComponentVisitor(ComponentTypeKey.SPAWNER,
            (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.SpawnerComponent spawnerComponent = cell.GetComponent<MatrixNode.SpawnerComponent>();
                VisitCellSpawner(spawnerComponent);
                spawnerComponent.OnUpdate();
                return true;
            });



        // ======== [[ METHODS ]] ================================== >>>>
        // ---- (( HANDLE DATA )) <PRIVATE_METHODS> ---- >>
        void VisitCellSpawner(MatrixNode.SpawnerComponent cellSpawner)
        {
            MatrixNode cell = cellSpawner.BaseCell;
            if (_dataObject == null) return;

            // << CELL SPAWNER DATA >> ------------------------------------ >>
            // Initialize the cell spawner data if it is null
            if (cellSpawner.Data == null)
                cellSpawner.Data = new MatrixNode.SpawnerComponent.InternalData(cell.Key)
                {
                    InheritCellHeight = _dataObject.InheritCellHeight,
                    InheritCellWidth = _dataObject.InheritCellWidth,
                    InheritCellNormal = _dataObject.InheritCellNormal,
                    OriginAnchor = _dataObject.DefaultOriginAnchor,
                    TargetAnchor = _dataObject.DefaultTargetAnchor
                };

            // << DATA MAP >> --------------------------------------------- >>
            // Initialize the data map if it is null
            if (_dataMap == null)
                _dataMap = new Dictionary<Vector2Int, MatrixNode.SpawnerComponent.InternalData>();
            // Else If the cell is not in the data map, add it
            else if (!_dataMap.ContainsKey(cell.Key))
                _dataMap.Add(cell.Key, cellSpawner.Data);

            // << SERIALIZED DATA >> -------------------------------------- >>
            // Initialize the serialized data if it is null
            if (SerializedData == null)
                SerializedData = new List<MatrixNode.SpawnerComponent.InternalData>();
            // Else If the serialized data is empty, add the cell spawner data
            else if (SerializedData.Count == 0)
            {
                SerializedData.Add(cellSpawner.Data);
                return;
            }
            // Else If the data key is not found in the serialized data, add it
            else if (!SerializedData.Exists(x => x.CellKey == cellSpawner.Data.CellKey))
                SerializedData.Add(cellSpawner.Data);
            // Else Update the data map from the serialized data
            else
            {
                MatrixNode.SpawnerComponent.InternalData savedSerializedData = SerializedData.Find(x => x.CellKey == cellSpawner.Data.CellKey);
                _dataMap[cellSpawner.Data.CellKey] = savedSerializedData;
                cellSpawner.Data = new MatrixNode.SpawnerComponent.InternalData(savedSerializedData);
            }
        }

        void UpdateData()
        {
            // Get all the valid keys from the grid
            HashSet<Vector2Int> validKeys = new HashSet<Vector2Int>(BaseGrid.CellKeys);
            List<Vector2Int> keysToRemove = new List<Vector2Int>();

            // << DATA MAP ITERATOR >> -------------------------------------- >>
            foreach (Vector2Int key in _dataMap.Keys)
            {
                if (!validKeys.Contains(key))
                    keysToRemove.Add(key);
            }
            // Remove invalid keys from the data map
            foreach (Vector2Int key in keysToRemove)
                _dataMap.Remove(key);

            // << SERIALIZED DATA ITERATOR >> -------------------------------- >>
            keysToRemove.Clear();
            foreach (MatrixNode.SpawnerComponent.InternalData data in SerializedData)
            {
                if (!validKeys.Contains(data.CellKey))
                {
                    keysToRemove.Add(data.CellKey);
                    continue;
                }

                // Update the data
                data.InheritCellWidth = DataObject.InheritCellWidth;
                data.InheritCellHeight = DataObject.InheritCellHeight;
                data.InheritCellNormal = DataObject.InheritCellNormal;
            }
            // Remove invalid keys from the serialized data
            foreach (Vector2Int key in keysToRemove)
                SerializedData.RemoveAll(x => x.CellKey == key);

            // Sort the _serializedSpawnData by CellKey in ascending order
            SerializedData.Sort((data1, data2) =>
            {
                int xComparison = data1.CellKey.x.CompareTo(data2.CellKey.x);
                if (xComparison == 0)
                {
                    // If x values are the same, compare y values
                    return data1.CellKey.y.CompareTo(data2.CellKey.y);
                }
                return xComparison;
            });
        }

        // ---- (( INTERFACE )) ---- >>
        public override void OnInitialize(Matrix grid)
        {
            CreateOrLoadDataObject();
            base.OnInitialize(grid);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateData();
        }

        public void AssignTransformToCell(Transform transform, MatrixNode cell)
        {
            MatrixNode.SpawnerComponent cellSpawner = cell.GetComponent<MatrixNode.SpawnerComponent>();
            cellSpawner.AttachTransformToCell(transform);
        }

        public List<Transform> GetAttachedTransformsAtCell(MatrixNode cell)
        {
            if (_dataMap.ContainsKey(cell.Key))
            {
                MatrixNode.SpawnerComponent.InternalData data = _dataMap[cell.Key];
                return data.AttachedTransforms;
            }
            return new List<Transform>();
        }

        public Spatial2D.AnchorPoint GetOriginAnchor(MatrixNode cell)
        {
            if (_dataMap.ContainsKey(cell.Key))
                return _dataMap[cell.Key].OriginAnchor;
            return Spatial2D.AnchorPoint.CENTER;
        }

        public Spatial2D.AnchorPoint GetTargetAnchor(MatrixNode cell)
        {
            if (_dataMap.ContainsKey(cell.Key))
                return _dataMap[cell.Key].TargetAnchor;
            return Spatial2D.AnchorPoint.CENTER;
        }

        public void SetAllCellsToDefault()
        {
            foreach (MatrixNode.SpawnerComponent.InternalData data in SerializedData)
            {
                data.OriginAnchor = _dataObject.DefaultOriginAnchor;
                data.TargetAnchor = _dataObject.DefaultTargetAnchor;
            }
        }

        MatrixSpawnerComponentPreset CreateOrLoadDataObject()
        {
            if (_dataObject != null) return _dataObject;

#if UNITY_EDITOR
            // Create or load the data object
            _dataObject = ScriptableObjectUtility.CreateOrLoadScriptableObject<MatrixSpawnerComponentPreset>("Assets/Resources/Darklight/Matrix", "DefaultSpawnerDataObject");
#endif
            return _dataObject;
        }


        // ======== [[ NESTED TYPES ]] ================================== >>>>

        [System.Serializable]
        public class ObjectAnchorPair
        {
            [ShowOnly] public Spatial2D.AnchorPoint targetAnchor;
            [ShowAssetPreview] public Object obj;
        }
    }
}