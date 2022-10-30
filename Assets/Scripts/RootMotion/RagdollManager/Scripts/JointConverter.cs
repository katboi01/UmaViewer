using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Contains tools for converting 3D Joints.
	/// </summary>
	public static class JointConverter {

		#region Public

		/// <summary>
		/// Converts any 3D joints on the root GameObject and it's children to ConfigurableJoints
		/// </summary>
		/// <param name="root">Root.</param>
		public static void ToConfigurable(GameObject root) {
			int count = 0;

			CharacterJoint[] characterJoints = root.GetComponentsInChildren<CharacterJoint>();
			for (int i = 0; i < characterJoints.Length; i++) {
				CharacterToConfigurable(characterJoints[i]);
				count ++;
			}
			
			HingeJoint[] hingeJoints = root.GetComponentsInChildren<HingeJoint>();
			for (int i = 0; i < hingeJoints.Length; i++) {
				HingeToConfigurable(hingeJoints[i]);
				count ++;
			}
			
			FixedJoint[] fixedJoints = root.GetComponentsInChildren<FixedJoint>();
			for (int i = 0; i < fixedJoints.Length; i++) {
				FixedToConfigurable(fixedJoints[i]);
				count ++;
			}
			
			SpringJoint[] springJoints = root.GetComponentsInChildren<SpringJoint>();
			for (int i = 0; i < springJoints.Length; i++) {
				SpringToConfigurable(springJoints[i]);
				count ++;
			}

			if (count > 0) {
				Debug.Log(count.ToString() + " joints were successfully converted to ConfigurableJoints.");
			} else {
				Debug.Log("No joints found in the children of " + root.name + " to convert to ConfigurableJoints.");
			}
		}

		/// <summary>
		/// Replaces a HingeJoint with a ConfigurableJoint.
		/// </summary>
		public static void HingeToConfigurable(HingeJoint src) {
			#if UNITY_EDITOR
			ConfigurableJoint conf = UnityEditor.Undo.AddComponent(src.gameObject, typeof(ConfigurableJoint)) as ConfigurableJoint;
			#else
			ConfigurableJoint conf = src.gameObject.AddComponent<ConfigurableJoint>();
			#endif

			ConvertJoint(ref conf, src as Joint);
			
			conf.secondaryAxis = Vector3.zero;
			
			conf.xMotion = ConfigurableJointMotion.Locked;
			conf.yMotion = ConfigurableJointMotion.Locked;
			conf.zMotion = ConfigurableJointMotion.Locked;
			
			conf.angularXMotion = src.useLimits? ConfigurableJointMotion.Limited: ConfigurableJointMotion.Free;
			conf.angularYMotion = ConfigurableJointMotion.Locked;
			conf.angularZMotion = ConfigurableJointMotion.Locked;
			
			conf.highAngularXLimit = ConvertToHighSoftJointLimit(src.limits, src.spring, src.useSpring);
			conf.angularXLimitSpring = ConvertToSoftJointLimitSpring(src.limits, src.spring, src.useSpring);
			conf.lowAngularXLimit = ConvertToLowSoftJointLimit(src.limits, src.spring, src.useSpring);

			if (src.useMotor) {
				Debug.LogWarning("Can not convert HingeJoint Motor to ConfigurableJoint.");
			}
		
			#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(src);
			#else
			GameObject.DestroyImmediate(src);
			#endif
		}

		/// <summary>
		/// Replaces a FixedJoint with a ConfigurableJoint.
		/// </summary>
		public static void FixedToConfigurable(FixedJoint src) {
			#if UNITY_EDITOR
			ConfigurableJoint conf = UnityEditor.Undo.AddComponent(src.gameObject, typeof(ConfigurableJoint)) as ConfigurableJoint;
			#else
			ConfigurableJoint conf = src.gameObject.AddComponent<ConfigurableJoint>();
			#endif

			ConvertJoint(ref conf, src as Joint);
			
			conf.secondaryAxis = Vector3.zero;
			
			conf.xMotion = ConfigurableJointMotion.Locked;
			conf.yMotion = ConfigurableJointMotion.Locked;
			conf.zMotion = ConfigurableJointMotion.Locked;
			
			conf.angularXMotion = ConfigurableJointMotion.Locked;
			conf.angularYMotion = ConfigurableJointMotion.Locked;
			conf.angularZMotion = ConfigurableJointMotion.Locked;
			
			#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(src);
			#else
			GameObject.DestroyImmediate(src);
			#endif
		}

		/// <summary>
		/// Replaces a SpringJoint with a ConfigurableJoint.
		/// </summary>
		public static void SpringToConfigurable(SpringJoint src) {
			#if UNITY_EDITOR
			ConfigurableJoint conf = UnityEditor.Undo.AddComponent(src.gameObject, typeof(ConfigurableJoint)) as ConfigurableJoint;
			#else
			ConfigurableJoint conf = src.gameObject.AddComponent<ConfigurableJoint>();
			#endif

			ConvertJoint(ref conf, src as Joint);
			
			conf.xMotion = ConfigurableJointMotion.Limited;
			conf.yMotion = ConfigurableJointMotion.Limited;
			conf.zMotion = ConfigurableJointMotion.Limited;
			
			conf.angularXMotion = ConfigurableJointMotion.Free;
			conf.angularYMotion = ConfigurableJointMotion.Free;
			conf.angularZMotion = ConfigurableJointMotion.Free;
			
			SoftJointLimit linearLimit = new SoftJointLimit();
			linearLimit.bounciness = 0f;
			linearLimit.limit = src.maxDistance;
			conf.linearLimit = linearLimit;

			SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
			linearLimitSpring.damper = src.damper;
			linearLimitSpring.spring = src.spring;
			conf.linearLimitSpring = linearLimitSpring;
			
			#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(src);
			#else
			GameObject.DestroyImmediate(src);
			#endif
		}
		
		/// <summary>
		/// Replaces a CharacterJoint with a ConfigurableJoint.
		/// </summary>
		public static void CharacterToConfigurable(CharacterJoint src) {
			#if UNITY_EDITOR
			ConfigurableJoint conf = UnityEditor.Undo.AddComponent(src.gameObject, typeof(ConfigurableJoint)) as ConfigurableJoint;
			#else
			ConfigurableJoint conf = src.gameObject.AddComponent<ConfigurableJoint>();
			#endif

			ConvertJoint(ref conf, src as Joint);

			conf.secondaryAxis = src.swingAxis;
			
			conf.xMotion = ConfigurableJointMotion.Locked;
			conf.yMotion = ConfigurableJointMotion.Locked;
			conf.zMotion = ConfigurableJointMotion.Locked;
			
			conf.angularXMotion = ConfigurableJointMotion.Limited;
			conf.angularYMotion = ConfigurableJointMotion.Limited;
			conf.angularZMotion = ConfigurableJointMotion.Limited;
			
			conf.highAngularXLimit = CopyLimit(src.highTwistLimit);
			conf.lowAngularXLimit = CopyLimit(src.lowTwistLimit);
			conf.angularYLimit = CopyLimit(src.swing1Limit);
			conf.angularZLimit = CopyLimit(src.swing2Limit);

			conf.angularXLimitSpring = CopyLimitSpring(src.twistLimitSpring);
			conf.angularYZLimitSpring = CopyLimitSpring(src.swingLimitSpring);

			conf.enableCollision = src.enableCollision;

			conf.projectionMode = src.enableProjection? JointProjectionMode.PositionAndRotation: JointProjectionMode.None;
			conf.projectionAngle = src.projectionAngle;
			conf.projectionDistance = src.projectionDistance;
			
			#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(src);
			#else
			GameObject.DestroyImmediate(src);
			#endif
		}

		#endregion Public
		
		// Common to all joints
		private static void ConvertJoint(ref ConfigurableJoint conf, Joint src) {
			conf.anchor = src.anchor;
			conf.autoConfigureConnectedAnchor = src.autoConfigureConnectedAnchor;
			conf.axis = src.axis;
			conf.breakForce = src.breakForce;
			conf.breakTorque = src.breakTorque;
			conf.connectedAnchor = src.connectedAnchor;
			conf.connectedBody = src.connectedBody;
			conf.enableCollision = src.enableCollision;
		}
		
		// Conversion from JointLimit and JointSpring to high SoftJointLimit
		private static SoftJointLimit ConvertToHighSoftJointLimit(JointLimits src, JointSpring spring, bool useSpring) {
			SoftJointLimit limit = new SoftJointLimit();
			limit.limit = -src.max;
			limit.bounciness = src.bounciness;
			return limit;
		}

		// Conversion from JointLimit and JointSpring to low SoftJointLimit
		private static SoftJointLimit ConvertToLowSoftJointLimit(JointLimits src, JointSpring spring, bool useSpring) {
			SoftJointLimit limit = new SoftJointLimit();
			limit.limit = -src.min;
			limit.bounciness = src.bounciness;
			return limit;
		}

		// Conversion from JointLimit and JointSpring to SoftJointLimitSpring
		private static SoftJointLimitSpring ConvertToSoftJointLimitSpring(JointLimits src, JointSpring spring, bool useSpring) {
			SoftJointLimitSpring limitSpring = new SoftJointLimitSpring();
			limitSpring.damper = useSpring? spring.damper: 0f;
			limitSpring.spring = useSpring? spring.spring: 0f;
			return limitSpring;
		}
		
		// Returns a copy of the specified SoftJointLimit
		private static SoftJointLimit CopyLimit(SoftJointLimit src) {
			SoftJointLimit limit = new SoftJointLimit();
			limit.limit = src.limit;
			limit.bounciness = src.bounciness;
			return limit;
		}

		// Returns a copy of the specified SoftJointLimitSpring
		private static SoftJointLimitSpring CopyLimitSpring(SoftJointLimitSpring src) {
			SoftJointLimitSpring limitSpring = new SoftJointLimitSpring();
			limitSpring.damper = src.damper;
			limitSpring.spring = src.spring;
			return limitSpring;
		}
	}
}