using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// The target of an effector in the InteractionSystem.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Target")]
	public class InteractionTarget : MonoBehaviour {

        // Open the User Manual URL
        [ContextMenu("User Manual")]
        void OpenUserManual()
        {
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
        }

        // Open the Script Reference URL
        [ContextMenu("Scrpt Reference")]
        void OpenScriptReference()
        {
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_target.html");
        }

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
		void OpenTutorial1() {
			Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
		}
		
		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
		void OpenTutorial2() {
			Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
		}
		
		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
		void OpenTutorial3() {
			Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}
		
		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
		void OpenTutorial4() {
			Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
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

		/// <summary>
		/// Multiplies the value of a weight curve for this effector target.
		/// </summary>
		[System.Serializable]
		public class Multiplier {
			/// <summary>
			/// The curve type (InteractionObject.WeightCurve.Type).
			/// </summary>
			[Tooltip("The curve type (InteractionObject.WeightCurve.Type).")]
			public InteractionObject.WeightCurve.Type curve;
			/// <summary>
			/// Multiplier of the curve's value.
			/// </summary>
			[Tooltip("Multiplier of the curve's value.")]
			public float multiplier;
		}

		/// <summary>
		/// The type of the FBBIK effector.
		/// </summary>
		[Tooltip("The type of the FBBIK effector.")]
		public FullBodyBipedEffector effectorType;
		/// <summary>
		/// InteractionObject weight curve multipliers for this effector target.
		/// </summary>
		[Tooltip("InteractionObject weight curve multipliers for this effector target.")]
		public Multiplier[] multipliers;
		/// <summary>
		/// The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.
		/// </summary>
		[Tooltip("The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.")]
		public float interactionSpeedMlp = 1f;
		/// <summary>
		/// The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.
		/// </summary>
		[Tooltip("The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.")]
		public Transform pivot;
		/// <summary>
		/// The axis of twisting the interaction target.
		/// </summary>
		[Tooltip("The axis of twisting the interaction target (blue line).")]
		public Vector3 twistAxis = Vector3.up;
		/// <summary>
		/// The weight of twisting the interaction target towards the effector bone in the start of the interaction.
		/// </summary>
		[Tooltip("The weight of twisting the interaction target towards the effector bone in the start of the interaction.")]
		public float twistWeight = 1f;
		/// <summary>
		/// The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.
		/// </summary>
		[Tooltip("The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.")]
		public float swingWeight;
		/// <summary>
		/// If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.
		/// </summary>
		[Tooltip("If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.")]
		public bool rotateOnce = true;

		private Quaternion defaultLocalRotation;
		private Transform lastPivot;

		// Should a curve of the Type be ignored for this effector?
		public float GetValue(InteractionObject.WeightCurve.Type curveType) {
			for (int i = 0; i < multipliers.Length; i++) if (multipliers[i].curve == curveType) return multipliers[i].multiplier;
			return 1f;
		}

		// Reset the twist and swing rotation of the target
		public void ResetRotation() {
			if (pivot != null) pivot.localRotation = defaultLocalRotation;
		}

		// Rotate this target towards a position
		public void RotateTo(Vector3 position) {
			if (pivot == null) return;

			if (pivot != lastPivot) {
				defaultLocalRotation = pivot.localRotation;
				lastPivot = pivot;
			}

			// Rotate to the default local rotation
			pivot.localRotation = defaultLocalRotation;

			// Twisting around the twist axis
			if (twistWeight > 0f) {
				Vector3 targetTangent = transform.position - pivot.position;
				Vector3 n = pivot.rotation * twistAxis;
				Vector3 normal = n;
				Vector3.OrthoNormalize(ref normal, ref targetTangent);

				normal = n;
				Vector3 direction = position - pivot.position;
				Vector3.OrthoNormalize(ref normal, ref direction);

				Quaternion q = QuaTools.FromToAroundAxis(targetTangent, direction, n);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, q, twistWeight) * pivot.rotation;
			}

			// Swinging freely
			if (swingWeight > 0f) {
				Quaternion s = Quaternion.FromToRotation(transform.position - pivot.position, position - pivot.position);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, s, swingWeight) * pivot.rotation;
			}
		}
	}
}
