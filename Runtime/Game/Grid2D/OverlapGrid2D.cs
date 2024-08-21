using UnityEngine;
using Darklight.UnityExt.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game
{
    /// <summary>
    /// A 2D Grid that stores Overlap_Grid2DData objects. 
    /// </summary>
    [ExecuteAlways]
    public class OverlapGrid2D : SimpleGrid2D
    {
        [SerializeField,
        Tooltip("OverlapGrid2D uses OverlapBoxAll to detect colliders in the grid. This is the layer mask used to filter which colliders are detected.")]
        private LayerMask layerMask;

        /*
        public OverlapCell GetBestOverlapGridData()
        {
            OverlapCell bestData = cellDataMap.Values.GetEnumerator().Current;

            foreach (OverlapCell data in cellDataMap.Values)
            {
                if (bestData == null) { bestData = data; }

                if (data._disabled) continue; // Skip disabled data
                if (data.colliders.Length > 0) continue; // Skip data with colliders

                // If the data has a higher or equal weight and less colliders, set it as the best data
                if (data._weight >= bestData._weight)
                {
                    bestData = data;
                }
            }
            //Debug.Log($"{this.name} OverlapGrid2D Best Data: {bestData.positionKey} - {bestData.worldPosition}");
            return bestData;
        }
        */
    }
}
