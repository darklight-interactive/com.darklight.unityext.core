using Darklight.UnityExt.Input;
using UnityEngine;

namespace Darklight.UnityExt.Core3D
{
    [RequireComponent(typeof(Rigidbody), (typeof(BoxCollider)))]
    public class Player3DController : UniversalInputController
    {
        const string PREFIX = "<color=green>[Player3DController]</color> ";
        Rigidbody _rb;

        bool _isPreloaded = false;


        [Header("Settings")]
        public int speed = 10;

        void Start() => Preload();
        void Preload()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError(PREFIX + "Rigidbody is not found.");
                return;
            }

            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _isPreloaded = true;
        }

        void Update()
        {
            if (!_isPreloaded) return;

            // Move the player based on input
            _rb.linearVelocity = new Vector3(MoveInput.x, 0, MoveInput.y) * speed;

            // Clamp the position to the nearest whole number
            Vector3 clampedPosition = new Vector3(
                Mathf.Round(_rb.position.x),
                Mathf.Round(_rb.position.y),
                Mathf.Round(_rb.position.z)
            );

            // Apply the clamped position
            _rb.MovePosition(clampedPosition);
        }

    }
}