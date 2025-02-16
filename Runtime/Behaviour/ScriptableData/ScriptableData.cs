using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Behaviour
{
    public abstract class ScriptableDataBase : ScriptableObject { }

    public abstract class ScriptableData<T> : ScriptableDataBase
        where T : new()
    {
        [SerializeField]
        protected T data = default;

        public virtual void SetData(T data) => this.data = data;

        public virtual T ToData() => data;

        public virtual void Refresh()
        {
            // Ensure the data is initialized. Structs are never null; classes can be initialized here.
            if (typeof(T).IsClass && data == null)
            {
                data = new T();
            }
        }

        // Implicit conversion to T
        public static implicit operator T(ScriptableData<T> scriptableData)
        {
            return scriptableData.ToData();
        }
    }
}
