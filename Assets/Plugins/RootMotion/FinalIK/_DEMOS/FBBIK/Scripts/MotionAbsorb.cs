using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Absorbing motion on FBBIK effectors on impact. Attach this to the GameObject that receives OnCollisionEnter calls.
	/// </summary>
	public class MotionAbsorb : OffsetModifier {

		[System.Serializable]
		public enum Mode {
			Position,
			PositionOffset
		}

		// Manages motion absorbing for an effector
		[System.Serializable]
		public class Absorber {

			[Tooltip("The type of effector (hand, foot, shoulder...) - this is just an enum")]
			public FullBodyBipedEffector effector;
			[Tooltip("How much should motion be absorbed on this effector")]
			public float weight = 1f;

			private Vector3 position;
			private Quaternion rotation = Quaternion.identity;

			private IKEffector e;

			// Set effector position and rotation to match it's bone
			public void SetToBone(IKSolverFullBodyBiped solver, Mode mode) {
				e = solver.GetEffector(effector);
				// Using world space position and rotation here for the sake of simplicity of the demo
				// Ideally we should use position and rotation relative to character's root, so we could move around while doing this.

				switch(mode) {
				case Mode.Position:
					e.position = e.bone.position;
					e.rotation = e.bone.rotation;
				return;
				case Mode.PositionOffset:
					position = e.bone.position;
					rotation = e.bone.rotation;
				return;
				}
			}

			// Set effector position and rotation weight to match the value, multiply with the weight of this Absorber
			public void UpdateEffectorWeights(float w) {
				e.positionWeight = w * weight;
				e.rotationWeight = w * weight;
			}

			// Set effector positionOffset to match the position, multiply with the weight of this Absorber
			public void SetPosition(float w) {
				e.positionOffset += (position - e.bone.position) * w * weight;
			}

			// Set effector bone rotation to match the rotation, multiply with the weight of this Absorber
			public void SetRotation(float w) {
				e.bone.rotation = Quaternion.Slerp(e.bone.rotation, rotation, w * weight);
			}
		}

		[Tooltip("Use either effector position, position weight, rotation, rotationWeight or positionOffset and rotating the bone directly.")]
		public Mode mode;
		[Tooltip("Array containing the absorbers")]
		public Absorber[] absorbers;
		[Tooltip("Weight falloff curve (how fast will the effect reduce after impact)")]
		public AnimationCurve falloff;
		[Tooltip("How fast will the impact fade away. (if 1, effect lasts for 1 second)")]
		public float falloffSpeed = 1f;

		private float timer; // Used for fading out the effect of the impact
		private float w;
		private Mode initialMode;

		protected override void Start() {
			base.Start();

			ik.solver.OnPostUpdate += AfterIK;

			initialMode = mode;
		}

		void OnCollisionEnter(Collision c) {
			// Don't register another contact until the effect of the last one has faded 
			if (timer > 0f) return;

			// Reset timer
			timer = 1f;

			// Set effector position and rotation to match it's bone
			for (int i = 0; i < absorbers.Length; i++) absorbers[i].SetToBone(ik.solver, mode);
		}

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			if (timer <= 0f) return;
			mode = initialMode;

			// Fading out the effect
			timer -= Time.deltaTime * falloffSpeed;

			// Evaluate the absorb weight
			w = falloff.Evaluate(timer);

			// Set the weights of the effectors
			if (mode == Mode.Position) {
				for (int i = 0; i < absorbers.Length; i++) absorbers[i].UpdateEffectorWeights(w * weight);
			} else {
				for (int i = 0; i < absorbers.Length; i++) absorbers[i].SetPosition(w * weight);
			}
		}

		void AfterIK() {
			if (timer <= 0f) return;
			if (mode == Mode.Position) return;

			for (int i = 0; i < absorbers.Length; i++) absorbers[i].SetRotation(w * weight);
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			if (ik != null) ik.solver.OnPostUpdate -= AfterIK;
		}
	}
}
