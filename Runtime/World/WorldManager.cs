using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.World
{
    public class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        public static WorldBounds WorldBounds
        {
            get
            {
                if (Instance != null)
                    return Instance._worldBounds;
                return null;
            }
        }

        [SerializeField, Expandable] WorldBounds _worldBounds;

        public override void Initialize()
        {

        }

        void OnDrawGizmosSelected()
        {
            _worldBounds?.DrawGizmos();
        }
    }
}
