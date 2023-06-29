using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %Constraints to rotation in world space
	/// </summary>
	[System.Serializable]
	public class ConstraintRotation : Constraint {
		
		#region Main Interface
		
		/// <summary>
		/// The target rotation.
		/// </summary>
		public Quaternion rotation;
		
		public override void UpdateConstraint() {
			if (weight <= 0) return;
			if (!isValid) return;
			
			// Slerping to target rotation
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, weight);
		}
		
		#endregion Main Interface
		
		public ConstraintRotation() {}
		public ConstraintRotation(Transform transform) {
			this.transform = transform;
		}
	}
}