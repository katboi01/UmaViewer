using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Mapping a bone hierarchy to 2 triangles defined by the hip and chest planes.
	/// </summary>
	[System.Serializable]
	public class IKMappingSpine: IKMapping {
		
		#region Main Interface

		/// <summary>
		/// The spine bones.
		/// </summary>
		public Transform[] spineBones;
		/// <summary>
		/// The left upper arm bone.
		/// </summary>
		public Transform leftUpperArmBone;
		/// <summary>
		/// The right upper arm bone.
		/// </summary>
		public Transform rightUpperArmBone;
		/// <summary>
		/// The left thigh bone.
		/// </summary>
		public Transform leftThighBone;
		/// <summary>
		/// The right thigh bone.
		/// </summary>
		public Transform rightThighBone;
		/// <summary>
		/// The number of iterations of the %FABRIK algorithm. Not used if there are 2 bones assigned to Spine in the References.
		/// </summary>
		[Range(1, 3)]
		public int iterations = 3;
		/// <summary>
		/// The weight of twisting the spine bones gradually to the orientation of the chest triangle. Relatively expensive, so set this to 0 if there is not much spine twisting going on.
		/// </summary>
		[Range(0f, 1f)]
		public float twistWeight = 1f;

		/// <summary>
		/// Determines whether this IKMappingSpine is valid
		/// </summary>
		public override bool IsValid(IKSolver solver, ref string message) {
			if (!base.IsValid(solver, ref message)) return false;
			
			foreach (Transform spineBone in spineBones) if (spineBone == null) {
				message = "Spine bones contains a null reference.";
				return false;
			}
			
			int nodes = 0;
			for (int i = 0; i < spineBones.Length; i++) {
				if (solver.GetPoint(spineBones[i]) != null) nodes ++;
			}
			
			if (nodes == 0) {
				message = "IKMappingSpine does not contain any nodes.";
				return false;
			}
			
			if (leftUpperArmBone == null) {
				message = "IKMappingSpine is missing the left upper arm bone.";
				return false;
			}
			
			if (rightUpperArmBone == null) {
				message = "IKMappingSpine is missing the right upper arm bone.";
				return false;
			}
			
			if (leftThighBone == null) {
				message = "IKMappingSpine is missing the left thigh bone.";
				return false;
			}
			
			if (rightThighBone == null) {
				message = "IKMappingSpine is missing the right thigh bone.";
				return false;
			}
			
			if (solver.GetPoint(leftUpperArmBone) == null) {
				message = "Full Body IK is missing the left upper arm node.";
				return false;
			}
			
			if (solver.GetPoint(rightUpperArmBone) == null) {
				message = "Full Body IK is missing the right upper arm node.";
				return false;
			}
			
			if (solver.GetPoint(leftThighBone) == null) {
				message = "Full Body IK is missing the left thigh node.";
				return false;
			}
			
			if (solver.GetPoint(rightThighBone) == null) {
				message = "Full Body IK is missing the right thigh node.";
				return false;
			}
			return true;
		}

		#endregion Main Interface
		
		private int rootNodeIndex;
		private BoneMap[] spine = new BoneMap[0];
		private BoneMap leftUpperArm = new BoneMap(), rightUpperArm = new BoneMap(), leftThigh = new BoneMap(), rightThigh = new BoneMap();
		private bool useFABRIK;
		
		public IKMappingSpine() {}
		
		public IKMappingSpine(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone) {
			SetBones(spineBones, leftUpperArmBone, rightUpperArmBone, leftThighBone, rightThighBone);
		}
		
		public void SetBones(Transform[] spineBones, Transform leftUpperArmBone, Transform rightUpperArmBone, Transform leftThighBone, Transform rightThighBone) {
			this.spineBones = spineBones;
			this.leftUpperArmBone = leftUpperArmBone;
			this.rightUpperArmBone = rightUpperArmBone;
			this.leftThighBone = leftThighBone;
			this.rightThighBone = rightThighBone;
		}

		public void StoreDefaultLocalState() {
			for (int i = 0; i < spine.Length; i++) {
				spine[i].StoreDefaultLocalState();
			}
		}
		
		public void FixTransforms() {
			for (int i = 0; i < spine.Length; i++) {
				spine[i].FixTransform(i == 0 || i == spine.Length - 1);
			}
		}
		
		/*
		 * Initiating and setting defaults
		 * */
		public override void Initiate(IKSolverFullBody solver) {
			if (iterations <= 0) iterations = 3;
			
			// Creating the bone maps
			if (spine == null || spine.Length != spineBones.Length) spine = new BoneMap[spineBones.Length];

			rootNodeIndex = -1;
			
			for (int i = 0; i < spineBones.Length; i++) {
				if (spine[i] == null) spine[i] = new BoneMap();
				spine[i].Initiate(spineBones[i], solver);

				// Finding the root node
				if (spine[i].isNodeBone) rootNodeIndex = i;
			}

			if (leftUpperArm == null) leftUpperArm = new BoneMap();
			if (rightUpperArm == null) rightUpperArm = new BoneMap();
			if (leftThigh == null) leftThigh = new BoneMap();
			if (rightThigh == null) rightThigh = new BoneMap();
			
			leftUpperArm.Initiate(leftUpperArmBone, solver);
			rightUpperArm.Initiate(rightUpperArmBone, solver);
			leftThigh.Initiate(leftThighBone, solver);
			rightThigh.Initiate(rightThighBone, solver);

			for (int i = 0; i < spine.Length; i++) spine[i].SetIKPosition();
			
			// Defining the plane for the first bone
			spine[0].SetPlane(solver, spine[rootNodeIndex].transform, leftThigh.transform, rightThigh.transform);
			
			// Finding bone lengths and axes
			for (int i = 0; i < spine.Length - 1; i++) {
				spine[i].SetLength(spine[i + 1]);
				spine[i].SetLocalSwingAxis(spine[i + 1]);

				spine[i].SetLocalTwistAxis(leftUpperArm.transform.position - rightUpperArm.transform.position, spine[i + 1].transform.position - spine[i].transform.position);
			}
			
			// Defining the plane for the last bone
			spine[spine.Length - 1].SetPlane(solver, spine[rootNodeIndex].transform, leftUpperArm.transform, rightUpperArm.transform);
			spine[spine.Length - 1].SetLocalSwingAxis(leftUpperArm, rightUpperArm);

			useFABRIK = UseFABRIK();
		}

		// Should the spine mapping use the FABRIK algorithm
		private bool UseFABRIK() {
			if (spine.Length > 3) return true;
			if (rootNodeIndex != 1) return true;
			return false;
		}

		/*
		 * Updating the bone maps to the current animated state of the character
		 * */
		public void ReadPose() {
			spine[0].UpdatePlane(true, true);
			
			for (int i = 0; i < spine.Length - 1; i++) {
				spine[i].SetLength(spine[i + 1]);

				spine[i].SetLocalSwingAxis(spine[i + 1]);
				spine[i].SetLocalTwistAxis(leftUpperArm.transform.position - rightUpperArm.transform.position, spine[i + 1].transform.position - spine[i].transform.position);
			}
			
			spine[spine.Length - 1].UpdatePlane(true, true);
			spine[spine.Length - 1].SetLocalSwingAxis(leftUpperArm, rightUpperArm);
		}

		/*
		 * Mapping the spine to the hip and chest planes
		 * */
		public void WritePose(IKSolverFullBody solver) {
			Vector3 firstPosition = spine[0].GetPlanePosition(solver);
			Vector3 rootPosition = solver.GetNode(spine[rootNodeIndex].chainIndex, spine[rootNodeIndex].nodeIndex).solverPosition;
			Vector3 lastPosition = spine[spine.Length - 1].GetPlanePosition(solver);

			// If we have more than 3 bones, use the FABRIK algorithm
			if (useFABRIK) {
				Vector3 offset = solver.GetNode(spine[rootNodeIndex].chainIndex, spine[rootNodeIndex].nodeIndex).solverPosition - spine[rootNodeIndex].transform.position;
					
				for (int i = 0; i < spine.Length; i++) {
					spine[i].ikPosition = spine[i].transform.position + offset;
				}
					
				// Iterating the FABRIK algorithm
				for (int i = 0; i < iterations; i++) {
					ForwardReach(lastPosition);
					BackwardReach(firstPosition);
					spine[rootNodeIndex].ikPosition = rootPosition;
				}
			} else {
				// When we have just 3 bones, we know their positions already
				spine[0].ikPosition = firstPosition;
				spine[rootNodeIndex].ikPosition = rootPosition;
			}

			spine[spine.Length - 1].ikPosition = lastPosition;

			// Mapping the spine bones to the solver
			MapToSolverPositions(solver);
		}
		
		/*
		 * Stage 1 of the FABRIK algorithm.
		 * */
		public void ForwardReach(Vector3 position) {
			// Lerp last bone's ikPosition to position
			spine[spineBones.Length - 1].ikPosition = position;
			
			for (int i = spine.Length - 2; i > -1; i--) {
				// Finding joint positions
				spine[i].ikPosition = SolveFABRIKJoint(spine[i].ikPosition, spine[i + 1].ikPosition, spine[i].length);
			}
		}
		
		/*
		 * Stage 2 of the FABRIK algorithm
		 * */
		private void BackwardReach(Vector3 position) {
			spine[0].ikPosition = position;
			
			// Finding joint positions
			for (int i = 1; i < spine.Length; i++) {
				spine[i].ikPosition = SolveFABRIKJoint(spine[i].ikPosition, spine[i - 1].ikPosition, spine[i - 1].length);
			}
		}
		
		/*
		 * Positioning and rotating the spine bones to match the solver positions
		 * */
		private void MapToSolverPositions(IKSolverFullBody solver) {
			// Translating the first bone
			// Note: spine here also includes the pelvis
			spine[0].SetToIKPosition();
			spine[0].RotateToPlane(solver, 1f);

			// Translating all the bones between the first and the last
			for (int i = 1; i < spine.Length - 1; i++) {
				spine[i].Swing(spine[i + 1].ikPosition, 1f);

				if (twistWeight > 0) {
					float bWeight = (float)i / ((float)spine.Length - 2);

					Vector3 s1 = solver.GetNode(leftUpperArm.chainIndex, leftUpperArm.nodeIndex).solverPosition;
					Vector3 s2 = solver.GetNode(rightUpperArm.chainIndex, rightUpperArm.nodeIndex).solverPosition;
					spine[i].Twist(s1 - s2, spine[i + 1].ikPosition - spine[i].transform.position, bWeight * twistWeight);
				}
			}
			
			// Translating the last bone
			spine[spine.Length - 1].SetToIKPosition();
			spine[spine.Length - 1].RotateToPlane(solver, 1f);
		}
	}
}
