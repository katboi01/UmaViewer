using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Contols animation for a third person person controller.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class CharacterAnimationThirdPerson: CharacterAnimationBase {
		
		public CharacterThirdPerson characterController;
		[SerializeField] float turnSensitivity = 0.2f; // Animator turning sensitivity
		[SerializeField]  float turnSpeed = 5f; // Animator turning interpolation speed
		[SerializeField]  float runCycleLegOffset = 0.2f; // The offset of leg positions in the running cycle
		[Range(0.1f,3f)] [SerializeField] float animSpeedMultiplier = 1; // How much the animation of the character will be multiplied by
		
		protected Animator animator;
		private Vector3 lastForward;
		private const string groundedDirectional = "Grounded Directional", groundedStrafe = "Grounded Strafe";
		private float deltaAngle;

		protected override void Start() {
			base.Start();

			animator = GetComponent<Animator>();

			lastForward = transform.forward;
		}
		
		public override Vector3 GetPivotPoint() {
			return animator.pivotPosition;
		}
		
		// Is the Animator playing the grounded animations?
		public override bool animationGrounded {
			get {
				return animator.GetCurrentAnimatorStateInfo(0).IsName(groundedDirectional) || animator.GetCurrentAnimatorStateInfo(0).IsName(groundedStrafe);
			}
		}
		
		// Update the Animator with the current state of the character controller
		protected virtual void Update() {
			if (Time.deltaTime == 0f) return;

			animatePhysics = animator.updateMode == AnimatorUpdateMode.AnimatePhysics;

			// Jumping
			if (characterController.animState.jump) {
				float runCycle = Mathf.Repeat (animator.GetCurrentAnimatorStateInfo (0).normalizedTime + runCycleLegOffset, 1);
				float jumpLeg = (runCycle < 0 ? 1 : -1) * characterController.animState.moveDirection.z;
				
				animator.SetFloat ("JumpLeg", jumpLeg);
			}
			
			// Calculate the angular delta in character rotation
			float angle = -GetAngleFromForward(lastForward) - deltaAngle;
			deltaAngle = 0f;
			lastForward = transform.forward;
			angle *= turnSensitivity * 0.01f;
			angle = Mathf.Clamp(angle / Time.deltaTime, -1f, 1f);
			
			// Update Animator params
			animator.SetFloat("Turn", Mathf.Lerp(animator.GetFloat("Turn"), angle, Time.deltaTime * turnSpeed));
			animator.SetFloat("Forward", characterController.animState.moveDirection.z);
			animator.SetFloat("Right", characterController.animState.moveDirection.x);
			animator.SetBool("Crouch", characterController.animState.crouch);
			animator.SetBool("OnGround", characterController.animState.onGround);
			animator.SetBool("IsStrafing", characterController.animState.isStrafing);
			
			if (!characterController.animState.onGround) {
				animator.SetFloat ("Jump", characterController.animState.yVelocity);
			}

			if (characterController.doubleJumpEnabled) animator.SetBool("DoubleJump", characterController.animState.doubleJump);
			characterController.animState.doubleJump = false;
			
			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector
			if (characterController.animState.onGround && characterController.animState.moveDirection.z > 0f) {
				animator.speed = animSpeedMultiplier;
			} else {
				// but we don't want to use that while airborne
				animator.speed = 1;
			}
		}

		// Call OnAnimatorMove manually on the character controller because it doesn't have the Animator component
		void OnAnimatorMove() {
			// For not using root rotation in Turn value calculation 
			Vector3 f = animator.deltaRotation * Vector3.forward;
			deltaAngle += Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;

            characterController.Move(animator.deltaPosition, animator.deltaRotation);
		}
	}
}
