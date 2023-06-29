using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Scales the puppet and/or any of it's muscles.
	public class PuppetScaling : MonoBehaviour {

		public PuppetMaster puppetMaster;
		[Range(0.01f, 10f)] public float masterScale = 1f;
		public int muscleIndex;
		[Range(0.01f, 10f)] public float muscleScale = 1f;

		private float defaultMuscleSpring;

		void Start() {
			puppetMaster.updateJointAnchors = true;
			puppetMaster.supportTranslationAnimation = true;

			defaultMuscleSpring = puppetMaster.muscleSpring;
		}

		void Update() {
			// Scaling the entire puppet
			puppetMaster.transform.parent.localScale = Vector3.one * masterScale;

			// Rigidbodies with larger colliders have larger inertia tensor values, requiring more force to rotate them so we need to pump up the muscle spring value
			puppetMaster.muscleSpring = defaultMuscleSpring * Mathf.Pow(masterScale, 2f);

			// Scaling each muscle individually
			muscleIndex = Mathf.Clamp(muscleIndex, 0, puppetMaster.muscles.Length - 1);

			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				if (i == muscleIndex) {
					puppetMaster.muscles[i].target.localScale = Vector3.one * muscleScale;
					puppetMaster.muscles[i].transform.localScale = Vector3.one * muscleScale;
				} else {
					puppetMaster.muscles[i].target.localScale = Vector3.one;
					puppetMaster.muscles[i].transform.localScale = Vector3.one;
				}
			}

			// If flat hierarchy, scale all child muscles too
			bool flatHierarchy = puppetMaster.muscles[1].transform.parent == puppetMaster.transform;

			if (flatHierarchy) {
				for (int i = 0; i < puppetMaster.muscles[muscleIndex].childIndexes.Length; i++) {
					puppetMaster.muscles[puppetMaster.muscles[muscleIndex].childIndexes[i]].transform.localScale = Vector3.one * muscleScale;
				}
			}
		}
	}
}