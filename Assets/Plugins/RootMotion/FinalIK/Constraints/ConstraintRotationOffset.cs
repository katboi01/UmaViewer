using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Offsets the transform from it's (animated) rotation
	/// </summary>
	[System.Serializable]
	public class ConstraintRotationOffset: Constraint {
		
		#region Main Interface
		
		/// <summary>
		/// The rotation offset in world space.
		/// </summary>
		public Quaternion offset;
		
		public override void UpdateConstraint() {
			if (weight <= 0) return;
			if (!isValid) return;
			
			// Initiating
			if (!initiated) {
				// Storing default rotations.
				defaultLocalRotation = transform.localRotation;
				lastLocalRotation = transform.localRotation;
				
				initiated = true;
			}
			
			// Check if rotation has changed. If true, set default local rotation to current.
			if (rotationChanged) defaultLocalRotation = transform.localRotation;
			
			// Offsetting the rotation
			transform.localRotation = defaultLocalRotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, offset, weight);
			
			// Store the current local rotation to check if it has changed in the next update.
			lastLocalRotation = transform.localRotation;
		}
		
		#endregion Main Interface
		
		public ConstraintRotationOffset() {}
		public ConstraintRotationOffset(Transform transform) {
			this.transform = transform;
		}
		
		private Quaternion defaultRotation, defaultLocalRotation, lastLocalRotation, defaultTargetLocalRotation;
		private bool initiated;
		
		/*
		 * Check if rotation has been changed by animation or any other external script. 
		 * If not, consider the object to be static and offset only from the default rotation.
		 * */
		private bool rotationChanged {
			get {
				return transform.localRotation != lastLocalRotation;
			}
		}
	}
}

