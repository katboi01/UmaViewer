using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Sets up the layers and collision matrix for the puppet without having to import/export Project Settings for the demo to work. 
	// Ideally you should set up the layers in the project and this component wouldn't be needed at all.
	[RequireComponent(typeof(PuppetMaster))]
	public class LayerSetup : MonoBehaviour {

		[Header("References")]

		[Tooltip("Reference to the character controller gameobject (the one that has the capsule collider/CharacterController.")]
		public Transform characterController;

		[Header("Layers")]

		[Tooltip("The layer to assign the character controller to. Collisions between this layer and the 'Ragdoll Layer' will be ignored, or else the ragdoll would collide with the character controller.")]
		public int characterControllerLayer;

		[Tooltip("The layer to assign the PuppetMaster and all it's muscles to. Collisions between this layer and the 'Character Controller Layer' will be ignored, or else the ragdoll would collide with the character controller.")]
		public int ragdollLayer;

		[Tooltip("Layers that will be ignored by the character controller")]
		public LayerMask ignoreCollisionWithCharacterController;

		[Tooltip("Layers that will not collide with the Ragdoll layer.")]
		public LayerMask ignoreCollisionWithRagdoll;

		private PuppetMaster puppetMaster;

		void Awake() {
			puppetMaster = GetComponent<PuppetMaster>();

			// Assign the ragdoll layers.
			puppetMaster.gameObject.layer = ragdollLayer;
			foreach (Muscle m in puppetMaster.muscles) m.joint.gameObject.layer = ragdollLayer;

			// Assign the character controller layer
			characterController.gameObject.layer = characterControllerLayer;

			// Ignore collisions between the ragdoll and the character controller
			Physics.IgnoreLayerCollision(characterControllerLayer, ragdollLayer);

			// Ignore collisions between character controllers
			Physics.IgnoreLayerCollision(characterControllerLayer, characterControllerLayer);

			// Ignore collisions between the puppet-damaging layers and the character controller layer
			int[] characterIgnoreIndexes = LayerMaskExtensions.MaskToNumbers(ignoreCollisionWithCharacterController);
			foreach (int index in characterIgnoreIndexes) {
				Physics.IgnoreLayerCollision(characterControllerLayer, index);
			}

			// Ignore collisions between the ragdoll and the ignoreCollisionWithRagdoll layers
			int[] ragdollIgnoreIndexes = LayerMaskExtensions.MaskToNumbers(ignoreCollisionWithRagdoll);
			foreach (int index in ragdollIgnoreIndexes) {
				Physics.IgnoreLayerCollision(ragdollLayer, index);
			}

			Destroy(this);
		}

	}
}
