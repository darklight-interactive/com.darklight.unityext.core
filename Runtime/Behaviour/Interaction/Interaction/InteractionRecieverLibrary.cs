using System.Collections.Generic;
using Darklight.Library;
using Darklight.Utility;
using UnityEngine;

namespace Darklight.Behaviour
{
    [System.Serializable]
    public class InteractionRecieverLibrary
        : EnumComponentLibrary<InteractionType, InteractionReciever>
    {
        public InteractionRecieverLibrary()
        {
            ReadOnlyKey = true;
            ReadOnlyValue = true;
            RequiredKeys = new InteractionType[] { };
            this.Refresh();
        }

        protected override void InternalClear()
        {
            //Debug.Log("InteractionRecieverLibrary: InternalClear called.");
            base.InternalClear();
        }
    }
}
