using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;

namespace HSVPicker.Editors
{
	[CustomEditor(typeof(BoxSlider), true)]
	[CanEditMultipleObjects]
	public class BoxSliderEditor : SelectableEditor
	{

		SerializedProperty m_HandleRect;
		SerializedProperty m_MinValue;
		SerializedProperty m_MaxValue;
		SerializedProperty m_WholeNumbers;
		SerializedProperty m_Value;
		SerializedProperty m_ValueY;
		SerializedProperty m_OnValueChanged;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			m_HandleRect = serializedObject.FindProperty("m_HandleRect");

			m_MinValue = serializedObject.FindProperty("m_MinValue");
			m_MaxValue = serializedObject.FindProperty("m_MaxValue");
			m_WholeNumbers = serializedObject.FindProperty("m_WholeNumbers");
			m_Value = serializedObject.FindProperty("m_Value");
			m_ValueY = serializedObject.FindProperty("m_ValueY");
			m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(m_HandleRect);
			
			if (m_HandleRect.objectReferenceValue != null)
			{
				EditorGUI.BeginChangeCheck();

				
				EditorGUILayout.PropertyField(m_MinValue);
				EditorGUILayout.PropertyField(m_MaxValue);
				EditorGUILayout.PropertyField(m_WholeNumbers);
				EditorGUILayout.Slider(m_Value, m_MinValue.floatValue, m_MaxValue.floatValue);
				EditorGUILayout.Slider(m_ValueY, m_MinValue.floatValue, m_MaxValue.floatValue);
				
				// Draw the event notification options
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(m_OnValueChanged);
			}
			else
			{
				EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}
