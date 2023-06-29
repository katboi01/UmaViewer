using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Full body biped IK effector types.
	/// </summary>
	[System.Serializable]
	public enum FullBodyBipedEffector {
		Body,
		LeftShoulder,
		RightShoulder,
		LeftThigh,
		RightThigh,
		LeftHand,
		RightHand,
		LeftFoot,
		RightFoot
	}

	/// <summary>
	/// Full body biped IK chain types.
	/// </summary>
	[System.Serializable]
	public enum FullBodyBipedChain {
		LeftArm,
		RightArm,
		LeftLeg,
		RightLeg
	}

	/// <summary>
	/// FBIK solver specialized to biped characters.
	/// </summary>
	[System.Serializable]
	public class IKSolverFullBodyBiped : IKSolverFullBody {
		
		#region Main Interface
		
		/// <summary>
		/// The central root node (body).
		/// </summary>
		public Transform rootNode;
		/// <summary>
		/// The stiffness of spine constraints.
		/// </summary>
		[Range(0f, 1f)]
		public float spineStiffness = 0.5f;
		/// <summary>
		/// Weight of hand effectors pulling the body vertically (relative to root rotation).
		/// </summary>
		[Range(-1f, 1f)]
		public float pullBodyVertical = 0.5f;
		/// <summary>
		/// Weight of hand effectors pulling the body horizontally (relative to root rotation).
		/// </summary>
		[Range(-1f, 1f)]
		public float pullBodyHorizontal = 0f;
		
		/// <summary>
		/// Gets the body effector.
		/// </summary>
		public IKEffector bodyEffector { get { return GetEffector(FullBodyBipedEffector.Body); }}
		/// <summary>
		/// Gets the left shoulder effector.
		/// </summary>
		public IKEffector leftShoulderEffector { get { return GetEffector(FullBodyBipedEffector.LeftShoulder); }}
		/// <summary>
		/// Gets the right shoulder effector.
		/// </summary>
		public IKEffector rightShoulderEffector { get { return GetEffector(FullBodyBipedEffector.RightShoulder); }}
		/// <summary>
		/// Gets the left thigh effector.
		/// </summary>
		public IKEffector leftThighEffector { get { return GetEffector(FullBodyBipedEffector.LeftThigh); }}
		/// <summary>
		/// Gets the right thigh effector.
		/// </summary>
		public IKEffector rightThighEffector { get { return GetEffector(FullBodyBipedEffector.RightThigh); }}
		/// <summary>
		/// Gets the left hand effector.
		/// </summary>
		public IKEffector leftHandEffector { get { return GetEffector(FullBodyBipedEffector.LeftHand); }}
		/// <summary>
		/// Gets the right hand effector.
		/// </summary>
		public IKEffector rightHandEffector { get { return GetEffector(FullBodyBipedEffector.RightHand); }}
		/// <summary>
		/// Gets the left foot effector.
		/// </summary>
		public IKEffector leftFootEffector { get { return GetEffector(FullBodyBipedEffector.LeftFoot); }}
		/// <summary>
		/// Gets the right foot effector.
		/// </summary>
		public IKEffector rightFootEffector { get { return GetEffector(FullBodyBipedEffector.RightFoot); }}
		
		/// <summary>
		/// Gets the left arm chain.
		/// </summary>
		public FBIKChain leftArmChain { get { return chain[1]; }}
		/// <summary>
		/// Gets the right arm chain.
		/// </summary>
		public FBIKChain rightArmChain { get { return chain[2]; }}
		/// <summary>
		/// Gets the left leg chain.
		/// </summary>
		public FBIKChain leftLegChain { get { return chain[3]; }}
		/// <summary>
		/// Gets the right leg chain.
		/// </summary>
		public FBIKChain rightLegChain { get { return chain[4]; }}

		/// <summary>
		/// Gets the left arm IK mapping.
		/// </summary>
		public IKMappingLimb leftArmMapping { get { return limbMappings[0]; }}
		/// <summary>
		/// Gets the right arm IK mapping.
		/// </summary>
		public IKMappingLimb rightArmMapping { get { return limbMappings[1]; }}
		/// <summary>
		/// Gets the left leg IK mapping.
		/// </summary>
		public IKMappingLimb leftLegMapping { get { return limbMappings[2]; }}
		/// <summary>
		/// Gets the right leg IK mapping.
		/// </summary>
		public IKMappingLimb rightLegMapping { get { return limbMappings[3]; }}
		/// <summary>
		/// Gets the head IK mapping.
		/// </summary>
		public IKMappingBone headMapping { get { return boneMappings[0]; }}

		/// <summary>
		/// Sets chain weights for the specified chain.
		/// </summary>
		public void SetChainWeights(FullBodyBipedChain c, float pull, float reach = 0f) {
			GetChain(c).pull = pull;
			GetChain(c).reach = reach;
		}
		
		/// <summary>
		/// Sets effector weights for the specified effector.
		/// </summary>
		public void SetEffectorWeights(FullBodyBipedEffector effector, float positionWeight, float rotationWeight) {
			GetEffector(effector).positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			GetEffector(effector).rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}
		
		/// <summary>
		///  Gets the chain of a limb.
		/// </summary>
		public FBIKChain GetChain(FullBodyBipedChain c) {
			switch(c) {
			case FullBodyBipedChain.LeftArm: return chain[1];
			case FullBodyBipedChain.RightArm: return chain[2];
			case FullBodyBipedChain.LeftLeg: return chain[3];
			case FullBodyBipedChain.RightLeg: return chain[4];
			}
			return null;
		}
		
		/// <summary>
		///  Gets the chain of the specified effector. 
		/// </summary>
		public FBIKChain GetChain(FullBodyBipedEffector effector) {
			switch(effector) {
			case FullBodyBipedEffector.Body: return chain[0];
			case FullBodyBipedEffector.LeftShoulder: return chain[1];
			case FullBodyBipedEffector.RightShoulder: return chain[2];
			case FullBodyBipedEffector.LeftThigh: return chain[3];
			case FullBodyBipedEffector.RightThigh: return chain[4];
			case FullBodyBipedEffector.LeftHand: return chain[1];
			case FullBodyBipedEffector.RightHand: return chain[2];
			case FullBodyBipedEffector.LeftFoot: return chain[3];
			case FullBodyBipedEffector.RightFoot: return chain[4];
			}
			return null;
		}
		
		/// <summary>
		///  Gets the effector of type. 
		/// </summary>
		public IKEffector GetEffector(FullBodyBipedEffector effector) {
			switch(effector) {
			case FullBodyBipedEffector.Body: return effectors[0];
			case FullBodyBipedEffector.LeftShoulder: return effectors[1];
			case FullBodyBipedEffector.RightShoulder: return effectors[2];
			case FullBodyBipedEffector.LeftThigh: return effectors[3];
			case FullBodyBipedEffector.RightThigh: return effectors[4];
			case FullBodyBipedEffector.LeftHand: return effectors[5];
			case FullBodyBipedEffector.RightHand: return effectors[6];
			case FullBodyBipedEffector.LeftFoot: return effectors[7];
			case FullBodyBipedEffector.RightFoot: return effectors[8];
			}
			return null;
		}

		/// <summary>
		///  Gets the effector of type. 
		/// </summary>
		public IKEffector GetEndEffector(FullBodyBipedChain c) {
			switch(c) {
			case FullBodyBipedChain.LeftArm: return effectors[5];
			case FullBodyBipedChain.RightArm: return effectors[6];
			case FullBodyBipedChain.LeftLeg: return effectors[7];
			case FullBodyBipedChain.RightLeg: return effectors[8];
			}
			return null;
		}
		
		/// <summary>
		/// Gets the limb mapping for the limb.
		/// </summary>
		public IKMappingLimb GetLimbMapping(FullBodyBipedChain chain) {
			switch(chain) {
			case FullBodyBipedChain.LeftArm: return limbMappings[0];
			case FullBodyBipedChain.RightArm: return limbMappings[1];
			case FullBodyBipedChain.LeftLeg: return limbMappings[2];
			case FullBodyBipedChain.RightLeg: return limbMappings[3];
			}
			return null;
		}

		/// <summary>
		/// Gets the limb mapping for the effector type.
		/// </summary>
		public IKMappingLimb GetLimbMapping(FullBodyBipedEffector effector) {
			switch(effector) {
			case FullBodyBipedEffector.LeftShoulder: return limbMappings[0];
			case FullBodyBipedEffector.RightShoulder: return limbMappings[1];
			case FullBodyBipedEffector.LeftThigh: return limbMappings[2];
			case FullBodyBipedEffector.RightThigh: return limbMappings[3];
			case FullBodyBipedEffector.LeftHand: return limbMappings[0];
			case FullBodyBipedEffector.RightHand: return limbMappings[1];
			case FullBodyBipedEffector.LeftFoot: return limbMappings[2];
			case FullBodyBipedEffector.RightFoot: return limbMappings[3];
			default: return null;
			}
		}
		
		/// <summary>
		/// Gets the spine mapping.
		/// </summary>
		public IKMappingSpine GetSpineMapping() {
			return spineMapping;
		}
		
		/// <summary>
		/// Gets the head mapping.
		/// </summary>
		public IKMappingBone GetHeadMapping() {
			return boneMappings[0];
		}
		
		/// <summary>
		/// Gets the bend constraint of a limb.
		/// </summary>
		public IKConstraintBend GetBendConstraint(FullBodyBipedChain limb) {
			switch(limb) {
			case FullBodyBipedChain.LeftArm: return chain[1].bendConstraint;
			case FullBodyBipedChain.RightArm: return chain[2].bendConstraint;
			case FullBodyBipedChain.LeftLeg: return chain[3].bendConstraint;
			case FullBodyBipedChain.RightLeg: return chain[4].bendConstraint;
			}
			return null;
		}
		
		public override bool IsValid(ref string message) {
			if (!base.IsValid(ref message)) return false;
			
			if (rootNode == null) {
				message = "Root Node bone is null. FBBIK will not initiate.";
				return false;
			}
			
			if (chain.Length != 5 ||
				chain[0].nodes.Length != 1 ||
			    chain[1].nodes.Length != 3 ||
			    chain[2].nodes.Length != 3 ||
			    chain[3].nodes.Length != 3 ||
			    chain[4].nodes.Length != 3 ||
				effectors.Length != 9 ||
				limbMappings.Length != 4
				) {
				message = "Invalid FBBIK setup. Please right-click on the component header and select 'Reinitiate'.";
				return false;
			}

			return true;
		}

		/// <summary>
		/// Sets up the solver to BipedReferences and reinitiates (if in runtime).
		/// </summary>
		/// <param name="references">Biped references.</param>
		/// <param name="rootNode">Root node (optional). if null, will try to detect the root node bone automatically. </param>
		public void SetToReferences(BipedReferences references, Transform rootNode = null) {
			root = references.root;

			if (rootNode == null) rootNode = DetectRootNodeBone(references);
			this.rootNode = rootNode;
			
			// Root Node
			if (chain == null || chain.Length != 5) chain = new FBIKChain[5];
			for (int i = 0; i < chain.Length; i++) {
				if (chain[i] == null) {
					chain[i] = new FBIKChain();
				}
			}

			chain[0].pin = 0f;
			chain[0].SetNodes(rootNode);
			chain[0].children = new int[4] { 1, 2, 3, 4 };
			
			// Left Arm
			chain[1].SetNodes(references.leftUpperArm, references.leftForearm, references.leftHand);
			
			// Right Arm
			chain[2].SetNodes(references.rightUpperArm, references.rightForearm, references.rightHand);
			
			// Left Leg
			chain[3].SetNodes(references.leftThigh, references.leftCalf, references.leftFoot);
			
			// Right Leg
			chain[4].SetNodes(references.rightThigh, references.rightCalf, references.rightFoot);
			
			// Effectors
			if (effectors.Length != 9) effectors = new IKEffector[9] {
				new IKEffector(), new IKEffector(), new IKEffector(), new IKEffector(), new IKEffector(), new IKEffector(), new IKEffector(), new IKEffector(), new IKEffector()
			};
			
			effectors[0].bone = rootNode;
			effectors[0].childBones = new Transform[2] { references.leftThigh, references.rightThigh };
			
			effectors[1].bone = references.leftUpperArm;
			effectors[2].bone = references.rightUpperArm;
			effectors[3].bone = references.leftThigh;
			effectors[4].bone = references.rightThigh;
			effectors[5].bone = references.leftHand;
			effectors[6].bone = references.rightHand;
			effectors[7].bone = references.leftFoot;
			effectors[8].bone = references.rightFoot;
			
			effectors[5].planeBone1 = references.leftUpperArm;
			effectors[5].planeBone2 = references.rightUpperArm;
			effectors[5].planeBone3 = rootNode;

			effectors[6].planeBone1 = references.rightUpperArm;
			effectors[6].planeBone2 = references.leftUpperArm;
			effectors[6].planeBone3 = rootNode;

			effectors[7].planeBone1 = references.leftThigh;
			effectors[7].planeBone2 = references.rightThigh;
			effectors[7].planeBone3 = rootNode;

			effectors[8].planeBone1 = references.rightThigh;
			effectors[8].planeBone2 = references.leftThigh;
			effectors[8].planeBone3 = rootNode;
			
			// Child Constraints
			chain[0].childConstraints = new FBIKChain.ChildConstraint[4] {
				new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightThigh, 0f, 1f),
				new FBIKChain.ChildConstraint(references.rightUpperArm, references.leftThigh, 0f, 1f),
				new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightUpperArm),
				new FBIKChain.ChildConstraint(references.leftThigh, references.rightThigh)
				
			};
			
			// IKMappingSpine
			Transform[] spineBones = new Transform[references.spine.Length + 1];
			spineBones[0] = references.pelvis;
			for (int i = 0; i < references.spine.Length; i++) {
				spineBones[i + 1] = references.spine[i];
			}
			
			if (spineMapping == null) {
				spineMapping = new IKMappingSpine();
				spineMapping.iterations = 3;
			}
			spineMapping.SetBones(spineBones, references.leftUpperArm, references.rightUpperArm, references.leftThigh, references.rightThigh);
			
			// IKMappingBone
			int boneMappingsCount = references.head != null? 1: 0;
			
			if (boneMappings.Length != boneMappingsCount) {
				boneMappings = new IKMappingBone[boneMappingsCount];
				for (int i = 0; i < boneMappings.Length; i++) {
					boneMappings[i] = new IKMappingBone();
				}
				if (boneMappingsCount == 1) boneMappings[0].maintainRotationWeight = 0f;
			}
			
			if (boneMappings.Length > 0) boneMappings[0].bone = references.head;
			
			// IKMappingLimb
			if (limbMappings.Length != 4) {
				limbMappings = new IKMappingLimb[4] {
					new IKMappingLimb(), new IKMappingLimb(), new IKMappingLimb(), new IKMappingLimb()
				};
				
				limbMappings[2].maintainRotationWeight = 1f;
				limbMappings[3].maintainRotationWeight = 1f;
			}
			
			limbMappings[0].SetBones(references.leftUpperArm, references.leftForearm, references.leftHand, GetLeftClavicle(references));
			limbMappings[1].SetBones(references.rightUpperArm, references.rightForearm, references.rightHand, GetRightClavicle(references));
			limbMappings[2].SetBones(references.leftThigh, references.leftCalf, references.leftFoot);
			limbMappings[3].SetBones(references.rightThigh, references.rightCalf, references.rightFoot);

			if (Application.isPlaying) Initiate(references.root);
		}

		/*
		 * Tries to guess which bone should be the root node
		 * */
		public static Transform DetectRootNodeBone(BipedReferences references) {
			if (!references.isFilled) return null;
			if (references.spine.Length < 1) return null;

			int spineLength = references.spine.Length;
			if (spineLength == 1) return references.spine[0];
			
			Vector3 hip = Vector3.Lerp(references.leftThigh.position, references.rightThigh.position, 0.5f);
			Vector3 neck = Vector3.Lerp(references.leftUpperArm.position, references.rightUpperArm.position, 0.5f);
			Vector3 toNeck = neck - hip;
			float toNeckMag = toNeck.magnitude;
			
			if (references.spine.Length < 2) return references.spine[0];

			int rootNodeBone = 0;

			for (int i = 1; i < spineLength; i++) {
				Vector3 hipToBone = references.spine[i].position - hip;
				Vector3 projection = Vector3.Project(hipToBone, toNeck);
				
				float dot = Vector3.Dot(projection.normalized, toNeck.normalized);
				if (dot > 0) {
					float mag = projection.magnitude / toNeckMag;
					if (mag < 0.5f) rootNodeBone = i;
				}
			}

			return references.spine[rootNodeBone];
		}

		/// <summary>
		/// Sets the bend directions of the limbs to the local axes specified by BipedLimbOrientations.
		/// </summary>
		public void SetLimbOrientations(BipedLimbOrientations o) {
			SetLimbOrientation(FullBodyBipedChain.LeftArm, o.leftArm);
			SetLimbOrientation(FullBodyBipedChain.RightArm, o.rightArm);
			SetLimbOrientation(FullBodyBipedChain.LeftLeg, o.leftLeg);
			SetLimbOrientation(FullBodyBipedChain.RightLeg, o.rightLeg);
		}

		#endregion Main Interface

		// Offset applied to the body effector by PullBody
		public Vector3 pullBodyOffset { get; private set; }

		/*
		 * Sets the bend direction of a limb to the local axes specified by the LimbOrientation.
		 * */
		private void SetLimbOrientation(FullBodyBipedChain chain, BipedLimbOrientations.LimbOrientation limbOrientation) {
			bool inverse = chain == FullBodyBipedChain.LeftArm || chain == FullBodyBipedChain.RightArm;
			
			if (inverse) {
				GetBendConstraint(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis, -limbOrientation.lastBoneLeftAxis);
				GetLimbMapping(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis);
			} else {
				GetBendConstraint(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis, limbOrientation.lastBoneLeftAxis);
				GetLimbMapping(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis);
			}
		}

		private static Transform GetLeftClavicle(BipedReferences references) {
			if (references.leftUpperArm == null) return null;
			if (!Contains(references.spine, references.leftUpperArm.parent)) return references.leftUpperArm.parent;
			return null;
		}
		
		private static Transform GetRightClavicle(BipedReferences references) {
			if (references.rightUpperArm == null) return null;
			if (!Contains(references.spine, references.rightUpperArm.parent)) return references.rightUpperArm.parent;
			return null;
		}
		
		private static bool Contains(Transform[] array, Transform transform) {
			foreach (Transform t in array) if (t == transform) return true;
			return false;
		}

		protected override void ReadPose() {
			// Set effectors to their targets
			for (int i = 0; i < effectors.Length; i++) effectors[i].SetToTarget();

			// Pulling the body with the hands
			PullBody();
			
			// Spine stiffness
			float s = Mathf.Clamp(1f - spineStiffness, 0f, 1f);
			chain[0].childConstraints[0].pushElasticity = s;
			chain[0].childConstraints[1].pushElasticity = s;

			base.ReadPose();
		}

		/*
		 * Pulling the body with the hands
		 * */
		private void PullBody() {
			if (iterations < 1) return;

			// Getting the body positionOffset
			if (pullBodyVertical != 0f || pullBodyHorizontal != 0f) {
				Vector3 offset = GetBodyOffset();

				pullBodyOffset = V3Tools.ExtractVertical(offset, root.up, pullBodyVertical) + V3Tools.ExtractHorizontal(offset, root.up, pullBodyHorizontal);
				bodyEffector.positionOffset += pullBodyOffset;
			}
		}

		/*
		 * Get pull offset of the body effector.
		 * */
		private Vector3 GetBodyOffset() {
			Vector3 offset = Vector3.zero + GetHandBodyPull(leftHandEffector, leftArmChain, Vector3.zero) * Mathf.Clamp(leftHandEffector.positionWeight, 0f, 1f);
			return offset + GetHandBodyPull(rightHandEffector, rightArmChain, offset) * Mathf.Clamp(rightHandEffector.positionWeight, 0f, 1f);
		}

		/*
		 * Get pull offset of a hand
		 * */
		private Vector3 GetHandBodyPull(IKEffector effector, FBIKChain arm, Vector3 offset) {
			// Get the vector from shoulder to hand effector
			Vector3 direction = effector.position - (arm.nodes[0].transform.position + offset);
			float armLength = arm.nodes[0].length + arm.nodes[1].length;
			
			// Find delta of effector distance and arm length
			float dirMag = direction.magnitude;

			if (dirMag < armLength) return Vector3.zero;
			float x = dirMag - armLength;

			return (direction / dirMag) * x;
		}

		private Vector3 offset;

		protected override void ApplyBendConstraints() {
			if (iterations > 0) {
				chain[1].bendConstraint.rotationOffset = leftHandEffector.planeRotationOffset;
				chain[2].bendConstraint.rotationOffset = rightHandEffector.planeRotationOffset;
				chain[3].bendConstraint.rotationOffset = leftFootEffector.planeRotationOffset;
				chain[4].bendConstraint.rotationOffset = rightFootEffector.planeRotationOffset;
			} else {
				offset = Vector3.Lerp(effectors[0].positionOffset, effectors[0].position - (effectors[0].bone.position + effectors[0].positionOffset), effectors[0].positionWeight);

				for (int i = 0; i < 5; i++) {
					effectors[i].GetNode(this).solverPosition += offset;
				}
			}

			base.ApplyBendConstraints(); 
		}

		protected override void WritePose() {
			if (iterations == 0) {
				spineMapping.spineBones[0].position += offset;
			}

			base.WritePose();
		}

	}
}
