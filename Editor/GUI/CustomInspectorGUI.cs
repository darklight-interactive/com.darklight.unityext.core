using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor.Utility;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Editor
{
	public static class CustomInspectorGUI
	{
#if UNITY_EDITOR
		static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

		/// <summary>
		/// Draws the default inspector for a SerializedObject, excluding the m_Script property.
		/// </summary>
		/// <param name="obj">
		/// 		The SerializedObject to draw the inspector for.
		/// </param>
		public static bool DrawDefaultInspectorWithoutSelfReference(SerializedObject obj)
		{
			EditorGUI.BeginChangeCheck();
			obj.UpdateIfRequiredOrScript();
			SerializedProperty iterator = obj.GetIterator();
			iterator.NextVisible(true); // skip first property
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren))
			{
				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					EditorGUILayout.PropertyField(iterator, true);
				}

				enterChildren = false;
			}

			obj.ApplyModifiedProperties();
			return EditorGUI.EndChangeCheck();
		}

		/// <summary>
		/// Focuses the Scene view on a specific point in 3D space.
		/// </summary>
		/// <param name="focusPoint">
		/// 		The point in 3D space to focus the Scene view on.
		/// </param>
		public static void FocusSceneView(Vector3 focusPoint)
		{
			if (SceneView.lastActiveSceneView != null)
			{
				// Set the Scene view camera pivot (center point) and size (zoom level)
				SceneView.lastActiveSceneView.pivot = focusPoint;

				// Repaint the scene view to immediately reflect changes
				SceneView.lastActiveSceneView.Repaint();
			}
		}

		#region < PUBLIC_STATIC_METHODS > [[ Draw Serialized Fields ]] ================================================================ 

		/// <summary>
		/// Draws a show-only array or list as a foldout in the Inspector with each element displayed as a label.
		/// </summary>
		/// <param name="property">The SerializedProperty representing the array or list.</param>
		/// <param name="label">The label to display for the foldout.</param>
		/// <param name="elementNameProvider">A function that takes an index and a SerializedProperty element and returns a custom string for the element name.</param>
		public static void DrawShowOnlyListFoldout(SerializedProperty property,
			string label, Func<int, SerializedProperty, string> elementNameProvider = null)
		{
			if (property == null || (!property.isArray && property.propertyType != SerializedPropertyType.Generic))
			{
				EditorGUILayout.HelpBox("Property is not an array or list", MessageType.Warning);
				return;
			}

			// Generate a unique key for foldout state
			string key = property.propertyPath;
			if (!foldoutStates.ContainsKey(key))
			{
				foldoutStates[key] = false;
			}

			// Draw the foldout
			foldoutStates[key] = EditorGUILayout.Foldout(foldoutStates[key], $"{label}", true);

			// If foldout is expanded, draw each element as a label
			if (foldoutStates[key])
			{
				EditorGUI.indentLevel++;
				for (int i = 0; i < property.arraySize; i++)
				{
					SerializedProperty element = property.GetArrayElementAtIndex(i);
					// Use the custom name provider if available, otherwise default to "Element {i}"
					string elementName = elementNameProvider != null ? elementNameProvider(i, element) : $"Element {i}";
					EditorGUILayout.LabelField(elementName, SerializedPropertyUtility.ConvertPropertyToString(element));
				}
				EditorGUI.indentLevel--;
			}
		}

		public static void DrawShowOnlySerializableClass(Rect position, SerializedProperty prop, GUIContent label)
		{
			// Draw the foldout arrow and label
			prop.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), prop.isExpanded, label);
			position.y += EditorGUIUtility.singleLineHeight;

			// If the foldout is expanded, display child properties
			if (prop.isExpanded)
			{
				EditorGUI.indentLevel++;

				SerializedProperty childProp = prop.Copy();
				bool enterChildren = true;

				while (childProp.NextVisible(enterChildren))
				{
					enterChildren = false;

					if (childProp.depth == 0)
					{
						break; // Exit when reaching next sibling property
					}

					// Draw each child property as a label
					Rect propertyPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.LabelField(propertyPosition, childProp.displayName, SerializedPropertyUtility.ConvertPropertyToString(childProp));
					position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				}

				EditorGUI.indentLevel--;
			}
		}

		/// <summary>
		/// Draws all fields in the given SerializedProperty.
		/// </summary>
		/// <param name="property">The SerializedProperty representing the object to draw.</param>
		/// <param name="header">An optional header label to display before the list of fields.</param>
		public static void DrawAllSerializedFields(SerializedProperty property)
		{
			DrawHeader(property.displayName);
			IterateSerializedProperties(property, (currentProperty) =>
			{
				EditorGUILayout.PropertyField(currentProperty, true);
			});
		}

		/// <summary>
		/// Draws all fields in the given SerializedProperty in a disabled (read-only) state.
		/// </summary>
		/// <param name="property">The SerializedProperty representing the object to draw.</param>
		/// <param name="header">An optional header label to display before the list of fields.</param>
		public static void DrawAllSerializedFieldsAsDisabled(SerializedProperty property)
		{
			DrawHeader(property.displayName);
			EditorGUI.BeginDisabledGroup(true);
			IterateSerializedProperties(property, (currentProperty) =>
			{
				EditorGUILayout.PropertyField(currentProperty, true);
			});
			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		/// Draws all fields in the given SerializedProperty as labels, displaying only the field names and their values.
		/// </summary>
		/// <param name="property">The SerializedProperty representing the object to draw.</param>
		/// <param name="header">An optional header label to display before the list of fields.</param>
		public static void DrawAllSerializedFieldsAsShowOnly(SerializedProperty property)
		{
			DrawHeader(property.displayName);
			IterateSerializedProperties(property, (currentProperty) =>
			{
				EditorGUILayout.LabelField(currentProperty.displayName, SerializedPropertyUtility.ConvertPropertyToString(currentProperty));
			});
		}

		/// <summary>
		/// Iterates through all visible fields in the given SerializedProperty, invoking a specified action on each field.
		/// </summary>
		/// <param name="property">The SerializedProperty containing the fields to iterate through.</param>
		/// <param name="action">The action to perform on each field, typically for drawing.</param>
		static void IterateSerializedProperties(SerializedProperty property, System.Action<SerializedProperty> action)
		{
			SerializedProperty currentProperty = property.Copy();
			SerializedProperty endProperty = property.GetEndProperty();

			// Iterate through all properties
			while (currentProperty.NextVisible(true) && !SerializedProperty.EqualContents(currentProperty, endProperty))
			{
				action(currentProperty);
			}
		}

		#endregion

		#region -- << GUI ELEMENTS >> ------------------------------------ >>
		public static void DrawHorizontalLine(Color color, int thickness = 1, int padding = 10)
		{
			Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
			rect.height = thickness;
			rect.y += padding / 2;
			EditorGUI.DrawRect(rect, color);
		}

		/// <summary>
		/// Draws a header label if one is provided.
		/// </summary>
		/// <param name="header">The header label to draw, or null if no header should be displayed.</param>
		private static void DrawHeader(string header)
		{
			if (!string.IsNullOrEmpty(header))
			{
				EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
				EditorGUILayout.Space(2);
			}
		}

		public static void CreateIntegerControl(string title, int currentValue, int minValue, int maxValue, System.Action<int> setValue)
		{
			GUIStyle controlBackgroundStyle = new GUIStyle();
			controlBackgroundStyle.normal.background = MakeTex(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
			controlBackgroundStyle.alignment = TextAnchor.MiddleCenter;
			controlBackgroundStyle.margin = new RectOffset(20, 20, 0, 0);

			EditorGUILayout.BeginVertical(controlBackgroundStyle);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField(title);
			GUILayout.FlexibleSpace();

			// +/- Buttons
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
			{
				setValue(Mathf.Max(minValue, currentValue - 1));
			}
			EditorGUILayout.LabelField($"{currentValue}", CustomGUIStyles.CenteredStyle, GUILayout.MaxWidth(50));
			if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
			{
				setValue(Mathf.Min(maxValue, currentValue + 1));
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			GUI.backgroundColor = Color.white;
		}

		/// <summary>
		/// Creates a foldout in the Unity Editor and executes the given Action if the foldout is expanded.
		/// </summary>
		/// <param name="foldoutLabel">Label for the foldout.</param>
		/// <param name="isFoldoutExpanded">Reference to the variable tracking the foldout's expanded state.</param>
		/// <param name="innerAction">Action to execute when the foldout is expanded. This action contains the UI elements to be drawn inside the foldout.</param>
		public static void CreateFoldout(ref bool isFoldoutExpanded, string foldoutLabel, Action innerAction)
		{
			// Draw the foldout
			isFoldoutExpanded = EditorGUILayout.Foldout(isFoldoutExpanded, foldoutLabel, true, EditorStyles.foldoutHeader);

			// If the foldout is expanded, execute the action
			if (isFoldoutExpanded && innerAction != null)
			{
				EditorGUI.indentLevel++; // Indent the contents of the foldout for better readability
				innerAction.Invoke(); // Execute the provided Action
				EditorGUI.indentLevel--; // Reset indentation
			}
		}

		public static void CreateTwoColumnLabel(string label, string value)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(label);
			EditorGUILayout.LabelField(value);
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Creates a label for an enum property with a dropdown to select the enum value.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="enumProperty"></param>
		/// <param name="label"></param>
		public static void DrawEnumProperty<TEnum>(ref TEnum enumProperty, string label) where TEnum : System.Enum
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label); // Adjust the width as needed

			GUILayout.FlexibleSpace();

			enumProperty = (TEnum)EditorGUILayout.EnumPopup(enumProperty);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Creates a label for an enum property with a dropdown to select the enum value.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="enumProperty"></param>
		/// <param name="label"></param>
		public static void DrawEnumValue<TEnum>(TEnum enumProperty, string label) where TEnum : System.Enum
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label); // Adjust the width as needed

			GUILayout.FlexibleSpace();

			enumProperty = (TEnum)EditorGUILayout.EnumPopup(enumProperty);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		#endregion

		public static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix[i] = col;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}

		public static bool IsObjectOrChildSelected(GameObject obj)
		{
			// Check if the direct object is selected
			if (Selection.activeGameObject == obj)
			{
				return true;
			}

			// Check if any of the selected objects is a child of the inspected object
			foreach (GameObject selectedObject in Selection.gameObjects)
			{
				if (selectedObject.transform.IsChildOf(obj.transform))
				{
					return true;
				}
			}

			return false;
		}
#endif

	}
}
