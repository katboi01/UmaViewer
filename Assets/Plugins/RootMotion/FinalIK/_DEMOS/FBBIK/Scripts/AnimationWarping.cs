using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Warping an effector from animation space to world space.
	/// The weight curve of the warp is used to add the offset from "Warp From" to "Warp To" to the effector.
	/// "Warp From" should be a Transform parented to the root of the character, hence in animation space (virtual position where a soccer player's foot hits the ball in the animation).
	/// "Warp To" should be a Transform in the world space (the actual ball).
	/// </summary>
	public class AnimationWarping : OffsetModifier {

		/// <summary>
		/// Definition of a warp from 'warpFrom' to 'warpTo' by normalized time of the animation
		/// </summary>
		[System.Serializable]
		public struct Warp {
			[Tooltip("Layer of the 'Animation State' in the Animator.")]
			public int animationLayer;
			[Tooltip("Name of the state in the Animator to warp.")]
			public string animationState;
			[Tooltip("Warping weight by normalized time of the animation state.")]
			public AnimationCurve weightCurve;
			[Tooltip("Animated point to warp from. This should be in character space so keep this Transform parented to the root of the character.")]
			public Transform warpFrom;
			[Tooltip("World space point to warp to.")]
			public Transform warpTo;
			[Tooltip("Which FBBIK effector to use?")]
			public FullBodyBipedEffector effector;
		}

		/// <summary>
		/// Using effector.positionOffset or effector.position with effector.positionWeight?
		/// </summary>
		[System.Serializable]
		public enum EffectorMode {
			PositionOffset,
			Position,
		}

		[Tooltip("Reference to the Animator component to use")]
		public Animator animator;
		[Tooltip("Using effector.positionOffset or effector.position with effector.positionWeight? " +
			"The former will enable you to use effector.position for other things, the latter will weigh in the effectors, hence using Reach and Pull in the process.")]
		public EffectorMode effectorMode;

		[Space(10)]
		[Tooltip("The array of warps, can have multiple simultaneous warps.")]
		public Warp[] warps;

		private EffectorMode lastMode;

		protected override void Start() {
			base.Start();

			lastMode = effectorMode;
		}

		/// <summary>
		/// Gets the current warping weight of the warp at the specified index.
		/// </summary>
		public float GetWarpWeight(int warpIndex) {
			if (warpIndex < 0) {
				Debug.LogError("Warp index out of range.");
				return 0f;
			}
			if (warpIndex >= warps.Length) {
				Debug.LogError("Warp index out of range.");
				return 0f;
			}
			if (animator == null) {
				Debug.LogError("Animator unassigned in AnimationWarping");
				return 0f;
			}

			// Get the animator state info
			AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(warps[warpIndex].animationLayer);

			// If not currently playing the animation state of the warp, return
			if (!info.IsName(warps[warpIndex].animationState)) return 0f;

			// Evaluate the weight of the warp by the current normalized time of the state
			return warps[warpIndex].weightCurve.Evaluate(info.normalizedTime - (int)info.normalizedTime);
		}

		// Called each time before FBBIK solves
		protected override void OnModifyOffset() {
			// Go through all the warps...
			for (int i = 0; i < warps.Length; i++) {
				float warpWeight = GetWarpWeight(i);

				// Get the offset form warpFrom to warpTo
				Vector3 offset = warps[i].warpTo.position - warps[i].warpFrom.position;

				// Add that offset to the effector (using positionOffset additively, because it will be reset to Vector3.zero by FBBIK after each update)
				switch(effectorMode) {
				case EffectorMode.PositionOffset:
					ik.solver.GetEffector(warps[i].effector).positionOffset += offset * warpWeight * weight;
					break;
				case EffectorMode.Position:
					ik.solver.GetEffector(warps[i].effector).position = ik.solver.GetEffector(warps[i].effector).bone.position + offset;
					ik.solver.GetEffector(warps[i].effector).positionWeight = weight * warpWeight;
					break;
				}
			}

			// Switching modes safely, weighing out effector positionWeights
			if (lastMode == EffectorMode.Position && effectorMode == EffectorMode.PositionOffset) {
				foreach (Warp warp in warps) {
					ik.solver.GetEffector(warp.effector).positionWeight = 0f;
				}
			}
			
			lastMode = effectorMode;
		}

		// Set effector positionWeights to 0 if in "Position" effector mode
		void OnDisable() {
			if (effectorMode != EffectorMode.Position) return;

			foreach (Warp warp in warps) {
				ik.solver.GetEffector(warp.effector).positionWeight = 0f;
			}
		}
	}
}
