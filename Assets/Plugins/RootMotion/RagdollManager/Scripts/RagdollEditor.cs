using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Interactive Scene View ragdoll editor.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=y-luLRVmL7E&index=1&list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL")]
	[AddComponentMenu("Scripts/RootMotion.Dynamics/Ragdoll Manager/Ragdoll Editor")]
	public class RagdollEditor : MonoBehaviour {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		void OpenUserManual() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page2.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		void OpenScriptReference() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_ragdoll_editor.html");
		}
		
		// Open a video tutorial about setting up the component
		[ContextMenu("TUTORIAL VIDEO")]
		void OpenTutorial() {
			Application.OpenURL("https://www.youtube.com/watch?v=y-luLRVmL7E&index=1&list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL");
		}

		[System.Serializable]
		public enum Mode {
			Colliders,
			Joints
		}

		[HideInInspector] public Rigidbody selectedRigidbody;
		[HideInInspector] public Collider selectedCollider;
		[HideInInspector] public bool symmetry = true;
		[HideInInspector] public Mode mode;

	}
}
