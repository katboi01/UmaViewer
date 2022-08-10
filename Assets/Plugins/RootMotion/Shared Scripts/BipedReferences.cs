using UnityEngine;
using System.Collections;
using System;

namespace RootMotion {

	/// <summary>
	/// Contains references to bones common to all biped characters.
	/// </summary>
	[System.Serializable]
	public class BipedReferences {
		
		#region Main Interface
		
		/// <summary>
		/// The root transform is the parent of all the biped's bones and should be located at ground level.
		/// </summary>
		public Transform root;
		/// <summary>
		/// The pelvis (hip) bone.
		/// </summary>
		public Transform pelvis;
		/// <summary>
		/// The first bone of the left leg.
		/// </summary>
		public Transform leftThigh;
		/// <summary>
		/// The second bone of the left leg.
		/// </summary>
		public Transform leftCalf;
		/// <summary>
		/// The third bone of the left leg.
		/// </summary>
		public Transform leftFoot;
		/// <summary>
		/// The first bone of the right leg.
		/// </summary>
		public Transform rightThigh;
		/// <summary>
		/// The second bone of the right leg.
		/// </summary>
		public Transform rightCalf;
		/// <summary>
		/// The third bone of the right leg.
		/// </summary>
		public Transform rightFoot;
		/// <summary>
		/// The first bone of the left arm.
		/// </summary>
		public Transform leftUpperArm;
		/// <summary>
		/// The second bone of the left arm.
		/// </summary>
		public Transform leftForearm;
		/// <summary>
		/// The third bone of the left arm.
		/// </summary>
		public Transform leftHand;
		/// <summary>
		/// The first bone of the right arm.
		/// </summary>
		public Transform rightUpperArm;
		/// <summary>
		/// The second bone of the right arm.
		/// </summary>
		public Transform rightForearm;
		/// <summary>
		/// The third bone of the right arm.
		/// </summary>
		public Transform rightHand;
		/// <summary>
		/// The head.
		/// </summary>
		public Transform head;
		/// <summary>
		/// The spine hierarchy. Should not contain any bone deeper in the hierarchy than the arms (neck or head).
		/// </summary>
		public Transform[] spine = new Transform[0];
		/// <summary>
		/// The eyes.
		/// </summary>
		public Transform[] eyes = new Transform[0];

		/// <summary>
		/// Check for null references.
		/// </summary>
		public virtual bool isFilled {
			get {
				if (root == null) return false;
				if (pelvis == null) return false;
				if (leftThigh == null || leftCalf == null || leftFoot == null) return false;
				if (rightThigh == null || rightCalf == null || rightFoot == null) return false;
				if (leftUpperArm == null || leftForearm == null || leftHand == null) return false;
				if (rightUpperArm == null || rightForearm == null || rightHand == null) return false;
					
				foreach (Transform s in spine) if (s == null) return false;
				foreach (Transform eye in eyes) if (eye == null) return false;
				return true;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="BipedReferences"/> is empty.
		/// </summary>
		public bool isEmpty {
			get {
				return IsEmpty(true);
			}
		}	
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="BipedReferences"/> is empty. If includeRoot is false, returns true(is empty) even if root Transform has been assigned.
		/// </summary>
		public virtual bool IsEmpty(bool includeRoot) {
			if (includeRoot && root != null) return false;
			if (pelvis != null || head != null) return false;
			if (leftThigh != null || leftCalf != null || leftFoot != null) return false;
			if (rightThigh != null || rightCalf != null || rightFoot != null) return false;
			if (leftUpperArm != null || leftForearm != null || leftHand != null) return false;
			if (rightUpperArm != null || rightForearm != null || rightHand != null) return false;
				
			foreach (Transform s in spine) if (s != null) return false;
			foreach (Transform eye in eyes) if (eye != null) return false;
			return true;
		}

		/// <summary>
		/// Returns true if the References contain the specified Transform
		/// </summary>
		public virtual bool Contains(Transform t, bool ignoreRoot = false) {
			if (!ignoreRoot && root == t) return true;
			if (pelvis == t) return true;
			if (leftThigh == t) return true;
			if (leftCalf == t) return true;
			if (leftFoot == t) return true;
			if (rightThigh == t) return true;
			if (rightCalf == t) return true;
			if (rightFoot == t) return true;
			if (leftUpperArm == t) return true;
			if (leftForearm == t) return true;
			if (leftHand == t) return true;
			if (rightUpperArm == t) return true;
			if (rightForearm == t) return true;
			if (rightHand == t) return true;
			if (head == t) return true;

			foreach (Transform s in spine) if (s == t) return true;
			foreach (Transform e in eyes) if (e == t) return true;
 
			return false;
		}

		/// <summary>
		/// Params for automatic biped recognition. (Using a struct here because I might need to add more parameters in the future).
		/// </summary>
		public struct AutoDetectParams {
			
			/// <summary>
			/// Should the immediate parent of the legs be included in the spine?.
			/// </summary>
			public bool legsParentInSpine;
			public bool includeEyes;
			
			public AutoDetectParams(bool legsParentInSpine, bool includeEyes) {
				this.legsParentInSpine = legsParentInSpine;
				this.includeEyes = includeEyes;
			}
			
			public static AutoDetectParams Default {
				get {
					return new AutoDetectParams(true, true);
				}
			}
		}
		
		/// <summary>
		/// Automatically detects biped bones. Returns true if a valid biped has been referenced.
		/// </summary>
		public static bool AutoDetectReferences(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams) {
			if (references == null) references = new BipedReferences();
			references.root = root;

			// If that failed try the Animator
			var animator = root.GetComponent<Animator>();
			if (animator != null && animator.isHuman) {
				AssignHumanoidReferences(ref references, animator, autoDetectParams);
				return true; // Assume humanoids are always valid
			}

			// Try with naming and hierarchy first
			DetectReferencesByNaming(ref references, root, autoDetectParams);

			Warning.logged = false;

			if (!references.isFilled) {
				Warning.Log("BipedReferences contains one or more missing Transforms.", root, true);
				return false;
			}

			string message = "";
			if (SetupError(references, ref message)) {
				Warning.Log(message, references.root, true);
				return false;
			}

			if (SetupWarning(references, ref message)) {
				Warning.Log(message, references.root, true);
			}
			
			return true;
		}
		
		/// <summary>
		/// Detects the references based on naming and hierarchy.
		/// </summary>
		public static void DetectReferencesByNaming(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams) {
			if (references == null) references = new BipedReferences();

			Transform[] children = root.GetComponentsInChildren<Transform>();
			
			// Find limbs
			DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Left, ref references.leftUpperArm, ref references.leftForearm, ref references.leftHand, children);
			DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Right, ref references.rightUpperArm, ref references.rightForearm, ref references.rightHand, children);
			DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Left, ref references.leftThigh, ref references.leftCalf, ref references.leftFoot, children);
			DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Right, ref references.rightThigh, ref references.rightCalf, ref references.rightFoot, children);
			
			// Find head bone
			references.head = BipedNaming.GetBone(children, BipedNaming.BoneType.Head);
			
			// Find Pelvis
			references.pelvis = BipedNaming.GetNamingMatch(children, BipedNaming.pelvis);
			
			// If pelvis is not an ancestor of a leg, it is not a valid pelvis
			if (references.pelvis == null || !Hierarchy.IsAncestor(references.leftThigh, references.pelvis)) {
				if (references.leftThigh != null) references.pelvis = references.leftThigh.parent;
			}
			
			// Find spine and head bones
			if (references.leftUpperArm != null && references.rightUpperArm != null && references.pelvis != null && references.leftThigh != null) {
				Transform neck = Hierarchy.GetFirstCommonAncestor(references.leftUpperArm, references.rightUpperArm);

				if (neck != null) {
					Transform[] inverseSpine = new Transform[1] { neck };
					Hierarchy.AddAncestors(inverseSpine[0], references.pelvis, ref inverseSpine);
					
					references.spine = new Transform[0];
					for (int i = inverseSpine.Length - 1; i > -1; i--) {
						if (AddBoneToSpine(inverseSpine[i], ref references, autoDetectParams)) {
							Array.Resize(ref references.spine, references.spine.Length + 1);
							references.spine[references.spine.Length - 1] = inverseSpine[i];
						}
					}
					
					// Head
					if (references.head == null) {
						for (int i = 0; i < neck.childCount; i++) {
							Transform child = neck.GetChild(i);
							
							if (!Hierarchy.ContainsChild(child, references.leftUpperArm) && !Hierarchy.ContainsChild(child, references.rightUpperArm)) {
								references.head = child;
								break;
							}
						}
					}
				}
			}
			
			// Find eye bones
			Transform[] eyes = BipedNaming.GetBonesOfType(BipedNaming.BoneType.Eye, children);
			references.eyes = new Transform[0];
			
			if (autoDetectParams.includeEyes) {
				for (int i = 0; i < eyes.Length; i++) {
					if (AddBoneToEyes(eyes[i], ref references, autoDetectParams)) {
						Array.Resize(ref references.eyes, references.eyes.Length + 1);
						references.eyes[references.eyes.Length - 1] = eyes[i];
					}
				}
			}
		}
		
		/// <summary>
		/// Fills in BipedReferences using Animator.GetBoneTransform().
		/// </summary>
		public static void AssignHumanoidReferences(ref BipedReferences references, Animator animator, AutoDetectParams autoDetectParams) {
			if (references == null) references = new BipedReferences();

			if (animator == null || !animator.isHuman) return;
			
			references.spine = new Transform[0];
			references.eyes = new Transform[0];
			
			references.head = animator.GetBoneTransform(HumanBodyBones.Head);
			
			references.leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			references.leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			references.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			
			references.rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			references.rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			references.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
			
			references.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			references.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			references.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			
			references.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			references.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			references.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
			
			references.pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
			
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Spine));
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Chest));
			
			// Make sure the neck bone is not above the arms
			if (references.leftUpperArm != null) {
				if (!IsNeckBone(animator.GetBoneTransform(HumanBodyBones.Neck), references.leftUpperArm)) AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Neck));
			}
			
			if (autoDetectParams.includeEyes) {
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform(HumanBodyBones.LeftEye));
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform(HumanBodyBones.RightEye));
			}
		}

		/// <summary>
		/// Checks the setup for definite problems.
		/// </summary>
		public static bool SetupError(BipedReferences references, ref string errorMessage) {
			if (!references.isFilled) {
				errorMessage = "BipedReferences contains one or more missing Transforms.";
				return true;
			}
			
			if (LimbError(references.leftThigh, references.leftCalf, references.leftFoot, ref errorMessage)) return true;
			if (LimbError(references.rightThigh, references.rightCalf, references.rightFoot, ref errorMessage)) return true;
			if (LimbError(references.leftUpperArm, references.leftForearm, references.leftHand, ref errorMessage)) return true;
			if (LimbError(references.rightUpperArm, references.rightForearm, references.rightHand, ref errorMessage)) return true;
			if (SpineError(references, ref errorMessage)) return true;
			if (EyesError(references, ref errorMessage)) return true;
			
			return false;
		}

		/// <summary>
		/// Checks the setup for possible problems.
		/// </summary>
		public static bool SetupWarning(BipedReferences references, ref string warningMessage) {
			if (LimbWarning(references.leftThigh, references.leftCalf, references.leftFoot, ref warningMessage)) return true;
			if (LimbWarning(references.rightThigh, references.rightCalf, references.rightFoot, ref warningMessage)) return true;
			if (LimbWarning(references.leftUpperArm, references.leftForearm, references.leftHand, ref warningMessage)) return true;
			if (LimbWarning(references.rightUpperArm, references.rightForearm, references.rightHand, ref warningMessage)) return true;
			if (SpineWarning(references, ref warningMessage)) return true;
			if (EyesWarning(references, ref warningMessage)) return true;
			if (RootHeightWarning(references, ref warningMessage)) return true;
			if (FacingAxisWarning(references, ref warningMessage)) return true;
			
			return false;
		}

		
		#endregion Main Interface

		// Determines whether a Transform is above the arms
		private static bool IsNeckBone(Transform bone, Transform leftUpperArm) {
			if (leftUpperArm.parent != null && leftUpperArm.parent == bone) return false;
			if (Hierarchy.IsAncestor(leftUpperArm, bone)) return false;
			return true;
		}

		// Determines whether a bone is valid for being added into the eyes array
		private static bool AddBoneToEyes(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams) {
			if (references.head != null) {
				if (!Hierarchy.IsAncestor(bone, references.head)) return false;
			}
			
			if (bone.GetComponent<SkinnedMeshRenderer>() != null) return false;
			
			return true;
		}
		
		// Determines whether a bone is valid for being added into the spine
		private static bool AddBoneToSpine(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams) {
			if (bone == references.root) return false;
			
			bool isLegsParent = bone == references.leftThigh.parent;
			if (isLegsParent && !autoDetectParams.legsParentInSpine) return false;
			
			if (references.pelvis != null) {
				if (bone == references.pelvis) return false;
				if (Hierarchy.IsAncestor(references.pelvis, bone)) return false;
			}
			
			return true;
		}
		
		// Tries to guess the limb bones based on naming
		private static void DetectLimb(BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide, ref Transform firstBone, ref Transform secondBone, ref Transform lastBone, Transform[] transforms) {
			Transform[] limb = BipedNaming.GetBonesOfTypeAndSide(boneType, boneSide, transforms);
			
			if (limb.Length < 3) {
				//Warning.Log("Unable to detect biped bones by bone names. Please manually assign bone references.", firstBone, true);
				return;
			}
			
			// Standard biped characters
			if (limb.Length == 3) {
				firstBone = limb[0];
				secondBone = limb[1];
				lastBone = limb[2];
			}
			
			// For Bootcamp soldier type of characters with more than 3 limb bones
			if (limb.Length > 3) {
				firstBone = limb[0];
				secondBone = limb[2];
				lastBone = limb[limb.Length - 1];
			}
		}
		
		// Adds transform to hierarchy if not null
		private static void AddBoneToHierarchy(ref Transform[] bones, Transform transform) {
			if (transform == null) return;
			
			Array.Resize(ref bones, bones.Length + 1);
			bones[bones.Length - 1] = transform;
		}
		
		// Check if the limb is properly set up
		private static bool LimbError(Transform bone1, Transform bone2, Transform bone3, ref string errorMessage) {
			if (bone1 == null) {
				errorMessage = "Bone 1 of a BipedReferences limb is null.";
				return true;
			}

			if (bone2 == null) {
				errorMessage = "Bone 2 of a BipedReferences limb is null.";
				return true;
			}

			if (bone3 == null) {
				errorMessage = "Bone 3 of a BipedReferences limb is null.";
				return true;
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(new Transform[3] { bone1, bone2, bone3 });
			if (duplicate != null) {
				errorMessage = duplicate.name + " is represented multiple times in the same BipedReferences limb.";
				return true;
			}

			if (bone2.position == bone1.position) {
				errorMessage = "Second bone's position equals first bone's position in the biped's limb.";
				return true;
			}
			
			if (bone3.position == bone2.position) {
				errorMessage = "Third bone's position equals second bone's position in the biped's limb.";
				return true;
			}
			
			if (!Hierarchy.HierarchyIsValid(new Transform[3] { bone1, bone2, bone3 })) {
				errorMessage = "BipedReferences limb hierarchy is invalid. Bone transforms in a limb do not belong to the same ancestry. Please make sure the bones are parented to each other. " + 
					"Bones: " + bone1.name + ", " + bone2.name + ", " + bone3.name;

				return true;
			}
			
			return false;
		}

		// Check if the limb is properly set up
		private static bool LimbWarning(Transform bone1, Transform bone2, Transform bone3, ref string warningMessage) {
			Vector3 cross = Vector3.Cross(bone2.position - bone1.position, bone3.position - bone1.position);
			
			if (cross == Vector3.zero) {
				warningMessage = "BipedReferences limb is completely stretched out in the initial pose. IK solver can not calculate the default bend plane for the limb. " +
					"Please make sure you character's limbs are at least slightly bent in the initial pose. " +
						"First bone: " + bone1.name + ", second bone: " + bone2.name + ".";

				return true;
			}
			
			return false;
		}
		
		// Check if spine is properly set up
		private static bool SpineError(BipedReferences references, ref string errorMessage) {
			// No spine might be a valid setup in some cases
			if (references.spine.Length == 0) return false;

			for (int i = 0; i < references.spine.Length; i++) {
				if (references.spine[i] == null) {
					errorMessage = "BipedReferences spine bone at index " + i + " is null.";
					return true;
				}
			}

			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(references.spine);
			if (duplicate != null) {
				errorMessage = duplicate.name + " is represented multiple times in BipedReferences spine.";
				return true;
			}
			
			if (!Hierarchy.HierarchyIsValid(references.spine)) {
				errorMessage = "BipedReferences spine hierarchy is invalid. Bone transforms in the spine do not belong to the same ancestry. Please make sure the bones are parented to each other.";
				return true;
			}
			
			for (int i = 0; i < references.spine.Length; i++) {
				bool matchesParentPosition = false;
				if (i == 0 && references.spine[i].position == references.pelvis.position) matchesParentPosition = true;
				if (i != 0 && references.spine.Length > 1 && references.spine[i].position == references.spine[i - 1].position) matchesParentPosition = true;

				if (matchesParentPosition) {
					errorMessage = "Biped's spine bone nr " + i + " position is the same as it's parent spine/pelvis bone's position. Please remove this bone from the spine.";
					return true;
				}
			}
			
			return false;
		}

		// Check if spine is properly set up
		private static bool SpineWarning(BipedReferences references, ref string warningMessage) {
			// Maybe need to add something here in the future
			return false;
		}
		
		// Check if eyes are properly set up
		private static bool EyesError(BipedReferences references, ref string errorMessage) {
			// No eyes might be a valid setup
			if (references.eyes.Length == 0) return false;

			for (int i = 0; i < references.eyes.Length; i++) {
				if (references.eyes[i] == null) {
					errorMessage = "BipedReferences eye bone at index " + i + " is null.";
					return true;
				}
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(references.eyes);
			if (duplicate != null) {
				errorMessage = duplicate.name + " is represented multiple times in BipedReferences eyes.";
				return true;
			}
			
			return false;
		}

		// Check if eyes are properly set up
		private static bool EyesWarning(BipedReferences references, ref string warningMessage) {
			// Maybe need to add something here in the future
			return false;
		}
		
		// Check if BipedIK transform position is at the character's feet
		private static bool RootHeightWarning(BipedReferences references, ref string warningMessage) {
			if (references.head == null) return false;
			
			float headHeight = GetVerticalOffset(references.head.position, references.leftFoot.position, references.root.rotation);
			float rootHeight = GetVerticalOffset(references.root.position, references.leftFoot.position, references.root.rotation);
			
			if (rootHeight / headHeight > 0.2f) {
				warningMessage = "Biped's root Transform's position should be at ground level relative to the character (at the character's feet not at it's pelvis).";
				return true;
			}
			
			return false;
		}
		
		// Check if the character is facing the correct axis
		private static bool FacingAxisWarning(BipedReferences references, ref string warningMessage) {
			Vector3 handsLeftToRight = references.rightHand.position - references.leftHand.position;
			Vector3 feetLeftToRight = references.rightFoot.position - references.leftFoot.position;
			
			float dotHands = Vector3.Dot(handsLeftToRight.normalized, references.root.right);
			float dotFeet = Vector3.Dot(feetLeftToRight.normalized, references.root.right);
			
			if (dotHands < 0 || dotFeet < 0) {
				warningMessage = "Biped does not seem to be facing it's forward axis. " +
					"Please make sure that in the initial pose the character is facing towards the positive Z axis of the Biped root gameobject.";
				return true;
			}
			
			return false;
		}
		
		// Gets vertical offset relative to a rotation
		private static float GetVerticalOffset(Vector3 p1, Vector3 p2, Quaternion rotation) {
			Vector3 v = Quaternion.Inverse(rotation) * (p1 - p2);
			return v.y;
		}
	}
}
