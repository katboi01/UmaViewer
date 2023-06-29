using UnityEngine;
using System.Collections;

namespace RootMotion {

	/// <summary>
	/// Forwards collider OnTrigger.. events.
	/// </summary>
	public class TriggerEventBroadcaster : MonoBehaviour {
	
		public GameObject target;
		
		void OnTriggerEnter(Collider collider) {
			if (target != null) target.SendMessage("OnTriggerEnter", collider, SendMessageOptions.DontRequireReceiver);
		}
		
		void OnTriggerStay(Collider collider) {
			if (target != null) target.SendMessage("OnTriggerStay", collider, SendMessageOptions.DontRequireReceiver);
		}
		
		void OnTriggerExit(Collider collider) {
			if (target != null) target.SendMessage("OnTriggerExit", collider, SendMessageOptions.DontRequireReceiver);
		}
	}
}
