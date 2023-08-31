using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Demonstrating character-character FBBIK interaction.
	/// </summary>
	public class InteractionC2CDemo : MonoBehaviour {

		// GUI for testing
		void OnGUI() {
			if (GUILayout.Button("Shake Hands")) {
				
				character1.StartInteraction(FullBodyBipedEffector.RightHand, handShake, true);
				character2.StartInteraction(FullBodyBipedEffector.RightHand, handShake, true);
				
			}
		}

		public InteractionSystem character1, character2; // The InteractionSystems of the characters
		public InteractionObject handShake; // The HandShake InteractionObject

		void LateUpdate() {
			// Positioning the handshake to the middle of the hands
			Vector3 handsCenter = Vector3.Lerp(character1.ik.solver.rightHandEffector.bone.position, character2.ik.solver.rightHandEffector.bone.position, 0.5f);
			handShake.transform.position = handsCenter;
		}

	}
}
