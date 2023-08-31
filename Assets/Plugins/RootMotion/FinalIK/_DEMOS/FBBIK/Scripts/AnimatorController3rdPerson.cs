using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Basic Mecanim Animator controller for 3rd person view.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class AnimatorController3rdPerson : MonoBehaviour {

		public float rotateSpeed = 7f; // Speed of rotating the character
		public float blendSpeed = 10f; // Animation blending speed
		public float maxAngle = 90f; // Max angular offset from camera direction
		public float moveSpeed = 1.5f; // The speed of moving the character with no root motion
		public float rootMotionWeight; // Crossfading between procedural movement and root motion

		protected Animator animator; // The Animator
		protected Vector3 moveBlend, moveInput, velocity;

		protected virtual void Start() {
			animator = GetComponent<Animator>();
		}

		// Moving the character
		void OnAnimatorMove() {
			velocity = Vector3.Lerp (velocity, transform.rotation * Vector3.ClampMagnitude(moveInput, 1f) * moveSpeed, Time.deltaTime * blendSpeed);

			// Crossfading between procedural movement and root motion.
			transform.position += Vector3.Lerp(velocity * Time.deltaTime, animator.deltaPosition, rootMotionWeight);
		}

		// Move the character
		public virtual void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget) {
			// Store variables that we need in other methods
			this.moveInput = moveInput;

			// Get the facing direction relative to the character rotation
			Vector3 faceDirectionLocal = transform.InverseTransformDirection(faceDirection);

			// Get the angle between the facing direction and character forward
			float angle = Mathf.Atan2(faceDirectionLocal.x, faceDirectionLocal.z) * Mathf.Rad2Deg;

			// Find the rotation
			float rotation = angle * Time.deltaTime * rotateSpeed;

			// Clamp the rotation to maxAngle
			if (angle > maxAngle) rotation = Mathf.Clamp(rotation, angle - maxAngle, rotation);
			if (angle < -maxAngle) rotation = Mathf.Clamp(rotation, rotation, angle + maxAngle);

			// Rotate the character
			transform.Rotate(Vector3.up, rotation);

			// Locomotion animation blending
			moveBlend = Vector3.Lerp(moveBlend, moveInput, Time.deltaTime * blendSpeed);

			// Set Animator parameters
			animator.SetFloat("X", moveBlend.x);
			animator.SetFloat("Z", moveBlend.z);
			animator.SetBool("IsMoving", isMoving);
		}
	}
}
