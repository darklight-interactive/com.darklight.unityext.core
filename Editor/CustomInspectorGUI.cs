using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.UnityExt.Editor
{
	public static class CustomInspectorGUI
	{

#if UNITY_EDITOR
		private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

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


		/// <summary>
		/// Draws a read-only array or list as a foldout in the Inspector with each element displayed as a label.
		/// </summary>
		/// <param name="property">The SerializedProperty representing the array or list.</param>
		/// <param name="label">The label to display for the foldout.</param>
		/// <param name="elementNameProvider">A function that takes an index and a SerializedProperty element and returns a custom string for the element name.</param>
		public static void DrawReadOnlyListFoldout(SerializedProperty property,
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
					EditorGUILayout.LabelField(elementName, ConvertElementToString(element));
				}
				EditorGUI.indentLevel--;
			}
		}

		public static void DrawSerializableClass(Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.LabelField(position, label.text, "Class Object");
			position.y += EditorGUIUtility.singleLineHeight;

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

				Rect propertyPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.LabelField(propertyPosition, childProp.displayName, ConvertElementToString(childProp));
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			}
			EditorGUI.indentLevel--;
		}

		/// <summary>
		/// Converts a SerializedProperty element to a string representation based on its type.
		/// </summary>
		/// <param name="element">The SerializedProperty element.</param>
		/// <returns>A string representation of the element.</returns>
		public static string ConvertElementToString(SerializedProperty element)
		{
			switch (element.propertyType)
			{
				case SerializedPropertyType.Integer:
					return element.intValue.ToString();
				case SerializedPropertyType.Boolean:
					return element.boolValue.ToString();
				case SerializedPropertyType.Float:
					return element.floatValue.ToString("0.00000");
				case SerializedPropertyType.String:
					return element.stringValue;
				case SerializedPropertyType.Enum:
					return element.enumDisplayNames[element.enumValueIndex];
				case SerializedPropertyType.ObjectReference:
					return element.objectReferenceValue != null ? element.objectReferenceValue.name : "None";
				case SerializedPropertyType.Vector2:
					return element.vector2Value.ToString();
				case SerializedPropertyType.Vector3:
					return element.vector3Value.ToString();
				case SerializedPropertyType.Vector2Int:
					return element.vector2IntValue.ToString();
				case SerializedPropertyType.Vector3Int:
					return element.vector3IntValue.ToString();
				case SerializedPropertyType.Quaternion:
					return element.quaternionValue.eulerAngles.ToString();
				case SerializedPropertyType.Color:
					return element.colorValue.ToString();
				case SerializedPropertyType.Bounds:
					return element.boundsValue.ToString();
				case SerializedPropertyType.Rect:
					return element.rectValue.ToString();
				default:
					return "[Unsupported Type]";
			}
		}

		public static string GetObjectReferenceString(SerializedProperty prop)
		{
			if (prop.objectReferenceValue == null)
				return "None";

			if (prop.objectReferenceValue is MonoBehaviour monoBehaviour)
				return $"MonoBehaviour: {monoBehaviour.name}";
			if (prop.objectReferenceValue is SceneAsset sceneAsset)
				return $"Scene: {sceneAsset.name}";
			if (prop.objectReferenceValue is ScriptableObject scriptableObject)
				return $"ScriptableObject: {scriptableObject.name}";

			return prop.objectReferenceValue.ToString();
		}

		#region -- << GUI ELEMENTS >> ------------------------------------ >>
		public static void DrawHorizontalLine(Color color, int thickness = 1, int padding = 10)
		{
			Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
			rect.height = thickness;
			rect.y += padding / 2;
			EditorGUI.DrawRect(rect, color);
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
