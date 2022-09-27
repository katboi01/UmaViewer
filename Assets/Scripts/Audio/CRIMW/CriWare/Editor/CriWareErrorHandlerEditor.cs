/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;

namespace CriWare {

[CustomEditor(typeof(CriWareErrorHandler))]
public class CriWareErrorHandlerEditor : UnityEditor.Editor {
	private SerializedProperty m_enableForceCrashOnError;
	private SerializedProperty m_dontDestroyOnLoad;

	private void OnEnable() {
		m_enableForceCrashOnError = serializedObject.FindProperty("enableForceCrashOnError");
		m_dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.PropertyField(m_enableForceCrashOnError, new GUIContent("Force Crash on Error"));
		EditorGUILayout.HelpBox("Force Crash on Error:\nAny CRIWARE error will cause the editor to crash.\nDon't enable this when debugging in the editor.", m_enableForceCrashOnError.boolValue ? MessageType.Warning : MessageType.Info);
		EditorGUILayout.PropertyField(m_dontDestroyOnLoad, new GUIContent("Dont Destroy on Load"));

		serializedObject.ApplyModifiedProperties();
	}

	private int GenIntField(string label_str, string tooltip, int field_value, int min, int max)
	{
		GUIContent content = new GUIContent(label_str, tooltip);
		return Mathf.Min(max, Mathf.Max(min, EditorGUILayout.IntField(content, field_value)));
	}
}

} //namespace CriWare
/* end of file */