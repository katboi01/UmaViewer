using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;
using RootMotion;

namespace RootMotion.Demos {
	
	// An example of extending the Prop class to create some additional custom functionalities.
	public class PropMelee: Prop {

		[LargeHeader("Melee")]

		[Tooltip("Switch to a CapsuleCollider when the prop is picked up so it behaves more smoothly when colliding with objects.")]
		public CapsuleCollider capsuleCollider;

		[Tooltip("The default BoxCollider used when this prop is not picked up.")]
		public BoxCollider boxCollider;

		[Tooltip("Temporarily increase the radius of the capsule collider when a hitting action is triggered, so it would not pass colliders too easily.")]
		public float actionColliderRadiusMlp = 1f;

		[Tooltip("Temporarily set (increase) the pin weight of the additional pin when a hitting action is triggered.")]
		[Range(0f, 1f)] public float actionAdditionalPinWeight = 1f;

		[Tooltip("Temporarily increase the mass of the Rigidbody when a hitting action is triggered.")]
		[Range(0.1f, 10f)] public float actionMassMlp = 1f;

		[Tooltip("Offset to the default center of mass of the Rigidbody (might improve prop handling).")]
		public Vector3 COMOffset;

		private float defaultColliderRadius;
		private float defaultMass;
		private float defaultAddMass;
		private Rigidbody r;

		public void StartAction(float duration) {
			StopAllCoroutines();
			StartCoroutine(Action(duration));
		}

		public IEnumerator Action(float duration) {
			capsuleCollider.radius = defaultColliderRadius * actionColliderRadiusMlp;
			r.mass = defaultMass * actionMassMlp;

			int additionalPinMuscleIndex = additionalPinTarget != null? propRoot.puppetMaster.GetMuscleIndex(additionalPinTarget): -1;
			if (additionalPinMuscleIndex != -1) {
				propRoot.puppetMaster.muscles[additionalPinMuscleIndex].props.pinWeight = actionAdditionalPinWeight;
			}

			yield return new WaitForSeconds(duration);

			capsuleCollider.radius = defaultColliderRadius;
			r.mass = defaultMass;
			if (additionalPinMuscleIndex != -1) {
				propRoot.puppetMaster.muscles[additionalPinMuscleIndex].props.pinWeight = additionalPinWeight;
			}

		}
		
		protected override void OnStart() {
			// Initiate stuff here.
			defaultColliderRadius = capsuleCollider.radius;

			r = muscle.GetComponent<Rigidbody>();
			r.centerOfMass += COMOffset;
			defaultMass = r.mass;
		}
		
		protected override void OnPickUp(PropRoot propRoot) {
			// Called when the prop has been picked up and connected to a PropRoot.
			capsuleCollider.radius = defaultColliderRadius;
			r.mass = defaultMass;

			capsuleCollider.enabled = true;
			boxCollider.enabled = false;

			StopAllCoroutines();
		}
		
		protected override void OnDrop() {
			// Called when the prop has been dropped.
			capsuleCollider.radius = defaultColliderRadius;
			r.mass = defaultMass;

			capsuleCollider.enabled = false;
			boxCollider.enabled = true;

			StopAllCoroutines();
		}
	}
}
