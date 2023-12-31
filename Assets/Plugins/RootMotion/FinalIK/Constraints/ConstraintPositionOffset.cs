using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Offsets the transform from it's (animated) position.
	/// </summary>
	[System.Serializable]
	public class ConstraintPositionOffset : Constraint {
		
		#region Main Interface
		
		/// <summary>
		/// The position offset in world space.
		/// </summary>
		public Vector3 offset;
		
		public override void UpdateConstraint() {
			if (weight <= 0) return;
			if (!isValid) return;
			
			// Initiating
			if (!initiated) {
				// Storing default values
				defaultLocalPosition = transform.localPosition;
				lastLocalPosition = transform.localPosition;
				
				initiated = true;
			}
			
			// Check if position has changed. If true, set default local position to current.
			if (positionChanged) defaultLocalPosition = transform.localPosition;
			
			// Offsetting the position
			transform.localPosition = defaultLocalPosition;
			transform.position += offset * weight;
			
			// Store the current local position to check if it has changed in the next update.
			lastLocalPosition = transform.localPosition;
		}
		
		#endregion Main Interface
		
		public ConstraintPositionOffset() {}
		
		public ConstraintPositionOffset(Transform transform) {
			this.transform = transform;
		}
		
		private Vector3 defaultLocalPosition, lastLocalPosition;
		private bool initiated;
		
		/*
		 * Check if position has been changed by animation or any other external script. 
		 * If not, consider the object to be static and offset only from the default rotation.
		 * */
		private bool positionChanged {
			get {
				return transform.localPosition != lastLocalPosition;
			}
		}
	}
}
