using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.Utility
{
    /// <summary>
    /// A class that assigns a priority to a tag
    /// </summary>
    [System.Serializable]
    public class PriorityTag
    {
        [SerializeField, Tag]
        string _tag;

        [SerializeField, Range(0, 1)]
        float _priority;

        public string Tag => _tag;
        public float Priority => _priority;

        /// <summary>
        /// A class that contains a list of priority tags and methods to compare them
        /// </summary>
        [System.Serializable]
        public class Comparator
        {
            [SerializeField]
            List<PriorityTag> _priorityTags = new();

            public List<PriorityTag> PriorityTags => _priorityTags;

            /// <summary>
            /// Get the highest priority tag from the list of colliders
            /// </summary>
            /// <param name="colliders">The list of colliders to check</param>
            /// <param name="highestPriorityTag">The highest priority tag found</param>
            public void GetHighestPriorityTag(
                List<Collider> colliders,
                out string highestPriorityTag
            )
            {
                highestPriorityTag = string.Empty;
                float highestPriority = -1;

                // Iterate through each priority tag and check if any colliders have that tag
                foreach (var priorityTag in PriorityTags)
                {
                    // If the priority tag is empty, skip it
                    if (string.IsNullOrEmpty(priorityTag.Tag))
                        continue;

                    bool hasTag = colliders.Any(c => c.CompareTag(priorityTag.Tag));
                    // If the collider has the tag and the priority is higher than the current highest priority, update the highest priority and tag
                    if (hasTag && priorityTag.Priority > highestPriority)
                    {
                        highestPriority = priorityTag.Priority;
                        highestPriorityTag = priorityTag.Tag;
                    }
                }
            }

            /// <summary>
            /// Get the colliders with the highest priority tag from the list of colliders
            /// </summary>
            /// <param name="colliders">The list of colliders to check</param>
            /// <param name="collidersWithHighestPriority">The list of colliders with the highest priority tag</param>
            public void GetCollidersWithHighestPriority(
                List<Collider> colliders,
                out List<Collider> collidersWithHighestPriority
            )
            {
                collidersWithHighestPriority = new();
                GetHighestPriorityTag(colliders, out string highestPriorityTag);

                // If the highest priority tag is not empty, filter the colliders to only include those with the highest priority tag
                if (!string.IsNullOrEmpty(highestPriorityTag))
                {
                    collidersWithHighestPriority = colliders
                        .Where(c => c.CompareTag(highestPriorityTag))
                        .ToList();
                }
            }

            /// <summary>
            /// Filter the colliders to only include those with the highest priority tag
            /// </summary>
            /// <param name="colliders">The list of colliders to filter</param>
            public void FilterCollidersWithHighestPriority(ref List<Collider> colliders)
            {
                GetCollidersWithHighestPriority(
                    colliders,
                    out List<Collider> collidersWithHighestPriority
                );
                colliders = collidersWithHighestPriority;
            }
        }
    }
}
