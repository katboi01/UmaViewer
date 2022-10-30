using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// All the required information when a muscle collides with something.
	/// </summary>
	public struct MuscleCollision {
		
		/// <summary>
		/// The index of the colliding muscle in the PuppetMaster.muscles array.
		/// </summary>
		public int muscleIndex;
		
		/// <summary>
		/// The collision from OnCollisionEnter/Stay/Exit.
		/// </summary>
		public Collision collision;

		public bool isStay;

		public MuscleCollision(int muscleIndex, Collision collision, bool isStay = false) {
			this.muscleIndex = muscleIndex;
			this.collision = collision;
			this.isStay = isStay;
		}
	}
	
	/// <summary>
	/// Hitting muscles via code, usually by raycasting.
	/// </summary>
	public struct MuscleHit {
		
		/// <summary>
		/// The index of the colliding muscle in the PuppetMaster.muscles array.
		/// </summary>
		public int muscleIndex;
		
		/// <summary>
		/// How much should the muscle be unpinned by the hit?
		/// </summary>
		public float unPin;
		
		/// <summary>
		/// The force to add to the muscle's Rigidbody.
		/// </summary>
		public Vector3 force;
		
		/// <summary>
		/// The world space hit point.
		/// </summary>
		public Vector3 position;
		
		public MuscleHit(int muscleIndex, float unPin, Vector3 force, Vector3 position) {
			this.muscleIndex = muscleIndex;
			this.unPin = unPin;
			this.force = force;
			this.position = position;
		}
	}

	/// <summary>
	/// Filters and broadcasts collisions with the Muscles to the Puppet Behaviours.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Muscle Collision Broadcaster")]
	public class MuscleCollisionBroadcaster : MonoBehaviour {

		/// <summary>
		/// The PuppetMaster that this muscle belongs to.
		/// </summary>
		[SerializeField][HideInInspector] public PuppetMaster puppetMaster;
		/// <summary>
		/// The index of this muscle in the PuppetMaster.muscles array.
		/// </summary>
		[SerializeField][HideInInspector] public int muscleIndex;
		
		private const string onMuscleHit = "OnMuscleHit";
		private const string onMuscleCollision = "OnMuscleCollision";
		private const string onMuscleCollisionExit = "OnMuscleCollisionExit";
		private MuscleCollisionBroadcaster otherBroadcaster;
		
		public void Hit(float unPin, Vector3 force, Vector3 position) {
			if (!enabled) return;
			foreach (BehaviourBase behaviour in puppetMaster.behaviours) {
				behaviour.OnMuscleHit(new MuscleHit(muscleIndex, unPin, force, position));
			}
		}
		
		void OnCollisionEnter(Collision collision) {
			if (!enabled) return;
			if (puppetMaster == null) return;
			if (collision.collider.transform.root == transform.root) return; // @todo make sure characters are not stacked to the same root
            if (puppetMaster.muscles[muscleIndex].state.isDisconnected) return;
			
			foreach (BehaviourBase behaviour in puppetMaster.behaviours) {
				behaviour.OnMuscleCollision(new MuscleCollision(muscleIndex, collision));
			}
		}

		void OnCollisionStay(Collision collision) {
			if (!enabled) return;
			if (puppetMaster == null) return;
			if (PuppetMasterSettings.instance != null && !PuppetMasterSettings.instance.collisionStayMessages) return;
			if (collision.collider.transform.root == transform.root) return;
            if (puppetMaster.muscles[muscleIndex].state.isDisconnected) return;

            foreach (BehaviourBase behaviour in puppetMaster.behaviours) {
				behaviour.OnMuscleCollision(new MuscleCollision(muscleIndex, collision, true));
			}
		}
		
		void OnCollisionExit(Collision collision) {
			if (!enabled) return;
			if (puppetMaster == null) return;
			if (PuppetMasterSettings.instance != null && !PuppetMasterSettings.instance.collisionExitMessages) return;
			if (collision.collider.transform.root == transform.root) return;
            if (puppetMaster.muscles[muscleIndex].state.isDisconnected) return;

            foreach (BehaviourBase behaviour in puppetMaster.behaviours) {
				behaviour.OnMuscleCollisionExit(new MuscleCollision(muscleIndex, collision));
			}
		}
	}
}
