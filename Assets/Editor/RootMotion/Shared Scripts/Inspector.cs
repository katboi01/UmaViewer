using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion {

	/*
	 * Inspector contains helper methods for creating undoable custom inspectors with enforced layout, tooltips and styling
	 * */
	public class Inspector {
		
		#region InspectorGUI tools
		
		public const int indent = 16;
		
		public delegate void DrawArrayElement(SerializedProperty prop, bool editHierarchy);
		public delegate void DrawArrayElementLabel(SerializedProperty prop, bool editHierarchy);
		public delegate void OnAddToArray(SerializedProperty prop);
		
		private static string arrayName;
		private static SerializedProperty property;
		private static SerializedProperty element;
		
		public static string GetArrayName(SerializedProperty array, string emptyName, string propertyName = "") {
			if (array.arraySize < 1) return emptyName;
			property = propertyName == ""? array.GetArrayElementAtIndex(0): array.GetArrayElementAtIndex(0).FindPropertyRelative(propertyName);
			return GetObjectReferenceName(property, emptyName);
		}
		
		public static string GetObjectReferenceName(SerializedProperty prop, string emptyName) {
			if (prop.objectReferenceValue == null) return emptyName;
			return prop.objectReferenceValue.name;
		}
		
		public static void AddArray(SerializedProperty prop, GUIContent guiContent, bool editHierarchy, bool changeOrder, DrawArrayElement drawArrayElement = null, OnAddToArray onAddToArray = null, DrawArrayElementLabel drawArrayElementLabel = null, bool showHeading = true) {
			int resetIndent = EditorGUI.indentLevel;

			// Array heading
			if (showHeading) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(EditorGUI.indentLevel * indent);
				
				if (drawArrayElement == null) {
					GUILayout.Label(guiContent.text + " (" + prop.arraySize.ToString() + ")", GUILayout.Width(150));
				} else {
					EditorGUILayout.PropertyField(prop, new GUIContent(guiContent.text + " (" + prop.arraySize.ToString() + ")", string.Empty), false, GUILayout.Width(150));
				}
				
				GUILayout.EndHorizontal();
			}
			
			int deleteIndex = -1;
			
			if (drawArrayElement == null || !showHeading) prop.isExpanded = true;
			
			// Draw Array elements
			if (prop.isExpanded) {			
				for(int i = 0; i < prop.arraySize; i++) {
					GUILayout.BeginHorizontal(); // Main
					GUILayout.Space(((EditorGUI.indentLevel + 1) * indent));
					GUILayout.BeginVertical();
					
					element = prop.GetArrayElementAtIndex(i);

					// Label
					GUILayout.BeginHorizontal(); 
				
					if (editHierarchy && GUILayout.Button(new GUIContent("-", "Remove"), changeOrder? EditorStyles.miniButtonLeft: EditorStyles.miniButton, GUILayout.Width(20))){
						deleteIndex = i;
					}
					
					if (changeOrder) {
						if (GUILayout.Button(new GUIContent("<", "Move up"), editHierarchy? EditorStyles.miniButtonMid: EditorStyles.miniButtonLeft, GUILayout.Width(20))) {
							int moveTo = i == 0? prop.arraySize - 1: i - 1;
							prop.MoveArrayElement(i, moveTo);
							prop.isExpanded = true;
						}
							
						if (GUILayout.Button(new GUIContent(">", "Move down"), EditorStyles.miniButtonRight, GUILayout.Width(20))) {
							int moveTo = i == prop.arraySize - 1? 0: i + 1;
							prop.MoveArrayElement(i, moveTo);
							prop.isExpanded = true;
						}
					}
					
					// Calling the DrawArrayElementLabel delegate
					if (drawArrayElementLabel != null) {
						drawArrayElementLabel(element, editHierarchy);
					}
					
					GUILayout.EndHorizontal(); // End Label
					
					// Array Element
					GUILayout.BeginVertical();
					if (element.isExpanded && drawArrayElement != null) {
						drawArrayElement(element, editHierarchy);
					}
					GUILayout.EndVertical();
					
					GUILayout.Space(5);
					
					GUILayout.EndVertical(); // End Style
					GUILayout.EndHorizontal(); // End Main
				}
				
				// Deleting array elements
				if (deleteIndex != -1) prop.DeleteArrayElementAtIndex(deleteIndex);
				
				// Adding array elements
				GUILayout.BeginHorizontal();
				GUILayout.Space(((EditorGUI.indentLevel + 1) * indent) + 4);
				GUILayout.BeginVertical();
				
				if (editHierarchy && GUILayout.Button(new GUIContent("+", "Add"), EditorStyles.miniButton, GUILayout.Width(20))) {
					prop.arraySize ++;
					
					if (onAddToArray != null) onAddToArray(prop.GetArrayElementAtIndex(prop.arraySize - 1));
				}
				
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
				
			EditorGUI.indentLevel = resetIndent;
		}
		
		public static void AddContent(SerializedContent content, bool addChildren = false, params GUILayoutOption[] options) {
			EditorGUILayout.PropertyField(content.prop, content.guiContent, addChildren, options);
		}
		
		public static void AddClampedFloat(SerializedContent content, float min = 0f, float max = 1f, params GUILayoutOption[] options) {
			AddClampedFloat(content.prop, content.guiContent, min, max, options);
		}
		
		public static void AddClampedInt(SerializedContent content, int min = int.MinValue, int max = int.MaxValue, params GUILayoutOption[] options) {
			AddClampedInt(content.prop, content.guiContent, min, max, options);
		}
		
		public static void AddClampedFloat(SerializedProperty prop, GUIContent guiContent, float min = 0f, float max = 1f, params GUILayoutOption[] options) {
			EditorGUILayout.PropertyField(prop, guiContent, options);
			prop.floatValue = Mathf.Clamp(prop.floatValue, min, max);
		}
		
		public static void AddClampedInt(SerializedProperty prop, GUIContent guiContent, int min = int.MinValue, int max = int.MaxValue, params GUILayoutOption[] options) {
			EditorGUILayout.PropertyField(prop, guiContent, options);
			prop.intValue = Mathf.Clamp(prop.intValue, min, max);
		}
		
		public static void AddObjectReference(SerializedProperty prop, GUIContent guiContent, bool editHierarchy, int labelWidth, bool alignToEdge = true) {
			if (alignToEdge) GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(guiContent, GUILayout.MinWidth(labelWidth));
			if (editHierarchy) {
				EditorGUILayout.PropertyField(prop, GUIContent.none);
			} else {
				UnityEngine.Object obj = prop.objectReferenceValue;
				EditorGUILayout.LabelField(new GUIContent(obj != null? obj.name: "None"));
			}
			if (alignToEdge) GUILayout.EndHorizontal();
		}
		
		public static void AddObjectReference(SerializedProperty prop, GUIContent guiContent, bool editHierarchy, int labelWidth, int propWidth, bool alignToEdge = true) {
			//if (alignToEdge) GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(guiContent, GUILayout.Width(labelWidth));
			if (editHierarchy) {
				EditorGUILayout.PropertyField(prop, GUIContent.none, GUILayout.Width(propWidth));
			} else {
				UnityEngine.Object obj = prop.objectReferenceValue;
				EditorGUILayout.LabelField(new GUIContent(obj != null? obj.name: "None"), GUILayout.Width(propWidth));
			}
			//if (alignToEdge) GUILayout.EndHorizontal();
		}
		
		#endregion InspectorGUI tools
		
		#region SceneGUI tools
		
		public static bool Button(string name, string toolTip, Object undoObject, params GUILayoutOption[] options) {
			bool button = GUILayout.Button(new GUIContent(name, toolTip), options);
			if (button && !Application.isPlaying) Undo.RecordObject(undoObject, name);
			return button;
		}
		
		public static Object AddObject(Object input, string name, string toolTip, System.Type type, bool allowSceneObjects, Object undoObject, params GUILayoutOption[] options) {
			Object newValue = EditorGUILayout.ObjectField(new GUIContent(name, toolTip), input, type, allowSceneObjects, options);
			
			return RecordObject(input, newValue, name, undoObject);
		}
		
		public static System.Enum AddEnum(System.Enum input, string name, string toolTip, Object undoObject, params GUILayoutOption[] options) {
			System.Enum newValue = EditorGUILayout.EnumPopup(new GUIContent(name, toolTip), input, options);
			
			if (newValue.ToString() != input.ToString() && !Application.isPlaying) Undo.RecordObject(undoObject, name);
			return newValue;
		}
		
		public static void AddHorizontalSlider(ref float input, float leftValue, float rightValue, string name, string toolTip, Object undoObject, int labelWidth, int minWidth) {
			EditorGUILayout.BeginHorizontal();
			
			GUILayout.Label(new GUIContent(name, toolTip), GUILayout.Width(labelWidth));
			
			input = RecordObjectFloat(input, GUILayout.HorizontalSlider(input, leftValue, rightValue, GUILayout.MinWidth(minWidth)), name, undoObject);
			input = Mathf.Clamp(RecordObjectFloat(input, EditorGUILayout.FloatField(string.Empty, input, GUILayout.Width(75)), name, undoObject), leftValue, rightValue);
			
			EditorGUILayout.EndHorizontal();
		}
		
		public static void AddFloat(ref float input, string name, string toolTip, Object undoObject, float min = -Mathf.Infinity, float max = Mathf.Infinity, params GUILayoutOption[] options) {
			input = Mathf.Clamp(RecordObjectFloat(input, EditorGUILayout.FloatField(new GUIContent(name, toolTip), input, options), name, undoObject), min, max);
		}
		
		public static void AddInt(ref int input, string name, string toolTip, Object undoObject, int min = -int.MaxValue, int max = int.MaxValue, params GUILayoutOption[] options) {
			input = Mathf.Clamp((int)RecordObjectFloat((float)input, (float)EditorGUILayout.IntField(new GUIContent(name, toolTip), input, options), name, undoObject), min, max);
		}
		
		public static void AddBool(ref bool input, string name, string toolTip, Object undoObject) {
			bool newValue = EditorGUILayout.Toggle(new GUIContent(name, toolTip), input);
			
			if (newValue != input && !Application.isPlaying) {
				Undo.RecordObject(undoObject, name);
			}
			
			input = newValue;
		}
		
		public static void AddVector3(ref Vector3 input, string name, Object undoObject, params GUILayoutOption[] options) {
			Vector3 newValue = EditorGUILayout.Vector3Field(name, input, options);
			
			if (newValue != input && !Application.isPlaying) {
				Undo.RecordObject(undoObject, name);
			}
			
			input = newValue;
		}
		
		private static float RecordObjectFloat(float input, float newValue, string name, Object undoObject) {
			if (newValue != input && !Application.isPlaying) Undo.RecordObject(undoObject, name);
			return newValue;
		}
		
		private static Object RecordObject(Object input, Object newValue, string name, Object undoObject) {
			if (newValue != input && !Application.isPlaying) Undo.RecordObject(undoObject, name);
			return newValue;
		}
		
		#endregion SceneGUI tools

		#region Platform dependent

		public static void SphereCap(int controlID, Vector3 position, Quaternion rotation, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.SphereHandleCap(controlID, position, rotation, size, EventType.Repaint);
			#else
			Handles.SphereCap(controlID, position, rotation, size);
			#endif
		}

		public static void CubeCap(int controlID, Vector3 position, Quaternion rotation, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.CubeHandleCap(controlID, position, rotation, size, EventType.Repaint);
			#else
			Handles.CubeCap(controlID, position, rotation, size);
			#endif
		}

		public static void ConeCap(int controlID, Vector3 position, Quaternion rotation, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.ConeHandleCap(controlID, position, rotation, size, EventType.Repaint);
			#else
			Handles.ConeCap(controlID, position, rotation, size);
			#endif
		}

		public static void ArrowCap(int controlID, Vector3 position, Quaternion rotation, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.ArrowHandleCap(controlID, position, rotation, size, EventType.Repaint);
			#else
			Handles.ArrowCap(controlID, position, rotation, size);
			#endif
		}

		public static void CircleCap(int controlID, Vector3 position, Quaternion rotation, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.CircleHandleCap(controlID, position, rotation, size, EventType.Repaint);
			#else
			Handles.CircleCap(controlID, position, rotation, size);
			#endif
		}

		public static void DotCap(int controlID, Vector3 position, Quaternion rotation, float size) {
			#if UNITY_5_6_OR_NEWER
			Handles.DotHandleCap(controlID, position, rotation, size, EventType.Repaint);
			#else
			Handles.DotCap(controlID, position, rotation, size);
			#endif
		}

		public static bool DotButton(Vector3 position, Quaternion direction, float size, float pickSize) {
			#if UNITY_5_6_OR_NEWER
			return Handles.Button(position, direction, size, pickSize, Handles.DotHandleCap);
			#else
			return Handles.Button(position, direction, size, pickSize, Handles.DotCap);
			#endif
		}

		public static bool SphereButton(Vector3 position, Quaternion direction, float size, float pickSize) {
			#if UNITY_5_6_OR_NEWER
			return Handles.Button(position, direction, size, pickSize, Handles.SphereHandleCap);
			#else
			return Handles.Button(position, direction, size, pickSize, Handles.SphereCap);
			#endif
		}

		public static float ScaleValueHandleSphere(float value, Vector3 position, Quaternion rotation, float size, float snap) {
			#if UNITY_5_6_OR_NEWER
			return Handles.ScaleValueHandle(value, position, rotation, size, Handles.SphereHandleCap, snap);
			#else
			return Handles.ScaleValueHandle(value, position, rotation, size, Handles.SphereCap, snap);
			#endif
		}

		#endregion Platform dependent
	}
}
