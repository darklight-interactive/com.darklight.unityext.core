using System;
using System.Collections.Generic;
using System.Linq;
using Darklight.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Darklight.Behaviour
{
    /// <summary>
    /// Not actually a vector field (https://en.wikipedia.org/wiki/Vector_field), but a sloppy interpretation of one.
    /// Basically, given a 2D grid of positions, a present position, and a given direction, where will you go based on the direction?
    /// </summary>
    /// <typeparam name="TElement">The selectables we'll be looking over.</typeparam>
    [Serializable]
    public class SelectableVectorField<TElement>
        where TElement : VisualElement, new()
    {
        const string SELECTED_CLASS = "selected";

        HashSet<TElement> _selectables = new HashSet<TElement>();
        TElement _previousSelection;
        TElement _currentSelection;

        [SerializeField]
        bool _debug = false;

        [SerializeField, ShowOnly]
        List<string> _selectableNames = new List<string>();

        [SerializeField, ShowOnly]
        string _previousSelectionName;

        [SerializeField, ShowOnly]
        string _currentSelectionName;

        public HashSet<TElement> Selectables
        {
            get { return _selectables; }
            set { _selectables = value; }
        }
        public TElement PreviousSelection
        {
            get => _previousSelection;
            private set
            {
                _previousSelection = value;
                _previousSelectionName = value?.name ?? "";
            }
        }
        public TElement CurrentSelection
        {
            get => _currentSelection;
            private set
            {
                _currentSelection = value;
                if (value != null)
                    _currentSelectionName = value.name;
            }
        }

        void Refresh()
        {
            _selectableNames.Clear();
            foreach (TElement selectable in _selectables)
            {
                _selectableNames.Add(selectable.name);
            }
        }

        void PrintSelectables()
        {
            string s = "Selectables: ";
            foreach (TElement selectable in _selectables)
            {
                s +=
                    $"{selectable}, {selectable.worldBound.center} {selectable == CurrentSelection}";
                s += "\n";
            }
            Debug.Log(s);
        }

        #region ---- Raycast Estimation -------- >>
        /// <summary>
        /// Like a raycast, except this also accounts for rays that are CLOSE enough for the MenuVector field to qualify as "good enough".
        /// So that means that this raycast returns some kind of Selectable, provided that Selectable is:
        /// Hit by the ray(the ray at some point is within its rectangle).
        /// This will return the selectable that has the closest distance to where the raycast "hits",
        /// provided the distance is within the "threshold".
        /// We prioritize elements that are closer to the current element, since this is meant to be used for navigation.
        /// </summary>
        /// <param name="from">Position in space.</param>
        /// <param name="direction">Any direction relative to the position.</param>
        /// <param name="threshhold">The maximum distance a given Selectable from `from` can have to be selected.</param>
        /// <returns>Index of closest selectable from raycast.</returns>
        TElement RaycastEstimate(Vector2 from, Vector2 direction, int threshhold = 9999)
        {
            float closestDir = -1;
            TElement selected = null;

            // We parameterize our direction vector.
            // Direction = (dx/dt, dy/dt).
            // So direction.y = dy/dt, y = direction.y * t + C_1.
            // And direction.x = dx/dt, x = direction.x * t + C_2.
            // C_1 and C_2 are just from.y and from.x respectively.
            foreach (TElement selectable in _selectables)
            {
                if (selectable != CurrentSelection)
                {
                    Vector2 selectable_center = selectable.worldBound.center;
                    //Debug.Log($"Selectable: {selectable.name} Center: {selectable_center}");

                    // The direction ray gives us an equation we can solve for. But first,
                    // Get a dot product to quickly see if we're headed in the right direction:
                    Vector2 to = selectable_center - from;
                    to.Normalize();
                    float dot = Vector3.Dot(to, direction);
                    if (dot > 0.5f)
                    {
                        // Now we construct a function that, given some value t, gives us the distance to the center of the rect.
                        // If you look at the problem on graph paper, you'll see that (For STRAIGHT LINES) the closest distance to any rect is also the closest point to that rect's center.
                        // Then we want to find the global minima of that function.
                        // So, we construct the distance function between any given point on our line, and the center of our rectangle:
                        // sqrt( (direction.y * t + C_1 - center.y, 2)^2 + (direction.x * t + C_2 - center.x)^2 )
                        // Then we optimize to find the global minima.
                        // Taking the derivative with respect to t, we get
                        // (direction.y * (direction.y * t + C_1 - center.y) + direction.x * (direction.x * t + C_2 - center.x))/the whole sqrt from before,I'm not gonna write it all out.
                        // We want the smallest value, so we need to find where the values of dt are zero. Setting the LHS to zero, we get:
                        // 0 = direction.y * (direction.y * t + C_1 - center.y) + direction.x * (direction.x * t + C_2 - center.x).
                        // And there's only one value of t, which is our global minima:
                        // t = (direction.y * (center.y - C_1) + direction.x * (center.x - C_2))/(direction.x^2 + direction.y^2).

                        // Visualization: https://www.desmos.com/calculator/kl2oypib1f
                        float t =
                            (
                                direction.y * (selectable_center.y - from.y)
                                + direction.x * (selectable_center.x - from.x)
                            ) / (Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.y, 2));
                        // And so we get the x and y from our t-value:
                        Vector2 dirClose = new Vector2(
                            direction.x * t + from.x,
                            direction.y * t + from.y
                        );
                        // And now we just find the distance, and see if it's closer than the other distances we've calculated.
                        float dist = Vector2.Distance(dirClose, selectable_center);

                        // Check if the point is within the rect bounds.
                        Vector2 selectFullPos = selectable.worldBound.position;
                        bool withinRectBounds =
                            dirClose.x >= selectFullPos.x
                            && dirClose.x <= selectFullPos.x + selectable.worldBound.width
                            && dirClose.y >= selectFullPos.y
                            && dirClose.y <= selectFullPos.y + selectable.worldBound.height;

                        // If we're within the rect bounds, && its the closest , select it.
                        float actualElementDist = Vector2.Distance(
                            from,
                            selectable.worldBound.center
                        );
                        if (
                            (dist <= threshhold || withinRectBounds)
                            && (actualElementDist < closestDir || closestDir == -1)
                        )
                        {
                            closestDir = actualElementDist;
                            selected = selectable;
                            //Debug.Log($"Selected: {selected.name} Input : {direction}  {actualElementDist} {closestDir}");
                        }
                    }
                }
            }
            return selected;
        }
        #endregion

        public void Add(TElement selectable)
        {
            _selectables.Add(selectable);
            if (CurrentSelection == null)
                CurrentSelection = selectable;
            Refresh();
        }

        public void Remove(TElement selectable)
        {
            _selectables.Remove(selectable);
            if (CurrentSelection == selectable)
            {
                CurrentSelection = _selectables.First();
            }
            Refresh();
        }

        public void Load(IEnumerable<TElement> selectables)
        {
            _selectables = new HashSet<TElement>(selectables);
            Reset();
            Select(_selectables.First());
            Refresh();
        }

        public void Clear()
        {
            _selectables.Clear();
            Reset();
        }

        public void AddRange(IEnumerable<TElement> selectables)
        {
            foreach (TElement selectable in selectables)
            {
                _selectables.Add(selectable);
            }
        }

        public void RemoveRange(IEnumerable<TElement> selectables)
        {
            foreach (TElement selectable in selectables)
            {
                _selectables.Remove(selectable);
            }
        }

        public void Reset()
        {
            foreach (TElement selectable in _selectables)
            {
                selectable.RemoveFromClassList(SELECTED_CLASS);
            }
            CurrentSelection = null;
            PreviousSelection = null;
            Refresh();
        }

        public bool Select(TElement selectable)
        {
            if (
                selectable == null
                || selectable == CurrentSelection
                || !_selectables.Contains(selectable)
            )
                return false;
            PreviousSelection = CurrentSelection;
            CurrentSelection = selectable;
            selectable.AddToClassList(SELECTED_CLASS);
            _previousSelection?.RemoveFromClassList(SELECTED_CLASS);
            _currentSelection?.AddToClassList(SELECTED_CLASS);
            return true;
        }

        /// <summary>
        /// Select an element given our currently selected element.
        /// </summary>
        /// <param name="dir">The direction from which to select.</param>
        /// <returns>If we found a successful element, a new pick. Otherwise it's just the previous one.</returns>
        public TElement SelectElementInDirection(Vector2 dir)
        {
            if (_selectables == null || _selectables.Count == 0)
                return null;

            //PrintSelectables();

            // If we have a direction, we can try to find the next element.
            if (dir != Vector2.zero)
            {
                //Debug.Log($"SelectableVectorField :: GetElementInDirection :: {dir}");
                // Potentially select a new element.
                TElement pick = RaycastEstimate(CurrentSelection.worldBound.center, dir);
                if (pick != null && pick.name != CurrentSelection.name)
                {
                    Select(pick);
                    if (_debug)
                    {
                        Debug.Log(
                            $"SelectableVectorField :: GetElementInDirection :: {dir} :: New selection: {pick.name}"
                        );
                    }
                }
                else
                {
                    // If we didn't find a new element, just return the old one.
                    pick = CurrentSelection;
                    if (_debug)
                    {
                        Debug.Log(
                            $"SelectableVectorField :: GetElementInDirection :: {dir} :: No new selection."
                        );
                    }
                }
                return pick;
            }
            else
            {
                if (_debug)
                {
                    Debug.LogError(
                        ">> SelectableVectorField :: GetElementInDirection :: Direction is zero."
                    );
                }
            }
            return CurrentSelection;
        }
    }
}
