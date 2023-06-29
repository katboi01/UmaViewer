using UnityEngine;
using System.Collections;
using RootMotion;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Picking up a box shaped object with both hands.
	/// </summary>
	public class PickUpBox : PickUp2Handed {

		// Rotate the pivot of the hand targets by 90 degrees so we could grab the object from any direction
		protected override void RotatePivot() {
			// Get the flat direction towards the character
			Vector3 characterDirection = (pivot.position - interactionSystem.transform.position).normalized;
			characterDirection.y = 0f;
			
			// Convert the direction to local space of the object
			Vector3 characterDirectionLocal = obj.transform.InverseTransformDirection(characterDirection);
			
			// QuaTools.GetAxis returns a 90 degree ortographic axis for any direction
			Vector3 axis = QuaTools.GetAxis(characterDirectionLocal);
			Vector3 upAxis = QuaTools.GetAxis(obj.transform.InverseTransformDirection(interactionSystem.transform.up));
			
			// Rotate towards axis and upAxis
			pivot.localRotation = Quaternion.LookRotation(axis, upAxis);
		}
	}
}
