using UnityEngine;
using System.Collections;
using System;

namespace RootMotion {

	/// <summary>
	/// Class for identifying biped bones based on most common naming conventions.
	/// </summary>
	public static class BipedNaming {
		
		/// <summary>
		/// Type of the bone.
		/// </summary>
		[System.Serializable]
		public enum BoneType {
			Unassigned,
			Spine,
			Head,
			Arm,
			Leg,
			Tail,
			Eye
		}
		
		/// <summary>
		/// Bone side: Left and Right for limbs and Center for spine, head and tail.
		/// </summary>
		[System.Serializable]
		public enum BoneSide {
			Center,
			Left,
			Right
		}
		
		// Bone identifications
		public static string[] 
		typeLeft = {" L ", "_L_", "-L-", " l ", "_l_", "-l-", "Left", "left", "CATRigL"},
		typeRight = {" R ", "_R_", "-R-", " r ", "_r_", "-r-", "Right", "right", "CATRigR"},
		
		typeSpine = {"Spine", "spine", "Pelvis", "pelvis", "Root", "root", "Torso", "torso", "Body", "body", "Hips", "hips", "Neck", "neck", "Chest", "chest"},
		typeHead = {"Head", "head"},	
		typeArm = {"Arm", "arm", "Hand", "hand", "Wrist", "Wrist", "Elbow", "elbow", "Palm", "palm"},
		typeLeg = {"Leg", "leg", "Thigh", "thigh", "Calf", "calf", "Femur", "femur", "Knee", "knee", "Foot", "foot", "Ankle", "ankle", "Hip", "hip"},
		typeTail = {"Tail", "tail"},
		typeEye = {"Eye", "eye"},
		
		typeExclude = {"Nub", "Dummy", "dummy", "Tip", "IK", "Mesh"},
		typeExcludeSpine = {"Head", "head"},
		typeExcludeHead = {"Top", "End" },
		typeExcludeArm = {"Collar", "collar", "Clavicle", "clavicle", "Finger", "finger", "Index", "index", "Mid", "mid", "Pinky", "pinky", "Ring", "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist"},
		typeExcludeLeg = {"Toe", "toe", "Platform", "Adjust", "adjust", "Twist", "twist"},
		typeExcludeTail = {},
		typeExcludeEye = {"Lid", "lid", "Brow", "brow", "Lash", "lash"},
		
		pelvis = {"Pelvis", "pelvis", "Hip", "hip"},
		hand = {"Hand", "hand", "Wrist", "wrist", "Palm", "palm"},
		foot = {"Foot", "foot", "Ankle", "ankle"};
		
		#region Public methods
		
		/// <summary>
		/// Returns only the bones with the specified BoneType.
		/// </summary>
		public static Transform[] GetBonesOfType(BoneType boneType, Transform[] bones) {
			Transform[] r = new Transform[0];
			foreach (Transform bone in bones) {
				if (bone != null && GetBoneType(bone.name) == boneType) {
					Array.Resize(ref r, r.Length + 1);
					r[r.Length - 1] = bone;
				}
			}
			return r;
		}
		
		/// <summary>
		/// Returns only the bones with the specified BoneSide.
		/// </summary>
		public static Transform[] GetBonesOfSide(BoneSide boneSide, Transform[] bones) {
			Transform[] r = new Transform[0];
			foreach (Transform bone in bones) {
				if (bone != null && GetBoneSide(bone.name) == boneSide) {
					Array.Resize(ref r, r.Length + 1);
					r[r.Length - 1] = bone;
				}
			}
			return r;
		}
		
		/// <summary>
		/// Gets the bones of type and side.
		/// </summary>
		public static Transform[] GetBonesOfTypeAndSide(BoneType boneType, BoneSide boneSide, Transform[] bones) {
			Transform[] bonesOfType = GetBonesOfType(boneType, bones);
			return GetBonesOfSide(boneSide, bonesOfType);
		}
		
		/// <summary>
		/// Gets the bone of type and side. If more than one is found, will return the first in the array.
		/// </summary>
		public static Transform GetFirstBoneOfTypeAndSide(BoneType boneType, BoneSide boneSide, Transform[] bones) {
			Transform[] b = GetBonesOfTypeAndSide(boneType, boneSide, bones);
			if (b.Length == 0) return null;
			return b[0];
		}
		
		/// <summary>
		/// Returns only the bones that match all the namings in params string[][] namings
		/// </summary>
		/// <returns>
		/// The matching Transforms
		/// </returns>
		/// <param name='transforms'>
		/// Transforms.
		/// </param>
		/// <param name='namings'>
		/// Namings.
		/// </param>
		public static Transform GetNamingMatch(Transform[] transforms, params string[][] namings) {
			foreach (Transform t in transforms) {
				bool match = true;
				foreach (string[] naming in namings) {
					if (!matchesNaming(t.name, naming)) {
						match = false;
						break;
					}
				}
				if (match) return t;
			}
			return null;
		}
		
		/// <summary>
		/// Gets the type of the bone.
		/// </summary>
		public static BoneType GetBoneType(string boneName) {	
			if (isSpine(boneName)) return BoneType.Spine;
			if (isHead(boneName)) return BoneType.Head;
			if (isArm (boneName)) return BoneType.Arm;
			if (isLeg(boneName)) return BoneType.Leg;
			if (isTail(boneName)) return BoneType.Tail;
			if (isEye(boneName)) return BoneType.Eye;
			
			return BoneType.Unassigned;
		}
		
		/// <summary>
		/// Gets the bone side.
		/// </summary>
		public static BoneSide GetBoneSide(string boneName) {
			if (isLeft(boneName)) return BoneSide.Left;
			if (isRight(boneName)) return BoneSide.Right;
			return BoneSide.Center;
		}
		
		/// <summary>
		/// Returns the bone of type and side with additional naming parameters.
		/// </summary>
		public static Transform GetBone(Transform[] transforms, BoneType boneType, BoneSide boneSide = BoneSide.Center, params string[][] namings) {
			Transform[] bones = GetBonesOfTypeAndSide(boneType, boneSide, transforms);
			return GetNamingMatch(bones, namings);
		}
		
		#endregion Public methods
		
		private static bool isLeft(string boneName) {
			return matchesNaming(boneName, typeLeft) || lastLetter(boneName) == "L" || firstLetter(boneName) == "L";
		}
		
		private static bool isRight(string boneName) {
			return matchesNaming(boneName, typeRight) || lastLetter(boneName) == "R" || firstLetter(boneName) == "R";
		}
		
		private static bool isSpine(string boneName) {
			return matchesNaming(boneName, typeSpine) && !excludesNaming(boneName, typeExcludeSpine);
		}
		
		private static bool isHead(string boneName) {
			return matchesNaming(boneName, typeHead) && !excludesNaming(boneName, typeExcludeHead);
		}
		
		private static bool isArm(string boneName) {
			return matchesNaming(boneName, typeArm) && !excludesNaming(boneName, typeExcludeArm);
		}
		
		private static bool isLeg(string boneName) {
			return matchesNaming(boneName, typeLeg) && !excludesNaming(boneName, typeExcludeLeg);
		}
		
		private static bool isTail(string boneName) {
			return matchesNaming(boneName, typeTail) && !excludesNaming(boneName, typeExcludeTail);
		}
		
		private static bool isEye(string boneName) {
			return matchesNaming(boneName, typeEye) && !excludesNaming(boneName, typeExcludeEye);
		}
		
		private static bool isTypeExclude(string boneName) {
			return matchesNaming(boneName, typeExclude);
		}
		
		private static bool matchesNaming(string boneName, string[] namingConvention) {
			if (excludesNaming(boneName, typeExclude)) return false;
			
			foreach(string n in namingConvention) {
				if (boneName.Contains(n)) return true;
			}
			return false;
		}
		
		private static bool excludesNaming(string boneName, string[] namingConvention) {
			foreach(string n in namingConvention) {
				if (boneName.Contains(n)) return true;
			}
			return false;
		}
		
		private static bool matchesLastLetter(string boneName, string[] namingConvention) {
			foreach(string n in namingConvention) {
				if (LastLetterIs(boneName, n)) return true;
			}
			return false;
		}
		
		private static bool LastLetterIs(string boneName, string letter) {
			string lastLetter = boneName.Substring(boneName.Length - 1, 1);
			return lastLetter == letter;
		}
		
		private static string firstLetter(string boneName) {
			if (boneName.Length > 0) return boneName.Substring(0, 1);
			return "";
		}
		
		private static string lastLetter(string boneName) {
			if (boneName.Length > 0) return boneName.Substring(boneName.Length - 1, 1);
			return "";
		}
	}
}
