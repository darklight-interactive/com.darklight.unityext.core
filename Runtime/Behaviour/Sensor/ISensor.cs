using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Behaviour
{
    public enum SensorShape
    {
        BOX,
        SPHERE
    }

    public interface ISensor
    {
        /// <summary>
        /// The settings for the sensor
        /// </summary>
        SensorSettings Settings { get; }

        /// <summary>
        /// The position of the sensor in world space.
        /// This is the position of the sensor's transform plus the offset position value.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The colliders that the sensor is currently detecting
        /// </summary>
        IEnumerable<Collider> Colliders { get; }

        /// <summary>
        /// Whether the sensor is disabled
        /// </summary>
        bool IsDisabled { get; }

        /// <summary>
        /// Whether the sensor is colliding with any colliders
        /// </summary>
        bool IsColliding { get; }

        /// <summary>
        /// Execute the sensor. This will update the colliders of the sensor and any other relevant data.
        /// </summary>
        void Execute();

        /// <summary>
        /// Disable the sensor for a duration
        /// </summary>
        /// <param name="duration">The duration to disable the sensor for</param>
        void TimedDisable(float duration);

        /// <summary>
        /// Get the current colliders that the sensor is detecting
        /// </summary>
        /// <param name="colliders">The colliders that the sensor is detecting</param>
        IEnumerable<Collider> GetCurrentColliders();

        /// <summary>
        /// Get the closest collider to the sensor
        /// </summary>
        /// <param name="closestCollider">The closest collider to the sensor within the sensor's radius</param>
        void TryGetClosestCollider(out Collider closestCollider);
    }
}
