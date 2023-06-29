using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// A wrapper for making IKSolverVRArm work with other IK components.
	/// </summary>
	[System.Serializable]
	public class IKSolverArm : IKSolver {

		[Range(0f, 1f)]
		public float IKRotationWeight = 1f;
		/// <summary>
		/// The %IK rotation target.
		/// </summary>
		public Quaternion IKRotation = Quaternion.identity;

		public IKSolver.Point chest = new IKSolver.Point();
		public IKSolver.Point shoulder = new IKSolver.Point();
		public IKSolver.Point upperArm = new IKSolver.Point();
		public IKSolver.Point forearm = new IKSolver.Point();
		public IKSolver.Point hand = new IKSolver.Point();

		public bool isLeft;
		
		public IKSolverVR.Arm arm = new IKSolverVR.Arm();

		private Vector3[] positions = new Vector3[6];
		private Quaternion[] rotations = new Quaternion[6];

		public override bool IsValid(ref string message) {
			if (chest.transform == null || shoulder.transform == null || upperArm.transform == null || forearm.transform == null || hand.transform == null) {
				message = "Please assign all bone slots of the Arm IK solver.";
				return false;
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(new Transform[5] { chest.transform, shoulder.transform, upperArm.transform, forearm.transform, hand.transform });
			if (duplicate != null) {
				message = duplicate.name + " is represented multiple times in the ArmIK.";
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reinitiate the solver with new bone Transforms.
		/// </summary>
		/// <returns>
		/// Returns true if the new chain is valid.
		/// </returns>
		public bool SetChain(Transform chest, Transform shoulder, Transform upperArm, Transform forearm, Transform hand, Transform root) {
			this.chest.transform = chest;
			this.shoulder.transform = shoulder;
			this.upperArm.transform = upperArm;
			this.forearm.transform = forearm;
			this.hand.transform = hand;
			
			Initiate(root);
			return initiated;
		}

		public override IKSolver.Point[] GetPoints() {
			return new IKSolver.Point[5] { (IKSolver.Point)chest, (IKSolver.Point)shoulder, (IKSolver.Point)upperArm, (IKSolver.Point)forearm, (IKSolver.Point)hand };
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			if (chest.transform == transform) return (IKSolver.Point)chest;
			if (shoulder.transform == transform) return (IKSolver.Point)shoulder;
			if (upperArm.transform == transform) return (IKSolver.Point)upperArm;
			if (forearm.transform == transform) return (IKSolver.Point)forearm;
			if (hand.transform == transform) return (IKSolver.Point)hand;
			return null;
		}
		
		public override void StoreDefaultLocalState() {
			shoulder.StoreDefaultLocalState();
			upperArm.StoreDefaultLocalState();
			forearm.StoreDefaultLocalState();
			hand.StoreDefaultLocalState();
		}
		
		public override void FixTransforms() {
			if (!initiated) return;

			shoulder.FixTransform();
			upperArm.FixTransform();
			forearm.FixTransform();
			hand.FixTransform();
		}

		protected override void OnInitiate() {
			IKPosition = hand.transform.position;
			IKRotation = hand.transform.rotation;

			Read ();
		}

		protected override void OnUpdate() {
			Read ();
			Solve ();
			Write ();
		}
		
		private void Solve() {
			arm.PreSolve ();
			arm.ApplyOffsets( );
			arm.Solve (isLeft);
			arm.ResetOffsets ();
		}
		
		private void Read() {
			arm.IKPosition = IKPosition;
			arm.positionWeight = IKPositionWeight;
			arm.IKRotation = IKRotation;
			arm.rotationWeight = IKRotationWeight;

			positions [0] = root.position;
			positions [1] = chest.transform.position;
			positions [2] = shoulder.transform.position;
			positions [3] = upperArm.transform.position;
			positions [4] = forearm.transform.position;
			positions [5] = hand.transform.position;
			
			rotations [0] = root.rotation;
			rotations [1] = chest.transform.rotation;
			rotations [2] = shoulder.transform.rotation;
			rotations [3] = upperArm.transform.rotation;
			rotations [4] = forearm.transform.rotation;
			rotations [5] = hand.transform.rotation;
			
			arm.Read(positions, rotations, false, false, true, false, false, 1, 2);
		}
		
		private void Write() {
			arm.Write (ref positions, ref rotations);
			
			shoulder.transform.rotation = rotations [2];
			upperArm.transform.rotation = rotations [3];
			forearm.transform.rotation = rotations [4];
			hand.transform.rotation = rotations [5];

			forearm.transform.position = positions[4];
			hand.transform.position = positions[5];
		}
	}
}