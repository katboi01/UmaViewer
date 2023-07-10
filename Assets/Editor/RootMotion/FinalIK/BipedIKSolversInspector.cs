using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for Biped IK Solvers.
	 * */
	public class BipedIKSolversInspector: IKSolverInspector {
		
		/*
		 * Returns all solvers SeiralizedProperties
		 * */
		public static SerializedProperty[] FindProperties(SerializedProperty prop) {
			SerializedProperty[] props = new SerializedProperty[8] {
				prop.FindPropertyRelative("leftFoot"),
				prop.FindPropertyRelative("rightFoot"),
				prop.FindPropertyRelative("leftHand"),
				prop.FindPropertyRelative("rightHand"),
				prop.FindPropertyRelative("spine"),
				prop.FindPropertyRelative("aim"),
				prop.FindPropertyRelative("lookAt"),
				prop.FindPropertyRelative("pelvis"),
			};
			
			return props;
		}
		
		/*
		 * Draws the custom inspector for BipedIK.Solvers
		 * */
		public static void AddInspector(SerializedProperty prop, SerializedProperty[] props) {
			EditorGUILayout.PropertyField(prop);
			
			if (prop.isExpanded) {
				for (int i = 0; i < props.Length; i++) {
					BeginProperty(props[i]);
					if (props[i].isExpanded) {
						if (i <= 3) IKSolverLimbInspector.AddInspector(props[i], false, false);
						else if (i == 4) IKSolverHeuristicInspector.AddInspector(props[i], false, false);
						else if (i == 5) IKSolverAimInspector.AddInspector(props[i], false);
						else if (i == 6) IKSolverLookAtInspector.AddInspector(props[i], false, false);
						else if (i == 7) ConstraintsInspector.AddInspector(props[i]);
					}
					EndProperty(props[i]);
				}
			}
		}
		
		/*
		 * Draws the scene view helpers for BipedIK.Solvers
		 * */
		public static void AddScene(BipedIKSolvers solvers, ref int selected) {
			// Draw limbs
			for (int i = 0; i < solvers.limbs.Length; i++) {
				IKSolverLimbInspector.AddScene(solvers.limbs[i] as IKSolverLimb, GetSolverColor(i), selected == i);
			}
			
			// Draw spine
			IKSolverHeuristicInspector.AddScene(solvers.spine, GetSolverColor(4), selected == 4);
			
			// Draw look at
			IKSolverLookAtInspector.AddScene(solvers.lookAt, GetSolverColor(5), selected == 5);
			
			// Draw aim
			IKSolverAimInspector.AddScene(solvers.aim, GetSolverColor(6), selected == 6);
			
			// Draw constraints
			ConstraintsInspector.AddScene(solvers.pelvis, GetSolverColor(7), selected == 7);

			// Selecting solvers
			if (Application.isPlaying) {
				for (int i = 0; i < solvers.ikSolvers.Length; i++) {
					Handles.color = GetSolverColor(i);
					if (solvers.ikSolvers[i].GetIKPositionWeight() > 0 && selected != i && solvers.ikSolvers[i].initiated) {
						if (Inspector.DotButton(solvers.ikSolvers[i].GetIKPosition(), Quaternion.identity, GetHandleSize(solvers.ikSolvers[i].GetIKPosition()), GetHandleSize(solvers.ikSolvers[i].GetIKPosition()))) selected = i;
					}
				}
				
				if ((solvers.pelvis.positionWeight > 0 || solvers.pelvis.rotationWeight > 0) && selected != solvers.ikSolvers.Length) {
					Handles.color = GetSolverColor(7);
					if (Inspector.DotButton(solvers.pelvis.position, Quaternion.identity, GetHandleSize(solvers.pelvis.position),  GetHandleSize(solvers.pelvis.position))) selected = solvers.ikSolvers.Length;
				}
			}
		}
		
		/*
		 * Gets the color of the solver at index.
		 * */
		private static Color GetSolverColor(int index) {
			if (index == 0 || index == 2) return new Color(0f, 0.8f, 1f, 1f); // Left limb
			if (index == 1 || index == 3) return new Color(0.3f, 1f, 0.3f, 1f); // Right limb
			if (index == 4) return new Color(1f, 0.5f, 0.5f, 1f); // Spine
			if (index == 5) return new Color(0.2f, 0.5f, 1f, 1f); // Look At
			if (index == 6) return new Color(1f, 0f, 0.5f, 1f); // Aim
			if (index == 7) return new Color(0.9f, 0.9f, 0.9f, 1f); // Pelvis
			return Color.white;
		}
		
		/*
		 * Begin property box
		 * */
		private static void BeginProperty(SerializedProperty prop) {
			EditorGUI.indentLevel = 1;
			EditorGUILayout.BeginVertical("Box");
			
			EditorGUILayout.PropertyField(prop);
		}
		
		/*
		 * End Property box
		 * */
		private static void EndProperty(SerializedProperty prop) {
			EditorGUILayout.EndVertical();
			if (prop.isExpanded) EditorGUILayout.Space();
			EditorGUI.indentLevel = 1;
		}
	}
}
