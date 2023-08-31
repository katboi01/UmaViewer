using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	// Demonstrating the use of RagdollUtility.cs.
	public class RagdollUtilityDemo : MonoBehaviour {

		public RagdollUtility ragdollUtility;
		public Transform root;
		public Rigidbody pelvis;

		void OnGUI() {
			GUILayout.Label(" Press R to switch to ragdoll. " +
			                "\n Weigh in one of the FBBIK effectors to make kinematic changes to the ragdoll pose." +
			                "\n A to blend back to animation");
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.R)) ragdollUtility.EnableRagdoll();
			if (Input.GetKeyDown(KeyCode.A)) {
				// Move the root of the character to where the pelvis is without moving the ragdoll
				Vector3 toPelvis = pelvis.position - root.position;
				root.position += toPelvis;
				pelvis.transform.position -= toPelvis;

				ragdollUtility.DisableRagdoll();
			}
		}

	}
}
