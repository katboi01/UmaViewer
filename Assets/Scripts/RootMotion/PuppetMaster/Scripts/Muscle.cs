using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// Uses a ConfigurableJoint to make a Rigidbody follow the position and rotation (in joint space) of an animated target.
    /// </summary>
    [System.Serializable]
    public class Muscle
    {

        #region Main Properties

        /// <summary>
        /// Muscle Groups are used by Puppet Behaviours to discriminate body parts.
        /// </summary>
        [System.Serializable]
        public enum Group
        {
            Hips,
            Spine,
            Head,
            Arm,
            Hand,
            Leg,
            Foot,
            Tail,
            Prop
        }

        /// <summary>
        /// Defines which muscles or muscle groups internal collisions are always ignored with.
        /// </summary>
        [System.Serializable]
        public class InternalCollisionIgnoreSettings
        {
            [Tooltip("If true, internal collisions between this muscle and all other muscles will be ingored.")]
            public bool ignoreAll;

            [Tooltip("Ignore internal collisions with all muscles in this array.")]
            public ConfigurableJoint[] muscles = new ConfigurableJoint[0];

            [Tooltip("Ignore internal collisions with all these groups.")]
            public Group[] groups = new Group[0];
        }

        /// <summary>
        /// The main properties of a muscle.
        /// </summary>
        [System.Serializable]
        public class Props
        {

            [Tooltip("Which body part does this muscle belong to?")]
            /// <summary>
            /// Which body part does this muscle belong to?
            /// </summary>
            public Group group;

            [Tooltip("The weight (multiplier) of mapping this muscle's target to the muscle.")]
            /// <summary>
            /// The weight (multiplier) of mapping this muscle's target to the muscle.
            /// </summary>
            [Range(0f, 1f)] public float mappingWeight = 1f;

            [Tooltip("The weight (multiplier) of pinning this muscle to it's target's position using a simple AddForce command.")]
            /// <summary>
            /// The weight (multiplier) of pinning this muscle to it's target's position using a simple AddForce command.
            /// </summary>
            [Range(0f, 1f)] public float pinWeight = 1f;

            [Tooltip("The muscle strength (multiplier).")]
            /// <summary>
            /// The muscle strength (multiplier).
            /// </summary>
            [Range(0f, 1f)] public float muscleWeight = 1f;

            [Tooltip("Multiplier of the positionDamper of the ConfigurableJoints' Slerp Drive.")]
            /// <summary>
            /// Multiplier of the positionDamper of the ConfigurableJoints' Slerp Drive.
            /// </summary>
            [Range(0f, 1f)] public float muscleDamper = 1f;

            [Tooltip("If true, will map the target to the world space position of the muscle. Normally this should be true for only the root muscle (the hips).")]
            /// <summary>
            /// If true, will map the target to the world space position of the muscle. Normally this should be true for only the root muscle (the hips).
            /// </summary>
            public bool mapPosition;

            [Tooltip("Defines which muscles or muscle groups internal collisions are always ignored with.")]
            /// <summary>
            /// Defines which muscles or muscle groups internal collisions are always ignored with.
            /// </summary>
            public InternalCollisionIgnoreSettings internalCollisionIgnores = new InternalCollisionIgnoreSettings();

            [Tooltip("List of animated bones parented to this muscle's Target, except for the bones that are targets or target children of any child muscles. This is used for stopping animation on those bones when the muscle has been disconnected using PuppetMaster.DisconnectMuscleRecursive().For example if you disconnected the spine02 muscle, you would want to have spine03 and clavicles in this list to stop them from animating.")]
            /// <summary>
            /// List of animated bones parented to this muscle's Target, except for the bones that are targets or target children of any child muscles. This is used for stopping animation on those bones when the muscle has been disconnected using PuppetMaster.DisconnectMuscleRecursive(). For example if you disconnected the spine02 muscle, you would want to have spine03 and clavicles in this list to stop them from animating.
            /// </summary>
            public Transform[] animatedTargetChildren = new Transform[0];

            /// <summary>
            /// Initializes a new instance of the <see cref="RootMotion.Dynamics.Muscle+Props"/> class.
            /// </summary>
            public Props()
            {
                this.mappingWeight = 1f;
                this.pinWeight = 1f;
                this.muscleWeight = 1f;
                this.muscleDamper = 1f;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RootMotion.Dynamics.Muscle+Props"/> class.
            /// </summary>
            /// <param name="pinWeight">Pin weight.</param>
            /// <param name="muscleWeight">Muscle weight.</param>
            /// <param name="mappingWeight">Mapping weight.</param>
            /// <param name="muscleDamper">Muscle damper.</param>
            /// <param name="mapPosition">If set to <c>true</c> the target will be mapped to also the world space position of the Muscle.</param>
            /// <param name="group">Group.</param>
            public Props(float pinWeight, float muscleWeight, float mappingWeight, float muscleDamper, bool mapPosition, Group group = Group.Hips)
            {
                this.pinWeight = pinWeight;
                this.muscleWeight = muscleWeight;
                this.mappingWeight = mappingWeight;
                this.muscleDamper = muscleDamper;
                this.group = group;
                this.mapPosition = mapPosition;
            }

            public void Clamp()
            {
                mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
                pinWeight = Mathf.Clamp(pinWeight, 0f, 1f);
                muscleWeight = Mathf.Clamp(muscleWeight, 0f, 1f);
                muscleDamper = Mathf.Clamp(muscleDamper, 0f, 1f);
            }
        }

        /// <summary>
        /// The current state of a muscle. While the similar values in the Props should be defined by the user, this multiplies or adds to them and is hidden and intended for being used by the Puppet Behaviours only.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// Multiplies the mapping weight of the muscle.
            /// </summary>
            public float mappingWeightMlp;

            /// <summary>
            /// Multiplies the pin weight of the muscle.
            /// </summary>
            public float pinWeightMlp;

            /// <summary>
            /// Multiplies the muscle weight.
            /// </summary>
            public float muscleWeightMlp;

            /// <summary>
            /// Multiplies slerp drive's max force.
            /// </summary>
            public float maxForceMlp;

            /// <summary>
            /// Used by the behaviours to cancel out muscle damper so it could be set to a specific value by muscleDamperAdd.
            /// </summary>
            public float muscleDamperMlp;

            /// <summary>
            /// Adds to the muscle's damper value (can't multiply because it might be set to zero).
            /// </summary>
            public float muscleDamperAdd;

            /// <summary>
            /// Immunity reduces damage from collisions and hits in some Puppet Behaviours (BehaviourPuppet).
            /// </summary>
            public float immunity;

            /// <summary>
            /// Larger impulse multiplier makes the muscles deal more damage to the muscles of other characters in some Puppet Behavours (BehaviourPuppet).
            /// </summary>
            public float impulseMlp;

            /// <summary>
            /// The velocity of the muscle in world space (filled in by the muscle itself).
            /// </summary>
            public Vector3 velocity;

            /// <summary>
            /// The angular velocity of the muscle (filled in by the muscle itself).
            /// </summary>
            public Vector3 angularVelocity;

            /// <summary>
            /// If the muscle is currently disconnected.
            /// </summary>
            public bool isDisconnected;

            /// <summary>
            /// If muscle joint orientation needs to be reset when reconnecting.
            /// </summary>
            public bool resetFlag;

            public static State Default
            {
                get
                {
                    State state = new State();

                    state.mappingWeightMlp = 1f;
                    state.pinWeightMlp = 1f;
                    state.muscleWeightMlp = 1f;
                    state.muscleDamperMlp = 1f;
                    state.muscleDamperAdd = 0f;
                    state.maxForceMlp = 1f;
                    state.immunity = 0f;
                    state.impulseMlp = 1f;
                    state.isDisconnected = false;

                    return state;
                }
            }

            public void Clamp()
            {
                mappingWeightMlp = Mathf.Clamp(mappingWeightMlp, 0f, 1f);
                pinWeightMlp = Mathf.Clamp(pinWeightMlp, 0f, 1f);
                muscleWeightMlp = Mathf.Clamp(muscleWeightMlp, 0f, muscleWeightMlp);
                immunity = Mathf.Clamp(immunity, 0f, 1f);
                impulseMlp = Mathf.Max(impulseMlp, 0f);
            }
        }

        // Only for displaying a meaningful name in the Inspector instead of "Element n".
        [HideInInspector] public string name;

        /// <summary>
        /// The ConfigurableJoint used by this muscle.
        /// </summary>
        public ConfigurableJoint joint;

        /// <summary>
        /// The target Transform that this muscle tries to follow.
        /// </summary>
        public Transform target;

        /// <summary>
        /// The main properties of the muscle.
        /// </summary>
        public Props props = new Props();

        /// <summary>
        /// The current state of the muscle. While the similar values in the Props should be defined by the user, this multiplies or adds to them and is hidden and intended for being used by the Puppet Behaviours only.
        /// </summary>
        public State state = State.Default;

        /// <summary>
        /// The indexes (of the PuppetMaster.muscles array) of all the parent muscles of this muscle.
        /// </summary>
        [HideInInspector] public int[] parentIndexes = new int[0];

        /// <summary>
        /// The indexes (of the PuppetMaster.muscles array) of all the child muscles of this muscle.
        /// </summary>
        [HideInInspector] public int[] childIndexes = new int[0];

        /// <summary>
        /// Flags for all the bones of the PuppetMaster indicating whether they are children of this muscle or not.
        /// </summary>
        [HideInInspector] public bool[] childFlags = new bool[0];

        /// <summary>
        /// How many muscles are between this muscle and all the other muscles of the PuppetMaster.
        /// </summary>
        [HideInInspector] public int[] kinshipDegrees = new int[0];

        /// <summary>
        /// The muscle collision event broadcaster component on the Rigidbody.
        /// </summary>
        [HideInInspector] public MuscleCollisionBroadcaster broadcaster;

        /// <summary>
        /// Broadcasts OnJointBreak events to PuppetMaster.
        /// </summary>
        [HideInInspector] public JointBreakBroadcaster jointBreakBroadcaster;

        /// <summary>
        /// Gets the offset of this muscle from it's target.
        /// </summary>
        [HideInInspector] public Vector3 positionOffset;

        /// <summary>
        /// Gets the Transform of this muscle. This is filled in only after the muscle has initiated in Start().
        /// </summary>
        public Transform transform { get; private set; }

        /// <summary>
        /// Gets the Rigidbody of this muscle. This is filled in only after the muscle has initiated in Start().
        /// </summary>
        public Rigidbody rigidbody { get; private set; }

        /// <summary>
        /// Gets the target of this muscle joint's connectedBody if it has any. This is filled in only after the muscle has initiated in Start().
        /// </summary>
        public Transform connectedBodyTarget { get; private set; }

        /// <summary>
        /// Gets the last read world space position of the target.
        /// </summary>
        public Vector3 targetAnimatedPosition { get; private set; }

        /// <summary>
		/// Gets the last read world space rotation of the target.
		/// </summary>
		public Quaternion targetAnimatedWorldRotation { get; private set; }

        /// <summary>
        /// All the colliders of this muscle (including possible compound colliders).
        /// </summary>
        public Collider[] colliders
        {
            get
            {
                return _colliders;
            }
            set
            {
                _colliders = value;
            }
        }

        /// <summary>
        /// Gets the velocity of the target Transform.
        /// </summary>
        public Vector3 targetVelocity { get; private set; }

        /*
		/// <summary>
		/// Gets the angular velocity of the target Transform.
		/// </summary>
		public Vector3 targetAngularVelocity { get; private set; }
        */

        /// <summary>
        /// The Rigidbody of additional pin.
        /// </summary>
        public Rigidbody additionalRigidbody { get; private set; }

        /// <summary>
        /// The additional pin joint (optional);
        /// </summary>
        [SerializeField] [HideInInspector] public ConfigurableJoint additionalPin;

        /// <summary>
        /// Target Transform of the additional pin.
        /// </summary>
        [SerializeField] [HideInInspector] public Transform additionalPinTarget;

        /// <summary>
        /// Weight of the additional pin (multiplies this muscle's pin weight).
        /// </summary>
        [SerializeField] [HideInInspector] public float additionalPinWeight;

        [HideInInspector] public Vector3 mappedVelocity;
        [HideInInspector] public Vector3 mappedAngularVelocity;
        [HideInInspector] public bool isPropMuscle;

        [SerializeField] [HideInInspector] public int index = -1;

        /// <summary>
        /// Gets the default sampled rotation offset of the Muscle from it's target. If the muscle's rotation matches with it's target's in the Editor (while not playing) this will return Quaternion.identity.
        /// </summary>
        public Quaternion targetRotationRelative { get; private set; }

        //[HideInInspector] public Vector3 offset;

        #endregion Main Properties

        // Returns true if we have enough to work with
        public bool IsValid(bool log)
        {
            if (joint == null)
            {
                if (log) Debug.LogError("Muscle joint is null");
                return false;
            }

            if (target == null)
            {
                if (log) Debug.LogError("Muscle " + joint.name + " target is null, please remove the muscle from PuppetMaster or disable PuppetMaster before destroying a muscle's target.");
                return false;
            }

            if (props == null)
            {
                if (log) Debug.LogError("Muscle " + joint.name + " props is null");
            }

            return true;
        }

        public Rigidbody rebuildConnectedBody { get; private set; }
        public Transform rebuildTargetParent { get; private set; }
        public Vector3 defaultTargetPosRelToMuscle { get; private set; }
        public Quaternion defaultTargetRotRelToMuscle { get; private set; }
        public Quaternion defaultMuscleRotRelToTarget { get; private set; }
        private Transform rebuildParent;

        private Vector3 rebuildPosition;
        private Quaternion rebuildRotation = Quaternion.identity;
        private Vector3 rebuildTargetPosition;
        private Quaternion rebuildTargetRotation = Quaternion.identity;
        private ConfigurableJointMotion rebuildAngularXMotion;
        private ConfigurableJointMotion rebuildAngularYMotion;
        private ConfigurableJointMotion rebuildAngularZMotion;

        public void Rebuild()
        {
            joint.transform.parent = rebuildParent;
            target.parent = rebuildTargetParent;
            joint.transform.position = rebuildPosition;
            joint.transform.rotation = rebuildRotation;
            target.position = rebuildTargetPosition;
            target.rotation = rebuildTargetRotation;
            joint.angularXMotion = rebuildAngularXMotion;
            joint.angularYMotion = rebuildAngularYMotion;
            joint.angularZMotion = rebuildAngularZMotion;

            state = State.Default;
        }

        // Initiate this Muscle
        public virtual void Initiate(Muscle[] colleagues)
        {
            initiated = false;
            if (!IsValid(true)) return;

            name = joint.name;

            state = State.Default;

            if (joint.connectedBody != null)
            {
                for (int i = 0; i < colleagues.Length; i++)
                {
                    if (colleagues[i].joint.GetComponent<Rigidbody>() == joint.connectedBody)
                    {
                        connectedBodyTarget = colleagues[i].target;
                    }
                }
            }

            transform = joint.transform;

            rigidbody = transform.GetComponent<Rigidbody>();

            InitiateAdditionalPin();

            SetKinematic(false);

            UpdateColliders();
            if (_colliders.Length == 0)
            {
                Vector3 size = Vector3.one * 0.1f;
                var renderer = transform.GetComponent<Renderer>();
                if (renderer != null) size = renderer.bounds.size;

                rigidbody.inertiaTensor = PhysXTools.CalculateInertiaTensorCuboid(size, rigidbody.mass);
            }

            targetParent = connectedBodyTarget != null ? connectedBodyTarget : target.parent;

            rebuildConnectedBody = joint.connectedBody;
            rebuildTargetParent = target.parent;
            rebuildParent = joint.transform.parent;
            rebuildPosition = joint.transform.position;
            rebuildRotation = joint.transform.rotation;
            rebuildTargetPosition = target.position;
            rebuildTargetRotation = target.rotation;
            rebuildAngularXMotion = joint.angularXMotion;
            rebuildAngularYMotion = joint.angularYMotion;
            rebuildAngularZMotion = joint.angularZMotion;

            defaultLocalRotation = localRotation;

            // Joint space
            Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            Vector3 up = Vector3.Cross(forward, joint.axis).normalized;

            if (forward == up)
            {
                Debug.LogError("Joint " + joint.name + " secondaryAxis is in the exact same direction as it's axis. Please make sure they are not aligned.");
                return;
            }

            rotationRelativeToTarget = Quaternion.Inverse(target.rotation) * transform.rotation;

            defaultTargetPosRelToMuscle = transform.InverseTransformPoint(target.position);
            defaultTargetRotRelToMuscle = Quaternion.Inverse(transform.rotation) * target.rotation;
            defaultTargetRotRelToMuscleInverse = Quaternion.Inverse(defaultTargetRotRelToMuscle);
            defaultMuscleRotRelToTarget = QuaTools.FromToRotation(target.rotation, transform.rotation);
            
            Quaternion toJointSpace = Quaternion.LookRotation(forward, up);
            toJointSpaceInverse = Quaternion.Inverse(toJointSpace);
            toJointSpaceDefault = defaultLocalRotation * toJointSpace;

            toParentSpace = Quaternion.Inverse(targetParentRotation) * parentRotation;

            localRotationConvert = Quaternion.Inverse(targetLocalRotation) * localRotation;

            // Anchoring
            if (joint.connectedBody != null)
            {
                joint.autoConfigureConnectedAnchor = false;
                connectedBodyTransform = joint.connectedBody.transform;

                directTargetParent = target.parent == connectedBodyTarget;
            }

            // Default angular motions and limits
            angularXMotionDefault = joint.angularXMotion;
            angularYMotionDefault = joint.angularYMotion;
            angularZMotionDefault = joint.angularZMotion;

            // Mapping
            if (joint.connectedBody == null) props.mapPosition = true;
            targetRotationRelative = Quaternion.Inverse(rigidbody.transform.rotation) * target.rotation;

            // Resetting
            if (joint.connectedBody == null)
            {
                defaultPosition = transform.localPosition;
                defaultRotation = transform.localRotation;
            }
            else
            {
                defaultPosition = joint.connectedBody.transform.InverseTransformPoint(transform.position);
                defaultRotation = Quaternion.Inverse(joint.connectedBody.transform.rotation) * transform.rotation;
            }

            // Fix target Transforms
            defaultTargetLocalPosition = target.localPosition;
            defaultTargetLocalRotation = target.localRotation;

            // Set necessary joint params
            joint.rotationDriveMode = RotationDriveMode.Slerp;

            if (!joint.gameObject.activeInHierarchy)
            {
                Debug.LogError("Can not initiate a puppet that has deactivated muscles.", joint.transform);
                return;
            }

            joint.configuredInWorldSpace = false;
#if UNITY_5_2
			slerpDrive.mode = JointDriveMode.PositionAndVelocity;
#endif
            joint.projectionMode = JointProjectionMode.None; //Other projection modes will cause sliding

            if (joint.anchor != Vector3.zero)
            {
                Debug.LogError("PuppetMaster joint anchors need to be Vector3.zero. Joint axis on " + transform.name + " is " + joint.anchor, transform);
                return;
            }

            //rigidbody.maxDepenetrationVelocity = 1f;
            targetAnimatedPosition = target.position;
            targetAnimatedCenterOfMass = rigidbody.worldCenterOfMass;
            targetAnimatedWorldRotation = target.rotation;
            targetAnimatedRotation = targetLocalRotation * localRotationConvert;
            
            Read();
            lastReadTime = Time.time;
            lastWriteTime = Time.time;
            lastMappedPosition = target.position;
            lastMappedRotation = target.rotation;

            targetChildren = new TargetChild[props.animatedTargetChildren.Length];
            for (int i = 0; i < targetChildren.Length; i++)
            {
                targetChildren[i] = new TargetChild(props.animatedTargetChildren[i]);
            }

            initiated = true;
        }

        public void InitiateAdditionalPin()
        {
            if (additionalPin != null)
            {
                additionalRigidbody = additionalPin.GetComponent<Rigidbody>();
                additionalPinTargetAnimatedCenterOfMass = additionalRigidbody.worldCenterOfMass;
                //additionalTargetAnimatedPosition = additionalPinTarget.position;
                additionalRigidbody.inertiaTensor = PhysXTools.CalculateInertiaTensorCuboid(Vector3.one * 0.1f, additionalRigidbody.mass);
            }
        }

        // Regather (compound) colliders associated with this muscle.
        public void UpdateColliders()
        {
            _colliders = new Collider[0];

            AddColliders(joint.transform, ref _colliders, true);

            int childCount = joint.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                AddCompoundColliders(joint.transform.GetChild(i), ref _colliders);
            }

            disabledColliders = new bool[_colliders.Length];
        }

        // Disables all colliders of this muscle.
        public void DisableColliders()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                // Do nothing if already disabled colliders. Without it EnableColliders won't work if DisableColliders called twice.
                if (disabledColliders[i]) return;
            }

            for (int i = 0; i < _colliders.Length; i++)
            {
                disabledColliders[i] = _colliders[i].enabled;
                _colliders[i].enabled = false;
            }
        }

        // Enables all colliders of this muscle.
        public void EnableColliders()
        {
            for (int i = 0; i < _colliders.Length; i++)
            {
                if (disabledColliders[i]) _colliders[i].enabled = true;
                disabledColliders[i] = false;
            }
        }

        // Add all non-trigger colliders on a Transform to an array of colliders
        private void AddColliders(Transform t, ref Collider[] C, bool includeMeshColliders)
        {
            var colliders = t.GetComponents<Collider>();
            int cCount = 0;
            foreach (Collider c in colliders)
            {
                bool isMeshCollider = c is MeshCollider;
                if (!c.isTrigger && (!includeMeshColliders || !isMeshCollider)) cCount++;
            }

            if (cCount == 0) return;

            int l = C.Length;
            Array.Resize(ref C, l + cCount);
            int addC = 0;

            for (int i = 0; i < colliders.Length; i++)
            {
                bool isMeshCollider = colliders[i] is MeshCollider;
                if (!colliders[i].isTrigger && (!includeMeshColliders || !isMeshCollider))
                {
                    C[l + addC] = colliders[i];
                    addC++;
                }
            }
        }

        // Recursively goes through all children of a Transform to find the compound colliders until stopped by a Rigidbody
        private void AddCompoundColliders(Transform t, ref Collider[] colliders)
        {
            if (t.GetComponent<Rigidbody>() != null) return;

            AddColliders(t, ref colliders, false);

            int childCount = t.childCount;
            for (int i = 0; i < childCount; i++)
            {
                AddCompoundColliders(t.GetChild(i), ref colliders);
            }
        }

        // Ignores or collisions with all the colliders of this and another Muscle
        public void IgnoreInternalCollisions(Muscle m)
        {
            if (m == this) return;
            if (state.isDisconnected || m.state.isDisconnected) return;

            foreach (Collider c in colliders)
            {
                foreach (Collider c2 in m.colliders)
                {
                    if (c != null && c2 != null && c.enabled && c2.enabled && c.gameObject.activeInHierarchy && c2.gameObject.activeInHierarchy)
                    {
                        Physics.IgnoreCollision(c, c2);
                    }
                }
            }
        }

        // Unignores collisions with all the colliders of this and another muscle (unless forced to ignore by props.internalCollisionIgnores)
        public void ResetInternalCollisions(Muscle m, bool useInternalCollisionIgnores)
        {
            if (m == this) return;
            bool forceIgnore = useInternalCollisionIgnores && ForceIgnore(m);

            foreach (Collider c in colliders)
            {
                foreach (Collider c2 in m.colliders)
                {
                    if (c != null && c2 != null && c.enabled && c2.enabled && c.gameObject.activeInHierarchy && c2.gameObject.activeInHierarchy)
                    {

                        if (!forceIgnore)
                        {
                            Physics.IgnoreCollision(c, c2, false);
                        }
                        else
                        {
                            Physics.IgnoreCollision(c, c2, true);
                        }
                    }
                }
            }
        }

        // Are internal collision between these two muscles forced to ignore?
        private bool ForceIgnore(Muscle otherMuscle)
        {
            if (state.isDisconnected) return false;
            if (otherMuscle.state.isDisconnected) return false;

            if (props.internalCollisionIgnores.ignoreAll || otherMuscle.props.internalCollisionIgnores.ignoreAll) return true;

            foreach (ConfigurableJoint j in props.internalCollisionIgnores.muscles)
            {
                if (j == otherMuscle.joint) return true;
            }

            foreach (Muscle.Group group in props.internalCollisionIgnores.groups)
            {
                if (group == otherMuscle.props.group) return true;
            }

            foreach (ConfigurableJoint j in otherMuscle.props.internalCollisionIgnores.muscles)
            {
                if (j == joint) return true;
            }

            foreach (Muscle.Group group in otherMuscle.props.internalCollisionIgnores.groups)
            {
                if (group == props.group) return true;
            }

            return false;
        }

        // Set joint angular motions to either free or to their default values
        public void IgnoreAngularLimits(bool ignore)
        {
            if (!initiated) return;
            // Do not use state.isDisconnected here, or severed muscles will not be freed when exploded later

            joint.angularXMotion = ignore ? ConfigurableJointMotion.Free : angularXMotionDefault;
            joint.angularYMotion = ignore ? ConfigurableJointMotion.Free : angularYMotionDefault;
            joint.angularZMotion = ignore ? ConfigurableJointMotion.Free : angularZMotionDefault;
        }

        // Reset target to its default localPosition and localRotation to protect from unanimated bone drifting
        public void FixTargetTransforms()
        {
            if (!initiated) return;

            target.localPosition = defaultTargetLocalPosition;
            target.localRotation = defaultTargetLocalRotation;
        }

        // Reset the Transform to the default state. This is necessary for activating/deactivating the ragdoll without messing it up
        public void Reset()
        {
            if (!initiated) return;
            if (joint == null) return;
            if (state.isDisconnected) return;
      
            if (joint.connectedBody == null)
            {
                transform.localPosition = defaultPosition;
                transform.localRotation = defaultRotation;
            }
            else
            {
                transform.position = joint.connectedBody.transform.TransformPoint(defaultPosition);
                transform.rotation = joint.connectedBody.transform.rotation * defaultRotation;
            }

            lastRotationDamper = -1f;
        }

        // Moves and rotates the muscle to match it's target
        public void MoveToTarget()
        {
            if (!initiated) return;
            if (state.isDisconnected) return;
            //if (!IsValid(true)) return;

            // Moving rigidbodies only won't animate the pose. MoveRotation does not work on a kinematic Rigidbody that is connected to another by a Joint
            transform.position = target.position;
            transform.rotation = target.rotation * rotationRelativeToTarget;
            rigidbody.MovePosition(transform.position);
            rigidbody.MoveRotation(transform.rotation);

            positionOffset = Vector3.zero;

            if (additionalPin != null)
            {
                additionalPin.transform.position = additionalPinTarget.position;
            }
        }

        // Toggles muscle Rigidbody.isKinematic
        public void SetKinematic(bool to)
        {
            if (to) rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            rigidbody.isKinematic = to;
            if (additionalPin != null) additionalRigidbody.isKinematic = to;
        }

        

        // Read the target
        public void Read()
        {
            if (state.isDisconnected) return;

            float readDeltaTime = Time.time - lastReadTime;
            lastReadTime = Time.time;

            Vector3 tAM = V3Tools.TransformPointUnscaled(target, defaultTargetRotRelToMuscleInverse * rigidbody.centerOfMass); // Center of mass is unscaled, so can't use Transform.TransformPoint() here

            if (readDeltaTime > 0f)
            {
                targetVelocity = (tAM - targetAnimatedCenterOfMass) / readDeltaTime;
                //targetAngularVelocity = QuaTools.FromToRotation(targetAnimatedWorldRotation, target.rotation).eulerAngles;
                //targetAngularVelocity = QuaTools.ToBiPolar(targetAngularVelocity) / readDeltaTime;
            }

            //if (props.mapPosition) targetLocalPosition = target.localPosition;

            targetAnimatedCenterOfMass = tAM;
            targetAnimatedPosition = target.position;
            targetAnimatedWorldRotation = target.rotation;

            if (additionalPin != null)
            {
                Vector3 aTAM =V3Tools.TransformPointUnscaled(additionalPinTarget, additionalRigidbody.centerOfMass); // Center of mass is unscaled, so can't use Transform.TransformPoint() here

                if (readDeltaTime > 0f)
                {
                    additionalTargetVelocity = (aTAM - additionalPinTargetAnimatedCenterOfMass) / readDeltaTime;
                    //additionalTargetVelocity = (additionalPinTarget.position - additionalTargetAnimatedPosition) / readDeltaTime;
                }
                additionalPinTargetAnimatedCenterOfMass = aTAM;
                //additionalTargetAnimatedPosition = additionalPinTarget.position;
            }

            if (joint.connectedBody != null)
            {
                targetAnimatedRotation = targetLocalRotation * localRotationConvert;
            }
        }

        public void ClearVelocities()
        {
            targetVelocity = Vector3.zero;
            //targetAngularVelocity = Vector3.zero;
            mappedVelocity = Vector3.zero;
            mappedAngularVelocity = Vector3.zero;
            additionalTargetVelocity = Vector3.zero;

            targetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
            targetAnimatedPosition = target.position;
            targetAnimatedWorldRotation = target.rotation;
            lastMappedPosition = target.position;
            lastMappedRotation = target.rotation;
            if (additionalPin != null)
            {
                additionalPinTargetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(additionalPinTarget, additionalRigidbody.centerOfMass);
                //additionalTargetAnimatedPosition = additionalPinTarget.position;
            }
        }

        // Update Joint connected anchor
        public void UpdateAnchor(bool supportTranslationAnimation)
        {
            //if (state.isDisconnected) return;
            if (joint.connectedBody == null || connectedBodyTarget == null) return;
            if (directTargetParent && !supportTranslationAnimation) return;
            if (state.isDisconnected) return;

            //if (props.mapPosition) target.localPosition = targetLocalPosition;

            Vector3 anchorUnscaled = joint.connectedAnchor = InverseTransformPointUnscaled(connectedBodyTarget.position, connectedBodyTarget.rotation * toParentSpace, target.position);
            float uniformScaleF = 1f / connectedBodyTransform.lossyScale.x;

            joint.connectedAnchor = anchorUnscaled * uniformScaleF;
        }

        // Update this Muscle
        public virtual void Update(float pinWeightMaster, float muscleWeightMaster, float muscleSpring, float muscleDamper, float pinPow, float pinDistanceFalloff, bool rotationTargetChanged, bool angularPinning, float deltaTime)
        {
            state.velocity = rigidbody.velocity;
            state.angularVelocity = rigidbody.angularVelocity;

            if (state.isDisconnected)
            {
                state.pinWeightMlp = 0f;
                state.muscleWeightMlp = 0f;
                state.muscleDamperAdd = 0f;
                state.muscleDamperMlp = 0f;
                state.mappingWeightMlp = 0f;
                state.maxForceMlp = 0f;
                state.immunity = 0f;
                state.impulseMlp = 1f;
            }

            props.Clamp();
            state.Clamp();

            Pin(pinWeightMaster, pinPow, pinDistanceFalloff, angularPinning, deltaTime);
            if (rotationTargetChanged) MuscleRotation(muscleWeightMaster, muscleSpring, muscleDamper);

            //offset = Vector3.Lerp(offset, Vector3.zero, Time.deltaTime * 5f);
        }

        [HideInInspector] public Vector3 targetMappedPosition;
        [HideInInspector] public Quaternion targetMappedRotation = Quaternion.identity;
        [HideInInspector] public Vector3 targetSampledPosition;
        [HideInInspector] public Quaternion targetSampledRotation = Quaternion.identity;

        public void StoreTargetMappedPosition()
        {
            targetMappedPosition = target.position;
        }

        public void StoreTargetMappedRotation()
        {
            targetMappedRotation = target.rotation;
        }

        // Map the target bone to this Rigidbody
        public void Map(float mappingWeightMaster)
        {
            float w = props.mappingWeight * mappingWeightMaster * state.mappingWeightMlp;
            if (w <= 0f) return;

            // rigidbody.position does not work with interpolation
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            if (w >= 1f)
            {
                target.rotation = rotation * targetRotationRelative;

                if (props.mapPosition)
                {
                    if (connectedBodyTransform != null)
                    {
                        // Mapping in local space of the parent
                        Vector3 relativePosition = connectedBodyTransform.InverseTransformPoint(position);
                        target.position = connectedBodyTarget.TransformPoint(relativePosition);
                    }
                    else
                    {
                        target.position = position;
                    }
                }

                return;
            }

            target.rotation = Quaternion.Lerp(target.rotation, rotation * targetRotationRelative, w);

            if (props.mapPosition)
            {
                if (connectedBodyTransform != null)
                {
                    // Mapping in local space of the parent
                    Vector3 relativePosition = connectedBodyTransform.InverseTransformPoint(position);
                    target.position = Vector3.Lerp(target.position, connectedBodyTarget.TransformPoint(relativePosition), w);
                }
                else
                {
                    target.position = Vector3.Lerp(target.position, position, w);
                }
            }
        }

        // Moves targetl localPosition back to default (used for reconnecting limb in Dead state when animation is not overriding target localPosition)
        public void ResetTargetLocalPosition()
        {
            target.localPosition = defaultTargetLocalPosition;
        }

        // How fast the mapped target is moving? Will be used to set rigidbody velocities when puppet is killed. 
        // Rigidbody velocities otherwise might be close to 0 when FixedUpdate called more than once per frame or velocity wrongully changing when mapping weights not 1.
        public void CalculateMappedVelocity()
        {
            float writeDeltaTime = Time.time - lastWriteTime;

            if (writeDeltaTime > 0f)
            {
                mappedVelocity = (target.position - lastMappedPosition) / writeDeltaTime;

                mappedAngularVelocity = QuaTools.FromToRotation(lastMappedRotation, target.rotation).eulerAngles;
                mappedAngularVelocity = QuaTools.ToBiPolar(mappedAngularVelocity) / writeDeltaTime;

                lastWriteTime = Time.time;
            }

            lastMappedPosition = target.position;
            lastMappedRotation = target.rotation;
        }

        // Move the target to the muscle if the muscle is disconnected
        public void MapDisconnected()
        {
            if (!state.isDisconnected) return;
            if (isPropMuscle) return;
            //if (!transform.gameObject.activeInHierarchy) return;

            target.position = transform.TransformPoint(defaultTargetPosRelToMuscle); // TODO Make private
            target.rotation = transform.rotation * defaultTargetRotRelToMuscle;

            foreach (TargetChild c in targetChildren) c.Map();
        }

        // Used for blocking animation on child bones of disconnected muscles
        public class TargetChild
        {
            public Transform t;
            public Vector3 defaultLocalPosition;
            public Quaternion defaultLocalRotation = Quaternion.identity;

            public TargetChild(Transform t)
            {
                this.t = t;

                defaultLocalPosition = t.localPosition;
                defaultLocalRotation = t.localRotation;
            }

            public void Map()
            {
                t.localPosition = defaultLocalPosition;
                t.localRotation = defaultLocalRotation;
            }
        }

        private JointDrive slerpDrive = new JointDrive();
        private float lastJointDriveRotationWeight = -1f, lastRotationDamper = -1f;
        private Vector3 defaultPosition, defaultTargetLocalPosition, lastMappedPosition;
        private Quaternion defaultLocalRotation, localRotationConvert, toParentSpace, toJointSpaceInverse, toJointSpaceDefault,
        targetAnimatedRotation, defaultRotation, rotationRelativeToTarget, defaultTargetLocalRotation, lastMappedRotation;
        private Transform targetParent, connectedBodyTransform;
        private ConfigurableJointMotion angularXMotionDefault, angularYMotionDefault, angularZMotionDefault;
        private bool directTargetParent;
        private bool initiated;
        private Collider[] _colliders = new Collider[0];
        private float lastReadTime, lastWriteTime;
        private bool[] disabledColliders = new bool[0];
        private TargetChild[] targetChildren = new TargetChild[0];
        //private Vector3 targetLocalPosition;
        //private Vector3 additionalTargetAnimatedPosition;
        private Vector3 additionalTargetVelocity;
        private Vector3 targetAnimatedCenterOfMass;
        private Vector3 additionalPinTargetAnimatedCenterOfMass;
        private Quaternion defaultTargetRotRelToMuscleInverse = Quaternion.identity;
        
        // Add force to the rigidbody to make it match the target position
        private void Pin(float pinWeightMaster, float pinPow, float pinDistanceFalloff, bool angularPinning, float deltaTime)
        {
            positionOffset = targetAnimatedCenterOfMass - rigidbody.worldCenterOfMass;
            if (float.IsNaN(positionOffset.x)) positionOffset = Vector3.zero;

            float w = pinWeightMaster * props.pinWeight * state.pinWeightMlp;

            if (w <= 0f) return;
            w = Mathf.Pow(w, pinPow);

            Pin(rigidbody, positionOffset, targetVelocity, w, pinDistanceFalloff, deltaTime);

            if (angularPinning)
            {
                Vector3 torque = PhysXTools.GetAngularAcceleration(rigidbody.rotation, defaultMuscleRotRelToTarget * targetAnimatedWorldRotation);
                
                torque -= rigidbody.angularVelocity;
                torque *= w;
                rigidbody.AddTorque(torque, ForceMode.VelocityChange);
            }

            if (additionalPin != null)
            {
                Vector3 additionalPositionOffset = Vector3.zero;
                additionalPositionOffset = additionalPinTargetAnimatedCenterOfMass - additionalRigidbody.worldCenterOfMass;

                if (float.IsNaN(additionalPositionOffset.x)) additionalPositionOffset = Vector3.zero;

                Pin(additionalRigidbody, additionalPositionOffset, additionalTargetVelocity, w * additionalPinWeight, pinDistanceFalloff, deltaTime);
            }
        }

        private void Pin(Rigidbody r, Vector3 posOffset, Vector3 targetVel, float w, float pinDistanceFalloff, float deltaTime)
        {
            Vector3 p = posOffset;
            if (deltaTime > 0f) p /= deltaTime;

            Vector3 force = -r.velocity + targetVel * deltaTime + p;
            force *= w;
            if (pinDistanceFalloff > 0f) force /= 1f + posOffset.sqrMagnitude * pinDistanceFalloff;

            r.AddForce(force, ForceMode.VelocityChange);
        }

        // Apply Joint targetRotation to match the target rotation
        private void MuscleRotation(float muscleWeightMaster, float muscleSpring, float muscleDamper)
        {
            float w = muscleWeightMaster * props.muscleWeight * muscleSpring * state.muscleWeightMlp * 10f;

            // If no connection point, don't rotate;
            if (joint.connectedBody == null) w = 0f;
            else if (w > 0f) joint.targetRotation = LocalToJointSpace(targetAnimatedRotation);

            float d = (props.muscleDamper * muscleDamper * state.muscleDamperMlp) + state.muscleDamperAdd;

            if (w == lastJointDriveRotationWeight && d == lastRotationDamper) return;
            lastJointDriveRotationWeight = w;

            lastRotationDamper = d;
            slerpDrive.positionSpring = w;
            slerpDrive.maximumForce = Mathf.Max(w, d) * state.maxForceMlp;
            slerpDrive.positionDamper = d;

            joint.slerpDrive = slerpDrive;
        }

        // Apply Joint targetRotation to match the target rotation
        public void SetMuscleRotation(float muscleWeightMaster, float muscleSpring, float muscleDamper)
        {
            float w = muscleWeightMaster * props.muscleWeight * muscleSpring * 10f;

            // If no connection point, don't rotate;
            if (joint.connectedBody == null) w = 0f;
            else if (w > 0f) joint.targetRotation = LocalToJointSpace(targetAnimatedRotation);

            float d = (props.muscleDamper * muscleDamper);

            slerpDrive.positionSpring = w;
            slerpDrive.maximumForce = Mathf.Max(w, d);
            slerpDrive.positionDamper = d;

            joint.slerpDrive = slerpDrive;
        }


        private Quaternion localRotation
        {
            get
            {
                return Quaternion.Inverse(parentRotation) * transform.rotation;
            }
        }

        private Quaternion parentRotation
        {
            get
            {
                if (joint.connectedBody != null) return joint.connectedBody.rotation;
                if (transform.parent == null) return Quaternion.identity;
                return transform.parent.rotation;
            }
        }

        private Quaternion targetParentRotation
        {
            get
            {
                if (targetParent == null) return Quaternion.identity;
                return targetParent.rotation;
            }
        }

        // Get the rotation of the target
        private Quaternion targetLocalRotation
        {
            get
            {
                return Quaternion.Inverse(targetParentRotation * toParentSpace) * target.rotation;
            }
        }

        // Convert a local rotation to local joint space rotation
        private Quaternion LocalToJointSpace(Quaternion localRotation)
        {
            return toJointSpaceInverse * Quaternion.Inverse(localRotation) * toJointSpaceDefault;
        }

        // Inversetransforms a point by the specified position and rotation
        private static Vector3 InverseTransformPointUnscaled(Vector3 position, Quaternion rotation, Vector3 point)
        {
            return Quaternion.Inverse(rotation) * (point - position);
        }
    }
}
