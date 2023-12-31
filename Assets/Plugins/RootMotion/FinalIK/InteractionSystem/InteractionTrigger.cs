using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK {

	/// <summary>
	/// When a character with an InteractionSystem component enters the trigger collider of this game object, this component will register itself to the InteractionSystem. 
	/// The InteractionSystem can then use it to find the most appropriate InteractionObject and effectors to interact with.
	/// Use InteractionSystem.GetClosestTriggerIndex() and InteractionSystem.TriggerInteration() to trigger the interactions that the character is in contact with.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Trigger")]
	public class InteractionTrigger: MonoBehaviour {

        // Open the User Manual URL
        [ContextMenu("User Manual")]
        void OpenUserManual()
        {
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
        }

        // Open the Script Reference URL
        [ContextMenu("Scrpt Reference")]
        void OpenScriptReference()
        {
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_trigger.html");
        }

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO")]
		void OpenTutorial4() {
			Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		// Link to the Final IK Google Group
		[ContextMenu("Support Group")]
		void SupportGroup() {
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}
		
		// Link to the Final IK Asset Store thread in the Unity Community
		[ContextMenu("Asset Store Thread")]
		void ASThread() {
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		/// <summary>
		/// Defines the valid range of the character's position and rotation relative to this trigger.
		/// </summary>
		[System.Serializable]
		public class CharacterPosition {
			
			/// <summary>
			/// If false, will not care where the character stands, as long as it is in contact with the trigger collider.
			/// </summary>
			[Tooltip("If false, will not care where the character stands, as long as it is in contact with the trigger collider.")]
			public bool use;
			/// <summary>
			/// The offset of the character's position relative to the trigger in XZ plane. Y position of the character is unlimited as long as it is contact with the collider.
			/// </summary>
			[Tooltip("The offset of the character's position relative to the trigger in XZ plane. Y position of the character is unlimited as long as it is contact with the collider.")]
			public Vector2 offset;
			/// <summary>
			/// Angle offset from the default forward direction..
			/// </summary>
			[Tooltip("Angle offset from the default forward direction.")]
			[Range(-180f, 180f)] public float angleOffset;
			/// <summary>
			/// Max angular offset of the character's forward from the direction of this trigger.
			/// </summary>
			[Tooltip("Max angular offset of the character's forward from the direction of this trigger.")]
			[Range(0f, 180f)] public float maxAngle = 45f;
			/// <summary>
			/// Max offset of the character's position from this range's center.
			/// </summary>
			[Tooltip("Max offset of the character's position from this range's center.")]
			public float radius = 0.5f;
			/// <summary>
			/// If true, will rotate the trigger around it's Y axis relative to the position of the character, so the object can be interacted with from all sides.
			/// </summary>
			[Tooltip("If true, will rotate the trigger around it's Y axis relative to the position of the character, so the object can be interacted with from all sides.")]
			public bool orbit;
			/// <summary>
			/// Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.
			/// For example a gun will be able to be picked up from the same direction relative to the barrel no matter which side the gun is resting on. 
			/// </summary>
			[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object. For example a gun will be able to be picked up from the same direction relative to the barrel no matter which side the gun is resting on.")]
			public bool fixYAxis;
			
			// Returns the 2D offset as 3D vector.
			public Vector3 offset3D { get { return new Vector3(offset.x, 0f, offset.y); }}
			
			// Returns the default direction of this character position in world space.
			public Vector3 direction3D { 
				get { 
					return Quaternion.AngleAxis(angleOffset, Vector3.up) * Vector3.forward;
				}
			}
			
			// Is the character in range with this character position?
			public bool IsInRange(Transform character, Transform trigger, out float error) {
				// Do not use this character position, trigger is still valid
				error = 0f;
				if (!use) return true;
				
				// Invalid character position conditions
				error = 180f;
				if (radius <= 0f) return false;
				if (maxAngle <= 0f) return false;
				
				Vector3 forward = trigger.forward;
				if (fixYAxis) forward.y = 0f;
				if (forward == Vector3.zero) return false; // Singularity
				
				Vector3 up = (fixYAxis? Vector3.up: trigger.up);
				
				Quaternion triggerRotation = Quaternion.LookRotation(forward, up);
				
				Vector3 position = trigger.position + triggerRotation * offset3D;
				
				Vector3 origin = orbit? trigger.position: position;
				Vector3 toCharacter = character.position - origin;
				Vector3.OrthoNormalize(ref up, ref toCharacter);
				toCharacter *= Vector3.Project(character.position - origin, toCharacter).magnitude;
				
				if (orbit) {
					float mag = offset.magnitude;
					float dist = toCharacter.magnitude;
					if (dist < mag - radius || dist > mag + radius) return false;
				} else {
					if (toCharacter.magnitude > radius) return false;
				}
				
				Vector3 d = triggerRotation * direction3D;
				Vector3.OrthoNormalize(ref up, ref d);
				
				if (orbit) {
					Vector3 toPosition = position - trigger.position;
					if (toPosition == Vector3.zero) toPosition = Vector3.forward;
					Quaternion r = Quaternion.LookRotation(toPosition, up);
					toCharacter = Quaternion.Inverse(r) * toCharacter;
					
					float a = Mathf.Atan2(toCharacter.x, toCharacter.z) * Mathf.Rad2Deg;
					d = Quaternion.AngleAxis(a, up) * d;
				}
				
				float angle = Vector3.Angle(d, character.forward);
				if (angle > maxAngle) return false;
				error = (angle / maxAngle) * 180f;
				
				return true;
			}
		}
		
		/// <summary>
		/// Defines the valid range of the camera's position relative to this trigger.
		/// </summary>
		[System.Serializable]
		public class CameraPosition {
			
			/// <summary>
			/// What the camera should be looking at to trigger the interaction?
			/// </summary>
			[Tooltip("What the camera should be looking at to trigger the interaction? If null, this camera position will not be used.")]
			public Collider lookAtTarget;
			/// <summary>
			/// The direction from the lookAtTarget towards the camera (in lookAtTarget's space).
			/// </summary>
			[Tooltip("The direction from the lookAtTarget towards the camera (in lookAtTarget's space).")]
			public Vector3 direction = -Vector3.forward;
			/// <summary>
			/// Max distance from the lookAtTarget to the camera.
			/// </summary>
			[Tooltip("Max distance from the lookAtTarget to the camera.")]
			public float maxDistance = 0.5f;
			/// <summary>
			/// Max angle between the direction and the direction towards the camera.
			/// </summary>
			[Tooltip("Max angle between the direction and the direction towards the camera.")]
			[Range(0f, 180f)] public float maxAngle = 45f;
			/// <summary>
			/// Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.
			/// </summary>
			[Tooltip("Fixes the Y axis of the trigger to Vector3.up. This makes the trigger symmetrical relative to the object.")]
			public bool fixYAxis;
			
			// Returns the rotation space of this CameraPosition.
			public Quaternion GetRotation() {
				Vector3 forward = lookAtTarget.transform.forward;
				if (fixYAxis) forward.y = 0f;
				if (forward == Vector3.zero) return Quaternion.identity; // Singularity
				Vector3 up = (fixYAxis? Vector3.up: lookAtTarget.transform.up);
				
				return Quaternion.LookRotation(forward, up);
			}
			
			// Is the camera raycast hit in range of this CameraPosition?
			public bool IsInRange(Transform raycastFrom, RaycastHit hit, Transform trigger, out float error) {
				// Not using the CameraPosition
				error = 0f;
				if (lookAtTarget == null) return true;
				
				// Not in range conditions
				error = 180f;
				if (raycastFrom == null) return false;
				if (hit.collider != lookAtTarget) return false;
				if (hit.distance > maxDistance) return false;
				if (direction == Vector3.zero) return false;
				if (maxDistance <= 0f) return false;
				if (maxAngle <= 0f) return false;
				
				Vector3 d = GetRotation() * direction;
				
				float a = Vector3.Angle(raycastFrom.position - hit.point, d);
				if (a > maxAngle) return false;
				error = (a / maxAngle) * 180f;
				
				return true;
			}
		}
		
		/// <summary>
		/// Defines the valid range of the character's and/or it's camera's position for one or multiple interactions.
		/// </summary>
		[System.Serializable]
		public class Range {
		
			[HideInInspector][SerializeField] public string name; // Name is composed automatically by InteractionTriggerInspector.cs. Editor only.
			[HideInInspector][SerializeField] public bool show = true; // Show this range in the Scene view? Editor only.
			
			/// <summary>
			/// Defines the interaction object and effectors that will be triggered when calling InteractionSystem.TriggerInteraction().
			/// </summary>
			[System.Serializable]
			public class Interaction {
				/// <summary>
				/// The InteractionObject to interact with.
				/// </summary>
				[Tooltip("The InteractionObject to interact with.")]
				public InteractionObject interactionObject;
				/// <summary>
				/// The effectors to interact with.
				/// </summary>
				[Tooltip("The effectors to interact with.")]
				public FullBodyBipedEffector[] effectors;
			}
			
			/// <summary>
			/// The range for the character's position and rotation.
			/// </summary>
			[Tooltip("The range for the character's position and rotation.")]
			public CharacterPosition characterPosition;
			/// <summary>
			/// The range for the character camera's position and rotation.
			/// </summary>
			[Tooltip("The range for the character camera's position and rotation.")]
			public CameraPosition cameraPosition;
			
			/// <summary>
			/// Definitions of the interactions associated with this range.
			/// </summary>
			[Tooltip("Definitions of the interactions associated with this range.")]
			public Interaction[] interactions;
			
			public bool IsInRange(Transform character, Transform raycastFrom, RaycastHit raycastHit, Transform trigger, out float maxError) {
				maxError = 0f;
				
				float characterError = 0f;
				float cameraError = 0f;
				
				if (!characterPosition.IsInRange(character, trigger, out characterError)) return false;
				if (!cameraPosition.IsInRange(raycastFrom, raycastHit, trigger, out cameraError)) return false;
				
				maxError = Mathf.Max(characterError, cameraError);
				
				return true;
			}
		}

		// Just to see the enabled box in the Editor
		void Start() {}
		
		/// <summary>
		/// The valid ranges of the character's and/or it's camera's position for triggering interaction when the character is in contact with the collider of this trigger.
		/// </summary>
		[Tooltip("The valid ranges of the character's and/or it's camera's position for triggering interaction when the character is in contact with the collider of this trigger.")]
		public Range[] ranges = new Range[0];
		
		// Returns the index of the ranges that is best fit for the current position/rotation of the character and it's camera.
		public int GetBestRangeIndex(Transform character, Transform raycastFrom, RaycastHit raycastHit) {
			if (GetComponent<Collider>() == null) {
				Warning.Log("Using the InteractionTrigger requires a Collider component.", transform);
				return -1;
			}
			
			int bestRangeIndex = -1;
			float smallestError = 180f;
			float error = 0f;
			
			for (int i = 0; i < ranges.Length; i++) {
				
				if (ranges[i].IsInRange(character, raycastFrom, raycastHit, transform, out error)) {
					if (error <= smallestError) {
						smallestError = error;
						bestRangeIndex = i;
					}
				}
			}
			
			return bestRangeIndex;
		}
	}
}
