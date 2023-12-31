using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %Constraints to position in world space.
	/// </summary>
	[System.Serializable]
	public class ConstraintPosition : Constraint {
		
		#region Main Interface
		
		/// <summary>
		/// The target position.
		/// </summary>
		public Vector3 position;
		
		public override void UpdateConstraint() {
			if (weight <= 0) return;
			if (!isValid) return;
			
			// Lerping to position
			transform.position = Vector3.Lerp(transform.position, position, weight);
		}
		
		#endregion Main Interface
		
		public ConstraintPosition() {}
		public ConstraintPosition(Transform transform) {
			this.transform = transform;
		}
	}
}
