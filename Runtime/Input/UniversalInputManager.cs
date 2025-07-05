using System.Collections.Generic;
using Darklight.Behaviour;
using Darklight.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Darklight.Input
{
    /// <summary>
    /// A MonoBehaviour singleton class that manages the input device type and the input actions for the current device.
    /// </summary>
    public class UniversalInputManager : MonoBehaviourSingleton<UniversalInputManager>
    {
        private bool _moveStarted;

        // -------------- [[ STATIC INPUT TYPE ]] -------------- >>
        public enum InputType
        {
            NULL,
            KEYBOARD,
            TOUCH,
            GAMEPAD
        }

        public static InputType DeviceInputType
        {
            get => Instance._deviceInputType;
            private set => Instance._deviceInputType = value;
        }

        // -------------- [[ SERIALIZED FIELDS ]] -------------- >>
        [SerializeField]
        private InputActionAsset _inputActionAsset;

        [SerializeField, ShowOnly]
        private InputType _deviceInputType;

        [SerializeField, ShowOnly]
        private List<string> _connectedDevices;

        [SerializeField, ShowOnly]
        private Vector2 _moveInput;

        [SerializeField, ShowOnly]
        private bool _primaryInteract;

        [SerializeField, ShowOnly]
        private bool _secondaryInteract;

        // -------------- [[ INPUT ACTION MAPS ]] -------------- >>
        InputActionMap _activeActionMap;
        InputActionMap _keyboardActionMap;
        InputActionMap _gamepadActionMap;
        InputActionMap _touchActionMap;

        // -------------- [[ INPUT ACTIONS ]] -------------- >>
        InputAction _moveInputAction => _activeActionMap.FindAction("MoveInput");
        InputAction _primaryButtonAction => _activeActionMap.FindAction("PrimaryInteract");
        InputAction _secondaryButtonAction => _activeActionMap.FindAction("SecondaryInteract");
        InputAction _menuButtonAction => _activeActionMap.FindAction("MenuButton");
        InputAction _mousePositionAction => _activeActionMap.FindAction("MousePosition");
        InputAction _mousePrimaryClickAction => _activeActionMap.FindAction("MousePrimaryClick");
        InputAction _mouseSecondaryClickAction =>
            _activeActionMap.FindAction("MouseSecondaryClick");

        // -------------- [[ INPUT EVENTS ]] -------------- >>
        public delegate void OnInput_Trigger();
        public delegate void OnInput_Vec2(Vector2 input);

        /// <summary> Event for the move input from the active device. </summary>
        public static event OnInput_Vec2 OnMoveInput;
        public static event OnInput_Vec2 OnMoveInputStarted;
        public static event OnInput_Trigger OnMoveInputCanceled;

        /// <summary> Event for the primary interaction input from the active device. </summary>
        public static event OnInput_Trigger OnPrimaryInteract;
        public static event OnInput_Trigger OnPrimaryInteractCanceled;

        /// <summary> Event for the secondary interaction input from the active device. </summary>
        public static event OnInput_Trigger OnSecondaryInteract;
        public static event OnInput_Trigger OnSecondaryInteractCanceled;

        /// <summary> Event for the menu button input from the active device. </summary>
        public static event OnInput_Trigger OnMenuButton;

        // << -- MOUSE INPUT EVENTS -- >>
        public static event OnInput_Vec2 OnMousePosition;
        public static event OnInput_Trigger OnMousePrimaryClick;
        public static event OnInput_Trigger OnMouseSecondaryClick;

        public InputActionMap ActiveActionMap => _activeActionMap;

        void OnEnable()
        {
            // Enable the input action asset
            _inputActionAsset.Enable();

            // Initialize all input action maps
            foreach (InputActionMap map in _inputActionAsset.actionMaps)
            {
                InitializeActionMap(map);
            }

            // Subscribe to device change event
            InputUser.onChange += OnDeviceChange;
        }

        void OnDisable()
        {
            // Disable the input action asset
            _inputActionAsset.Disable();

            // Disable all input action maps
            foreach (InputActionMap map in _inputActionAsset.actionMaps)
            {
                map.Disable();
            }

            // Unsubscribe to device change event
            InputUser.onChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (
                change == InputUserChange.ControlSchemeChanged
                || change == InputUserChange.DevicePaired
                || change == InputUserChange.DeviceUnpaired
            )
            {
                //string currentDevice = device.displayName;
                //Debug.Log("Current input device: " + currentDevice);
            }
        }

        private void OnDestroy()
        {
            ResetInputEvents();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Print all connected devices
            LoadAllConnectedDevices();
        }

        // Method to print all connected devices
        private void LoadAllConnectedDevices()
        {
            _connectedDevices = new List<string>();
            foreach (InputDevice device in InputSystem.devices)
            {
                _connectedDevices.Add(device.displayName);
            }
        }

        #region ---- [[ DEVICE INPUT DETECTION ]] ---->>

        bool InitializeActionMap(InputActionMap map)
        {
            // Set the active action map
            _activeActionMap = map;
            _activeActionMap.Enable();

            try
            {
                // Enable the actions
                _moveInputAction.Enable();
                _primaryButtonAction.Enable();
                _secondaryButtonAction.Enable();
                _menuButtonAction.Enable();
                _mousePrimaryClickAction.Enable();
                _mouseSecondaryClickAction.Enable();

                // << -- Set the input events -- >>
                _moveInputAction.started += HandleMoveStarted;
                _moveInputAction.performed += HandleMovePerformed;
                _moveInputAction.canceled += HandleMoveCanceled;

                _primaryButtonAction.performed += HandlePrimaryPerformed;
                _primaryButtonAction.canceled += HandlePrimaryCanceled;

                _secondaryButtonAction.performed += HandleSecondaryPerformed;
                _secondaryButtonAction.canceled += HandleSecondaryCanceled;

                _menuButtonAction.started += HandleMenuStarted;

                _mousePositionAction.performed += HandleMousePosition;
                _mousePrimaryClickAction.performed += HandleMousePrimaryClick;
                _mouseSecondaryClickAction.performed += HandleMouseSecondaryClick;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"{Prefix} {map.name} Error: {e.Message}");
                return false;
            }

            return true;
        }

        void ResetInputEvents()
        {
            Debug.Log($"{Prefix} Reset Input Events ");

            // Unsubscribe from all input events
            _activeActionMap?.Disable();

            try
            {
                _moveInputAction.started -= HandleMoveStarted;
                _moveInputAction.performed -= HandleMovePerformed;
                _moveInputAction.canceled -= HandleMoveCanceled;

                _primaryButtonAction.performed -= HandlePrimaryPerformed;
                _primaryButtonAction.canceled -= HandlePrimaryCanceled;

                _secondaryButtonAction.performed -= HandleSecondaryPerformed;
                _secondaryButtonAction.canceled -= HandleSecondaryCanceled;

                _menuButtonAction.started -= HandleMenuStarted;

                _mousePrimaryClickAction.performed -= HandleMousePrimaryClick;
                _mouseSecondaryClickAction.performed -= HandleMouseSecondaryClick;
                _mousePositionAction.performed -= HandleMousePosition;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"{Prefix} Reset Input Events Error: {e.Message}");
            }
        }
        #endregion

        private void HandleMoveStarted(InputAction.CallbackContext ctx)
        {
            if (!_moveStarted)
            {
                Vector2 input = ctx.ReadValue<Vector2>();
                OnMoveInputStarted?.Invoke(input);
                _moveStarted = true;
            }
        }

        private void HandleMovePerformed(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
            OnMoveInput?.Invoke(_moveInput);
        }

        private void HandleMoveCanceled(InputAction.CallbackContext ctx)
        {
            _moveStarted = false;
            _moveInput = Vector2.zero;
            OnMoveInputCanceled?.Invoke();
        }

        private void HandlePrimaryPerformed(InputAction.CallbackContext ctx)
        {
            _primaryInteract = true;
            OnPrimaryInteract?.Invoke();
        }

        private void HandlePrimaryCanceled(InputAction.CallbackContext ctx)
        {
            _primaryInteract = false;
            OnPrimaryInteractCanceled?.Invoke();
        }

        private void HandleSecondaryPerformed(InputAction.CallbackContext ctx)
        {
            _secondaryInteract = true;
            OnSecondaryInteract?.Invoke();
        }

        private void HandleSecondaryCanceled(InputAction.CallbackContext ctx)
        {
            _secondaryInteract = false;
            OnSecondaryInteractCanceled?.Invoke();
        }

        private void HandleMenuStarted(InputAction.CallbackContext ctx)
        {
            OnMenuButton?.Invoke();
        }

        private void HandleMousePosition(InputAction.CallbackContext ctx)
        {
            OnMousePosition?.Invoke(ctx.ReadValue<Vector2>());
        }

        private void HandleMousePrimaryClick(InputAction.CallbackContext ctx)
        {
            OnMousePrimaryClick?.Invoke();
        }

        private void HandleMouseSecondaryClick(InputAction.CallbackContext ctx)
        {
            OnMouseSecondaryClick?.Invoke();
        }
    }
}
