using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	// Extends the default Animator controller for 3rd person view to add IK
	[RequireComponent(typeof(AimIK))]
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class AnimatorController3rdPersonIK: AnimatorController3rdPerson {

		[Range(0f, 1f)] public float headLookWeight = 1f;
		public Vector3 gunHoldOffset;
		public Vector3 leftHandOffset;
		public Recoil recoil;

		// The IK components
		private AimIK aim;
		private FullBodyBipedIK ik;

		private Vector3 headLookAxis;
		private Vector3 leftHandPosRelToRightHand;
		private Quaternion leftHandRotRelToRightHand;
		private Vector3 aimTarget;
		private Quaternion rightHandRotation;

		protected override void Start() {
			base.Start();
			
			// Find the IK components
			aim = GetComponent<AimIK>();
			ik = GetComponent<FullBodyBipedIK>();
			ik.solver.OnPreRead += OnPreRead;
			
			// Disable the IK components to manage their updating
			aim.enabled = false;
			ik.enabled = false;

			// Presuming head is rotated towards character forward at Start
			headLookAxis = ik.references.head.InverseTransformVector(ik.references.root.forward);
			
			// Enable the upper-body aiming pose
			animator.SetLayerWeight(1, 1f);
		}

		public override void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget) {
			base.Move(moveInput, isMoving, faceDirection, aimTarget);

			// Snatch the aim target from the Move call, it will be used by AimIK (Move is called by CharacterController3rdPerson that controls the actual motion of the character)
			this.aimTarget = aimTarget;

			// IK procedures, make sure this updates AFTER the camera is moved/rotated
			// Sample something from the current pose of the character
			Read();

            // AimIK pass
            AimIK();

            // FBBIK pass - put the left hand back to where it was relative to the right hand before AimIK solved
            FBBIK();

            // AimIK pass
            AimIK();

            // Rotate the head to look at the aim target
            HeadLookAt(aimTarget);
		}

		private void Read() {
			// Remember the position and rotation of the left hand relative to the right hand
			leftHandPosRelToRightHand = ik.references.rightHand.InverseTransformPoint(ik.references.leftHand.position);
			leftHandRotRelToRightHand = Quaternion.Inverse(ik.references.rightHand.rotation) * ik.references.leftHand.rotation;
		}

		private void AimIK() {
			// Set AimIK target position and update
			aim.solver.IKPosition = aimTarget;
			aim.solver.Update(); // Update AimIK
		}

		// Positioning the left hand on the gun after aiming has finished
		private void FBBIK() {
			// Store the current rotation of the right hand
			rightHandRotation = ik.references.rightHand.rotation;

			// Offsetting hands, you might need that to support multiple weapons with the same aiming pose
			Vector3 rightHandOffset = ik.references.rightHand.rotation * gunHoldOffset;
			ik.solver.rightHandEffector.positionOffset += rightHandOffset;

			if (recoil != null) recoil.SetHandRotations(rightHandRotation * leftHandRotRelToRightHand, rightHandRotation);

			// Update FBBIK
			ik.solver.Update();
			
			// Rotating the hand bones after IK has finished
			if (recoil != null) {
				ik.references.rightHand.rotation = recoil.rotationOffset * rightHandRotation;
				ik.references.leftHand.rotation = recoil.rotationOffset * rightHandRotation * leftHandRotRelToRightHand;
			} else {
				ik.references.rightHand.rotation = rightHandRotation;
				ik.references.leftHand.rotation = rightHandRotation * leftHandRotRelToRightHand;
			}
		}

		// Final calculations before FBBIK solves. Recoil has already solved by, so we can use it's calculated offsets. 
		// Here we set the left hand position relative to the position and rotation of the right hand.
		private void OnPreRead() {
			Quaternion r = recoil != null? recoil.rotationOffset * rightHandRotation: rightHandRotation;
			Vector3 leftHandTarget = ik.references.rightHand.position + ik.solver.rightHandEffector.positionOffset + r * leftHandPosRelToRightHand;
			ik.solver.leftHandEffector.positionOffset += leftHandTarget - ik.references.leftHand.position - ik.solver.leftHandEffector.positionOffset + r * leftHandOffset;
		}

		// Rotating the head to look at the target
		private void HeadLookAt(Vector3 lookAtTarget) {
			Quaternion headRotationTarget = Quaternion.FromToRotation(ik.references.head.rotation * headLookAxis, lookAtTarget - ik.references.head.position);
			ik.references.head.rotation = Quaternion.Lerp(Quaternion.identity, headRotationTarget, headLookWeight) * ik.references.head.rotation;
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (ik != null) ik.solver.OnPreRead -= OnPreRead;
		}
	}
}
