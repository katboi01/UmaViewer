using UnityEngine;
using System.Collections;
using RootMotion;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Demo for aiming a Turret. All it does is call Transform.LookAt on each part and apply rotation limits one by one, starting from the parent.
	/// </summary>
	public class Turret : MonoBehaviour {

		/// <summary>
		/// An independent part of the turret
		/// </summary>
		[System.Serializable]
		public class Part {

			public Transform transform; // The Transform
			private RotationLimit rotationLimit; // The Rotation Limit component

			// Aim this part at the target
			public void AimAt(Transform target) {
				transform.LookAt(target.position, transform.up);

				// Finding the Rotation Limit
				if (rotationLimit == null) {
					rotationLimit = transform.GetComponent<RotationLimit>();
					rotationLimit.Disable();
				}

				// Apply rotation limits
				rotationLimit.Apply();
			}
		}

		public Transform target; // The aiming target
		public Part[] parts; // All the turret parts

		void Update() {
			// Rotate the turret parts one by one
			foreach (Part part in parts) part.AimAt(target);
		}
	}
}
