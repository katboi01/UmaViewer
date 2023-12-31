using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {
	
	/*
	 * Custom inspector and scene view tools for IKSolverArm
	 * */
	public class IKSolverArmInspector: IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Draws the custom inspector for IKSolverTrigonometric
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences) {
			EditorGUI.indentLevel = 0;
			
			// Bone references
			if (showReferences) {
				EditorGUILayout.Space();
				AddObjectReference(prop.FindPropertyRelative("chest.transform"), new GUIContent("Chest", "The last spine bone."), editHierarchy, 100);
				AddObjectReference(prop.FindPropertyRelative("shoulder.transform"), new GUIContent("Shoulder", "The shoulder (clavicle) bone."), editHierarchy, 100);
				AddObjectReference(prop.FindPropertyRelative("upperArm.transform"), new GUIContent("Upper Arm", "The upper arm bone."), editHierarchy, 100);
				AddObjectReference(prop.FindPropertyRelative("forearm.transform"), new GUIContent("Forearm", "The forearm bone."), editHierarchy, 100);
				AddObjectReference(prop.FindPropertyRelative("hand.transform"), new GUIContent("Hand", "The hand bone."), editHierarchy, 100);
				
				EditorGUILayout.Space();
			}
			
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.target"), new GUIContent("Target", "The target Transform. Solver IKPosition will be automatically set to the position of the target."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.bendGoal"), new GUIContent("Bend Goal", "If assigned, the knee will be bent in the direction towards this transform."));

			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Position Weight", "Solver weight for smooth blending."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("IKRotationWeight"), new GUIContent("Rotation Weight", "Weight of last bone's rotation to IKRotation."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("isLeft"), new GUIContent("Is Left", "Check this if this is the left arm, uncheck if right."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.shoulderRotationMode"), new GUIContent("Shoulder Rotation Mode", " Different techniques for shoulder bone rotation."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.shoulderRotationWeight"), new GUIContent("Shoulder Rotation Weight", " The weight of shoulder rotation"));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.bendGoalWeight"), new GUIContent("Bend Goal Weight", "Weight of the bend goal."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.swivelOffset"), new GUIContent("Swivel Offset", "Angular offset of the arm's bending direction."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.wristToPalmAxis"), new GUIContent("Wrist To Palm Axis", "Local axis of the hand bone that points from the wrist towards the palm. Used for defining hand bone orientation."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.palmToThumbAxis"), new GUIContent("Palm To Thumb Axis", "Local axis of the hand bone that points from the palm towards the thumb. Used for defining hand bone orientation."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.armLengthMlp"), new GUIContent("Arm Length Mlp", "Use this to make the arm shorter/longer."));
			EditorGUILayout.PropertyField(prop.FindPropertyRelative("arm.stretchCurve"), new GUIContent("Stretch Curve", "Evaluates stretching of the arm by target distance relative to arm length. Value at time 1 represents stretching amount at the point where distance to the target is equal to arm length. Value at time 2 represents stretching amount at the point where distance to the target is double the arm length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce elbow snapping (start stretching the arm slightly before target distance reaches arm length)."));
		}
		
		/*
		 * Draws the scene view helpers for IKSolverTrigonometric
		 * */
		public static void AddScene(IKSolverArm solver, Color color, bool modifiable) {
			if (Application.isPlaying && !solver.initiated) return;
			if (!Application.isPlaying && !solver.IsValid()) return;
			
			//float length = Vector3.Distance(solver.bone1.transform.position, solver.bone2.transform.position) + Vector3.Distance(solver.bone2.transform.position, solver.bone3.transform.position);
			//float size = length * 0.05f;
			
			Handles.color = color;
			GUI.color = color;
			
			// Chain lines
			Handles.DrawLine(solver.chest.transform.position, solver.shoulder.transform.position);
			Handles.DrawLine(solver.shoulder.transform.position, solver.upperArm.transform.position);
			Handles.DrawLine(solver.upperArm.transform.position, solver.forearm.transform.position);
			Handles.DrawLine(solver.forearm.transform.position, solver.hand.transform.position);

			// Joints
			Inspector.SphereCap(0, solver.chest.transform.position, Quaternion.identity, GetHandleSize(solver.chest.transform.position));
			Inspector.SphereCap(0, solver.shoulder.transform.position, Quaternion.identity, GetHandleSize(solver.shoulder.transform.position));
			Inspector.SphereCap(0, solver.upperArm.transform.position, Quaternion.identity, GetHandleSize(solver.upperArm.transform.position));
			Inspector.SphereCap(0, solver.forearm.transform.position, Quaternion.identity, GetHandleSize(solver.forearm.transform.position));
			Inspector.SphereCap(0, solver.hand.transform.position, Quaternion.identity, GetHandleSize(solver.hand.transform.position));

			if (Application.isPlaying && (solver.IKPositionWeight > 0 || solver.IKRotationWeight > 0)) {
				if (modifiable) {
					Inspector.CubeCap(0, solver.IKPosition, solver.IKRotation, GetHandleSize(solver.IKPosition));
					
					// Manipulating position and rotation
					switch(Tools.current) {
					case Tool.Move:
						if (solver.arm.target == null) solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
						break;
					case Tool.Rotate:
						if (solver.arm.target == null) solver.IKRotation = Handles.RotationHandle(solver.IKRotation, solver.IKPosition);
						break;
					}
				}
				
				// Target
				Handles.color = new Color(color.r, color.g, color.b, color.a * Mathf.Max(solver.IKPositionWeight, solver.IKRotationWeight));
				Handles.DrawLine(solver.hand.transform.position, solver.IKPosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
	}
}

