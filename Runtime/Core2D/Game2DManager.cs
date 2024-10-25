using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Core2D;
using Darklight.UnityExt.Input;
using Darklight.UnityExt.Utility;
using NaughtyAttributes;
using UnityEngine;


[RequireComponent(typeof(UniversalInputManager))]
public class Game2DManager : MonoBehaviourSingleton<Game2DManager>
{
    public const string ASSET_PATH = "Assets/Resources/Darklight/Game2D";
    public static World2DBounds WorldBounds { get => Instance._bounds; }
    [SerializeField, Expandable] World2DBounds _bounds;

    public override void Initialize()
    {
        if (_bounds == null)
            _bounds = ScriptableObjectUtility.CreateOrLoadScriptableObject<World2DBounds>(ASSET_PATH + "/World", "DefaultWorld2DBounds");
    }

    void OnDrawGizmos()
    {
        if (_bounds != null)
            _bounds.DrawGizmos();
    }
}