using RootMotion.Demos;
using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	// Demonstrating 360-degree aiming system built with 6 static aiming poses and AimIK.
	public class SimpleAimingSystem : MonoBehaviour {
		
		[Tooltip("AimPoser is a tool that returns an animation name based on direction.")]
		public AimPoser aimPoser;
		
		[Tooltip("Reference to the AimIK component.")]
		public AimIK aim;
		
		[Tooltip("Reference to the LookAt component (only used for the head in this instance).")]
		public LookAtIK lookAt;
		
		[Tooltip("Reference to the Animator component.")]
		public Animator animator;
		
		[Tooltip("Time of cross-fading from pose to pose.")]
		public float crossfadeTime = 0.2f;

		[Tooltip("Will keep the aim target at a distance.")]
		public float minAimDistance = 0.5f;
		
		private AimPoser.Pose aimPose, lastPose;

		void Start() {
			// Disable IK components to manage their updating order
			aim.enabled = false;
			lookAt.enabled = false;
		}
		
		// LateUpdate is called once per frame
		void LateUpdate () {
			if (aim.solver.target == null) {
				Debug.LogWarning("AimIK and LookAtIK need to have their 'Target' value assigned.", transform);
			}

			// Switch aim poses (Legacy animation)
			Pose();

			// Update IK solvers
			aim.solver.Update();
			if (lookAt != null) lookAt.solver.Update();
		}
		
		private void Pose() {
			// Make sure aiming target is not too close (might make the solver instable when the target is closer to the first bone than the last bone is).
			LimitAimTarget();

			// Get the aiming direction
			Vector3 direction = (aim.solver.target.position - aim.solver.bones[0].transform.position);
			// Getting the direction relative to the root transform
			Vector3 localDirection = transform.InverseTransformDirection(direction);
			
			// Get the Pose from AimPoser
			aimPose = aimPoser.GetPose(localDirection);
			
			// If the Pose has changed
			if (aimPose != lastPose) {
				// Increase the angle buffer of the pose so we won't switch back too soon if the direction changes a bit
				aimPoser.SetPoseActive(aimPose);
				
				// Store the pose so we know if it changes
				lastPose = aimPose;
			}
			
			// Direct blending
			foreach (AimPoser.Pose pose in aimPoser.poses) {
				if (pose == aimPose) {
					DirectCrossFade(pose.name, 1f);
				} else {
					DirectCrossFade(pose.name, 0f);
				}
			}
		}
		
		// Make sure aiming target is not too close (might make the solver instable when the target is closer to the first bone than the last bone is).
		void LimitAimTarget() {
			Vector3 aimFrom = aim.solver.bones[0].transform.position;
			Vector3 direction = (aim.solver.target.position - aimFrom);
			direction = direction.normalized * Mathf.Max(direction.magnitude, minAimDistance);
			
			aim.solver.target.position = aimFrom + direction;
		}
		
		// Uses Mecanim's Direct blend trees for cross-fading
		private void DirectCrossFade(string state, float target) {
			float f = Mathf.MoveTowards(animator.GetFloat(state), target, Time.deltaTime * (1f / crossfadeTime));
			animator.SetFloat(state, f);
		}
	}
}
