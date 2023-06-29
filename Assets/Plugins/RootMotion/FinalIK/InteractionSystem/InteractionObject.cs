using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Object than the InteractionSystem can interact with.
	/// </summary>
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Object")]
	public class InteractionObject : MonoBehaviour {

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
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_object.html");
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
		/// Predefined interaction events for pausing, picking up, triggering animations and sending messages.
		/// </summary>
		[System.Serializable]
		public class InteractionEvent {

			/// <summary>
			/// The time of the event since interaction start.
			/// </summary>
			[Tooltip("The time of the event since interaction start.")]
			public float time;
			/// <summary>
			/// If true, the interaction will be paused on this event. The interaction can be resumed by InteractionSystem.ResumeInteraction() or InteractionSystem.ResumeAll;
			/// </summary>
			[Tooltip("If true, the interaction will be paused on this event. The interaction can be resumed by InteractionSystem.ResumeInteraction() or InteractionSystem.ResumeAll;")]
			public bool pause;
			/// <summary>
			/// If true, the object will be parented to the effector bone on this event. Note that picking up like this can be done by only a single effector at a time.
			/// If you wish to pick up an object with both hands, see the Interaction PickUp2Handed demo scene.
			/// </summary>
			[Tooltip("If true, the object will be parented to the effector bone on this event. Note that picking up like this can be done by only a single effector at a time. If you wish to pick up an object with both hands, see the Interaction PickUp2Handed demo scene.")]
			public bool pickUp;
			/// <summary>
			/// The animations called on this event.
			/// </summary>
			[Tooltip("The animations called on this event.")]
			public AnimatorEvent[] animations;
			/// <summary>
			/// The messages sent on this event using GameObject.SendMessage().
			/// </summary>
			[Tooltip("The messages sent on this event using GameObject.SendMessage().")]
			public Message[] messages;

			[TooltipAttribute("The UnityEvent to invoke on this event.")]
			/// <summary>
			/// The UnityEvent to invoke on this event.
			/// </summary>
			public UnityEvent unityEvent;

			// Activates this event
			public void Activate(Transform t) {
				unityEvent.Invoke();
				foreach (AnimatorEvent e in animations) e.Activate(pickUp);
				foreach (Message m in messages) m.Send(t);
			}
		}

		/// <summary>
		/// Definition of a message sent by an InteractionEvent.
		/// </summary>
		[System.Serializable]
		public class Message {

			/// <summary>
			/// The name of the function called.
			/// </summary>
			[Tooltip("The name of the function called.")]
			public string function;
			/// <summary>
			/// The recipient game object.
			/// </summary>
			[Tooltip("The recipient game object.")]
			public GameObject recipient;

			private const string empty = "";

			// Sends the message to the recipient
			public void Send(Transform t) {
				if (recipient == null) return;
				if (function == string.Empty || function == empty) return;

				recipient.SendMessage(function, t, SendMessageOptions.RequireReceiver);
			}
		}

		/// <summary>
		/// Calls an animation on an interaction event.
		/// </summary>
		[System.Serializable]
		public class AnimatorEvent {

			/// <summary>
			/// The Animator component that will receive the AnimatorEvents.
			/// </summary>
			[Tooltip("The Animator component that will receive the AnimatorEvents.")]
			public Animator animator;
			/// <summary>
			/// The Animation component that will receive the AnimatorEvents (Legacy).
			/// </summary>
			[Tooltip("The Animation component that will receive the AnimatorEvents (Legacy).")]
			public Animation animation;
			/// <summary>
			/// The name of the animation state.
			/// </summary>
			[Tooltip("The name of the animation state.")]
			public string animationState;
			/// <summary>
			/// The crossfading time.
			/// </summary>
			[Tooltip("The crossfading time.")]
			public float crossfadeTime = 0.3f;
			/// <summary>
			/// The layer of the animation state (if using Legacy, the animation state will be forced to this layer).
			/// </summary>
			[Tooltip("The layer of the animation state (if using Legacy, the animation state will be forced to this layer).")]
			public int layer;
			/// <summary>
			/// Should the animation always start from 0 normalized time?
			/// </summary>
			[Tooltip("Should the animation always start from 0 normalized time?")]
			public bool resetNormalizedTime;

			private const string empty = "";

			// Activate the animation
			public void Activate(bool pickUp) {
				if (animator != null) {
					// disable root motion because it may become a child of another Animator. Workaround for a Unity bug with an error message: "Transform.rotation on 'gameobject name' is no longer valid..."
					if (pickUp) animator.applyRootMotion = false;
					Activate(animator);
				}
				if (animation != null) Activate(animation);
			}

			// Activate a Mecanim animation
			private void Activate(Animator animator) {
				if (animationState == empty) return;
				
				if (resetNormalizedTime) animator.CrossFade(animationState, crossfadeTime, layer, 0f);
				else animator.CrossFade(animationState, crossfadeTime, layer);
			}
			
			// Activate a Legacy animation
			private void Activate(Animation animation) {
				if (animationState == empty) return;
				
				if (resetNormalizedTime) animation[animationState].normalizedTime = 0f;
				
				animation[animationState].layer = layer;
				
				animation.CrossFade(animationState, crossfadeTime);
			}
		}

		/// <summary>
		/// A Weight curve for various FBBIK channels.
		/// </summary>
		[System.Serializable]
		public class WeightCurve {
			
			/// <summary>
			/// The type of the weight curve
			/// </summary>
			[System.Serializable]
			public enum Type {
				PositionWeight, // IKEffector.positionWeight
				RotationWeight, // IKEffector.rotationWeight
				PositionOffsetX, // X offset from the interpolation direction relative to the character rotation
				PositionOffsetY, // Y offse from the interpolation direction relative to the character rotation
				PositionOffsetZ, // Z offset from the interpolation direction relative to the character rotation
				Pull, // FBIKChain.pull
				Reach, // FBIKChain.reach
				RotateBoneWeight, // Rotating the bone after FBBIK is finished
				Push, // FBIKChain.push
				PushParent, // FBIKChain.pushParent
				PoserWeight, // Weight of hand/generic Poser
                BendGoalWeight // Weight of the bend goal
			}
			
			/// <summary>
			/// The type of the curve (InteractionObject.WeightCurve.Type).
			/// </summary>
			[Tooltip("The type of the curve (InteractionObject.WeightCurve.Type).")]
			public Type type;
			/// <summary>
			/// The weight curve.
			/// </summary>
			[Tooltip("The weight curve.")]
			public AnimationCurve curve;
			
			// Evaluate the curve at the specified time
			public float GetValue(float timer) {
				return curve.Evaluate(timer);
			}
		}
		
		/// <summary>
		/// Multiplies a weight curve and uses the result for another FBBIK channel. (to reduce the amount of work with AnimationCurves)
		/// </summary>
		[System.Serializable]
		public class Multiplier {
			
			/// <summary>
			/// The curve type to multiply.
			/// </summary>
			[Tooltip("The curve type to multiply.")]
			public WeightCurve.Type curve;
			/// <summary>
			/// The multiplier of the curve's value.
			/// </summary>
			[Tooltip("The multiplier of the curve's value.")]
			public float multiplier = 1f;
			/// <summary>
			/// The resulting value will be applied to this channel.
			/// </summary>
			[Tooltip("The resulting value will be applied to this channel.")]
			public WeightCurve.Type result;
			
			// Get the multiplied value of the curve at the specified time
			public float GetValue(WeightCurve weightCurve, float timer) {
				return weightCurve.GetValue(timer) * multiplier;
			}
		}

		/// <summary>
		/// If the Interaction System has a 'Look At' LookAtIK component assigned, will use it to make the character look at the specified Transform. If unassigned, will look at this GameObject.
		/// </summary>
		[Tooltip("If the Interaction System has a 'Look At' LookAtIK component assigned, will use it to make the character look at the specified Transform. If unassigned, will look at this GameObject.")]
		public Transform otherLookAtTarget;
		/// <summary>
		/// The root Transform of the InteractionTargets. If null, will use this GameObject. GetComponentsInChildren<InteractionTarget>() will be used at initiation to find all InteractionTargets associated with this InteractionObject.
		/// </summary>
		[Tooltip("The root Transform of the InteractionTargets. If null, will use this GameObject. GetComponentsInChildren<InteractionTarget>() will be used at initiation to find all InteractionTargets associated with this InteractionObject.")]
		public Transform otherTargetsRoot;
		/// <summary>
		/// If assigned, all PositionOffset channels will be applied in the rotation space of this Transform. If not, they will be in the rotation space of the character.
		/// </summary>
		[Tooltip("If assigned, all PositionOffset channels will be applied in the rotation space of this Transform. If not, they will be in the rotation space of the character.")]
		public Transform positionOffsetSpace;
		/// <summary>
		/// The weight curves for the interaction.
		/// </summary>
		public WeightCurve[] weightCurves;
		/// <summary>
		/// The weight curve multipliers for the interaction.
		/// </summary>
		public Multiplier[] multipliers;
		/// <summary>
		/// The interaction events.
		/// </summary>
		public InteractionEvent[] events;
		/// <summary>
		/// Gets the length of the interaction (the longest curve).
		/// </summary>
		public float length { get; private set; }
		/// <summary>
		/// The last InteractionSystem that started an interaction with this InteractionObject.
		/// </summary>
		/// <value>The last used interaction system.</value>
		public InteractionSystem lastUsedInteractionSystem { get; private set; }

		/// <summary>
		/// Call if you have changed the curves in play mode or added/removed InteractionTargets.
		/// </summary>
		public void Initiate() {
			// Push length to the last weight curve key
			for (int i = 0; i < weightCurves.Length; i++) {
				if (weightCurves[i].curve.length > 0) {
					float l = weightCurves[i].curve.keys[weightCurves[i].curve.length - 1].time;
					length = Mathf.Clamp(length, l, length);
				}
			}

			// Push length to the last event time
			for (int i = 0; i < events.Length; i++) {
				length = Mathf.Clamp(length, events[i].time, length);
			}
			
			targets = targetsRoot.GetComponentsInChildren<InteractionTarget>();
		}

		/// <summary>
		/// Gets the look at target (returns otherLookAtTarget if not null).
		/// </summary>
		public Transform lookAtTarget {
			get {
				if (otherLookAtTarget != null) return otherLookAtTarget;
				return transform;
			}
		}

		/// <summary>
		/// Gets the InteractionTarget of the specified effector type and InteractionSystem tag.
		/// </summary>
		public InteractionTarget GetTarget(FullBodyBipedEffector effectorType, InteractionSystem interactionSystem) {
			if (interactionSystem.tag == string.Empty || interactionSystem.tag == "") {
				foreach (InteractionTarget target in targets) {
					if (target.effectorType == effectorType) return target;
				}

				return null;
			}

			foreach (InteractionTarget target in targets) {
				if (target.effectorType == effectorType && target.tag == interactionSystem.tag) return target;
			}
			
			return null;
		}

		#endregion Main Interface

		// Returns true if the specified WeightCurve.Type is used by this InteractionObject
		public bool CurveUsed(WeightCurve.Type type) {
			foreach (WeightCurve curve in weightCurves) {
				if (curve.type == type) return true;
			}
			foreach (Multiplier multiplier in multipliers) {
				if (multiplier.result == type) return true;
			}
			return false;
		}

		// Returns all the InteractionTargets of this object
		public InteractionTarget[] GetTargets() {
			return targets;
		}

		// Returns the InteractionTarget of effector type and tag
		public Transform GetTarget(FullBodyBipedEffector effectorType, string tag) {
			if (tag == string.Empty || tag == "") return GetTarget(effectorType);
			
			for (int i = 0; i < targets.Length; i++) {
				if (targets[i].effectorType == effectorType && targets[i].tag == tag) return targets[i].transform;
			}

			return transform;
		}

		// Called when interaction is started with this InteractionObject
		public void OnStartInteraction(InteractionSystem interactionSystem) {
			this.lastUsedInteractionSystem = interactionSystem;
		}

		// Applies the weight curves and multipliers to the FBBIK solver
		public void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionTarget target, float timer, float weight) {

			for (int i = 0; i < weightCurves.Length; i++) {
				float mlp = target == null? 1f: target.GetValue(weightCurves[i].type);

				Apply(solver, effector, weightCurves[i].type, weightCurves[i].GetValue(timer), weight * mlp);
			}

			for (int i = 0; i < multipliers.Length; i++) {
				if (multipliers[i].curve == multipliers[i].result) {
					if (!Warning.logged) Warning.Log("InteractionObject Multiplier 'Curve' " + multipliers[i].curve.ToString() + "and 'Result' are the same.", transform);
				}

				int curveIndex = GetWeightCurveIndex(multipliers[i].curve);
					
				if (curveIndex != -1) {
					float mlp = target == null? 1f: target.GetValue(multipliers[i].result);

					Apply(solver, effector, multipliers[i].result, multipliers[i].GetValue(weightCurves[curveIndex], timer), weight * mlp);
				} else {
					if (!Warning.logged) Warning.Log("InteractionObject Multiplier curve " + multipliers[i].curve.ToString() + "does not exist.", transform);
				}
			}
		}

		// Gets the value of a weight curve/multiplier
		public float GetValue(WeightCurve.Type weightCurveType, InteractionTarget target, float timer) {
			int index = GetWeightCurveIndex(weightCurveType);

			if (index != -1) {
				float mlp = target == null? 1f: target.GetValue(weightCurveType);

				return weightCurves[index].GetValue(timer) * mlp;
			}

			for (int i = 0; i < multipliers.Length; i++) {
				if (multipliers[i].result == weightCurveType) {

					int wIndex = GetWeightCurveIndex(multipliers[i].curve);
					if (wIndex != -1) {
						float mlp = target == null? 1f: target.GetValue(multipliers[i].result);

						return multipliers[i].GetValue(weightCurves[wIndex], timer) * mlp;
					}
				}
			}

			return 0f;
		}

		// Get the root Transform of the targets
		public Transform targetsRoot {
			get {
				if (otherTargetsRoot != null) return otherTargetsRoot;
				return transform;
			}
		}

		private InteractionTarget[] targets = new InteractionTarget[0];

		// Initiate this Interaction Object
		void Start() {
			Initiate();
		}

		// Apply the curve to the specified solver, effector, with the value and weight.
		private void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, WeightCurve.Type type, float value, float weight) {
			switch(type) {
			case WeightCurve.Type.PositionWeight:
				solver.GetEffector(effector).positionWeight = Mathf.Lerp(solver.GetEffector(effector).positionWeight, value, weight);
				return;
			case WeightCurve.Type.RotationWeight:
				solver.GetEffector(effector).rotationWeight = Mathf.Lerp(solver.GetEffector(effector).rotationWeight, value, weight);
				return;
			case WeightCurve.Type.PositionOffsetX:
				solver.GetEffector(effector).position += (positionOffsetSpace != null? positionOffsetSpace.rotation: solver.GetRoot().rotation) * Vector3.right * value * weight;
				return;
			case WeightCurve.Type.PositionOffsetY:
				solver.GetEffector(effector).position += (positionOffsetSpace != null? positionOffsetSpace.rotation: solver.GetRoot().rotation) * Vector3.up * value * weight;
				return;
			case WeightCurve.Type.PositionOffsetZ:
				solver.GetEffector(effector).position += (positionOffsetSpace != null? positionOffsetSpace.rotation: solver.GetRoot().rotation) * Vector3.forward * value * weight;
				return;
			case WeightCurve.Type.Pull:
				solver.GetChain(effector).pull = Mathf.Lerp(solver.GetChain(effector).pull, value, weight);
				return;
			case WeightCurve.Type.Reach:
				solver.GetChain(effector).reach = Mathf.Lerp(solver.GetChain(effector).reach, value, weight);
				return;
			case WeightCurve.Type.Push:
				solver.GetChain(effector).push = Mathf.Lerp(solver.GetChain(effector).push, value, weight);
				return;
			case WeightCurve.Type.PushParent:
				solver.GetChain(effector).pushParent = Mathf.Lerp(solver.GetChain(effector).pushParent, value, weight);
				return;
            case WeightCurve.Type.BendGoalWeight:
                solver.GetChain(effector).bendConstraint.weight = Mathf.Lerp(solver.GetChain(effector).bendConstraint.weight, value, weight);
                return;
            }
		}

		// Gets the interaction target Transform
		private Transform GetTarget(FullBodyBipedEffector effectorType) {
			for (int i = 0; i < targets.Length; i++) {
				if (targets[i].effectorType == effectorType) return targets[i].transform;
			}
			return transform;
		}

		// Get the index of a weight curve of type
		private int GetWeightCurveIndex(WeightCurve.Type weightCurveType) {
			for (int i = 0; i < weightCurves.Length; i++) {
				if (weightCurves[i].type == weightCurveType) return i;
			}
			return -1;
		}

		// Get the index of a multiplayer of type
		private int GetMultiplierIndex(WeightCurve.Type weightCurveType) {
			for (int i = 0; i < multipliers.Length; i++) {
				if (multipliers[i].result == weightCurveType) return i;
			}
			return -1;
		}
	}
}
