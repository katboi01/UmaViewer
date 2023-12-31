using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Handles FBBIK interactions for a character.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction System")]
	public class InteractionSystem : MonoBehaviour {

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
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_system.html");
        }

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
		void OpenTutorial1() {
			Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
		}

		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
		void OpenTutorial2() {
			Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
		}

		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
		void OpenTutorial3() {
			Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		// Open a video tutorial video
		[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
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

		#region Main Interface

		/// <summary>
		/// If not empty, only the targets with the specified tag will be used by this Interaction System.
		/// </summary>
		[Tooltip("If not empty, only the targets with the specified tag will be used by this Interaction System.")]
		public string targetTag = "";
		/// <summary>
		/// The fade in time of the interaction.
		/// </summary>
		[Tooltip("The fade in time of the interaction.")]
		public float fadeInTime = 0.3f;
		/// <summary>
		/// The master speed for all interactions.
		/// </summary>
		[Tooltip("The master speed for all interactions.")]
		public float speed = 1f;
		/// <summary>
		/// If > 0, lerps all the FBBIK channels used by the Interaction System back to their default or initial values when not in interaction.
		/// </summary>
		[Tooltip("If > 0, lerps all the FBBIK channels used by the Interaction System back to their default or initial values when not in interaction.")]
		public float resetToDefaultsSpeed = 1f;

		[Header("Triggering")]
		/// <summary>
		/// The collider that registers OnTriggerEnter and OnTriggerExit events with InteractionTriggers.
		/// </summary>
		[Tooltip("The collider that registers OnTriggerEnter and OnTriggerExit events with InteractionTriggers.")]
		[FormerlySerializedAs("collider")]
		public Collider characterCollider;
		/// <summary>
		/// Will be used by Interaction Triggers that need the camera's position. Assign the first person view character camera.
		/// </summary>
		[Tooltip("Will be used by Interaction Triggers that need the camera's position. Assign the first person view character camera.")]
		[FormerlySerializedAs("camera")]
		public Transform FPSCamera;
		/// <summary>
		/// The layers that will be raycasted from the camera (along camera.forward). All InteractionTrigger look at target colliders should be included.
		/// </summary>
		[Tooltip("The layers that will be raycasted from the camera (along camera.forward). All InteractionTrigger look at target colliders should be included.")]
		public LayerMask camRaycastLayers;
		/// <summary>
		/// Max distance of raycasting from the camera.
		/// </summary>
		[Tooltip("Max distance of raycasting from the camera.")]
		public float camRaycastDistance = 1f;
		
		/// <summary>
		/// Returns true if any of the effectors is in interaction and not paused.
		/// </summary>
		public bool inInteraction {
			get {
				if (!IsValid(true)) return false;

				for (int i = 0; i < interactionEffectors.Length; i++) {
					if (interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused) return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Determines whether this effector is interaction and not paused
		/// </summary>
		public bool IsInInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether this effector is  paused
		/// </summary>
		public bool IsPaused(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;
			
			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if any of the effectors is paused
		/// </summary>
		public bool IsPaused() {
			if (!IsValid(true)) return false;
			
			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused) return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if either all effectors in interaction are paused or none is.
		/// </summary>
		public bool IsInSync() {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].isPaused) {
					for (int n = 0; n < interactionEffectors.Length; n++) {
						if (n != i && interactionEffectors[n].inInteraction && !interactionEffectors[n].isPaused) return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Starts the interaction between an effector and an interaction object.
		/// </summary>
		public bool StartInteraction(FullBodyBipedEffector effectorType, InteractionObject interactionObject, bool interrupt) {
			if (!IsValid(true)) return false;

			if (interactionObject == null) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Start(interactionObject, targetTag, fadeInTime, interrupt);
				}
			}

			return false;
		}

		/// <summary>
		/// Pauses the interaction of an effector.
		/// </summary>
		public bool PauseInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Pause();
				}
			}

			return false;
		}

		/// <summary>
		/// Resumes the paused interaction of an effector.
		/// </summary>
		public bool ResumeInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Resume();
				}
			}

			return false;
		}

		/// <summary>
		/// Stops the interaction of an effector.
		/// </summary>
		public bool StopInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Stop();
				}
			}

			return false;
		}

		/// <summary>
		/// Pauses all the interaction effectors.
		/// </summary>
		public void PauseAll() {
			if (!IsValid(true)) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Pause();
		}

		/// <summary>
		/// Resumes all the paused interaction effectors.
		/// </summary>
		public void ResumeAll() {
			if (!IsValid(true)) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Resume();
		}

		/// <summary>
		/// Stops all interactions.
		/// </summary>
		public void StopAll() {
			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Stop();
		}

		/// <summary>
		/// Gets the current interaction object of an effector.
		/// </summary>
		public InteractionObject GetInteractionObject(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return null;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].interactionObject;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the progress of any interaction with the specified effector.
		/// </summary>
		public float GetProgress(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return 0f;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].progress;
				}
			}
			return 0f;
		}

		/// <summary>
		/// Gets the minimum progress of any active interaction
		/// </summary>
		public float GetMinActiveProgress() {
			if (!IsValid(true)) return 0f;
			float min = 1f;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].inInteraction) {
					float p = interactionEffectors[i].progress;
					if (p > 0f && p < min) min = p;
				}
			}

			return min;
		}

		/// <summary>
		/// Triggers all interactions of an InteractionTrigger. Returns false if unsuccessful (maybe out of range).
		/// </summary>
		public bool TriggerInteraction(int index, bool interrupt) {
			if (!IsValid(true)) return false;

			if (!TriggerIndexIsValid(index)) return false;

			bool all = true;

			var range = triggersInRange[index].ranges[bestRangeIndexes[index]];

			for (int i = 0; i < range.interactions.Length; i++) {
				for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
					bool s = StartInteraction(range.interactions[i].effectors[e], range.interactions[i].interactionObject, interrupt);
					if (!s) all = false;
				}
			}

			return all;
		}

		/// <summary>
		/// Triggers all interactions of an InteractionTrigger. Returns false if unsuccessful (maybe out of range).
		/// </summary>
		public bool TriggerInteraction(int index, bool interrupt, out InteractionObject interactionObject) {
			interactionObject = null;
			if (!IsValid(true)) return false;
			
			if (!TriggerIndexIsValid(index)) return false;
			
			bool all = true;
			
			var range = triggersInRange[index].ranges[bestRangeIndexes[index]];
			
			for (int i = 0; i < range.interactions.Length; i++) {
				for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
					interactionObject = range.interactions[i].interactionObject;
					bool s = StartInteraction(range.interactions[i].effectors[e], interactionObject, interrupt);
					if (!s) all = false;
				}
			}
			
			return all;
		}

		/// <summary>
		/// Triggers all interactions of an InteractionTrigger. Returns false if unsuccessful (maybe out of range).
		/// </summary>
		public bool TriggerInteraction(int index, bool interrupt, out InteractionTarget interactionTarget) {
			interactionTarget = null;
			if (!IsValid(true)) return false;
			
			if (!TriggerIndexIsValid(index)) return false;
			
			bool all = true;
			
			var range = triggersInRange[index].ranges[bestRangeIndexes[index]];
			
			for (int i = 0; i < range.interactions.Length; i++) {
				for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
					var interactionObject = range.interactions[i].interactionObject;
					var t = interactionObject.GetTarget(range.interactions[i].effectors[e], tag);
					if (t != null) interactionTarget = t.GetComponent<InteractionTarget>();
					bool s = StartInteraction(range.interactions[i].effectors[e], interactionObject, interrupt);
					if (!s) all = false;
				}
			}
			
			return all;
		}

		/// <summary>
		/// Gets the closest InteractionTrigger Range.
		/// </summary>
		public InteractionTrigger.Range GetClosestInteractionRange() {
			if (!IsValid(true)) return null;

			int index = GetClosestTriggerIndex();
			if (index < 0 || index >= triggersInRange.Count) return null;
			
			return triggersInRange[index].ranges[bestRangeIndexes[index]];
		}

		/// <summary>
		/// Gets the closest InteractionObject in range.
		/// </summary>
		public InteractionObject GetClosestInteractionObjectInRange() {
			var range = GetClosestInteractionRange();
			if (range == null) return null;
			return range.interactions[0].interactionObject;
		}

		/// <summary>
		/// Gets the closest InteractionTarget in range.
		/// </summary>
		public InteractionTarget GetClosestInteractionTargetInRange() {
			var range = GetClosestInteractionRange();
			if (range == null) return null;
			return range.interactions[0].interactionObject.GetTarget(range.interactions[0].effectors[0], this);
		}

		/// <summary>
		/// Gets the closest InteractionObjects in range.
		/// </summary>
		public InteractionObject[] GetClosestInteractionObjectsInRange() {
			var range = GetClosestInteractionRange();
			if (range == null) return new InteractionObject[0];

			InteractionObject[] objects = new InteractionObject[range.interactions.Length];

			for (int i = 0; i < range.interactions.Length; i++) {
				objects[i] = range.interactions[i].interactionObject;
			}

			return objects;
		}

		/// <summary>
		/// Gets the closest InteractionTargets in range.
		/// </summary>
		public InteractionTarget[] GetClosestInteractionTargetsInRange() {
			var range = GetClosestInteractionRange();
			if (range == null) return new InteractionTarget[0];

			List<InteractionTarget> targets = new List<InteractionTarget>();

			foreach (InteractionTrigger.Range.Interaction interaction in range.interactions) {
				foreach (FullBodyBipedEffector effectorType in interaction.effectors) {
					targets.Add (interaction.interactionObject.GetTarget(effectorType, this));
				}
			}

			return (InteractionTarget[])targets.ToArray();
		}

		/// <summary>
		/// Returns true if all effectors of a trigger are either not in interaction or paused
		/// </summary>
		public bool TriggerEffectorsReady(int index) {
			if (!IsValid(true)) return false;
			
			if (!TriggerIndexIsValid(index)) return false;

			for (int r = 0; r < triggersInRange[index].ranges.Length; r++) {
				var range = triggersInRange[index].ranges[r];
				
				for (int i = 0; i < range.interactions.Length; i++) {
					for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
						if (IsInInteraction(range.interactions[i].effectors[e])) return false;
					}
				}

				for (int i = 0; i < range.interactions.Length; i++) {
					for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
						if (IsPaused(range.interactions[i].effectors[e])) {
							for (int n = 0; n < range.interactions[i].effectors.Length; n++) {
								if (n != e && !IsPaused(range.interactions[i].effectors[n])) return false;
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Return the current most appropriate range of an InteractionTrigger listed in triggersInRange.
		/// </summary>
		public InteractionTrigger.Range GetTriggerRange(int index) {
			if (!IsValid(true)) return null;
			
			if (index < 0 || index >= bestRangeIndexes.Count) {
				Warning.Log("Index out of range.", transform);
				return null;
			}

			return triggersInRange[index].ranges[bestRangeIndexes[index]];
		}

		/// <summary>
		/// Returns the InteractionTrigger that is in range and closest to the character.
		/// </summary>
		public int GetClosestTriggerIndex() {
			if (!IsValid(true)) return -1;

			if (triggersInRange.Count == 0) return -1;
			if (triggersInRange.Count == 1) return 0;

			int closest = -1;
			float closestSqrMag = Mathf.Infinity;

			for (int i = 0; i < triggersInRange.Count; i++) {
				if (triggersInRange[i] != null) {
					float sqrMag = Vector3.SqrMagnitude(triggersInRange[i].transform.position - transform.position);

					if (sqrMag < closestSqrMag) {
						closest = i;
						closestSqrMag = sqrMag;
					}
				}
			}

			return closest;
		}

		/// <summary>
		/// Gets the FullBodyBipedIK component.
		/// </summary>
		public FullBodyBipedIK ik {
			get {
				return fullBody;
			}
			set {
				fullBody = value;
			}
		}

		/// <summary>
		/// Gets the in contact.
		/// </summary>
		/// <value>The in contact.</value>
		public List<InteractionTrigger> triggersInRange { get; private set; }
		private List<InteractionTrigger> inContact = new List<InteractionTrigger>();
		private List<int> bestRangeIndexes = new List<int>();

		/// <summary>
		/// Interaction delegate
		/// </summary>
		public delegate void InteractionDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject);
		/// <summary>
		/// Interaction event delegate
		/// </summary>
		public delegate void InteractionEventDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject, InteractionObject.InteractionEvent interactionEvent);

		/// <summary>
		/// Called when an InteractionEvent has been started
		/// </summary>
		public InteractionDelegate OnInteractionStart;
		/// <summary>
		/// Called when an Interaction has been paused
		/// </summary>
		public InteractionDelegate OnInteractionPause;
		/// <summary>
		/// Called when an InteractionObject has been picked up.
		/// </summary>
		public InteractionDelegate OnInteractionPickUp;
		/// <summary>
		/// Called when a paused Interaction has been resumed
		/// </summary>
		public InteractionDelegate OnInteractionResume;
		/// <summary>
		/// Called when an Interaction has been stopped
		/// </summary>
		public InteractionDelegate OnInteractionStop;
		/// <summary>
		/// Called when an interaction event occurs.
		/// </summary>
		public InteractionEventDelegate OnInteractionEvent;
		/// <summary>
		/// Gets the RaycastHit from trigger seeking.
		/// </summary>
		/// <value>The hit.</value>
		public RaycastHit raycastHit;

		#endregion Main Interface

		[Space(10)]

		[Tooltip("Reference to the FBBIK component.")]
		[SerializeField] FullBodyBipedIK fullBody; // Reference to the FBBIK component.

		/// <summary>
		/// Handles looking at the interactions.
		/// </summary>
		[Tooltip("Handles looking at the interactions.")]
		public InteractionLookAt lookAt = new InteractionLookAt();

		// The array of Interaction Effectors
		private InteractionEffector[] interactionEffectors = new InteractionEffector[9] {
			new InteractionEffector(FullBodyBipedEffector.Body),
			new InteractionEffector(FullBodyBipedEffector.LeftFoot),
			new InteractionEffector(FullBodyBipedEffector.LeftHand),
			new InteractionEffector(FullBodyBipedEffector.LeftShoulder),
			new InteractionEffector(FullBodyBipedEffector.LeftThigh),
			new InteractionEffector(FullBodyBipedEffector.RightFoot),
			new InteractionEffector(FullBodyBipedEffector.RightHand),
			new InteractionEffector(FullBodyBipedEffector.RightShoulder),
			new InteractionEffector(FullBodyBipedEffector.RightThigh)
		};

		private bool initiated;
		private Collider lastCollider, c;

		// Initiate
		public void Start() {
			if (fullBody == null) fullBody = GetComponent<FullBodyBipedIK>();
			//Debug.Log(fullBody);
			if (fullBody == null) {
				Warning.Log("InteractionSystem can not find a FullBodyBipedIK component", transform);
				return;
			}

			// Add to the FBBIK OnPostUpdate delegate to get a call when it has finished updating
			fullBody.solver.OnPreUpdate += OnPreFBBIK;
			fullBody.solver.OnPostUpdate += OnPostFBBIK;
			fullBody.solver.OnFixTransforms += OnFixTransforms;
			OnInteractionStart += LookAtInteraction;
			OnInteractionPause += InteractionPause;
			OnInteractionResume += InteractionResume;
			OnInteractionStop += InteractionStop;

			foreach (InteractionEffector e in interactionEffectors) e.Initiate(this);
	
			triggersInRange = new List<InteractionTrigger>();
			
			c = GetComponent<Collider>();
			UpdateTriggerEventBroadcasting();
			
			initiated = true;
		}

		private void InteractionPause(FullBodyBipedEffector effector, InteractionObject interactionObject) {
			lookAt.isPaused = true;
		}

		private void InteractionResume(FullBodyBipedEffector effector, InteractionObject interactionObject) {
			lookAt.isPaused = false;
		}

		private void InteractionStop(FullBodyBipedEffector effector, InteractionObject interactionObject) {
			lookAt.isPaused = false;
		
		}

		// Called by the delegate
		private void LookAtInteraction(FullBodyBipedEffector effector, InteractionObject interactionObject) {
			lookAt.Look(interactionObject.lookAtTarget, Time.time + (interactionObject.length * 0.5f));
		}

		public void OnTriggerEnter(Collider c) {
			if (fullBody == null) return;

			var trigger = c.GetComponent<InteractionTrigger>();

			if (trigger == null) return;
			if (inContact.Contains(trigger)) return;
			
			inContact.Add(trigger);
		}
		
		public void OnTriggerExit(Collider c) {
			if (fullBody == null) return;

			var trigger = c.GetComponent<InteractionTrigger>();
			if (trigger == null) return;
			
			inContact.Remove(trigger);
		}

		// Is the InteractionObject trigger in range of any effectors? If the trigger collider is bigger than any effector ranges, then the object in contact is still unreachable.
		private bool ContactIsInRange(int index, out int bestRangeIndex) {
			bestRangeIndex = -1;

			if (!IsValid(true)) return false;
			
			if (index < 0 || index >= inContact.Count) {
				Warning.Log("Index out of range.", transform);
				return false;
			}
			
			if (inContact[index] == null) {
				Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", transform);
				return false;
			}
			
			bestRangeIndex = inContact[index].GetBestRangeIndex(transform, FPSCamera, raycastHit);
			if (bestRangeIndex == -1) return false;
			
			return true;
		}

		// Using this to assign some default values in Editor
		void OnDrawGizmosSelected() {
			if (Application.isPlaying) return;
			
			if (fullBody == null) fullBody = GetComponent<FullBodyBipedIK>();
			if (characterCollider == null) characterCollider = GetComponent<Collider>();
		}
		
		public void Update() {
			if (fullBody == null) return;
			
			UpdateTriggerEventBroadcasting();
			
			Raycasting();
			
			// Finding the triggers in contact and in range
			triggersInRange.Clear();
			bestRangeIndexes.Clear();

			for (int i = 0; i < inContact.Count; i++) {
				int bestRangeIndex = -1;

				if (inContact[i] != null && inContact[i].gameObject.activeInHierarchy && inContact[i].enabled && ContactIsInRange(i, out bestRangeIndex)) {
					triggersInRange.Add(inContact[i]);
					bestRangeIndexes.Add(bestRangeIndex);
				}
			}

			// Update LookAt
			lookAt.Update();
		}
		
		// Finds triggers that need camera position and rotation
		private void Raycasting() {
			if (camRaycastLayers == -1) return;
			if (FPSCamera == null) return;
			
			Physics.Raycast(FPSCamera.position, FPSCamera.forward, out raycastHit, camRaycastDistance, camRaycastLayers);
		}
		
		// Update collider and TriggerEventBroadcaster
		private void UpdateTriggerEventBroadcasting() {
			if (characterCollider == null) characterCollider = c;
			
			if (characterCollider != null && characterCollider != c) {
				
				if (characterCollider.GetComponent<TriggerEventBroadcaster>() == null) {
					var t = characterCollider.gameObject.AddComponent<TriggerEventBroadcaster>();
					t.target = gameObject;
				}
				
				if (lastCollider != null && lastCollider != c && lastCollider != characterCollider) {
					var t = lastCollider.GetComponent<TriggerEventBroadcaster>();
					if (t != null) Destroy(t);
				}
			}
			
			lastCollider = characterCollider;
			
		}
		
		// Update the interaction
		void UpdateEffectors() {
			if (fullBody == null) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Update(transform, speed);

			// Interpolate to default pull, reach values
			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].ResetToDefaults(resetToDefaultsSpeed * speed);
		}

		// Used for using LookAtIK to rotate the spine
		private void OnPreFBBIK() {
			//if (!enabled) return;
			if (fullBody == null) return;

			lookAt.SolveSpine();

			UpdateEffectors();
		}

		// Used for rotating the hands after FBBIK has finished
		private void OnPostFBBIK() {
			//if (!enabled) return;
			if (fullBody == null) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].OnPostFBBIK();

			// Update LookAtIK head
			lookAt.SolveHead();
		}

		void OnFixTransforms() {
			lookAt.OnFixTransforms();
		}

		// Remove the delegates
		void OnDestroy() {
			if (fullBody == null) return;
			fullBody.solver.OnPreUpdate -= OnPreFBBIK;
			fullBody.solver.OnPostUpdate -= OnPostFBBIK;
			fullBody.solver.OnFixTransforms -= OnFixTransforms;

			OnInteractionStart -= LookAtInteraction;
			OnInteractionPause -= InteractionPause;
			OnInteractionResume -= InteractionResume;
			OnInteractionStop -= InteractionStop;
		}

		// Is this InteractionSystem valid and initiated
		private bool IsValid(bool log) {
			if (fullBody == null) {
				if (log) Warning.Log("FBBIK is null. Will not update the InteractionSystem", transform);
				return false;
			}
			if (!initiated) {
				if (log) Warning.Log("The InteractionSystem has not been initiated yet.", transform);
				return false;
			}
			return true;
		}

		// Is the index of triggersInRange valid?
		private bool TriggerIndexIsValid(int index) {
			if (index < 0 || index >= triggersInRange.Count) {
				Warning.Log("Index out of range.", transform);
				return false;
			}
			
			if (triggersInRange[index] == null) {
				Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", transform);
				return false;
			}
			
			return true;
		}
	}
}
