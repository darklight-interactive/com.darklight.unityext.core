
using System;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICell2DComponent
    {
        string Name { get; }
        int Guid { get; }
        Cell2D Cell { get; }
        TypeKey Type { get; }

        void Initialize(Cell2D cell);
        void Update();
        void DrawGizmos();
        void DrawEditorGizmos();
        void Copy(ICell2DComponent component);

        /// <summary>
        /// Enum to represent the different types of components that can be attached to a cell.
        /// Intended to be used as a bit mask to determine which components are present on a cell.
        /// </summary>
        public enum TypeKey
        {
            Base = 0,
            Overlap = 1 << 0,
            Shape = 1 << 1,
            Weight = 1 << 2
        }
    }






}
