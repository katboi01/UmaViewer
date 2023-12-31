using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// A wrapper for making IKSolverVRLeg work with other IK components and the Grounder.
	/// </summary>
	[System.Serializable]
	public class IKSolverLeg : IKSolver {

		[Range(0f, 1f)]
		public float IKRotationWeight = 1f;
		/// <summary>
		/// The %IK rotation target.
		/// </summary>
		public Quaternion IKRotation = Quaternion.identity;

		public IKSolver.Point pelvis = new IKSolver.Point();
		public IKSolver.Point thigh = new IKSolver.Point();
		public IKSolver.Point calf = new IKSolver.Point();
		public IKSolver.Point foot = new IKSolver.Point();
		public IKSolver.Point toe = new IKSolver.Point();
		
		public IKSolverVR.Leg leg = new IKSolverVR.Leg();
		public Vector3 heelOffset;
		
		private Vector3[] positions = new Vector3[6];
		private Quaternion[] rotations = new Quaternion[6];

		public override bool IsValid(ref string message) {
			if (pelvis.transform == null || thigh.transform == null || calf.transform == null || foot.transform == null || toe.transform == null) {
				message = "Please assign all bone slots of the Leg IK solver.";
				return false;
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(new Transform[5] { pelvis.transform, thigh.transform, calf.transform, foot.transform, toe.transform });
			if (duplicate != null) {
				message = duplicate.name + " is represented multiple times in the LegIK.";
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
		public bool SetChain(Transform pelvis, Transform thigh, Transform calf, Transform foot, Transform toe, Transform root) {
			this.pelvis.transform = pelvis;
			this.thigh.transform = thigh;
			this.calf.transform = calf;
			this.foot.transform = foot;
			this.toe.transform = toe;
			
			Initiate(root);
			return initiated;
		}

		public override IKSolver.Point[] GetPoints() {
			return new IKSolver.Point[5] { (IKSolver.Point)pelvis, (IKSolver.Point)thigh, (IKSolver.Point)calf, (IKSolver.Point)foot, (IKSolver.Point)toe };
		}
		
		public override IKSolver.Point GetPoint(Transform transform) {
			if (pelvis.transform == transform) return (IKSolver.Point)pelvis;
			if (thigh.transform == transform) return (IKSolver.Point)thigh;
			if (calf.transform == transform) return (IKSolver.Point)calf;
			if (foot.transform == transform) return (IKSolver.Point)foot;
			if (toe.transform == transform) return (IKSolver.Point)toe;
			return null;
		}
		
		public override void StoreDefaultLocalState() {
			thigh.StoreDefaultLocalState();
			calf.StoreDefaultLocalState();
			foot.StoreDefaultLocalState();
			toe.StoreDefaultLocalState();
		}
		
		public override void FixTransforms() {
			if (!initiated) return;

			thigh.FixTransform();
			calf.FixTransform();
			foot.FixTransform();
			toe.FixTransform();
		}

		protected override void OnInitiate() {
			IKPosition = toe.transform.position;
			IKRotation = toe.transform.rotation;

			Read ();
		}

		protected override void OnUpdate() {
			Read ();
			Solve ();
			Write ();
		}
		
		private void Solve() {
			leg.heelPositionOffset += heelOffset;
			
			leg.PreSolve ();
			leg.ApplyOffsets( );
			leg.Solve (true);
			leg.ResetOffsets ();
		}
		
		private void Read() {
			leg.IKPosition = IKPosition;
			leg.positionWeight = IKPositionWeight;
			leg.IKRotation = IKRotation;
			leg.rotationWeight = IKRotationWeight;

			positions [0] = root.position;
			positions [1] = pelvis.transform.position;
			positions [2] = thigh.transform.position;
			positions [3] = calf.transform.position;
			positions [4] = foot.transform.position;
			positions [5] = toe.transform.position;
			
			rotations [0] = root.rotation;
			rotations [1] = pelvis.transform.rotation;
			rotations [2] = thigh.transform.rotation;
			rotations [3] = calf.transform.rotation;
			rotations [4] = foot.transform.rotation;
			rotations [5] = toe.transform.rotation;
			
			leg.Read(positions, rotations, false, false, false, true, true, 1, 2);
		}
		
		private void Write() {
			leg.Write (ref positions, ref rotations);
			
			thigh.transform.rotation = rotations [2];
			calf.transform.rotation = rotations [3];
			foot.transform.rotation = rotations [4];
			toe.transform.rotation = rotations [5];

			calf.transform.position = positions[3];
			foot.transform.position = positions[4];
		}
	}
}