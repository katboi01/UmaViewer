using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	/// <summary>
	/// Basic Mecanim character controller for 3rd person view.
	/// </summary>
	[RequireComponent(typeof(AnimatorController3rdPerson))]
	public class CharacterController3rdPerson: MonoBehaviour {

		public CameraController cam; // The camera

		private AnimatorController3rdPerson animatorController; // The Animator controller

		void Start() {
			animatorController = GetComponent<AnimatorController3rdPerson>();

			cam.enabled = false;
		}

		void LateUpdate() {
			// Update the camera first so we always have its final translation in the frame
			cam.UpdateInput();
			cam.UpdateTransform();

			// Read the input
			Vector3 input = inputVector;

			// Should the character be moving? 
			// inputVectorRaw is required here for not starting a transition to idle on that one frame where inputVector is Vector3.zero when reversing directions.
			bool isMoving = inputVector != Vector3.zero || inputVectorRaw != Vector3.zero;

			// Character look at vector.
			Vector3 lookDirection = cam.transform.forward;

			// Aiming target
			Vector3 aimTarget = cam.transform.position + (lookDirection * 10f);

			// Move the character.
			animatorController.Move(input, isMoving, lookDirection, aimTarget);
		}

		// Convert the input axis to a vector
		private static Vector3 inputVector {
			get {
				return new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			}
		}

		// Convert the raw input axis to a vector
		private static Vector3 inputVectorRaw {
			get {
				return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			}
		}
	}
}