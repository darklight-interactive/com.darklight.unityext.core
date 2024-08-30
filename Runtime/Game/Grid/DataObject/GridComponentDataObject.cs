using UnityEngine;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Grid
{
    [CreateAssetMenu(menuName = "Darklight/Grid/ComponentDataObject")]
    public class GridComponentDataObject : ScriptableObject
    {
        [EnumFlags]
        [SerializeField] CellComponentType componentTypes;

        // Dictionary to map component types to their creation functions
        private readonly Dictionary<CellComponentType, System.Func<ICellComponent>> componentFactories = new Dictionary<CellComponentType, System.Func<ICellComponent>>()
        {
            { CellComponentType.Base, () => new BaseCellComponent() },
            { CellComponentType.Overlap, () => new Overlap2DComponent() },
            { CellComponentType.Shape, () => new Shape2DComponent() },
            { CellComponentType.Weight, () => new WeightComponent() }
        };

        public void UpdateComponents(BaseCell cell)
        {
            if (cell == null)
            {
                Debug.LogError("Cannot add or remove components to a null cell.");
                return;
            }

            // Iterate through the component types and update the cell accordingly
            foreach (CellComponentType type in System.Enum.GetValues(typeof(CellComponentType)))
            {
                bool shouldHaveComponent = ShouldHaveComponent(type);
                UpdateComponentForCell(cell, type, shouldHaveComponent);
            }

            /*
            Debug.Log($"{cell.Name} has components: " +
                $"Base={ShouldHaveComponent(CellComponentType.Base)}, " +
                $"Overlap={ShouldHaveComponent(CellComponentType.Overlap)}, " +
                $"Shape={ShouldHaveComponent(CellComponentType.Shape)}, " +
                $"Weight={ShouldHaveComponent(CellComponentType.Weight)}");
            */
        }

        private bool ShouldHaveComponent(CellComponentType type)
        {
            return (componentTypes & type) == type;
        }

        private void UpdateComponentForCell(BaseCell cell, CellComponentType type, bool shouldHave)
        {
            if (shouldHave)
            {
                AddComponentToCell(cell, type);
            }
            else
            {
                RemoveComponentFromCell(cell, type);
            }
        }

        private void AddComponentToCell(BaseCell cell, CellComponentType type)
        {
            if (componentFactories.TryGetValue(type, out var factory))
            {
                ICellComponent component = factory.Invoke();
                component.Initialize(cell);
                cell.AddComponent(component);
            }
            else
            {
                Debug.LogError($"Cannot add component of unknown type: {type}");
            }
        }

        private void RemoveComponentFromCell(BaseCell cell, CellComponentType type)
        {
            if (cell.Components == null || cell.Components.Count == 0)
            {
                Debug.LogError("Cannot remove component from a cell with no components.");
                return;
            }

            ICellComponent component = cell.Components.Find(c => c.type == type);
            if (component != null)
            {
                cell.RemoveComponent(component);
            }
        }
    }
}
