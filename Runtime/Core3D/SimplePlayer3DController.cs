using Darklight.UnityExt.Input;
using UnityEngine;

namespace Darklight.UnityExt.Core3D
{
    [RequireComponent(typeof(Rigidbody), (typeof(BoxCollider)))]
    public class SimplePlayer3DController : UniversalInputController
    {
        const string PREFIX = "<color=green>[Player3DController]</color> ";
        Rigidbody _rb;

        bool _isPreloaded = false;
        Vector3 _moveTarget = Vector3.zero;


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
            _moveTarget = transform.position;
            _moveTarget += new Vector3(MoveInput.x, 0, MoveInput.y) * speed;

            // Clamp the position to the nearest whole number
            Vector3 clampedPosition = new Vector3(
                Mathf.Round(_moveTarget.x),
                Mathf.Round(_moveTarget.y),
                Mathf.Round(_moveTarget.z)
            );

            // Apply the clamped position
            _rb.MovePosition(clampedPosition);
        }

    }
}