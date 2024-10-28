using System;

using UnityEngine;

namespace Darklight.UnityExt.Behaviour
{
    public abstract class ScriptableDataBase : ScriptableObject { }

    public abstract class ScriptableData<T> : ScriptableDataBase
        where T : new()
    {
        // Use [SerializeReference] only if T is a class
        [SerializeReference] T _dataClass = default;
        [SerializeField] T _dataStruct = default;
        bool IsClassType => typeof(T).IsClass;

        public void SetData(T data)
        {
            if (IsClassType) _dataClass = data;
            else _dataStruct = data;
        }

        public T ToData() => IsClassType ? _dataClass : _dataStruct;

        public virtual void Refresh()
        {
            if (IsClassType)
            {
                if (_dataClass == null) _dataClass = new T();
            }
            else
            {
                _dataStruct = new T(); // Structs are non-nullable, so this resets to default.
            }
        }
    }
}
