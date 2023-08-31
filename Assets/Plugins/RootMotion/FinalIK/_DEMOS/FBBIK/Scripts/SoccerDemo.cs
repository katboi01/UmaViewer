using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	[RequireComponent(typeof(Animator))]
	public class SoccerDemo : MonoBehaviour {

		private Animator animator;
		private Vector3 defaultPosition;
		private Quaternion defaultRotation;

		void Start () {
			animator = GetComponent<Animator>();

			// Remember the default position and rotation of the character
			defaultPosition = transform.position;
			defaultRotation = transform.rotation;

			StartCoroutine(ResetDelayed());
		}

		// Reset the character after some time and restart the animation
		private IEnumerator ResetDelayed() {
			while (true) {
				yield return new WaitForSeconds(3f);

				transform.position = defaultPosition;
				transform.rotation = defaultRotation;

				animator.CrossFade("SoccerKick", 0f, 0, 0f);

				yield return null;
			}

		}
	}
}
