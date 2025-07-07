using System.Collections.Generic;
using Darklight.Collection;
using UnityEngine;

namespace Darklight.Behaviour
{
    [CreateAssetMenu(menuName = "Darklight/Interaction/Interactable/RequestPreset")]
    public class InteractionRequestDataObject : ScriptableObject
    {
        [SerializeField]
        CollectionDictionary<InteractionType, GameObject> _data =
            new CollectionDictionary<InteractionType, GameObject>();

        public CollectionDictionary<InteractionType, GameObject> DataCollection => _data;
    }
}
