using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	// Using the Unity's built in Animator IK to adjust the target pose of the Puppet.
	[RequireComponent(typeof(Animator))]
	public class AnimatorIKDemo : MonoBehaviour {

		public Transform leftHandIKTarget;

		private Animator animator;

		void Start() {
			animator = GetComponent<Animator>();
		}

		void OnAnimatorIK(int layer) {
			animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
		}
	}
}
