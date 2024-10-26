using System.Collections.Generic;

using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    public class MatrixWeightComponent : Matrix.BaseComponent
    {
        const string DATA_OBJECT_PATH = "Assets/Resources/Darklight/Grid2D/WeightData";
        const int DEFAULT_WEIGHT = 5;
        const int MIN_WEIGHT = 0;

        // ======== [[ FIELDS ]] ================================== >>>>
        [SerializeField, Expandable] MatrixWeightComponentPreset _weightDataObject;

        // ======== [[ PROPERTIES ]] ================================== >>>>
        #region -- (( BASE VISITORS )) -------- ))
        protected override MatrixNode.ComponentVisitor CellComponent_InitVisitor =>
            MatrixNode.VisitorFactory.CreateComponentVisitor(ComponentTypeKey.WEIGHT,
            (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();

                if (_weightDataObject == null)
                    return false;

                // << SET WEIGHT FROM MAP >>
                if (!_weightDataObject.ContainsKey(cell.Key))
                {
                    weightComponent.SetWeight(DEFAULT_WEIGHT);
                    _weightDataObject.Add(cell.Key, DEFAULT_WEIGHT);

                }
                else
                    weightComponent.SetWeight(_weightDataObject[cell.Key]);

                weightComponent.OnInitialize(cell);
                return true;
            });

        protected override MatrixNode.ComponentVisitor CellComponent_UpdateVisitor =>
            MatrixNode.VisitorFactory.CreateComponentVisitor(ComponentTypeKey.WEIGHT,
            (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();

                if (_weightDataObject == null)
                    return false;

                // << UPDATE WEIGHT FROM MAP >>
                if (_weightDataObject.ContainsKey(cell.Key)
                    && _weightDataObject[cell.Key] != weightComponent.GetWeight())
                {
                    weightComponent.SetWeight(_weightDataObject[cell.Key]);
                    //Debug.Log($"Updating Weight: {cell.Key} to {_weightDataObject[cell.Key]}");
                }
                weightComponent.OnUpdate();
                return true;
            });

        // -- (( CUSTOM VISITORS )) -------- ))
        private MatrixNode.ComponentVisitor _randomizeVisitor => MatrixNode.VisitorFactory.CreateComponentVisitor
            (ComponentTypeKey.WEIGHT, (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();

                // << SET RANDOM WEIGHT >>
                weightComponent.SetRandomWeight();
                return true;
            });
        private MatrixNode.ComponentVisitor _resetVisitor => MatrixNode.VisitorFactory.CreateComponentVisitor
            (ComponentTypeKey.WEIGHT, (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();

                // << SET WEIGHT TO DEFAULT >>
                weightComponent.SetWeight(DEFAULT_WEIGHT);
                return true;
            });

        private MatrixNode.ComponentVisitor _loadDataVisitor => MatrixNode.VisitorFactory.CreateComponentVisitor
            (ComponentTypeKey.WEIGHT, (MatrixNode cell, ComponentTypeKey type) =>
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();

                if (_weightDataObject == null)
                    return false;

                // Set the cell's weight component to the internal weight data
                if (_weightDataObject.ContainsKey(cell.Key))
                    weightComponent.SetWeight(_weightDataObject[cell.Key]);
                return true;
            });
        #endregion

        // ======== [[ METHODS ]] ================================== >>>>
        private void OnValidate()
        {

        }

        // -- (( INTERFACE )) : IComponent -------- ))
        public override void OnInitialize(Matrix baseObj)
        {
#if UNITY_EDITOR
            if (_weightDataObject == null)
            {
                _weightDataObject = ScriptableObjectUtility.CreateOrLoadScriptableObject<MatrixWeightComponentPreset>(DATA_OBJECT_PATH, "DefaultWeightDataObject");
            }
#endif

            base.OnInitialize(baseObj);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        // -- (( HANDLER METHODS )) -------- ))
        public void RandomizeWeights()
        {
            BaseGrid.SendVisitorToAllCells(_randomizeVisitor);
        }

        public void ResetWeights()
        {
            BaseGrid.SendVisitorToAllCells(_resetVisitor);
        }

        // -- (( GETTERS )) -------- ))
        public MatrixNode GetRandomCellByWeight()
        {
            List<MatrixNode.WeightComponent> weightComponents = BaseGrid.GetComponentsByType<MatrixNode.WeightComponent>();
            MatrixNode chosenCell = WeightedDataSelector.SelectRandomWeightedItem(weightComponents, (MatrixNode.WeightComponent weightComponent) =>
            {
                return weightComponent.BaseCell;
            });

            // Begin recursive search for a cell with a weight
            if (chosenCell == null)
            {
                return GetRandomCellByWeight();
            }

            Debug.Log($"Random Weight Chosen Cell: {chosenCell.Key}");
            return chosenCell;
        }

        public MatrixNode GetRandomCellByWeight(List<MatrixNode> cells)
        {
            List<MatrixNode.WeightComponent> weightComponents = new List<MatrixNode.WeightComponent>();
            foreach (MatrixNode cell in cells)
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();
                if (weightComponent != null)
                    weightComponents.Add(weightComponent);
            }

            MatrixNode chosenCell = WeightedDataSelector.SelectRandomWeightedItem(weightComponents, (MatrixNode.WeightComponent weightComponent) =>
            {
                return weightComponent.BaseCell;
            });

            // Begin recursive search for a cell with a weight
            if (chosenCell == null)
            {
                return GetRandomCellByWeight(cells);
            }

            Debug.Log($"Random Weight Chosen Cell: {chosenCell.Key}");
            return chosenCell;
        }

        public Dictionary<int, List<MatrixNode>> GetAllCellsByWeight()
        {
            Dictionary<int, List<MatrixNode>> weightMap = new Dictionary<int, List<MatrixNode>>();
            foreach (KeyValuePair<Vector2Int, int> pair in _weightDataObject)
            {
                if (!weightMap.ContainsKey(pair.Value))
                    weightMap[pair.Value] = new List<MatrixNode>();
                weightMap[pair.Value].Add(BaseGrid.GetCell(pair.Key));
            }
            return weightMap;
        }

        public List<MatrixNode> GetCellsWithWeight(int weight)
        {
            List<MatrixNode> cells = new List<MatrixNode>();
            foreach (KeyValuePair<Vector2Int, int> pair in _weightDataObject)
            {
                if (pair.Value == weight)
                    cells.Add(BaseGrid.GetCell(pair.Key));
            }
            return cells;
        }

        public MatrixNode GetCellWithHighestWeight()
        {
            Dictionary<int, List<MatrixNode>> weightMap = GetAllCellsByWeight();
            int highestWeight = 0;
            foreach (int weight in weightMap.Keys)
            {
                if (weight > highestWeight)
                    highestWeight = weight;
            }

            if (highestWeight == 0)
                return null;

            List<MatrixNode> cells = weightMap[highestWeight];
            return GetRandomCellByWeight(cells);
        }

        public MatrixNode GetCellWithHighestWeight(List<MatrixNode> cells)
        {
            if (cells.Count == 0)
            {
                Debug.LogError("Cannot get cell with highest weight from empty list.", this);
            }

            MatrixNode highestWeightCell = cells[0];
            int highestWeight = 0;
            foreach (MatrixNode cell in cells)
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();
                if (highestWeightCell == null || weightComponent.GetWeight() > highestWeight)
                {
                    highestWeight = weightComponent.GetWeight();
                    highestWeightCell = cell;
                }
            }
            return highestWeightCell;
        }

        public MatrixNode GetCellWithLowestWeight(List<MatrixNode> cells)
        {
            if (cells.Count == 0)
            {
                Debug.LogError("Cannot get cell with highest weight from empty list.", this);
            }

            MatrixNode lowestWeightCell = cells[0];
            int lowestWeight = cells[0].ComponentReg.GetComponent<MatrixNode.WeightComponent>().GetWeight();
            foreach (MatrixNode cell in cells)
            {
                MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();
                if (lowestWeightCell == null || weightComponent.GetWeight() < lowestWeight)
                {
                    lowestWeight = weightComponent.GetWeight();
                    lowestWeightCell = cell;
                }
            }
            return lowestWeightCell;
        }

        // -- (( SETTERS )) -------- ))
        public void SetCellToWeight(MatrixNode cell, int weight)
        {
            MatrixNode.WeightComponent weightComponent = cell.ComponentReg.GetComponent<MatrixNode.WeightComponent>();
            weightComponent.SetWeight(weight);
        }

        // ======== [[ PRIVATE METHODS ]] ================================== >>>>
        void LoadWeightDataToCells()
        {
        }

        // ======== [[ NESTED TYPES ]] ================================== >>>>

        [System.Serializable]
        public class Weighted_SerializedCellData
        {
            [ShowOnly] public string name;
            [ShowOnly] public Vector2Int key;
            [Range(MIN_WEIGHT, 10)] public int weight;
            public Weighted_SerializedCellData(Vector2Int key, int weight)
            {
                this.name = $"Cell ({key.x},{key.y})";
                this.key = key;
                this.weight = weight;
            }
        }
    }
}

