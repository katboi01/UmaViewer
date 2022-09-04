using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	// Applies initial velocity to a Rigidbody
	public class InitialVelocity : MonoBehaviour {

		public Vector3 initialVelocity;

		void Start () {
			GetComponent<Rigidbody>().velocity = initialVelocity;
		}
	}
}
