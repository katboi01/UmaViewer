using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Triggering Hit Reactions on mouse button.
	/// </summary>
	public class HitReactionVRIKTrigger: MonoBehaviour {

        public HitReactionVRIK hitReaction;
        public float hitForce = 1f;

        private string colliderName;

		void Update() {
			// On left mouse button...
			if (Input.GetMouseButtonDown(0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				// Raycast to find a ragdoll collider
				RaycastHit hit = new RaycastHit();
				if (Physics.Raycast(ray, out hit, 100f)) {

					// Use the HitReaction
					hitReaction.Hit(hit.collider, ray.direction * hitForce, hit.point);

					// Just for GUI
					colliderName = hit.collider.name;
				}
			}
		}

		void OnGUI() {
			GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.");
			if (colliderName != string.Empty) GUILayout.Label("Last Bone Hit: " + colliderName);
		}
	}
}