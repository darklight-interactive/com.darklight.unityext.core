/*
 * --------------------------------------||----->>
 * Darklight Interactive Core Plugin
 * Copyright (c) 2024 Darklight Interactive. All rights reserved.
 * ----------------------------------------------------------------- [[ )) 
 * Licensed under the Darklight Interactive Software License Agreement.
 * See LICENSE.md file in the project root for full license information.
 * ---------------------------------------------------------------------------- [[ )) 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ----------------------------------------------------------------- [[ )) 
 * For questions regarding this software or licensing, please contact:
 * - Email: skysfalling22@gmail.com
 * - Discord: skysfalling
 * =========================================================== }}
 * Major Authors: 
 * Sky Casey
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using static Darklight.UnityExt.Editor.CustomInspectorGUI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Game.Camera
{
    /// <summary>
    /// This Camera Rig is the main Monobehaviour reference for the full Camera System. 
    /// It should be set as the parent object for all cameras in the scene.
    /// </summary>
    [ExecuteAlways]
    public class CameraRig : MonoBehaviour
    {

        // << GET ALL CAMERAS IN CHILDREN >> -----------------------------------
        [Header("Cameras")]
        [Tooltip("All cameras that are children of this object.")]
        [SerializeField] UnityEngine.Camera[] _camerasInChildren = new UnityEngine.Camera[0];
        public void GetCamerasInChildren()
        {
            _camerasInChildren = GetComponentsInChildren<UnityEngine.Camera>();
        }

        // << FOCUS TARGET >> -------------------------------------------------
        [Header("Focus Target")]
        [Tooltip("The target that the camera will focus on.")]
        [SerializeField] private Transform _focusTarget;
        [SerializeField, ShowOnly] private Vector3 _focusTargetPosition = Vector3.zero;
        [SerializeField, ShowOnly] private Vector3 _focusTargetPositionOffset = Vector3.zero;
        [SerializeField, Range(-5f, 5)] float _focusOffsetY = 0f;
        public void SetFocusTarget(Transform target)
        {
            _focusTarget = target;
        }
        private CameraBounds cameraBounds;

        [SerializeField, ShowOnly] private Vector3 _offsetPosition;
        public void SetOffsetRotation(Transform mainTarget, Transform secondTarget)
        {
            float mainX = mainTarget.position.x;
            float secondX = secondTarget.position.x;
            float middleX = (secondX - mainX) / 2;
        }

        [Header("Lerp Speed")]
        [SerializeField, Range(0, 10)] private float _positionLerpSpeed = 10f;
        [SerializeField, Range(0, 10)] private float _rotationLerpSpeed = 10f;
        [SerializeField, Range(0, 10)] private float _fovLerpSpeed = 10f;

        // TODO : Rotate the Camera around the target
        // this is instead of including _distanceX

        [Space(10), Header("Distance")]
        [SerializeField, Range(-3000, 3000)] private float _distanceX = 0f; // distance from the target on the X axis
        [SerializeField, Range(-3000, 3000)] private float _distanceY = 0f; // distance from the target on the Y axis
        [SerializeField, Range(-3000, 3000)] private float _distanceZ = 10f; // distance from the target on the Z axis



        [Header("Field of View")]
        [SerializeField, Range(0.1f, 190)] private float _baseFOV = 5f;
        [SerializeField, ShowOnly] private float _offsetFOV;
        public void SetOffsetFOV(float value)
        {
            _offsetFOV = value;
        }
        [SerializeField, ShowOnly] private float _currentFOV;
        public float GetCurrentFOV()
        {
            _currentFOV = _baseFOV + _offsetFOV;
            return _currentFOV;
        }

        private void Awake() {
            CameraBounds[] bounds = FindObjectsByType<CameraBounds>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (bounds.Length > 0)
            {
                cameraBounds = bounds[0];
            }
            else
            {
                cameraBounds = null;
            }
        }

        public virtual void Update()
        {
            GetCamerasInChildren();

            // << IF THERE IS NO FOCUS TARGET, SET THE POSITION TO ZERO >>
            if (_focusTarget == null)
            {
                _focusTargetPosition = Vector3.zero;
            }
            else
            {
                _focusTargetPosition = _focusTarget.position;
            }

            // set the offsets
            _offsetPosition = new Vector3(_distanceX, _distanceY, _distanceZ);
            _focusTargetPositionOffset = new Vector3(0, _focusOffsetY, 0);

            // set the position
            Vector3 newPosition = _focusTargetPosition + _offsetPosition;
            Vector3 offsetDirection = (newPosition - transform.position).normalized;
            float halfWidth = Mathf.Tan(0.5f*Mathf.Deg2Rad*GetCurrentFOV())*_distanceZ*_camerasInChildren[0].aspect;
            if (cameraBounds)
            {
                if ((transform.position.x - halfWidth > cameraBounds.leftBound && offsetDirection.x < 0) || (transform.position.x + halfWidth < cameraBounds.rightBound && offsetDirection.x > 0)){
                    transform.position = Vector3.Lerp(transform.position, newPosition, _positionLerpSpeed * Time.deltaTime);
                }
            }
            else{
                transform.position = Vector3.Lerp(transform.position, newPosition, _positionLerpSpeed * Time.deltaTime);
            }

            // set the rotation
            Quaternion newRotation = GetLookRotation(newPosition, _focusTargetPosition + _focusTargetPositionOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, _rotationLerpSpeed * Time.deltaTime);

            // << UPDATE ALL CAMERAS >>
            foreach (UnityEngine.Camera camera in _camerasInChildren)
            {
                if (camera != null)
                {
                    // Reset the local position and rotation of the camera
                    camera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                    // Lerp the field of view
                    camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, GetCurrentFOV(), _fovLerpSpeed * Time.deltaTime);
                }
            }
        }

        Quaternion GetLookRotation(Vector3 originPosition, Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - originPosition).normalized;
            if (direction == Vector3.zero) return Quaternion.identity;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            return lookRotation;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CameraRig))]
    public class CameraRigCustomEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        CameraRig _script;
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (CameraRig)target;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
