using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Contains high level API calls for the PuppetMaster.
	public partial class PuppetMaster: MonoBehaviour {

		/// <summary>
		/// Sets the muscle weights for all muscles in the specified group.
		/// </summary>
		public void SetMuscleWeights(Muscle.Group group, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f) {
			if (!CheckIfInitiated()) return;

			foreach (Muscle m in muscles) {
				if (m.props.group == group) {
					m.props.muscleWeight = muscleWeight;
					m.props.pinWeight = pinWeight;
					m.props.mappingWeight = mappingWeight;
					m.props.muscleDamper = muscleDamper;
				}
			}
		}

		/// <summary>
		/// Sets the muscle weights for the muscle that has the humanBodyBone target (works only with a Humanoid avatar).
		/// </summary>
		public void SetMuscleWeights(Transform target, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f) {
			if (!CheckIfInitiated()) return;
			
			int index = GetMuscleIndex(target);
			if (index == -1) return;
			SetMuscleWeights(index, muscleWeight, pinWeight, mappingWeight, muscleDamper);
		}

		/// <summary>
		/// Sets the muscle weights for the muscle that has the humanBodyBone target (works only with a Humanoid avatar).
		/// </summary>
		public void SetMuscleWeights(HumanBodyBones humanBodyBone, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f) {
			if (!CheckIfInitiated()) return;
			
			int index = GetMuscleIndex(humanBodyBone);
			if (index == -1) return;
			SetMuscleWeights(index, muscleWeight, pinWeight, mappingWeight, muscleDamper);
		}

		/// <summary>
		/// Sets the muscle weights for the muscle with the specified target and all it's child muscles (when called on the upper arm, will set weights for the upper arm, forearm and hand of the same limb). 
		/// </summary>
		public void SetMuscleWeightsRecursive(Transform target, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f) {
			if (!CheckIfInitiated()) return;

			for (int i = 0; i < muscles.Length; i++) {
				if (muscles[i].target == target) {
					SetMuscleWeightsRecursive(i, muscleWeight, pinWeight, mappingWeight, muscleDamper);
					return;
				}
			}
		}

		/// <summary>
		/// Sets the muscle weights for the muscle of the specified muscle index and all it's child muscles (when called on the upper arm, will set weights for the upper arm, forearm and hand of the same limb).
		/// </summary>
		public void SetMuscleWeightsRecursive(int muscleIndex, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f) {
			if (!CheckIfInitiated()) return;

			SetMuscleWeights(muscleIndex, muscleWeight, pinWeight, mappingWeight, muscleDamper);

			for (int i = 0; i < muscles[muscleIndex].childIndexes.Length; i++) {
				int childIndex = muscles[muscleIndex].childIndexes[i];
				SetMuscleWeights(childIndex, muscleWeight, pinWeight, mappingWeight, muscleDamper);
			}
		}

		/// <summary>
		/// Sets the muscle weights for the muscle that has the humanBodyBone target (works only with a Humanoid avatar) and all it's child muscles (when called on the upper arm, will set weights for the upper arm, forearm and hand of the same limb).
		/// </summary>
		public void SetMuscleWeightsRecursive(HumanBodyBones humanBodyBone, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f) {
			if (!CheckIfInitiated()) return;

			int index = GetMuscleIndex(humanBodyBone);
			if (index == -1) return;
			SetMuscleWeightsRecursive(index, muscleWeight, pinWeight, mappingWeight, muscleDamper);
		}

		/// <summary>
		/// Sets the muscle weights for the muscle with the specified index.
		/// </summary>
		public void SetMuscleWeights(int muscleIndex, float muscleWeight, float pinWeight, float mappingWeight, float muscleDamper) {
			if (!CheckIfInitiated()) return;

			if (muscleIndex < 0f || muscleIndex >= muscles.Length) {
				Debug.LogWarning("Muscle index out of range (" + muscleIndex + ").", transform);
				return;
			}

			muscles[muscleIndex].props.muscleWeight = muscleWeight;
			muscles[muscleIndex].props.pinWeight = pinWeight;
			muscles[muscleIndex].props.mappingWeight = mappingWeight;
			muscles[muscleIndex].props.muscleDamper = muscleDamper;
		}

		/// <summary>
		/// Returns the muscle that has the specified target.
		/// </summary>
		public Muscle GetMuscle(Transform target) {
			int index = GetMuscleIndex(target);
			if (index == -1) return null;
			return muscles[index];
		}

		/// <summary>
		/// Returns the muscle of the specified Rigidbody.
		/// </summary>
		public Muscle GetMuscle(Rigidbody rigidbody) {
			int index = GetMuscleIndex(rigidbody);
			if (index == -1) return null;
			return muscles[index];
		}

		/// <summary>
		/// Returns the muscle of the specified Joint.
		/// </summary>
		public Muscle GetMuscle(ConfigurableJoint joint) {
			int index = GetMuscleIndex(joint);
			if (index == -1) return null;
			return muscles[index];
		}

		/// <summary>
		/// Does the PuppetMaster have a muscle for the specified joint.
		/// </summary>
		public bool ContainsJoint(ConfigurableJoint joint) {
			if (!CheckIfInitiated()) return false;

			foreach (Muscle m in muscles) {
				if (m.joint == joint) return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the index of the muscle that has the humanBodyBone target (works only with a Humanoid avatar).
		/// </summary>
		public int GetMuscleIndex(HumanBodyBones humanBodyBone) {
			if (!CheckIfInitiated()) return -1;
			
			if (targetAnimator == null) {
				Debug.LogWarning("PuppetMaster 'Target Root' has no Animator component on it nor on it's children.", transform);
				return -1;
			}
			if (!targetAnimator.isHuman) {
				Debug.LogWarning("PuppetMaster target's Animator does not belong to a Humanoid, can hot get human muscle index.", transform);
				return -1;
			}
			
			var bone = targetAnimator.GetBoneTransform(humanBodyBone);
			if (bone == null) {
				Debug.LogWarning("PuppetMaster target's Avatar does not contain a bone Transform for " + humanBodyBone, transform);
				return -1;
			}
			
			return GetMuscleIndex(bone);
		}

		/// <summary>
		/// Returns the index of the muscle that has the specified target. Returns -1 if not found.
		/// </summary>
		public int GetMuscleIndex(Transform target) {
			if (!CheckIfInitiated()) return -1;

			if (target == null) {
				Debug.LogWarning("Target is null, can not get muscle index.", transform);
				return -1;
			}

			for (int i = 0; i < muscles.Length; i++) {
				if (muscles[i].target == target) return i;
			}

			Debug.LogWarning("No muscle with target " + target.name + "found on the PuppetMaster.", transform);
			return -1;
		}
		
		/// <summary>
		/// Returns the index of the muscle that has the specified Rigidbody. Returns -1 if not found.
		/// </summary>
		public int GetMuscleIndex(Rigidbody rigidbody) {
			if (!CheckIfInitiated()) return -1;

			if (rigidbody == null) {
				Debug.LogWarning("Rigidbody is null, can not get muscle index.", transform);
				return -1;
			}

			for (int i = 0; i < muscles.Length; i++) {
				if (muscles[i].rigidbody == rigidbody) return i;
			}

			Debug.LogWarning("No muscle with Rigidbody " + rigidbody.name + "found on the PuppetMaster.", transform);
			return -1;
		}
		
		/// <summary>
		/// Returns the index of the muscle that has the specified Joint. Returns -1 if not found.
		/// </summary>
		public int GetMuscleIndex(ConfigurableJoint joint) {
			if (joint == null) {
				Debug.LogWarning("Joint is null, can not get muscle index.", transform);
				return -1;
			}

			for (int i = 0; i < muscles.Length; i++) {
				if (muscles[i].joint == joint) return i;
			}

			Debug.LogWarning("No muscle with Joint " + joint.name + "found on the PuppetMaster.", transform);
			return -1;
		}
	}
}

