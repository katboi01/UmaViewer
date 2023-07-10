using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverLookAt
	 * */
	public class IKSolverLookAtInspector: IKSolverInspector {

		#region Public methods

		/*
		 * Draws the custom inspector for IKSolverLookAt
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences) {

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("target"), new GUIContent("Target", "The target Transform. Solver IKPosition will be automatically set to the position of the target."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight for smooth blending."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("bodyWeight"), new GUIContent("Body Weight", "Weight of rotating spine to target."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("headWeight"), new GUIContent("Head Weight", "Weight of rotating head to target."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("eyesWeight"), new GUIContent("Eyes Weight", "Weight of rotating eyes to target."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("clampWeight"), new GUIContent("Clamp Weight", "Clamping rotation of spine and head. 0 is free rotation, 1 is completely clamped to forward."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("clampWeightHead"), new GUIContent("Clamp Weight Head", "Clamping rotation of the head. 0 is free rotation, 1 is completely clamped to forward."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("clampWeightEyes"), new GUIContent("Clamp Weight Eyes", "Clamping rotation of the eyes. 0 is free rotation, 1 is completely clamped to forward."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("clampSmoothing"), new GUIContent("Clamp Smoothing", "Number of sine smoothing iterations applied on clamping to make the clamping point smoother."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("spineWeightCurve"), new GUIContent("Spine Weight Curve", "Weight distribution between spine bones (first bone is evaluated at time 0.0, last bone at 1.0)."));

			// References
			if (showReferences) {
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(prop.FindPropertyRelative("head.transform"), new GUIContent("Head", "The head bone."));
				
				EditorGUILayout.Space();
				AddArray(prop.FindPropertyRelative("spine"), new GUIContent("Spine", string.Empty), editHierarchy, false, null, null, DrawArrayElementLabelBone);
				
				EditorGUILayout.Space();
				AddArray(prop.FindPropertyRelative("eyes"), new GUIContent("Eyes", string.Empty), editHierarchy, false, null, null, DrawArrayElementLabelBone);
			}
			
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverLookAt
		 * */
		public static void AddScene(IKSolverLookAt solver, Color color, bool modifiable) {
			// Protect from null reference errors
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;

			// Display the Spine
			if (solver.spine.Length > 0) {
				Handles.color = color;
				GUI.color = color;
				
				for (int i = 0; i < solver.spine.Length; i++) {
					IKSolverLookAt.LookAtBone bone = solver.spine[i];
					
					if (i < solver.spine.Length - 1) Handles.DrawLine(bone.transform.position, solver.spine[i + 1].transform.position);
					Inspector.SphereCap(0, bone.transform.position, Quaternion.identity, GetHandleSize(bone.transform.position));
				}
				
				// Draw a transparent line from last bone to IKPosition
				if (Application.isPlaying) {
					Handles.color = new Color(color.r, color.g, color.b, color.a * solver.IKPositionWeight * solver.bodyWeight);
					Handles.DrawLine(solver.spine[solver.spine.Length - 1].transform.position, solver.IKPosition);
				}
			}
			
			// Display the eyes
			if (solver.eyes.Length > 0) {
				for (int i = 0; i < solver.eyes.Length; i++) {
					DrawLookAtBoneInScene(solver.eyes[i], solver.IKPosition, color, solver.IKPositionWeight * solver.eyesWeight);
				}
			}
			
			// Display the head
			if (solver.head.transform != null) {
				DrawLookAtBoneInScene(solver.head, solver.IKPosition, color, solver.IKPositionWeight * solver.headWeight);
			}
			
			Handles.color = color;
			GUI.color = color;
			
			// Selecting joint and manipulating IKPosition
			if (Application.isPlaying && solver.IKPositionWeight > 0) {
				if (modifiable) {
					Inspector.SphereCap(0, solver.IKPosition, Quaternion.identity, GetHandleSize(solver.IKPosition));
						
					// Manipulating position
					if (solver.target == null) solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
				}
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
		
		private static void DrawArrayElementLabelBone(SerializedProperty bone, bool editHierarchy) {
			AddObjectReference(bone.FindPropertyRelative("transform"), GUIContent.none, editHierarchy, 0, 300);
		}
		
		private static void DrawLookAtBoneInScene(IKSolverLookAt.LookAtBone bone, Vector3 IKPosition, Color color, float lineWeight) {
			Handles.color = color;
			GUI.color = color;
					
			Inspector.SphereCap(0, bone.transform.position, Quaternion.identity, GetHandleSize(bone.transform.position));
			
			// Draw a transparent line from last bone to IKPosition
			if (Application.isPlaying && lineWeight > 0) {
				Handles.color = new Color(color.r, color.g, color.b, color.a * lineWeight);
				Handles.DrawLine(bone.transform.position, IKPosition);
			}
		}
	}
}

