using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	// Delayed destruction of the GameObject.
	public class Destructor : MonoBehaviour {

		public float delay = 5f;

		void Start() {
			StartCoroutine(Destruct());
		}

		private IEnumerator Destruct() {
			yield return new WaitForSeconds(delay);

			Destroy(gameObject);
		}
	}
}
