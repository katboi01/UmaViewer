using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

[RequireComponent(typeof(ParticleSystem))]
public class FXCollisionBlood : MonoBehaviour {

		public BehaviourPuppet puppet;
		public float minCollisionImpulse = 100f;
		public int emission = 2;
		public float emissionImpulseAdd = 0.01f;
		public int maxEmission = 7;

		private ParticleSystem particles;

		void Start () {
			particles = GetComponent<ParticleSystem>();

			puppet.OnCollisionImpulse += OnCollisionImpulse;
		}

		void OnCollisionImpulse(MuscleCollision m, float impulse) {
			if (m.collision.contacts.Length == 0) return;
			if (impulse < minCollisionImpulse) return;

            // Do not emit blood from prop contacts with static objects
            if (puppet.puppetMaster.muscles[m.muscleIndex].props.group == Muscle.Group.Prop && (m.collision.collider.attachedRigidbody == null || m.collision.collider.attachedRigidbody.isKinematic)) return;

#if UNITY_2018_3_OR_NEWER
            transform.position = m.collision.GetContact(0).point; // Non-GC-allocating way of getting the contact.
#else
            transform.position = m.collision.contacts[0].point;
#endif
            transform.rotation = Quaternion.LookRotation(m.collision.contacts[0].normal);

			particles.Emit(Mathf.Min(emission + (int)(emissionImpulseAdd * impulse), maxEmission));
		}

		void OnDestroy() {
			if (puppet != null) puppet.OnCollisionImpulse -= OnCollisionImpulse;
		}
	}
}
