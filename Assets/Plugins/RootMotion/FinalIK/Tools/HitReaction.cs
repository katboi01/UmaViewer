using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Class for creating procedural FBBIK hit reactions.
	/// </summary>
	public class HitReaction : OffsetModifier {

		/// <summary>
		/// Hit point definition
		/// </summary>
		[System.Serializable]
		public abstract class HitPoint {

			[Tooltip("Just for visual clarity, not used at all")]
			public string name;
			[Tooltip("Linking this hit point to a collider")]
			public Collider collider;
			[Tooltip("Only used if this hit point gets hit when already processing another hit")]
			[SerializeField] float crossFadeTime = 0.1f;

			public bool inProgress { get { return timer < length; } }

			protected float crossFader { get; private set; }
			protected float timer { get; private set; }
			protected Vector3 force { get; private set; }
			protected Vector3 point { get; private set; }

			private float length;
			private float crossFadeSpeed;
			private float lastTime;

			// Start processing the hit
			public void Hit(Vector3 force, Vector3 point) {
				if (length == 0f) length = GetLength();
				if (length <= 0f) {
					Debug.LogError("Hit Point WeightCurve length is zero.");
					return;
				}

				// Start crossfading if the last hit has not completed yet
				if (timer < 1f) crossFader = 0f;
				crossFadeSpeed = crossFadeTime > 0f? 1f / crossFadeTime: 0f;
				CrossFadeStart();

				// Reset timer
				timer = 0f;

				// Remember hit direction and point
				this.force = force;
				this.point = point;
			}

			// Apply to IKSolverFullBodyBiped
			public void Apply(IKSolverFullBodyBiped solver, float weight) {
				float deltaTime = Time.time - lastTime;
				lastTime = Time.time;

				if (timer >= length) {
					return;
				}

				// Advance the timer
				timer = Mathf.Clamp(timer + deltaTime, 0f, length);

				// Advance the crossFader
				if (crossFadeSpeed > 0f) crossFader = Mathf.Clamp(crossFader + (deltaTime * crossFadeSpeed), 0f, 1f);
				else crossFader = 1f;

				// Pass this on to the hit points
				OnApply(solver, weight);
			}

			protected abstract float GetLength();
			protected abstract void CrossFadeStart();
			protected abstract void OnApply(IKSolverFullBodyBiped solver, float weight);
		}

		/// <summary>
		/// Hit Point for FBBIK effectors
		/// </summary>
		[System.Serializable]
		public class HitPointEffector: HitPoint {

			/// <summary>
			/// Linking a FBBIK effector to this effector hit point
			/// </summary>
			[System.Serializable]
			public class EffectorLink {
				[Tooltip("The FBBIK effector type")]
				public FullBodyBipedEffector effector;
				[Tooltip("The weight of this effector (could also be negative)")]
				public float weight;

				private Vector3 lastValue;
				private Vector3 current;

				// Apply an offset to this effector
				public void Apply(IKSolverFullBodyBiped solver, Vector3 offset, float crossFader) {
					current = Vector3.Lerp(lastValue, offset * weight, crossFader);

					solver.GetEffector(effector).positionOffset += current;
				}

				// Remember the current offset value, so we can smoothly crossfade from it
				public void CrossFadeStart() {
					lastValue = current;
				}
			}

			[Tooltip("Offset magnitude in the direction of the hit force")]
			public AnimationCurve offsetInForceDirection; // 
			[Tooltip("Offset magnitude in the direction of character.up")]
			public AnimationCurve offsetInUpDirection; // 
			[Tooltip("Linking this offset to the FBBIK effectors")]
			public EffectorLink[] effectorLinks;

			// Returns the length of this hit (last key in the AnimationCurves)
			protected override float GetLength() {
				float time1 = offsetInForceDirection.keys.Length > 0? offsetInForceDirection.keys[offsetInForceDirection.length - 1].time: 0f;
				float time2 = offsetInUpDirection.keys.Length > 0? offsetInUpDirection.keys[offsetInUpDirection.length - 1].time: 0f;
				return Mathf.Clamp(time1, time2, time1);
			}

			// Remember the current offset values for each effector, so we can smoothly crossfade from it
			protected override void CrossFadeStart() {
				foreach (EffectorLink e in effectorLinks) e.CrossFadeStart();
			}

			// Calculate offset, apply to FBBIK effectors
			protected override void OnApply(IKSolverFullBodyBiped solver, float weight) {
				Vector3 up = solver.GetRoot().up * force.magnitude;

				Vector3 offset = (offsetInForceDirection.Evaluate(timer) * force) + (offsetInUpDirection.Evaluate(timer) * up);
				offset *= weight;

				foreach (EffectorLink e in effectorLinks) e.Apply(solver, offset, crossFader);
			}

		}

		/// <summary>
		/// Hit Point for simple bone Transforms that don't have a FBBIK effector
		/// </summary>
		[System.Serializable]
		public class HitPointBone: HitPoint {

			/// <summary>
			/// Linking a bone Transform to this bone hit point
			/// </summary>
			[System.Serializable]
			public class BoneLink {
				[Tooltip("Reference to the bone that this hit point rotates")]
				public Transform bone;
				[Tooltip("Weight of rotating the bone")]
				[Range(0f, 1f)] public float weight;

				private Quaternion lastValue = Quaternion.identity;
				private Quaternion current = Quaternion.identity;

				// Apply a rotational offset to this effector
				public void Apply(IKSolverFullBodyBiped solver, Quaternion offset, float crossFader) {
					current = Quaternion.Lerp(lastValue, Quaternion.Lerp(Quaternion.identity, offset, weight), crossFader);

					bone.rotation = current * bone.rotation;
				}

				// Remember the current offset value, so we can smoothly crossfade from it
				public void CrossFadeStart() {
					lastValue = current;
				}
			}

			[Tooltip("The angle to rotate the bone around it's rigidbody's world center of mass")]
			public AnimationCurve aroundCenterOfMass;
			[Tooltip("Linking this hit point to bone(s)")]
			public BoneLink[] boneLinks;

			private Rigidbody rigidbody;

			// Returns the length of this hit (last key in the AnimationCurves)
			protected override float GetLength() {
				return aroundCenterOfMass.keys.Length > 0? aroundCenterOfMass.keys[aroundCenterOfMass.length - 1].time: 0f;
			}

			// Remember the current offset values for each bone, so we can smoothly crossfade from it
			protected override void CrossFadeStart() {
				foreach (BoneLink b in boneLinks) b.CrossFadeStart();
			}

			// Calculate offset, apply to the bones
			protected override void OnApply(IKSolverFullBodyBiped solver, float weight) {
				if (rigidbody == null) rigidbody = collider.GetComponent<Rigidbody>();
				if (rigidbody != null) {
					Vector3 comAxis = Vector3.Cross(force, point - rigidbody.worldCenterOfMass);
					float comValue = aroundCenterOfMass.Evaluate(timer) * weight;
					Quaternion offset = Quaternion.AngleAxis(comValue, comAxis);

					foreach (BoneLink b in boneLinks) b.Apply(solver, offset, crossFader);
				}
			}

		}

		[Tooltip("Hit points for the FBBIK effectors")]
		public HitPointEffector[] effectorHitPoints;
		[Tooltip(" Hit points for bones without an effector, such as the head")]
		public HitPointBone[] boneHitPoints;

		/// <summary>
		/// Returns true if any of the hits are being processed.
		/// </summary>
		public bool inProgress {
			get {
				foreach (HitPointEffector h in effectorHitPoints) {
					if (h.inProgress) return true;
				}
				foreach (HitPointBone h in boneHitPoints) {
					if (h.inProgress) return true;
				}
				return false;
			}
		}

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			foreach (HitPointEffector e in effectorHitPoints) e.Apply(ik.solver, weight);
			foreach (HitPointBone b in boneHitPoints) b.Apply(ik.solver, weight);
		}

		// Hit one of the hit points (defined by hit.collider)
		public void Hit(Collider collider, Vector3 force, Vector3 point) {
			if (ik == null) {
				Debug.LogError("No IK assigned in HitReaction");
				return;
			}

			foreach (HitPointEffector e in effectorHitPoints) {
				if (e.collider == collider) e.Hit(force, point);
			}

			foreach (HitPointBone b in boneHitPoints) {
				if (b.collider == collider) b.Hit(force, point);
			}
		}
	}
}
