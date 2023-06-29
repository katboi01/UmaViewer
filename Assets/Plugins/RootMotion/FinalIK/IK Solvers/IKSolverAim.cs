using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Rotates a hierarchy of bones to make a Transform aim at a target.
	/// If there are problems with continuity and the solver get's jumpy, make sure to keep IKPosition at a safe distance from the transform and try decreasing solver and bone weights.
	/// </summary>
	[System.Serializable]
	public class IKSolverAim : IKSolverHeuristic {
		
		#region Main Interface

		/// <summary>
		/// The transform that we want to aim at IKPosition.
		/// </summary>
		public Transform transform;
		/// <summary>
		/// The local axis of the Transform that you want to be aimed at IKPosition.
		/// </summary>
		public Vector3 axis = Vector3.forward;
		/// <summary>
		/// Keeps that axis of the Aim Transform directed at the polePosition.
		/// </summary>
		public Vector3 poleAxis = Vector3.up;
		/// <summary>
		/// The position in world space to keep the pole axis of the Aim Transform directed at.
		/// </summary>
		public Vector3 polePosition;
		/// <summary>
		/// The weight of the Pole.
		/// </summary>
		[Range(0f, 1f)]
		public float poleWeight;
		/// <summary>
		/// If assigned, will automatically set polePosition to the position of this Transform.
		/// </summary>
		public Transform poleTarget;
		/// <summary>
		/// Clamping rotation of the solver. 0 is free rotation, 1 is completely clamped to transform axis.
		/// </summary>
		[Range(0f, 1f)]
		public float clampWeight = 0.1f;
		/// <summary>
		/// Number of sine smoothing iterations applied to clamping to make it smoother.
		/// </summary>
		[Range(0, 2)]
		public int clampSmoothing = 2;

		/// <summary>
		/// Gets the angular offset.
		/// </summary>
		public float GetAngle() {
			return Vector3.Angle(transformAxis, IKPosition - transform.position);
		}

		/// <summary>
		/// Gets the Axis of the AimTransform is world space.
		/// </summary>
		public Vector3 transformAxis {
			get {
				return transform.rotation * axis;
			}
		}

		/// <summary>
		/// Gets the Pole Axis of the AimTransform is world space.
		/// </summary>
		public Vector3 transformPoleAxis {
			get {
				return transform.rotation * poleAxis;
			}
		}

		/// <summary>
		/// Called before each iteration of the solver.
		/// </summary>
		public IterationDelegate OnPreIteration;

		#endregion Main Interface
		
		protected override void OnInitiate() {
			if ((firstInitiation || !Application.isPlaying) && transform != null) {
				IKPosition = transform.position + transformAxis * 3f;
				polePosition = transform.position + transformPoleAxis * 3f;
			}
			
			// Disable Rotation Limits from updating to take control of their execution order
			for (int i = 0; i < bones.Length; i++) {
				if (bones[i].rotationLimit != null) bones[i].rotationLimit.Disable();
			}

			step = 1f / (float)bones.Length;
			if (Application.isPlaying) axis = axis.normalized;
		}
		
		protected override void OnUpdate() {
			if (axis == Vector3.zero) {
				if (!Warning.logged) LogWarning("IKSolverAim axis is Vector3.zero.");
				return;
			}

			if (poleAxis == Vector3.zero && poleWeight > 0f) {
				if (!Warning.logged) LogWarning("IKSolverAim poleAxis is Vector3.zero.");
				return;
			}

			if (target != null) IKPosition = target.position;
			if (poleTarget != null) polePosition = poleTarget.position;

			if (XY) IKPosition.z = bones[0].transform.position.z;
			
			// Clamping weights
			if (IKPositionWeight <= 0) return;
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);

			// Rotation Limit on the Aim Transform
			if (transform != lastTransform) {
				transformLimit = transform.GetComponent<RotationLimit>();
				if (transformLimit != null) transformLimit.enabled = false;
				lastTransform = transform;
			}

			if (transformLimit != null) transformLimit.Apply();
			
			// In case transform becomes unassigned in runtime
			if (transform == null) {
				if (!Warning.logged) LogWarning("Aim Transform unassigned in Aim IK solver. Please Assign a Transform (lineal descendant to the last bone in the spine) that you want to be aimed at IKPosition");
				return;
			}
			
			clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
			clampedIKPosition = GetClampedIKPosition();

			Vector3 dir = clampedIKPosition - transform.position;
			dir = Vector3.Slerp(transformAxis * dir.magnitude, dir, IKPositionWeight);
			clampedIKPosition = transform.position + dir;

			// Iterating the solver
			for (int i = 0; i < maxIterations; i++) {
				
				// Optimizations
				if (i >= 1 && tolerance > 0 && GetAngle() < tolerance) break;
				lastLocalDirection = localDirection;

				if (OnPreIteration != null) OnPreIteration(i);
				
				Solve();
			}
			
			lastLocalDirection = localDirection;
		}
		
		protected override int minBones { get { return 1; }}
		
		private float step;
		private Vector3 clampedIKPosition;
		private RotationLimit transformLimit;
		private Transform lastTransform;

        /*
		 * Solving the hierarchy
		 * */
        private void Solve() {
			// Rotating bones to get closer to target.
			for (int i = 0; i < bones.Length - 1; i++) RotateToTarget(clampedIKPosition, bones[i], step * (i + 1) * IKPositionWeight * bones[i].weight);
			RotateToTarget(clampedIKPosition, bones[bones.Length - 1], IKPositionWeight * bones[bones.Length - 1].weight);
		}
		
		/*
		 * Clamping the IKPosition to legal range
		 * */
		private Vector3 GetClampedIKPosition() {
			if (clampWeight <= 0f) return IKPosition;
			if (clampWeight >= 1f) return transform.position + transformAxis * (IKPosition - transform.position).magnitude;
			
			// Getting the dot product of IK direction and transformAxis
			//float dot = (Vector3.Dot(transformAxis, (IKPosition - transform.position).normalized) + 1) * 0.5f;
			float angle = Vector3.Angle(transformAxis, (IKPosition - transform.position));
			float dot = 1f - (angle / 180f);

			// Clamping the target
			float targetClampMlp = clampWeight > 0? Mathf.Clamp(1f - ((clampWeight - dot) / (1f - dot)), 0f, 1f): 1f;
			
			// Calculating the clamp multiplier
			float clampMlp = clampWeight > 0? Mathf.Clamp(dot / clampWeight, 0f, 1f): 1f;

			for (int i = 0; i < clampSmoothing; i++) {
				float sinF = clampMlp * Mathf.PI * 0.5f;
				clampMlp = Mathf.Sin(sinF);
			}

			// Slerping the IK direction (don't use Lerp here, it breaks it)
			return transform.position + Vector3.Slerp(transformAxis * 10f, IKPosition - transform.position, clampMlp * targetClampMlp);
		}
		
		/*
		 * Rotating bone to get transform aim closer to target
		 * */
		private void RotateToTarget(Vector3 targetPosition, IKSolver.Bone bone, float weight) {
			// Swing
			if (XY) {
				/*
				if (weight >= 0f) {
					Vector3 dir = transformAxis;
					Vector3 targetDir = targetPosition - transform.position;

					float angleDir = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
					float angleTarget = Mathf.Atan2(targetDir.x, targetDir.y) * Mathf.Rad2Deg;

					bone.transform.rotation = Quaternion.AngleAxis((angleTarget - angleDir) * weight, Vector3.back) * bone.transform.rotation;
				}
				*/

				if (weight >= 0f) {
					Vector3 dir = transformAxis;
					Vector3 targetDir = targetPosition - transform.position;
					
					float angleDir = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
					float angleTarget = Mathf.Atan2(targetDir.x, targetDir.y) * Mathf.Rad2Deg;

					bone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(angleDir, angleTarget), Vector3.back) * bone.transform.rotation;
				}
			} else {
				if (weight >= 0f) {
					Quaternion rotationOffset = Quaternion.FromToRotation(transformAxis, targetPosition - transform.position);

					if (weight >= 1f) {
						bone.transform.rotation = rotationOffset * bone.transform.rotation;
					} else {
						bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, rotationOffset, weight) * bone.transform.rotation;
					}
				}

				// Pole
				if (poleWeight > 0f) {
					Vector3 poleDirection = polePosition - transform.position;

					// Ortho-normalize to transform axis to make this a twisting only operation
					Vector3 poleDirOrtho = poleDirection;
					Vector3 normal = transformAxis;
					Vector3.OrthoNormalize(ref normal, ref poleDirOrtho);

					Quaternion toPole = Quaternion.FromToRotation(transformPoleAxis, poleDirOrtho);
					bone.transform.rotation = Quaternion.Lerp(Quaternion.identity, toPole, weight * poleWeight) * bone.transform.rotation;
				}
			}

			if (useRotationLimits && bone.rotationLimit != null) bone.rotationLimit.Apply();
		}
		
		/*
		 * Gets the direction from last bone's forward in first bone's local space.
		 * */
		protected override Vector3 localDirection {
			get {
				return bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.forward);
			}
		}
	}
}
