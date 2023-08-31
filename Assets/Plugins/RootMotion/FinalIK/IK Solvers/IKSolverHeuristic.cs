using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Contains methods common for all heuristic solvers.
	/// </summary>
	[System.Serializable]
	public class IKSolverHeuristic: IKSolver {
		
		#region Main Interface

		/// <summary>
		/// The target Transform. Solver IKPosition will be automatically set to the position of the target.
		/// </summary>
		public Transform target;
		/// <summary>
		/// Minimum distance from last reached position. Will stop solving if difference from previous reached position is less than tolerance. If tolerance is zero, will iterate until maxIterations.
		/// </summary>
		public float tolerance = 0f;
		/// <summary>
		/// Max iterations per frame
		/// </summary>
		public int maxIterations = 4;
		/// <summary>
		/// If true, rotation limits (if existing) will be applied on each iteration.
		/// </summary>
		public bool useRotationLimits = true;
		/// <summary>
		/// Solve in 2D?
		/// </summary>
		public bool XY;
		/// <summary>
		/// The hierarchy of bones.
		/// </summary>
		public Bone[] bones = new Bone[0];
		
		/// <summary>
		/// Rebuild the bone hierarcy and reinitiate the solver.
		/// </summary>
		/// <returns>
		/// Returns true if the new chain is valid.
		/// </returns>
		public bool SetChain(Transform[] hierarchy, Transform root) {
			if (bones == null || bones.Length != hierarchy.Length) bones = new Bone[hierarchy.Length];
			for (int i = 0; i < hierarchy.Length; i++) {
				if (bones[i] == null) bones[i] = new IKSolver.Bone();
				bones[i].transform = hierarchy[i];
			}
			
			Initiate(root);
			return initiated;
		}

		/// <summary>
		/// Adds a bone to the chain.
		/// </summary>
		public void AddBone(Transform bone) {
			Transform[] newBones = new Transform[bones.Length + 1];
			
			for (int i = 0; i < bones.Length; i++) {
				newBones[i] = bones[i].transform;
			}
			
			newBones[newBones.Length - 1] = bone;
			
			SetChain(newBones, root);
		}
		
		public override void StoreDefaultLocalState() {
			for (int i = 0; i < bones.Length; i++) bones[i].StoreDefaultLocalState();
		}

		public override void FixTransforms() {
			if (!initiated) return;
			if (IKPositionWeight <= 0f) return;

			for (int i = 0; i < bones.Length; i++) bones[i].FixTransform();
		}
		
		public override bool IsValid(ref string message) {
			if (bones.Length == 0) {
				message = "IK chain has no Bones.";
				return false;
			}
			if (bones.Length < minBones) {
				message = "IK chain has less than " + minBones + " Bones.";
				return false;
			}
			foreach (Bone bone in bones) {
				if (bone.transform == null) {
					message = "One of the Bones is null.";
					return false;
				}
			}

			Transform duplicate = ContainsDuplicateBone(bones);
			if (duplicate != null) {
				message = duplicate.name + " is represented multiple times in the Bones.";
				return false;
			}

			if (!allowCommonParent && !HierarchyIsValid(bones)) {
				message = "Invalid bone hierarchy detected. IK requires for it's bones to be parented to each other in descending order.";
				return false;
			}
			
			if (!boneLengthCanBeZero) {
				for (int i = 0; i < bones.Length - 1; i++) {
					float l = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
					if (l == 0) {
						message = "Bone " + i + " length is zero.";
						return false;
					}
				}
			}
			return true;
		}
		
		public override IKSolver.Point[] GetPoints() {
			return bones as IKSolver.Point[];
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			for (int i = 0; i < bones.Length; i++) if (bones[i].transform == transform) return bones[i] as IKSolver.Point;
			return null;
		}
		
		#endregion Main Interface
		
		protected virtual int minBones { get { return 2; }}
		protected virtual bool boneLengthCanBeZero { get { return true; }}
		protected virtual bool allowCommonParent { get { return false; }}
		protected override void OnInitiate() {}
		protected override void OnUpdate() {}
		protected Vector3 lastLocalDirection;
		protected float chainLength;
		
		/*
		 * Initiates all bones to match their current state
		 * */
		protected void InitiateBones() {
			chainLength = 0;
			
			for (int i = 0; i < bones.Length; i++) {
				// Find out which local axis is directed at child/target position
				if (i < bones.Length - 1) {
					bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
					chainLength += bones[i].length;
					
					Vector3 nextPosition = bones[i + 1].transform.position;
					bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (nextPosition - bones[i].transform.position);
						
					// Disable Rotation Limits from updating to take control of their execution order
					if (bones[i].rotationLimit != null) {
						if (XY) {
							if (bones[i].rotationLimit is RotationLimitHinge) {
							} else Warning.Log("Only Hinge Rotation Limits should be used on 2D IK solvers.", bones[i].transform);
						}
						bones[i].rotationLimit.Disable();
					}
				} else {
					bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[bones.Length - 1].transform.position - bones[0].transform.position);
				}
			}
		}
		
		#region Optimizations
		
		/*
		 * Gets the direction from last bone to first bone in first bone's local space.
		 * */
		protected virtual Vector3 localDirection {
			get {
				return bones[0].transform.InverseTransformDirection(bones[bones.Length - 1].transform.position - bones[0].transform.position);
			}
		}
		
		/*
		 * Gets the offset from last position of the last bone to its current position.
		 * */
		protected float positionOffset {
			get {
				return Vector3.SqrMagnitude(localDirection - lastLocalDirection);
			}
		}
		
		#endregion Optimizations
		
		/*
		 * Get target offset to break out of the linear singularity issue
		 * */
		protected Vector3 GetSingularityOffset() {
			if (!SingularityDetected()) return Vector3.zero;
			
			Vector3 IKDirection = (IKPosition - bones[0].transform.position).normalized;
			
			Vector3 secondaryDirection = new Vector3(IKDirection.y, IKDirection.z, IKDirection.x);
			
			// Avoiding getting locked by the Hinge Rotation Limit
			if (useRotationLimits && bones[bones.Length - 2].rotationLimit != null && bones[bones.Length - 2].rotationLimit is RotationLimitHinge) {
				secondaryDirection = bones[bones.Length - 2].transform.rotation * bones[bones.Length - 2].rotationLimit.axis;
			}
			
			return Vector3.Cross(IKDirection, secondaryDirection) * bones[bones.Length - 2].length * 0.5f;
		}
		
		/*
		 * Detects linear singularity issue when the direction from first bone to IKPosition matches the direction from first bone to the last bone.
		 * */
		private bool SingularityDetected() {
			if (!initiated) return false;
			
			Vector3 toLastBone = bones[bones.Length - 1].transform.position - bones[0].transform.position;
			Vector3 toIKPosition = IKPosition - bones[0].transform.position;
			
			float toLastBoneDistance = toLastBone.magnitude;
			float toIKPositionDistance = toIKPosition.magnitude;

			if (toLastBoneDistance < toIKPositionDistance) return false;
			if (toLastBoneDistance < chainLength - (bones[bones.Length - 2].length * 0.1f)) return false;
			if (toLastBoneDistance == 0) return false;
			if (toIKPositionDistance == 0) return false;
			if (toIKPositionDistance > toLastBoneDistance) return false;
			
			float dot = Vector3.Dot(toLastBone / toLastBoneDistance, toIKPosition / toIKPositionDistance);
			if (dot < 0.999f) return false;

			return true;
		}
		
	}
}
