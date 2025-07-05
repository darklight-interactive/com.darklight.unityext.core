using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Editor.Utility
{
#if UNITY_EDITOR

    public static class SceneViewUtility
    {
        public enum ViewDirection
        {
            TOP,
            LEFT,
            FRONT,
            RIGHT,
            BACK,
            BOTTOM
        }

        public static Vector3 GetDirectionVector(ViewDirection direction)
        {
            switch (direction)
            {
                case ViewDirection.TOP:
                    return Vector3.up;
                case ViewDirection.BOTTOM:
                    return Vector3.down;
                case ViewDirection.LEFT:
                    return Vector3.left;
                case ViewDirection.RIGHT:
                    return Vector3.right;
                case ViewDirection.FRONT:
                    return Vector3.forward;
                case ViewDirection.BACK:
                    return Vector3.back;
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Snaps the Scene view camera to a specified direction.
        /// </summary>
        /// <param name="direction">Direction vector to snap the camera to.</param>
        public static void SnapViewToDirection(Vector3 direction)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.LookAt(sceneView.pivot, Quaternion.LookRotation(direction));
                sceneView.Repaint();
            }
            else
            {
                Debug.LogWarning("No active Scene view found to snap the camera.");
            }
        }

        /// <summary>
        /// Snaps the Scene view camera to a specified direction.
        /// </summary>
        /// <param name="direction">Direction to snap the camera to.</param>
        public static void SnapViewToDirection(ViewDirection direction)
        {
            SnapViewToDirection(GetDirectionVector(direction));
        }

        /// <summary>
        /// Snaps the Scene view camera to a specified position.
        /// </summary>
        /// <param name="position">Position to snap the camera to.</param>
        public static void SnapViewToPosition(Vector3 position)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.LookAt(position, sceneView.rotation);
                sceneView.Repaint();
            }
        }

        public static void SnapViewToPositionAndDirection(Vector3 position, Vector3 direction)
        {
            SnapViewToPosition(position);
            SnapViewToDirection(direction);
        }

        /// <summary>
        /// Rotates the Scene view camera around the pivot point.
        /// </summary>
        /// <param name="angle">Angle in degrees to rotate around the pivot.</param>
        public static void RotateView(float angle)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.rotation = Quaternion.Euler(0, angle, 0) * sceneView.rotation;
                sceneView.Repaint();
            }
        }

        /// <summary>
        /// Resets the Scene view camera to the default position and orientation.
        /// </summary>
        public static void ResetView()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.LookAt(Vector3.zero, Quaternion.identity, 10f);
                sceneView.Repaint();
            }
        }

        /// <summary>
        /// Snaps the Scene view camera to the selected GameObject.
        /// </summary>
        public static void SnapToSelected()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && Selection.activeTransform != null)
            {
                sceneView.LookAt(Selection.activeTransform.position, sceneView.rotation);
                sceneView.Repaint();
            }
            else
            {
                Debug.LogWarning("No GameObject selected to snap the camera.");
            }
        }

        /// <summary>
        /// Toggles the visibility of the Scene view grid.
        /// </summary>
        public static void ToggleGrid()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.showGrid = !sceneView.showGrid;
                sceneView.Repaint();
            }
        }

        /// <summary>
        /// Frames the Scene view camera on the selected GameObject(s).
        /// </summary>
        public static void FrameSelected()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && Selection.transforms.Length > 0)
            {
                sceneView.FrameSelected();
            }
            else
            {
                Debug.LogWarning("No GameObject selected to frame.");
            }
        }
    }
#endif
}
