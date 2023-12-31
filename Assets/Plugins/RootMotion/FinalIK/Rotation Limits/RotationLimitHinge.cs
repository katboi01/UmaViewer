using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// The hinge rotation limit limits the rotation to 1 degree of freedom around Axis. This rotation limit is additive which means the limits can exceed 360 degrees.
	/// </summary>
	[HelpURL("http://www.root-motion.com/finalikdox/html/page14.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Hinge")]
	public class RotationLimitHinge : RotationLimit {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		private void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page14.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_hinge.html");
		}
		
		// Link to the Final IK Google Group
		[ContextMenu("Support Group")]
		void SupportGroup() {
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}
		
		// Link to the Final IK Asset Store thread in the Unity Community
		[ContextMenu("Asset Store Thread")]
		void ASThread() {
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		#region Main Interface
		
		/// <summary>
		/// Should the rotation be limited around the axis?
		/// </summary>
		public bool useLimits = true;
		/// <summary>
		/// The min limit around the axis.
		/// </summary>
		public float min = -45;
		/// <summary>
		/// The max limit around the axis.
		/// </summary>
		public float max = 90;
		
		#endregion Main Interface
		
		/*
		 * Limits the rotation in the local space of this instance's Transform.
		 * */
		protected override Quaternion LimitRotation(Quaternion rotation) {
			lastRotation = LimitHinge(rotation);
			return lastRotation;
		}

		[HideInInspector] public float zeroAxisDisplayOffset; // Angular offset of the scene view display of the Hinge rotation limit
		
		private Quaternion lastRotation = Quaternion.identity;
		private float lastAngle;
		
		/*
		 * Apply the hinge rotation limit
		 * */
		private Quaternion LimitHinge(Quaternion rotation) {
			// If limit is zero return rotation fixed to axis
			if (min == 0 && max == 0 && useLimits) return Quaternion.AngleAxis(0, axis);
			
			// Get 1 degree of freedom rotation along axis
			Quaternion free1DOF = Limit1DOF(rotation, axis);
			if (!useLimits) return free1DOF;

			// Get offset from last rotation in angle-axis representation
			Quaternion addR = free1DOF * Quaternion.Inverse(lastRotation);

			float addA = Quaternion.Angle(Quaternion.identity, addR);

			Vector3 secondaryAxis = new Vector3(axis.z, axis.x, axis.y);
			Vector3 cross = Vector3.Cross(secondaryAxis, axis);
			if (Vector3.Dot(addR * secondaryAxis, cross) > 0f) addA = - addA;
			
			// Clamp to limits
			lastAngle = Mathf.Clamp(lastAngle + addA, min, max);
			
			return Quaternion.AngleAxis(lastAngle, axis);
		}
	}
}
