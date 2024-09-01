
using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    partial class Cell2D
    {
        /// <summary>
        /// Holds a function that can be applied to a cell.
        /// </summary>
        public class Visitor
        {
            public delegate void VisitFunction(Cell2D cell);

            private VisitFunction _visitFunction;
            public Visitor(VisitFunction visitFunction)
            {
                _visitFunction = visitFunction;
            }

            public void Visit(Cell2D cell)
            {
                _visitFunction(cell);
            }
        }
    }
}
