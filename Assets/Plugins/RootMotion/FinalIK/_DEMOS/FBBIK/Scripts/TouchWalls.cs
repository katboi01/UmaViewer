using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Demo script for using the Interaction System for touching scene geometry.
	/// </summary>
	public class TouchWalls : MonoBehaviour {

		/// <summary>
		/// Touching functionality for an effector
		/// </summary>
		[System.Serializable]
		public class EffectorLink {
			public bool enabled = true; // Can this effector start touching?
			public FullBodyBipedEffector effectorType; // The type of the effector
			public InteractionObject interactionObject; // The touch interaction object
			public Transform spherecastFrom; // The bone to start the sperecast from
			public float spherecastRadius = 0.1f; // The radius of sperecasting
			public float minDistance = 0.3f; // The minimum spherecast radius to touch
			public LayerMask touchLayers; // The layers to touch
			public float lerpSpeed = 10f; // The speed of lerping the interaction object
			public float minSwitchTime = 0.2f; // The minimum time to switch between touching
			public float releaseDistance= 0.4f; // The distance from the original touch point where the effector will lose contact
			public bool sliding; // Can the effector slide smoothly over the surfaces?

			private Vector3 raycastDirectionLocal;
			private float raycastDistance;
			private bool inTouch;
			private RaycastHit hit = new RaycastHit();
			private Vector3 targetPosition;
			private Quaternion targetRotation;
			private bool initiated;
			private float nextSwitchTime;
			private float speedF;

			// Initiate this instance
			public void Initiate(InteractionSystem interactionSystem) {
				// Find the direction to the interaction objet relative to the sperecastFrom Transform
				raycastDirectionLocal = spherecastFrom.InverseTransformDirection(interactionObject.transform.position - spherecastFrom.position);

				// Find the max distance the sperecast
				raycastDistance = Vector3.Distance(spherecastFrom.position, interactionObject.transform.position);

				// Add to the interaction system delegates
				interactionSystem.OnInteractionStart += OnInteractionStart;
				interactionSystem.OnInteractionResume += OnInteractionResume;
				interactionSystem.OnInteractionStop += OnInteractionStop;

				hit.normal = Vector3.forward;
				targetPosition = interactionObject.transform.position;
				targetRotation = interactionObject.transform.rotation;

				initiated = true;
			}

			// Spherecasting to find the walls
			private bool FindWalls(Vector3 direction) {
				if (!enabled) return false;
				bool found = Physics.SphereCast(spherecastFrom.position, spherecastRadius, direction, out hit, raycastDistance, touchLayers);

				if (hit.distance < minDistance) found = false;
				return found;
			}

			// Update this instance
			public void Update(InteractionSystem interactionSystem) {
				if (!initiated) return;

				// The default position
				Vector3 direction = spherecastFrom.TransformDirection(raycastDirectionLocal);
				hit.point = spherecastFrom.position + direction;

				// Spherecasting
				bool wallFound = FindWalls(direction);

				// Starting and ending the interactions
				if (!inTouch) {
					if (wallFound && Time.time > nextSwitchTime) {
						interactionObject.transform.parent = null;
						interactionSystem.StartInteraction(effectorType, interactionObject, true);
						nextSwitchTime = Time.time + minSwitchTime / interactionSystem.speed;

						targetPosition = hit.point;
						targetRotation = Quaternion.LookRotation(-hit.normal);

						interactionObject.transform.position = targetPosition;
						interactionObject.transform.rotation = targetRotation;
					}
				} else {
					if (!wallFound) {
						// Resume if no wall found
						StopTouch(interactionSystem);
					} else {
						if (!interactionSystem.IsPaused(effectorType) || sliding) {
							targetPosition = hit.point;
							targetRotation = Quaternion.LookRotation(-hit.normal);
						}
					}

					if (Vector3.Distance(interactionObject.transform.position, hit.point) > releaseDistance) {
						if (wallFound) {
							targetPosition = hit.point;
							targetRotation = Quaternion.LookRotation(-hit.normal);
						} else {
							StopTouch(interactionSystem);
						}
					}
				}

				float speedFTarget = !inTouch || (interactionSystem.IsPaused(effectorType) && interactionObject.transform.position == targetPosition)? 0f: 1f;
				speedF = Mathf.Lerp(speedF, speedFTarget, Time.deltaTime * 3f * interactionSystem.speed);

				// Interpolating the interaction object
				float s = Time.deltaTime * lerpSpeed * speedF * interactionSystem.speed;
				interactionObject.transform.position = Vector3.Lerp(interactionObject.transform.position, targetPosition, s);
				interactionObject.transform.rotation = Quaternion.Slerp(interactionObject.transform.rotation, targetRotation, s);
			}

			// Stop touching the walls
			private void StopTouch(InteractionSystem interactionSystem) {
				interactionObject.transform.parent = interactionSystem.transform;
				nextSwitchTime = Time.time + minSwitchTime / interactionSystem.speed;

				if (interactionSystem.IsPaused(effectorType)) interactionSystem.ResumeInteraction(effectorType);
				else {
					speedF = 0f;
					targetPosition = hit.point;
					targetRotation = Quaternion.LookRotation(-hit.normal);
				}
			}
			
			// Called by the interaction system
			private void OnInteractionStart(FullBodyBipedEffector effectorType, InteractionObject interactionObject) {
				if (effectorType != this.effectorType || interactionObject != this.interactionObject) return;

				inTouch = true;
			}

			// Called by the interaction system
			private void OnInteractionResume(FullBodyBipedEffector effectorType, InteractionObject interactionObject) {
				if (effectorType != this.effectorType || interactionObject != this.interactionObject) return;

				inTouch = false;
			}

			// Called by the interaction system
			private void OnInteractionStop(FullBodyBipedEffector effectorType, InteractionObject interactionObject) {
				if (effectorType != this.effectorType || interactionObject != this.interactionObject) return;
				
				inTouch = false;
			}

			// Cleaning up the delegates
			public void Destroy(InteractionSystem interactionSystem) {
				if (!initiated) return;

				interactionSystem.OnInteractionStart -= OnInteractionStart;
				interactionSystem.OnInteractionResume -= OnInteractionResume;
				interactionSystem.OnInteractionStop -= OnInteractionStop;
			}
		}

		public InteractionSystem interactionSystem; // Reference to the interaciton sytem of the character
		public EffectorLink[] effectorLinks; // The wall touching effectors

		// Initiate the effector links
		void Start() {
			foreach (EffectorLink e in effectorLinks) e.Initiate(interactionSystem);
		}

		// Update touching
		void FixedUpdate() {
			for (int i = 0; i < effectorLinks.Length; i++) effectorLinks[i].Update(interactionSystem);
		}

		// Clean up the delegates
		void OnDestroy() {
			if (interactionSystem != null) {
				for (int i = 0; i < effectorLinks.Length; i++) effectorLinks[i].Destroy(interactionSystem);
			}
		}

	}
}
