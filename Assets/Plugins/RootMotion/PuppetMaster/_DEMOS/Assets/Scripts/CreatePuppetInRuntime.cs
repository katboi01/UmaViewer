using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Code example for creating a Puppet in runtime.
	public class CreatePuppetInRuntime : MonoBehaviour {

		[Tooltip("Creating a Puppet from a ragdoll character prefab.")]
		public Transform ragdollPrefab;

		[Tooltip("What will the Puppet be named?")]
		public string instanceName = "My Character";

		[Tooltip("The layer to assign the character controller to. Collisions between this layer and the 'Ragdoll Layer' will be ignored, or else the ragdoll would collide with the character controller.")]
		public int characterControllerLayer;
		
		[Tooltip("The layer to assign the PuppetMaster and all it's muscles to. Collisions between this layer and the 'Character Controller Layer' will be ignored, or else the ragdoll would collide with the character controller.")]
		public int ragdollLayer;

		void Start() {
			Transform ragdoll = GameObject.Instantiate(ragdollPrefab, transform.position, transform.rotation) as Transform;
			ragdoll.name = instanceName;

            // This will duplicate the ragdoll character, remove the ragdoll components from the original and use it as the animated target.
			PuppetMaster.SetUp(ragdoll, characterControllerLayer, ragdollLayer);
            
			Debug.Log("A ragdoll was successfully converted to a Puppet.");
		}
	}
}
