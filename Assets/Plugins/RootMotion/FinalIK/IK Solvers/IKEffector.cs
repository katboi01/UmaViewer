using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	///  Effector for manipulating node based %IK solvers.
	/// </summary>
	[System.Serializable]
	public class IKEffector {
		
		#region Main Interface

		/// <summary>
		/// Gets the main node.
		/// </summary>
		public IKSolver.Node GetNode(IKSolverFullBody solver) {
			return solver.chain[chainIndex].nodes[nodeIndex];
		}

		/// <summary>
		/// The node transform used by this effector.
		/// </summary>
		public Transform bone;
		/// <summary>
		/// The target Transform (optional, you can use just the position and rotation instead).
		/// </summary>
		public Transform target;
		/// <summary>
		/// The position weight.
		/// </summary>
		[Range(0f, 1f)]
		public float positionWeight;
		/// <summary>
		/// The rotation weight.
		/// </summary>
		[Range(0f, 1f)]
		public float rotationWeight;
		/// <summary>
		/// The effector position in world space.
		/// </summary>
		public Vector3 position = Vector3.zero;
		/// <summary>
		/// The effector rotation relative to default rotation in world space.
		/// </summary>
		public Quaternion rotation = Quaternion.identity;
		/// <summary>
		/// The position offset in world space. positionOffset will be reset to Vector3.zero each frame after the solver is complete.
		/// </summary>
		public Vector3 positionOffset;
		/// <summary>
		/// Is this the last effector of a node chain?
		/// </summary>
		public bool isEndEffector { get; private set; }
		/// <summary>
		/// If false, child nodes will be ignored by this effector (if it has any).
		/// </summary>
		public bool effectChildNodes = true;
		/// <summary>
		/// Keeps the node position relative to the triangle defined by the plane bones (applies only to end-effectors).
		/// </summary>
		[Range(0f, 1f)]
		public float maintainRelativePositionWeight;

		/// <summary>
		/// Pins the effector to the animated position of it's bone.
		/// </summary>
		public void PinToBone(float positionWeight, float rotationWeight) {
			position = bone.position;
			this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);

			rotation = bone.rotation;
			this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}

		#endregion Main Interface

		public Transform[] childBones = new Transform[0]; // The optional list of other bones that positionOffset and position of this effector will be applied to.
		public Transform planeBone1; // The first bone defining the parent plane.
		public Transform planeBone2; // The second bone defining the parent plane.
		public Transform planeBone3; // The third bone defining the parent plane.
		public Quaternion planeRotationOffset = Quaternion.identity; // Used by Bend Constraints

		private float posW, rotW;
		private Vector3[] localPositions = new Vector3[0];
		private bool usePlaneNodes;
		private Quaternion animatedPlaneRotation = Quaternion.identity;
		private Vector3 animatedPosition;
		private bool firstUpdate;
		private int chainIndex = -1;
		private int nodeIndex = -1;
		private int plane1ChainIndex;
		private int plane1NodeIndex = -1;
		private int plane2ChainIndex = -1;
		private int plane2NodeIndex = -1;
		private int plane3ChainIndex = -1;
		private int plane3NodeIndex = -1;
		private int[] childChainIndexes = new int[0];
		private int[] childNodeIndexes = new int[0];
		
		public IKEffector() {}
		
		public IKEffector (Transform bone, Transform[] childBones) {
			this.bone = bone;
			this.childBones = childBones;
		}
		
		/*
		 * Determines whether this IKEffector is valid or not.
		 * */
		public bool IsValid(IKSolver solver, ref string message) {

			if (bone == null) {
				message = "IK Effector bone is null.";
				return false;
			}
			
			if (solver.GetPoint(bone) == null) {
				message = "IK Effector is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.";
				return false;
			}

			foreach (Transform b in childBones) {
				if (b == null) {
					message = "IK Effector contains a null reference.";
					return false;
				}
			}
			
			foreach (Transform b in childBones) {
				if (solver.GetPoint(b) == null) {
					message = "IK Effector is referencing to a bone '" + b.name + "' that does not excist in the Node Chain.";
					return false;
				}
			}

			if (planeBone1 != null && solver.GetPoint(planeBone1) == null) {
				message = "IK Effector is referencing to a bone '" + planeBone1.name + "' that does not excist in the Node Chain.";
				return false;
			}

			if (planeBone2 != null && solver.GetPoint(planeBone2) == null) {
				message = "IK Effector is referencing to a bone '" + planeBone2.name + "' that does not excist in the Node Chain.";
				return false;
			}

			if (planeBone3 != null && solver.GetPoint(planeBone3) == null) {
				message = "IK Effector is referencing to a bone '" + planeBone3.name + "' that does not excist in the Node Chain.";
				return false;
			}

			return true;
		}

		/*
		 * Initiate the effector, set default values
		 * */
		public void Initiate(IKSolverFullBody solver) {
			position = bone.position;
			rotation = bone.rotation;
			animatedPlaneRotation = Quaternion.identity;

			// Getting the node
			solver.GetChainAndNodeIndexes(bone, out chainIndex, out nodeIndex);

			// Child nodes
			childChainIndexes = new int[childBones.Length];
			childNodeIndexes = new int[childBones.Length];

			for (int i = 0; i < childBones.Length; i++) {
				solver.GetChainAndNodeIndexes(childBones[i], out childChainIndexes[i], out childNodeIndexes[i]);
			}

			localPositions = new Vector3[childBones.Length];

			// Plane nodes
			usePlaneNodes = false;

			if (planeBone1 != null) {
				solver.GetChainAndNodeIndexes(planeBone1, out plane1ChainIndex, out plane1NodeIndex);

				if (planeBone2 != null) {
					solver.GetChainAndNodeIndexes(planeBone2, out plane2ChainIndex, out plane2NodeIndex);

					if (planeBone3 != null) {
						solver.GetChainAndNodeIndexes(planeBone3, out plane3ChainIndex, out plane3NodeIndex);

						usePlaneNodes = true;
					}
				}

				isEndEffector = true;
			} else isEndEffector = false; 
		}
		
		/*
		 * Clear node offset
		 * */
		public void ResetOffset(IKSolverFullBody solver) {
			solver.GetNode(chainIndex, nodeIndex).offset = Vector3.zero;

			for (int i = 0; i < childChainIndexes.Length; i++) {
				solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).offset = Vector3.zero;
			}
		}

		/*
		 * Set the position and rotation to match the target
		 * */
		public void SetToTarget() {
			if (target == null) return;
			position = target.position;
			rotation = target.rotation;
		}
		
		/*
		 * Presolving, applying offset
		 * */
		public void OnPreSolve(IKSolverFullBody solver) {
			positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
			maintainRelativePositionWeight = Mathf.Clamp(maintainRelativePositionWeight, 0f, 1f);

			// Calculating weights
			posW = positionWeight * solver.IKPositionWeight;
			rotW = rotationWeight * solver.IKPositionWeight;

			solver.GetNode(chainIndex, nodeIndex).effectorPositionWeight = posW;
			solver.GetNode(chainIndex, nodeIndex).effectorRotationWeight = rotW;

			solver.GetNode(chainIndex, nodeIndex).solverRotation = rotation;

			if (float.IsInfinity(positionOffset.x) ||
				float.IsInfinity(positionOffset.y) ||
				float.IsInfinity(positionOffset.z)
			    ) Debug.LogError("Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.", bone);

			if (float.IsNaN(positionOffset.x) ||
			    float.IsNaN(positionOffset.y) ||
			    float.IsNaN(positionOffset.z)
			    ) Debug.LogError("Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.", bone);

			if (positionOffset.sqrMagnitude > 10000000000f) Debug.LogError("Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.", bone);

			if (float.IsInfinity(position.x) ||
			    float.IsInfinity(position.y) ||
			    float.IsInfinity(position.z)
			    ) Debug.LogError("Invalid IKEffector.position (contains Infinity)!");

			solver.GetNode(chainIndex, nodeIndex).offset += positionOffset * solver.IKPositionWeight;

			if (effectChildNodes && solver.iterations > 0) {
				for (int i = 0; i < childBones.Length; i++) {
					localPositions[i] = childBones[i].transform.position - bone.transform.position;

					solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).offset += positionOffset * solver.IKPositionWeight;
				}
			}

			// Relative to Plane
			if (usePlaneNodes && maintainRelativePositionWeight > 0f) {
				animatedPlaneRotation = Quaternion.LookRotation(planeBone2.position - planeBone1.position, planeBone3.position - planeBone1.position);;
			}

			firstUpdate = true;
		}

		/*
		 * Called after writing the pose
		 * */
		public void OnPostWrite() {
			positionOffset = Vector3.zero;
		}

		/*
		* Rotation of plane nodes in the solver
		* */
		private Quaternion GetPlaneRotation(IKSolverFullBody solver) {
			Vector3 p1 = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition;
			Vector3 p2 = solver.GetNode(plane2ChainIndex, plane2NodeIndex).solverPosition;
			Vector3 p3 = solver.GetNode(plane3ChainIndex, plane3NodeIndex).solverPosition;

			Vector3 viewingVector = p2 - p1;
			Vector3 upVector = p3 - p1;

			if (viewingVector == Vector3.zero) {
				Warning.Log("Make sure you are not placing 2 or more FBBIK effectors of the same chain to exactly the same position.", bone);
				return Quaternion.identity;
			}
			
			return Quaternion.LookRotation(viewingVector, upVector);
		}

		/*
		 * Manipulating node solverPosition
		 * */
		public void Update(IKSolverFullBody solver) {
			if (firstUpdate) {
				animatedPosition = bone.position + solver.GetNode(chainIndex, nodeIndex).offset;
				firstUpdate = false;
			}

			solver.GetNode(chainIndex, nodeIndex).solverPosition = Vector3.Lerp(GetPosition(solver, out planeRotationOffset), position, posW);

			// Child nodes
			if (!effectChildNodes) return;

			for (int i = 0; i < childBones.Length; i++) {
				solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).solverPosition = Vector3.Lerp(solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).solverPosition, solver.GetNode(chainIndex, nodeIndex).solverPosition + localPositions[i], posW);
			}
		}

		/*
		 * Gets the starting position of the iteration
		 * */
		private Vector3 GetPosition(IKSolverFullBody solver, out Quaternion planeRotationOffset) {
			planeRotationOffset = Quaternion.identity;
			if (!isEndEffector) return solver.GetNode(chainIndex, nodeIndex).solverPosition; // non end-effectors are always free

			if (maintainRelativePositionWeight <= 0f) return animatedPosition;

			// Maintain relative position
			Vector3 p = bone.position;
			Vector3 dir = p - planeBone1.position;
				
			planeRotationOffset = GetPlaneRotation(solver) * Quaternion.Inverse(animatedPlaneRotation);

			p = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition + planeRotationOffset * dir;

			// Interpolate the rotation offset
			planeRotationOffset = Quaternion.Lerp(Quaternion.identity, planeRotationOffset, maintainRelativePositionWeight);

			return Vector3.Lerp(animatedPosition, p + solver.GetNode(chainIndex, nodeIndex).offset, maintainRelativePositionWeight);
		}
	}
}