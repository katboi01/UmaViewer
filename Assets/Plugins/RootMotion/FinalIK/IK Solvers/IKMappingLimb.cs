using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Maps a 3-segmented bone hierarchy to a node chain of an %IK Solver
	/// </summary>
	[System.Serializable]
	public class IKMappingLimb: IKMapping {
		
		#region Main Interface

		/// <summary>
		/// Limb Bone Map type
		/// </summary>
		[System.Serializable]
		public enum BoneMapType {
			Parent,
			Bone1,
			Bone2,
			Bone3
		}

		/// <summary>
		/// The optional parent bone (clavicle).
		/// </summary>
		public Transform parentBone;
		/// <summary>
		/// The first bone (upper arm or thigh).
		/// </summary>
		public Transform bone1;
		/// <summary>
		/// The second bone (forearm or calf).
		/// </summary>
		public Transform bone2;
		/// <summary>
		/// The third bone (hand or foot).
		/// </summary>
		public Transform bone3;
		/// <summary>
		/// The weight of maintaining the third bone's rotation as it was in the animation
		/// </summary>
		[Range(0f, 1f)]
		public float maintainRotationWeight;
		/// <summary>
		/// The weight of mapping the limb to it's IK pose. This can be useful if you want to disable the effect of IK for the limb.
		/// </summary>
		[Range(0f, 1f)]
		public float weight = 1f; // Added in 0.2
        /// <summary>
        /// Disable this to maintain original sampled rotations of the limb bones relative to each other.
        /// </summary>
        [System.NonSerializedAttribute] public bool updatePlaneRotations = true;

        /// <summary>
        /// Determines whether this IKMappingLimb is valid
        /// </summary>
        public override bool IsValid(IKSolver solver, ref string message) {
			if (!base.IsValid(solver, ref message)) return false;
			
			if (!BoneIsValid(bone1, solver, ref message)) return false;
			if (!BoneIsValid(bone2, solver, ref message)) return false;
			if (!BoneIsValid(bone3, solver, ref message)) return false;
			
			return true;
		}

		/// <summary>
		/// Gets the bone map of the specified bone.
		/// </summary>
		public BoneMap GetBoneMap(BoneMapType boneMap) {
			switch(boneMap) {
			case BoneMapType.Parent:
				if (parentBone == null) Warning.Log("This limb does not have a parent (shoulder) bone", bone1);
				return boneMapParent;
			case BoneMapType.Bone1: return boneMap1;
			case BoneMapType.Bone2: return boneMap2;
			default: return boneMap3;
			}
		}

		/// <summary>
		/// Makes the limb mapped to the specific local directions of the bones. Added in 0.3
		/// </summary>
		public void SetLimbOrientation(Vector3 upper, Vector3 lower) {
			boneMap1.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone1.rotation) * Quaternion.LookRotation(bone2.position - bone1.position, bone1.rotation * -upper));
			boneMap2.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone2.rotation) * Quaternion.LookRotation(bone3.position - bone2.position, bone2.rotation * -lower));
		}
		
		#endregion Main Interface
		
		private BoneMap boneMapParent = new BoneMap(), boneMap1 = new BoneMap(), boneMap2 = new BoneMap(), boneMap3 = new BoneMap();
		
		public IKMappingLimb() {}
		
		public IKMappingLimb(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null) {
			SetBones(bone1, bone2, bone3, parentBone);
		}
		
		public void SetBones(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null) {
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.bone3 = bone3;
			this.parentBone = parentBone;
		}
		
		public void StoreDefaultLocalState() {
			if (parentBone != null) boneMapParent.StoreDefaultLocalState();
			boneMap1.StoreDefaultLocalState();
			boneMap2.StoreDefaultLocalState();
			boneMap3.StoreDefaultLocalState();
		}
		
		public void FixTransforms() {
			if (parentBone != null) boneMapParent.FixTransform(false);
			boneMap1.FixTransform(true);
			boneMap2.FixTransform(false);
			boneMap3.FixTransform(false);
		}
		
		/*
		 * Initiating and setting defaults
		 * */
		public override void Initiate(IKSolverFullBody solver) {
			if (boneMapParent == null) boneMapParent = new BoneMap();
			if (boneMap1 == null) boneMap1 = new BoneMap();
			if (boneMap2 == null) boneMap2 = new BoneMap();
			if (boneMap3 == null) boneMap3 = new BoneMap();

			// Finding the nodes
			if (parentBone != null) boneMapParent.Initiate(parentBone, solver);
			boneMap1.Initiate(bone1, solver);
			boneMap2.Initiate(bone2, solver);
			boneMap3.Initiate(bone3, solver);

			// Define plane points for the bone maps
			boneMap1.SetPlane(solver, boneMap1.transform, boneMap2.transform, boneMap3.transform);
			boneMap2.SetPlane(solver, boneMap2.transform, boneMap3.transform, boneMap1.transform);

			// Find the swing axis for the parent bone
			if (parentBone != null) boneMapParent.SetLocalSwingAxis(boneMap1);
		}
		
		/*
		 * Presolving the bones and maintaining rotation
		 * */
		public void ReadPose() {
            boneMap1.UpdatePlane(updatePlaneRotations, true);
            boneMap2.UpdatePlane(updatePlaneRotations, false);

            // Clamping weights
            weight = Mathf.Clamp(weight, 0f, 1f);

			// Define plane points for the bone maps
			boneMap3.MaintainRotation();
		}

		public void WritePose(IKSolverFullBody solver, bool fullBody) {
			if (weight <= 0f) return;

			// Swing the parent bone to look at the first node's position
			if (fullBody) {
				if (parentBone != null) {
					boneMapParent.Swing(solver.GetNode(boneMap1.chainIndex, boneMap1.nodeIndex).solverPosition, weight);
					//boneMapParent.Swing(boneMap1.node.solverPosition, weight);
				}
				
				// Fix the first bone to it's node
				boneMap1.FixToNode(solver, weight);
			}

			// Rotate the 2 first bones to the plane points
			boneMap1.RotateToPlane(solver, weight);
			boneMap2.RotateToPlane(solver, weight);

			// Rotate the third bone to the rotation it had before solving
			boneMap3.RotateToMaintain(maintainRotationWeight * weight * solver.IKPositionWeight);
			
			// Rotate the third bone to the effector rotation
			boneMap3.RotateToEffector(solver, weight);
		}
	}
}
