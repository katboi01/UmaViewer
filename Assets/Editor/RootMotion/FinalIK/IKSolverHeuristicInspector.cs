using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IK solvers extending IKSolverHeuristic
	 * */
	public class IKSolverHeuristicInspector: IKSolverInspector {

		#region Public methods
		
		/*
		 * Draws the custom inspector for IKSolverHeuristic
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool editWeights) {
			AddTarget(prop);
			AddIKPositionWeight(prop);
			AddProps(prop);
			AddBones(prop, editHierarchy, editWeights);
		}

		public static void AddTarget(SerializedProperty prop) {
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("target"), new GUIContent("Target", "The target Transform. Solver IKPosition will be automatically set to the position of the target."));
		}

		public static void AddIKPositionWeight(SerializedProperty prop) {
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight for smooth blending."));
		}

		public static void AddProps(SerializedProperty prop) {
			AddClampedFloat(prop.FindPropertyRelative("tolerance"), new GUIContent("Tolerance", "Minimum offset from last reached position. Will stop solving if offset is less than tolerance. If tolerance is zero, will iterate until maxIterations."));
			AddClampedInt(prop.FindPropertyRelative("maxIterations"), new GUIContent("Max Iterations", "Max solver iterations per frame."), 0, int.MaxValue);
		}

		public static void AddBones(SerializedProperty prop, bool editHierarchy, bool editWeights) {
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("useRotationLimits"), new GUIContent("Use Rotation Limits", "If true, rotation limits (if existing) will be applied on each iteration."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("XY"), new GUIContent("2D", "If true, will solve only in the XY plane."));

			EditorGUILayout.Space();
			weights = editWeights;
			if (editHierarchy || editWeights) {
				AddArray(prop.FindPropertyRelative("bones"), new GUIContent("Bones", string.Empty), editHierarchy, false, null, OnAddToArrayBone, DrawArrayElementLabelBone);
			}
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverHeuristic
		 * */
		public static void AddScene(IKSolverHeuristic solver, Color color, bool modifiable) {
			// Protect from null reference errors
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;

			Handles.color = color;
			GUI.color = color;
			
			// Display the bones
			for (int i = 0; i < solver.bones.Length; i++) {
				IKSolver.Bone bone = solver.bones[i];

				if (i < solver.bones.Length - 1) Handles.DrawLine(bone.transform.position, solver.bones[i + 1].transform.position);
				Inspector.SphereCap(0, solver.bones[i].transform.position, Quaternion.identity, GetHandleSize(solver.bones[i].transform.position));
			}
			
			// Selecting joint and manipulating IKPosition
			if (Application.isPlaying && solver.IKPositionWeight > 0) {
				if (modifiable) {
					Inspector.CubeCap(0, solver.IKPosition, solver.GetRoot().rotation, GetHandleSize(solver.IKPosition));
						
					// Manipulating position
					if (solver.target == null) solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
				}
				
				// Draw a transparent line from last bone to IKPosition
				Handles.color = new Color(color.r, color.g, color.b, color.a * solver.IKPositionWeight);
				Handles.DrawLine(solver.bones[solver.bones.Length - 1].transform.position, solver.IKPosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
		
		private static bool weights;
		
		private static void DrawArrayElementLabelBone(SerializedProperty bone, bool editHierarchy) {
			AddObjectReference(bone.FindPropertyRelative("transform"), GUIContent.none, editHierarchy, 0, weights? 100: 200);
			if (weights) AddWeightSlider(bone.FindPropertyRelative("weight"));
		}
		
		private static void OnAddToArrayBone(SerializedProperty bone) {
			bone.FindPropertyRelative("weight").floatValue = 1f;
		}
		
		private static void AddWeightSlider(SerializedProperty prop) {
			GUILayout.Label("Weight", GUILayout.Width(45));
			EditorGUILayout.PropertyField(prop, GUIContent.none);
		}
	}
}
