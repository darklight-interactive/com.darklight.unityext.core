using System;

using UnityEngine;

#if UNITY_EDITOR

#endif

namespace Darklight.UnityExt.Behaviour
{
    public abstract class ScriptableDataBase : ScriptableObject { }
    public abstract class ScriptableData<T> : ScriptableDataBase
        where T : class
    {
        [SerializeReference] T _data = default;
        
        public virtual void SetData(T data) => _data = data;
        public virtual T ToData() => _data;

        public virtual void Refresh()
        {
            if (_data == null)
                _data = Activator.CreateInstance<T>();
        }
    }
}

