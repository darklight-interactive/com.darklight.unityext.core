using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Core2D;

using UnityEngine;

#if UNITY_EDITOR
#endif
namespace Darklight.UnityExt.Matrix
{
    [RequireComponent(typeof(Matrix))]
    public class OverlapWeightSpawnerMatrixComposite : Matrix.CompositeComponent<
        MatrixOverlapComponent, MatrixWeightComponent, MatrixSpawnerComponent>
    {
        public MatrixOverlapComponent OverlapComponent { get => _componentA; }
        public MatrixWeightComponent WeightComponent { get => _componentB; }
        public MatrixSpawnerComponent SpawnerComponent { get => _componentC; }

        // ======== [[ METHODS ]] ================================== >>>>
        public List<Vector2Int> GetCellKeys()
        {
            return BaseGrid.CellKeys.ToList();
        }

        public MatrixNode GetBestCell()
        {
            // From all available cells, get the cells with the lowest collider count
            List<MatrixNode> availableCells = BaseGrid.GetCells();

            // Get the cells with the lowest collider count
            List<MatrixNode> lowestColliderCells = OverlapComponent.GetCellsWithColliderCount(0);
            if (lowestColliderCells.Count > 0)
            {
                // If there are cells with no colliders, return one of them
                //Debug.Log($"Found {lowestColliderCells.Count} cells with no colliders");
                MatrixNode bestCell = WeightComponent.GetCellWithHighestWeight(lowestColliderCells);
                if (bestCell == null)
                {
                    Debug.LogError("No best cell found");
                    return null;
                }

                //Debug.Log($"{this.name} OverlapWeightSpawner: Best cell found {bestCell.Name}");
                return bestCell;
            }

            // If all cells have colliders, return the cell with the lowest weight from all available cells
            return WeightComponent.GetCellWithHighestWeight(availableCells.ToList());
        }

        public MatrixNode GetNextAvailableCell()
        {
            // << GET CELLS WITH NO OVERLAP >>
            List<MatrixNode> availableCells = OverlapComponent.GetCellsWithColliderCount(0);

            // << CELLS WITH NO ATTTACHED TRANSFORMS >>
            List<MatrixNode> cellsWithNoAttachedObjects = new List<MatrixNode>();
            foreach (MatrixNode cell in availableCells)
            {
                if (SpawnerComponent.GetAttachedTransformsAtCell(cell).Count == 0)
                {
                    cellsWithNoAttachedObjects.Add(cell);
                }
            }

            // << GET CELL WITH HIGHEST WEIGHT >>
            if (cellsWithNoAttachedObjects.Count > 0)
            {
                return WeightComponent.GetCellWithHighestWeight(cellsWithNoAttachedObjects);
            }

            return null;
        }

        public Spatial2D.AnchorPoint GetAnchorPointFromCell(MatrixNode cell)
        {
            return SpawnerComponent.GetTargetAnchor(cell);
        }

        public Spatial2D.AnchorPoint GetOriginPointFromCell(MatrixNode cell)
        {
            return SpawnerComponent.GetOriginAnchor(cell);
        }
    }
}
