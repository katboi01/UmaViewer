using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Procedural recoil using FBBIK.
	/// </summary>
	public class Recoil : OffsetModifier {
		
		[System.Serializable]
		public class RecoilOffset {

			[Tooltip("Offset vector for the associated effector when doing recoil.")]
			public Vector3 offset;
			[Tooltip("When firing before the last recoil has faded, how much of the current recoil offset will be maintained?")]
			[Range(0f, 1f)] public float additivity = 1f;
			[Tooltip("Max additive recoil for automatic fire.")]
			public float maxAdditiveOffsetMag = 0.2f;
			
			// Linking this to an effector
			[System.Serializable]
			public class EffectorLink {
				[Tooltip("Type of the FBBIK effector to use")]
				public FullBodyBipedEffector effector;
				[Tooltip("Weight of using this effector")]
				public float weight;
			}

			[Tooltip("Linking this recoil offset to FBBIK effectors.")]
			public EffectorLink[] effectorLinks;

			private Vector3 additiveOffset;
			private Vector3 lastOffset;

			// Start recoil
			public void Start() {
				if (additivity <= 0f) return;

				additiveOffset = Vector3.ClampMagnitude(lastOffset * additivity, maxAdditiveOffsetMag);
			}

			// Apply offset to FBBIK effectors
			public void Apply(IKSolverFullBodyBiped solver, Quaternion rotation, float masterWeight, float length, float timeLeft) {
				additiveOffset = Vector3.Lerp(Vector3.zero, additiveOffset, timeLeft / length);
				lastOffset = (rotation * (offset * masterWeight)) + (rotation * additiveOffset);

				foreach (EffectorLink e in effectorLinks) {
					solver.GetEffector(e.effector).positionOffset += lastOffset * e.weight;
				}
			}
		}

		[System.Serializable]
		public enum Handedness {
			Right,
			Left
		}

		[Tooltip("Reference to the AimIK component. Optional, only used to getting the aiming direction.")]
		public AimIK aimIK;
		[Tooltip("Set this true if you are using IKExecutionOrder.cs or a custom script to force AimIK solve after FBBIK.")]
		public bool aimIKSolvedLast;
		[Tooltip("Which hand is holding the weapon?")]
		public Handedness handedness;
		[Tooltip("Check for 2-handed weapons.")]
		public bool twoHanded = true;
		[Tooltip("Weight curve for the recoil offsets. Recoil procedure is as long as this curve.")]
		public AnimationCurve recoilWeight;
		[Tooltip("How much is the magnitude randomized each time Recoil is called?")]
		public float magnitudeRandom = 0.1f;
		[Tooltip("How much is the rotation randomized each time Recoil is called?")]
		public Vector3 rotationRandom;
		[Tooltip("Rotating the primary hand bone for the recoil (in local space).")]
		public Vector3 handRotationOffset;
		[Tooltip("Time of blending in another recoil when doing automatic fire.")]
		public float blendTime;

		[Space(10)]

		[Tooltip("FBBIK effector position offsets for the recoil (in aiming direction space).")]
		public RecoilOffset[] offsets;

		[HideInInspector] public Quaternion rotationOffset = Quaternion.identity;

		private float magnitudeMlp = 1f;
		private float endTime = -1f;
		private Quaternion handRotation, secondaryHandRelativeRotation, randomRotation;
		private float length = 1f;
		private bool initiated;
		private float blendWeight;
		private float w;
		private Quaternion primaryHandRotation = Quaternion.identity;
		//private Quaternion secondaryHandRotation = Quaternion.identity;
		private bool handRotationsSet;
		private Vector3 aimIKAxis;

		/// <summary>
		/// Returns true if recoil has finished or has not been called at all.
		/// </summary>
		public bool isFinished {
			get {
				return Time.time > endTime;
			}
		}

		/// <summary>
		/// Sets the starting rotations for the hands for 1 frame. Use this if the final rotation of the hands will not be the same as before FBBIK solves.
		/// </summary>
		public void SetHandRotations(Quaternion leftHandRotation, Quaternion rightHandRotation) {
			if (handedness == Handedness.Left) {
				primaryHandRotation = leftHandRotation;
				//secondaryHandRotation = rightHandRotation;
			} else {
				primaryHandRotation = rightHandRotation;
				//secondaryHandRotation = leftHandRotation;
			}

			handRotationsSet = true;
		}

		/// <summary>
		/// Starts the recoil procedure.
		/// </summary>
		public void Fire(float magnitude) {
			float rnd = magnitude * UnityEngine.Random.value * magnitudeRandom;
			magnitudeMlp = magnitude + rnd;
			
			randomRotation = Quaternion.Euler(rotationRandom * UnityEngine.Random.value);

			foreach (RecoilOffset offset in offsets) {
				offset.Start();
			}

			if (Time.time < endTime) blendWeight = 0f;
			else blendWeight = 1f;

			Keyframe[] keys = recoilWeight.keys;
			length = keys[keys.Length - 1].time;
			endTime = Time.time + length;
		}

		protected override void OnModifyOffset() {
			if (aimIK != null) aimIKAxis = aimIK.solver.axis;

			if (Time.time >= endTime) {
				rotationOffset = Quaternion.identity;
				return;
			}

			if (!initiated && ik != null) {
				initiated = true;
				ik.solver.OnPostUpdate += AfterFBBIK;
				if (aimIK != null) aimIK.solver.OnPostUpdate += AfterAimIK;
			}

			blendTime = Mathf.Max(blendTime, 0f);
			if (blendTime > 0f) blendWeight = Mathf.Min(blendWeight + Time.deltaTime * (1f / blendTime), 1f);
			else blendWeight = 1f;

			// Current weight of offset
			float wTarget = recoilWeight.Evaluate(length - (endTime - Time.time)) * magnitudeMlp;
			w = Mathf.Lerp(w, wTarget, blendWeight);

			// Find the rotation space of the recoil
			Quaternion lookRotation = aimIK != null && aimIK.solver.transform != null && !aimIKSolvedLast? Quaternion.LookRotation(aimIK.solver.IKPosition - aimIK.solver.transform.position, ik.references.root.up): ik.references.root.rotation;
			lookRotation = randomRotation * lookRotation;

			// Apply FBBIK effector positionOffsets
			foreach (RecoilOffset offset in offsets) {
				offset.Apply(ik.solver, lookRotation, w, length, endTime - Time.time);
			}

			if (!handRotationsSet) {
				primaryHandRotation = primaryHand.rotation;
				//if (twoHanded) secondaryHandRotation = secondaryHand.rotation;
			}
			handRotationsSet = false;

			// Rotation offset of the primary hand
			rotationOffset = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(randomRotation * primaryHandRotation * handRotationOffset), w);
			handRotation = rotationOffset * primaryHandRotation;
				
			// Fix the secondary hand relative to the primary hand
			if (twoHanded) {
				Vector3 secondaryHandRelativePosition = Quaternion.Inverse(primaryHand.rotation) * (secondaryHand.position - primaryHand.position);
				secondaryHandRelativeRotation = Quaternion.Inverse(primaryHand.rotation) * secondaryHand.rotation;

				Vector3 primaryHandPosition = primaryHand.position + primaryHandEffector.positionOffset;
				Vector3 secondaryHandPosition = primaryHandPosition + handRotation * secondaryHandRelativePosition;

				secondaryHandEffector.positionOffset += secondaryHandPosition - (secondaryHand.position + secondaryHandEffector.positionOffset);
			}

			if (aimIK != null && aimIKSolvedLast) aimIK.solver.axis = Quaternion.Inverse(ik.references.root.rotation) * Quaternion.Inverse(rotationOffset) * aimIKAxis;
		}

		private void AfterFBBIK() {
			if (Time.time >= endTime) return;

			// Rotate the hand bones
			primaryHand.rotation = handRotation;
			if (twoHanded) secondaryHand.rotation = primaryHand.rotation * secondaryHandRelativeRotation;
		}

		private void AfterAimIK() {
			if (aimIKSolvedLast) aimIK.solver.axis = aimIKAxis;
		}

		// Shortcuts
		private IKEffector primaryHandEffector {
			get {
				if (handedness == Handedness.Right) return ik.solver.rightHandEffector;
				return ik.solver.leftHandEffector;
			}
		}
		
		private IKEffector secondaryHandEffector {
			get {
				if (handedness == Handedness.Right) return ik.solver.leftHandEffector;
				return ik.solver.rightHandEffector;
			}
		}
		
		private Transform primaryHand {
			get {
				return primaryHandEffector.bone;
			}
		}
		
		private Transform secondaryHand {
			get {
				return secondaryHandEffector.bone;
			}
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			if (ik != null && initiated) {
				ik.solver.OnPostUpdate -= AfterFBBIK;
				if (aimIK != null) aimIK.solver.OnPostUpdate -= AfterAimIK;
			}
		}
		
	}
}
