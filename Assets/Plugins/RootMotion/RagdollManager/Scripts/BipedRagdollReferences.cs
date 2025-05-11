using UnityEngine;
using System.Collections;
using RootMotion;
using System.Collections.Generic;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Holds references to all Transforms required for a biped ragdoll.
	/// </summary>
	[System.Serializable]
	public struct BipedRagdollReferences {

		/// <summary>
		/// The root transform is the parent of all the biped's bones and should be located at ground level.
		/// </summary>
		public Transform root;
		/// <summary>
		/// The pelvis bone.
		/// </summary>
		public Transform hips;
		/// <summary>
		/// The middle spine bone.
		/// </summary>
		public Transform spine;
		/// <summary>
		/// The last spine bone.
		/// </summary>
		public Transform chest;
		/// <summary>
		/// The head.
		/// </summary>
		public Transform head;
		/// <summary>
		/// The first bone of the left leg.
		/// </summary>
		public Transform leftUpperLeg;
		/// <summary>
		/// The second bone of the left leg.
		/// </summary>
		public Transform leftLowerLeg;
		/// <summary>
		/// The third bone of the left leg.
		/// </summary>
		public Transform leftFoot;
		/// <summary>
		/// The first bone of the right leg.
		/// </summary>
		public Transform rightUpperLeg;
		/// <summary>
		/// The second bone of the right leg.
		/// </summary>
		public Transform rightLowerLeg;
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
		public Transform leftLowerArm;
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
		public Transform rightLowerArm;
		/// <summary>
		/// The third bone of the right arm.
		/// </summary>
		public Transform rightHand;

		public bool IsValid(ref string msg) {
			if (root == null ||
				hips == null ||
			    head == null ||
			    leftUpperArm == null ||
			    leftLowerArm == null ||
			    leftHand == null ||

			    rightUpperArm == null ||
			    rightLowerArm == null ||
			    rightHand == null ||

			    leftUpperLeg == null ||
			    leftLowerLeg == null ||
			    leftFoot == null ||

			    rightUpperLeg == null ||
			    rightLowerLeg == null ||
			    rightFoot == null
			    ) 
			{
				msg = "Invalid References, one or more Transforms missing.";
			    return false;
			}

			var array = new Transform[15] {
				root, hips, head, leftUpperArm, leftLowerArm, leftHand, rightUpperArm, rightLowerArm, rightHand, leftUpperLeg, leftLowerLeg, leftFoot, rightUpperLeg, rightLowerLeg, rightFoot
			};

			// Check if all are parented to the root
			for (int i = 1; i < array.Length; i++) {
				if (!IsChildRecursive(array[i], root)) {
					msg = "Invalid References, " + array[i].name + " is not in the Root's hierarchy.";
					return false;
				}
			}

			// Check for duplicates
			for (int i = 0; i < array.Length; i++) {
				for (int i2 = 0; i2 < array.Length; i2++) {
					if (i != i2 && array[i] == array[i2]) {
						msg = "Invalid References, " + array[i].name + " is represented more than once.";
						return false;
					}
				}
			}

			return true;
		}

		private bool IsChildRecursive(Transform t, Transform parent) {
			if (t.parent == parent) return true;
			if (t.parent != null) return IsChildRecursive(t.parent, parent);
			return false;
		}

		public bool IsEmpty(bool considerRoot) {
			if (considerRoot && root != null) return false;

			if (hips != null ||
			    head != null ||
			    spine != null ||
			    chest != null ||
			    leftUpperArm != null ||
			    leftLowerArm != null ||
			    leftHand != null ||
			    rightUpperArm != null ||
			    rightLowerArm != null ||
			    rightHand != null ||
			    
			    leftUpperLeg != null ||
			    leftLowerLeg != null ||
			    leftFoot != null ||
			    
			    rightUpperLeg != null ||
			    rightLowerLeg != null ||
			    rightFoot != null
			    ) return false;

			return true;
		}

		/// <summary>
		/// Returns true if the References contain the specified Transform
		/// </summary>
		public bool Contains(Transform t, bool ignoreRoot = false) {
			if (!ignoreRoot && root == t) return true;
			if (hips == t) return true;
			if (spine == t) return true;
			if (chest == t) return true;
			if (leftUpperLeg == t) return true;
			if (leftLowerLeg == t) return true;
			if (leftFoot == t) return true;
			if (rightUpperLeg == t) return true;
			if (rightLowerLeg == t) return true;
			if (rightFoot == t) return true;
			if (leftUpperArm == t) return true;
			if (leftLowerArm == t) return true;
			if (leftHand == t) return true;
			if (rightUpperArm == t) return true;
			if (rightLowerArm == t) return true;
			if (rightHand == t) return true;
			if (head == t) return true;

			return false;
		}

		public Transform[] GetRagdollTransforms() {
			return new Transform[16] {
				hips,
				spine,
				chest,
				head,
				leftUpperArm,
				leftLowerArm,
				leftHand,
				rightUpperArm,
				rightLowerArm,
				rightHand,
				leftUpperLeg,
				leftLowerLeg,
				leftFoot,
				rightUpperLeg,
				rightLowerLeg,
				rightFoot
			};
		}

		public static BipedRagdollReferences FromAvatar(Animator animator) {
			BipedRagdollReferences r = new BipedRagdollReferences();

			if (!animator.isHuman)
            {
                Dictionary<string, Transform> transformDict = new Dictionary<string, Transform>();
                foreach (Transform t in animator.gameObject.GetComponentsInChildren<Transform>())
                {
                    if(!transformDict.ContainsKey(t.name))
                    {
                        transformDict[t.name] = t;
                    }
                }

                r.root = transformDict.ContainsKey("Position") ? transformDict["Position"] : null;

                r.hips = transformDict.ContainsKey("Hip") ? transformDict["Hip"] : null;
                r.spine = transformDict.ContainsKey("Spine") ? transformDict["Spine"] : null;
                r.chest = transformDict.ContainsKey("Chest") ? transformDict["Chest"] : null;
                r.head = transformDict.ContainsKey("Head") ? transformDict["Head"] : null;

                r.leftUpperArm = transformDict.ContainsKey("Arm_L") ? transformDict["Arm_L"] : null;
                r.leftLowerArm = transformDict.ContainsKey("Elbow_L") ? transformDict["Elbow_L"] : null;
                r.leftHand = transformDict.ContainsKey("Wrist_L") ? transformDict["Wrist_L"] : null;

                r.rightUpperArm = transformDict.ContainsKey("Arm_R") ? transformDict["Arm_R"] : null;
                r.rightLowerArm = transformDict.ContainsKey("Elbow_R") ? transformDict["Elbow_R"] : null;
                r.rightHand = transformDict.ContainsKey("Wrist_R") ? transformDict["Wrist_R"] : null;

                r.leftUpperLeg = transformDict.ContainsKey("Thigh_L") ? transformDict["Thigh_L"] : null;
                r.leftLowerLeg = transformDict.ContainsKey("Knee_L") ? transformDict["Knee_L"] : null;
                r.leftFoot = transformDict.ContainsKey("Ankle_L") ? transformDict["Ankle_L"] : null;

                r.rightUpperLeg = transformDict.ContainsKey("Thigh_R") ? transformDict["Thigh_R"] : null;
                r.rightLowerLeg = transformDict.ContainsKey("Knee_R") ? transformDict["Knee_R"] : null;
                r.rightFoot = transformDict.ContainsKey("Ankle_R") ? transformDict["Ankle_R"] : null;

                return r;
            }

            r.root = animator.transform;

			r.hips = animator.GetBoneTransform(HumanBodyBones.Hips);
			r.spine = animator.GetBoneTransform(HumanBodyBones.Spine);
			r.chest = animator.GetBoneTransform(HumanBodyBones.Chest);
			r.head = animator.GetBoneTransform(HumanBodyBones.Head);
			
			r.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			r.leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			r.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			
			r.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			r.rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			r.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
			
			r.leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			r.leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			r.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			
			r.rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			r.rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			r.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

			return r;
		}

		public static BipedRagdollReferences FromBipedReferences(BipedReferences biped) {
			BipedRagdollReferences r = new BipedRagdollReferences();

			r.root = biped.root;

			r.hips = biped.pelvis;

			if (biped.spine != null && biped.spine.Length > 0) {
				r.spine = biped.spine[0];
				if (biped.spine.Length > 1) r.chest = biped.spine[biped.spine.Length - 1];
			}

			r.head = biped.head;
			
			r.leftUpperArm = biped.leftUpperArm;
			r.leftLowerArm = biped.leftForearm;
			r.leftHand = biped.leftHand;
			
			r.rightUpperArm = biped.rightUpperArm;
			r.rightLowerArm = biped.rightForearm;
			r.rightHand = biped.rightHand;
			
			r.leftUpperLeg = biped.leftThigh;
			r.leftLowerLeg = biped.leftCalf;
			r.leftFoot = biped.leftFoot;
			
			r.rightUpperLeg = biped.rightThigh;
			r.rightLowerLeg = biped.rightCalf;
			r.rightFoot = biped.rightFoot;
			
			return r;
		}
	}
}
