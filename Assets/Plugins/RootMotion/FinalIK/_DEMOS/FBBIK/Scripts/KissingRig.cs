using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// FBBIK example rig for a kissing emote. 
	/// As the IK targets of one FBBIK partner depend on the solved pose of another FBBIK partner and vice versa, the rig will have to be iterated a couple of times to make it work.
	/// </summary>
	public class KissingRig : MonoBehaviour {

		/// <summary>
		/// A partner in the emote
		/// </summary>
		[System.Serializable]
		public class Partner {

			public FullBodyBipedIK ik; // Reference to the FBBIK component
			public Transform mouth; // The mouth bone, should be attached to the head
			public Transform mouthTarget; // The target that we want to set the mouth bone to
			public Transform touchTargetLeftHand, touchTargetRightHand; // Touch targets for the hands
			public float bodyWeightHorizontal = 0.4f; // The body effector horizontal weight
			public float bodyWeightVertical = 1f; // The body effector vertical weight
			public float neckRotationWeight = 0.3f; // The neck rotation weight
			public float headTiltAngle = 10f; // Tilting the head
			public Vector3 headTiltAxis; // Head tilt axis

			private Quaternion neckRotation;

			public void Initiate() {
				// Disable the FBBIK component to manage its updating
				ik.enabled = false;
			}

			public void Update(float weight) {
				// Set IK position and rotation weights
				ik.solver.leftShoulderEffector.positionWeight = weight;
				ik.solver.rightShoulderEffector.positionWeight = weight;
				ik.solver.leftHandEffector.positionWeight = weight;
				ik.solver.rightHandEffector.positionWeight = weight;
				ik.solver.leftHandEffector.rotationWeight = weight;
				ik.solver.rightHandEffector.rotationWeight = weight;
				ik.solver.bodyEffector.positionWeight = weight;

				// Inverse transform the shoulder and body effectors to set them relative to the mouth bone's position in the animation
				InverseTransformEffector(FullBodyBipedEffector.LeftShoulder, mouth, mouthTarget.position, weight);
				InverseTransformEffector(FullBodyBipedEffector.RightShoulder, mouth, mouthTarget.position, weight);
				InverseTransformEffector(FullBodyBipedEffector.Body, mouth, mouthTarget.position, weight);

				// Positioning the body effector horizontally
				ik.solver.bodyEffector.position = Vector3.Lerp(new Vector3(ik.solver.bodyEffector.position.x, ik.solver.bodyEffector.bone.position.y, ik.solver.bodyEffector.position.z), ik.solver.bodyEffector.position, bodyWeightVertical * weight);

				// Positioning the body effector vertically
				ik.solver.bodyEffector.position = Vector3.Lerp(new Vector3(ik.solver.bodyEffector.bone.position.x, ik.solver.bodyEffector.position.y, ik.solver.bodyEffector.bone.position.z), ik.solver.bodyEffector.position, bodyWeightHorizontal * weight);

				// Set hand effector positions to touch targets
				ik.solver.leftHandEffector.position = touchTargetLeftHand.position;
				ik.solver.rightHandEffector.position = touchTargetRightHand.position;
				ik.solver.leftHandEffector.rotation = touchTargetLeftHand.rotation;
				ik.solver.rightHandEffector.rotation = touchTargetRightHand.rotation;

				// Store the neck rotation so we could slerp back to it after updating FBBIK
				neckRotation = neck.rotation;

				// Update the FBBIK solver
				ik.solver.Update();

				// Revert the neck back to its animated rotation
				neck.rotation = Quaternion.Slerp(neck.rotation, neckRotation, neckRotationWeight * weight);

				// Head tilting
				ik.references.head.localRotation = Quaternion.AngleAxis(headTiltAngle * weight, headTiltAxis) * ik.references.head.localRotation;
			}

			// Get the neck bone
			private Transform neck {
				get {
					return ik.solver.spineMapping.spineBones[ik.solver.spineMapping.spineBones.Length - 1];
				}
			}

			// Placing an effector so that an arbitrary Transform (target) ends up at targetPosition
			private void InverseTransformEffector(FullBodyBipedEffector effector, Transform target, Vector3 targetPosition, float weight) {
				// Direction from the target to the effector
				Vector3 toEffector = ik.solver.GetEffector(effector).bone.position - target.position;

				// Positioning the effector
				ik.solver.GetEffector(effector).position = Vector3.Lerp(ik.solver.GetEffector(effector).bone.position, targetPosition + toEffector, weight);
			}
		}

		public Partner partner1, partner2; // The partners

		public float weight; // The master weight
		public int iterations = 3; // The number of iterating this rig. 
		// As the IK targets of one FBBIK partner depend on the solved pose of another FBBIK partner and vice versa, 
		// the rig will have to be iterated a couple of times to make it work.

		void Start() {
			// Initiating the partners
			partner1.Initiate();
			partner2.Initiate();
		}

		void LateUpdate() {
			// Iterate the rig
			for (int i = 0; i < iterations; i++) {
				partner1.Update(weight);
				partner2.Update(weight);
			}
		}
	}
}
