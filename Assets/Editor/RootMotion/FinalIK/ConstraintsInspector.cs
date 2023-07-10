using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for Constraints
	 * */
	public class ConstraintsInspector: IKSolverInspector {
		
		#region Public methods

		/*
		 * Draws the custom inspector for Constraints
		 * */
		public static void AddInspector(SerializedProperty prop) {
			if (!prop.isExpanded) return;
			
			// Main properties
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("target"), new GUIContent("Target", "Target transform for the pelvis (optional). If assigned, will overwrite pelvis.position in each update."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("positionOffset"), new GUIContent("Pos Offset", "Pelvis offset from animation. If there is no animation playing and Fix Transforms is unchecked, it will make the character fly away."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("positionWeight"), new GUIContent("Pos Weight", "The weight of lerping the pelvis to bipedIK.solvers.pelvis.position."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("rotationOffset"), new GUIContent("Rot Offset", "Pelvis rotation offset from animation. If there is no animation playing and Fix Transforms is unchecked, it will make the character spin."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("rotationWeight"), new GUIContent("Rot Weight", "The weiight of slerping the pelvis to bipedIK.solver.pelvis.rotation."));

			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for Constraints
		 * */
		public static void AddScene(Constraints constraints, Color color, bool modifiable) {
			if (!constraints.IsValid()) return;
			
			Handles.color = color;
			GUI.color = color;
			
			// Transform
			Inspector.SphereCap(0, constraints.transform.position, Quaternion.identity, GetHandleSize(constraints.transform.position));

			// Target
			Handles.color = new Color(color.r, color.g, color.b, color.a * constraints.positionWeight);
			Handles.DrawLine(constraints.transform.position, constraints.position);
			Handles.color = color;
			
			if (Application.isPlaying && modifiable && (constraints.positionWeight > 0 || constraints.rotationWeight > 0)) {
				Inspector.CubeCap(0, constraints.position, Quaternion.Euler(constraints.rotation), GetHandleSize(constraints.transform.position));
					
				// Manipulating position and rotation
				switch(Tools.current) {
				case Tool.Move:
					constraints.position = Handles.PositionHandle(constraints.position, Quaternion.Euler(constraints.rotation));
					break;
				case Tool.Rotate:
					constraints.rotation = Handles.RotationHandle(Quaternion.Euler(constraints.rotation), constraints.position).eulerAngles;
					break;
				}
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}

		#endregion Public methods
	}
}
