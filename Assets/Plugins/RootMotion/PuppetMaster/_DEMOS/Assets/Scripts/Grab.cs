using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Demonstrates grabbing other puppets.
	public class Grab : MonoBehaviour {

		[Tooltip("The PuppetMaster this muscle belongs to.")]
		public PuppetMaster puppetMaster;

		[Tooltip("Used for switching walk/run by default.")]
		public UserControlMelee userControl;

		[Tooltip("The layers we wish to grab (optimization).")]
		public int grabLayer;

		private bool grabbed;
		private Rigidbody r;
		private Collider c;
		private BehaviourPuppet otherPuppet;
		private Collider otherCollider;
		private ConfigurableJoint joint;
		private float nextGrabTime;

		private const float massMlp = 5f;
		private const int solverIterationMlp = 10;

		void Start() {
			r = GetComponent<Rigidbody>();
			c = GetComponent<Collider>();
		}

		void OnCollisionEnter(Collision collision) {
			if (grabbed) return; // If we have not grabbed anything yet...
			if (Time.time < nextGrabTime) return; // ...and enough time has passed since the last release...
			if (collision.gameObject.layer != grabLayer) return; // ...and the collider is on the right layer...
			if (collision.rigidbody == null) return; // ...and it has a rigidbody attached.

			// Find MuscleCollisionBroadcaster that is a component added to all muscles by the PM, it broadcasts collisions events to PM and its behaviours.
			var m = collision.gameObject.GetComponent<MuscleCollisionBroadcaster>();
			if (m == null) return; // Make sure the collider we collided with is a muscle of a Puppet...
			if (m.puppetMaster == puppetMaster) return; // ...and it is not the same puppet as ours.

			// Unpin the puppet we collided with
			foreach (BehaviourBase b in m.puppetMaster.behaviours) {
				if (b is BehaviourPuppet) {
					otherPuppet = b as BehaviourPuppet;
					otherPuppet.SetState(BehaviourPuppet.State.Unpinned); // Unpin
					otherPuppet.canGetUp = false; // Make it not get up while being held
				}
			}

			if (otherPuppet == null) return; // If not BehaviourPuppet found, break out

			// Adding a ConfigurableJoint to link the two puppets
			joint = gameObject.AddComponent<ConfigurableJoint>();
			joint.connectedBody = collision.rigidbody;

			// Move the anchor to where the hand is (since we done have a rigidbody for the hand)
			joint.anchor = new Vector3(-0.35f, 0f, 0f);

			// Lock linear and angular motion of the joint
			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;
			joint.angularXMotion = ConfigurableJointMotion.Locked;
			joint.angularYMotion = ConfigurableJointMotion.Locked;
			joint.angularZMotion = ConfigurableJointMotion.Locked;

			// Increasing the mass of the linked Rigidbody when it is a part of a chain is the easiest way to improve link stability.
			r.mass *= massMlp;

			// Solver iteration count needs to be increased to improve stability of the link between 2 long joint chains.
			puppetMaster.solverIterationCount *= solverIterationMlp;

			// Ignore collisions with the object we grabbed
			otherCollider = collision.collider;
			Physics.IgnoreCollision(c, otherCollider, true);

			// The other puppet is heavy so slow down..
			userControl.walkByDefault = true;

			// We have successfully grabbed the other puppet
			grabbed = true;
		}

		void Update() {
			if (!grabbed) return;

			// Releasing the other puppet, restoring the initial state
			if (Input.GetKeyDown(KeyCode.X)) {
				Destroy(joint);
				r.mass /= massMlp;
				puppetMaster.solverIterationCount /= solverIterationMlp;
				userControl.walkByDefault = false;
				Physics.IgnoreCollision(c, otherCollider, false);
				otherPuppet.canGetUp = true;
				otherPuppet = null;
				otherCollider = null;
				grabbed = false;
				nextGrabTime = Time.time + 1f;
			}
		}
	}
}
