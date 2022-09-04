using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	// Just shooting objects from the camera towards the mouse position.
	public class BallShooter : MonoBehaviour {

		public KeyCode keyCode = KeyCode.Mouse0;
		public GameObject ball;
		public Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
		public Vector3 force = new Vector3(0f, 0f, 7f);
		public float mass = 3f;

		void Update () {
			if (Input.GetKeyDown(keyCode)) {
				GameObject b = (GameObject)GameObject.Instantiate(ball, transform.position + transform.rotation * spawnOffset, transform.rotation);
				var r = b.GetComponent<Rigidbody>();

				if (r != null) {
					r.mass = mass;

					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					r.AddForce(Quaternion.LookRotation(ray.direction) * force, ForceMode.VelocityChange);
				}
			}
		}
	}
}
