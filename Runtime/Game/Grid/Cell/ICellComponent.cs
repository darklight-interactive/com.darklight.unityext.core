using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public interface ICellComponent
    {

    }

    public interface IOverlap : ICellComponent
    {
        List<Collider2D> Colliders { get; }
        LayerMask LayerMask { get; }
        void UpdateColliders();
    }

    public interface IWeighted : ICellComponent
    {
        int Weight { get; }
        void UpdateWeight();
    }


    public interface IShape : ICellComponent
    {
        Shape2D Shape { get; }
        void UpdateShape(Vector3 center, float radius, int segments, Vector3 normal, Color gizmoColor);
    }
}


