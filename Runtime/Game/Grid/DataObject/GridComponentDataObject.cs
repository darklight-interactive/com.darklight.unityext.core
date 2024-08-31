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

        private bool _hasOverlap;
        private bool _hasShape;
        private bool _hasWeight;

        // Dictionary to map component types to their creation functions
        private readonly Dictionary<CellComponentType, System.Func<ICellComponent>> componentFactories = new Dictionary<CellComponentType, System.Func<ICellComponent>>()
        {
            { CellComponentType.Base, () => new BaseCellComponent() },
            { CellComponentType.Overlap, () => new Overlap2DComponent() },
            { CellComponentType.Shape, () => new Shape2DComponent() },
            { CellComponentType.Weight, () => new WeightComponent() }
        };


        [EnumFlags]
        [SerializeField] CellComponentType componentTypes;

        [Header("Component Templates")]
        [SerializeField, ShowIf("_hasOverlap")]
        Overlap2DComponent overlapComponentTemplate = new Overlap2DComponent();

        [SerializeField, ShowIf("_hasShape")]
        Shape2DComponent shapeComponentTemplate = new Shape2DComponent();

        [SerializeField, ShowIf("_hasWeight")]
        WeightComponent weightComponentTemplate = new WeightComponent();

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

            _hasOverlap = ShouldHaveComponent(CellComponentType.Overlap);
            _hasShape = ShouldHaveComponent(CellComponentType.Shape);
            _hasWeight = ShouldHaveComponent(CellComponentType.Weight);

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
                ICellComponent component = CreateComponent(type);
                if (type == CellComponentType.Overlap)
                {
                    Overlap2DComponent overlapComponent = new Overlap2DComponent(cell, overlapComponentTemplate);
                    AddComponentToCell(cell, overlapComponent);
                }
                else if (type == CellComponentType.Shape)
                {
                    Shape2DComponent shapeComponent = new Shape2DComponent(cell, shapeComponentTemplate);
                    AddComponentToCell(cell, shapeComponent);
                }
                else if (type == CellComponentType.Weight)
                {
                    WeightComponent weightComponent = new WeightComponent(cell, weightComponentTemplate);
                    AddComponentToCell(cell, weightComponent);
                }
                else
                {
                    AddComponentToCell(cell, component);
                }
            }
            else
            {
                RemoveComponentFromCell(cell, type);
            }
        }

        private ICellComponent CreateComponent(CellComponentType type)
        {
            if (componentFactories.TryGetValue(type, out var factory))
            {
                return factory.Invoke();
            }
            else
            {
                Debug.LogError($"Cannot create component of unknown type: {type}");
                return null;
            }
        }

        private void AddComponentToCell(BaseCell cell, ICellComponent component)
        {
            if (cell == null || component == null)
            {
                Debug.LogError("Cannot add component to a null cell.");
                return;
            }

            cell.AddComponent(component);
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
