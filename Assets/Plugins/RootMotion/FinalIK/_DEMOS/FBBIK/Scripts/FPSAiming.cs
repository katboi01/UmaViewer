using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Basic full body FPS IK controller.
	/// 
	/// If aimWeight is weighed in, the character will simply use AimIK to aim his gun towards the camera forward direction.
	/// If sightWeight is weighed in, the character will also use FBBIK to pose the gun to a predefined position relative to the camera so it stays fixed in view.
	/// That position was simply defined by making a copy of the gun (gunTarget), parenting it to the camera and positioning it so that the camera would look down it's sights.
	/// </summary>
	public class FPSAiming : MonoBehaviour {

		[Range(0f, 1f)] public float aimWeight = 1f; // The weight of aiming the gun towards camera forward
		[Range(0f, 1f)] public float sightWeight = 1f; // the weight of aiming down the sight (multiplied by aimWeight)
		[Range(0f, 180f)] public float maxAngle = 80f; // The maximum angular offset of the aiming direction from the character forward. Character will be rotated to comply.
		public Vector3 aimOffset; // Can be used to adjust the aiming angle

        public bool animatePhysics; // Is Animate Physiscs turned on for the character?
        public Transform gun; // The gun that the character is holding
        public Transform gunTarget; // The copy of the gun that has been parented to the camera
        public FullBodyBipedIK ik; // Reference to the FBBIK component
        public AimIK gunAim; // Reference to the AimIK component
        public CameraControllerFPS cam; // Reference to the FPS camera
        public Recoil recoil; // The recoil component (optional)
        [Range(0f, 1f)] public float cameraRecoilWeight = 0.5f; // How much of the recoil motion is added to the camera?

        public Vector3 gunTargetDefaultLocalPosition;
		public Vector3 gunTargetDefaultLocalRotation;
		private Vector3 camDefaultLocalPosition;
		private Vector3 camRelativeToGunTarget;
		private bool updateFrame;

		void Start() {
			// Remember some default local positions
			gunTargetDefaultLocalPosition = gunTarget.localPosition;
			gunTargetDefaultLocalRotation = gunTarget.localEulerAngles;
			camDefaultLocalPosition = cam.transform.localPosition;

			// Disable the camera and IK components so we can handle their execution order
			cam.enabled = false;
			gunAim.enabled = false;
			ik.enabled = false;

			if (recoil != null && ik.solver.iterations == 0) Debug.LogWarning("FPSAiming with Recoil needs FBBIK solver iteration count to be at least 1 to maintain accuracy.");
		}

		void FixedUpdate() {
			// Making sure this works with Animate Physics
			updateFrame = true;
		}


		void LateUpdate() {
			// Making sure this works with Animate Physics
			if (!animatePhysics) updateFrame = true;
			if (!updateFrame) return;
			updateFrame = false;

			// Put the camera back to it's default local position relative to the head
			cam.transform.localPosition = camDefaultLocalPosition;

			// Remember the camera's position relative to the gun target
			camRelativeToGunTarget = gunTarget.InverseTransformPoint(cam.transform.position);

			// Update the camera
			cam.LateUpdate();

			// Rotating the root of the character if it is past maxAngle from the camera forward
			RotateCharacter();

			Aiming();

			LookDownTheSight();
		}

		private void Aiming() {
			if (aimWeight <= 0f) return;
			
			// Remember the rotation of the camera because we need to reset it later so the IK would not interfere with the rotating of the camera
			Quaternion camRotation = cam.transform.rotation;

			// Aim the gun towards camera forward
			gunAim.solver.IKPosition = cam.transform.position + cam.transform.forward * 10f + cam.transform.rotation * aimOffset;
			gunAim.solver.IKPositionWeight = aimWeight;
			gunAim.solver.Update();
			cam.transform.rotation = camRotation;
		}

		private void LookDownTheSight() {
			float sW = aimWeight * sightWeight;
			//if (sW <= 0f && recoil == null) return;

			// Interpolate the gunTarget from the current animated position of the gun to the position fixed to the camera
			gunTarget.position = Vector3.Lerp(gun.position, gunTarget.parent.TransformPoint(gunTargetDefaultLocalPosition), sW);
			gunTarget.rotation = Quaternion.Lerp(gun.rotation, gunTarget.parent.rotation * Quaternion.Euler(gunTargetDefaultLocalRotation), sW);

			// Get the current positions of the hands relative to the gun
			Vector3 leftHandRelativePosition = gun.InverseTransformPoint(ik.solver.leftHandEffector.bone.position);
			Vector3 rightHandRelativePosition = gun.InverseTransformPoint(ik.solver.rightHandEffector.bone.position);

			// Get the current rotations of the hands relative to the gun
			Quaternion leftHandRelativeRotation = Quaternion.Inverse(gun.rotation) * ik.solver.leftHandEffector.bone.rotation;
			Quaternion rightHandRelativeRotation = Quaternion.Inverse(gun.rotation) * ik.solver.rightHandEffector.bone.rotation;

			//float handWeight = aimWeight > 0 && sightWeight > 0? aimWeight * sightWeight: 0f;
			float handWeight = 1f;//aimWeight * sightWeight;

			ik.solver.leftHandEffector.positionOffset += (gunTarget.TransformPoint(leftHandRelativePosition) - (ik.solver.leftHandEffector.bone.position + ik.solver.leftHandEffector.positionOffset)) * handWeight;
			ik.solver.rightHandEffector.positionOffset += (gunTarget.TransformPoint(rightHandRelativePosition) - (ik.solver.rightHandEffector.bone.position + ik.solver.rightHandEffector.positionOffset)) * handWeight;

			// Make sure the head does not rotate
			ik.solver.headMapping.maintainRotationWeight = 1f;

			if (recoil != null) recoil.SetHandRotations(gunTarget.rotation * leftHandRelativeRotation, gunTarget.rotation * rightHandRelativeRotation);

			// Update FBBIK
			ik.solver.Update();

			// Rotate the hand bones relative to the gun target the same way they are rotated relative to the gun
			if (recoil != null) {
				ik.references.leftHand.rotation = recoil.rotationOffset * (gunTarget.rotation * leftHandRelativeRotation);
				ik.references.rightHand.rotation = recoil.rotationOffset * (gunTarget.rotation * rightHandRelativeRotation);
			} else {
				ik.references.leftHand.rotation = gunTarget.rotation * leftHandRelativeRotation;
				ik.references.rightHand.rotation = gunTarget.rotation * rightHandRelativeRotation;
			}

			// Position the camera to where it was before FBBIK relative to the gun
			cam.transform.position = Vector3.Lerp(cam.transform.position, Vector3.Lerp(gunTarget.TransformPoint(camRelativeToGunTarget), gun.transform.TransformPoint(camRelativeToGunTarget), cameraRecoilWeight), sW);
		}

		// Rotating the root of the character if it is past maxAngle from the camera forward
		private void RotateCharacter() {
			if (maxAngle >= 180f) return;

			// If no angular difference is allowed, just rotate the character to the flattened camera forward
			if (maxAngle <= 0f) {
				transform.rotation = Quaternion.LookRotation(new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z));
				return;
			}

			// Get camera forward in the character's rotation space
			Vector3 camRelative = transform.InverseTransformDirection(cam.transform.forward);

			// Get the angle of the camera forward relative to the character forward
			float angle = Mathf.Atan2(camRelative.x, camRelative.z) * Mathf.Rad2Deg;

			// Making sure the angle does not exceed maxangle
			if (Mathf.Abs(angle) > Mathf.Abs(maxAngle)) {
				float a = angle - maxAngle;
				if (angle < 0f) a = angle + maxAngle;
				transform.rotation = Quaternion.AngleAxis(a, transform.up) * transform.rotation;
			}
		}
	}

}
