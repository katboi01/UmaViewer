using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Contains and manages a set of constraints.
	/// </summary>
	[System.Serializable]
	public class Constraints {
		
		#region Main Interface

		/// <summary>
		/// The transform.
		/// </summary>
		public Transform transform;
		/// <summary>
		/// The target.
		/// </summary>
		public Transform target;
		/// <summary>
		/// The position offset.
		/// </summary>
		public Vector3 positionOffset;
		/// <summary>
		/// The position to lerp to by positionWeight
		/// </summary>
		public Vector3 position;
		/// <summary>
		/// The weight of lerping to position
		/// </summary>
		[Range(0f, 1f)]
		public float positionWeight;
		/// <summary>
		/// The rotation offset.
		/// </summary>
		public Vector3 rotationOffset;
		/// <summary>
		/// The rotation to slerp to by rotationWeight
		/// </summary>
		public Vector3 rotation;
		/// <summary>
		/// The weight of slerping to rotation
		/// </summary>
		[Range(0f, 1f)]
		public float rotationWeight;

		/// <summary>
		/// Determines whether this instance is valid.
		/// </summary>
		public bool IsValid() {
			return transform != null;
		}

		/// <summary>
		/// Initiate to the specified transform.
		/// </summary>
		public void Initiate(Transform transform) {
			this.transform = transform;
			this.position = transform.position;
			this.rotation = transform.eulerAngles;
		}

		/// <summary>
		/// Updates the constraints.
		/// </summary>
		public void Update() {
			if (!IsValid()) return;

			// Position
			if (target != null) position = target.position;
			transform.position += positionOffset;
			if (positionWeight > 0f) transform.position = Vector3.Lerp(transform.position, position, positionWeight);

			// Rotation
			if (target != null) rotation = target.eulerAngles;
			transform.rotation = Quaternion.Euler(rotationOffset) * transform.rotation;
			if (rotationWeight > 0f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), rotationWeight);
		}
		
		#endregion Main Interface
	}
}
