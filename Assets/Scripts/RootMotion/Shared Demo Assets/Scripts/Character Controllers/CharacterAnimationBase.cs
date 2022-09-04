using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	/// <summary>
	/// The base abstract class for all character animation controllers.
	/// </summary>
	public abstract class CharacterAnimationBase: MonoBehaviour {

		public bool smoothFollow = true;
		public float smoothFollowSpeed = 20f;

		protected bool animatePhysics;
		private Vector3 lastPosition;
		private Vector3 localPosition;
		private Quaternion localRotation;
		private Quaternion lastRotation;

		// Gets the rotation pivot of the character
		public virtual Vector3 GetPivotPoint() {
			return transform.position;
		}

		// Is the animator playing the grounded state?
		public virtual bool animationGrounded { 
			get {
				return true;
			}
		}

		// Gets angle around y axis from a world space direction
		public float GetAngleFromForward(Vector3 worldDirection) {
			Vector3 local = transform.InverseTransformDirection(worldDirection);
			return Mathf.Atan2 (local.x, local.z) * Mathf.Rad2Deg;
		}

		protected virtual void Start() {
			if (transform.parent.GetComponent<CharacterBase>() == null) {
				Debug.LogWarning("Animation controllers should be parented to character controllers!", transform);
			}

			lastPosition = transform.position;
			localPosition = transform.localPosition;
			lastRotation = transform.rotation;
			localRotation = transform.localRotation;
		}

		protected virtual void LateUpdate() {
			if (animatePhysics) return;

			SmoothFollow();
		}

		// Smooth interpolation of character position. Helps to smooth out hectic rigidbody motion
		protected virtual void FixedUpdate() {
			if (!animatePhysics) return;

			SmoothFollow();
		}

		private void SmoothFollow() {
			if (smoothFollow) {
				transform.position = Vector3.Lerp(lastPosition, transform.parent.TransformPoint(localPosition), Time.deltaTime * smoothFollowSpeed);
				transform.rotation = Quaternion.Lerp(lastRotation, transform.parent.rotation * localRotation, Time.deltaTime * smoothFollowSpeed);
			} else
            {
                transform.localPosition = localPosition;
                transform.localRotation = localRotation;
            }

			lastPosition = transform.position;
			lastRotation = transform.rotation;
		}
	}

}
