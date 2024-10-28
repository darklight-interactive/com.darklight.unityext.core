using System;

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    public abstract class ScriptableDataBase : ScriptableObject { }

    public abstract class ScriptableData<T> : ScriptableDataBase
        where T : new()
    {
        [SerializeField] private T _data = default;

        public virtual void SetData(T data) => _data = data;
        public virtual T ToData() => _data;

        public virtual void Refresh()
        {
            // Ensure the data is initialized. Structs are never null; classes can be initialized here.
            if (typeof(T).IsClass && _data == null)
            {
                _data = new T();
            }
        }
    }
}
