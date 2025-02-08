using System;
using System.Collections;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Input
{
    public abstract class UniversalInputController : MonoBehaviour
    {
        Vector2 _moveInput;
        bool _moveInputStarted;
        bool _moveInputCanceled;
        bool _primaryInteractStarted;
        bool _primaryInteractCanceled;
        bool _secondaryInteractStarted;
        bool _secondaryInteractCanceled;
        bool _menuButtonStarted;
        bool _menuButtonCanceled;

        public Vector2 MoveInput => _moveInput;
        public bool IsMoveInputActive => MoveInput.magnitude > 0;
        public bool IsMoveInputStarted => _moveInputStarted;
        public bool IsMoveInputCanceled => _moveInputCanceled;
        public bool IsPrimaryInteractStarted => _primaryInteractStarted;
        public bool IsPrimaryInteractCanceled => _primaryInteractCanceled;
        public bool IsSecondaryInteractStarted => _secondaryInteractStarted;
        public bool IsSecondaryInteractCanceled => _secondaryInteractCanceled;
        public bool IsMenuButtonStarted => _menuButtonStarted;
        public bool IsMenuButtonCanceled => _menuButtonCanceled;

        public Action OnMoveInputStarted;
        public Action<Vector2> OnMoveInput;
        public Action OnMoveInputCanceled;
        public Action OnPrimaryInteractStarted;
        public Action OnPrimaryInteractCanceled;
        public Action OnSecondaryInteractStarted;
        public Action OnSecondaryInteractCanceled;
        public Action OnMenuButtonStarted;
        public Action OnMenuButtonCanceled;

        protected virtual void Awake()
        {
            EnableInputs();

            if (Application.isPlaying && UniversalInputManager.Instance == null)
                Debug.LogError("UniversalInputManager is not attached to the scene!");
        }

        #region < NONPUBLIC_METHODS > [[ ENABLE/DISABLE INPUTS ]] ================================================================
        protected virtual void EnableInputs()
        {
            UniversalInputManager.OnMoveInputStarted += HandleOnMoveInputStarted;
            UniversalInputManager.OnMoveInput += HandleOnMoveInput;
            UniversalInputManager.OnMoveInputCanceled += HandleOnMoveInputCanceled;
            UniversalInputManager.OnPrimaryInteract += HandleOnPrimaryInteractStarted;
            UniversalInputManager.OnPrimaryInteractCanceled += HandleOnPrimaryInteractCanceled;
            UniversalInputManager.OnSecondaryInteract += HandleOnSecondaryInteractStarted;
            UniversalInputManager.OnSecondaryInteractCanceled += HandleOnSecondaryInteractCanceled;
            UniversalInputManager.OnMenuButton += HandleOnMenuButtonStarted;
        }

        protected virtual void DisableInputs()
        {
            UniversalInputManager.OnMoveInputStarted -= HandleOnMoveInputStarted;
            UniversalInputManager.OnMoveInput -= HandleOnMoveInput;
            UniversalInputManager.OnMoveInputCanceled -= HandleOnMoveInputCanceled;
            UniversalInputManager.OnPrimaryInteractCanceled -= HandleOnPrimaryInteractCanceled;
            UniversalInputManager.OnSecondaryInteractCanceled -= HandleOnSecondaryInteractCanceled;
            UniversalInputManager.OnMenuButton -= HandleOnMenuButtonStarted;
        }
        #endregion


        #region < NONPUBLIC_METHODS > [[ HANDLE MOVE INPUT ]] ==================================================================

        protected virtual void HandleOnMoveInputStarted(Vector2 moveInput)
        {
            _moveInput = moveInput;
            _moveInputStarted = true;
            OnMoveInputStarted?.Invoke();
        }

        protected virtual void HandleOnMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }

        protected virtual void HandleOnMoveInputCanceled()
        {
            _moveInput = Vector2.zero;
            _moveInputStarted = false;
            _moveInputCanceled = true;
            StartCoroutine(MoveInputCanceled());
            OnMoveInputCanceled?.Invoke();
        }

        IEnumerator MoveInputCanceled()
        {
            yield return new WaitForEndOfFrame();
            _moveInputCanceled = true;

            yield return new WaitForEndOfFrame();
            _moveInputCanceled = false;
        }
        #endregion

        #region < NONPUBLIC_METHODS > [[ HANDLE PRIMARY INTERACT INPUT ]] ======================================================

        protected virtual void HandleOnPrimaryInteractStarted()
        {
            if (_primaryInteractStarted)
                return;

            _primaryInteractStarted = true;
            _primaryInteractCanceled = false;
            OnPrimaryInteractStarted?.Invoke();
        }

        protected virtual void HandleOnPrimaryInteractCanceled()
        {
            if (_primaryInteractCanceled)
                return;

            _primaryInteractStarted = false;
            _primaryInteractCanceled = true;
            OnPrimaryInteractCanceled?.Invoke();
        }

        #endregion

        #region < NONPUBLIC_METHODS > [[ HANDLE SECONDARY INTERACT INPUT ]] ====================================================

        protected virtual void HandleOnSecondaryInteractStarted()
        {
            _secondaryInteractStarted = true;
            _secondaryInteractCanceled = false;
            OnSecondaryInteractStarted?.Invoke();
        }

        protected virtual void HandleOnSecondaryInteractCanceled()
        {
            _secondaryInteractStarted = false;
            _secondaryInteractCanceled = true;
            OnSecondaryInteractCanceled?.Invoke();
        }

        #endregion

        #region < NONPUBLIC_METHODS > [[ HANDLE MENU BUTTON INPUT ]] ==========================================================

        protected virtual void HandleOnMenuButtonStarted()
        {
            _menuButtonStarted = true;
            _menuButtonCanceled = false;
            OnMenuButtonStarted?.Invoke();
        }

        protected virtual void HandleOnMenuButtonCanceled()
        {
            _menuButtonStarted = false;
            _menuButtonCanceled = true;
            OnMenuButtonCanceled?.Invoke();
        }

        #endregion
    }
}
