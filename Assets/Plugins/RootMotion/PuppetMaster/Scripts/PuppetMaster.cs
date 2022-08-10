using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using RootMotion;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// The master of puppets. Enables character animation to be played physically in muscle space.
    /// </summary>
    [HelpURL("https://www.youtube.com/watch?v=LYusqeqHAUc")]
    [AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Puppet Master")]
    public partial class PuppetMaster : MonoBehaviour
    {

        // Open the User Manual URL
        [ContextMenu("User Manual (Setup)")]
        void OpenUserManualSetup()
        {
            Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page4.html");
        }

        // Open the User Manual URL
        [ContextMenu("User Manual (Component)")]
        void OpenUserManualComponent()
        {
            Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page5.html");
        }

        [ContextMenu("User Manual (Performance)")]
        void OpenUserManualPerformance()
        {
            Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page8.html");
        }

        // Open the Script Reference URL
        [ContextMenu("Scrpt Reference")]
        void OpenScriptReference()
        {
            Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_puppet_master.html");
        }

        // Open a video tutorial about setting up the component
        [ContextMenu("TUTORIAL VIDEO (SETUP)")]
        void OpenSetupTutorial()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=mIN9bxJgfOU&index=2&list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL");
        }

        // Open a video tutorial about setting up the component
        [ContextMenu("TUTORIAL VIDEO (COMPONENT)")]
        void OpenComponentTutorial()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=LYusqeqHAUc");
        }

        /// <summary>
        /// Active mode means all muscles are active and the character is physically simulated. Kinematic mode sets rigidbody.isKinematic to true for all the muscles and simply updates their position/rotation to match the target's. Disabled mode disables the ragdoll. Switching modes is done by simply changing this value, blending in/out will be handled automatically by the PuppetMaster.
        /// </summary>
        [System.Serializable]
        public enum Mode
        {
            Active,
            Kinematic,
            Disabled
        }

        [Tooltip("Humanoid Config allows you to easily share PuppetMaster properties, including individual muscle props between Humanoid puppets.")]
        /// <summary>
        /// Humanoid Config allows you to easily share PuppetMaster properties, including individual muscle props between Humanoid puppets.
        /// </summary>
        public PuppetMasterHumanoidConfig humanoidConfig;

        /// <summary>
        /// The root Transform of the animated target character.
        /// </summary>
        public Transform targetRoot;// { get; private set; }

        [LargeHeader("Simulation")]

        [Tooltip("Sets/sets the state of the puppet (Alive, Dead or Frozen). Frozen means the ragdoll will be deactivated once it comes to stop in dead state.")]
        /// <summary>
        /// Sets/sets the state of the puppet (Alive, Dead or Frozen) Frozen means the ragdoll will be deactivated once it comes to stop in dead state..
        /// </summary>
        public State state;

        [ContextMenuItem("Reset To Default", "ResetStateSettings")]
        [Tooltip("Settings for killing and freezing the puppet.")]
        /// <summary>
        /// Settings for killing and freezing the puppet..
        /// </summary>
        public StateSettings stateSettings = StateSettings.Default;

        void ResetStateSettings()
        {
            stateSettings = StateSettings.Default;
        }

        [Tooltip("Active mode means all muscles are active and the character is physically simulated. Kinematic mode sets rigidbody.isKinematic to true for all the muscles and simply updates their position/rotation to match the target's. Disabled mode disables the ragdoll. Switching modes is done by simply changing this value, blending in/out will be handled automatically by the PuppetMaster.")]
        /// <summary>
        /// Active mode means all muscles are active and the character is physically simulated. Kinematic mode sets rigidbody.isKinematic to true for all the muscles and simply updates their position/rotation to match the target's. Disabled mode disables the ragdoll. Switching modes is done by simply changing this value, blending in/out will be handled automatically by the PuppetMaster.
        /// </summary>
        public Mode mode;

        [Tooltip("The time of blending when switching from Active to Kinematic/Disabled or from Kinematic/Disabled to Active. Switching from Kinematic to Disabled or vice versa will be done instantly.")]
        /// <summary>
        /// The time of blending when switching from Active to Kinematic/Disabled or from Kinematic/Disabled to Active. Switching from Kinematic to Disabled or vice versa will be done instantly.
        /// </summary>
        public float blendTime = 0.1f;

        [Tooltip("If true, will fix the target character's Transforms to their default local positions and rotations in each update cycle to avoid drifting from additive reading-writing. Use this only if the target contains unanimated bones.")]
        /// <summary>
        /// If true, will fix the target character's Transforms to their default local positions and rotations in each update cycle to avoid drifting from additive reading-writing. Use this only if the target contains unanimated bones.
        /// </summary>
        public bool fixTargetTransforms = true;

        [Tooltip("Rigidbody.solverIterationCount for the muscles of this Puppet.")]
        /// <summary>
        /// Rigidbody.solverIterationCount for the muscles of this Puppet.
        /// </summary>
        public int solverIterationCount = 6;

        [Tooltip("If true, will draw the target's pose as green lines in the Scene view. This runs in the Editor only. If you wish to profile PuppetMaster, switch this off.")]
        /// <summary>
        /// If true, will draw the target's pose as green lines in the Scene view. This runs in the Editor only. If you wish to profile PuppetMaster, switch this off.
        /// </summary>
        public bool visualizeTargetPose = true;

        [LargeHeader("Master Weights")]

        [Tooltip("The weight of mapping the animated character to the ragdoll pose.")]
        /// <summary>
        /// The weight of mapping the animated character to the ragdoll pose.
        /// </summary>
        [Range(0f, 1f)] public float mappingWeight = 1f;

        [Tooltip("The weight of pinning the muscles to the position of their animated targets using simple AddForce.")]
        /// <summary>
        /// The weight of pinning the muscles to the position of their animated targets using simple AddForce.
        /// </summary>
        [Range(0f, 1f)] public float pinWeight = 1f;

        [Tooltip("The normalized strength of the muscles.")]
        /// <summary>
        /// The normalized strength of the muscles. Useful for blending muscle strength in/out when you have multiple puppets with various Muscle Spring values.
        /// </summary>
        [Range(0f, 1f)] public float muscleWeight = 1f;

        [LargeHeader("Joint and Muscle Settings")]

        [Tooltip("The positionSpring of the ConfigurableJoints' Slerp Drive.")]
        /// <summary>
        /// The general strength of the muscles. PositionSpring of the ConfigurableJoints' Slerp Drive.
        /// </summary>
        public float muscleSpring = 100f;

        [Tooltip("The positionDamper of the ConfigurableJoints' Slerp Drive.")]
        /// <summary>
        /// The positionDamper of the ConfigurableJoints' Slerp Drive.
        /// </summary>
        public float muscleDamper = 0f;

        [Tooltip("Adjusts the slope of the pinWeight curve. Has effect only while interpolating pinWeight from 0 to 1 and back.")]
        /// <summary>
        /// Adjusts the slope of the pinWeight curve. Has effect only while interpolating pinWeight from 0 to 1 and back.
        /// </summary>
        [Range(1f, 8f)] public float pinPow = 4f;

        [Tooltip("Reduces pinning force the farther away the target is. Bigger value loosens the pinning, resulting in sloppier behaviour.")]
        /// <summary>
        /// Reduces pinning force the farther away the target is. Bigger value loosens the pinning, resulting in sloppier behaviour.
        /// </summary>
        [Range(0f, 100f)] public float pinDistanceFalloff = 5;

        [Tooltip("If disabled, only world space AddForce will be used to pin the ragdoll to the animation while 'Pin Weight' > 0. If enabled, AddTorque will also be used for rotational pinning. Keep it disabled if you don't see any noticeable improvement from it to avoid wasting CPU resources.")]
        /// <summary>
        /// If disabled, only world space AddForce will be used to pin the ragdoll to the animation while 'Pin Weight' > 0. If enabled, AddTorque will also be used for rotational pinning. Keep it disabled if you don't see any noticeable improvement from it to avoid wasting CPU resources.
        /// </summary>
        public bool angularPinning;

        [Tooltip("When the target has animated bones between the muscle bones, the joint anchors need to be updated in every update cycle because the muscles' targets move relative to each other in position space. This gives much more accurate results, but is computationally expensive so consider leaving it off.")]
        /// <summary>
        /// When the target has animated bones between the muscle bones, the joint anchors need to be updated in every update cycle because the muscles' targets move relative to each other in position space. This gives much more accurate results, but is computationally expensive so consider leaving it off.
        /// </summary>
        public bool updateJointAnchors = true;

        [Tooltip("Enable this if any of the target's bones has translation animation.")]
        /// <summary>
        /// Enable this if any of the target's bones has translation animation.
        /// </summary>
        public bool supportTranslationAnimation;

        [Tooltip("Should the joints use angular limits? If the PuppetMaster fails to match the target's pose, it might be because the joint limits are too stiff and do not allow for such motion. Uncheck this to see if the limits are clamping the range of your puppet's animation. Since the joints are actuated, most PuppetMaster simulations will not actually require using joint limits at all.")]
        /// <summary>
        /// Should the joints use angular limits? If the PuppetMaster fails to match the target's pose, it might be because the joint limits are too stiff and do not allow for such motion. Uncheck this to see if the limits are clamping the range of your puppet's animation. Since the joints are actuated, most PuppetMaster simulations will not actually require using joint limits at all.
        /// </summary>
        public bool angularLimits;

        [Tooltip("Should the muscles collide with each other? Consider leaving this off while the puppet is pinned for performance and better accuracy.  Since the joints are actuated, most PuppetMaster simulations will not actually require internal collisions at all.")]
        /// <summary>
        /// Should the muscles collide with each other? Consider leaving this off while the puppet is pinned for performance and better accuracy.  Since the joints are actuated, most PuppetMaster simulations will not actually require internal collisions at all.
        /// </summary>
        public bool internalCollisions;

        [LargeHeader("Individual Muscle Settings")]

        [Tooltip("The Muscles managed by this PuppetMaster.")]
        /// <summary>
        /// The Muscles managed by this PuppetMaster.
        /// </summary>
        public Muscle[] muscles = new Muscle[0];

        /// <summary>
        /// All PropMuscles added to this PuppetMaster.
        /// </summary>
        [SerializeField] [HideInInspector] public PropMuscle[] propMuscles = new PropMuscle[0];

        public delegate void UpdateDelegate();
        public delegate void MuscleDelegate(Muscle muscle);

        /// <summary>
        /// Called after the puppet has initiated.
        /// </summary>
        public UpdateDelegate OnPostInitiate;

        /// <summary>
        /// Called before (and only if) reading.
        /// </summary>
        public UpdateDelegate OnRead;

        /// <summary>
        /// Called after (and only if) writing
        /// </summary>
        public UpdateDelegate OnWrite;

        /// <summary>
        /// Called after each LateUpdate.
        /// </summary>
        public UpdateDelegate OnPostLateUpdate;

        /// <summary>
        /// Called when it's the right time to fix target transforms.
        /// </summary>
        public UpdateDelegate OnFixTransforms;

        /// <summary>
        /// Called when the puppet hierarchy has changed by adding/removing muscles
        /// </summary>
        public UpdateDelegate OnHierarchyChanged;

        /// <summary>
        /// Called when a muscle has been removed.
        /// </summary>
        public MuscleDelegate OnMuscleRemoved;

        /// <summary>
        /// Called when a muscle has been disconnected.
        /// </summary>
        public MuscleDelegate OnMuscleDisconnected;

        /// <summary>
        /// Called when muscles have been reconnected.
        /// </summary>
        public MuscleDelegate OnMuscleReconnected;

        /// <summary>
        /// Gets the Animator on the target.
        /// </summary>
        public Animator targetAnimator
        {
            get
            {
                // Protect from the Animator being replaced (UMA)
                if (_targetAnimator == null) _targetAnimator = targetRoot.GetComponentInChildren<Animator>();
                if (_targetAnimator == null && targetRoot.parent != null) _targetAnimator = targetRoot.parent.GetComponentInChildren<Animator>();
                return _targetAnimator;
            }
            set
            {
                _targetAnimator = value;
            }
        }
        private Animator _targetAnimator;

        /// <summary>
        /// Gets the Animation component on the target.
        /// </summary>
        public Animation targetAnimation { get; private set; }

        /// <summary>
        /// Array of all Puppet Behaviours
        /// </summary>
        /// <value>The behaviours.</value>
        public BehaviourBase[] behaviours { get; private set; } // @todo add/remove behaviours in runtime (add OnDestroy to BehaviourBase)

        /// <summary>
        /// Returns true if the PuppetMaster is in active mode or blending in/out of it.
        /// </summary>
        public bool isActive { get { return gameObject.activeInHierarchy && initiated && (activeMode == Mode.Active || isBlending); } }

        /// <summary>
        /// Has this PuppetMaster successfully initiated?
        /// </summary>
        public bool initiated { get; private set; }

        /// <summary>
        /// The list of solvers that will be updated by this PuppetMaster. When you add a Final-IK component in runtime after PuppetMaster has initiated, add it to this list using solver.Add(SolverManager solverManager).
        /// </summary>
        [HideInInspector] public List<SolverManager> solvers = new List<SolverManager>();

        /// <summary>
        /// If true, PuppetMaster will not handle internal collision ignores and you can have full control over handling it (call SetInternalCollisionsManual();).
        /// </summary>
        [HideInInspector] [NonSerialized] public bool manualInternalCollisionControl;
        /// <summary>
        /// If true, PuppetMaster will not handle angular limits and you can have full control over handling it (call SetAngularLimitsManual();).
        /// </summary>
        [HideInInspector] [NonSerialized] public bool manualAngularLimitControl;

        /// <summary>
        /// If disabled, disconnected bones will not be mapped to disconnected ragdoll parts.
        /// </summary>
        [SerializeField] [HideInInspector] public bool mapDisconnectedMuscles = true;

        /// <summary>
        /// Normal means Animator is in Normal or Unscaled Time or Animation has Animate Physics unchecked.
        /// AnimatePhysics is Legacy only, when the Animation component has Animate Physics checked.
        /// FixedUpdate means Animator is used and in Animate Physics mode. In this case PuppetMaster will take control of updating the Animator in FixedUpdate.
        /// </summary>
        [System.Serializable]
        public enum UpdateMode
        {
            Normal,
            AnimatePhysics,
            FixedUpdate
        }

        /// <summary>
        /// Gets the current update mode.
        /// </summary>
        /// <value>The update mode.</value>
        public UpdateMode updateMode
        {
            get
            {
                return targetUpdateMode == AnimatorUpdateMode.AnimatePhysics ? (isLegacy ? UpdateMode.AnimatePhysics : UpdateMode.FixedUpdate) : UpdateMode.Normal;
            }
        }

        /// <summary>
        /// If the Animator's update mode is "Animate Phyics", PuppetMaster will take control of updating the Animator (in FixedUpdate). This does not happen with Legacy.
        /// </summary>
        public bool controlsAnimator
        {
            get
            {
                return isActiveAndEnabled && isActive && initiated && updateMode == UpdateMode.FixedUpdate;
            }
        }

        /// <summary>
        /// Is the PuppetMaster currently switching state or mode?
        /// </summary>
        public bool isBlending
        {
            get
            {
                return isSwitchingMode || isSwitchingState;
            }
        }

        /// <summary>
        /// Teleports the puppet to the specified position and rotation. The operation will not be processed immediatelly, but the next time PuppetMaster reads.
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation, bool moveToTarget)
        {
            teleport = true;
            teleportPosition = position;
            teleportRotation = rotation;
            teleportMoveToTarget = moveToTarget;

            // If disabled, teleport immedietely.
            if (activeMode == Mode.Disabled) Read();
        }

        /// <summary>
        /// Used for manual override of internal collision ignores.
        /// </summary>
        public void SetInternalCollisionsManual(bool collide, bool useInternalCollisionIgnores)
        {
            for (int i = 0; i < muscles.Length; i++)
            {
                for (int i2 = i; i2 < muscles.Length; i2++)
                {
                    if (i != i2)
                    {
                        if (collide)
                        {
                            muscles[i].ResetInternalCollisions(muscles[i2], useInternalCollisionIgnores);
                        }
                        else
                        {
                            muscles[i].IgnoreInternalCollisions(muscles[i2]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Used for manual override of angular limit ignores.
        /// </summary>
        public void SetAngularLimitsManual(bool limited)
        {
            for (int i = 0; i < muscles.Length; i++)
            {
                if (!muscles[i].state.isDisconnected) muscles[i].IgnoreAngularLimits(!limited);
            }
        }


        #region Update Sequence

        private bool internalCollisionsEnabled = true;
        private bool angularLimitsEnabled = true;
        private bool fixedFrame;
        private int lastSolverIterationCount;
        private bool isLegacy;
        private bool animatorDisabled;
        private bool awakeFailed;
        private bool interpolated;
        private bool freezeFlag;
        private bool hasBeenDisabled;
        private bool hierarchyIsFlat;
        private bool teleport;
        private Vector3 teleportPosition;
        private Quaternion teleportRotation = Quaternion.identity;
        private bool teleportMoveToTarget;
        private bool rebuildFlag;
        private bool onPostRebuildFlag;
        private bool[] disconnectMuscleFlags = new bool[0];
        private MuscleDisconnectMode[] muscleDisconnectModes = new MuscleDisconnectMode[0];
        private bool[] disconnectDeactivateFlags = new bool[0];
        private bool[] reconnectMuscleFlags = new bool[0];

        private bool autoSimulate
        {
            get
            {
#if UNITY_2018_3_OR_NEWER
            return Physics.autoSimulation;
#else
                return true;
#endif
            }
        }

        // If PuppetMaster has been deactivated externally
        void OnDisable()
        {
            if (!gameObject.activeInHierarchy && initiated && Application.isPlaying)
            {
                foreach (Muscle m in muscles) m.Reset();
            }
            hasBeenDisabled = true;
        }

        // If reactivating a PuppetMaster that has been forcefully deactivated and state/mode switching interrupted
        void OnEnable()
        {
            if (gameObject.activeInHierarchy && initiated && hasBeenDisabled && Application.isPlaying)
            {
                // Reset mode
                isSwitchingMode = false;
                activeMode = mode;
                lastMode = mode;
                mappingBlend = mode == Mode.Active ? 1f : 0f;

                // Reset state
                activeState = state;
                lastState = state;
                isKilling = false;
                freezeFlag = false;

                // Animation
                SetAnimationEnabled(state == State.Alive);
                if (state == State.Alive && targetAnimator != null && targetAnimator.gameObject.activeInHierarchy)
                {
                    targetAnimator.Update(0.001f);
                }

                // Muscle weights
                foreach (Muscle m in muscles)
                {
                    m.state.pinWeightMlp = state == State.Alive ? 1f : 0f;
                    m.state.muscleWeightMlp = state == State.Alive ? 1f : stateSettings.deadMuscleWeight;
                    m.state.muscleDamperAdd = 0f;
                    //m.state.immunity = 0f;
                }

                // Ragdoll and behaviours
                if (state != State.Frozen && mode != Mode.Disabled)
                {
                    ActivateRagdoll(mode == Mode.Kinematic);

                    foreach (BehaviourBase behaviour in behaviours)
                    {
                        behaviour.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Deactivate/Freeze
                    foreach (Muscle m in muscles)
                    {
                        m.joint.gameObject.SetActive(false);
                    }

                    // Freeze
                    if (state == State.Frozen)
                    {
                        foreach (BehaviourBase behaviour in behaviours)
                        {
                            if (behaviour.gameObject.activeSelf)
                            {
                                behaviour.deactivated = true;
                                behaviour.gameObject.SetActive(false);
                            }
                        }

                        if (stateSettings.freezePermanently)
                        {
                            if (behaviours.Length > 0 && behaviours[0] != null)
                            {
                                Destroy(behaviours[0].transform.parent.gameObject);
                            }
                            Destroy(gameObject);
                            return;
                        }
                    }
                }

                // Reactivate behaviours
                foreach (BehaviourBase behaviour in behaviours)
                {
                    behaviour.OnReactivate();
                }
            }
        }

        void Awake()
        {
#if UNITY_5_1
			Debug.LogError("PuppetMaster requires at least Unity 5.2.2.");
			awakeFailed = true;
			return;
#endif

            // Do not initiate when the component has been added in run-time. The muscles have not been set up yet.
            if (muscles.Length == 0) return;

            Initiate();
            if (!initiated) awakeFailed = true;
        }

        public void Start()
        {
            /*
#if UNITY_EDITOR
			if (Profiler.enabled && visualizeTargetPose) Debug.Log("Switch 'Visualize Target Pose' off when profiling PuppetMaster.", transform);
#endif
*/
            if (!initiated && !awakeFailed)
            {
                Initiate();
            }

            if (!initiated) return;

            // Find the SolverManagers on the Target hierarchy
            var solversArray = (SolverManager[])targetRoot.GetComponentsInChildren<SolverManager>();
            solvers.AddRange(solversArray);
        }

        public Transform FindTargetRootRecursive(Transform t)
        {
            if (t.parent == null) return null;

            foreach (Transform child in t.parent)
            {
                if (child == transform) return t;
            }

            return FindTargetRootRecursive(t.parent);
        }

        private Muscle[] defaultMuscles = new Muscle[0];

        private void Initiate()
        {
            initiated = false;

            // Find the target root
            if (muscles.Length > 0 && muscles[0].target != null && targetRoot == null) targetRoot = FindTargetRootRecursive(muscles[0].target);
            if (targetRoot != null && targetAnimator == null)
            {
                targetAnimator = targetRoot.GetComponentInChildren<Animator>();
                if (targetAnimator == null) targetAnimation = targetRoot.GetComponentInChildren<Animation>();
            }

            // Validation
            if (!IsValid(true)) return;

            if (humanoidConfig != null && targetAnimator != null && targetAnimator.isHuman)
            {
                humanoidConfig.ApplyTo(this);
            }

            isLegacy = targetAnimator == null && targetAnimation != null;

            behaviours = transform.GetComponentsInChildren<BehaviourBase>();
            if (behaviours.Length == 0 && transform.parent != null) behaviours = transform.parent.GetComponentsInChildren<BehaviourBase>();

            for (int i = 0; i < muscles.Length; i++)
            {
                // Initiating the muscles
                muscles[i].Initiate(muscles);

                // Collision event broadcasters
                if (behaviours.Length > 0)
                {
                    muscles[i].broadcaster = muscles[i].joint.gameObject.GetComponent<MuscleCollisionBroadcaster>();
                    if (muscles[i].broadcaster == null) muscles[i].broadcaster = muscles[i].joint.gameObject.AddComponent<MuscleCollisionBroadcaster>();
                    muscles[i].broadcaster.puppetMaster = this;
                    muscles[i].broadcaster.muscleIndex = i;
                }

                muscles[i].jointBreakBroadcaster = muscles[i].joint.gameObject.GetComponent<JointBreakBroadcaster>();
                if (muscles[i].jointBreakBroadcaster == null) muscles[i].jointBreakBroadcaster = muscles[i].joint.gameObject.AddComponent<JointBreakBroadcaster>();
                muscles[i].jointBreakBroadcaster.puppetMaster = this;
                muscles[i].jointBreakBroadcaster.muscleIndex = i;
            }

            UpdateHierarchies();

            foreach (PropMuscle propMuscle in propMuscles) propMuscle.OnInitiate();

            hierarchyIsFlat = HierarchyIsFlat();

            FlagInternalCollisionsForUpdate();
            FlagAngularLimitsForUpdate();

            initiated = true;

            // Initiate behaviours
            foreach (BehaviourBase behaviour in behaviours)
            {
                behaviour.puppetMaster = this;
            }

            foreach (BehaviourBase behaviour in behaviours)
            {
                behaviour.Initiate();
            }

            // Switching states
            SwitchStates();

            // Switching modes
            SwitchModes();

            foreach (Muscle m in muscles) m.Read();

            // Mapping
            StoreTargetMappedState();

            if (PuppetMasterSettings.instance != null)
            {
                PuppetMasterSettings.instance.Register(this);
            }

            // Choose the behaviour to activate. If BehaviourPuppet is present and enabled, start with that, if not, start with the first enabled behaviour.
            bool hasPuppet = false;
            foreach (BehaviourBase behaviour in behaviours)
            {
                if (behaviour is BehaviourPuppet && behaviour.enabled)
                {
                    ActivateBehaviour(behaviour);
                    hasPuppet = true;
                    break;
                }
            }

            if (!hasPuppet && behaviours.Length > 0)
            {
                foreach (BehaviourBase behaviour in behaviours)
                {
                    if (behaviour.enabled)
                    {
                        ActivateBehaviour(behaviour);
                        break;
                    }
                }
            }

            defaultMuscles = (Muscle[])muscles.Clone();

            if (OnPostInitiate != null) OnPostInitiate();

            if (!autoSimulate) enabled = false;
        }

        private void ActivateBehaviour(BehaviourBase behaviour)
        {
            foreach (BehaviourBase b in behaviours)
            {
                b.enabled = b == behaviour;

                // Call activate manually (also called in behaviour.OnEnable) because behaviour.OnEnable has been called already by Unity with the beahviour uninitiated.
                if (b.enabled) b.Activate();
            }
        }

        void OnDestroy()
        {
            if (PuppetMasterSettings.instance != null)
            {
                PuppetMasterSettings.instance.Unregister(this);
            }
        }

        private bool IsInterpolated()
        {
            if (!initiated) return false;

            foreach (Muscle m in muscles)
            {
                if (m.rigidbody.interpolation != RigidbodyInterpolation.None) return true;
            }

            return false;
        }

        private Vector3 rebuildPelvisPos;
        private Quaternion rebuildPelvisRot = Quaternion.identity;

        private void OnRebuild()
        {
            rebuildFlag = false;

            if (activeMode == Mode.Disabled)
            {
                Debug.LogError("Can not rebuild a puppet in Disabled mode");
                return;
            }


            rebuildPelvisPos = defaultMuscles[0].target.position;
            rebuildPelvisRot = defaultMuscles[0].target.rotation;

            foreach (Muscle m in defaultMuscles)
            {
                m.Rebuild();
            }

            foreach (Muscle m in defaultMuscles)
            {
                if (!ContainsJoint(m.joint))
                {
                    AddMuscle(m.joint, m.target, m.rebuildConnectedBody, m.rebuildTargetParent);
                }
            }

            FlagInternalCollisionsForUpdate();
            FlagAngularLimitsForUpdate();

            foreach (BehaviourBase b in behaviours)
            {
                b.OnReactivate();
            }

            onPostRebuildFlag = true;
        }

        /// <summary>
        /// To be called before Physics.Simulate() if Physics.autoSimulation is false and PuppetMaster component disabled. Note that this method also updates the Animator so that is forced to disabled.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void OnPreSimulate(float deltaTime)
        {
            foreach (BehaviourBase b in behaviours)
            {
                b.UpdateB();
                b.FixedUpdateB();
            }

            if (!initiated) return;
            if (rebuildFlag) OnRebuild();
            foreach (PropMuscle propMuscle in propMuscles) propMuscle.OnUpdate();
            ProcessDisconnects();
            ProcessReconnects();

            if (muscles.Length <= 0) return;

            interpolated = IsInterpolated();

            if (!isActive)
            {
                if (teleport) Read();
                return;
            }

            pinWeight = Mathf.Clamp(pinWeight, 0f, 1f);
            muscleWeight = Mathf.Clamp(muscleWeight, 0f, 1f);
            muscleSpring = Mathf.Clamp(muscleSpring, 0f, muscleSpring);
            muscleDamper = Mathf.Clamp(muscleDamper, 0f, muscleDamper);
            pinPow = Mathf.Clamp(pinPow, 1f, 8f);
            pinDistanceFalloff = Mathf.Max(pinDistanceFalloff, 0f);

            FixTargetTransforms();
            if (targetAnimator.enabled) targetAnimator.enabled = false;

            targetAnimator.Update(deltaTime);

            foreach (SolverManager solver in solvers)
            {
                if (solver != null) solver.UpdateSolverExternal();
            }

            if (OnRead != null) OnRead(); // Update IK
            foreach (BehaviourBase behaviour in behaviours) behaviour.OnRead();

            Read();

            if (!isFrozen)
            {
                // Update internal collision ignoring
                UpdateInternalCollisions();

                // Update angular limit ignoring
                UpdateAngularLimits();

                // Update anchors
                /*
				if (isAlive && updateJointAnchors) {
					// @todo not last animated pose here, but last mapped pose, move this to LateUpdate maybe, remove muscle.targetLocalPosition
					// @todo might it be that when Kinematic, joints are under stress for not having their anchors updated and will fly away when the puppet is activated?
					for (int i = 0; i < muscles.Length; i++) muscles[i].UpdateAnchor(supportTranslationAnimation);
				}
				*/

                // Set solver iteration count
                if (solverIterationCount != lastSolverIterationCount)
                {
                    for (int i = 0; i < muscles.Length; i++)
                    {
                        muscles[i].rigidbody.solverIterations = solverIterationCount;
                    }

                    lastSolverIterationCount = solverIterationCount;
                }

                // Update Muscles
                for (int i = 0; i < muscles.Length; i++)
                {
                    muscles[i].Update(pinWeight, muscleWeight, muscleSpring, muscleDamper, pinPow, pinDistanceFalloff, true, angularPinning, deltaTime);
                }
            }

            // Fix transforms to be sure of not having any drifting when the target bones are not animated
            if (updateMode == UpdateMode.AnimatePhysics) FixTargetTransforms();
        }

        /// <summary>
        /// To be called after Physics.Simulate() if Physics.autoSimulation is false and PuppetMaster component disabled.
        /// </summary>
        public void OnPostSimulate()
        {
            foreach (BehaviourBase b in behaviours) b.LateUpdateB();

            if (muscles.Length <= 0) return;

            if (initiated)
            {
                // Switching states
                SwitchStates();

                // Switching modes
                SwitchModes();

                // Mapping
                if (!isFrozen)
                {
                    mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
                    float mW = mappingWeight * mappingBlend;

                    if (mW > 0f)
                    {
                        if (isActive)
                        {
                            for (int i = 0; i < muscles.Length; i++) muscles[i].Map(mW);
                        }
                    }
                    else
                    {
                        // Moving to Target when in Kinematic mode
                        if (activeMode == Mode.Kinematic) MoveToTarget();
                    }

                    foreach (BehaviourBase behaviour in behaviours) behaviour.OnWrite();
                    if (OnWrite != null) OnWrite();

                    StoreTargetMappedState(); //@todo no need to do this all the time

                    foreach (Muscle m in muscles) m.CalculateMappedVelocity();
                }

                if (mapDisconnectedMuscles)
                {
                    for (int i = 0; i < muscles.Length; i++) muscles[i].MapDisconnected();
                }

                // Freezing
                if (freezeFlag) OnFreezeFlag();
            }

            if (onPostRebuildFlag)
            {
                defaultMuscles[0].target.position = rebuildPelvisPos;
                defaultMuscles[0].target.rotation = rebuildPelvisRot;

                foreach (Muscle m in muscles)
                {
                    m.MoveToTarget();
                    m.ClearVelocities();
                }

                onPostRebuildFlag = false;
            }

            if (OnPostLateUpdate != null) OnPostLateUpdate();
        }

        protected virtual void FixedUpdate()
        {
            foreach (BehaviourBase b in behaviours) b.FixedUpdateB();

            if (!initiated) return;
            if (!autoSimulate) return;

            if (rebuildFlag) OnRebuild();
            foreach (PropMuscle propMuscle in propMuscles) propMuscle.OnUpdate();
            ProcessDisconnects();
            ProcessReconnects();

            if (muscles.Length <= 0) return;

            interpolated = IsInterpolated();

            fixedFrame = true;
            if (!isActive)
            {
                if (teleport)
                {
                    Read();
                }
                return;
            }

            pinWeight = Mathf.Clamp(pinWeight, 0f, 1f);
            muscleWeight = Mathf.Clamp(muscleWeight, 0f, 1f);
            muscleSpring = Mathf.Clamp(muscleSpring, 0f, muscleSpring);
            muscleDamper = Mathf.Clamp(muscleDamper, 0f, muscleDamper);
            pinPow = Mathf.Clamp(pinPow, 1f, 8f);
            pinDistanceFalloff = Mathf.Max(pinDistanceFalloff, 0f);

            // If updating the Animator manually here in FixedUpdate
            if (updateMode == UpdateMode.FixedUpdate)
            {
                FixTargetTransforms();

                if (targetAnimator.enabled || (!targetAnimator.enabled && animatorDisabled))
                {
                    targetAnimator.enabled = false;
                    animatorDisabled = true;
                    targetAnimator.Update(Time.fixedDeltaTime);
                }
                else
                {
                    animatorDisabled = false;
                    targetAnimator.enabled = false;
                }

                foreach (SolverManager solver in solvers)
                {
                    if (solver != null) solver.UpdateSolverExternal();
                }

                if (OnRead != null) OnRead();
                foreach (BehaviourBase behaviour in behaviours) behaviour.OnRead();
                Read();
                readInFixedUpdate = true;
            }

            if (!isFrozen)
            {
                // Update internal collision ignoring
                UpdateInternalCollisions();

                // Update angular limit ignoring
                UpdateAngularLimits();

                // Update anchors
                /*
				if (isAlive && updateJointAnchors) {
					// @todo not last animated pose here, but last mapped pose, move this to LateUpdate maybe, remove muscle.targetLocalPosition
					// @todo might it be that when Kinematic, joints are under stress for not having their anchors updated and will fly away when the puppet is activated?
					for (int i = 0; i < muscles.Length; i++) muscles[i].UpdateAnchor(supportTranslationAnimation);
				}
				*/


                // Set solver iteration count
                if (solverIterationCount != lastSolverIterationCount)
                {
                    for (int i = 0; i < muscles.Length; i++)
                    {
                        muscles[i].rigidbody.solverIterations = solverIterationCount;
                    }

                    lastSolverIterationCount = solverIterationCount;
                }

                // Update Muscles
                for (int i = 0; i < muscles.Length; i++)
                {
                    muscles[i].Update(pinWeight, muscleWeight, muscleSpring, muscleDamper, pinPow, pinDistanceFalloff, true, angularPinning, Time.fixedDeltaTime);
                }
            }

            // Fix transforms to be sure of not having any drifting when the target bones are not animated
            if (updateMode == UpdateMode.AnimatePhysics) FixTargetTransforms();
        }

        protected virtual void Update()
        {
            foreach (BehaviourBase b in behaviours) b.UpdateB();

            if (!initiated) return;
            if (!autoSimulate) return;
            if (muscles.Length <= 0) return;

            if (animatorDisabled)
            {
                targetAnimator.enabled = true;
                animatorDisabled = false;
            }

            if (updateMode != UpdateMode.Normal) return;

            // Fix transforms to be sure of not having any drifting when the target bones are not animated
            FixTargetTransforms();
        }

        protected virtual void LateUpdate()
        {
            foreach (BehaviourBase b in behaviours) b.LateUpdateB();

            if (!autoSimulate) return;
            if (muscles.Length <= 0) return;

            OnLateUpdate();

            if (onPostRebuildFlag)
            {
                defaultMuscles[0].target.position = rebuildPelvisPos;
                defaultMuscles[0].target.rotation = rebuildPelvisRot;

                foreach (Muscle m in muscles)
                {
                    m.MoveToTarget();
                    m.ClearVelocities();
                }

                onPostRebuildFlag = false;
            }

            if (OnPostLateUpdate != null) OnPostLateUpdate();
        }

        private bool readInFixedUpdate;

        protected virtual void OnLateUpdate()
        {
            if (!initiated) return;

            if (animatorDisabled)
            {
                targetAnimator.enabled = true;
                animatorDisabled = false;
            }

            bool animationApplied = updateMode == UpdateMode.Normal || (!readInFixedUpdate && fixedFrame);
            readInFixedUpdate = false;
            bool muscleRead = animationApplied && isActive; // If disabled, reading will be done in PuppetMasterModes.cs

            if (animationApplied)
            {
                if (OnRead != null) OnRead(); // Update IK
                foreach (BehaviourBase behaviour in behaviours) behaviour.OnRead();
            }
            if (muscleRead) Read();
            
            // Switching states
            SwitchStates();

            // Switching modes
            SwitchModes();

            switch (updateMode)
            {
                case UpdateMode.FixedUpdate:
                    if (!fixedFrame && !interpolated) return;
                    break;
                case UpdateMode.AnimatePhysics:
                    if (!fixedFrame && !interpolated) return;
                    break;
            }

            // Below is common code for all update modes! For AnimatePhysics modes the following code will run only in fixed frames
            fixedFrame = false;

            // Mapping
            if (!isFrozen)
            {
                mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
                float mW = mappingWeight * mappingBlend;

                if (mW > 0f)
                {
                    if (isActive)
                    {
                        //Debug.DrawLine(muscles[0].transform.position, Vector3.zero, Color.blue, 1f);
                        //if (muscles[0].transform.position.y > 2.5f) Debug.Break();

                        for (int i = 0; i < muscles.Length; i++) muscles[i].Map(mW);
                    }
                }
                else
                {
                    // Moving to Target when in Kinematic mode
                    if (activeMode == Mode.Kinematic) MoveToTarget();
                }

                foreach (BehaviourBase behaviour in behaviours) behaviour.OnWrite();
                if (OnWrite != null) OnWrite();

                StoreTargetMappedState(); //@todo no need to do this all the time

                foreach (Muscle m in muscles) m.CalculateMappedVelocity();
            }
            
            if (mapDisconnectedMuscles)
            {
                for (int i = 0; i < muscles.Length; i++) muscles[i].MapDisconnected();
            }
            

            // Freezing
            if (freezeFlag) OnFreezeFlag();
        }

        /*
        protected virtual void OnLateUpdate()
        {
            if (!initiated) return;

            if (animatorDisabled)
            {
                targetAnimator.enabled = true;
                animatorDisabled = false;
            }

            // Switching states
            SwitchStates();

            // Switching modes
            SwitchModes();

            // Update modes
            switch (updateMode)
            {
                case UpdateMode.FixedUpdate:
                    if (!isActive && fixedFrame && OnRead != null) OnRead();
                    if (!fixedFrame && !interpolated) return;
                    break;
                case UpdateMode.AnimatePhysics: // Legacy AnimatePhysics
                    if (!fixedFrame && !interpolated) return;
                    if (isActive && !fixedFrame) Read();
                    break;
                case UpdateMode.Normal:
                    if (isActive) Read();
                    else if (OnRead != null) OnRead();
                    break;
            }

            // Below is common code for all update modes! For AnimatePhysics modes the following code will run only in fixed frames
            fixedFrame = false;

            // Mapping
            if (!isFrozen)
            {
                mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
                float mW = mappingWeight * mappingBlend;

                if (mW > 0f)
                {
                    if (isActive)
                    {
                        for (int i = 0; i < muscles.Length; i++) muscles[i].Map(mW);
                    }
                }
                else
                {
                    // Moving to Target when in Kinematic mode
                    if (activeMode == Mode.Kinematic) MoveToTarget();
                }

                foreach (BehaviourBase behaviour in behaviours) behaviour.OnWrite();
                if (OnWrite != null) OnWrite();

                StoreTargetMappedState(); //@todo no need to do this all the time

                foreach (Muscle m in muscles) m.CalculateMappedVelocity();
            }

            if (mapDisconnectedMuscles)
            {
                for (int i = 0; i < muscles.Length; i++) muscles[i].MapDisconnected();
            }

            // Freezing
            if (freezeFlag) OnFreezeFlag();
        }
        */
        // Moves the muscles to where their targets are.
        private void MoveToTarget()
        {
            if (PuppetMasterSettings.instance == null || (PuppetMasterSettings.instance != null && PuppetMasterSettings.instance.UpdateMoveToTarget(this)))
            {
                foreach (Muscle m in muscles)
                {
                    m.MoveToTarget();
                }
            }
        }

        // Read the current animated target pose
        private void Read()
        {
            // Teleporting
            if (teleport)
            {
                GameObject c = new GameObject();
                c.transform.position = transform.parent != null ? transform.parent.position : Vector3.zero;
                c.transform.rotation = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
                var parent = transform.parent;
                var targetParent = targetRoot.parent;
                transform.parent = c.transform;
                targetRoot.parent = c.transform;

                Vector3 p = transform.parent.position;

                Quaternion targetDeltaRotation = RootMotion.QuaTools.FromToRotation(targetRoot.rotation, teleportRotation);
                transform.parent.rotation = targetDeltaRotation * transform.parent.rotation;

                Vector3 targetDeltaPosition = teleportPosition - targetRoot.position;
                transform.parent.position += targetDeltaPosition;

                transform.parent = parent;
                targetRoot.parent = targetParent;
                Destroy(c);

                muscles[0].targetMappedPosition = p + targetDeltaRotation * (muscles[0].targetMappedPosition - p) + targetDeltaPosition;
                muscles[0].targetSampledPosition = p + targetDeltaRotation * (muscles[0].targetSampledPosition - p) + targetDeltaPosition;

                muscles[0].targetMappedRotation = targetDeltaRotation * muscles[0].targetMappedRotation;
                muscles[0].targetSampledRotation = targetDeltaRotation * muscles[0].targetSampledRotation;

                if (teleportMoveToTarget)
                {
                    foreach (Muscle m in muscles)
                    {
                        m.MoveToTarget();
                    }
                }

                foreach (Muscle m in muscles)
                {
                    m.ClearVelocities();
                }

                foreach (BehaviourBase behaviour in behaviours) behaviour.OnTeleport(targetDeltaRotation, targetDeltaPosition, p, teleportMoveToTarget);

                teleport = false;
            }

            if (!isAlive) return;

#if UNITY_EDITOR
            VisualizeTargetPose();
#endif

            foreach (Muscle m in muscles) m.Read();

            if (isAlive && updateJointAnchors)
            {
                for (int i = 0; i < muscles.Length; i++) muscles[i].UpdateAnchor(supportTranslationAnimation);
            }
        }

        // Fix transforms to be sure of not having any drifting when the target bones are not animated
        private void FixTargetTransforms()
        {
            if (!isAlive) return;
            if (OnFixTransforms != null) OnFixTransforms();

            foreach (BehaviourBase behaviour in behaviours) behaviour.OnFixTransforms();

            if (!fixTargetTransforms && !hasProp) return;
            if (!isActive) return;

            mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);

            float mW = mappingWeight * mappingBlend;
            if (mW <= 0f) return;

            for (int i = 0; i < muscles.Length; i++)
            {
                if (fixTargetTransforms || muscles[i].props.group == Muscle.Group.Prop)
                {
                    muscles[i].FixTargetTransforms();
                }
            }
        }

        // Which update mode is the target's Animator/Animation using?
        private AnimatorUpdateMode targetUpdateMode
        {
            get
            {
                if (targetAnimator != null) return targetAnimator.updateMode;
                if (targetAnimation != null) return targetAnimation.animatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
                return AnimatorUpdateMode.Normal;
            }
        }

        #endregion Update Sequence

        // Visualizes the target pose exactly as it is read by the PuppetMaster
        private void VisualizeTargetPose()
        {
            if (!visualizeTargetPose) return;
            if (!Application.isEditor) return;
            if (!isActive) return;

            foreach (Muscle m in muscles)
            {
                if (m.joint.connectedBody != null && m.connectedBodyTarget != null)
                {
                    Debug.DrawLine(m.target.position, m.connectedBodyTarget.position, Color.cyan);

                    bool isEndMuscle = true;
                    foreach (Muscle m2 in muscles)
                    {
                        if (m != m2 && m2.joint.connectedBody == m.rigidbody)
                        {
                            isEndMuscle = false;
                            break;
                        }
                    }

                    if (isEndMuscle) VisualizeHierarchy(m.target, Color.cyan);
                }
            }
        }

        // Recursively visualizes a bone hierarchy
        private void VisualizeHierarchy(Transform t, Color color)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Debug.DrawLine(t.position, t.GetChild(i).position, color);
                VisualizeHierarchy(t.GetChild(i), color);
            }
        }

        /// <summary>
        /// Call this if you have made changes to muscle.props.internalCollisionIgnores at runtime.
        /// </summary>
        public void FlagInternalCollisionsForUpdate()
        {
            if (manualInternalCollisionControl) return;
            internalCollisionsEnabled = !internalCollisions;
        }

        private void UpdateInternalCollisions()
        {
            if (manualInternalCollisionControl) return;
            if (internalCollisionsEnabled == internalCollisions) return;

            if (internalCollisions) ResetInternalCollisions();
            else IgnoreInternalCollisions();
        }

        public void UpdateInternalCollisions(Muscle m)
        {
            if (manualInternalCollisionControl) return;

            foreach (Muscle otherMuscle in muscles)
            {
                if (otherMuscle != m)
                {
                    if (internalCollisions)
                    {
                        m.ResetInternalCollisions(otherMuscle, true);
                    }
                    else
                    {
                        m.IgnoreInternalCollisions(otherMuscle);
                    }
                }
            }
        }

        private void IgnoreInternalCollisions()
        {
            if (manualInternalCollisionControl) return;

            for (int i = 0; i < muscles.Length; i++)
            {
                for (int i2 = i; i2 < muscles.Length; i2++)
                {
                    if (i != i2)
                    {
                        muscles[i].IgnoreInternalCollisions(muscles[i2]);
                    }
                }
            }

            internalCollisions = false;
            internalCollisionsEnabled = false;
        }

        public void IgnoreInternalCollisions(Muscle m)
        {
            if (manualInternalCollisionControl) return;

            foreach (Muscle otherMuscle in muscles)
            {
                if (otherMuscle != m)
                {
                    m.IgnoreInternalCollisions(otherMuscle);
                }
            }
        }

        private void ResetInternalCollisions()
        {
            if (manualInternalCollisionControl) return;

            for (int i = 0; i < muscles.Length; i++)
            {
                for (int i2 = i; i2 < muscles.Length; i2++)
                {
                    if (i != i2)
                    {
                        muscles[i].ResetInternalCollisions(muscles[i2], true);
                    }
                }
            }

            internalCollisions = true;
            internalCollisionsEnabled = true;
        }

        public void ResetInternalCollisions(Muscle m, bool useInternalCollisionIgnores)
        {
            if (manualInternalCollisionControl) return;

            foreach (Muscle otherMuscle in muscles)
            {
                if (otherMuscle != m)
                {
                    m.ResetInternalCollisions(otherMuscle, useInternalCollisionIgnores);
                }
            }
        }

        public void FlagAngularLimitsForUpdate()
        {
            if (manualAngularLimitControl) return;
            angularLimitsEnabled = !angularLimits;
        }

        private void UpdateAngularLimits()
        {
            if (manualAngularLimitControl) return;
            if (angularLimitsEnabled == angularLimits) return;

            for (int i = 0; i < muscles.Length; i++)
            {
                if (!muscles[i].state.isDisconnected) muscles[i].IgnoreAngularLimits(!angularLimits);
            }

            angularLimitsEnabled = angularLimits;
        }
    }
}
