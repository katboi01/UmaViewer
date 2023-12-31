using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Manages the execution order of IK components.
	/// </summary>
	public class IKExecutionOrder : MonoBehaviour {

		/// <summary>
		/// The IK components, assign in the order in which you wish to update them.
		/// </summary>
		[Tooltip("The IK components, assign in the order in which you wish to update them.")]
		public IK[] IKComponents;

		[Tooltip("Optional. Assign it if you are using 'Animate Physics' as the Update Mode.")]
		/// <summary>
		/// Optional. Assign it if you are using 'Animate Physics' as the Update Mode.
		/// </summary>
		public Animator animator;

		private bool fixedFrame;

		private bool animatePhysics {
			get {
				if (animator == null) return false;
				return animator.updateMode == AnimatorUpdateMode.AnimatePhysics;
			}
		}

		// Disable the IK components
		void Start() {
			for (int i = 0; i < IKComponents.Length; i++) IKComponents[i].enabled = false;
		}

		// Fix Transforms in Normal update mode
		void Update() {
			if (animatePhysics) return;

			FixTransforms ();
		}

		// Fix Transforms in Animate Physics update mode
		void FixedUpdate() {
			fixedFrame = true;

			if (animatePhysics) FixTransforms ();
		}

		// Update the IK components in a specific order
		void LateUpdate() {
			if (!animatePhysics || fixedFrame) {
				for (int i = 0; i < IKComponents.Length; i++) {
					IKComponents [i].GetIKSolver ().Update ();
				}

				fixedFrame = false;
			}
		}

		private void FixTransforms() {
			for (int i = 0; i < IKComponents.Length; i++) {
				if (IKComponents[i].fixTransforms) IKComponents[i].GetIKSolver().FixTransforms();
			}
		}
	}
}
