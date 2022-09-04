using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;
using RootMotion;

namespace RootMotion.Demos
{

    // An example of extending the PuppetMasterProp class to create some additional custom functionalities.
    public class PuppetMasterPropMelee : PuppetMasterProp
    {

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
        [Range(1f, 10f)] public float actionMassMlp = 1f;

        [Tooltip("Offset to the default center of mass of the Rigidbody (might improve prop handling).")]
        public Vector3 COMOffset;

        private float defaultColliderRadius;
        private float defaultMass;
        private float defaultAdditionalPinWeight;

        public void StartAction(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(Action(duration));
        }

        public IEnumerator Action(float duration)
        {
            // When action starts...
            // Increase collider radius
            capsuleCollider.radius = defaultColliderRadius * actionColliderRadiusMlp;

            // Increase rigidbody mass
            mass = defaultMass * actionMassMlp;

            // Increase additional pin weight
            additionalPinWeight = actionAdditionalPinWeight;

            // Wait for action to end...
            yield return new WaitForSeconds(duration);

            // Reset changes made above
            capsuleCollider.radius = defaultColliderRadius;
            mass = defaultMass;
            additionalPinWeight = defaultAdditionalPinWeight;
        }

        protected override void Start()
        {
            base.Start();

            // Store defaults
            defaultColliderRadius = capsuleCollider.radius;
            defaultAdditionalPinWeight = additionalPinWeight;
            defaultMass = mass;

            capsuleCollider.enabled = false;
            boxCollider.enabled = true;
        }

        // Called by PropMuscle when this prop is picked up
        protected override void OnPickUp(PuppetMaster puppetMaster, int propMuscleIndex)
        {
            // Called when the prop has been picked up and connected to a PropRoot.
            capsuleCollider.radius = defaultColliderRadius;

            propMuscle.rigidbody.centerOfMass += COMOffset;
            mass = defaultMass;

            // Toggle colliders (capsule collisions are smoother and capsule radius bigger)
            capsuleCollider.enabled = true;
            boxCollider.enabled = false;

            StopAllCoroutines();
        }

        // Called by PropMuscle when this prop is dropped
        protected override void OnDrop(PuppetMaster puppetMaster, int propMuscleIndex)
        {
            // Called when the prop has been dropped.
            capsuleCollider.radius = defaultColliderRadius;

            // Toggle colliders back so the prop accurately collides with the scene while dropped.
            capsuleCollider.enabled = false;
            boxCollider.enabled = true;

            StopAllCoroutines();
        }
    }
}
