using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	// The default prop template with no functionality
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page6.html")]
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Prop Template")]
	public class PropTemplate: Prop {

		protected override void OnStart() {
			// Initiate stuff here.
		}

		protected override void OnPickUp(PropRoot propRoot) {
			// Called when the prop has been picked up and connected to a PropRoot.
		}
		
		protected override void OnDrop() {
			// Called when the prop has been dropped.
		}
	}
}
