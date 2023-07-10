using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKEffector
	 * */
	public class IKEffectorInspector: IKSolverInspector {
		
		#region Public methods
		
		public static void DrawArrayElementEffector(SerializedProperty effector, bool editHierarchy) {
			if (!editHierarchy) return;
			
			if (effector.FindPropertyRelative("bones").arraySize > 1) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(indent);
				AddClampedFloat(effector.FindPropertyRelative("falloff"), new GUIContent("Distance Falloff", string.Empty), 0f, Mathf.Infinity);
				GUILayout.EndHorizontal();
			}
			
			AddArray(effector.FindPropertyRelative("bones"), new GUIContent("Bones", string.Empty), editHierarchy, false, null, OnAddToArrayBone, DrawArrayElementLabelBone, false);
			
			if (effector.isExpanded) EditorGUILayout.Space();
		}
		
		public static void OnAddToArrayEffector(SerializedProperty effector) {
			effector.FindPropertyRelative("positionWeight").floatValue = 0f;
			effector.FindPropertyRelative("rotationWeight").floatValue = 0f;
			effector.FindPropertyRelative("falloff").floatValue = 0.5f;
			effector.FindPropertyRelative("position").vector3Value = Vector3.zero;
			effector.FindPropertyRelative("positionOffset").vector3Value = Vector3.zero;
		}
		
		public static void DrawArrayElementLabelEffector(SerializedProperty effector, bool editHierarchy) {
			GUILayout.Space(Inspector.indent);
			if (editHierarchy) {
				EditorGUILayout.PropertyField(effector, new GUIContent(GetArrayName(effector.FindPropertyRelative("bones"), "Effector"), string.Empty), false, GUILayout.Width(100));
			} else {
				EditorGUILayout.LabelField(new GUIContent(GetArrayName(effector.FindPropertyRelative("bones"), "Effector"), string.Empty), GUILayout.Width(100));
			}
			
			GUILayout.Space(10);
			
			GUILayout.Label("Position", GUILayout.Width(50));
			effector.FindPropertyRelative("positionWeight").floatValue = GUILayout.HorizontalSlider(effector.FindPropertyRelative("positionWeight").floatValue, 0f, 1f, GUILayout.Width(50));
			
			GUILayout.Space(5);
				
			GUILayout.Label("Rotation", GUILayout.Width(50));
			effector.FindPropertyRelative("rotationWeight").floatValue = GUILayout.HorizontalSlider(effector.FindPropertyRelative("rotationWeight").floatValue, 0f, 1f, GUILayout.Width(50));
			
			if (!editHierarchy && effector.FindPropertyRelative("bones").arraySize > 1) {
				EditorGUILayout.LabelField(new GUIContent("Falloff", string.Empty), GUILayout.Width(50));
				EditorGUILayout.PropertyField(effector.FindPropertyRelative("falloff"), GUIContent.none);
				effector.FindPropertyRelative("falloff").floatValue = Mathf.Clamp(effector.FindPropertyRelative("falloff").floatValue, 0f, Mathf.Infinity);
			}
			
		}
		
		public static void AddScene(IKEffector e, Color color, bool modifiable, float size) {
			if (!modifiable) return;
			
			// Draw effectors
			bool rotate = e.isEndEffector;
			float weight = rotate? Mathf.Max(e.positionWeight, e.rotationWeight): e.positionWeight;
			
			if (e.bone != null && weight > 0) {
					
				//if (Application.isPlaying) {
					Handles.color = new Color(color.r, color.g, color.b, weight);

					Handles.DrawLine(e.position, e.bone.position);
					Inspector.SphereCap(0, e.bone.position, Quaternion.identity, size * 0.5f);

					// Manipulating position and rotation
					if (e.target == null) {
						switch(Tools.current) {
						case Tool.Move:
							e.position = Handles.PositionHandle(e.position, Quaternion.identity);
							break;
						case Tool.Rotate:
							if (rotate) e.rotation = Handles.RotationHandle(e.rotation, e.position);
							break;
						}
					}
					
					if (rotate) Inspector.CubeCap(0, e.position, e.rotation, size);
					else Inspector.SphereCap(0, e.position, Quaternion.identity, size);
				//}
			}
		}
		
		#endregion Public methods
		
		private static void DrawArrayElementLabelBone(SerializedProperty bone, bool editHierarchy) {
			AddObjectReference(bone, GUIContent.none, editHierarchy, 0, 300);
		}
		
		private static void OnAddToArrayBone(SerializedProperty bone) {
			bone.objectReferenceValue = null;
		}
	}
}
