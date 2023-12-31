using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverLimb
	 * */
	public class IKSolverLimbInspector: IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Draws the custom inspector for IKSolverLimb
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences) {
			// Draw the trigonometric IK inspector
			IKSolverTrigonometricInspector.AddInspector(prop, editHierarchy, showReferences);
			
			EditorGUILayout.Space();
			
			if (showReferences && editHierarchy) {
				EditorGUILayout.PropertyField(prop.FindPropertyRelative("goal"), new GUIContent("Avatar IK Goal", "Avatar IK Goal here is only used by the 'Arm' bend modifier."));
			}

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("maintainRotationWeight"), new GUIContent("Maintain Rotation", "Weight of rotating the last bone back to the rotation it had before solving IK."));
			
			// Bend normal modifier.
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("bendModifier"), new GUIContent("Bend Modifier", "Bend normal modifier."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("bendModifierWeight"), new GUIContent("Bend Modifier Weight", "Weight of the bend modifier."));

			if (prop.FindPropertyRelative("bendModifier").enumValueIndex == 4) EditorGUILayout.PropertyField(prop.FindPropertyRelative("bendGoal"), new GUIContent("Bend Goal", "The bend goal Transform (optional, you can also use IKSolverTrigonometric.SetBendGoalPosition(Vector position, float weight)."));

			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverLimb
		 * */
		public static void AddScene(IKSolverLimb solver, Color color, bool modifiable) {
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;

			if (solver.bendGoal != null && solver.bendModifierWeight > 0f) {
				Color c = color;
				c.a = solver.bendModifierWeight;
				Handles.color = c;

				Handles.DrawLine(solver.bone2.transform.position, solver.bendGoal.position);
				Inspector.SphereCap(0, solver.bendGoal.position, Quaternion.identity, GetHandleSize(solver.bendGoal.position) * 0.5f);

				Handles.color = Color.white;
			}

			IKSolverTrigonometricInspector.AddScene(solver as IKSolverTrigonometric, color, modifiable);
		}
		
		#endregion Public methods
	}
}

