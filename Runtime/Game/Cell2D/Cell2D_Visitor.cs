
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
            ComponentTypeKey _type;
            VisitCellComponentEvent _registerFunc;
            VisitCellComponentEvent _initFunc;
            VisitCellComponentEvent _updateComponentFunc;

            // ======== [[ PROPERTIES ]] ======================================================= >>>>
            #region -- (( EVENT FUNCTIONS )) -------- ))
            /// <summary>
            /// Register the component to the cell.
            /// </summary>
            public VisitCellComponentEvent RegisterFunc
            {
                get
                {
                    if (_registerFunc == null)
                    {
                        _registerFunc = (Cell2D cell, ComponentTypeKey type) =>
                        {
                            Component component = cell.ComponentReg.RegisterComponent(type);
                        };
                    }
                    return _registerFunc;
                }
                set => _registerFunc = value;
            }

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
            public ComponentVisitor(Cell2D.Component component) => _type = component.GetTypeKey();

            // ======== [[ METHODS ]] ======================================================= >>>>
            public void Visit(Cell2D cell)
            {
                Cell2D.ComponentRegistry componentRegistry = cell.ComponentReg;

                // Check if the component exists
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
    }
}
