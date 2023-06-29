using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Transfer motion from this Transform to the "to" Transform.
	/// </summary>
	public class TransferMotion : MonoBehaviour {

		[Tooltip("The Transform to transfer motion to.")]
		public Transform to;

		[Tooltip("The amount of motion to transfer.")]
		[Range(0f, 1f)] public float transferMotion = 0.9f;
		
		private Vector3 lastPosition;

		void OnEnable() {
			lastPosition = transform.position;
		}
		
		void Update() {
			Vector3 delta = transform.position - lastPosition;

			// Add the position delta of this Transform to the other Transform
			to.position += delta * transferMotion;
			
			lastPosition = transform.position;
		}
	}
}