using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Switching and blending between Modes
	public partial class PuppetMaster: MonoBehaviour {

		[System.Serializable]
		public enum State {
			Alive,
			Dead,
			Frozen
		}

		[System.Serializable]
		public struct StateSettings {

			[TooltipAttribute("How much does it take to weigh out muscle weight to deadMuscleWeight?")]
			public float killDuration;

			[TooltipAttribute("The muscle weight mlp while the puppet is Dead.")]
			public float deadMuscleWeight;

			[TooltipAttribute("The muscle damper add while the puppet is Dead.")]
			public float deadMuscleDamper;

			[TooltipAttribute("The max square velocity of the ragdoll bones for freezing the puppet.")]
			public float maxFreezeSqrVelocity;

			[TooltipAttribute("If true, PuppetMaster, all it's behaviours and the ragdoll will be destroyed when the puppet is frozen.")]
			public bool freezePermanently;

			[TooltipAttribute("If true, will enable angular limits when killing the puppet.")]
			public bool enableAngularLimitsOnKill;

			[TooltipAttribute("If true, will enable internal collisions when killing the puppet.")]
			public bool enableInternalCollisionsOnKill;

			public StateSettings(float killDuration, float deadMuscleWeight = 0.01f, float deadMuscleDamper = 2f, float maxFreezeSqrVelocity = 0.02f, bool freezePermanently = false, bool enableAngularLimitsOnKill = true, bool enableInternalCollisionsOnKill = true) {
				this.killDuration = killDuration;
				this.deadMuscleWeight = deadMuscleWeight;
				this.deadMuscleDamper = deadMuscleDamper;
				this.maxFreezeSqrVelocity = maxFreezeSqrVelocity;
				this.freezePermanently = freezePermanently;
				this.enableAngularLimitsOnKill = enableAngularLimitsOnKill;
				this.enableInternalCollisionsOnKill = enableInternalCollisionsOnKill;
			}

			public static StateSettings Default {
				get {
					return new StateSettings(1f, 0.01f, 2f, 0.02f, false, true, true);
				}
			}
		}

		/// <summary>
		/// Returns true if PuppetMaster is in the middle of switching states. Don't deactivate the PuppetMaster nor any of it's behaviours while it returns true.
		/// </summary>
		public bool isSwitchingState {
			get {
				return activeState != state;
			}
		}

		/// <summary>
		/// Is the puppet in the middle of a killing procedure (started by PuppetMaster.Kill())?
		/// </summary>
		public bool isKilling { get; private set; }

		/// <summary>
		/// Is the puppet alive or still dying?
		/// </summary>
		public bool isAlive { get { return activeState == State.Alive; }}

		/// <summary>
		/// Is the puppet frozen?
		/// </summary>
		public bool isFrozen { get { return activeState == State.Frozen; }}

		/// <summary>
		/// Kill the puppet with the specified StateSettings
		/// </summary>
		public void Kill() {
			state = State.Dead;
		}

		/// <summary>
		/// Kill the puppet with the specified StateSettings
		/// </summary>
		public void Kill(StateSettings stateSettings) {
			this.stateSettings = stateSettings;
			state = State.Dead;
		}

		/// <summary>
		/// Unfreeze the puppet with the specified StateSettings.
		/// </summary>
		public void Freeze() {
			state = State.Frozen;
		}

		/// <summary>
		/// Unfreeze the puppet with the specified StateSettings.
		/// </summary>
		public void Freeze(StateSettings stateSettings) {
			this.stateSettings = stateSettings;
			state = State.Frozen;
		}

		/// <summary>
		/// Resurrect this instance from the Dead or Frozen state (unless frozen permanently).
		/// </summary>
		public void Resurrect() {
			state = State.Alive;
		}

		public UpdateDelegate OnFreeze;
		public UpdateDelegate OnUnfreeze;
		public UpdateDelegate OnDeath;
		public UpdateDelegate OnResurrection;

		private State activeState;
		private State lastState;
		private bool angularLimitsEnabledOnKill;
		private bool internalCollisionsEnabledOnKill;
		private bool animationDisabledbyStates;

		// Master controller for switching modes. Mode switching is done by simply changing PuppetMaster.mode and can not be interrupted.
		protected virtual void SwitchStates() {
			if (state == lastState) return;
			if (isKilling) return;

			if (freezeFlag) {
				if (state == State.Alive) {
					activeState = State.Dead;
					lastState = State.Dead;
					freezeFlag = false;
				}
				else if (state == State.Dead) {
					lastState = State.Dead;
					freezeFlag = false;
					return;
				}

				if (freezeFlag) return;
			}

			if (lastState == State.Alive) {
				if (state == State.Dead) StartCoroutine(AliveToDead(false));
				else if (state == State.Frozen) StartCoroutine(AliveToDead(true));
			}
			else if (lastState == State.Dead) {
				if (state == State.Alive) DeadToAlive();
				else if (state == State.Frozen) DeadToFrozen();
			}
			else if (lastState == State.Frozen) {
				if (state == State.Alive) FrozenToAlive();
				else if (state == State.Dead) FrozenToDead();
			}

			lastState = state;
		}

		private IEnumerator AliveToDead(bool freeze) {
			isKilling = true;
			
			mode = Mode.Active;

			if (stateSettings.enableAngularLimitsOnKill) {
				if (!angularLimits) {
					angularLimits = true;
					angularLimitsEnabledOnKill = true;
				}
			}

			if (stateSettings.enableInternalCollisionsOnKill) {
				if (!internalCollisions) {
					internalCollisions = true;
					internalCollisionsEnabledOnKill = true;
				}
			}
			
			// Set pin weight to 0 to play with joint target rotations only
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected)
                {
                    m.state.pinWeightMlp = 0f;
                    m.state.muscleDamperAdd = stateSettings.deadMuscleDamper;

                    m.rigidbody.velocity = m.mappedVelocity;
                    m.rigidbody.angularVelocity = m.mappedAngularVelocity;
                }
			}

			float range = muscles[0].state.muscleWeightMlp - stateSettings.deadMuscleWeight;
			foreach (BehaviourBase behaviour in behaviours) behaviour.KillStart();

			if (stateSettings.killDuration > 0f && range > 0f) {
				float mW = muscles[0].state.muscleWeightMlp;

				while (mW > stateSettings.deadMuscleWeight) {
					mW = Mathf.Max(mW - Time.deltaTime * (range / stateSettings.killDuration), stateSettings.deadMuscleWeight);

					foreach (Muscle m in muscles) m.state.muscleWeightMlp = mW;
					
					yield return null;
				}
			} 
			foreach (Muscle m in muscles) m.state.muscleWeightMlp = stateSettings.deadMuscleWeight;

			// Disable the Animator
			SetAnimationEnabled(false);

			isKilling = false;
			activeState = State.Dead;

			if (freeze) freezeFlag = true;
			foreach (BehaviourBase behaviour in behaviours) behaviour.KillEnd();

			if (OnDeath != null) OnDeath();
		}

		private void OnFreezeFlag() {
			if (!CanFreeze()) return;

			SetAnimationEnabled(false);

			foreach (Muscle m in muscles) {
				m.joint.gameObject.SetActive(false);
			}

			foreach (BehaviourBase behaviour in behaviours) {
				behaviour.Freeze();
				
				if (behaviour.gameObject.activeSelf) {
					behaviour.deactivated = true;
					behaviour.gameObject.SetActive(false);
				}
			}

			freezeFlag = false;
			activeState = State.Frozen;

			if (OnFreeze != null) OnFreeze();

			if (stateSettings.freezePermanently) {
				if (behaviours.Length > 0 && behaviours[0] != null) {
					Destroy(behaviours[0].transform.parent.gameObject);
				}
				Destroy(gameObject);
			}
		}
	
		private void DeadToAlive() {
			foreach (Muscle m in muscles) {
				m.state.pinWeightMlp = 1f;
				m.state.muscleWeightMlp = 1f;
				m.state.muscleDamperAdd = 0f;
			}

			if (angularLimitsEnabledOnKill) {
				angularLimits = false;
				angularLimitsEnabledOnKill = false;
			}
			if (internalCollisionsEnabledOnKill) {
				internalCollisions = false;
				internalCollisionsEnabledOnKill = false;
			}
			
			// Reset all
			foreach (BehaviourBase b in behaviours) {
				b.Resurrect();
			}
			
			SetAnimationEnabled(true);
			
			activeState = State.Alive;
			if (OnResurrection != null) OnResurrection();
		}

		private void SetAnimationEnabled(bool to) {
			// What if animator not disabled while still dying?
			//if (!to) animationDisabledbyStates = true;
			//else if (!animationDisabledbyStates) return;

			animatorDisabled = false;

			if (targetAnimator != null) {
                targetAnimator.enabled = to;
				//if (to) targetAnimator.Update(0.001f); // Don't do this when getting up, will flicker animated pose for a moment
			}

			if (targetAnimation != null) targetAnimation.enabled = to;
		}

		private void DeadToFrozen() {
			freezeFlag = true;
		}

		private void FrozenToAlive() {
			freezeFlag = false;

			foreach (Muscle m in muscles) {
				m.state.pinWeightMlp = 1f;
				m.state.muscleWeightMlp = 1f;
				m.state.muscleDamperAdd = 0f;
			}

			if (angularLimitsEnabledOnKill) {
				angularLimits = false;
				angularLimitsEnabledOnKill = false;
			}
			if (internalCollisionsEnabledOnKill) {
				internalCollisions = false;
				internalCollisionsEnabledOnKill = false;
			}

			ActivateRagdoll();
			
			foreach (BehaviourBase behaviour in behaviours) {
				behaviour.Unfreeze();
				behaviour.Resurrect();

				if (behaviour.deactivated) behaviour.gameObject.SetActive(true);
			}

			if (targetAnimator != null) targetAnimator.enabled = true;
			if (targetAnimation != null) targetAnimation.enabled = true;
			
			activeState = State.Alive;

			if (OnUnfreeze != null) OnUnfreeze();
			if (OnResurrection != null) OnResurrection();
		}

		private void FrozenToDead() {
			freezeFlag = false;

			ActivateRagdoll();
			
			foreach (BehaviourBase behaviour in behaviours) {
				behaviour.Unfreeze();

				if (behaviour.deactivated) behaviour.gameObject.SetActive(true);
			}

			activeState = State.Dead;
			if (OnUnfreeze != null) OnUnfreeze();
		}

		private void ActivateRagdoll(bool kinematic = false) {
			foreach (Muscle m in muscles) {
				m.Reset();
			}

			foreach (Muscle m in muscles) {
				m.joint.gameObject.SetActive(true);
                if (kinematic) m.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                m.SetKinematic(kinematic);
				m.rigidbody.velocity = Vector3.zero;
				m.rigidbody.angularVelocity = Vector3.zero;
			}

            FlagInternalCollisionsForUpdate();

            // Fix target to whatever it was mapped to last frame
            //SampleTargetMappedState();
            //FixTargetToSampledState(1f);


            Read();

            foreach (Muscle m in muscles) {
				m.MoveToTarget();
			}

        }

		private bool CanFreeze() {
			foreach (Muscle m in muscles) {
				if (m.rigidbody.velocity.sqrMagnitude > stateSettings.maxFreezeSqrVelocity) return false;
			}
			return true;
		}
	}
}