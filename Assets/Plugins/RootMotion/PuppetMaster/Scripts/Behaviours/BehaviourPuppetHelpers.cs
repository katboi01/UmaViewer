using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {
	
	// Helper methods for BehaviourPuppet
	public partial class BehaviourPuppet : BehaviourBase {
		
		/// <summary>
		/// Determines whether this ragdoll is facing up (false) or down (true).
		/// </summary>
		public bool IsProne() {
			float dot = Vector3.Dot(puppetMaster.muscles[0].transform.rotation * hipsForward, puppetMaster.targetRoot.up);
			return dot < 0f;
		}

		// Gets the falloff value of muscle 'i' according to it's kinship degree from muscle 'muscleIndex' and the parent and child falloff values.
		private float GetFalloff(int i, int muscleIndex, float falloffParents, float falloffChildren) {
			if (i == muscleIndex) return 1f;
			
			bool isChild = puppetMaster.muscles[muscleIndex].childFlags[i];
			int kinshipDegree = puppetMaster.muscles[muscleIndex].kinshipDegrees[i];
			
			return Mathf.Pow(isChild? falloffChildren: falloffParents, kinshipDegree);
		}

		// Gets the falloff value of muscle 'i' according to it's kinship degree from muscle 'muscleIndex' and the parent, child and group falloff values.
		private float GetFalloff(int i, int muscleIndex, float falloffParents, float falloffChildren, float falloffGroup) {
			float falloff = GetFalloff(i, muscleIndex, falloffParents, falloffChildren);
			
			if (falloffGroup > 0f && i != muscleIndex && InGroup(puppetMaster.muscles[i].props.group, puppetMaster.muscles[muscleIndex].props.group)) {
				falloff = Mathf.Max(falloff, falloffGroup);
			}
			
			return falloff;
		}

		// Returns true is the groups match directly OR in the group overrides.
		private bool InGroup(Muscle.Group group1, Muscle.Group group2) {
			if (group1 == group2) return true;
			
			foreach (MusclePropsGroup musclePropsGroup in groupOverrides) {
				foreach (Muscle.Group g in musclePropsGroup.groups) {
					if (g == group1) {
						foreach (Muscle.Group g2 in musclePropsGroup.groups) {
							if (g2 == group2) return true;
						}
					}
				}
			}
			
			return false;
		}

		// Returns the MusclePropsGroup of the specified muscle group.
		private MuscleProps GetProps(Muscle.Group group) {
			foreach (MusclePropsGroup g in groupOverrides) {
				foreach (Muscle.Group group2 in g.groups) {
					if (group2 == group) return g.props;
				}
			}
			return defaults;
		}
	}
}