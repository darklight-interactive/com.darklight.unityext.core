using System;
using UnityEngine;

namespace Darklight.Behaviour
{
    public abstract class InteractableData : ScriptableObject
    {
        [SerializeField]
        private int _id;

        [SerializeField]
        private string _key;

        [SerializeField]
        private LayerMask _layer;

        public int ID
        {
            get => _id;
            protected set => _id = value;
        }

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

        public virtual void GenerateData(Interactable interactable)
        {
            SetData(
                interactable.gameObject.GetInstanceID(),
                interactable.gameObject.name,
                interactable.gameObject.layer
            );
        }

        public virtual void SetData(int id, string key, LayerMask layer)
        {
            _id = id;
            _key = key;
            _layer = layer;
        }

        public virtual void CopyData(InteractableData data)
        {
            SetData(data.ID, data.Key, data.Layer);
        }
    }
}
