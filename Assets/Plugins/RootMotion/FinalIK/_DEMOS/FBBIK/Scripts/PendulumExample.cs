using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Making a character hold on to a target and swing about it while maintaining his animation.
	/// </summary>
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class PendulumExample : MonoBehaviour {

		[Tooltip("The master weight of this script.")]
		[Range(0f, 1f)] public float weight = 1f;

		[Tooltip("Multiplier for the distance of the root to the target.")]
		public float hangingDistanceMlp = 1.3f;

		[Tooltip("Where does the root of the character land when weight is blended out?")]
		[HideInInspector] public Vector3 rootTargetPosition;

		[Tooltip("How is the root of the character rotated when weight is blended out?")]
		[HideInInspector] public Quaternion rootTargetRotation;

        public Transform target;
        public Transform leftHandTarget;
        public Transform rightHandTarget;
        public Transform leftFootTarget;
        public Transform rightFootTarget;
        public Transform pelvisTarget;
        public Transform bodyTarget;
        public Transform headTarget;
        public Vector3 pelvisDownAxis = Vector3.right;

        private FullBodyBipedIK ik;
		private Quaternion rootRelativeToPelvis;
		private Vector3 pelvisToRoot;
		private float lastWeight;

		void Start() {
			ik = GetComponent<FullBodyBipedIK>();

			// Connect the left hand to the target
			Quaternion targetRotation = target.rotation;
			target.rotation = leftHandTarget.rotation;

			FixedJoint j = target.gameObject.AddComponent<FixedJoint>();
			j.connectedBody = leftHandTarget.GetComponent<Rigidbody>();

			target.GetComponent<Rigidbody>().MoveRotation(targetRotation);
			//target.rotation = targetRotation;

			// Remember the rotation of the root relative to the pelvis
			rootRelativeToPelvis = Quaternion.Inverse(pelvisTarget.rotation) * transform.rotation;

			// Remember the position of the root relative to the pelvis
			pelvisToRoot = Quaternion.Inverse(ik.references.pelvis.rotation) * (transform.position - ik.references.pelvis.position);

			rootTargetPosition = transform.position;
			rootTargetRotation = transform.rotation;

			lastWeight = weight;
		}

		void LateUpdate() {
			// Set effector weights
			if (weight > 0f) {
				ik.solver.leftHandEffector.positionWeight = weight;
				ik.solver.leftHandEffector.rotationWeight = weight;
			} else {
				rootTargetPosition = transform.position;
				rootTargetRotation = transform.rotation;

				if (lastWeight > 0f) {
					ik.solver.leftHandEffector.positionWeight = 0f;
					ik.solver.leftHandEffector.rotationWeight = 0f;
				}
			}

			lastWeight = weight;
			if (weight <= 0f) return;

			// Position the character relative to the ragdoll pelvis
			transform.position = Vector3.Lerp(rootTargetPosition, pelvisTarget.position + pelvisTarget.rotation * pelvisToRoot * hangingDistanceMlp, weight);

			// Rotate the character to the ragdoll pelvis
			transform.rotation = Quaternion.Lerp(rootTargetRotation, pelvisTarget.rotation * rootRelativeToPelvis, weight);

			// Set ik effector positions
			ik.solver.leftHandEffector.position = leftHandTarget.position;
			ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;

			// Get the normal hanging direction
			Vector3 dir = ik.references.pelvis.rotation * pelvisDownAxis;

			// Rotating the limbs
			// Get the rotation from normal hangind direction to the right arm ragdoll direction
			Quaternion rightArmRot = Quaternion.FromToRotation(dir, rightHandTarget.position - headTarget.position);
			// Rotate the right arm by that offset
			ik.references.rightUpperArm.rotation = Quaternion.Lerp(Quaternion.identity, rightArmRot, weight) * ik.references.rightUpperArm.rotation;
			
			Quaternion leftLegRot = Quaternion.FromToRotation(dir, leftFootTarget.position - bodyTarget.position);
			ik.references.leftThigh.rotation = Quaternion.Lerp(Quaternion.identity, leftLegRot, weight) * ik.references.leftThigh.rotation;
			
			Quaternion rightLegRot = Quaternion.FromToRotation(dir, rightFootTarget.position - bodyTarget.position);
			ik.references.rightThigh.rotation = Quaternion.Lerp(Quaternion.identity, rightLegRot, weight) * ik.references.rightThigh.rotation;
		}
	}
}
