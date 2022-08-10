using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;
using RootMotion;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// PuppetMaster prop designed to work with Prop Muscles.
    /// </summary>
    public class PuppetMasterProp : MonoBehaviour
    {
        [Tooltip("Mesh Root will be parented to Prop Muscle's target when this prop is picked up. To make sure the mesh and the colliders match up, Mesh Root's localPosition/Rotation must be zero.")]
        /// <summary>
        /// Mesh Root will be parented to Prop Muscle's target when this prop is picked up. To make sure the mesh and the colliders match up, Mesh Root's localPosition/Rotation must be zero.
        /// </summary>
        public Transform meshRoot;

        [Tooltip("The muscle properties that will be applied to the Prop Muscle when this prop is picked up.")]
        /// <summary>
        /// The muscle properties that will be applied to the Prop Muscle when this prop is picked up.
        /// </summary>
        public Muscle.Props muscleProps;

        [Tooltip("If true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.")]
        /// <summary>
        /// If true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.
        /// </summary>
        public bool forceLayers = true;

        [Tooltip("Mass of the prop while picked up. When dropped, mass of the original Rigidbody will be used.")]
        /// <summary>
        /// Mass of the prop while picked up. When dropped, mass of the original Rigidbody will be used.
        /// </summary>
        public float mass = 1f;

        [Tooltip("This has no other purpose but helping you distinguish props by PropMuscle.currentProp.propType.")]
        /// <summary>
        /// This has no other purpose but helping you distinguish PropMuscle.currentProp by type.
        /// </summary>
        public int propType;

        [LargeHeader("Materials")]

        [Tooltip("If assigned, sets prop colliders to this PhysicMaterial when picked up. If no material assigned, will maintain the original PhysicMaterial (unless otherwise controlled by BehaviourPuppet's Group Overrides).")]
        /// <summary>
        /// If assigned, sets prop colliders to this PhysicMaterial when picked up. If no materials assigned here, will maintain the original PhysicMaterial.
        /// </summary>
        public PhysicMaterial pickedUpMaterial;

        [LargeHeader("Additional Pin")]

        [Tooltip("Adds this to Prop Muscle's 'Additional Pin Offset' when this prop is picked up.")]
        /// <summary>
        /// Adds this to Prop Muscle's 'Additional Pin Offset' when this prop is picked up.
        /// </summary>
        public Vector3 additionalPinOffsetAdd;

        [Tooltip("The pin weight multiplier of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.")]
        /// <summary>
        /// The pin weight of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.
        /// </summary>
        [Range(0f, 1f)] public float additionalPinWeight = 1f;

        [Tooltip("Multiplies the mass of the additional pin by this value when this prop is picked up. The Rigidbody on this prop will be destroyed on pick-up and reattached on drop, so it's mass is not used while picked up.")]
        /// <summary>
        /// Multiplies the mass of the additional pin by this value when this prop is picked up. The Rigidbody on this prop will be destroyed on pick-up and reattached on drop, so it's mass is not used while picked up.
        /// </summary>
        public float additionalPinMass = 1f;

        /// <summary>
        /// Is the prop currently picked up?
        /// </summary>
        public bool isPickedUp { get; private set; }

        /// <summary>
        /// Returns either the Rigidbody of the prop when it is dropped or the Rigidbody of the prop muscle when it is picked up.
        /// </summary>
        public Rigidbody GetRigidbody()
        {
            if (r != null) return r;
            if (isPickedUp) return propMuscle.rigidbody;
            return null;
        }

        protected virtual void OnPickUp(PuppetMaster puppetMaster, int propMuscleIndex) { }
        protected virtual void OnDrop(PuppetMaster puppetMaster, int propMuscleIndex) { }
        protected Muscle propMuscle { get; private set; }

        private int defaultLayer;
        private Transform defaultParent;
        private Collider[] colliders = new Collider[0];
        private PhysicMaterial[] droppedMaterials = new PhysicMaterial[0];

        private Rigidbody r;
        private float _mass;
        private float _drag;
        private float _angularDrag;
        private bool _useGravity;
        private bool _isKinematic;
        private RigidbodyInterpolation _interpolation;
        private CollisionDetectionMode _collisionDetectionMode;
        private RigidbodyConstraints _constraints;
        private Collider[] emptyColliders = new Collider[0];

        // Called by PropMuscle when this prop is picked up
        public void PickUp(PuppetMaster puppetMaster, int propMuscleIndex)
        {
            RemoveRigidbody();

            transform.parent = puppetMaster.muscles[propMuscleIndex].transform;
            transform.position = puppetMaster.muscles[propMuscleIndex].transform.position;
            transform.rotation = puppetMaster.muscles[propMuscleIndex].transform.rotation;

            meshRoot.parent = puppetMaster.muscles[propMuscleIndex].target;
            meshRoot.localPosition = Vector3.zero;
            meshRoot.localRotation = Quaternion.identity;

            puppetMaster.muscles[propMuscleIndex].props = muscleProps;

            if (pickedUpMaterial != null)
            {
                foreach (Collider c in colliders)
                {
                    c.sharedMaterial = pickedUpMaterial;
                }
            }

            if (forceLayers)
            {
                foreach (Collider c in colliders)
                {
                    if (!c.isTrigger) c.gameObject.layer = puppetMaster.muscles[propMuscleIndex].joint.gameObject.layer;
                }
            }

            puppetMaster.muscles[propMuscleIndex].colliders = colliders;
            puppetMaster.UpdateInternalCollisions(puppetMaster.muscles[propMuscleIndex]);

            isPickedUp = true;
            propMuscle = puppetMaster.muscles[propMuscleIndex];

            OnPickUp(puppetMaster, propMuscleIndex);
        }

        // Called by PropMuscle when this prop is dropped
        public void Drop(PuppetMaster puppetMaster, int propMuscleIndex)
        {
            if (!puppetMaster.muscles[propMuscleIndex].joint.gameObject.activeInHierarchy)
            {
                transform.position = puppetMaster.muscles[propMuscleIndex].target.position;
                transform.rotation = puppetMaster.muscles[propMuscleIndex].target.rotation;
            }

            ReattachRigidbody();

            if (!puppetMaster.muscles[propMuscleIndex].joint.gameObject.activeInHierarchy || puppetMaster.muscles[propMuscleIndex].rigidbody.isKinematic)
            {
                r.velocity = puppetMaster.muscles[propMuscleIndex].mappedVelocity;
                r.angularVelocity = puppetMaster.muscles[propMuscleIndex].mappedAngularVelocity;
            }

            transform.parent = defaultParent;

            meshRoot.parent = transform;
            meshRoot.localPosition = Vector3.zero;
            meshRoot.localRotation = Quaternion.identity;

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].sharedMaterial = droppedMaterials[i];
            }

            puppetMaster.ResetInternalCollisions(puppetMaster.muscles[propMuscleIndex], false);
            puppetMaster.muscles[propMuscleIndex].colliders = emptyColliders;

            if (forceLayers)
            {
                foreach (Collider c in colliders)
                {
                    if (!c.isTrigger) c.gameObject.layer = defaultLayer;
                }
            }

            isPickedUp = false;
            propMuscle = null;

            OnDrop(puppetMaster, propMuscleIndex);
        }

        protected virtual void Awake()
        {
            r = GetComponent<Rigidbody>();
            defaultParent = transform.parent;

            colliders = GetComponentsInChildren<Collider>();

            droppedMaterials = new PhysicMaterial[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                droppedMaterials[i] = colliders[i].sharedMaterial;
            }
        }

        protected virtual void Start()
        {
            muscleProps.group = Muscle.Group.Prop;

            if (meshRoot == null)
            {
                Debug.LogError("PuppetMasterProp does not have a 'Mesh Root' Transform assigned.", transform);
                enabled = false;
                return;
            }
            if (meshRoot == transform)
            {
                Debug.LogError("PuppetMasterProp's 'Mesh Root' can not be the PuppetMasterProp's own Transform.", transform);
                enabled = false;
                return;
            }

            defaultLayer = gameObject.layer;
            foreach (Collider c in colliders)
            {
                if (!c.isTrigger)
                {
                    defaultLayer = c.gameObject.layer;
                    break;
                }
            }
        }

        protected virtual void Update()
        {
            if (isPickedUp)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        private void RemoveRigidbody()
        {
            if (r == null) return;

            _mass = r.mass;
            _drag = r.drag;
            _angularDrag = r.angularDrag;
            _useGravity = r.useGravity;
            _isKinematic = r.isKinematic;
            _interpolation = r.interpolation;
            _collisionDetectionMode = r.collisionDetectionMode;
            _constraints = r.constraints;

            Destroy(r);
        }

        private void ReattachRigidbody()
        {
            if (r != null) return;

            r = gameObject.AddComponent<Rigidbody>();
            r.mass = _mass;
            r.drag = _drag;
            r.angularDrag = _angularDrag;
            r.useGravity = _useGravity;
            r.isKinematic = _isKinematic;
            r.interpolation = _interpolation;
            r.collisionDetectionMode = _collisionDetectionMode;
            r.constraints = _constraints;
        }

        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            if (muscleProps != null) muscleProps.group = Muscle.Group.Prop;

            if (meshRoot != null && meshRoot != transform)
            {
                meshRoot.parent = transform;

                meshRoot.position = transform.position;
                meshRoot.rotation = transform.rotation;
            }
        }
    }
}
