using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {
	
	// Handles damaging the puppet via collisions and hits.
	public partial class BehaviourPuppet : BehaviourBase {

		private MuscleCollisionBroadcaster broadcaster;

		/// <summary>
		/// Knock out this puppet.
		/// </summary>
		public void Unpin() {
			Debug.Log("BehaviourPuppet.Unpin() has been deprecated. Use SetState(BehaviourPuppet.State) instead.");
			SetState(State.Unpinned);
		}

		// When a muscle is hit (through MuscleCollisionBroadcaster.Hit(...))
		protected override void OnMuscleHitBehaviour(MuscleHit hit) {
			// Should we activate the puppet?
			if (masterProps.normalMode == NormalMode.Kinematic) puppetMaster.mode = PuppetMaster.Mode.Active;

			// Unpin the muscle (and other muscles) and add force
			UnPin(hit.muscleIndex, hit.unPin);

			// Add force
			puppetMaster.muscles[hit.muscleIndex].SetKinematic(false);
			puppetMaster.muscles[hit.muscleIndex].rigidbody.AddForceAtPosition(hit.force, hit.position);
		}

        // When a muscle collides with something (called by the MuscleCollisionBroadcaster component on the muscle).
        protected override void OnMuscleCollisionBehaviour(MuscleCollision m) {
            if (OnCollision != null) OnCollision(m);

            // All the conditions for ignoring this
            if (!enabled) return;
            if (state == State.Unpinned) return;
            if (collisions > maxCollisions) return;
            if (!LayerMaskExtensions.Contains(collisionLayers, m.collision.gameObject.layer)) return;

            if (LayerMaskExtensions.Contains(groundLayers, m.collision.gameObject.layer)) {
                if (state == State.GetUp) return; // Do not damage if contact with ground layers and in getup state
                if (puppetMaster.muscles[m.muscleIndex].props.group == Muscle.Group.Foot) return; // Do not damage if feet in contact with ground layers
            }
            if (masterProps.normalMode == NormalMode.Kinematic && !puppetMaster.isActive && !masterProps.activateOnStaticCollisions && m.collision.gameObject.isStatic) return;

            // Get the collision impulse on the muscle
            float cT = collisionThreshold;
			float impulse = GetImpulse(m, ref cT);

			float minImpulseMlp = PuppetMasterSettings.instance != null? (1f + PuppetMasterSettings.instance.currentlyActivePuppets * PuppetMasterSettings.instance.activePuppetCollisionThresholdMlp): 1f;
			float minImpulse = cT * minImpulseMlp;

			if (impulse <= minImpulse) return;
			collisions ++;

            // Try to find out if it collided with another puppet's muscle
            if (m.collision.collider.attachedRigidbody != null) {	
				broadcaster = m.collision.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
				if (broadcaster != null) {
					if (broadcaster.muscleIndex < broadcaster.puppetMaster.muscles.Length) {
						// Multiply impulse (if the other muscle has been boosted)
						impulse *= broadcaster.puppetMaster.muscles[broadcaster.muscleIndex].state.impulseMlp;

						//float stayF = m.isStay? 0.05f: 0.1f;
						//broadcaster.puppetMaster.muscles[broadcaster.muscleIndex].offset -= m.collision.impulse * Time.deltaTime * stayF;
					}
				}
			}

            // DO not move this up, the impulse value will be wrong.
            // Let other scripts know about the collision (even the ones below collision threshold)
            if (OnCollisionImpulse != null) OnCollisionImpulse(m, impulse);

			// Should we activate the puppet?
			if (Activate(m.collision, impulse)) puppetMaster.mode = PuppetMaster.Mode.Active;

			// Unpin the muscle (and other muscles)
			UnPin(m.muscleIndex, impulse);
		}

		// Calculating the impulse magnitude from a collision
		private float GetImpulse(MuscleCollision m, ref float layerThreshold) {
			float i = m.collision.impulse.sqrMagnitude;

			i /= puppetMaster.muscles [m.muscleIndex].rigidbody.mass;
			i *= 0.3f; // Coeficient for evening out for pre-0.3 versions

			// Collision resistance multipliers
			foreach (CollisionResistanceMultiplier crm in collisionResistanceMultipliers) {
				if (LayerMaskExtensions.Contains(crm.layers, m.collision.collider.gameObject.layer)) {
					if (crm.multiplier <= 0f) i = Mathf.Infinity;
					else i /= crm.multiplier;

					layerThreshold = crm.collisionThreshold;

					break;
				}
			}

			return i;
		}

		// Unpin a muscle and other muscles linked to it
		private void UnPin(int muscleIndex, float unpin) {
			if (muscleIndex >= puppetMaster.muscles.Length) return;

			BehaviourPuppet.MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);
			
			for (int i = 0; i < puppetMaster.muscles.Length; i++) {
				UnPinMuscle(i, unpin * GetFalloff(i, muscleIndex, props.unpinParents, props.unpinChildren, props.unpinGroup));
			}

			hasCollidedSinceGetUp = true;
		}

		// Unpin a specific muscle according to it's collision resistance, immunity and other values
		private void UnPinMuscle(int muscleIndex, float unpin) {
			// All the conditions to ignore this
			if (unpin <= 0f) return;
			if (puppetMaster.muscles[muscleIndex].state.immunity >= 1f) return;

			// Find the group properties
			BehaviourPuppet.MuscleProps props = GetProps(puppetMaster.muscles[muscleIndex].props.group);

			// Making the puppet more resistant to collisions while getting up
			float stateF = 1f;
			if (state == State.GetUp) stateF = Mathf.Lerp(getUpCollisionResistanceMlp, 1f, puppetMaster.muscles[muscleIndex].state.pinWeightMlp);

			// Applying collision resistance
			float cR = collisionResistance.mode == Weight.Mode.Float? collisionResistance.floatValue: collisionResistance.GetValue(puppetMaster.muscles[muscleIndex].targetVelocity.magnitude);
			float damage = unpin / (props.collisionResistance * cR * stateF);
			damage *= 1f - puppetMaster.muscles[muscleIndex].state.immunity;

            // Finally apply the damage
            //puppetMaster.muscles[muscleIndex].state.pinWeightMlp -= damage;
            if (!puppetMaster.muscles[muscleIndex].state.isDisconnected)
            {
                puppetMaster.muscles[muscleIndex].state.pinWeightMlp = Mathf.Max(puppetMaster.muscles[muscleIndex].state.pinWeightMlp - damage, props.minPinWeight);
            }
        }
		
		private bool Activate(Collision collision, float impulse) {
			if (masterProps.normalMode != NormalMode.Kinematic) return false;
			if (puppetMaster.mode != PuppetMaster.Mode.Kinematic) return false;
			if (impulse < masterProps.activateOnImpulse) return false;
			
			if (collision.gameObject.isStatic) {
				return masterProps.activateOnStaticCollisions;
			}
			
			return true;
		}

	}
}