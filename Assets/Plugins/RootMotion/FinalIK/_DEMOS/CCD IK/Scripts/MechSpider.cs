using UnityEngine;
using System.Collections;

namespace RootMotion.Demos {

	/// <summary>
	/// Mech spider Demo.
	/// </summary>
	public class MechSpider : MonoBehaviour {

		public LayerMask raycastLayers; // The ground layers
		public float scale = 1f; // For resizing the values when the mech is resized
		public Transform body; // The body transform, the root of the legs
		public MechSpiderLeg[] legs; // All the legs of this spider
		public float legRotationWeight = 1f; // The weight of rotating the body to each leg
		public float rootPositionSpeed = 5f; // The speed of positioning the root
		public float rootRotationSpeed = 30f; // The slerp speed of rotating the root to leg heights
		public float breatheSpeed = 2f; // Speed of the breathing cycle
		public float breatheMagnitude = 0.2f; // Magnitude of breathing
		public float height = 3.5f; // Height from ground
		public float minHeight = 2f; // The minimum height from ground
		public float raycastHeight = 10f; // The height of ray origin
		public float raycastDistance = 5f; // The distance of rays (total ray length = raycastHeight + raycastDistance)

		private Vector3 lastPosition;
		private Vector3 defaultBodyLocalPosition;
		private float sine;
		private RaycastHit rootHit;

		void Update() {
			// Find the normal of the plane defined by leg positions
			Vector3 legsPlaneNormal = GetLegsPlaneNormal();

			// Rotating the root
			Quaternion fromTo = Quaternion.FromToRotation(transform.up, legsPlaneNormal);
			transform.rotation = Quaternion.Slerp(transform.rotation, fromTo * transform.rotation, Time.deltaTime * rootRotationSpeed);

			// Positioning the root
			Vector3 legCentroid = GetLegCentroid();
			Vector3 heightOffset = Vector3.Project((legCentroid + transform.up * height * scale) - transform.position, transform.up);
			transform.position += heightOffset * Time.deltaTime * (rootPositionSpeed * scale);

			if (Physics.Raycast(transform.position + transform.up * raycastHeight * scale, -transform.up, out rootHit, (raycastHeight * scale) + (raycastDistance * scale), raycastLayers)) {
				rootHit.distance -= (raycastHeight * scale) + (minHeight * scale);

				if (rootHit.distance < 0f) {
					Vector3 targetPosition = transform.position - transform.up * rootHit.distance;
					transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * rootPositionSpeed * scale);
				}
			}

			// Update Breathing
			sine += Time.deltaTime * breatheSpeed;
			if (sine >= Mathf.PI * 2f) sine -= Mathf.PI * 2f;
			float br = Mathf.Sin(sine) * breatheMagnitude * scale;

			// Apply breathing
			Vector3 breatheOffset = transform.up * br;
			body.transform.position = transform.position + breatheOffset;
		}

		// Calculate the normal of the plane defined by leg positions, so we know how to rotate the body
		private Vector3 GetLegCentroid() {
			Vector3 position = Vector3.zero;

			float footWeight = 1f / (float)legs.Length;
			
			// Go through all the legs, rotate the normal by it's offset
			for (int i = 0; i < legs.Length; i++) {
				position += legs[i].position * footWeight;
			}
			
			return position;
		}

		// Calculate the normal of the plane defined by leg positions, so we know how to rotate the body
		private Vector3 GetLegsPlaneNormal() {
			Vector3 normal = transform.up;

			if (legRotationWeight <= 0f) return normal;

			float legWeight = 1f / Mathf.Lerp(legs.Length, 1f, legRotationWeight);

			// Go through all the legs, rotate the normal by it's offset
			for (int i = 0; i < legs.Length; i++) {
				// Direction from the root to the leg
				Vector3 legDirection = legs[i].position - (transform.position - transform.up * height * scale); 

				// Find the tangent to transform.up
				Vector3 legNormal = transform.up;
				Vector3 legTangent = legDirection;
				Vector3.OrthoNormalize(ref legNormal, ref legTangent);

				// Find the rotation offset from the tangent to the direction
				Quaternion fromTo = Quaternion.FromToRotation(legTangent, legDirection);
				fromTo = Quaternion.Lerp(Quaternion.identity, fromTo, legWeight);

				// Rotate the normal
				normal = fromTo * normal;
			}
			
			return normal;
		}
	}
}
