using System.Collections.Generic;
using UnityEngine;

namespace Darklight.Behaviour
{
    public interface IInteractor : ISensor
    {
        /// <summary>
        /// The overlapped interactables of the interactor
        /// </summary>
        IEnumerable<Interactable> OverlapInteractables { get; }

        /// <summary>
        /// The target interactable of the interactor. Typically the closest interactable.
        /// </summary>
        Interactable TargetInteractable { get; }

        /// <summary>
        /// Try to add an interactable to the interactor
        /// </summary>
        /// <param name="interactable"></param>
        void TryAddInteractable(Interactable interactable);

        /// <summary>
        /// Remove an interactable from the interactor
        /// </summary>
        void RemoveInteractable(Interactable interactable);

        /// <summary>
        /// Find all interactables that overlap with the interactor
        /// </summary>
        /// <returns>An enumerable of interactables that overlap with the interactor</returns>
        IEnumerable<Interactable> GetOverlapInteractables();

        /// <summary>
        /// Get the closest interactable to the interactor
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Interactable GetClosestReadyInteractable(Vector3 position);

        /// <summary>
        /// Try to assign a target interactable to the interactor
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns>True if the target was assigned, false otherwise</returns>
        bool TryAssignTarget(Interactable interactable);

        /// <summary>
        /// Clear the target interactable of the interactor
        /// </summary>
        void ClearTarget();

        /// <summary>
        /// Interact with a specific interactable
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="force"></param>
        /// <returns>True if the interactable was interacted with, false otherwise</returns>
        bool InteractWith(Interactable interactable, bool force = false);

        /// <summary>
        /// Interact with the target interactable
        /// </summary>
        /// <returns>True if the target interactable was interacted with, false otherwise</returns>
        bool InteractWithTarget();

        /// <summary>
        /// Refresh the nearby interactables of the interactor
        /// </summary>
        void RefreshOverlapInteractables();
    }
}
