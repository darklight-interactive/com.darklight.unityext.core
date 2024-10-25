using Darklight.UnityExt.Input;
using UnityEngine;

namespace Darklight.UnityExt.Core3D
{
    [RequireComponent(typeof(Rigidbody), (typeof(BoxCollider)))]
    public class IsoPlayer3DController : UniversalInputController
    {
        const string PREFIX = "<color=green>[Player3DController]</color> ";
        Rigidbody _rb;

        bool _isPreloaded = false;

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

        // Update is called once per frame
        void Update()
        {
            if (!_isPreloaded) return;

            _rb.linearVelocity = new Vector3(MoveInput.x, 0, MoveInput.y) * speed;
        }
    }
}