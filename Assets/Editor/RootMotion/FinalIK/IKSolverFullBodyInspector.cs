using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverFullBody
	 * */
	public class IKSolverFullBodyInspector : IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Draws the custom inspector for IKSolverFullBody
		 * */
		public static void AddInspector(SerializedProperty prop, bool editWeights) {
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight for smooth blending (ik.solver.IKPositionWeight)."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("iterations"), new GUIContent("Iterations", "Solver iterations per frame."));
		}

		/*
		 * Draws the scene view helpers for IKSolverFullBody
		 * */
		public static void AddScene(UnityEngine.Object target, IKSolverFullBody solver, Color color, bool modifiable, ref int selectedEffector, float size) {
			if (!modifiable) return;
			if (!solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;

			// Effectors
			for (int i = 0; i < solver.effectors.Length; i++) {
				bool rotate = solver.effectors[i].isEndEffector;
				float weight = rotate? Mathf.Max(solver.effectors[i].positionWeight, solver.effectors[i].rotationWeight): solver.effectors[i].positionWeight;
				
				if (weight > 0 && selectedEffector != i) {
					Handles.color = color;

					if (rotate) {
						if (Inspector.DotButton(solver.effectors[i].position, solver.effectors[i].rotation, size * 0.5f, size * 0.5f)) {
							selectedEffector = i;
							return;
						}
					} else {
						if (Inspector.SphereButton(solver.effectors[i].position, solver.effectors[i].rotation, size, size)) {
							selectedEffector = i;
							return;
						}
					}
				}
			}
			
			for (int i = 0; i < solver.effectors.Length; i++) IKEffectorInspector.AddScene(solver.effectors[i], color, modifiable && i == selectedEffector, size);
		
			if (GUI.changed) EditorUtility.SetDirty(target);
		}
		
		#endregion Public methods
		
		public static void AddChain(FBIKChain[] chain, int index, Color color, float size) {
			Handles.color = color;
			
			for (int i = 0; i < chain[index].nodes.Length - 1; i++) {
				Handles.DrawLine(GetNodePosition(chain[index].nodes[i]), GetNodePosition(chain[index].nodes[i + 1]));
				Inspector.SphereCap(0, GetNodePosition(chain[index].nodes[i]), Quaternion.identity, size);
			}
			
			Inspector.SphereCap(0, GetNodePosition(chain[index].nodes[chain[index].nodes.Length - 1]), Quaternion.identity, size);

			for (int i = 0; i < chain[index].children.Length; i++) {
				Handles.DrawLine(GetNodePosition(chain[index].nodes[chain[index].nodes.Length - 1]), GetNodePosition(chain[chain[index].children[i]].nodes[0]));
			}
		}
		
		private static Vector3 GetNodePosition(IKSolver.Node node) {
			if (Application.isPlaying) return node.solverPosition;
			return node.transform.position;
		}
	}
}
