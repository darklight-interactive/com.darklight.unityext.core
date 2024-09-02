using UnityEngine;
using Darklight.UnityExt.Behaviour.Interface;


namespace Darklight.UnityExt.Game.Grid
{
    [System.Serializable]
    public abstract class Grid2D_Component : MonoBehaviour, IComponent<Grid2D, Grid2D_Component.Type>
    {
        [SerializeField] int _guid;
        Grid2D _base;
        Type _tag;

        public int Guid => _guid;
        public Grid2D Base => _base;
        public Type Tag => _tag;

        public Grid2D_Component(Grid2D baseComponent)
        {
            _guid = System.Guid.NewGuid().GetHashCode();
            _base = baseComponent;
        }

        public abstract void Initialize();
        public abstract void Update();
        public abstract void DrawGizmos();
        public abstract void DrawEditorGizmos();

        // ======== [[ NESTED TYPES ]] ================================== >>>>
        public enum Type
        {
            BASE = 0,
            CONFIG = 1
        }
    }


}
