using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Demo script that shows how BipedIK performs compared to the built-in Animator IK
	/// </summary>
	public class BipedIKvsAnimatorIK: MonoBehaviour {

		[LargeHeader("References")]
		public Animator animator;
		public BipedIK bipedIK;

		// Look At
		[LargeHeader("Look At")]
		public Transform lookAtTargetBiped;
		public Transform lookAtTargetAnimator;
		[Range(0f, 1f)] public float lookAtWeight = 1f;
		[Range(0f, 1f)] public float lookAtBodyWeight = 1f;
		[Range(0f, 1f)] public float lookAtHeadWeight = 1f;
		[Range(0f, 1f)] public float lookAtEyesWeight = 1f;
		[Range(0f, 1f)] public float lookAtClampWeight = 0.5f;
		[Range(0f, 1f)] public float lookAtClampWeightHead = 0.5f;
		[Range(0f, 1f)] public float lookAtClampWeightEyes = 0.5f;

		// Foot
		[LargeHeader("Foot")]
		public Transform footTargetBiped;
		public Transform footTargetAnimator;
		[Range(0f, 1f)] public float footPositionWeight = 0f;
		[Range(0f, 1f)] public float footRotationWeight = 0f;

		// Hand
		[LargeHeader("Hand")]
		public Transform handTargetBiped;
		public Transform handTargetAnimator;
		[Range(0f, 1f)] public float handPositionWeight = 0f;
		[Range(0f, 1f)] public float handRotationWeight = 0f;

		void OnAnimatorIK(int layer) {
			animator.transform.rotation = bipedIK.transform.rotation;
			Vector3 offset = animator.transform.position - bipedIK.transform.position;

			// Look At
			lookAtTargetAnimator.position = lookAtTargetBiped.position + offset;

			bipedIK.SetLookAtPosition(lookAtTargetBiped.position);
			bipedIK.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, lookAtHeadWeight, lookAtEyesWeight, lookAtClampWeight, lookAtClampWeightHead, lookAtClampWeightEyes);
			
			animator.SetLookAtPosition(lookAtTargetAnimator.position);
			animator.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, lookAtHeadWeight, lookAtEyesWeight, lookAtClampWeight);

			// Foot
			footTargetAnimator.position = footTargetBiped.position + offset;
			footTargetAnimator.rotation = footTargetBiped.rotation;

			bipedIK.SetIKPosition(AvatarIKGoal.LeftFoot, footTargetBiped.position);
			bipedIK.SetIKRotation(AvatarIKGoal.LeftFoot, footTargetBiped.rotation);
			bipedIK.SetIKPositionWeight(AvatarIKGoal.LeftFoot, footPositionWeight);
			bipedIK.SetIKRotationWeight(AvatarIKGoal.LeftFoot, footRotationWeight);

			animator.SetIKPosition(AvatarIKGoal.LeftFoot, footTargetAnimator.position);
			animator.SetIKRotation(AvatarIKGoal.LeftFoot, footTargetAnimator.rotation);
			animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, footPositionWeight);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, footRotationWeight);

			// Hand
			handTargetAnimator.position = handTargetBiped.position + offset;
			handTargetAnimator.rotation = handTargetBiped.rotation;
			
			bipedIK.SetIKPosition(AvatarIKGoal.LeftHand, handTargetBiped.position);
			bipedIK.SetIKRotation(AvatarIKGoal.LeftHand, handTargetBiped.rotation);
			bipedIK.SetIKPositionWeight(AvatarIKGoal.LeftHand, handPositionWeight);
			bipedIK.SetIKRotationWeight(AvatarIKGoal.LeftHand, handRotationWeight);
			
			animator.SetIKPosition(AvatarIKGoal.LeftHand, handTargetAnimator.position);
			animator.SetIKRotation(AvatarIKGoal.LeftHand, handTargetAnimator.rotation);
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handPositionWeight);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handRotationWeight);
		}
	}
}
