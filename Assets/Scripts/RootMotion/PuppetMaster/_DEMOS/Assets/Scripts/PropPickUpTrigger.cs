using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	public class PropPickUpTrigger : MonoBehaviour {

		public PuppetMasterProp prop;
		public LayerMask characterLayers;

		private CharacterPuppet characterPuppet;

		void OnTriggerEnter(Collider collider) {
			if (prop.isPickedUp) return;
			if (!LayerMaskExtensions.Contains(characterLayers, collider.gameObject.layer)) return;

			characterPuppet = collider.GetComponent<CharacterPuppet>();
			if (characterPuppet == null) return;

			if (characterPuppet.puppet.state != BehaviourPuppet.State.Puppet) return;

			if (characterPuppet.propMuscle == null) return;
			if (characterPuppet.propMuscle.currentProp != null) return;

			characterPuppet.propMuscle.currentProp = prop;
		}
	}
}
