using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight.UnityExt.Game
{
    [RequireComponent(typeof(PlayerInput))]
    public class SimplePlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;

        private Vector2 moveInput;


        // Start is called before the first frame update
        void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
            {
                Debug.LogError("PlayerInput component not found.");
                return;
            }

            _playerInput.onActionTriggered += OnActionTriggered;
        }

        void OnDestroy()
        {
            // Unsubscribe from input events
            if (_playerInput != null)
            {
                _playerInput.onActionTriggered -= OnActionTriggered;
            }
        }

        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveInput")
            {
                // Read the input value
                moveInput = context.ReadValue<Vector2>();
            }
        }

        private void Update()
        {
            // Move the transform based on the input
            if (moveInput != Vector2.zero)
            {
                Vector3 move = new Vector3(moveInput.x, moveInput.y, 0);
                transform.Translate(move * Time.deltaTime);
            }
        }
    }
}
