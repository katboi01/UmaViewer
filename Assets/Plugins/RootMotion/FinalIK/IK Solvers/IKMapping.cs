using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Maps a bone or a collection of bones to a node based %IK solver
	/// </summary>
	[System.Serializable]
	public class IKMapping {
		
		#region Main Interface
		
		/// <summary>
		/// Contains mapping information of a single bone
		/// </summary>
		[System.Serializable]
		public class BoneMap {
			/// <summary>
			/// The transform.
			/// </summary>
			public Transform transform;
			/// <summary>
			/// The node in %IK Solver.
			/// </summary>
			//public IKSolver.Node node;

			public int chainIndex = -1;
			public int nodeIndex = -1;

			public Vector3 defaultLocalPosition;
			public Quaternion defaultLocalRotation;
			public Vector3 localSwingAxis, localTwistAxis, planePosition, ikPosition;
			public Quaternion defaultLocalTargetRotation;
			private Quaternion maintainRotation;
			public float length;
			public Quaternion animatedRotation;

			private Transform planeBone1, planeBone2, planeBone3;
			private int plane1ChainIndex = -1;
			private int plane1NodeIndex = -1;
			private int plane2ChainIndex = -1;
			private int plane2NodeIndex = -1;
			private int plane3ChainIndex = -1;
			private int plane3NodeIndex = -1;

			//private IKSolver.Node planeNode1, planeNode2, planeNode3;

			public void Initiate(Transform transform, IKSolverFullBody solver) {
				this.transform = transform;

				solver.GetChainAndNodeIndexes(transform, out chainIndex, out nodeIndex);
				//IKSolver.Point point = solver.GetPoint(transform);
				//this.node = point as IKSolver.Node;
			}

			/// <summary>
			/// Gets the current swing direction of the bone in world space.
			/// </summary>
			public Vector3 swingDirection {
				get {
					return transform.rotation * localSwingAxis;
				}
			}

			public void StoreDefaultLocalState() {
				defaultLocalPosition = transform.localPosition;
				defaultLocalRotation = transform.localRotation;
			}
			
			public void FixTransform(bool position) {
				if (position) transform.localPosition = defaultLocalPosition;
				transform.localRotation = defaultLocalRotation;
			}
			
			#region Reading
			
			/*
			 * Does this bone have a node in the IK Solver?
			 * */
			public bool isNodeBone {
				get {
					return nodeIndex != -1;
					//return node != null;
				}
			}
			
			/*
			 * Calculate length of the bone
			 * */
			public void SetLength(BoneMap nextBone) {
				length = Vector3.Distance(transform.position, nextBone.transform.position);
			}
			
			/*
			 * Sets the direction to the swing target in local space
			 * */
			public void SetLocalSwingAxis(BoneMap swingTarget) {
				SetLocalSwingAxis(swingTarget, this);
			}
			
			/*
			 * Sets the direction to the swing target in local space
			 * */
			public void SetLocalSwingAxis(BoneMap bone1, BoneMap bone2) {
				localSwingAxis = Quaternion.Inverse(transform.rotation) * (bone1.transform.position - bone2.transform.position);
			}
			
			/*
			 * Sets the direction to the twist target in local space
			 * */
			public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection) {
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				localTwistAxis = Quaternion.Inverse(transform.rotation) * twistDirection;
			}

			/*
			 * Sets the 3 points defining a plane for this bone
			 * */
			public void SetPlane(IKSolverFullBody solver, Transform planeBone1, Transform planeBone2, Transform planeBone3) {
				this.planeBone1 = planeBone1;
				this.planeBone2 = planeBone2;
				this.planeBone3 = planeBone3;

				solver.GetChainAndNodeIndexes(planeBone1, out plane1ChainIndex, out plane1NodeIndex);
				solver.GetChainAndNodeIndexes(planeBone2, out plane2ChainIndex, out plane2NodeIndex);
				solver.GetChainAndNodeIndexes(planeBone3, out plane3ChainIndex, out plane3NodeIndex);

				//this.planeNode1 = planeNode1;
				//this.planeNode2 = planeNode2;
				//this.planeNode3 = planeNode3;
				
				UpdatePlane(true, true);
			}
			
			/*
			 * Updates the 3 plane points
			 * */
			public void UpdatePlane(bool rotation, bool position) {
				Quaternion t = lastAnimatedTargetRotation;

				if (rotation) defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(transform.rotation, t);
				if (position) planePosition = Quaternion.Inverse(t) * (transform.position - planeBone1.position);
			}
			
			/*
			 * Sets the virtual position for this bone
			 * */
			public void SetIKPosition() {
				ikPosition = transform.position;
			}

			/*
			 * Stores the current rotation for later use.
			 * */
			public void MaintainRotation() {
				maintainRotation = transform.rotation;
			}
			
			#endregion Reading
			
			#region Writing
			
			/*
			 * Moves the bone to its virtual position
			 * */
			public void SetToIKPosition() {
				transform.position = ikPosition;
			}
			
			/*
			 * Moves the bone to the solver position of it's node
			 * */
			public void FixToNode(IKSolverFullBody solver, float weight, IKSolver.Node fixNode = null) {
				if (fixNode == null) fixNode = solver.GetNode(chainIndex, nodeIndex);

				if (weight >= 1f) {
					transform.position = fixNode.solverPosition;
					return;
				}

				transform.position = Vector3.Lerp(transform.position, fixNode.solverPosition, weight);
			}
			
			/*
			 * Gets the bone's position relative to it's 3 plane nodes
			 * */
			public Vector3 GetPlanePosition(IKSolverFullBody solver) {
				return solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition + (GetTargetRotation(solver) * planePosition);
				//return planeNode1.solverPosition + (targetRotation * planePosition);
			}
			
			/*
			 * Positions the bone relative to it's 3 plane nodes
			 * */
			public void PositionToPlane(IKSolverFullBody solver) {
				transform.position = GetPlanePosition(solver);
			}
			
			/*
			 * Rotates the bone relative to it's 3 plane nodes
			 * */
			public void RotateToPlane(IKSolverFullBody solver, float weight) {
				Quaternion r = GetTargetRotation(solver) * defaultLocalTargetRotation;

				if (weight >= 1f) {
					transform.rotation = r;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, r, weight);
			}

			/*
			 * Swings to the swing target
			 * */
			public void Swing(Vector3 swingTarget, float weight) {
				Swing(swingTarget, transform.position, weight);
			}
			
			/*
			 * Swings to a direction from pos2 to pos1
			 * */
			public void Swing(Vector3 pos1, Vector3 pos2, float weight) {
				Quaternion r = Quaternion.FromToRotation(transform.rotation * localSwingAxis, pos1 - pos2) * transform.rotation;

				if (weight >= 1f) {
					transform.rotation = r;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, r, weight);
			}
			
			/*
			 * Twists to the twist target
			 * */
			public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight) {
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);

				Quaternion r = Quaternion.FromToRotation(transform.rotation * localTwistAxis, twistDirection) * transform.rotation;

				if (weight >= 1f) {
					transform.rotation = r;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, r, weight);
			}

			/*
			 * Rotates back to the last animated local rotation
			 * */
			public void RotateToMaintain(float weight) {
				if (weight <= 0f) return;

				transform.rotation = Quaternion.Lerp(transform.rotation, maintainRotation, weight);
			}
			
			/*
			 * Rotates to match the effector rotation
			 * */
			public void RotateToEffector(IKSolverFullBody solver, float weight) {
				if (!isNodeBone) return;
				float w = weight * solver.GetNode(chainIndex, nodeIndex).effectorRotationWeight;
				if (w <= 0f) return;

				if (w >= 1f) {
					transform.rotation = solver.GetNode(chainIndex, nodeIndex).solverRotation;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, solver.GetNode(chainIndex, nodeIndex).solverRotation, w);
			}
			
			#endregion Writing
			
			/*
			 * Rotation of plane nodes in the solver
			 * */
			private Quaternion GetTargetRotation(IKSolverFullBody solver) {
				Vector3 p1 = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition;
				Vector3 p2 = solver.GetNode(plane2ChainIndex, plane2NodeIndex).solverPosition;
				Vector3 p3 = solver.GetNode(plane3ChainIndex, plane3NodeIndex).solverPosition;

				if (p1 == p3) return Quaternion.identity;
				return Quaternion.LookRotation(p2 - p1, p3 - p1);

				//if (planeNode1.solverPosition == planeNode3.solverPosition) return Quaternion.identity;
				//return Quaternion.LookRotation(planeNode2.solverPosition - planeNode1.solverPosition, planeNode3.solverPosition - planeNode1.solverPosition);
			}
			
			/*
			 * Rotation of plane nodes in the animation
			 * */
			private Quaternion lastAnimatedTargetRotation {
				get {
					if (planeBone1.position == planeBone3.position) return Quaternion.identity;
					return Quaternion.LookRotation(planeBone2.position - planeBone1.position, planeBone3.position - planeBone1.position);
				}
			}
		}
		
		/// <summary>
		/// Determines whether this IKMapping is valid.
		/// </summary>
		public virtual bool IsValid(IKSolver solver, ref string message) {
			return true;
		}

		#endregion Main Interface
		
		public virtual void Initiate(IKSolverFullBody solver) {}
		
		protected bool BoneIsValid(Transform bone, IKSolver solver, ref string message, Warning.Logger logger = null) {
			if (bone == null) {
				message = "IKMappingLimb contains a null reference.";
				if (logger != null) logger(message);
				return false;
			}
			if (solver.GetPoint(bone) == null) {
				message = "IKMappingLimb is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.";
				if (logger != null) logger(message);
				return false;
			}
			return true;
		}

		/*
		 * Interpolates the joint position to match the bone's length
		*/
		protected Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length) {
			return pos2 + (pos1 - pos2).normalized * length;
		}
	}
}
