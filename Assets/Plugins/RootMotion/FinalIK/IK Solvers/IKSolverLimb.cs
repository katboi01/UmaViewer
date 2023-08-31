using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Extends IKSolverTrigonometric to add automatic bend and rotation modes.
	/// </summary>
	[System.Serializable]
	public class IKSolverLimb : IKSolverTrigonometric {
		
		#region Main Interface
		
		/// <summary>
		/// The AvatarIKGoal of this solver.
		/// </summary>
		public AvatarIKGoal goal;
		/// <summary>
		/// Bend normal modifier.
		/// </summary>
		public BendModifier bendModifier;
		/// <summary>
		/// Weight of maintaining the rotation of the third bone as it was before solving.
		/// </summary>
		[Range(0f, 1f)]
		public float maintainRotationWeight;
		/// <summary>
		/// Weight of bend normal modifier.
		/// </summary>
		[Range(0f, 1f)]
		public float bendModifierWeight = 1f;
		/// <summary>
		/// The bend goal Transform.
		/// </summary>
		public Transform bendGoal;

		/// <summary>
		/// Used to record rotation of the last bone for one frame. 
		/// If MaintainRotation is not called and maintainRotationWeight > 0, the solver will maintain the rotation of the last bone as it was before solving the %IK.
		/// You will probably need this if you wanted to maintain the animated rotation of a foot despite of any other %IK solver that manipulates it's parents' rotation.
		/// So you would call %MaintainRotation() in LateUpdate() after animation and before updating the Spine %IK solver that would change the foot's rotation.
		/// </summary>
		public void MaintainRotation() {
			if (!initiated) return;
			
			maintainRotation = bone3.transform.rotation;
			maintainRotationFor1Frame = true;
		}
		
		/// <summary>
		/// If Auto Bend is on "Animation', %MaintainBend() can be used to set the bend axis relative to the first bone's rotation.
		/// </summary>
		public void MaintainBend() {
			if (!initiated) return;
			
			animationNormal = bone1.GetBendNormalFromCurrentRotation();
			
			maintainBendFor1Frame = true;
		}
		
		/// <summary>
		/// Automatic bend modes.
		/// </summary>
		[System.Serializable]
		public enum BendModifier {
			Animation, // Bending relative to the animated rotation of the first bone
			Target, // Bending relative to IKRotation
			Parent, // Bending relative to parentBone
			Arm, // Arm modifier tries to find the most biometrically natural and relaxed arm bend plane
			Goal // Use the bend goal Transform
		}
		
		#endregion Main Interface
		
		/*
		 * Override before Initiate
		 * */
		protected override void OnInitiateVirtual() {
			defaultRootRotation = root.rotation;
			
			if (bone1.transform.parent != null) {
				parentDefaultRotation = Quaternion.Inverse(defaultRootRotation) * bone1.transform.parent.rotation;
			}
			
			if (bone3.rotationLimit != null) bone3.rotationLimit.Disable();
			bone3DefaultRotation = bone3.transform.rotation;
			
			// Set bend plane to current (cant use the public SetBendPlaneToCurrent() method here because the solver has not initiated yet)
			Vector3 normal = Vector3.Cross(bone2.transform.position - bone1.transform.position, bone3.transform.position - bone2.transform.position);
			if (normal != Vector3.zero) bendNormal = normal;
			
			animationNormal = bendNormal;
			
			StoreAxisDirections(ref axisDirectionsLeft);
			StoreAxisDirections(ref axisDirectionsRight);
		}
		
		/*
		 * Changing stuff in Update() before solving
		 * */
		protected override void OnUpdateVirtual() {
			if (IKPositionWeight > 0) {
				// Clamping weights
				bendModifierWeight = Mathf.Clamp(bendModifierWeight, 0f, 1f);
				maintainRotationWeight = Mathf.Clamp(maintainRotationWeight, 0f, 1f);
				
				// Storing the bendNormal for reverting after solving
				_bendNormal = bendNormal;
				
				// Modifying bendNormal
				bendNormal = GetModifiedBendNormal();
			}
			
			if (maintainRotationWeight * IKPositionWeight > 0) {
				// Storing bone3 rotation
				bone3RotationBeforeSolve = maintainRotationFor1Frame? maintainRotation : bone3.transform.rotation;
				maintainRotationFor1Frame = false;
			}
		}
		
		/*
		 * Changing stuff in Update() after solving
		 * */
		protected override void OnPostSolveVirtual() {
			// Revert bendNormal to what it was before solving
			if (IKPositionWeight > 0) bendNormal = _bendNormal;
			
			// Auto rotation modes
			if (maintainRotationWeight * IKPositionWeight > 0) {
				bone3.transform.rotation = Quaternion.Slerp(bone3.transform.rotation, bone3RotationBeforeSolve, maintainRotationWeight * IKPositionWeight);
			}
		}
		
		/*
		 * Axis direction contains an arm bend axis for a specific IKPosition direction from the first bone. Used in Arm BendModifier mode.
		 * */
		[System.Serializable]
		public struct AxisDirection {
			public Vector3 direction;
			public Vector3 axis;
			public float dot;
			
			public AxisDirection(Vector3 direction, Vector3 axis) {
				this.direction = direction.normalized;
				this.axis = axis.normalized;
				this.dot = 0;
			}
		}
		
		public IKSolverLimb() {}
		
		public IKSolverLimb(AvatarIKGoal goal) {
			this.goal = goal;
		}
		
		private bool maintainBendFor1Frame, maintainRotationFor1Frame;
		private Quaternion defaultRootRotation, parentDefaultRotation, bone3RotationBeforeSolve, maintainRotation, bone3DefaultRotation;
		private Vector3 _bendNormal, animationNormal;
		private AxisDirection[] axisDirectionsLeft = new AxisDirection[4];
		private AxisDirection[] axisDirectionsRight = new AxisDirection[4];
		private AxisDirection[] axisDirections {
			get {
				if (goal == AvatarIKGoal.LeftHand) return axisDirectionsLeft;
				return axisDirectionsRight;
			}
		}
		
		/*
		 * Storing Bend axes for arm directions. The directions and axes here were found empirically, feel free to edit, add or remove.
		 * All vectors are in default character rotation space, where x is right and z is forward.
		 * */
		private void StoreAxisDirections(ref AxisDirection[] axisDirections) {
			axisDirections[0] = new AxisDirection(Vector3.zero, new Vector3(-1f, 0f, 0f)); // default
			axisDirections[1] = new AxisDirection(new Vector3(0.5f, 0f, -0.2f), new Vector3(-0.5f, -1f, 1f)); // behind head
			axisDirections[2] = new AxisDirection(new Vector3(-0.5f, -1f, -0.2f), new Vector3(0f, 0.5f, -1f)); // arm twist
			axisDirections[3] = new AxisDirection(new Vector3(-0.5f, -0.5f, 1f), new Vector3(-1f, -1f, -1f)); // cross heart
		}
		
		/*
		 * Modifying bendNormal
		 * */
		private Vector3 GetModifiedBendNormal() {
			float weight = bendModifierWeight;
			if (weight <= 0) return bendNormal;
			
			switch(bendModifier) {
			// Animation Bend Mode attempts to maintain the bend axis as it is in the animation
			case BendModifier.Animation:
				if (!maintainBendFor1Frame) MaintainBend();
				maintainBendFor1Frame = false;
				return Vector3.Lerp(bendNormal, animationNormal, weight);
				
			// Bending relative to the parent of the first bone
			case BendModifier.Parent:
				if (bone1.transform.parent == null) return bendNormal;
				
				Quaternion parentRotation = bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation);
				return Quaternion.Slerp(Quaternion.identity, parentRotation * Quaternion.Inverse(defaultRootRotation), weight) * bendNormal;
				
			// Bending relative to IKRotation
			case BendModifier.Target:
				Quaternion targetRotation = IKRotation * Quaternion.Inverse(bone3DefaultRotation);
				return Quaternion.Slerp(Quaternion.identity, targetRotation, weight) * bendNormal;

			// Anatomic Arm
			case BendModifier.Arm:
				if (bone1.transform.parent == null) return bendNormal;
				
				// Disabling this for legs
				if (goal == AvatarIKGoal.LeftFoot || goal == AvatarIKGoal.RightFoot) {
					if (!Warning.logged) LogWarning("Trying to use the 'Arm' bend modifier on a leg.");
					return bendNormal;
				}
				
				Vector3 direction = (IKPosition - bone1.transform.position).normalized;
				
				// Converting direction to default world space
				direction = Quaternion.Inverse(bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation)) * direction;
				
				// Inverting direction for left hand
				if (goal == AvatarIKGoal.LeftHand) direction.x = -direction.x;
				
				// Calculating dot products for all AxisDirections
				for (int i = 1; i < axisDirections.Length; i++) {
					axisDirections[i].dot = Mathf.Clamp(Vector3.Dot(axisDirections[i].direction, direction), 0f, 1f);
					axisDirections[i].dot = Interp.Float(axisDirections[i].dot, InterpolationMode.InOutQuintic);
				}
				
				// Summing up the arm bend axis
				Vector3 sum = axisDirections[0].axis;
				
				//for (int i = 1; i < axisDirections.Length; i++) sum = Vector3.Lerp(sum, axisDirections[i].axis, axisDirections[i].dot);
				for (int i = 1; i < axisDirections.Length; i++) sum = Vector3.Slerp(sum, axisDirections[i].axis, axisDirections[i].dot);
				
				// Inverting sum for left hand
				if (goal == AvatarIKGoal.LeftHand) {
					sum.x = -sum.x;
					sum = -sum;
				}
				
				// Converting sum back to parent space
				Vector3 armBendNormal = bone1.transform.parent.rotation * Quaternion.Inverse(parentDefaultRotation) * sum;
				
				if (weight >= 1) return armBendNormal;
				return Vector3.Lerp(bendNormal, armBendNormal, weight);
			
			// Bending towards the bend goal Transform
			case BendModifier.Goal:
				if (bendGoal == null) {
					if (!Warning.logged) LogWarning("Trying to use the 'Goal' Bend Modifier, but the Bend Goal is unassigned.");
					return bendNormal;
				}

				Vector3 normal = Vector3.Cross(bendGoal.position - bone1.transform.position, IKPosition - bone1.transform.position);

				if (normal == Vector3.zero) return bendNormal;

				if (weight >= 1f) return normal;
				return Vector3.Lerp(bendNormal, normal, weight);
			default: return bendNormal;
			}
		}
	}
}
