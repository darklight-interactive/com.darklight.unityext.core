using Darklight.UnityExt.Game;
using Darklight.UnityExt.World;
using UnityEngine;

namespace Darklight.UnityExt.Core2D
{
    public class CameraRig2D : CameraRig
    {
        [SerializeField]
        WorldSpaceBounds _bounds;

        public Vector3 BoundsCenter
        {
            get
            {
                if (_bounds != null)
                    return _bounds.Center;
                return Vector3.zero;
            }
        }

        public override Vector3 Origin
        {
            get
            {
                if (_bounds != null)
                    return _bounds.Center;
                return base.Origin;
            }
        }

        /// <summary>
        /// Calculate the target position of the camera based on the preset values.
        /// </summary>
        /// <returns></returns>
        protected override Vector3 CalculateTargetPosition()
        {
            Vector3 offset = new Vector3(
                Settings.PositionOffsetX,
                Settings.PositionOffsetY,
                Settings.PositionOffsetZ
            );

            Vector3 adjustedPosition = Origin + offset;
            if (_bounds != null)
                adjustedPosition = EnforceBounds(adjustedPosition);

            if (Mathf.Abs(Settings.OrbitAngle) > 0)
            {
                // Calculate the orbit position based on the radius and current offset (angle in degrees)
                float orbitRadians = (Settings.OrbitAngle + 90) * Mathf.Deg2Rad; // Convert degrees to radians

                // Set the radius to match the z offset
                float orbitRadius = Settings.PositionOffsetZ;

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

        protected override float CalculateTargetFOV()
        {
            if (_bounds == null)
                return Settings.FOV;

            // Get necessary parameters
            float distance = CameraZOffset; // Distance from the camera to the target (absolute value of z offset)
            float aspectRatio = CameraAspect; // Camera's aspect ratio (width divided by height)
            float width = _bounds.Width; // Width of the bounds to fit

            // Calculate the maximum vertical FOV that fits within the bounds width
            float maxVerticalFOVToFitWidth =
                2f * Mathf.Atan(width / (2f * distance * aspectRatio)) * Mathf.Rad2Deg;

            // Clamp the target FOV to not exceed the maximum allowed FOV
            float minFOV = 1f; // Optional: Set a minimum FOV to prevent extreme zoom-in
            float maxFOV = maxVerticalFOVToFitWidth;
            float targetFOV = Mathf.Clamp(Settings.FOV, minFOV, maxFOV);

            //Debug.Log($"Calculated Vertical FOV: {targetFOV} degrees (Max Allowed FOV: {maxFOV} degrees)");

            return targetFOV;
        }

        Vector3 EnforceBounds(Vector3 position)
        {
            float minXBound = _bounds.Left;
            float maxXBound = _bounds.Right;
            float minYBound = _bounds.Bottom;
            float maxYBound = _bounds.Top;

            // << CALCULATE POSITION >> ------------------------------
            Vector3 adjustedPosition = position;

            // ( Check the adjusted position against the X bounds )
            if (adjustedPosition.x < minXBound)
                adjustedPosition.x = minXBound + HalfWidth; // Add half width to align left edge
            else if (adjustedPosition.x > maxXBound)
                adjustedPosition.x = maxXBound - HalfWidth; // Subtract half width to align right edge

            // ( Check the adjusted position against the Y bounds )
            if (adjustedPosition.y < minYBound)
                adjustedPosition.y = minYBound + HalfHeight;
            else if (adjustedPosition.y > maxYBound)
                adjustedPosition.y = maxYBound - HalfHeight;

            // << CALCULATE FRUSTRUM OFFSET >> ------------------------------
            Vector3 frustrumOffset = Vector3.zero;
            Vector3[] frustumCorners = CalculateFrustumCorners(adjustedPosition, CameraRotation);
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                Vector3 corner = frustumCorners[i];

                // ( X Axis Bounds ) ------------------------------------------------------
                // If the corner is outside the bounds, adjust the offset
                // If the offset is larger than the difference between the corner and the bound,
                //   keep the larger offset value
                if (corner.x < minXBound)
                    frustrumOffset.x = Mathf.Max(frustrumOffset.x, minXBound - corner.x);
                else if (corner.x > maxXBound)
                    frustrumOffset.x = Mathf.Min(frustrumOffset.x, maxXBound - corner.x);

                // ( Y Axis Bounds ) ------------------------------------------------------
                if (corner.y < minYBound)
                    frustrumOffset.y = Mathf.Max(frustrumOffset.y, minYBound - corner.y);
                else if (corner.y > maxYBound)
                    frustrumOffset.y = Mathf.Min(frustrumOffset.y, maxYBound - corner.y);
            }
            return adjustedPosition + frustrumOffset;
        }

        /// <summary>
        /// Calculate the frustum corners of the camera based on the given parameters.
        /// </summary>
        Vector3[] CalculateFrustumCorners(Vector3 position, Quaternion rotation)
        {
            Vector3[] frustumCorners = new Vector3[4];

            // Define the corners in local space (relative to the camera's orientation)
            Vector3 topLeft = new Vector3(-HalfWidth, HalfHeight, CameraZOffset);
            Vector3 topRight = new Vector3(HalfWidth, HalfHeight, CameraZOffset);
            Vector3 bottomLeft = new Vector3(-HalfWidth, -HalfHeight, CameraZOffset);
            Vector3 bottomRight = new Vector3(HalfWidth, -HalfHeight, CameraZOffset);

            // Transform the corners to world space
            frustumCorners[0] = position + rotation * topLeft;
            frustumCorners[1] = position + rotation * topRight;
            frustumCorners[2] = position + rotation * bottomLeft;
            frustumCorners[3] = position + rotation * bottomRight;

            return frustumCorners;
        }
    }
}
