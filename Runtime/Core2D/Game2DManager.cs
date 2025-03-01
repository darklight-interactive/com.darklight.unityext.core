using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Core2D;
using Darklight.UnityExt.Input;
using Darklight.UnityExt.Utility;
using Darklight.UnityExt.World;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UniversalInputManager))]
public class Game2DManager : MonoBehaviourSingleton<Game2DManager>
{
    public const string ASSET_PATH = "Assets/Resources/Darklight/Game2D";
    public static TripleAxisBounds WorldBounds
    {
        get => Instance._bounds;
    }

    [SerializeField, Expandable]
    TripleAxisBounds _bounds;

    public override void Initialize()
    {
        if (_bounds == null)
            _bounds = ScriptableObjectUtility.CreateOrLoadScriptableObject<TripleAxisBounds>(
                ASSET_PATH + "/World",
                "DefaultWorld2DBounds"
            );
    }

    void OnDrawGizmos()
    {
        if (_bounds != null)
            _bounds.DrawGizmos();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
