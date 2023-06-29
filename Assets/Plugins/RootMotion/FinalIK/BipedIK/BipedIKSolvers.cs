using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// BipedIK solver collection.
	/// </summary>
	[System.Serializable]
	public class BipedIKSolvers {
		/// <summary>
		/// The left foot
		/// </summary>
		public IKSolverLimb leftFoot = new IKSolverLimb(AvatarIKGoal.LeftFoot);
		/// <summary>
		/// The right foot.
		/// </summary>
		public IKSolverLimb rightFoot = new IKSolverLimb(AvatarIKGoal.RightFoot);
		/// <summary>
		/// The left hand.
		/// </summary>
		public IKSolverLimb leftHand = new IKSolverLimb(AvatarIKGoal.LeftHand);
		/// <summary>
		/// The right hand.
		/// </summary>
		public IKSolverLimb rightHand = new IKSolverLimb(AvatarIKGoal.RightHand);
		/// <summary>
		/// The spine.
		/// </summary>
		public IKSolverFABRIK spine = new IKSolverFABRIK();
		/// <summary>
		/// The Look At %IK.
		/// </summary>
		public IKSolverLookAt lookAt = new IKSolverLookAt();
		/// <summary>
		/// The Aim %IK. Rotates the spine to aim a transform's forward towards the target.
		/// </summary>
		public IKSolverAim aim = new IKSolverAim();
		/// <summary>
		/// %Constraints for manipulating the character's pelvis.
		/// </summary>
		public Constraints pelvis = new Constraints();

		/// <summary>
		/// Gets the array containing all the limbs.
		/// </summary>
		public IKSolverLimb[] limbs {
			get {
				if (_limbs == null || (_limbs != null && _limbs.Length != 4)) _limbs = new IKSolverLimb[4] { leftFoot, rightFoot, leftHand, rightHand };
				return _limbs;
			}	
		}
		private IKSolverLimb[] _limbs;
		
		/// <summary>
		/// Gets the array containing all %IK solvers.
		/// </summary>
		public IKSolver[] ikSolvers {
			get {
				if (_ikSolvers == null || (_ikSolvers != null && _ikSolvers.Length != 7)) _ikSolvers = new IKSolver[7] { leftFoot, rightFoot, leftHand, rightHand, spine, lookAt, aim };
				return _ikSolvers;
			}
		}
		private IKSolver[] _ikSolvers;
		
		public void AssignReferences(BipedReferences references) {
			// Assigning limbs from references
			leftHand.SetChain(references.leftUpperArm, references.leftForearm, references.leftHand, references.root);
			rightHand.SetChain(references.rightUpperArm, references.rightForearm, references.rightHand, references.root);
			leftFoot.SetChain(references.leftThigh, references.leftCalf, references.leftFoot, references.root);
			rightFoot.SetChain(references.rightThigh, references.rightCalf, references.rightFoot, references.root);

			// Assigning spine bones from references
			spine.SetChain(references.spine, references.root);

			// Assigning lookAt bones from references
			lookAt.SetChain(references.spine, references.head, references.eyes, references.root);

			// Assigning Aim bones from references
			aim.SetChain(references.spine, references.root);

			leftFoot.goal = AvatarIKGoal.LeftFoot;
			rightFoot.goal = AvatarIKGoal.RightFoot;
			leftHand.goal = AvatarIKGoal.LeftHand;
			rightHand.goal = AvatarIKGoal.RightHand;
		}
	}
}
