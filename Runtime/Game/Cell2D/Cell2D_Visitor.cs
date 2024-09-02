
using System;
using Darklight.UnityExt.Behaviour.Interface;
using Darklight.UnityExt.Editor;
using UnityEngine;
using static Darklight.UnityExt.Game.Grid.Cell2D.EventRegistry;
namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        public class Visitor : IVisitor<Cell2D>
        {
            VisitCellEvent _visitFunction;
            public Visitor(VisitCellEvent visitFunction)
            {
                _visitFunction = visitFunction;
            }

            public virtual void Visit(Cell2D cell)
            {
                _visitFunction(cell);
            }
        }

        public class ComponentVisitor : IVisitor<Cell2D>
        {
            // ======== [[ FIELDS ]] ======================================================= >>>>
            ComponentTypeKey _type; // The type of component to look for
            VisitCellComponentEvent _initFunc; // The function to initialize the component
            VisitCellComponentEvent _updateComponentFunc; // The function to update the component

            // ======== [[ PROPERTIES ]] ======================================================= >>>>
            #region -- (( EVENT FUNCTIONS )) -------- ))
            /// <summary>
            /// Register the component to the cell.
            /// </summary>
            public VisitCellComponentEvent RegisterFunc => (Cell2D cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.RegisterComponent(type);
            };

            /// <summary>
            /// Initialize the component to the cell.
            /// </summary>
            public VisitCellComponentEvent InitFunc
            {
                get
                {
                    if (_initFunc == null)
                    {
                        _initFunc = (Cell2D cell, ComponentTypeKey type) =>
                        {
                            Component component = cell.ComponentReg.GetComponent(type);
                            component.Initialize(cell);
                        };
                    }
                    return _initFunc;
                }
                set => _initFunc = value;
            }

            // Update the component in the cell.
            public VisitCellComponentEvent UpdateComponentFunc
            {
                get
                {
                    if (_updateComponentFunc == null)
                    {
                        _updateComponentFunc = (Cell2D cell, ComponentTypeKey type) =>
                        {
                            Component component = cell.ComponentReg.GetComponent(type);
                            component.Updater();
                        };
                    }
                    return _updateComponentFunc;
                }
                set => _updateComponentFunc = value;
            }
            #endregion

            // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
            public ComponentVisitor(ComponentTypeKey type) => _type = type;
            public ComponentVisitor(ComponentTypeKey type, VisitCellComponentEvent initFunc) : this(type)
            {
                InitFunc = initFunc;
            }
            public ComponentVisitor(ComponentTypeKey type,
                VisitCellComponentEvent initFunc, VisitCellComponentEvent updateFunc) : this(type, initFunc)
            {
                UpdateComponentFunc = updateFunc;
            }

            // ======== [[ METHODS ]] ======================================================= >>>>
            public void Visit(Cell2D cell)
            {
                ComponentRegistry componentRegistry = cell.ComponentReg;

                // Check if the stored component type exists in the cell
                if (componentRegistry.HasComponent(_type))
                {
                    // Check if the component is initialized
                    Component component = componentRegistry.GetComponent(_type);
                    if (!component.Initialized)
                    {
                        // Initialize the component
                        InitFunc(cell, _type);
                    }
                    else
                    {
                        // Update the component
                        UpdateComponentFunc(cell, _type);
                    }
                }
                // Register the component if it doesn't exist
                else
                {
                    RegisterFunc(cell, _type);
                }
            }

        }

        public static class VisitorFactory
        {
            public static Visitor Create(VisitCellEvent visitFunction)
            {
                return new Visitor(visitFunction);
            }

            public static ComponentVisitor Create(ComponentTypeKey type)
            {
                return new ComponentVisitor(type);
            }

            public static ComponentVisitor Create(ComponentTypeKey type, VisitCellComponentEvent initFunc)
            {
                return new ComponentVisitor(type, initFunc);
            }

            public static ComponentVisitor Create(ComponentTypeKey type, VisitCellComponentEvent initFunc, VisitCellComponentEvent updateFunc)
            {
                return new ComponentVisitor(type, initFunc, updateFunc);
            }

            public static ComponentVisitor CreateGizmosVisitor(ComponentTypeKey type)
            {
                return new ComponentVisitor(type, null, (Cell2D cell, ComponentTypeKey type) =>
                {
                    Component component = cell.ComponentReg.GetComponent(type);
                    component.DrawGizmos();
                });
            }

            public static ComponentVisitor CreateEditorGizmosVisitor(ComponentTypeKey type)
            {
                return new ComponentVisitor(type, null, (Cell2D cell, ComponentTypeKey type) =>
                {
                    Component component = cell.ComponentReg.GetComponent(type);
                    component.DrawEditorGizmos();
                });
            }
        }

    }
}
