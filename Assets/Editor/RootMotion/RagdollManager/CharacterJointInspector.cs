using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace RootMotion.Dynamics {

	[CustomEditor(typeof(CharacterJoint))]
	public class CharacterJointInspector : JointInspector {
		
		private static Color 
			highTwistColor = new Color(1, 0.75f, 0, 0.3f),
			lowTwistColor = new Color(0.8f, 0.65f, 0, 0.3f),
			swing1Color = new Color(0, 1, 0, 0.3f),
			swing2Color = new Color(0, 0, 1, 0.3f);

		public static void DrawJoint(CharacterJoint joint, bool modifiable = true, float alphaMlp = 1f) {
			if (joint == null) return;
			
			Vector3 axis = Vector3.zero;
			if (joint.axis != Vector3.zero) axis = joint.axis.normalized;
			Vector3 swingAxis = Vector3.zero;
			if (joint.swingAxis != Vector3.zero) swingAxis = joint.swingAxis.normalized;
			Vector3 crossAxis = swingAxis;
			if (swingAxis != axis) crossAxis = Vector3.Cross(swingAxis, axis);

			float newLowTwistLimit = DrawJointLimit(joint as Joint, "Low Twist Limit", axis, joint.lowTwistLimit.limit, MlpAlpha(lowTwistColor, alphaMlp), -25, modifiable);
			joint.lowTwistLimit = NewJointLimit(newLowTwistLimit, joint.lowTwistLimit, -180, 180);
			
			float newHighTwistLimit = joint.highTwistLimit.limit;
			if (newLowTwistLimit != 0 || (newLowTwistLimit == 0 && newHighTwistLimit != 0)) newHighTwistLimit = DrawJointLimit(joint as Joint, "High Twist Limit", axis, joint.highTwistLimit.limit, MlpAlpha(highTwistColor, alphaMlp), 25, modifiable);
			joint.highTwistLimit = NewJointLimit(newHighTwistLimit, joint.highTwistLimit, -180, 180);
			
			float newSwing1Limit = DrawJointLimit(joint as Joint, "Swing1 Limit", swingAxis, -joint.swing1Limit.limit, MlpAlpha(swing1Color, alphaMlp), 25, false);
			newSwing1Limit = DrawJointLimit(joint as Joint, "Swing1 Limit", swingAxis, -newSwing1Limit, MlpAlpha(swing1Color, alphaMlp), 25, modifiable);
			joint.swing1Limit = NewJointLimit(newSwing1Limit, joint.swing1Limit, 0, 180);
			
			float newSwing2Limit = DrawJointLimit(joint as Joint, "Swing2 Limit", crossAxis, -joint.swing2Limit.limit, MlpAlpha(swing2Color, alphaMlp), 25, false);
			newSwing2Limit = DrawJointLimit(joint as Joint, "Swing2 Limit", crossAxis, -newSwing2Limit, MlpAlpha(swing2Color, alphaMlp), 25, modifiable);
			joint.swing2Limit = NewJointLimit(newSwing2Limit, joint.swing2Limit, 0, 180);
		}

		public void OnSceneGUI() {
			DrawJoint(target as CharacterJoint);
		}
	}
}
