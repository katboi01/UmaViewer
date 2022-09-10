/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CriWare {

[CustomEditor(typeof(CriAtomTransceiver)), CanEditMultipleObjects]
public class CriAtomTransceiverEditor : UnityEditor.Editor {
	/* parameters */
	private SerializedProperty m_regionOnStart;
	private SerializedProperty m_useDedicatedInput;
	private SerializedProperty m_dedicatedInput;
	private SerializedProperty m_outputVolume;
	private SerializedProperty m_directAudioRadius;
	private SerializedProperty m_crossFadeDistance;
	private SerializedProperty m_coneInsideAngle;
	private SerializedProperty m_coneOutsideAngle;
	private SerializedProperty m_coneOutsideVolume;
	private SerializedProperty m_transceiverRadius;
	private SerializedProperty m_interiorDistance;
	private SerializedProperty m_minAttenuation;
	private SerializedProperty m_maxAttenuation;
	private SerializedProperty m_globalAisacName;
	private SerializedProperty m_maxAngleAisacDelta;
	private SerializedProperty m_distanceAisacControlId;
	private SerializedProperty m_listenerAzimuthAisacControlId;
	private SerializedProperty m_listenerElevationAisacControlId;
	private SerializedProperty m_outputAzimuthAisacControlId;
	private SerializedProperty m_outputElevationAisacControlId;
	private MethodInfo m_applyParametersMethod;

	static private readonly GUIContent cUseDedicatedInputLabel = new GUIContent("Use Dedicated Input", "Use another game object as input position");
	static private readonly GUIContent cTranceiverRadiusLabel = new GUIContent("Non-Panning Radius");
	static private readonly GUIContent cConeInsideAngleLabel = new GUIContent("Inside");
	static private readonly GUIContent cConeOutsideAngle = new GUIContent("Outside");
	static private readonly GUIContent cDistanceAisacLabel = new GUIContent("Distance");
	static private readonly GUIContent cListenerAzimuthAisacLabel = new GUIContent("Listener Azimuth Angle");
	static private readonly GUIContent cListenerElevationAisacLabel = new GUIContent("Listener Elevation Angle");
	static private readonly GUIContent cOutputAzimuthAisacLabel = new GUIContent("Output Azimuth Angle");
	static private readonly GUIContent cOutputElevationAisacLabel = new GUIContent("Output Elevation Angle");

	private void OnEnable() {
		m_regionOnStart = serializedObject.FindProperty("regionOnStart");
		m_useDedicatedInput = serializedObject.FindProperty("useDedicatedInput");
		m_dedicatedInput = serializedObject.FindProperty("dedicatedInput");
		m_outputVolume = serializedObject.FindProperty("outputVolume");
		m_directAudioRadius = serializedObject.FindProperty("directAudioRadius");
		m_crossFadeDistance = serializedObject.FindProperty("crossFadeDistance");
		m_coneInsideAngle = serializedObject.FindProperty("coneInsideAngle");
		m_coneOutsideAngle = serializedObject.FindProperty("coneOutsideAngle");
		m_coneOutsideVolume = serializedObject.FindProperty("coneOutsideVolume");
		m_transceiverRadius = serializedObject.FindProperty("transceiverRadius");
		m_interiorDistance = serializedObject.FindProperty("interiorDistance");
		m_minAttenuation = serializedObject.FindProperty("minAttenuation");
		m_maxAttenuation = serializedObject.FindProperty("maxAttenuation");
		m_globalAisacName = serializedObject.FindProperty("globalAisacName");
		m_maxAngleAisacDelta = serializedObject.FindProperty("maxAngleAisacDelta");
		m_distanceAisacControlId = serializedObject.FindProperty("distanceAisacControlId");
		m_listenerAzimuthAisacControlId = serializedObject.FindProperty("listenerAzimuthAisacControlId");
		m_listenerElevationAisacControlId = serializedObject.FindProperty("listenerElevationAisacControlId");
		m_outputAzimuthAisacControlId = serializedObject.FindProperty("outputAzimuthAisacControlId");
		m_outputElevationAisacControlId = serializedObject.FindProperty("outputElevationAisacControlId");

		m_applyParametersMethod = typeof(CriAtomTransceiver).GetMethod("ApplyParameters", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
	}

	public override void OnInspectorGUI() {
		var targetObj = target as CriAtomTransceiver;

		EditorGUILayout.PropertyField(m_regionOnStart);
		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(m_useDedicatedInput, cUseDedicatedInputLabel);
		if (m_useDedicatedInput.boolValue) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_dedicatedInput);
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(m_outputVolume);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Transceiving");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(m_directAudioRadius);
		EditorGUILayout.PropertyField(m_crossFadeDistance);
		EditorGUI.indentLevel--;
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Cone Angles");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(m_coneInsideAngle, cConeInsideAngleLabel);
		EditorGUILayout.PropertyField(m_coneOutsideAngle, cConeOutsideAngle);
		using (var hScope = new EditorGUILayout.HorizontalScope()) {
			EditorGUILayout.PrefixLabel(" ");
			if (GUILayout.Button("Set to Omni-Directional", GUILayout.Width(150))) {
				m_coneInsideAngle.floatValue = 360f;
				m_coneOutsideAngle.floatValue = 360f;
			}
		}
		EditorGUI.indentLevel--;
		EditorGUILayout.PropertyField(m_coneOutsideVolume);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Panning");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(m_transceiverRadius, cTranceiverRadiusLabel);
		EditorGUILayout.PropertyField(m_interiorDistance);
		EditorGUI.indentLevel--;
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Attenuation");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(m_minAttenuation);
		EditorGUILayout.PropertyField(m_maxAttenuation);
		EditorGUI.indentLevel--;
		EditorGUILayout.Space();

		if (targetObj.inspectorAisacSettingFoldout = EditorGUILayout.Foldout(targetObj.inspectorAisacSettingFoldout, "AISAC")) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_globalAisacName);
			EditorGUILayout.PropertyField(m_maxAngleAisacDelta);
			EditorGUILayout.LabelField("AISAC Control IDs");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_distanceAisacControlId, cDistanceAisacLabel);
			EditorGUILayout.PropertyField(m_listenerAzimuthAisacControlId, cListenerAzimuthAisacLabel);
			EditorGUILayout.PropertyField(m_listenerElevationAisacControlId, cListenerElevationAisacLabel);
			EditorGUILayout.PropertyField(m_outputAzimuthAisacControlId, cOutputAzimuthAisacLabel);
			EditorGUILayout.PropertyField(m_outputElevationAisacControlId, cOutputElevationAisacLabel);
			EditorGUI.indentLevel -= 2;
		}

		if (GUI.changed) {
			ValidateParameters();
			serializedObject.ApplyModifiedProperties();
			if (m_applyParametersMethod != null) {
				m_applyParametersMethod.Invoke(targetObj, null);
			}
		}
	}

	private void ValidateParameters() {
		if (m_outputVolume.floatValue < 0) { m_outputVolume.floatValue = 0; }
		if (m_directAudioRadius.floatValue < 0) { m_directAudioRadius.floatValue = 0; }
		if (m_crossFadeDistance.floatValue < 0) { m_crossFadeDistance.floatValue = 0; }
		m_coneInsideAngle.floatValue = Mathf.Clamp(m_coneInsideAngle.floatValue, 0, 360);
		m_coneOutsideAngle.floatValue = Mathf.Clamp(m_coneOutsideAngle.floatValue, 0, 360);
		m_coneInsideAngle.floatValue = Mathf.Min(m_coneInsideAngle.floatValue, m_coneOutsideAngle.floatValue);
		if (m_coneOutsideVolume.floatValue < 0) { m_coneOutsideVolume.floatValue = 0; }
		if (m_transceiverRadius.floatValue < 0) { m_transceiverRadius.floatValue = 0; }
		if (m_interiorDistance.floatValue < 0) { m_interiorDistance.floatValue = 0; }
		if (m_minAttenuation.floatValue < 0) { m_minAttenuation.floatValue = 0; }
		if (m_maxAttenuation.floatValue < 0) { m_maxAttenuation.floatValue = 0; }
		m_minAttenuation.floatValue = Mathf.Min(m_minAttenuation.floatValue, m_maxAttenuation.floatValue);
		if (m_maxAngleAisacDelta.floatValue < 0) { m_maxAngleAisacDelta.floatValue = 0; }
	}

	private void OnSceneGUI() {
		var targetObj = target as CriAtomTransceiver;
		if (targetObj.enabled == false) { return; }

		var transformObj = targetObj.transform;
		var outsideArcStartVector = Quaternion.AngleAxis(-m_coneOutsideAngle.floatValue / 2f, transformObj.up) * transformObj.forward;
		var insideArcStartVector = Quaternion.AngleAxis(-m_coneInsideAngle.floatValue / 2f, transformObj.up) * transformObj.forward;
		float screenSpaceConstant = Mathf.Min(HandleUtility.GetHandleSize(transformObj.position) * 3, targetObj.maxAttenuation);
		var dedicatedInput = m_dedicatedInput.objectReferenceValue as GameObject;

		Handles.color = new Color(1, 1, 1, 0.3f);
		Handles.DrawDottedLine(transformObj.position, transformObj.position + transformObj.forward * screenSpaceConstant, 5f);
		Handles.color = new Color(1, 1, 1, 0.03f);
		if (m_coneInsideAngle.floatValue < 360) {
			Handles.DrawSolidArc(transformObj.position, transformObj.up, outsideArcStartVector, m_coneOutsideAngle.floatValue, screenSpaceConstant);
			Handles.DrawSolidArc(transformObj.position, transformObj.up, insideArcStartVector, m_coneInsideAngle.floatValue, screenSpaceConstant);
		}
		Handles.color = new Color(0.627f, 0.890f, 0.388f);
		Handles.DrawWireArc(transformObj.position, transformObj.up, outsideArcStartVector, m_coneOutsideAngle.floatValue, m_transceiverRadius.floatValue);
		Handles.color = new Color(1f, 0.537f, 0);
		Handles.DrawWireArc(transformObj.position, transformObj.up, outsideArcStartVector, m_coneOutsideAngle.floatValue, m_transceiverRadius.floatValue + m_interiorDistance.floatValue);

		Handles.color = new Color(0.890f, 0.862f, 0.388f, 0.1f);
		if (m_useDedicatedInput.boolValue && dedicatedInput != null) {
			Handles.DrawSolidDisc(dedicatedInput.transform.position, dedicatedInput.transform.up, m_directAudioRadius.floatValue);
		} else {
			Handles.DrawSolidDisc(transformObj.position, transformObj.up, m_directAudioRadius.floatValue);
		}
		Handles.color = new Color(0.290f, 0.556f, 0.827f, 0.1f);
		if (m_useDedicatedInput.boolValue && dedicatedInput != null) {
			Handles.DrawSolidDisc(dedicatedInput.transform.position, dedicatedInput.transform.up, m_directAudioRadius.floatValue + m_crossFadeDistance.floatValue);
		} else {
			Handles.DrawSolidDisc(transformObj.position, transformObj.up, m_directAudioRadius.floatValue + m_crossFadeDistance.floatValue);
		}

		using (var changeScope = new EditorGUI.ChangeCheckScope()) {
			Handles.color = new Color(0.3317462f, 0.6611561f, 0.990566f, 0.7f);
			float minAtt = Handles.RadiusHandle(Quaternion.identity, transformObj.position, targetObj.minAttenuation);
			float maxAtt = Handles.RadiusHandle(Quaternion.identity, transformObj.position, targetObj.maxAttenuation);
			if (changeScope.changed) {
				Undo.RecordObject(target, "Change Transceiver Attenuation");

				minAtt = Mathf.Min(minAtt, maxAtt);
				targetObj.minAttenuation = minAtt;
				targetObj.maxAttenuation = maxAtt;

				EditorUtility.SetDirty(target);

				if (m_applyParametersMethod != null) {
					m_applyParametersMethod.Invoke(targetObj, null);
				}
			}
		}
	}
}

} //namespace CriWare
/* end of file */