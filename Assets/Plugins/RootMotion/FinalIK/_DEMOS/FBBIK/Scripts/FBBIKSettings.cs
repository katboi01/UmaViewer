using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Manages FBBIK settings that are not visible in the FBBIK custom inspector.
	/// </summary>
	public class FBBIKSettings : MonoBehaviour {

		/// <summary>
		/// Settings for a limb
		/// </summary>
		[System.Serializable]
		public class Limb {
			public FBIKChain.Smoothing reachSmoothing; // Smoothing of the Reach effect (since 0.2)
			public float maintainRelativePositionWeight; // Weight of maintaining the limb's position relative to the body part that it is attached to (since 0.2, used to be IKEffector.Mode.MaintainRelativePosition)
			public float mappingWeight = 1f;

			// Apply the settings
			public void Apply(FullBodyBipedChain chain, IKSolverFullBodyBiped solver) {
				solver.GetChain(chain).reachSmoothing = reachSmoothing;
				solver.GetEndEffector(chain).maintainRelativePositionWeight = maintainRelativePositionWeight;
				solver.GetLimbMapping(chain).weight = mappingWeight;
			}
		}

		public FullBodyBipedIK ik; // Reference to the FBBIK component
		public bool disableAfterStart; // If true, will not update after Start
		public Limb leftArm, rightArm, leftLeg, rightLeg; // The Limbs

		public float rootPin = 0f; // Weight of pinning the root node to it's animated position
		public bool bodyEffectChildNodes = true; // If true, the body effector will also drag the thigh effectors

		// Apply all the settings to the FBBIK solver
		public void UpdateSettings() {
			if (ik == null) return;

			leftArm.Apply(FullBodyBipedChain.LeftArm, ik.solver);
			rightArm.Apply(FullBodyBipedChain.RightArm, ik.solver);
			leftLeg.Apply(FullBodyBipedChain.LeftLeg, ik.solver);
			rightLeg.Apply(FullBodyBipedChain.RightLeg, ik.solver);

			ik.solver.chain[0].pin = rootPin;
			ik.solver.bodyEffector.effectChildNodes = bodyEffectChildNodes;
		}

		void Start() {
			Debug.Log("FBBIKSettings is deprecated, you can now edit all the settings from the custom inspector of the FullBodyBipedIK component.");

			UpdateSettings();
			if (disableAfterStart) this.enabled = false;
		}

		void Update() {
			UpdateSettings();
		}
	}
}
