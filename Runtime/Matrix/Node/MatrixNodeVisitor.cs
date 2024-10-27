using Darklight.UnityExt.Behaviour;

using static Darklight.UnityExt.Matrix.MatrixNode.EventRegistry;

namespace Darklight.UnityExt.Matrix
{
    partial class MatrixNode
    {

        public static class VisitorFactory
        {
            public static ComponentVisitor CreateComponentVisitor
                (ComponentTypeKey type, VisitCellComponentEvent visitFunction)
            {
                return new ComponentVisitor(type, visitFunction);
            }

            public static ComponentVisitor CreateComponentVisitor
                (Matrix.Component gridComponent, VisitCellComponentEvent visitFunction)
            {
                return new ComponentVisitor(gridComponent.TypeKey, visitFunction);
            }
        }
        public class Visitor : IVisitor<MatrixNode>
        {
            VisitCellEvent _visitFunction;
            public Visitor(VisitCellEvent visitFunction)
            {
                _visitFunction = visitFunction;
            }

            public virtual void Visit(MatrixNode cell)
            {
                _visitFunction(cell);
            }
        }

        public class ComponentVisitor : IVisitor<MatrixNode>
        {
            // ======== [[ FIELDS ]] ======================================================= >>>>
            ComponentTypeKey _type; // The type of component to look for
            VisitCellComponentEvent _visitFunction; // The function to call when visiting the cell

            // ======== [[ CONSTRUCTOR ]] ======================================================= >>>>
            public ComponentVisitor(ComponentTypeKey type, VisitCellComponentEvent visitFunction)
            {
                _type = type;
                VisitFunc = visitFunction;
            }

            // ======== [[ PROPERTIES ]] ======================================================= >>>>
            public VisitCellComponentEvent VisitFunc
            {
                get
                {
                    if (_visitFunction == null)
                        _visitFunction = BaseUpdateFunc;
                    return _visitFunction;
                }
                set => _visitFunction = value;
            }

            // ======== [[ METHODS ]] ======================================================= >>>>
            public void Visit(MatrixNode cell)
            {
                InternalComponentRegistry componentRegistry = cell.ComponentReg;

                // Check if the stored component type exists in the cell
                if (componentRegistry.HasComponent(_type))
                {
                    VisitFunc(cell, _type);
                }
                // Register the component if it doesn't exist
                else
                {
                    BaseRegisterFunc(cell, _type);
                }
            }

        }

    }
}
