using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	public class LookAtController : MonoBehaviour {

		public LookAtIK ik;

		[Header("Target Smoothing")]

		[Tooltip("The target to look at. Do not use the Target transform that is assigned to LookAtIK. Set to null if you wish to stop looking.")]
		public Transform target;

		[Range(0f, 1f)] public float weight = 1f;

		public Vector3 offset;

		[Tooltip("The time it takes to switch targets.")]
		public float targetSwitchSmoothTime = 0.3f;

		[Tooltip("The time it takes to blend in/out of LookAtIK weight.")]
		public float weightSmoothTime = 0.3f;

		[Header("Turning Towards The Target")]

		[Tooltip("Enables smooth turning towards the target according to the parameters under this header.")]
		public bool smoothTurnTowardsTarget = true;

		[Tooltip("Speed of turning towards the target using Vector3.RotateTowards.")]
		public float maxRadiansDelta = 3f;

		[Tooltip("Speed of moving towards the target using Vector3.RotateTowards.")]
		public float maxMagnitudeDelta = 3f;

		[Tooltip("Speed of slerping towards the target.")]
		public float slerpSpeed = 3f;

		[Tooltip("The position of the pivot that the look at target is rotated around relative to the root of the character.")] 
		public Vector3 pivotOffsetFromRoot = Vector3.up;

		[Tooltip("Minimum distance of looking from the first bone. Keeps the solver from failing if the target is too close.")] 
		public float minDistance = 1f;

		[Header("RootRotation")]
		[Tooltip("Character root will be rotate around the Y axis to keep root forward within this angle from the look direction.")]
		[Range(0f, 180f)]
		public float maxRootAngle = 45f;

		private Transform lastTarget;
		private float switchWeight, switchWeightV;
		private float weightV;
		private Vector3 lastPosition;
		private Vector3 dir;
		private bool lastSmoothTowardsTarget;

		void Start() {
			lastPosition = ik.solver.IKPosition;
			dir = ik.solver.IKPosition - pivot;
		}

		void LateUpdate () {
			// If target has changed...
			if (target != lastTarget) {
                if (lastTarget == null && target != null && ik.solver.IKPositionWeight <= 0f) { 
                    lastPosition = target.position;
					dir = target.position - pivot;
					ik.solver.IKPosition = target.position + offset;
				} else {
					lastPosition = ik.solver.IKPosition;
					dir = ik.solver.IKPosition - pivot;
				}

				switchWeight = 0f;
				lastTarget = target;
			}

			// Smooth weight
			ik.solver.IKPositionWeight = Mathf.SmoothDamp(ik.solver.IKPositionWeight, (target != null? weight: 0f), ref weightV, weightSmoothTime);
			if (ik.solver.IKPositionWeight >= 0.999f) ik.solver.IKPositionWeight = 1f;
			if (ik.solver.IKPositionWeight <= 0.001f) ik.solver.IKPositionWeight = 0f;

			if (ik.solver.IKPositionWeight <= 0f) return;

			// Smooth target switching
			switchWeight = Mathf.SmoothDamp(switchWeight, 1f, ref switchWeightV, targetSwitchSmoothTime);
			if (switchWeight >= 0.999f) switchWeight = 1f;

			if (target != null) {
				ik.solver.IKPosition = Vector3.Lerp(lastPosition, target.position + offset, switchWeight);
			}

			// Smooth turn towards target
			if (smoothTurnTowardsTarget != lastSmoothTowardsTarget) {
				dir = ik.solver.IKPosition - pivot;
				lastSmoothTowardsTarget = smoothTurnTowardsTarget;
			}

			if (smoothTurnTowardsTarget) {
				Vector3 targetDir = ik.solver.IKPosition - pivot;
				dir = Vector3.Slerp(dir, targetDir, Time.deltaTime * slerpSpeed);
				dir = Vector3.RotateTowards(dir, targetDir, Time.deltaTime * maxRadiansDelta, maxMagnitudeDelta);
				ik.solver.IKPosition = pivot + dir;
			}

			// Min distance from the pivot
			ApplyMinDistance();

			// Root rotation
			RootRotation();
		}

		// Pivot of rotating the aiming direction.
		private Vector3 pivot {
			get {
				return ik.transform.position + ik.transform.rotation * pivotOffsetFromRoot;
			}
		}

		// Make sure aiming target is not too close (might make the solver instable when the target is closer to the first bone than the last bone is).
		void ApplyMinDistance() {
			Vector3 aimFrom = pivot;
			Vector3 direction = (ik.solver.IKPosition - aimFrom);
			direction = direction.normalized * Mathf.Max(direction.magnitude, minDistance);
				
			ik.solver.IKPosition = aimFrom + direction;
		}

		// Character root will be rotate around the Y axis to keep root forward within this angle from the looking direction.
		private void RootRotation() {
			float max = Mathf.Lerp(180f, maxRootAngle, ik.solver.IKPositionWeight);

			if (max < 180f) {
				Vector3 faceDirLocal = Quaternion.Inverse(ik.transform.rotation) * (ik.solver.IKPosition - pivot);
				float angle = Mathf.Atan2(faceDirLocal.x, faceDirLocal.z) * Mathf.Rad2Deg;

				float rotation = 0f;

				if (angle > max) {
					rotation = angle - max;
				}
				if (angle < -max) {
					rotation = angle + max;
				}

				ik.transform.rotation = Quaternion.AngleAxis(rotation, ik.transform.up) * ik.transform.rotation;		
			}
		}
	}
}
