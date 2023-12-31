using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.Dynamics {

	public enum JointAxis {
		Primary,
		Secondary,
		Tertiary
	}

	// Tools for editing Joints.
	public class JointTools: Editor {

		public static Quaternion LocalToJointSpace(ConfigurableJoint joint) {
			Vector3 forward = Vector3.Cross (joint.axis, joint.secondaryAxis).normalized;
			Vector3 up = Vector3.Cross(forward, joint.axis).normalized;

			if (forward == up) {
				Debug.LogWarning("Joint " + joint.name + " secondaryAxis is in the exact same direction as it's axis. Please make sure they are not aligned.");
			}

			return Quaternion.LookRotation(forward, up);
		}

		public static Quaternion LocalToJointSpace(CharacterJoint joint) {
			Vector3 forward = Vector3.Cross (joint.axis, joint.swingAxis).normalized;
			Vector3 up = Vector3.Cross(forward, joint.axis).normalized;
			
			if (forward == up) {
				Debug.LogWarning("Joint " + joint.name + " swingAxis is in the exact same direction as it's axis. Please make sure they are not aligned.");
			}
			
			return Quaternion.LookRotation(forward, up);
		}

		public static Quaternion JointToLocalSpace(ConfigurableJoint joint) {
			return Quaternion.Inverse(LocalToJointSpace(joint));
		}

		public static Quaternion JointToLocalSpace(CharacterJoint joint) {
			return Quaternion.Inverse(LocalToJointSpace(joint));
		}

		public static JointAxis GetJointAxis(ConfigurableJoint joint, Vector3 vLocal) {
			return GetJointAxis(joint.axis, joint.secondaryAxis, vLocal);
		}

		public static JointAxis GetJointAxis(CharacterJoint joint, Vector3 vLocal) {
			return GetJointAxis(joint.axis, joint.swingAxis, vLocal);
		}

		public static JointAxis GetJointAxis(Vector3 axis, Vector3 secondaryAxis, Vector3 vLocal) {
			vLocal = vLocal.normalized;
			
			float axisDot = Mathf.Abs(Vector3.Dot(axis, vLocal));
			float secondaryAxisDot = Mathf.Abs(Vector3.Dot(secondaryAxis, vLocal));
			float tertiaryAxisDot = Mathf.Abs(Vector3.Dot(Vector3.Cross(axis, secondaryAxis), vLocal));
			
			if (axisDot > secondaryAxisDot && axisDot > tertiaryAxisDot) return JointAxis.Primary;
			if (secondaryAxisDot > axisDot && secondaryAxisDot > tertiaryAxisDot) return JointAxis.Secondary;
			return JointAxis.Tertiary;
		}

		public static JointAxis GetSymmetricJointAxis(ConfigurableJoint joint, JointAxis jointAxis, ConfigurableJoint symmetricJoint, Transform root) {
			return GetSymmetricJointAxis(joint.transform, joint.axis, joint.secondaryAxis, jointAxis, symmetricJoint.transform, symmetricJoint.axis, symmetricJoint.secondaryAxis, root);
		}

		public static JointAxis GetSymmetricJointAxis(CharacterJoint joint, JointAxis jointAxis, CharacterJoint symmetricJoint, Transform root) {
			return GetSymmetricJointAxis(joint.transform, joint.axis, joint.swingAxis, jointAxis, symmetricJoint.transform, symmetricJoint.axis, symmetricJoint.swingAxis, root);
		}

		public static JointAxis GetSymmetricJointAxis(Transform jointTransform, Vector3 axis, Vector3 secondaryAxis, JointAxis jointAxis, Transform symmetricTransform, Vector3 symmetricAxis, Vector3 symmetricSecondaryAxis, Transform root) {
			Vector3 a = JointAxisToVector3(axis, secondaryAxis, jointAxis);
			Vector3 aWorld = jointTransform.rotation * a;
			
			Vector3 aWorldMirror = SymmetryTools.Mirror(aWorld, root);
			Vector3 aLocalMirror = Quaternion.Inverse(symmetricTransform.rotation) * aWorldMirror;
			
			Vector3 sAVector = AxisTools.GetAxisVectorToDirection(symmetricTransform, aWorldMirror);
			Vector3 sA = Vector3.Project(aLocalMirror, sAVector);
			
			return GetJointAxis(symmetricAxis, symmetricSecondaryAxis, sA);
		}

		public static Vector3 JointAxisToVector3(ConfigurableJoint joint, JointAxis jointAxis) {
			return JointAxisToVector3(joint.axis, joint.secondaryAxis, jointAxis);
		}

		public static Vector3 JointAxisToVector3(CharacterJoint joint, JointAxis jointAxis) {
			return JointAxisToVector3(joint.axis, joint.swingAxis, jointAxis);
		}

		public static Vector3 JointAxisToVector3(Vector3 axis, Vector3 secondaryAxis, JointAxis jointAxis) {
			switch(jointAxis) {
			case JointAxis.Primary: return axis;
			case JointAxis.Secondary: return secondaryAxis;
			default: return Vector3.Cross(axis, secondaryAxis);
			}
		}

		public static void ApplyXDeltaToJointLimit(ref ConfigurableJoint joint, float delta, JointAxis jointAxis, bool low) {
			switch(jointAxis) {
			case JointAxis.Primary:
				if (low) {
					SoftJointLimit lowX = joint.lowAngularXLimit;
					lowX.limit += delta;
					joint.lowAngularXLimit = lowX;
				} else {
					SoftJointLimit highX = joint.highAngularXLimit;
					highX.limit -= delta;
					joint.highAngularXLimit = highX;
				}
				break;
			case JointAxis.Secondary:
				SoftJointLimit y = joint.angularYLimit;
				y.limit += delta * 0.5f;
				joint.angularYLimit = y;
				break;
			case JointAxis.Tertiary:
				SoftJointLimit z = joint.angularZLimit;
				z.limit += delta * 0.5f;
				joint.angularZLimit = z;
				break;
			}
		}

		public static void ApplyXDeltaToJointLimit(ref CharacterJoint joint, float delta, JointAxis jointAxis, bool low) {
			switch(jointAxis) {
			case JointAxis.Primary:
				if (low) {
					SoftJointLimit lowX = joint.lowTwistLimit;
					lowX.limit += delta;
					joint.lowTwistLimit = lowX;
				} else {
					SoftJointLimit highX = joint.highTwistLimit;
					highX.limit -= delta;
					joint.highTwistLimit = highX;
				}
				break;
			case JointAxis.Secondary:
				SoftJointLimit y = joint.swing1Limit;
				y.limit += delta * 0.5f;
				joint.swing1Limit = y;
				break;
			case JointAxis.Tertiary:
				SoftJointLimit z = joint.swing2Limit;
				z.limit += delta * 0.5f;
				joint.swing2Limit = z;
				break;
			}
		}

		public static void ApplyDeltaToJointLimit(ref ConfigurableJoint joint, float delta, JointAxis jointAxis) {
			switch(jointAxis) {
			case JointAxis.Primary:
				SoftJointLimit lowX = joint.lowAngularXLimit;
				lowX.limit += delta;
				joint.lowAngularXLimit = lowX;

				SoftJointLimit highX = joint.highAngularXLimit;
				highX.limit += delta;
				joint.highAngularXLimit = highX;
				break;
			case JointAxis.Secondary:
				SoftJointLimit y = joint.angularYLimit;
				y.limit += delta;
				joint.angularYLimit = y;
				break;
			case JointAxis.Tertiary:
				SoftJointLimit z = joint.angularZLimit;
				z.limit += delta;
				joint.angularZLimit = z;
				break;
			}
		}

		public static void ApplyDeltaToJointLimit(ref CharacterJoint joint, float delta, JointAxis jointAxis) {
			switch(jointAxis) {
			case JointAxis.Primary:
				SoftJointLimit lowX = joint.lowTwistLimit;
				lowX.limit += delta;
				joint.lowTwistLimit = lowX;
				
				SoftJointLimit highX = joint.highTwistLimit;
				highX.limit += delta;
				joint.highTwistLimit = highX;
				break;
			case JointAxis.Secondary:
				SoftJointLimit y = joint.swing1Limit;
				y.limit += delta;
				joint.swing1Limit = y;
				break;
			case JointAxis.Tertiary:
				SoftJointLimit z = joint.swing2Limit;
				z.limit += delta;
				joint.swing2Limit = z;
				break;
			}
		}

		public static Vector3 GetLowXAxisWorld(Joint joint) {
			return joint.transform.rotation * joint.axis;
		}

		public static Vector3 GetHighXAxisWorld(Joint joint) {
			return joint.transform.rotation * -joint.axis;
		}

		public static void SwitchXY(ref ConfigurableJoint joint) {
			Undo.RecordObject(joint, "Switch Yellow/Green Joint Axis");

			Vector3 axis = joint.axis;
			joint.axis = joint.secondaryAxis;
			joint.secondaryAxis = axis;
		}

		public static void SwitchXZ(ref ConfigurableJoint joint) {
			Undo.RecordObject(joint, "Switch Yellow/Blue Joint Axis");
			
			Vector3 cross = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
			joint.axis = cross;
		}

		public static void SwitchYZ(ref ConfigurableJoint joint) {
			Undo.RecordObject(joint, "Switch Green/Blue Joint Axis");
			
			Vector3 cross = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
			joint.secondaryAxis = cross;
		}

		public static void SwitchXY(ref CharacterJoint joint) {
			Undo.RecordObject(joint, "Switch Yellow/Green Joint Axis");
			
			Vector3 axis = joint.axis;
			joint.axis = joint.swingAxis;
			joint.swingAxis = axis;
		}
		
		public static void SwitchXZ(ref CharacterJoint joint) {
			Undo.RecordObject(joint, "Switch Yellow/Blue Joint Axis");
			
			Vector3 cross = Vector3.Cross(joint.axis, joint.swingAxis).normalized;
			joint.axis = cross;
		}
		
		public static void SwitchYZ(ref CharacterJoint joint) {
			Undo.RecordObject(joint, "Switch Green/Blue Joint Axis");
			
			Vector3 cross = Vector3.Cross(joint.axis, joint.swingAxis).normalized;
			joint.swingAxis = cross;
		}

		public static void InvertAxis(ref Joint joint) {
			Undo.RecordObject(joint, "Invert Axis");

			joint.axis = -joint.axis;
		}

		public static void InvertSecondaryAxis(ref Joint joint) {
			if (joint is ConfigurableJoint) {
				var j = joint as ConfigurableJoint;
				Undo.RecordObject(j, "Invert Secondary Axis");
				j.secondaryAxis = -j.secondaryAxis;
			}

			if (joint is CharacterJoint) {
				var j = joint as CharacterJoint;
				Undo.RecordObject(j, "Invert Swing Axis");
				j.swingAxis = -j.swingAxis;
			}
		}

	}
}
