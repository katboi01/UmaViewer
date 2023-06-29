using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// User control demo for Interaction Triggers.
	/// </summary>
	public class UserControlInteractions : UserControlThirdPerson {

		public CharacterThirdPerson character;
        public InteractionSystem interactionSystem; // Reference to the InteractionSystem of the character
        public bool disableInputInInteraction = true; // If true, will keep the character stopped while an interaction is in progress
		public float enableInputAtProgress = 0.8f; // The normalized interaction progress after which the character is able to move again

		protected override void Update() {
            // Disable input when in interaction
            if (disableInputInInteraction && interactionSystem != null && (interactionSystem.inInteraction || interactionSystem.IsPaused())) {

				// Get the least interaction progress
				float progress = interactionSystem.GetMinActiveProgress();

				// Keep the character in place
				if (progress > 0f && progress < enableInputAtProgress) {
					state.move = Vector3.zero;
					state.jump = false;
					return;
				}
			}

            
			// Pass on the FixedUpdate call
			base.Update();
		}

		// Triggering the interactions
		void OnGUI() {
			// If jumping or falling, do nothing
			if (!character.onGround) return;

			// If an interaction is paused, resume on user input
			if (interactionSystem.IsPaused() && interactionSystem.IsInSync()) {
				GUILayout.Label("Press E to resume interaction");

				if (Input.GetKey(KeyCode.E)) {
					interactionSystem.ResumeAll();
				}

				return;
			}

			// If not paused, find the closest InteractionTrigger that the character is in contact with
			int closestTriggerIndex = interactionSystem.GetClosestTriggerIndex();

			// ...if none found, do nothing
			if (closestTriggerIndex == -1) return;

			// ...if the effectors associated with the trigger are in interaction, do nothing
			if (!interactionSystem.TriggerEffectorsReady(closestTriggerIndex)) return;

			// Its OK now to start the trigger
			GUILayout.Label("Press E to start interaction");

			if (Input.GetKey(KeyCode.E)) {
				interactionSystem.TriggerInteraction(closestTriggerIndex, false);
			}
		}
	}
}
