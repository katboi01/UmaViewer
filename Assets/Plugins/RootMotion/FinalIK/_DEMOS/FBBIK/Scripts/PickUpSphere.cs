using UnityEngine;
using System.Collections;
using RootMotion;
using RootMotion.FinalIK;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Picking up a sphere with both hands.
	/// </summary>
	public class PickUpSphere : PickUp2Handed {

		// Rotate the pivot of the hand targets so we could grab the object from any direction
		protected override void RotatePivot() {
			// Find the center of the hand bones
			Vector3 handsCenter = Vector3.Lerp(interactionSystem.ik.solver.leftHandEffector.bone.position, interactionSystem.ik.solver.rightHandEffector.bone.position, 0.5f);

			// Direction from that center to the interaction object
			Vector3 dir = obj.transform.position - handsCenter;

			// Rotate the pivot of the hand targets
			pivot.rotation = Quaternion.LookRotation(dir);
		}
	}
}
