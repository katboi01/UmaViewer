using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Class for creating procedural FBBIK hit reactions.
	/// </summary>
	public class HitReactionVRIK : OffsetModifierVRIK {

		public AnimationCurve[] offsetCurves;

		/// <summary>
		/// Hit point definition
		/// </summary>
		[System.Serializable]
		public abstract class Offset {

			[Tooltip("Just for visual clarity, not used at all")]
			public string name;
			[Tooltip("Linking this hit point to a collider")]
			public Collider collider;
			[Tooltip("Only used if this hit point gets hit when already processing another hit")]
			[SerializeField] float crossFadeTime = 0.1f;

			protected float crossFader { get; private set; }
			protected float timer { get; private set; }
			protected Vector3 force { get; private set; }
			protected Vector3 point { get; private set; }

			private float length;
			private float crossFadeSpeed;
			private float lastTime;

			// Start processing the hit
			public void Hit(Vector3 force, AnimationCurve[] curves, Vector3 point) {
				if (length == 0f) length = GetLength(curves);
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
			public void Apply(VRIK ik, AnimationCurve[] curves, float weight) {
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
				OnApply(ik, curves, weight);
			}

			protected abstract float GetLength(AnimationCurve[] curves);
			protected abstract void CrossFadeStart();
			protected abstract void OnApply(VRIK ik, AnimationCurve[] curves, float weight);
		}

		/// <summary>
		/// Hit Point for FBBIK effectors
		/// </summary>
		[System.Serializable]
		public class PositionOffset: Offset {

			/// <summary>
			/// Linking a FBBIK effector to this effector hit point
			/// </summary>
			[System.Serializable]
			public class PositionOffsetLink {
				[Tooltip("The FBBIK effector type")]
				public IKSolverVR.PositionOffset positionOffset;
				[Tooltip("The weight of this effector (could also be negative)")]
				public float weight;

				private Vector3 lastValue;
				private Vector3 current;

				// Apply an offset to this effector
				public void Apply(VRIK ik, Vector3 offset, float crossFader) {
					current = Vector3.Lerp(lastValue, offset * weight, crossFader);

					ik.solver.AddPositionOffset(positionOffset, current);
				}

				// Remember the current offset value, so we can smoothly crossfade from it
				public void CrossFadeStart() {
					lastValue = current;
				}
			}

			[Tooltip("Offset magnitude in the direction of the hit force")]
			public int forceDirCurveIndex;
			[Tooltip("Offset magnitude in the direction of character.up")]
			public int upDirCurveIndex = 1;
			[Tooltip("Linking this offset to the VRIK position offsets")]
			public PositionOffsetLink[] offsetLinks;

			// Returns the length of this hit (last key in the AnimationCurves)
			protected override float GetLength(AnimationCurve[] curves) {
				float time1 = curves[forceDirCurveIndex].keys.Length > 0? curves[forceDirCurveIndex].keys[curves[forceDirCurveIndex].length - 1].time: 0f;
				float time2 = curves[upDirCurveIndex].keys.Length > 0? curves[upDirCurveIndex].keys[curves[upDirCurveIndex].length - 1].time: 0f;
				return Mathf.Clamp(time1, time2, time1);
			}

			// Remember the current offset values for each effector, so we can smoothly crossfade from it
			protected override void CrossFadeStart() {
				foreach (PositionOffsetLink l in offsetLinks) l.CrossFadeStart();
			}

			// Calculate offset, apply to FBBIK effectors
			protected override void OnApply(VRIK ik, AnimationCurve[] curves, float weight) {
				Vector3 up = ik.transform.up * force.magnitude;

				Vector3 offset = (curves[forceDirCurveIndex].Evaluate(timer) * force) + (curves[upDirCurveIndex].Evaluate(timer) * up);
				offset *= weight;

				foreach (PositionOffsetLink l in offsetLinks) l.Apply(ik, offset, crossFader);
			}

		}

		/// <summary>
		/// Hit Point for simple bone Transforms that don't have a FBBIK effector
		/// </summary>
		[System.Serializable]
		public class RotationOffset: Offset {

			/// <summary>
			/// Linking a bone Transform to this bone hit point
			/// </summary>
			[System.Serializable]
			public class RotationOffsetLink {
				[Tooltip("Reference to the bone that this hit point rotates")]
				public IKSolverVR.RotationOffset rotationOffset;
				[Tooltip("Weight of rotating the bone")]
				[Range(0f, 1f)] public float weight;

				private Quaternion lastValue = Quaternion.identity;
				private Quaternion current = Quaternion.identity;

				// Apply a rotational offset to this effector
				public void Apply(VRIK ik, Quaternion offset, float crossFader) {
					current = Quaternion.Lerp(lastValue, Quaternion.Lerp(Quaternion.identity, offset, weight), crossFader);

					ik.solver.AddRotationOffset(rotationOffset, current);
				}

				// Remember the current offset value, so we can smoothly crossfade from it
				public void CrossFadeStart() {
					lastValue = current;
				}
			}

			[Tooltip("The angle to rotate the bone around it's rigidbody's world center of mass")]
			public int curveIndex;
			[Tooltip("Linking this hit point to bone(s)")]
			public RotationOffsetLink[] offsetLinks;

			private Rigidbody rigidbody;

			// Returns the length of this hit (last key in the AnimationCurves)
			protected override float GetLength(AnimationCurve[] curves) {
				return curves[curveIndex].keys.Length > 0?  curves[curveIndex].keys[ curves[curveIndex].length - 1].time: 0f;
			}

			// Remember the current offset values for each bone, so we can smoothly crossfade from it
			protected override void CrossFadeStart() {
				foreach (RotationOffsetLink l in offsetLinks) l.CrossFadeStart();
			}

			// Calculate offset, apply to the bones
			protected override void OnApply(VRIK ik, AnimationCurve[] curves, float weight) {
				if (collider == null) {
					Debug.LogError ("No collider assigned for a HitPointBone in the HitReaction component.");
					return;
				}

				if (rigidbody == null) rigidbody = collider.GetComponent<Rigidbody>();
				if (rigidbody != null) {
					Vector3 comAxis = Vector3.Cross(force, point - rigidbody.worldCenterOfMass);
					float comValue = curves[curveIndex].Evaluate(timer) * weight;
					Quaternion offset = Quaternion.AngleAxis(comValue, comAxis);

					foreach (RotationOffsetLink l in offsetLinks) l.Apply(ik, offset, crossFader);
				}
			}

		}

		[Tooltip("Hit points for the FBBIK effectors")]
		public PositionOffset[] positionOffsets;
		[Tooltip(" Hit points for bones without an effector, such as the head")]
		public RotationOffset[] rotationOffsets;

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			foreach (PositionOffset p in positionOffsets) p.Apply(ik, offsetCurves, weight);
			foreach (RotationOffset r in rotationOffsets) r.Apply(ik, offsetCurves, weight);
		}

		// Hit one of the hit points (defined by hit.collider)
		public void Hit(Collider collider, Vector3 force, Vector3 point) {
			if (ik == null) {
				Debug.LogError("No IK assigned in HitReaction");
				return;
			}

			foreach (PositionOffset p in positionOffsets) {
				if (p.collider == collider) p.Hit(force, offsetCurves, point);
			}

			foreach (RotationOffset r in rotationOffsets) {
				if (r.collider == collider) r.Hit(force, offsetCurves, point);
			}
		}
	}
}
