using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverAim
	 * */
	public class IKSolverAimInspector: IKSolverInspector {

		#region Public methods
		
		/// <summary>
		/// Draws the custom inspector for IKSolverAim
		/// </summary>
		public static void AddInspector(SerializedProperty prop, bool editHierarchy) {
			IKSolverHeuristicInspector.AddTarget(prop);
			if (!prop.FindPropertyRelative("XY").boolValue) EditorGUILayout.PropertyField(prop.FindPropertyRelative("poleTarget"), new GUIContent("Pole Target", "If assigned, will automatically set polePosition to the position of this Transform."));

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("transform"), new GUIContent("Aim Transform", "The transform that you want to be aimed at the target. Needs to be a lineal descendant of the bone hierarchy. For example, if you wish to aim a gun, it should be the gun, one of it's children or the hand bone."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("axis"), new GUIContent("Axis", "The local axis of the Transform that you want to be aimed at IKPosition."));
			if (!prop.FindPropertyRelative("XY").boolValue) EditorGUILayout.PropertyField(prop.FindPropertyRelative("poleAxis"), new GUIContent("Pole Axis", "Keeps that axis of the Aim Transform directed at the polePosition."));

			EditorGUILayout.Space();
			IKSolverHeuristicInspector.AddIKPositionWeight(prop);

			if (!prop.FindPropertyRelative("XY").boolValue) EditorGUILayout.PropertyField(prop.FindPropertyRelative("poleWeight"), new GUIContent("Pole Weight", "The weight of the Pole."));

			IKSolverHeuristicInspector.AddProps(prop);

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("clampWeight"), new GUIContent("Clamp Weight", "Clamping rotation of the solver. 0 is free rotation, 1 is completely clamped to transform axis."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("clampSmoothing"), new GUIContent("Clamp Smoothing", "Number of sine smoothing iterations applied on clamping to make the clamping point smoother."));

			IKSolverHeuristicInspector.AddBones(prop, editHierarchy, true);
		}
		
		/// <summary>
		/// Draws the scene view helpers for IKSolverAim
		/// </summary>
		public static void AddScene(IKSolverAim solver, Color color, bool modifiable) {
			// Protect from null reference errors
			if (solver.transform == null) return;
			if (Application.isPlaying && !solver.initiated) return;

			if (!Application.isPlaying) {
				string message = string.Empty;
				if (!solver.IsValid(ref message)) return;
			}
			
			Handles.color = color;
			GUI.color = color;
			
			// Display the bones
			for (int i = 0; i < solver.bones.Length; i++) {
				IKSolver.Bone bone = solver.bones[i];

				if (i < solver.bones.Length - 1) Handles.DrawLine(bone.transform.position, solver.bones[i + 1].transform.position);
				Inspector.SphereCap(0, solver.bones[i].transform.position, Quaternion.identity, GetHandleSize(solver.bones[i].transform.position));
			}
			
			if (solver.axis != Vector3.zero) Inspector.ConeCap(0, solver.transform.position, Quaternion.LookRotation(solver.transform.rotation * solver.axis), GetHandleSize(solver.transform.position) * 2f);
			
			// Selecting joint and manipulating IKPosition
			if (Application.isPlaying && solver.IKPositionWeight > 0) {
				if (modifiable) {
					Inspector.SphereCap(0, solver.IKPosition, Quaternion.identity, GetHandleSize(solver.IKPosition));
						
					// Manipulating position
					solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
				}
				
				// Draw a transparent line from transform to IKPosition
				Handles.color = new Color(color.r, color.g, color.b, color.a * solver.IKPositionWeight);
				Handles.DrawLine(solver.bones[solver.bones.Length - 1].transform.position, solver.transform.position);
				Handles.DrawLine(solver.transform.position, solver.IKPosition);
			}

			Handles.color = color;

			// Pole
			if (Application.isPlaying && solver.poleWeight > 0f) {
				if (modifiable) {
					Inspector.SphereCap(0, solver.polePosition, Quaternion.identity, GetHandleSize(solver.IKPosition) * 0.5f);
					
					// Manipulating position
					solver.polePosition = Handles.PositionHandle(solver.polePosition, Quaternion.identity);
				}

				// Draw a transparent line from transform to polePosition
				Handles.color = new Color(color.r, color.g, color.b, color.a * solver.poleWeight);
				Handles.DrawLine(solver.transform.position, solver.polePosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods

	}
}
