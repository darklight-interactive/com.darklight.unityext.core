using System;
using UnityEngine;

namespace Darklight.Behaviour
{
    public abstract class InteractableData : ScriptableObject
    {
        [SerializeField]
        private string _key;

        [SerializeField]
        private LayerMask _layer;

        public string Key
        {
            get => _key;
            protected set => _key = value;
        }

        public LayerMask Layer
        {
            get => _layer;
            protected set => _layer = value;
        }

        public virtual void SetData(string key, LayerMask layer)
        {
            _key = key;
            _layer = layer;
        }

        public virtual void CopyData(InteractableData data)
        {
            SetData(data.Key, data.Layer);
        }
    }
}
