using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	/// <summary>
	/// Moving and rotating platforms.
	/// </summary>
	public class PlatformRotator : MonoBehaviour {

		public float maxAngle = 70f; // Maximum angular offset from the default rotation
		public float switchRotationTime = 0.5f; // Base time for switching to another target rotation
		public float random = 0.5f; // The random mlp for timers
		public float rotationSpeed = 50f; // The slerp speed
		public Vector3 movePosition; // Move to offset
		public float moveSpeed = 5f; // Moving speed
		public int characterLayer; // Which layer are the characters on?

		private Quaternion defaultRotation;
		private Quaternion targetRotation;
		private Vector3 targetPosition;
		private Vector3 velocity;
		private Rigidbody r;

		void Start () {
			// Store defaults
			defaultRotation = transform.rotation;
			targetPosition = transform.position + movePosition;

			r = GetComponent<Rigidbody>();

			// Start switching target rotations
			StartCoroutine(SwitchRotation());
		}

		void FixedUpdate() {
			// Moving
			r.MovePosition(Vector3.SmoothDamp(r.position, targetPosition, ref velocity, 1f, moveSpeed));

			if (Vector3.Distance(GetComponent<Rigidbody>().position, targetPosition) < 0.1f) {
				movePosition = -movePosition;
				targetPosition += movePosition;
			}

			// Rotating
			r.MoveRotation(Quaternion.RotateTowards(r.rotation, targetRotation, rotationSpeed * Time.deltaTime));
		}

		// Switching the  target rotation
		private IEnumerator SwitchRotation() {
			while (true) {
				// Random rotation around a random axis
				float angle = UnityEngine.Random.Range(-maxAngle, maxAngle);
				Vector3 axis = UnityEngine.Random.onUnitSphere;
				targetRotation = Quaternion.AngleAxis(angle, axis) * defaultRotation;

				yield return new WaitForSeconds(switchRotationTime + UnityEngine.Random.value * random);
			}
		}

		// Disable fixed time step smoothing on the characters that hit this platform
		void OnCollisionEnter(Collision collision) {
			if (collision.gameObject.layer == characterLayer) {
				CharacterThirdPerson c = collision.gameObject.GetComponent<CharacterThirdPerson>();
				if (c == null) return;
				if (c.smoothPhysics) {
					c.smoothPhysics = false;
				}
			}
		}

		// Re-enable fixed time step smoothing on the characters that exited this platform
		void OnCollisionExit(Collision collision) {
			if (collision.gameObject.layer == characterLayer) {
				CharacterThirdPerson c = collision.gameObject.GetComponent<CharacterThirdPerson>();
				if (c == null) return;
				c.smoothPhysics = true;
			}
		}
	}
}
