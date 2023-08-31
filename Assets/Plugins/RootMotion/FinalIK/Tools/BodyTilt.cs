using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Procedural body tilting with FBBIK.
	/// </summary>
	public class BodyTilt: OffsetModifier {

		[Tooltip("Speed of tilting")]
		public float tiltSpeed = 6f;
		[Tooltip("Sensitivity of tilting")]
		public float tiltSensitivity = 0.07f;
		[Tooltip("The OffsetPose components")]
		public OffsetPose poseLeft, poseRight;

		private float tiltAngle;
		private Vector3 lastForward;

		protected override void Start() {
			base.Start();

			// Store current character forward axis and Time
			lastForward = transform.forward;
		}

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			// Calculate the angular delta in character rotation
			Quaternion change = Quaternion.FromToRotation(lastForward, transform.forward);
			float deltaAngle = 0;
			Vector3 axis = Vector3.zero;
			change.ToAngleAxis(out deltaAngle, out axis);
			if (axis.y > 0) deltaAngle = -deltaAngle;

			deltaAngle *= tiltSensitivity * 0.01f;
			deltaAngle /= deltaTime;
			deltaAngle = Mathf.Clamp(deltaAngle, -1f, 1f);

			tiltAngle = Mathf.Lerp(tiltAngle, deltaAngle, deltaTime * tiltSpeed);

			// Applying positionOffsets
			float tiltF = Mathf.Abs(tiltAngle) / 1f;
			if (tiltAngle < 0) poseRight.Apply(ik.solver, tiltF);
			else poseLeft.Apply(ik.solver, tiltF);

			// Store current character forward axis and Time
			lastForward = transform.forward;
		}
	}
}
