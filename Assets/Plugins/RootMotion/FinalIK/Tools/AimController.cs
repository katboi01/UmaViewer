using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Handles smooth aim target switching, weight blending, target interpolation and root rotation.
	/// </summary>
	public class AimController : MonoBehaviour {

		[Tooltip("Reference to the AimIK component.")]
		/// <summary>
		/// Reference to the AimIK component.
		/// </summary>
		public AimIK ik;

		[Tooltip("Master weight of the IK solver.")]
		/// <summary>
		/// Master weight of the IK solver.
		/// </summary>
		[Range(0f, 1f)] public float weight = 1f;

		[Header("Target Smoothing")]

		[Tooltip("The target to aim at. Do not use the Target transform that is assigned to AimIK. Set to null if you wish to stop aiming.")]
		/// <summary>
		/// The target to aim at. Do not use the Target transform that is assigned to AimIK. Set to null if you wish to stop aiming.
		/// </summary>
		public Transform target;

		[Tooltip("The time it takes to switch targets.")]
		/// <summary>
		/// The time it takes to switch targets.
		/// </summary>
		public float targetSwitchSmoothTime = 0.3f;

		[Tooltip("The time it takes to blend in/out of AimIK weight.")]
		/// <summary>
		/// The time it takes to blend in/out of AimIK weight.
		/// </summary>
		public float weightSmoothTime = 0.3f;

		[Header("Turning Towards The Target")]

		[Tooltip("Enables smooth turning towards the target according to the parameters under this header.")]
		/// <summary>
		/// Enables smooth turning towards the target according to the parameters under this header.
		/// </summary>
		public bool smoothTurnTowardsTarget = true;

		[Tooltip("Speed of turning towards the target using Vector3.RotateTowards.")]
		/// <summary>
		/// Speed of turning towards the target using Vector3.RotateTowards.
		/// </summary>
		public float maxRadiansDelta = 3f;

		[Tooltip("Speed of moving towards the target using Vector3.RotateTowards.")]
		/// <summary>
		/// Speed of moving towards the target using Vector3.RotateTowards.
		/// </summary>
		public float maxMagnitudeDelta = 3f;

		[Tooltip("Speed of slerping towards the target.")]
		/// <summary>
		/// Speed of slerping towards the target.
		/// </summary>
		public float slerpSpeed = 3f;

		[Tooltip("The position of the pivot that the aim target is rotated around relative to the root of the character.")]
		/// <summary>
		/// The position of the pivot that the aim target is rotated around relative to the root of the character.
		/// </summary>
		public Vector3 pivotOffsetFromRoot = Vector3.up;

		[Tooltip("Minimum distance of aiming from the first bone. Keeps the solver from failing if the target is too close.")]
		/// <summary>
		/// Minimum distance of aiming from the first bone. Keeps the solver from failing if the target is too close.
		/// </summary>
		public float minDistance = 1f;

		[Tooltip("Offset applied to the target in world space. Convenient for scripting aiming inaccuracy.")]
		/// <summary>
		/// Offset applied to the target in world space. Convenient for scripting aiming inaccuracy.
		/// </summary>
		public Vector3 offset;

		[Header("RootRotation")]

		[Tooltip("Character root will be rotate around the Y axis to keep root forward within this angle from the aiming direction.")]
		/// <summary>
		///Character root will be rotate around the Y axis to keep root forward within this angle from the aiming direction.
		/// </summary>
		[Range(0f, 180f)] public float maxRootAngle = 45f;

        [Tooltip("If enabled, aligns the root forward to target direction after 'Max Root Angle' has been exceeded.")]
        /// <summary>
        /// If enabled, aligns the root forward to target direction after 'Max Root Angle' has been exceeded.
        /// </summary>
        public bool turnToTarget;

        [Tooltip("The time of turning towards the target direction if 'Max Root Angle has been exceeded and 'Turn To Target' is enabled.")]
        /// <summary>
        /// The time of turning towards the target direction if 'Max Root Angle has been exceeded and 'Turn To Target' is enabled.
        /// </summary>
        public float turnToTargetTime = 0.2f;

        [Header("Mode")]

		[Tooltip("If true, AimIK will consider whatever the current direction of the weapon to be the forward aiming direction and work additively on top of that. This enables you to use recoil and reloading animations seamlessly with AimIK. Adjust the Vector3 value below if the weapon is not aiming perfectly forward in the aiming animation clip.")]
		/// <summary>
		/// If true, AimIK will consider whatever the current direction of the weapon to be the forward aiming direction and work additively on top of that. This enables you to use recoil and reloading animations seamlessly with AimIK. Adjust the Vector3 value below if the weapon is not aiming perfectly forward in the aiming animation clip.
		/// </summary>
		public bool useAnimatedAimDirection;

		[Tooltip("The direction of the animated weapon aiming in character space. Tweak this value to adjust the aiming. 'Use Animated Aim Direction' must be enabled for this property to work.")]
		/// <summary>
		/// The direction of the animated weapon aiming in character space. Tweak this value to adjust the aiming. 'Use Animated Aim Direction' must be enabled for this property to work.
		/// </summary>
		public Vector3 animatedAimDirection = Vector3.forward;

		private Transform lastTarget;
		private float switchWeight, switchWeightV;
		private float weightV;
		private Vector3 lastPosition;
		private Vector3 dir;
		private bool lastSmoothTowardsTarget;
        private bool turningToTarget;
        private float turnToTargetMlp = 1f;
        private float turnToTargetMlpV;

        void Start() {
			lastPosition = ik.solver.IKPosition;
			dir = ik.solver.IKPosition - pivot;

			ik.solver.target = null;
		}

		void LateUpdate () {
			// If target has changed...
			if (target != lastTarget) {
                if (lastTarget == null && target != null && ik.solver.IKPositionWeight <= 0f)
                {
                    lastPosition = target.position;
                    dir = target.position - pivot;
                    ik.solver.IKPosition = target.position + offset;
                }
                else
                {
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

			// Offset mode
			if (useAnimatedAimDirection) {
				ik.solver.axis = ik.solver.transform.InverseTransformVector(ik.transform.rotation * animatedAimDirection);
			}
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

		// Character root will be rotate around the Y axis to keep root forward within this angle from the aiming direction.
		private void RootRotation() {
            float max = Mathf.Lerp(180f, maxRootAngle * turnToTargetMlp, ik.solver.IKPositionWeight);

			if (max < 180f) {
				Vector3 faceDirLocal = Quaternion.Inverse(ik.transform.rotation) * (ik.solver.IKPosition - pivot);
				float angle = Mathf.Atan2(faceDirLocal.x, faceDirLocal.z) * Mathf.Rad2Deg;

				float rotation = 0f;

				if (angle > max) {
					rotation = angle - max;
                    if (!turningToTarget && turnToTarget) StartCoroutine(TurnToTarget());
				}
				if (angle < -max) {
					rotation = angle + max;
                    if (!turningToTarget && turnToTarget) StartCoroutine(TurnToTarget());
                }

				ik.transform.rotation = Quaternion.AngleAxis(rotation, ik.transform.up) * ik.transform.rotation;		
			}
		}

        // Aligns the root forward to target direction after "Max Root Angle" has been exceeded.
        private IEnumerator TurnToTarget()
        {
            turningToTarget = true;
            
            while (turnToTargetMlp > 0f)
            {
                turnToTargetMlp = Mathf.SmoothDamp(turnToTargetMlp, 0f, ref turnToTargetMlpV, turnToTargetTime);
                if (turnToTargetMlp < 0.01f) turnToTargetMlp = 0f;
                
                 yield return null;
            }

            turnToTargetMlp = 1f;
            turningToTarget = false;
        }
	}
}
