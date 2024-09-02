using System.Collections.Generic;
using Darklight.UnityExt.Behaviour.Interface;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    [RequireComponent(typeof(Grid2D_OverlapComponent), typeof(Grid2D_WeightComponent))]
    public class Grid2D_WeightedSpawnerComponent : Grid2D_Component
    {
        Grid2D_OverlapComponent _overlapComponent;
        Grid2D_WeightComponent _weightComponent;

        // ======== [[ METHODS ]] ================================== >>>>
        public override void Initialize(Grid2D baseObj)
        {
            base.Initialize(baseObj);

            _overlapComponent = GetComponent<Grid2D_OverlapComponent>();
            if (_overlapComponent == null)
                _overlapComponent = gameObject.AddComponent<Grid2D_OverlapComponent>();

            _weightComponent = GetComponent<Grid2D_WeightComponent>();
            if (_weightComponent == null)
                _weightComponent = gameObject.AddComponent<Grid2D_WeightComponent>();

        }
    }
}