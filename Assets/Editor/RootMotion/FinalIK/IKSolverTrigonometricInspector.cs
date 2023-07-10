using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverTrigonometric
	 * */
	public class IKSolverTrigonometricInspector: IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Draws the custom inspector for IKSolverTrigonometric
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences) {
			EditorGUI.indentLevel = 0;

			// Bone references
			if (showReferences) {
				EditorGUILayout.Space();
				AddObjectReference(prop.FindPropertyRelative("bone1.transform"), new GUIContent("Bone 1", "The first bone in the hierarchy (thigh or upper arm)."), editHierarchy, 100);
				AddObjectReference(prop.FindPropertyRelative("bone2.transform"), new GUIContent("Bone 2", "The second bone in the hierarchy (calf or forearm)."), editHierarchy, 100);
				AddObjectReference(prop.FindPropertyRelative("bone3.transform"), new GUIContent("Bone 3", "The last bone in the hierarchy (foot or hand)."), editHierarchy, 100);

				EditorGUILayout.Space();
			}

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("target"), new GUIContent("Target", "The target Transform. Solver IKPosition will be automatically set to the position of the target."));

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Position Weight", "Solver weight for smooth blending."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKRotationWeight"), new GUIContent("Rotation Weight", "Weight of last bone's rotation to IKRotation."));
			
			if (showReferences) {
				EditorGUILayout.PropertyField(prop.FindPropertyRelative("bendNormal"), new GUIContent("Bend Normal", "Bend plane normal."));
			}
		}
		
		public static void AddTestInspector(SerializedObject obj) {
			SerializedProperty solver = obj.FindProperty("solver");
			EditorGUILayout.PropertyField(solver.FindPropertyRelative("bendNormal"));
		}
		
		/*
		 * Draws the scene view helpers for IKSolverTrigonometric
		 * */
		public static void AddScene(IKSolverTrigonometric solver, Color color, bool modifiable) {
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;

			//float length = Vector3.Distance(solver.bone1.transform.position, solver.bone2.transform.position) + Vector3.Distance(solver.bone2.transform.position, solver.bone3.transform.position);
			//float size = length * 0.05f;

			Handles.color = color;
			GUI.color = color;
			
			Vector3 bendPosition = solver.bone2.transform.position;
			Vector3 endPosition = solver.bone3.transform.position;
			
			// Chain lines
			Handles.DrawLine(solver.bone1.transform.position, bendPosition);
			Handles.DrawLine(bendPosition, endPosition);
			
			// Joints
			Inspector.SphereCap(0, solver.bone1.transform.position, Quaternion.identity, GetHandleSize(solver.bone1.transform.position));
			Inspector.SphereCap(0, bendPosition, Quaternion.identity, GetHandleSize(bendPosition));
			Inspector.SphereCap(0, endPosition, Quaternion.identity, GetHandleSize(endPosition));
			
			if (Application.isPlaying && (solver.IKPositionWeight > 0 || solver.IKRotationWeight > 0)) {
				if (modifiable) {
					Inspector.CubeCap(0, solver.IKPosition, solver.IKRotation, GetHandleSize(solver.IKPosition));
						
					// Manipulating position and rotation
					switch(Tools.current) {
					case Tool.Move:
						if (solver.target == null) solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
						break;
					case Tool.Rotate:
						if (solver.target == null) solver.IKRotation = Handles.RotationHandle(solver.IKRotation, solver.IKPosition);
						break;
					}
				}
				
				// Target
				Handles.color = new Color(color.r, color.g, color.b, color.a * Mathf.Max(solver.IKPositionWeight, solver.IKRotationWeight));
				Handles.DrawLine(endPosition, solver.IKPosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
	}
}

