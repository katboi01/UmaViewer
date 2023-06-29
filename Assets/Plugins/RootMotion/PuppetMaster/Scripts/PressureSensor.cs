using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	public class PressureSensor : MonoBehaviour {

		public bool visualize;
		public LayerMask layers;

		public Vector3 center { get; private set; }
		public bool inContact { get; private set; }
		public Vector3 bottom { get; private set; }

		public Rigidbody r { get; private set; }
		private bool fixedFrame;
		private Vector3 P;
		private int count;

		void Awake() {
			r = GetComponent<Rigidbody>();
			center = transform.position;
		}

		void OnCollisionEnter(Collision c) {
			ProcessCollision(c);
		}

		void OnCollisionStay(Collision c) {
			ProcessCollision(c);
		}

		void OnCollisionExit(Collision c) {
			inContact = false;
		}
		
		void FixedUpdate() {
			fixedFrame = true;

			if (!r.IsSleeping()) {
				P = Vector3.zero;
				count = 0;
			}
		}

		void LateUpdate() {
			if (fixedFrame) {
				if (count > 0) {
					center = P / count;
					//center = Vector3.Lerp(transform.position + transform.rotation * bottom, center, 0.0f);
				}

				fixedFrame = false;
			}
		}

		private void ProcessCollision(Collision c) {
			if (!LayerMaskExtensions.Contains(layers, c.gameObject.layer)) return;

			Vector3 collisionCenter = Vector3.zero;
			
			for (int i = 0; i < c.contacts.Length; i++) {
				collisionCenter += c.contacts[i].point;
			}
			
			collisionCenter /= c.contacts.Length;

			P += collisionCenter;// * pressure // TODO process each collision based on it's pressure

			count++;
			inContact = true;
		}

		void OnDrawGizmos() {
			if (!visualize) return;
			Gizmos.DrawSphere(center, 0.1f);
		}
	}
}