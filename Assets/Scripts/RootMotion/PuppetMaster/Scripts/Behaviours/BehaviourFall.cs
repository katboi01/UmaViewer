using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.Dynamics {
	
	/// <summary>
	/// Blends between two animation clips in a blend tree depending on the height of the ragdoll from the ground.
	/// </summary>
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page11.html")]
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourFall")]
	public class BehaviourFall : BehaviourBase {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		void OpenUserManual() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page11.html");
		}

		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		void OpenScriptReference() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_behaviour_fall.html");
		}

		[LargeHeader("Animation State")]

		[Tooltip("Animation State to crossfade to when this behaviour is activated.")]
		/// <summary>
		/// Animation State to crosfade to when this behaviour is activated.
		/// </summary>
		public string stateName = "Falling";

		[Tooltip("The duration of crossfading to 'State Name'. Value is in seconds.")]
		/// <summary>
		/// The duration of crossfading to "State Name". Value is in seconds.
		/// </summary>
		public float transitionDuration = 0.4f;

		[Tooltip("Layer index containing the destination state. If no layer is specified or layer is -1, the first state that is found with the given name or hash will be played.")]
		/// <summary>
		/// Layer index containing the destination state. If no layer is specified or layer is -1, the first state that is found with the given name or hash will be played.
		/// </summary>
		public int layer;

		[Tooltip("Start time of the current destination state. Value is in seconds. If no explicit fixedTime is specified or fixedTime value is float.NegativeInfinity, the state will either be played from the start if it's not already playing, or will continue playing from its current time and no transition will happen.")]
		/// <summary>
		/// Start time of the current destination state. Value is in seconds. If no explicit fixedTime is specified or fixedTime value is float.NegativeInfinity, the state will either be played from the start if it's not already playing, or will continue playing from its current time and no transition will happen.
		/// </summary>
		public float fixedTime;

		[LargeHeader("Blending")]

		[Tooltip("The layers that will be raycasted against to find colliding objects.")]
		/// <summary>
		/// The layers that will be raycasted against to find colliding objects.
		/// </summary>
		public LayerMask raycastLayers;

		[Tooltip("The parameter in the Animator that blends between catch fall and writhe animations.")]
		/// <summary>
		/// The parameter in the Animator that blends between catch fall and writhe animations.
		/// </summary>
		public string blendParameter = "FallBlend";

		[Tooltip("The height of the pelvis from the ground at which will blend to writhe animation.")]
		/// <summary>
		/// The height of the pelvis from the ground at which will blend to writhe animation.
		/// </summary>
		public float writheHeight = 4f;

		[Tooltip("The vertical velocity of the pelvis at which will blend to writhe animation.")]
		/// <summary>
		/// The vertical velocity of the pelvis at which will blend to writhe animation.
		/// </summary>
		public float writheYVelocity = 1f;

		[Tooltip("The speed of blendig between the two falling animations.")]
		/// <summary>
		/// The speed of blendig between the two falling animations.
		/// </summary>
		public float blendSpeed = 3f;

		[Tooltip("The speed of blending in mapping on activation.")]
		/// <summary>
		/// The speed of blending in mapping on activation.
		/// </summary>
		public float blendMappingSpeed = 1f;

		[LargeHeader("Ending")]

		[Tooltip("If false, this behaviour will never end.")]
		/// <summary>
		/// If false, this behaviour will never end.
		/// </summary>
		public bool canEnd;

		[Tooltip("The minimum time since this behaviour activated before it can end.")]
		/// <summary>
		/// The minimum time since this behaviour activated before it can end.
		/// </summary>
		public float minTime = 1.5f;

		[Tooltip("If the velocity of the pelvis falls below this value, can end the behaviour.")]
		/// <summary>
		/// If the velocity of the pelvis falls below this value, can end the behaviour.
		/// </summary>
		public float maxEndVelocity = 0.5f;

		[Tooltip("Event triggered when all end conditions are met.")]
		/// <summary>
		/// Event triggered when all end conditions are met.
		/// </summary>
		public PuppetEvent onEnd;

		private float timer;
		private bool endTriggered;
		
		protected override void OnActivate() {
			forceActive = true;
			StopAllCoroutines();
			StartCoroutine(SmoothActivate());
		}

		protected override void OnDeactivate() {
			forceActive = false;
		}

		public override void OnReactivate() {
			timer = 0f;
			endTriggered = false;
		}

		// Making sure all params are smoothly blended, not jumping simultaneously
		private IEnumerator SmoothActivate() {
			timer = 0f;
			endTriggered = false;
			puppetMaster.targetAnimator.CrossFadeInFixedTime(stateName, transitionDuration, layer, fixedTime);

			foreach (Muscle m in puppetMaster.muscles) {
				m.state.pinWeightMlp = 0f;

				m.rigidbody.velocity = m.mappedVelocity;
				m.rigidbody.angularVelocity = m.mappedAngularVelocity;
			}

			float fader = 0f;

			while (fader < 1f) {
				fader += Time.deltaTime;

				foreach (Muscle m in puppetMaster.muscles) {
					m.state.pinWeightMlp -= Time.deltaTime;
					m.state.mappingWeightMlp += Time.deltaTime * blendMappingSpeed;
				}

				yield return null;
			}
		}
		
		protected override void OnFixedUpdate() {
			if (raycastLayers == -1) Debug.LogWarning("BehaviourFall has no layers to raycast to.", transform);

			// Blending between catch fall and writhe animations
			float blendTarget = GetBlendTarget(GetGroundHeight());
			float blend = Mathf.MoveTowards(puppetMaster.targetAnimator.GetFloat(blendParameter), blendTarget, Time.deltaTime * blendSpeed);

			puppetMaster.targetAnimator.SetFloat(blendParameter, blend);

			// Ending conditions
			timer += Time.deltaTime;

			if (!endTriggered && canEnd && timer >= minTime && !puppetMaster.isBlending && puppetMaster.muscles[0].rigidbody.velocity.magnitude < maxEndVelocity) {
				endTriggered = true;
				onEnd.Trigger(puppetMaster);
				return;
			}
		}

		protected override void OnLateUpdate() {
			puppetMaster.targetRoot.position += puppetMaster.muscles[0].transform.position - puppetMaster.muscles[0].target.position;
			GroundTarget(raycastLayers);
		}

		public override void Resurrect() {
			foreach (Muscle m in puppetMaster.muscles) {
				m.state.pinWeightMlp = 0f;
			}
		}

		// 1 is writhe animation, 0 is catch fall
		private float GetBlendTarget(float groundHeight) {
			if (groundHeight > writheHeight) return 1f;

			Vector3 verticalVelocity = V3Tools.ExtractVertical(puppetMaster.muscles[0].rigidbody.velocity, puppetMaster.targetRoot.up, 1f);
			float velocityY = verticalVelocity.magnitude;
			if (Vector3.Dot(verticalVelocity, puppetMaster.targetRoot.up) < 0f) velocityY = -velocityY;

			if (velocityY > writheYVelocity) return 1f;

			//if (puppetMaster.muscles[0].rigidbody.velocity.y > writheYVelocity) return 1f;
			return 0f;
		}

		// Returns the height of the first muscle from the ground
		private float GetGroundHeight() {
			RaycastHit hit = new RaycastHit();
			
			if (Physics.Raycast(puppetMaster.muscles[0].rigidbody.position, -puppetMaster.targetRoot.up, out hit, 100f, raycastLayers)) {
				return hit.distance;
			}
			
			return Mathf.Infinity;
		}

        public override void OnMuscleReconnected(Muscle m)
        {
            base.OnMuscleReconnected(m);

            m.state.pinWeightMlp = 0f;
            m.state.muscleWeightMlp = 1;
            m.state.muscleDamperMlp = 1;
            m.state.maxForceMlp = 1;
            m.state.mappingWeightMlp = 1f;
        }
    }
}
