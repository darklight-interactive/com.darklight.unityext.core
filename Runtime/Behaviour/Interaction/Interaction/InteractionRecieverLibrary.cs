using System.Collections.Generic;
using Darklight.Collection;
using Darklight.Utility;
using UnityEngine;

namespace Darklight.Behaviour
{
    [System.Serializable]
    public class InteractionRecieverLibrary
        : CollectionDictionary<InteractionType, InteractionReciever>
    {
        public InteractionRecieverLibrary()
        {
            this.Refresh();
        }

        //public override void Clear() => base.Clear();
    }
}
