using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Analytic %IK solver based on the Law of Cosines.
	/// </summary>
	[System.Serializable]
	public class IKSolverTrigonometric: IKSolver {
		
		#region Main Interface

		/// <summary>
		/// The target Transform.
		/// </summary>
		public Transform target;
		/// <summary>
		/// The %IK rotation weight (rotation of the last bone).
		/// </summary>
		[Range(0f, 1f)]
		public float IKRotationWeight = 1f;
		/// <summary>
		/// The %IK rotation target.
		/// </summary>
		public Quaternion IKRotation = Quaternion.identity;
		/// <summary>
		/// The bend plane normal.
		/// </summary>
		public Vector3 bendNormal = Vector3.right;
		/// <summary>
		/// The first bone (upper arm or thigh).
		/// </summary>
		public TrigonometricBone bone1 = new TrigonometricBone();
		/// <summary>
		/// The second bone (forearm or calf).
		/// </summary>
		public TrigonometricBone bone2 = new TrigonometricBone();
		/// <summary>
		/// The third bone (hand or foot).
		/// </summary>
		public TrigonometricBone bone3 = new TrigonometricBone();	
		
		/// <summary>
		/// Sets the bend goal position.
		/// </summary>
		/// <param name='goalPosition'>
		/// Goal position.
		/// </param>
		public void SetBendGoalPosition(Vector3 goalPosition, float weight) {
			if (!initiated) return;
			if (weight <= 0f) return;
			
			Vector3 normal = Vector3.Cross(goalPosition - bone1.transform.position, IKPosition - bone1.transform.position);
			if (normal != Vector3.zero) {
				if (weight >= 1f) {
					bendNormal = normal;
					return;
				}

				bendNormal = Vector3.Lerp(bendNormal, normal, weight);
			}
		}
		
		/// <summary>
		/// Sets the bend plane to match current bone rotations.
		/// </summary>
		public void SetBendPlaneToCurrent() {
			if (!initiated) return;
			
			Vector3 normal = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
			if (normal != Vector3.zero) bendNormal = normal;
		}
		
		/// <summary>
		/// Sets the %IK rotation.
		/// </summary>
		public void SetIKRotation(Quaternion rotation) {
			IKRotation = rotation;
		}
		
		/// <summary>
		/// Sets the %IK rotation weight.
		/// </summary>
		public void SetIKRotationWeight(float weight) {
			IKRotationWeight = Mathf.Clamp(weight, 0f, 1f);
		}
		
		/// <summary>
		/// Gets the %IK rotation.
		/// </summary>
		public Quaternion GetIKRotation() {
			return IKRotation;
		}
		
		/// <summary>
		/// Gets the %IK rotation weight.
		/// </summary>
		public float GetIKRotationWeight() {
			return IKRotationWeight;
		}
		
		public override IKSolver.Point[] GetPoints() {
			return new IKSolver.Point[3] { (IKSolver.Point)bone1, (IKSolver.Point)bone2, (IKSolver.Point)bone3 };
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			if (bone1.transform == transform) return (IKSolver.Point)bone1;
			if (bone2.transform == transform) return (IKSolver.Point)bone2;
			if (bone3.transform == transform) return (IKSolver.Point)bone3;
			return null;
		}

		public override void StoreDefaultLocalState() {
			bone1.StoreDefaultLocalState();
			bone2.StoreDefaultLocalState();
			bone3.StoreDefaultLocalState();
		}
		
		public override void FixTransforms() {
			if (!initiated) return;

			bone1.FixTransform();
			bone2.FixTransform();
			bone3.FixTransform();
		}
		
		public override bool IsValid(ref string message) {
			if (bone1.transform == null || bone2.transform == null || bone3.transform == null) {
				message = "Please assign all Bones to the IK solver.";
				return false;
			}

			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(new Transform[3] { bone1.transform, bone2.transform, bone3.transform });
			if (duplicate != null) {
				message = duplicate.name + " is represented multiple times in the Bones.";
				return false;
			}

			if (bone1.transform.position == bone2.transform.position) {
				message = "first bone position is the same as second bone position.";
				return false;
			}
			if (bone2.transform.position == bone3.transform.position) {
				message = "second bone position is the same as third bone position.";
				return false;
			}

			return true;
		}
		
		/// <summary>
		/// Bone type used by IKSolverTrigonometric.
		/// </summary>
		[System.Serializable]
		public class TrigonometricBone: IKSolver.Bone {
			
			private Quaternion targetToLocalSpace;
			private Vector3 defaultLocalBendNormal;
			
			#region Public methods
			
			/*
			 * Initiates the bone, precalculates values.
			 * */
			public void Initiate(Vector3 childPosition, Vector3 bendNormal) {
				// Get default target rotation that looks at child position with bendNormal as up
				Quaternion defaultTargetRotation = Quaternion.LookRotation(childPosition - transform.position, bendNormal);
				
				// Covert default target rotation to local space
				targetToLocalSpace = QuaTools.RotationToLocalSpace(transform.rotation, defaultTargetRotation);
				
				defaultLocalBendNormal = Quaternion.Inverse(transform.rotation) * bendNormal;
			}
			
			/*
			 * Calculates the rotation of this bone to targetPosition.
			 * */
			public Quaternion GetRotation(Vector3 direction, Vector3 bendNormal) {
				return Quaternion.LookRotation(direction, bendNormal) * targetToLocalSpace;
			}
			
			/*
			 * Gets the bend normal from current bone rotation.
			 * */
			public Vector3 GetBendNormalFromCurrentRotation() {
				return transform.rotation * defaultLocalBendNormal;
			}
			
			#endregion Public methods
		}

		/// <summary>
		/// Reinitiate the solver with new bone Transforms.
		/// </summary>
		/// <returns>
		/// Returns true if the new chain is valid.
		/// </returns>
		public bool SetChain(Transform bone1, Transform bone2, Transform bone3, Transform root) {
			this.bone1.transform = bone1;
			this.bone2.transform = bone2;
			this.bone3.transform = bone3;
			
			Initiate(root);
			return initiated;
		}
		
		#endregion Main Interface

		#region Class Methods

		/// <summary>
		/// Solve the bone chain.
		/// </summary>
		public static void Solve(Transform bone1, Transform bone2, Transform bone3, Vector3 targetPosition, Vector3 bendNormal, float weight) {
			if (weight <= 0f) return;

			// Direction of the limb in solver
			targetPosition = Vector3.Lerp(bone3.position, targetPosition, weight);
			
			Vector3 dir = targetPosition - bone1.position;
			
			// Distance between the first and the last node solver positions
			float length = dir.magnitude;
			if (length == 0f) return;
			
			float sqrMag1 = (bone2.position - bone1.position).sqrMagnitude;
			float sqrMag2 = (bone3.position - bone2.position).sqrMagnitude;
			
			// Get the general world space bending direction
			Vector3 bendDir = Vector3.Cross(dir, bendNormal);
			
			// Get the direction to the trigonometrically solved position of the second node
			Vector3 toBendPoint = GetDirectionToBendPoint(dir, length, bendDir, sqrMag1, sqrMag2);
			
			// Position the second node
			Quaternion q1 = Quaternion.FromToRotation(bone2.position - bone1.position, toBendPoint);
			if (weight < 1f) q1 = Quaternion.Lerp(Quaternion.identity, q1, weight);

			bone1.rotation = q1 * bone1.rotation;

			Quaternion q2 = Quaternion.FromToRotation(bone3.position - bone2.position, targetPosition - bone2.position);
			if (weight < 1f) q2 = Quaternion.Lerp(Quaternion.identity, q2, weight);

			bone2.rotation = q2 * bone2.rotation;
		}

		//Calculates the bend direction based on the law of cosines. NB! Magnitude of the returned vector does not equal to the length of the first bone!
		private static Vector3 GetDirectionToBendPoint(Vector3 direction, float directionMag, Vector3 bendDirection, float sqrMag1, float sqrMag2) {
			float x = ((directionMag * directionMag) + (sqrMag1 - sqrMag2)) / 2f / directionMag;
			float y = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - x * x, 0, Mathf.Infinity));
			
			if (direction == Vector3.zero) return Vector3.zero;
			return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, x);
		}

		#endregion Class Methods
		
		protected override void OnInitiate() {
			if (bendNormal == Vector3.zero) bendNormal = Vector3.right;
			
			OnInitiateVirtual();
			
			IKPosition = bone3.transform.position;
			IKRotation = bone3.transform.rotation;
			
			// Initiating bones
			InitiateBones();

			directHierarchy = IsDirectHierarchy();
		}

		// Are the bones parented directly to each other?
		private bool IsDirectHierarchy() {
			if (bone3.transform.parent != bone2.transform) return false;
			if (bone2.transform.parent != bone1.transform) return false;
			return true;
		}

		// Set the defaults for the bones
		private void InitiateBones() {
			bone1.Initiate(bone2.transform.position, bendNormal);
			bone2.Initiate(bone3.transform.position, bendNormal);

			SetBendPlaneToCurrent();
		}
		
		protected override void OnUpdate() {
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			IKRotationWeight = Mathf.Clamp(IKRotationWeight, 0f, 1f);

			if (target != null) {
				IKPosition = target.position;
				IKRotation = target.rotation;
			}

			OnUpdateVirtual();
			
			if (IKPositionWeight > 0) {

				// Reinitiating the bones when the hierarchy is not direct. This allows for skipping animated bones in the hierarchy.
				if (!directHierarchy) {
					bone1.Initiate(bone2.transform.position, bendNormal);
					bone2.Initiate(bone3.transform.position, bendNormal);
				}

				// Find out if bone lengths should be updated
				bone1.sqrMag = (bone2.transform.position - bone1.transform.position).sqrMagnitude;
				bone2.sqrMag = (bone3.transform.position - bone2.transform.position).sqrMagnitude;
				
				if (bendNormal == Vector3.zero && !Warning.logged) LogWarning("IKSolverTrigonometric Bend Normal is Vector3.zero.");
				
				weightIKPosition = Vector3.Lerp(bone3.transform.position, IKPosition, IKPositionWeight);
				
				// Interpolating bend normal
				Vector3 currentBendNormal = Vector3.Lerp(bone1.GetBendNormalFromCurrentRotation(), bendNormal, IKPositionWeight);
				
				// Calculating and interpolating bend direction
				Vector3 bendDirection = Vector3.Lerp(bone2.transform.position - bone1.transform.position, GetBendDirection(weightIKPosition, currentBendNormal), IKPositionWeight);
				
				if (bendDirection == Vector3.zero) bendDirection = bone2.transform.position - bone1.transform.position;
				
				// Rotating bone1
				bone1.transform.rotation = bone1.GetRotation(bendDirection, currentBendNormal);
				
				// Rotating bone 2
				bone2.transform.rotation = bone2.GetRotation(weightIKPosition - bone2.transform.position, bone2.GetBendNormalFromCurrentRotation());
			}
			
			// Rotating bone3
			if (IKRotationWeight > 0) {
				bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, IKRotation, IKRotationWeight);
			}
			
			OnPostSolveVirtual();
		}
		
		protected Vector3 weightIKPosition;
		protected virtual void OnInitiateVirtual() {}
		protected virtual void OnUpdateVirtual() {}
		protected virtual void OnPostSolveVirtual() {}
		protected bool directHierarchy = true;
		
		/*
		 * Calculates the bend direction based on the Law of Cosines.
		 * */
		protected Vector3 GetBendDirection(Vector3 IKPosition, Vector3 bendNormal) {
			Vector3 direction = IKPosition - bone1.transform.position;
			if (direction == Vector3.zero) return Vector3.zero;
			
			float directionSqrMag = direction.sqrMagnitude;
			float directionMagnitude = (float)Math.Sqrt(directionSqrMag);
			
			float x = (directionSqrMag + bone1.sqrMag - bone2.sqrMag) / 2f / directionMagnitude;
			float y = (float)Math.Sqrt(Mathf.Clamp(bone1.sqrMag - x * x, 0, Mathf.Infinity));
			
			Vector3 yDirection = Vector3.Cross(direction / directionMagnitude, bendNormal);
			return Quaternion.LookRotation(direction, yDirection) * new Vector3(0f, y, x);
		}
	}
}
		
