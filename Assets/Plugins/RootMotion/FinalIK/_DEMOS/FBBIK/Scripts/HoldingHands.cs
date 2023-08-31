using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Holding hands rig.
	/// </summary>
	public class HoldingHands : MonoBehaviour {

		public FullBodyBipedIK rightHandChar, leftHandChar; // The characters

		public Transform rightHandTarget, leftHandTarget; // IK targets for the hands
		public float crossFade; // Which character is dominating?
		public float speed = 10f; // Speed of smoothly lerping the hands target

		private Quaternion rightHandRotation, leftHandRotation;

		void Start() {
			// Find the rotations of the hands target (this gameobject) in the rotation spaces of the hand bones
			rightHandRotation = Quaternion.Inverse(rightHandChar.solver.rightHandEffector.bone.rotation) * transform.rotation;
			leftHandRotation = Quaternion.Inverse(leftHandChar.solver.leftHandEffector.bone.rotation) * transform.rotation;
		}

		void LateUpdate () {
			// Positioning the hands target
			Vector3 targetPosition = Vector3.Lerp(rightHandChar.solver.rightHandEffector.bone.position, leftHandChar.solver.leftHandEffector.bone.position, crossFade);
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

			// Rotating the hands target
			transform.rotation = Quaternion.Slerp(rightHandChar.solver.rightHandEffector.bone.rotation * rightHandRotation, leftHandChar.solver.leftHandEffector.bone.rotation * leftHandRotation, crossFade);

			// Set effector positions and rotations
			rightHandChar.solver.rightHandEffector.position = rightHandTarget.position;
			rightHandChar.solver.rightHandEffector.rotation = rightHandTarget.rotation;
			
			leftHandChar.solver.leftHandEffector.position = leftHandTarget.position;
			leftHandChar.solver.leftHandEffector.rotation = leftHandTarget.rotation;
		}
	}
}
