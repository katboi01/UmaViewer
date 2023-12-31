using UnityEngine;
using UnityEditor;
using System.Collections;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverFABRIKRoot
	 * */
	public class IKSolverFABRIKRootInspector : IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Draws the custom inspector for IKSolverFABRIKRoot
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy) {
			AddClampedInt(prop.FindPropertyRelative("iterations"), new GUIContent("Iterations", "Solver iterations."), 0, int.MaxValue);

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("rootPin"), new GUIContent("Root Pin", "Weight of keeping all FABRIK Trees pinned to the root position."));
			
			EditorGUILayout.Space();

			EditorGUI.indentLevel = 0;
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("chains"), new GUIContent("Chains", "FABRIK chains."), true);
			
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverFABRIKRoot
		 * */
		public static void AddScene(IKSolverFABRIKRoot solver, Color color, bool modifiable, ref FABRIKChain selected) {
			// Protect from null reference errors
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying) {
				string message = string.Empty;
				if (!solver.IsValid(ref message)) return;
			}
			Handles.color = color;
			
			// Selecting solvers
			if (Application.isPlaying) {
				SelectChain(solver.chains, ref selected, color);
			}
			
			AddSceneChain(solver.chains, color, selected);
			
			// Root pin
			Handles.color = new Color(Mathf.Lerp(1f, color.r, solver.rootPin), Mathf.Lerp(1f, color.g, solver.rootPin), Mathf.Lerp(1f, color.b, solver.rootPin), Mathf.Lerp(0.5f, 1f, solver.rootPin));
			if (solver.GetRoot() != null) {
				Handles.DrawLine(solver.chains[0].ik.solver.bones[0].transform.position, solver.GetRoot().position);
				Inspector.CubeCap(0, solver.GetRoot().position, Quaternion.identity, GetHandleSize(solver.GetRoot().position));
			}
		}
		
		#endregion Public methods
		
		private static Color col, midColor, endColor;
		
		private static void SelectChain(FABRIKChain[] chain, ref FABRIKChain selected, Color color) {
			foreach (FABRIKChain c in chain) {
				if (c.ik.solver.IKPositionWeight > 0 && selected != c) {
					Handles.color = GetChainColor(c, color);

					if (Inspector.DotButton(c.ik.solver.GetIKPosition(), Quaternion.identity, GetHandleSize(c.ik.solver.GetIKPosition()), GetHandleSize(c.ik.solver.GetIKPosition()))) {
						selected = c;
						return;
					}
				}
			}
		}
		
		private static Color GetChainColor(FABRIKChain chain, Color color) {
			float midWeight = chain.pin;
			midColor = new Color(Mathf.Lerp(1f, color.r, midWeight), Mathf.Lerp(1f, color.g, midWeight), Mathf.Lerp(1f, color.b, midWeight), Mathf.Lerp(0.5f, 1f, midWeight));
			
			float endWeight = chain.pull;
			endColor = new Color(Mathf.Lerp(1f, color.r, endWeight), Mathf.Lerp(0f, color.g, endWeight), Mathf.Lerp(0f, color.b, endWeight), Mathf.Lerp(0.5f, 1f, endWeight));
			
			return chain.children.Length == 0? endColor: midColor;
		}
		
		private static void AddSceneChain(FABRIKChain[] chain, Color color, FABRIKChain selected) {
			foreach (FABRIKChain c in chain) {
				col = GetChainColor(c, color);
				
				IKSolverHeuristicInspector.AddScene(c.ik.solver as IKSolverHeuristic, col, selected == c);
			}
		}
	}
}

