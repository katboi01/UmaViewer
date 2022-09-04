using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace RootMotion.Dynamics {

	[CustomEditor(typeof(ConfigurableJoint))]
	public class ConfigurableJointInspector : JointInspector {
		
		private static Color 
			highAngularXColor = new Color(1, 0.75f, 0, 0.3f),
			lowAngularXColor = new Color(0.8f, 0.65f, 0, 0.3f),
			angularYColor = new Color(0, 1, 0, 0.3f),
			angularZColor = new Color(0, 0, 1, 0.3f);

		private const string lowX = "Low X", highX = "High X", y = "Y", z = "Z";

		public static void DrawJoint(ConfigurableJoint joint, bool modifiable = true, float alphaMlp = 1f) {
			if (joint == null) return;
			
			Vector3 axis = Vector3.zero;
			if (joint.axis != Vector3.zero) axis = joint.axis.normalized;
			Vector3 secondaryAxis = Vector3.zero;
			if (joint.secondaryAxis != Vector3.zero) secondaryAxis = joint.secondaryAxis.normalized;
			Vector3 crossAxis = secondaryAxis;
			if (secondaryAxis != axis) crossAxis = Vector3.Cross(secondaryAxis, axis);

			if (joint.angularXMotion == ConfigurableJointMotion.Limited) {
				float newLowAngularXLimit = DrawJointLimit(joint as Joint, lowX, axis, joint.lowAngularXLimit.limit, MlpAlpha(lowAngularXColor, alphaMlp), -25, modifiable);
				joint.lowAngularXLimit = NewJointLimit(newLowAngularXLimit, joint.lowAngularXLimit, -180, 180);
				
				float newHighAngularXLimit = joint.highAngularXLimit.limit;
				if (newLowAngularXLimit != 0 || (newLowAngularXLimit == 0 && newHighAngularXLimit != 0)) newHighAngularXLimit = DrawJointLimit(joint as Joint, highX, axis, joint.highAngularXLimit.limit, MlpAlpha(highAngularXColor, alphaMlp), 25, modifiable);
				joint.highAngularXLimit = NewJointLimit(newHighAngularXLimit, joint.highAngularXLimit, -180, 180);
			}
			
			if (joint.angularYMotion == ConfigurableJointMotion.Limited) {
				float newAngularYLimit = DrawJointLimit(joint as Joint, y, secondaryAxis, -joint.angularYLimit.limit, MlpAlpha(angularYColor, alphaMlp), 25, false);
				newAngularYLimit = DrawJointLimit(joint as Joint, y, secondaryAxis, -newAngularYLimit, MlpAlpha(angularYColor, alphaMlp), 25, modifiable);
				joint.angularYLimit = NewJointLimit(newAngularYLimit, joint.angularYLimit, 0, 180);
			}
			
			if (joint.angularZMotion == ConfigurableJointMotion.Limited) {
				float newAngularZLimit = DrawJointLimit(joint as Joint, z, crossAxis, -joint.angularZLimit.limit, MlpAlpha(angularZColor, alphaMlp), 25, false);
				newAngularZLimit = DrawJointLimit(joint as Joint, z, crossAxis, -newAngularZLimit, MlpAlpha(angularZColor, alphaMlp), 25, modifiable);
				joint.angularZLimit = NewJointLimit(newAngularZLimit, joint.angularZLimit, 0, 180);
			}
		}

		public void OnSceneGUI() {
			DrawJoint(target as ConfigurableJoint);
		}
	}
}
