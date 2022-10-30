using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {
	
	// Switching and blending between Modes
	public partial class PuppetMaster: MonoBehaviour {

        /// <summary>
        /// Switches this PuppetMaster to PuppetMaster.Mode.Active.
        /// </summary>
        public void SwitchToActiveMode()
        {
            mode = Mode.Active;
        }

        /// <summary>
        /// Switches this PuppetMaster to PuppetMaster.Mode.Kinematic.
        /// </summary>
        public void SwitchToKinematicMode()
        {
            mode = Mode.Kinematic;
        }

        /// <summary>
        /// Switches this PuppetMaster to PuppetMaster.Mode.Disabled.
        /// </summary>
        public void SwitchToDisabledMode()
        {
            mode = Mode.Disabled;
        }

		/// <summary>
		/// Returns true if the PuppetMaster is in the middle of blending from a mode to mode.
		/// </summary>
		public bool isSwitchingMode { get; private set; }

		private Mode activeMode;
		private Mode lastMode;
		private float mappingBlend = 1f;

		/// <summary>
		/// Disables the Puppet immediately without waiting for normal mode switching procedures.
		/// </summary>
		public void DisableImmediately() {
			mappingBlend = 0f;
			isSwitchingMode = false;
			mode = Mode.Disabled;
			activeMode = mode;
			lastMode = mode;

			foreach (Muscle m in muscles) {
				m.rigidbody.gameObject.SetActive(false);
			}
		}

		// Master controller for switching modes. Mode switching is done by simply changing PuppetMaster.mode and can not be interrupted.
		protected virtual void SwitchModes() {
			if (!initiated) return;
			if (isKilling) mode = Mode.Active;
			if (!isAlive) mode = Mode.Active;

			foreach (BehaviourBase behaviour in behaviours) {
				if (behaviour.forceActive) {
					mode = Mode.Active;
					break;
				}
			}

			if (mode == lastMode) return;
			if (isSwitchingMode) return;
			if (isKilling && mode != Mode.Active) return;
			if (state != State.Alive && mode != Mode.Active) return;

            // Enable state switching here or else mapping won't be blended correctly

            isSwitchingMode = true;
			
			if (lastMode == Mode.Disabled) {
                if (mode == Mode.Kinematic) DisabledToKinematic();
                else if (mode == Mode.Active) StartCoroutine(DisabledToActive());
			}
			
			else if (lastMode == Mode.Kinematic) {
				if (mode == Mode.Disabled) KinematicToDisabled();
				else if (mode == Mode.Active) StartCoroutine(KinematicToActive());
			}
			
			else if (lastMode == Mode.Active) {
				if (mode == Mode.Disabled) StartCoroutine(ActiveToDisabled());
				else if (mode == Mode.Kinematic) StartCoroutine(ActiveToKinematic());
			}
			
			lastMode = mode;
		}

		// Switch from Disabled to Kinematic mode
		private void DisabledToKinematic() {
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected) m.Reset();
			}
			
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected)
                {
                    m.rigidbody.gameObject.SetActive(true);
                    m.SetKinematic(true);
                }
			}

            FlagInternalCollisionsForUpdate();

            foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected) m.MoveToTarget();
			}
			
			activeMode = Mode.Kinematic;
			isSwitchingMode = false;
		}

		// Blend from Disabled to Active mode
		private IEnumerator DisabledToActive() {
            foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected) m.Reset();
            }

            foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected)
                {
                    m.rigidbody.gameObject.SetActive(true);
                    m.SetKinematic(false);
                    m.rigidbody.WakeUp();
                    m.rigidbody.velocity = m.mappedVelocity;
                    m.rigidbody.angularVelocity = m.mappedAngularVelocity;
                }
			}
            

            FlagInternalCollisionsForUpdate();

            foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected) m.MoveToTarget();
            }

            Read();

            if (blendTime > 0f)
            {
                while (mappingBlend < 1f)
                {
                    mappingBlend = Mathf.Clamp(mappingBlend + Time.deltaTime / blendTime, 0f, 1f);
                    yield return null;
                }
            } else
            {
                mappingBlend = 1f;
            }
			
			activeMode = Mode.Active;
			isSwitchingMode = false;
		}

		// Switch from Kinematic to Disabled
		private void KinematicToDisabled() {
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected)
                {
                    m.rigidbody.gameObject.SetActive(false);
                }
			}
			
			activeMode = Mode.Disabled;
			isSwitchingMode = false;
		}

		// Blend from Kinematic to Active mode
		private IEnumerator KinematicToActive() {
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected)
                {
                    m.SetKinematic(false);
                    m.rigidbody.WakeUp();
                    m.rigidbody.velocity = m.mappedVelocity;
                    m.rigidbody.angularVelocity = m.mappedAngularVelocity;
                }
			}

			foreach (Muscle m in muscles) {
				if (!m.state.isDisconnected) m.MoveToTarget();
			}

            Read();

            if (blendTime > 0f)
            {
                while (mappingBlend < 1f)
                {
                    mappingBlend = Mathf.Clamp(mappingBlend + Time.deltaTime / blendTime, 0f, 1f);
                    yield return null;
                }
            }
            else
            {
                mappingBlend = 1f;
            }

            activeMode = Mode.Active;
			isSwitchingMode = false;
		}

		// Blend from Active to Disabled mode
		private IEnumerator ActiveToDisabled() {
            if (blendTime > 0f)
            {
                while (mappingBlend > 0f)
                {
                    mappingBlend = Mathf.Max(mappingBlend - Time.deltaTime / blendTime, 0f);
                    yield return null;
                }
            } else
            {
                mappingBlend = 0f;
            }
			
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected)
                {
                    m.rigidbody.gameObject.SetActive(false);
                }
			}

			activeMode = Mode.Disabled;
			isSwitchingMode = false;
		}

		// Blend from Active to Kinematic mode
		private IEnumerator ActiveToKinematic() {
            if (blendTime > 0f)
            {
                while (mappingBlend > 0f)
                {
                    mappingBlend = Mathf.Max(mappingBlend - Time.deltaTime / blendTime, 0f);
                    yield return null;
                }
            } else
            {
                mappingBlend = 0f;
            }
			
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected) m.SetKinematic(true);
			}
			
			foreach (Muscle m in muscles) {
                if (!m.state.isDisconnected) m.MoveToTarget();
			}

			activeMode = Mode.Kinematic;
			isSwitchingMode = false;
		}
	}
}