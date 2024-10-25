using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Utility;

using NaughtyAttributes;

using UnityEngine;

using Camera = UnityEngine.Camera;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Darklight.UnityExt.Game
{

    /// <summary>
    /// This Camera Rig is the main Monobehaviour reference for the full Camera System. 
    /// It should be set as the parent object for all cameras in the scene.
    /// </summary>
    [ExecuteAlways]
    public class CameraRig : MonoBehaviour
    {
        [SerializeField, ShowOnly] Vector3 _rigOrigin;
        [SerializeField, ShowOnly] Vector3 _targetPosition;
        [SerializeField, ShowOnly] Quaternion _targetRotation;
        [SerializeField, ShowOnly] float _targetFOV;


        [Header("Cameras")]
        [SerializeField] Camera _mainCamera;
        [SerializeField, ShowOnly] List<Camera> _overlayCameras = new List<Camera>();

        [Header("Settings")]
        [SerializeField] Transform _followTarget;
        [SerializeField] CameraRigSettings _settings;


        [Header("Debug")]
        [SerializeField] bool _showGizmos;
        [SerializeField] bool _lerpInEditor;

        // << PROPERTIES >> -------------------------------------------------

        public virtual Vector3 Origin
        {
            get
            {
                if (_followTarget != null)
                    _rigOrigin = _followTarget.position;
                return _rigOrigin;
            }
        }
        public Vector3 CameraPosition => _mainCamera.transform.position;
        public virtual Quaternion CameraRotation => _mainCamera.transform.rotation;
        public float CameraZOffset => Mathf.Abs(_settings.PositionOffsetZ);
        public float CameraFOV => _settings.FOV;
        public float CameraAspect => _mainCamera.aspect;
        public Transform FollowTarget => _followTarget;
        public CameraRigSettings Settings => _settings;
        public float HalfWidth
        {
            get
            {
                // Calculate the half width of the camera frustum at the target depth
                float halfWidth = Mathf.Tan(0.5f * Mathf.Deg2Rad * _targetFOV) * CameraZOffset * CameraAspect;
                return Mathf.Abs(halfWidth); // Return the absolute value
            }
        }
        public float HalfHeight
        {
            get
            {
                // Calculate the half-height of the frustum at the given distance offset
                float HalfHeight = Mathf.Tan(0.5f * Mathf.Deg2Rad * _targetFOV) * CameraZOffset;
                return Mathf.Abs(HalfHeight); // Return the absolute value
            }
        }


        #region ( EDITOR UPDATE ) <PRIVATE_METHODS> ================================================
        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

        void EditorUpdate()
        {
            // This ensures smooth updates in the editor
            if (!Application.isPlaying)
            {
                UpdateCameraRig(_lerpInEditor);
            }
        }
        #endregion

        #region ( UNITY_RUNTIME ) <PRIVATE_METHODS> ================================================
        void Awake()
        {
            // -- MAIN CAMERA -------- >>
            if (_mainCamera == null)
            {
                // Try to find the main camera in the scene
                if (Camera.main != null)
                    _mainCamera = Camera.main;
                // Try to find a camera in the children of this object
                else if (GetComponentInChildren<Camera>() != null)
                    _mainCamera = GetComponentInChildren<Camera>();
                // Create a new camera if one doesn't exist
                else
                    _mainCamera = new GameObject("Main Camera").AddComponent<Camera>();
            }

            // < INITIALIZE MAIN CAMERA >
            _mainCamera.transform.SetParent(transform);
        }

        void FixedUpdate()
        {
            if (Application.isPlaying)
                UpdateCameraRig(true);
        }
        #endregion

        #region < NONPUBLIC_METHODS > [[ CALCULATIONS ]] ================================================================ 
        /// <summary>
        /// Calculate the target position of the camera based on the preset values.
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 CalculateTargetPosition()
        {
            Vector3 offset = new Vector3(
                _settings.PositionOffsetX,
                _settings.PositionOffsetY,
                _settings.PositionOffsetZ
            );

            Vector3 adjustedPosition = Origin + offset;

            if (Mathf.Abs(_settings.OrbitAngle) > 0)
            {
                // Calculate the orbit position based on the radius and current offset (angle in degrees)
                float orbitRadians = (_settings.OrbitAngle + 90) * Mathf.Deg2Rad; // Convert degrees to radians

                // Set the radius to match the z offset 
                float orbitRadius = _settings.PositionOffsetZ;

                // Calculate orbit based off of enforced bounds 
                Vector3 orbitPosition = new Vector3(
                    adjustedPosition.x + Mathf.Cos(orbitRadians) * orbitRadius,
                    adjustedPosition.y, // Keep the camera at the desired height
                    Origin.z + Mathf.Sin(orbitRadians) * orbitRadius
                );
                adjustedPosition = orbitPosition;

            }

            return adjustedPosition;
        }

        protected virtual Quaternion CalculateTargetRotation()
        {
            Quaternion targetRotation = Quaternion.Euler(Vector3.zero);

            if (_settings.OrbitTarget)
            {
                Vector3 camPosition = _targetPosition;
                targetRotation = Quaternion.LookRotation(Origin - camPosition);
                targetRotation = Quaternion.Euler(_settings.RotOffsetX, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
            }
            else
            {
                targetRotation = Quaternion.Euler(
                    _settings.RotOffsetX,
                    _settings.RotOffsetY,
                    _settings.RotOffsetZ
                );
            }

            return targetRotation;
        }

        protected virtual float CalculateTargetFOV()
        {
            return _settings.FOV;
        }

        #endregion

        #region ( INTERNAL_UPDATE ) <PRIVATE_METHODS> ================================================
        void UpdateCameraRig(bool useLerp)
        {
            // Update the main camera
            UpdateMainCamera(_mainCamera, useLerp);

            // Find all overlay cameras in the main camera's hierarchy
            _overlayCameras = _mainCamera.GetComponentsInChildren<Camera>().ToList();
            UpdateOverlayCameras(_overlayCameras);
        }

        void UpdateMainCamera(Camera cam, bool useLerp)
        {
            // << CALCULATE TARGET VALUES >> -------------------------------------
            _targetPosition = CalculateTargetPosition();
            _targetRotation = CalculateTargetRotation();
            _targetFOV = CalculateTargetFOV();

            // << UPDATE CAMERA VALUES >> -------------------------------------
            if (useLerp)
            {
                // ( Lerp Camera Position ) ---------------------------------------
                cam.transform.position = Vector3.Lerp(_mainCamera.transform.position, _targetPosition, _settings.PosSpeed * Time.deltaTime);

                // ( Slerp Camera Rotation ) ---------------------------------------
                cam.transform.rotation = Quaternion.Slerp(_mainCamera.transform.rotation, _targetRotation, _settings.RotSpeed * Time.deltaTime);

                // ( Lerp Camera Field of View ) ---------------------------------
                cam.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _targetFOV, _settings.FOVSpeed * Time.deltaTime);
            }
            else
            {
                // ( Set Camera Position ) ---------------------------------------
                cam.transform.position = _targetPosition;

                // ( Set Camera Rotation ) ---------------------------------------
                cam.transform.rotation = _targetRotation;

                // ( Set Camera Field of View ) ---------------------------------
                cam.fieldOfView = _targetFOV;
            }
        }

        void UpdateOverlayCameras(List<Camera> cameras)
        {
            foreach (Camera camera in cameras)
            {
                if (camera == _mainCamera) continue;
                if (camera.transform.parent != _mainCamera.transform)
                    camera.transform.SetParent(_mainCamera.transform);

                // Reset the local position and rotation of the camera
                camera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                camera.fieldOfView = _mainCamera.fieldOfView;
            }
        }

        #endregion

        #region ( HANDLERS ) <PUBLIC_METHODS> ================================================
        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
        }
        #endregion

        #region ( GIZMOS ) <PRIVATE_METHODS> ================================================

        void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            //if (_bounds != null)
            //_bounds.DrawGizmos();

            // Draw the camera frustum
            Gizmos.color = Color.yellow;
            DrawCameraFrustum(_mainCamera);

            // Draw the camera view
            Gizmos.color = Color.cyan;
            DrawCameraView();
        }

        void DrawCameraFrustum(Camera cam)
        {
            Vector3[] frustumCorners = new Vector3[4];
            cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), CameraZOffset, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

            // Transform the corners to world space considering the entire transform hierarchy
            for (int i = 0; i < 4; i++)
            {
                frustumCorners[i] = cam.transform.TransformPoint(frustumCorners[i]);
            }

            // Draw the frustum edges
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(frustumCorners[0], frustumCorners[1]); // Bottom-left to Bottom-right
            Gizmos.DrawLine(frustumCorners[1], frustumCorners[2]); // Bottom-right to Top-right
            Gizmos.DrawLine(frustumCorners[2], frustumCorners[3]); // Top-right to Top-left
            Gizmos.DrawLine(frustumCorners[3], frustumCorners[0]); // Top-left to Bottom-left

            // Draw lines from the camera position to each corner
            Vector3 camPosition = cam.transform.position;
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(camPosition, frustumCorners[i]);
            }
        }

        void DrawCameraView()
        {
            Vector3[] frustumCorners = new Vector3[4];
            _mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), CameraZOffset, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
            for (int i = 0; i < 4; i++)
            {
                frustumCorners[i] = _mainCamera.transform.TransformPoint(frustumCorners[i]);
            }

            Vector3 worldMin = Vector3.Min(frustumCorners[0], frustumCorners[2]);
            Vector3 worldMax = Vector3.Max(frustumCorners[1], frustumCorners[3]);

            Gizmos.DrawLine(frustumCorners[0], frustumCorners[1]);
            Gizmos.DrawLine(frustumCorners[1], frustumCorners[2]);
            Gizmos.DrawLine(frustumCorners[2], frustumCorners[3]);
            Gizmos.DrawLine(frustumCorners[3], frustumCorners[0]);
        }
        #endregion



#if UNITY_EDITOR
        [CustomEditor(typeof(CameraRig), true)]
        public class CameraRigCustomEditor : UnityEditor.Editor
        {
            SerializedObject _serializedObject;
            CameraRig _script;
            private void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (CameraRig)target;
                _script.Awake();
            }

            public override void OnInspectorGUI()
            {
                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                // Manually draw the bounds field
                //EditorGUILayout.PropertyField(_serializedObject.FindProperty("_bounds"));

                base.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();

                    // Update the camera rig when properties change
                    if (!_script._lerpInEditor)
                        _script.UpdateCameraRig(false);

                    EditorUtility.SetDirty(target);
                    SceneView.RepaintAll(); // Ensure the Scene view is refreshed
                }
            }
        }
#endif
    }
}
