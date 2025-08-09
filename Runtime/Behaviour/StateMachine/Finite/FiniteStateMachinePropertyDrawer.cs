/*
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Darklight.Behaviour;

namespace Darklight.Behaviour.Editor
{
    /// <summary>
    /// Custom property drawer for FiniteStateMachine that provides a clean editor interface
    /// showing current state, previous state, and state management controls.
    /// </summary>
    [CustomPropertyDrawer(typeof(FiniteStateMachine<>), true)]
    public class FiniteStateMachinePropertyDrawer : PropertyDrawer
    {
        private readonly float LINE_HEIGHT = EditorGUIUtility.singleLineHeight;
        private const float SPACING = 2f;
        private const float BUTTON_HEIGHT = 20f;
        private const float STATE_BOX_HEIGHT = 40f;
        private const float PADDING = 4f;

        // Colors for state display
        private static readonly Color CurrentStateColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
        private static readonly Color PreviousStateColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        private static readonly Color BorderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color BackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return LINE_HEIGHT;

            // Calculate height based on expanded state
            float height = LINE_HEIGHT; // Label
            height += SPACING;
            height += STATE_BOX_HEIGHT; // Current state box
            height += SPACING;
            height += STATE_BOX_HEIGHT; // Previous state box
            height += SPACING;
            height += BUTTON_HEIGHT; // State transition buttons
            height += SPACING;
            height += LINE_HEIGHT; // State count info
            height += PADDING * 2; // Bottom padding

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout label
            Rect foldoutRect = new Rect(position.x, position.y, position.width, LINE_HEIGHT);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float currentY = position.y + LINE_HEIGHT + SPACING;

                // Draw current state
                DrawStateBox(
                    new Rect(position.x, currentY, position.width, STATE_BOX_HEIGHT),
                    "Current State",
                    GetCurrentStateName(property),
                    CurrentStateColor
                );

                currentY += STATE_BOX_HEIGHT + SPACING;

                // Draw previous state
                DrawStateBox(
                    new Rect(position.x, currentY, position.width, STATE_BOX_HEIGHT),
                    "Previous State",
                    GetPreviousStateName(property),
                    PreviousStateColor
                );

                currentY += STATE_BOX_HEIGHT + SPACING;

                // Draw state transition buttons
                DrawStateTransitionButtons(
                    new Rect(position.x, currentY, position.width, BUTTON_HEIGHT),
                    property
                );

                currentY += BUTTON_HEIGHT + SPACING;

                // Draw state count info
                DrawStateInfo(
                    new Rect(position.x, currentY, position.width, LINE_HEIGHT),
                    property
                );

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws a state information box with background and border
        /// </summary>
        /// <param name="rect">The rectangle to draw in</param>
        /// <param name="label">The label for the state</param>
        /// <param name="stateName">The name of the state</param>
        /// <param name="backgroundColor">The background color for the box</param>
        private void DrawStateBox(Rect rect, string label, string stateName, Color backgroundColor)
        {
            // Draw background
            EditorGUI.DrawRect(rect, BackgroundColor);

            // Draw border
            Rect borderRect = new Rect(rect.x, rect.y, rect.width, 1f);
            EditorGUI.DrawRect(borderRect, BorderColor);
            borderRect.y = rect.y + rect.height - 1f;
            EditorGUI.DrawRect(borderRect, BorderColor);
            borderRect = new Rect(rect.x, rect.y, 1f, rect.height);
            EditorGUI.DrawRect(borderRect, BorderColor);
            borderRect.x = rect.x + rect.width - 1f;
            EditorGUI.DrawRect(borderRect, BorderColor);

            // Draw state background
            Rect stateRect = new Rect(
                rect.x + PADDING,
                rect.y + PADDING,
                rect.width - PADDING * 2,
                rect.height - PADDING * 2
            );
            EditorGUI.DrawRect(stateRect, backgroundColor);

            // Draw label and state name
            Rect labelRect = new Rect(
                rect.x + PADDING * 2,
                rect.y + PADDING * 2,
                rect.width - PADDING * 4,
                LINE_HEIGHT
            );
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

            Rect stateNameRect = new Rect(
                rect.x + PADDING * 2,
                rect.y + PADDING + LINE_HEIGHT,
                rect.width - PADDING * 4,
                LINE_HEIGHT
            );
            EditorGUI.LabelField(stateNameRect, stateName, EditorStyles.miniLabel);
        }

        /// <summary>
        /// Draws state transition buttons for testing state changes
        /// </summary>
        /// <param name="rect">The rectangle to draw in</param>
        /// <param name="property">The serialized property</param>
        private void DrawStateTransitionButtons(Rect rect, SerializedProperty property)
        {
            // Get the target object to access runtime methods
            object targetObject = GetTargetObject(property);
            if (targetObject == null)
                return;

            // Get all possible states
            var stateEnums = GetAllStateEnums(targetObject);
            if (stateEnums == null || stateEnums.Length == 0)
                return;

            // Calculate button width
            float buttonWidth =
                (rect.width - (stateEnums.Length - 1) * SPACING) / stateEnums.Length;

            for (int i = 0; i < stateEnums.Length; i++)
            {
                Rect buttonRect = new Rect(
                    rect.x + i * (buttonWidth + SPACING),
                    rect.y,
                    buttonWidth,
                    rect.height
                );

                string stateName = stateEnums[i].ToString();
                bool isCurrentState = IsCurrentState(targetObject, stateEnums[i]);

                // Disable button if it's the current state
                EditorGUI.BeginDisabledGroup(isCurrentState);

                if (GUI.Button(buttonRect, stateName, EditorStyles.miniButton))
                {
                    TransitionToState(targetObject, stateEnums[i]);
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary>
        /// Draws state machine information
        /// </summary>
        /// <param name="rect">The rectangle to draw in</param>
        /// <param name="property">The serialized property</param>
        private void DrawStateInfo(Rect rect, SerializedProperty property)
        {
            object targetObject = GetTargetObject(property);
            if (targetObject == null)
                return;

            var stateEnums = GetAllStateEnums(targetObject);
            string info = $"Total States: {stateEnums?.Length ?? 0}";

            EditorGUI.LabelField(rect, info, EditorStyles.miniLabel);
        }

        /// <summary>
        /// Gets the current state name from the state machine
        /// </summary>
        /// <param name="property">The serialized property</param>
        /// <returns>The current state name or "None" if not available</returns>
        private string GetCurrentStateName(SerializedProperty property)
        {
            object targetObject = GetTargetObject(property);
            if (targetObject == null)
                return "None";

            try
            {
                var currentStateProperty = targetObject.GetType().GetProperty("CurrentState");
                if (currentStateProperty != null)
                {
                    var currentState = currentStateProperty.GetValue(targetObject);
                    return currentState?.ToString() ?? "None";
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get current state: {e.Message}");
            }

            return "None";
        }

        /// <summary>
        /// Gets the previous state name from the state machine
        /// </summary>
        /// <param name="property">The serialized property</param>
        /// <returns>The previous state name or "None" if not available</returns>
        private string GetPreviousStateName(SerializedProperty property)
        {
            object targetObject = GetTargetObject(property);
            if (targetObject == null)
                return "None";

            try
            {
                var previousStateProperty = targetObject
                    .GetType()
                    .GetProperty("PreviousFiniteState");
                if (previousStateProperty != null)
                {
                    var previousState = previousStateProperty.GetValue(targetObject);
                    if (previousState != null)
                    {
                        var stateTypeProperty = previousState.GetType().GetProperty("StateType");
                        if (stateTypeProperty != null)
                        {
                            var stateType = stateTypeProperty.GetValue(previousState);
                            return stateType?.ToString() ?? "None";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get previous state: {e.Message}");
            }

            return "None";
        }

        /// <summary>
        /// Gets the target object from the serialized property
        /// </summary>
        /// <param name="property">The serialized property</param>
        /// <returns>The target object or null if not available</returns>
        private object GetTargetObject(SerializedProperty property)
        {
            try
            {
                return property.serializedObject.targetObject;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get target object: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all possible state enums from the state machine
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <returns>Array of state enums or null if not available</returns>
        private Enum[] GetAllStateEnums(object targetObject)
        {
            try
            {
                var type = targetObject.GetType();
                var genericArguments = type.GetGenericArguments();
                if (genericArguments.Length > 0)
                {
                    var enumType = genericArguments[0];
                    if (enumType.IsEnum)
                    {
                        return Enum.GetValues(enumType).Cast<Enum>().ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get state enums: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Checks if the given state is the current state
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <param name="state">The state to check</param>
        /// <returns>True if the state is current</returns>
        private bool IsCurrentState(object targetObject, Enum state)
        {
            try
            {
                var currentStateProperty = targetObject.GetType().GetProperty("CurrentState");
                if (currentStateProperty != null)
                {
                    var currentState = currentStateProperty.GetValue(targetObject);
                    return Equals(currentState, state);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to check current state: {e.Message}");
            }

            return false;
        }

        /// <summary>
        /// Transitions the state machine to the specified state
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <param name="newState">The state to transition to</param>
        private void TransitionToState(object targetObject, Enum newState)
        {
            try
            {
                var goToStateMethod = targetObject.GetType().GetMethod("GoToState");
                if (goToStateMethod != null)
                {
                    goToStateMethod.Invoke(targetObject, new object[] { newState, true });

                    // Mark the scene as dirty to save changes
                    if (targetObject is UnityEngine.Object unityObject)
                    {
                        EditorUtility.SetDirty(unityObject);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to transition to state {newState}: {e.Message}");
            }
        }
    }
}
#endif
*/
