using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	// The simplest multi-purpose locomotion controller for demo purposes. Can use root motion, simple procedural motion or the CharacterController
	[RequireComponent(typeof(Animator))]
	public class SimpleLocomotion : MonoBehaviour {

		// The character rotation mode
		[System.Serializable]
		public enum RotationMode {
			Smooth,
			Linear
		}

        [Tooltip("The component that updates the camera.")]
        public CameraController cameraController;

        [Tooltip("Acceleration of movement.")]
        public float accelerationTime = 0.2f;

        [Tooltip("Turning speed.")]
        public float turnTime = 0.2f;

        [Tooltip("If true, will run on left shift, if not will walk on left shift.")]
        public bool walkByDefault = true;

        [Tooltip("Smooth or linear rotation.")]
        public RotationMode rotationMode;

        [Tooltip("Procedural motion speed (if not using root motion).")]
        public float moveSpeed = 3f;

        // Is the character grounded (using very simple y < something here for simplicity's sake)?
        public bool isGrounded { get; private set; }

		private Animator animator;
		private float speed;
		private float angleVel;
		private float speedVel;
		private Vector3 linearTargetDirection;
		private CharacterController characterController;

		void Start() {
			animator = GetComponent<Animator>();
			characterController = GetComponent<CharacterController>();
			cameraController.enabled = false;
		}

		void Update() {
			// Very basic planar method, should use collision events
			isGrounded = transform.position.y < 0.1f;

			Rotate();
			Move();
		}

		void LateUpdate() {
			// Update the camera last
			cameraController.UpdateInput();
			cameraController.UpdateTransform();
		}

		private void Rotate() {
			if (!isGrounded) return;

			// Updating the rotation of the character
			Vector3 inputVector = GetInputVector();
			if (inputVector == Vector3.zero) return;

			Vector3 forward = transform.forward;

			switch(rotationMode) {
			case RotationMode.Smooth:
				Vector3 targetDirection = cameraController.transform.rotation * inputVector;
					
				float angleForward = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
				float angleTarget = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
					
				// Smoothly rotating the character
				float angle = Mathf.SmoothDampAngle(angleForward, angleTarget, ref angleVel, turnTime);
				transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

				break;
			case RotationMode.Linear:
				Vector3 inputVectorRaw = GetInputVectorRaw();
				if (inputVectorRaw != Vector3.zero) linearTargetDirection = cameraController.transform.rotation * inputVectorRaw;

				forward = Vector3.RotateTowards(forward, linearTargetDirection, Time.deltaTime * (1f /turnTime), 1f);
				forward.y = 0f;
				transform.rotation = Quaternion.LookRotation(forward);
				break;
			}
		}

		private void Move() {
			// Speed interpolation
			float speedTarget = walkByDefault? (Input.GetKey(KeyCode.LeftShift)? 1f: 0.5f): (Input.GetKey(KeyCode.LeftShift)? 0.5f: 1f);
			speed = Mathf.SmoothDamp(speed, speedTarget, ref speedVel, accelerationTime);

			// Moving the character by root motion
			float s = GetInputVector().magnitude * speed;
			animator.SetFloat("Speed", s);

			// Procedural motion if we don't have root motion
			bool proceduralMotion = !animator.hasRootMotion && isGrounded;

			if (proceduralMotion) {
				Vector3 move = transform.forward * s * moveSpeed;

				if (characterController != null) {
					characterController.SimpleMove(move);
				} else {
					transform.position += move * Time.deltaTime;
				}
			}
		}
		
		// Reads the Input to get the movement direction.
		private Vector3 GetInputVector() {
			Vector3 d = new Vector3(
				Input.GetAxis("Horizontal"),
				0f,
				Input.GetAxis("Vertical")
				);
			
			d.z += Mathf.Abs(d.x) * 0.05f;
			d.x -= Mathf.Abs(d.z) * 0.05f;

			return d;
		}

		private Vector3 GetInputVectorRaw() {
			return new Vector3(
				Input.GetAxisRaw("Horizontal"),
				0f,
				Input.GetAxisRaw("Vertical")
				);
		}
	}
}
