using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	/// <summary>
	/// Motion Absorb demo character controller.
	/// </summary>
	public class MotionAbsorbCharacter : MonoBehaviour {

		public Animator animator;
		public MotionAbsorb motionAbsorb;
		public Transform cube; // The cube we are hitting
		public float cubeRandomPosition = 0.1f; // Randomizing cube position after each hit
		public AnimationCurve motionAbsorbWeight;

		private Vector3 cubeDefaultPosition;
		private AnimatorStateInfo info;
		private Rigidbody cubeRigidbody;
		
		void Start() {
			// Storing the default position of the cube
			cubeDefaultPosition = cube.position;
			cubeRigidbody = cube.GetComponent<Rigidbody>();
		}

		void Update () {
			// Set motion absorb weight
			//motionAbsorb.weight = animator.GetFloat("MotionAbsorbWeight"); // NB! Using Mecanim curves is PRO only

			// Using an animation curve so it works with Unity Free as well
			info = animator.GetCurrentAnimatorStateInfo(0);
			motionAbsorb.weight = motionAbsorbWeight.Evaluate(info.normalizedTime - (int)info.normalizedTime);
		}

		// Mecanim event
		void SwingStart() {
			// Reset the cube
			cubeRigidbody.MovePosition(cubeDefaultPosition + UnityEngine.Random.insideUnitSphere * cubeRandomPosition);
			cubeRigidbody.MoveRotation(Quaternion.identity);
			cubeRigidbody.velocity = Vector3.zero;
			cubeRigidbody.angularVelocity = Vector3.zero;
		}
	}
}
