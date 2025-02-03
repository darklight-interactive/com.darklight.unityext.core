using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Core3D;
using Darklight.UnityExt.Input;
using Darklight.UnityExt.Utility;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Core3D
{
    [RequireComponent(typeof(UniversalInputManager))]
    public class Game3DManager : MonoBehaviourSingleton<Game3DManager>
    {
        public const string ASSET_PATH = "Assets/Resources/Darklight/Game3D";
        public static WorldSpaceBounds WorldBounds
        {
            get => Instance._bounds;
        }

        [SerializeField]
        bool _enableBounds = true;

        [SerializeField, Expandable, ShowIf("_enableBounds")]
        WorldSpaceBounds _bounds;

        public override void Initialize()
        {
            if (!_enableBounds)
            {
                _bounds = null;
                return;
            }

            if (_bounds == null)
            {
                _bounds = ScriptableObjectUtility.CreateOrLoadScriptableObject<WorldSpaceBounds>(
                    ASSET_PATH + "/World",
                    "DefaultWorld3DBounds"
                );
            }
        }

        void OnDrawGizmos()
        {
            if (_bounds != null)
                _bounds.DrawGizmos();
        }
    }
}
