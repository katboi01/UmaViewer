using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// This is just a commented template for creating new Puppet Behaviours.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourTemplate")]
	public class BehaviourTemplate : BehaviourBase {

		// Just for demonstrating the intended use of sub-behaviours. 
		// The sub-behaviours take care of behaviour code reusability.
		// While there can be only one active Puppet Behaviour at a time, that active behaviour can use multiple independent and reusable sub-behaviours simultaneously.
		// For example the SubBehaviourCOM is responsible for calculating everything about the center of mass and can be used by any behaviour or even other sub-behaviours that need CoM calculations.
		// This is the base abstract class for all sub-behaviours.
		public SubBehaviourCOM centerOfMass;

		// Used by the SubBehaviourCOM
		public LayerMask groundLayers;

		// Just for demonstrating the intended use of PuppetEvents
		public PuppetEvent onLoseBalance;

		// A PuppetEvent will be called when the balance angle exceeds this point.
		public float loseBalanceAngle = 60f;

		protected override void OnInitiate() {
			// Initiate something. This is called only once by the PuppetMaster in Start().

			// Initiating sub-behaviours. SubBehaviourCOM will update automatically once it has been initiated
			centerOfMass.Initiate(this as BehaviourBase, groundLayers);
		}
		
		protected override void OnActivate() {
			// When this becomes the active behaviour. There can only be one active behaviour. 
			// Switching behaviours is done by the behaviours themselves, using PuppetEvents.
			// Each behaviour should know when it is no longer required and which behaviours to switch to in each case.
		}

		public override void OnReactivate() {
			// Called when the PuppetMaster has been deactivated (by parenting it to an inactive hierarchy or calling SetActive(false)).
		}

		protected override void OnDeactivate() {
			// Called when this behaviour is exited. OnActivate is the place for resetting variables to defaults though.
		}

		protected override void OnFixedUpdate() {
			// Everything happening in the fixed time step.

			// Example of using PuppetEvents
			if (centerOfMass.angle > loseBalanceAngle) {
				// If the angle between Vector3.up and the vector from the center of pressure to the center of mass > loseBalanceangle, lose balance (maybe switch to another behaviour).
				onLoseBalance.Trigger(puppetMaster);
			}
		}

		protected override void OnLateUpdate() {
			// Everything happening in LateUpdate().
		}

		protected override void OnMuscleHitBehaviour(MuscleHit hit) {
			if (!enabled) return;

			// If the muscle has been hit via code using MuscleCollisionBroadcaster.Hit(float unPin, Vector3 force, Vector3 position);
			// This is used for shooting based on raycasting instead of physical collisions.
		}

		protected override void OnMuscleCollisionBehaviour(MuscleCollision m) {
			if (!enabled) return;

			// If the muscle has collided with something that is on the PuppetMaster's collision layers.
		}
	}
}
