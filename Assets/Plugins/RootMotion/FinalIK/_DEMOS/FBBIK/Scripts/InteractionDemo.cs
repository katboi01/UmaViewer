using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Simple demo controller for the InteractionSystem.
	/// </summary>
	public class InteractionDemo : MonoBehaviour {

		public InteractionSystem interactionSystem; // Reference to the InteractionSystem of the character

		public bool interrupt; // Can we interrupt an interaction of an effector?

		// The interaction objects
		public InteractionObject ball, benchMain, benchHands, button, cigarette, door;

		private bool isSitting;
        
		// GUI for calling the interactions
		void OnGUI() {
            interrupt = GUILayout.Toggle(interrupt, "Interrupt");

			// While seated
			if (isSitting) {

				if (!interactionSystem.inInteraction && GUILayout.Button("Stand Up")) {
					interactionSystem.ResumeAll();

					isSitting = false;
				}

				return;
			}

			// While standing

			if (GUILayout.Button("Pick Up Ball")) {
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, ball, interrupt);
			}

			if (GUILayout.Button("Button Left Hand")) {
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, button, interrupt);
			}

			if (GUILayout.Button("Button Right Hand")) {
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, button, interrupt);
			}

			if (GUILayout.Button("Put Out Cigarette")) {
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightFoot, cigarette, interrupt);
			}

			if (GUILayout.Button("Open Door")) {
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, door, interrupt);
			}

			// This is a multiple-effector interaction
			if (!interactionSystem.inInteraction && GUILayout.Button("Sit Down")) {
				interactionSystem.StartInteraction(FullBodyBipedEffector.Body, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftThigh, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightThigh, benchMain, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftFoot, benchMain, interrupt);
				
				interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, benchHands, interrupt);
				interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, benchHands, interrupt);
				
				isSitting = true;
			}
		}
	}
}
