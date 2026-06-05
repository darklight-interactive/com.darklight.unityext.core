using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.Behaviour;
using Darklight.Collections;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Behaviour
{
    [ExecuteAlways]
    public abstract class Interactor : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private bool _enabled;
        
        [SerializeField, ReadOnly]
        Interactable _currentTarget;

        public bool IsEnabled => _enabled;
        
        /// <summary>
        /// Represents the current target being interacted with by the interactor.
        /// </summary>
        /// <remarks>
        /// This property provides access to the currently active target object of type <see cref="Interactable"/>
        /// that the interactor is focused on. The property is read-only for external access and can only be
        /// updated internally. <br/> <br/>
        /// Setting this property will invoke appropriate actions based on the value being set:
        /// <list type="bullet">
        /// <item><description>If the value is set to null, the current target is lost, and <see cref="OnTargetLost"/> is triggered.</description></item>
        /// <item><description>If the value is different from the current target, the previous target is reset, and <see cref="OnTargetAccepted"/> is triggered.</description></item>
        /// </list>
        /// </remarks>
        /// <value>
        /// The <see cref="Interactable"/> object currently being targeted.
        /// </value>
        public Interactable CurrentTarget
        {
            get => _currentTarget;
            private set
            {
                // << TARGET LOST >>
                if (value == null)
                {
                    _currentTarget = null;
                    OnTargetLost?.Invoke();
                    return;
                }
                
                // << TARGET ACCEPTED >>
                // Reset old target if it's different from the new one'
                if (_currentTarget != null && _currentTarget != value)
                    _currentTarget.Reset();
                
                _currentTarget = value;
                OnTargetAccepted?.Invoke();
            }
        }
        public Vector2 HorzPosition => new Vector2(transform.position.x, transform.position.z);

        public Action OnEnabled;
        public Action OnDisabled;
        public Action OnTargetAccepted;
        public Action OnTargetLost;
        public Action OnInteractAccepted;
        
        public virtual void Enable()
        {
            _enabled = true;
            OnEnabled?.Invoke();
        }

        public virtual void Disable()
        {
            _enabled = false;
            OnDisabled?.Invoke();
            
            CurrentTarget = null;
        }
        
        protected bool TryGetInteractable(Collider col, out Interactable interactable)
        {
            interactable = null;
            
            if (col == null) return false;
            if (!col.TryGetComponent(out interactable)) return false;
            
            return true;
        }

        protected bool TrySetTarget(Interactable interactable, bool force = false)
        {
            // << TARGET LOST >>
            if (interactable == null)
            {
                if (force) ClearTarget();
                return false;
            }
            
            // << TARGET ALREADY SET >>
            if (!force && CurrentTarget != null)
                return false;
            
            // << TARGET ACCEPTED >>
            if (interactable.AcceptTarget(this, out string errMsg, force))
            {
                CurrentTarget = interactable;
                return true;
            }
            else
            {
                Debug.LogError(errMsg, this);
            }
            
            // << TARGET REJECTED >>
            if (force) ClearTarget();
            return false;
        }

        protected void ClearTarget()
        {
            if (CurrentTarget != null)
                CurrentTarget.Reset();
            CurrentTarget = null;
        }
        
        /// <summary>
        /// Attempts to perform an interaction with the specified interactable object.
        /// </summary>
        /// <param name="interactable">
        /// The interactable object to interact with. If null, the interaction cannot be performed.
        /// </param>
        /// <param name="errMsg">
        /// When the method returns, contains an error message if the interaction fails; otherwise, it will be null or empty.
        /// </param>
        /// <param name="force">
        /// Specifies whether to force the interaction to occur, even if the regular conditions are not met.
        /// </param>
        /// <returns>
        /// Returns true if the interaction is successfully performed; otherwise, false.
        /// </returns>
        protected bool TryInteractWith(Interactable interactable, out string errMsg, bool force = false)
        {
            errMsg = "";

            if (interactable == null)
            {
                errMsg = $"[{name}] InteractWith: Interactable is null";
                return false;
            }

            bool result = interactable.AcceptInteraction(this, out errMsg, force);
            if (result)
                OnInteractAccepted?.Invoke();
            return result;
        }

        public bool TryInteractWithTarget(out string errMsg) => TryInteractWith(_currentTarget, out errMsg);


        


    }
}
