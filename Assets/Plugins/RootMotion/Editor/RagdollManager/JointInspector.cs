using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace RootMotion.Dynamics {

	public class JointInspector : Editor {

		#region Public methods

		private static bool isDragging;

		public static float DrawJointLimit(Joint joint, string label, Vector3 axis, float limit, Color color, float openValue, bool drawHandles) {
			if (Event.current.type == EventType.MouseDown) isDragging = false;
			if (Event.current.type == EventType.MouseUp) isDragging = false;

			float radius = HandleUtility.GetHandleSize(joint.transform.position);
			Vector3 center = joint.transform.TransformPoint(joint.anchor);

			Quaternion axisOffset = Quaternion.AngleAxis(GetViewOffset(joint, axis), axis);
			Vector3 limitAxis = joint.transform.TransformDirection(axisOffset * axis).normalized;		
			Vector3 limitAxisOrtho = joint.transform.TransformDirection(axisOffset * new Vector3(axis.z, axis.x, axis.y)).normalized;
			Vector3 limitAxisCross = Vector3.Cross(limitAxis, limitAxisOrtho);
			
			Handles.color = color;
			Handles.DrawSolidArc(center, limitAxis, limitAxisCross, -limit, radius);
			
			Handles.color = new Color(color.r, color.g, color.b, 1f);
			GUI.color = new Color(color.r, color.g, color.b, 1f);

			//if (limitAxis != Vector3.zero) Handles.CircleCap(0, center, Quaternion.LookRotation(limitAxis), radius);

			Quaternion angleAxis = Quaternion.AngleAxis(-limit, limitAxis);
			Vector3 handleVector = angleAxis * limitAxisCross;
			
			Quaternion handleRotation = Quaternion.AngleAxis(-limit, limitAxis);
			if (limitAxisCross != Vector3.zero && limitAxisOrtho != Vector3.zero) {
				handleRotation = Quaternion.AngleAxis(-limit, limitAxis) * Quaternion.LookRotation(limitAxisCross) * Quaternion.LookRotation(limitAxisOrtho);
			}
			
			if (!drawHandles) return limit;

			float newLimit = Inspector.ScaleValueHandleSphere(limit, center + handleVector * radius, Quaternion.identity, radius, 1);
			//float newLimit = Handles.han(handleRotation, center + handleVector * radius, limit);

			string labelInfo = label;
			
			if (newLimit == 0) {
				labelInfo = "Open " + label;
				if (Inspector.SphereButton(center + handleVector * radius, handleRotation, radius * 0.2f, radius * 0.07f)) {
					newLimit = openValue;
				}
			}
			
			Handles.Label(center + handleVector * radius * 1.2f, labelInfo);

			if (newLimit != limit) {
				if (!isDragging) {
					Undo.RecordObject(joint, "Change Joint Limits");
					isDragging = true;
				}
			}

			GUI.color = Color.white;
			return newLimit;
		}
		
		public static SoftJointLimit NewJointLimit(float limit, SoftJointLimit referenceJointLimit, float min, float max) {
			SoftJointLimit newJointLimit = new SoftJointLimit();
			newJointLimit.limit = Mathf.Clamp(limit, min, max);
			newJointLimit.bounciness = referenceJointLimit.bounciness;
			return newJointLimit;
		}

		public static Color MlpAlpha(Color color, float alphaMlp) {
			return new Color(color.r, color.g, color.b, color.a * alphaMlp);
		}
		
		#endregion
		
		private static float GetViewOffset(Joint joint, Vector3 axis) {
			if (joint.connectedBody == null) return 0;

			Vector3 directionAxis = joint.transform.right;
			Axis viewAxis = GetViewAxis(joint);

			switch(viewAxis) {
			case Axis.Y: directionAxis = joint.transform.up; break;
			case Axis.Z: directionAxis = joint.transform.forward; break;
			}
			
			if (FlipAxis(joint, viewAxis)) directionAxis = -directionAxis;
			
			Vector3 axisWorld = joint.transform.TransformDirection(axis).normalized;
			Vector3 axisWorldOrtho = joint.transform.TransformDirection(new Vector3(axis.z, axis.x, axis.y)).normalized;
			
			float dot = Vector3.Dot(axisWorldOrtho, directionAxis);
			float dot2 = Vector3.Dot(axisWorldOrtho, Vector3.Cross(directionAxis, axisWorld));
			
			if (dot >= 0.5f) return -90;
			if (dot2 >= 0.5f) return 0;
			if (dot < -0.5f) return 90;
			if (dot2 < -0.5f) return 180;
			return 0;
		}
		
		private static Axis GetViewAxis(Joint joint) {
			if (joint.connectedBody == null) return Axis.Z;

			CapsuleCollider capsule = joint.GetComponent<CapsuleCollider>();
			if (capsule != null && capsule.center != Vector3.zero) return AxisTools.ToAxis(capsule.center);
			else {
				BoxCollider box = joint.GetComponent<BoxCollider>();
				if (box != null && box.center != Vector3.zero) return AxisTools.ToAxis(box.center);
			}

			return AxisTools.GetAxisToPoint(joint.transform, joint.connectedBody.worldCenterOfMass);
		}

		private static bool FlipAxis(Joint joint, Axis axis) {
			Vector3 direction = Vector3.Normalize(joint.transform.position - joint.connectedBody.worldCenterOfMass);
			
			CapsuleCollider capsuleCollider = joint.GetComponent<CapsuleCollider>();
			if (capsuleCollider != null) {
				direction = Vector3.Normalize(joint.transform.position - (joint.transform.position - (joint.transform.rotation * capsuleCollider.center)));
			}
			
			switch(axis) {
			case Axis.X:
				return Vector3.Dot(joint.transform.right, direction) < 0;
			case Axis.Y:
				return Vector3.Dot(joint.transform.up, direction) < 0;
			default: return Vector3.Dot(joint.transform.forward, direction) < 0;
			}
		}

	}
}
