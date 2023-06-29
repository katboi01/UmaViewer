using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Code example for creating a ragdoll in runtime using the BipedRagdollCreator.
	public class CreateRagdollInRuntime : MonoBehaviour {

		[Tooltip("The character prefab/FBX.")]
		public GameObject prefab;

		void Start() {
			// Instantiate the character
			GameObject instance = GameObject.Instantiate(prefab);

			// Find bones (Humanoids)
			BipedRagdollReferences r = BipedRagdollReferences.FromAvatar(instance.GetComponent<Animator>());

			// How would you like your ragdoll?
			BipedRagdollCreator.Options options = BipedRagdollCreator.AutodetectOptions(r);

			// Edit options here if you need to
			//options.headCollider = RagdollCreator.ColliderType.Box;
			//options.weight *= 2f;
			//options.joints = RagdollCreator.JointType.Character;

			// Create the ragdoll
			BipedRagdollCreator.Create(r, options);

			Debug.Log("A ragdoll was successfully created.");
		}

		void Update() {
			// If bone proportions have changed, just clear and recreate:
			//BipedRagdollCreator.ClearBipedRagdoll(r); //ClearAll if you have changed references
			//BipedRagdollCreator.Create(r, options);
		}
	}
}
