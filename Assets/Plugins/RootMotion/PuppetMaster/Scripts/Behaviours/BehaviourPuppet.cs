using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.Dynamics {

	/// <summary>
	/// This behaviour handles pinning and unpinning puppets when they collide with objects or are hit via code, also automates getting up from an unbalanced state.
	/// </summary>
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page10.html")]
	[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourPuppet")]
	public partial class BehaviourPuppet : BehaviourBase {

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		void OpenUserManual() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page10.html");
		}

		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		void OpenScriptReference() {
			Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_behaviour_puppet.html");
		}

		#region Main Properties

		/// <summary>
		/// Puppet means the character is in normal state and pinned. Unpinned means the character has lost balance and is animated physically in muscle space only. GetUp is a transition state from Unpinned to Puppet.
		/// </summary>
		[System.Serializable]
		public enum State {
			Puppet,
			Unpinned,
			GetUp
		}

		[System.Serializable]
		public enum NormalMode {
			Active,
			Unmapped,
			Kinematic
		}

		/// <summary>
		/// Master properties for BehaviourPuppet.  Options for switching modes and disabling mapping when the Puppet is out of contact.
		/// </summary>
		[System.Serializable]
		public class MasterProps  {

			/// <summary>
			/// How does the puppet behave when currently not in contact with anything? Active mode keeps the PuppetMaster Active and mapped at all times. Unmapped blends out mapping to maintain 100% animation quality. Kenamatic keeps the PuppetMaster in Kinematic mode until there is a collision.
			/// </summary>
			public NormalMode normalMode;
			
			/// <summary>
			/// "The speed of blending in mapping in case of contact when in Unmapped normal mode."
			/// </summary>
			public float mappingBlendSpeed = 10f;
			
			/// <summary>
			/// If false, static colliders will not activate the puppet when they collide with the muscles. Note that the static colliders need to have a kinematic Rigidbody attached for this to work. Used only in Kinematic normal mode.
			/// </summary>
			public bool activateOnStaticCollisions;
			
			/// <summary>
			/// Minimum collision impulse for activating the puppet. Used only in Kinematic normal mode.
			/// </summary>
			public float activateOnImpulse = 0f;
		}

		/// <summary>
		/// Defines the properties of muscle behaviour.
		/// </summary>
		[System.Serializable]
		public struct MuscleProps {
			[TooltipAttribute("How much will collisions with muscles of this group unpin parent muscles?")]
			/// <summary>
			/// How much will collisions with muscles of this group unpin parent muscles?
			/// </summary>
			[Range(0f, 1f)] public float unpinParents;

			[TooltipAttribute("How much will collisions with muscles of this group unpin child muscles?")]
			/// <summary>
			/// How much will collisions with muscles of this group unpin child muscles?
			/// </summary>
			[Range(0f, 1f)] public float unpinChildren;

			[TooltipAttribute("How much will collisions with muscles of this group unpin muscles of the same group?")]
			/// <summary>
			/// How much will collisions with muscles of this group unpin muscles of the same group?
			/// </summary>
			[Range(0f, 1f)] public float unpinGroup;

			[TooltipAttribute("If 1, muscles of this group will always be mapped to the ragdoll.")]
			/// <summary>
			/// If 1, muscles of this group will always be mapped to the ragdoll.
			/// </summary>
			[Range(0f, 1f)] public float minMappingWeight;

			[TooltipAttribute("If 0, muscles of this group will not be mapped to the ragdoll pose even if they are unpinned.")]
			/// <summary>
			/// If 0, muscles of this group will not be mapped to the ragdoll pose even if they are unpinned.
			/// </summary>
			[Range(0f, 1f)] public float maxMappingWeight;

            [TooltipAttribute("Defines minimum pin weight for the muscles. Muscle pin weight can’t be reduced beyond this value when damage occurs from collisions.")]
            /// <summary>
            /// Defines minimum pin weight for the muscles. Muscle pin weight can’t be reduced beyond this value when damage occurs from collisions.
            /// </summary>
            [Range(0f, 1f)] public float minPinWeight;

            [TooltipAttribute("If true, muscles of this group will have their colliders disabled while in puppet state (not unbalanced nor getting up).")]
			/// <summary>
			/// If true, muscles of this group will have their colliders disabled while in puppet state (not unbalanced nor getting up).
			/// </summary>
			public bool disableColliders;

			[TooltipAttribute("How fast will muscles of this group regain their pin weight (multiplier)?")]
			/// <summary>
			/// How fast will muscles of this group regain their pin weight (multiplier)?
			/// </summary>
			public float regainPinSpeed;

			[TooltipAttribute("Smaller value means more unpinning from collisions (multiplier).")]
			/// <summary>
			/// Smaller value means more unpinning from collisions (multiplier).
			/// </summary>
			public float collisionResistance;

			[TooltipAttribute("If the distance from the muscle to it's target is larger than this value, the character will be knocked out.")]
			/// <summary>
			/// If the distance from the muscle to it's target is larger than this value, the character will be knocked out.
			/// </summary>
			public float knockOutDistance;

            [Tooltip("The PhysicsMaterial applied to the muscles while the character is in Puppet or GetUp state. Using a lower friction material reduces the risk of muscles getting stuck and pulled out of their joints.")]
			/// <summary>
			/// The PhysicsMaterial applied to the muscles while the character is in Puppet or GetUp state. Using a lower friction material reduces the risk of muscles getting stuck and pulled out of their joints.
			/// </summary>
			public PhysicMaterial puppetMaterial;
			
			[Tooltip("The PhysicsMaterial applied to the muscles while the character is in Unpinned state.")]
			/// <summary>
			/// The PhysicsMaterial applied to the muscles while the character is in Unpinned state.
			/// </summary>
			public PhysicMaterial unpinnedMaterial;
		}

		/// <summary>
		/// Defines the properties of muscle behaviour for certain muscle group(s).
		/// </summary>
		[System.Serializable]
		public struct MusclePropsGroup {

			// just for the Editor
			[HideInInspector] public string name;

			[TooltipAttribute("Muscle groups to which those properties apply.")]
			/// <summary>
			/// Muscle groups to which those properties apply.
			/// </summary>
			public Muscle.Group[] groups;

			[TooltipAttribute("The muscle properties for those muscle groups.")]
			/// <summary>
			/// The muscle properties for those muscle groups.
			/// </summary>
			public MuscleProps props;
		}

		/// <summary>
		/// Multiplies collision resistance for the specified layers.
		/// </summary>
		[System.Serializable]
		public struct CollisionResistanceMultiplier {
			public LayerMask layers;

			[TooltipAttribute("Multiplier for the 'Collision Resistance' for these layers.")]
			public float multiplier;

			[TooltipAttribute("Overrides 'Collision Threshold' for these layers.")]
			/// <summary>
			/// Overrides 'Collision Threshold' for these layers.
			/// </summary>
			public float collisionThreshold;
		}

		[LargeHeader("Collision And Recovery")]

		/// <summary>
		/// Master properties for BehaviourPuppet. Options for switching modes and disabling mapping when the Puppet is out of contact.
		/// </summary>
		public MasterProps masterProps = new MasterProps();

		[Tooltip("Will ground the target to those layers when getting up.")]
		/// <summary>
		/// Will ground the target to those layers when getting up.
		/// </summary>
		public LayerMask groundLayers;

		[Tooltip("Will unpin the muscles that collide with those layers.")]
		/// <summary>
		/// Will unpin the muscles that collide with those layers.
		/// </summary>
		public LayerMask collisionLayers;
		
		[Tooltip("The collision impulse sqrMagnitude threshold under which collisions will be ignored.")]
		/// <summary>
		/// The collision impulse sqrMagnitude threshold under which collisions will be ignored.
		/// </summary>
		public float collisionThreshold;

		/// <summary>
		/// Smaller value means more unpinning from collisions so the characters get knocked out more easily. If using a curve, the value will be evaluated by each muscle's target velocity magnitude. This can be used to make collision resistance higher while the character moves or animates faster.
		/// </summary>
		public Weight collisionResistance = new Weight(3f, "Smaller value means more unpinning from collisions so the characters get knocked out more easily. If using a curve, the value will be evaluated by each muscle's target velocity magnitude. This can be used to make collision resistance higher while the character moves or animates faster.");

		[Tooltip("Multiplies collision resistance for the specified layers.")]
		/// <summary>
		/// Multiplies collision resistance for the specified layers.
		/// </summary>
		public CollisionResistanceMultiplier[] collisionResistanceMultipliers;

		[Tooltip("An optimisation. Will only process up to this number of collisions per physics step.")]
		/// <summary>
		/// An optimisation. Will only process up to this number of collisions per physics step.
		/// </summary>
		[Range(1, 30)] public int maxCollisions = 30;

		[Tooltip("How fast will the muscles of this group regain their pin weight?")]
		/// <summary>
		/// How fast will the muscles of this group regain their pin weight?
		/// </summary>
		[Range(0.001f, 10f)] public float regainPinSpeed = 1f;

		[Tooltip("'Boosting' is a term used for making muscles temporarily immune to collisions and/or deal more damage to the muscles of other characters. That is done by increasing Muscle.State.immunity and Muscle.State.impulseMlp. For example when you set muscle.state.immunity to 1, boostFalloff will determine how fast this value will fall back to normal (0). Use BehaviourPuppet.BoostImmunity() and BehaviourPuppet.BoostImpulseMlp() for boosting from your own scripts. It is helpful for making the puppet stronger and deliever more punch while playing a melee hitting/kicking animation.")]
		/// <summary>
		/// 'Boosting' is a term used for making muscles temporarily immune to collisions and/or deal more damage to the muscles of other characters. That is done by increasing Muscle.State.immunity and Muscle.State.impulseMlp. For example when you set muscle.state.immunity to 1, boostFalloff will determine how fast this value will fall back to normal (0). Use BehaviourPuppet.BoostImmunity() and BehaviourPuppet.BoostImpulseMlp() for boosting from your own scripts. It is helpful for making the puppet stronger and deliever more punch while playing a melee hitting/kicking animation.
		/// </summary>
		public float boostFalloff = 1f;

		[LargeHeader("Muscle Group Properties")]
		
		[Tooltip("The default muscle properties. If there are no 'Group Overrides', this will be used for all muscles.")]
		/// <summary>
		/// The default muscle properties. If there are no 'Group Overrides', this will be used for all muscles.
		/// </summary>
		public MuscleProps defaults;
		
		[Tooltip("Overriding default muscle properties for some muscle groups (for example making the feet stiffer or the hands looser).")]
		/// <summary>
		/// Overriding default muscle properties for some muscle groups (for example making the feet stiffer or the hands looser).
		/// </summary>
		public MusclePropsGroup[] groupOverrides;

		[LargeHeader("Losing Balance")]

		[Tooltip("If the distance from the muscle to it's target is larger than this value, the character will be knocked out.")]
		/// <summary>
		/// If the distance from the muscle to it's target is larger than this value, the character will be knocked out.
		/// </summary>
		[Range(0.001f, 10f)] public float knockOutDistance = 1f;

		[Tooltip("Smaller value makes the muscles weaker when the puppet is knocked out.")]
		/// <summary>
		/// Smaller value makes the muscles weaker when the puppet is knocked out.
		/// </summary>
		[Range(0f, 1f)] public float unpinnedMuscleWeightMlp = 0.3f;

		[Tooltip("Most character controllers apply supernatural accelerations to characters when changing running direction or jumping. It will require major pinning forces to be applied on the ragdoll to keep up with that acceleration. When a puppet collides with something at that point and is unpinned, those forces might shoot the puppet off to space. This variable limits the velocity of the ragdoll's Rigidbodies when the puppet is unpinned.")]
		/// <summary>
		/// Most character controllers apply supernatural accelerations to characters when changing running direction or jumping. It will require major pinning forces to be applied on the ragdoll to keep up with that acceleration. When a puppet collides with something at that point and is unpinned, those forces might shoot the puppet off to space. This variable limits the velocity of the ragdoll's Rigidbodies when the puppet is unpinned.
		/// </summary>
		public float maxRigidbodyVelocity = 10f;

		[Tooltip("If a muscle has drifted farther than 'Knock Out Distance', will only unpin the puppet if it's pin weight is less than this value. Lowering this value will make puppets less likely to lose balance on minor collisions.")]
		/// <summary>
		/// If a muscle has drifted farther than 'Knock Out Distance', will only unpin the puppet if it's pin weight is less than this value. Lowering this value will make puppets less likely to lose balance on minor collisions.
		/// </summary>
		[Range(0f, 1f)] public float pinWeightThreshold = 1f;

		[Tooltip("If false, will not unbalance the puppet by muscles that have their pin weight set to 0 in PuppetMaster muscle settings.")]
		/// <summary>
		/// If false, will not unbalance the puppet by muscles that have their pin weight set to 0 in PuppetMaster muscle settings.
		/// </summary>
		public bool unpinnedMuscleKnockout = true;

		[Tooltip("If true, all muscles of the 'Prop' group will be detached from the puppet when it loses balance.")]
		/// <summary>
		/// If true, all muscles of the 'Prop' group will be detached from the puppet when it loses balance.
		/// </summary>
		public bool dropProps;

		[LargeHeader("Getting Up")]

		[Tooltip("If true, GetUp state will be triggerred automatically after 'Get Up Delay' and when the velocity of the hip muscle is less than 'Max Get Up Velocity'.")]
		/// <summary>
		/// If true, GetUp state will be triggerred automatically after 'Get Up Delay' and when the velocity of the hip muscle is less than 'Max Get Up Velocity'.
		/// </summary>
		public bool canGetUp = true;

		[Tooltip("Minimum delay for getting up after loosing balance. After that time has passed, will wait for the velocity of the hip muscle to come down below 'Max Get Up Velocity' and then switch to the GetUp state.")]
		/// <summary>
		/// Minimum delay for getting up after loosing balance. After that time has passed, will wait for the velocity of the hip muscle to come down below 'Max Get Up Velocity' and then switch to the GetUp state.
		/// </summary>
		public float getUpDelay = 5f;

		[Tooltip("The duration of blending the animation target from the ragdoll pose to the getting up animation once the GetUp state has been triggered.")]
		/// <summary>
		/// The duration of blending the animation target from the ragdoll pose to the getting up animation once the GetUp state has been triggered.
		/// </summary>
		public float blendToAnimationTime = 0.2f;

		[Tooltip("Will not get up before the velocity of the hip muscle has come down to this value.")]
		/// <summary>
		/// Will not get up before the velocity of the hip muscle has come down to this value.
		/// </summary>
		public float maxGetUpVelocity = 0.3f;

		[Tooltip("The duration of the 'GetUp' state after which it switches to the 'Puppetä state.")]
		/// <summary>
		/// The duration of the "GetUp" state after which it switches to the "Puppet" state.
		/// </summary>
		public float minGetUpDuration = 1f;

		[Tooltip("Collision resistance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
		/// <summary>
		/// Collision resistance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
		/// </summary>
		public float getUpCollisionResistanceMlp = 2f;

		[Tooltip("Regain pin weight speed multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
		/// <summary>
		/// Regain pin weight speed multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
		/// </summary>
		public float getUpRegainPinSpeedMlp = 2f;

		[Tooltip("Knock out distance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.")]
		/// <summary>
		/// Knock out distance multiplier while in the GetUp state. Increasing this will prevent the character from loosing balance again immediatelly after going from Unpinned to GetUp state.
		/// </summary>
		public float getUpKnockOutDistanceMlp = 10f;

		[Tooltip("Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a prone pose. Tweak this value if your character slides a bit when starting to get up.")]
		/// <summary>
		/// Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a prone pose. Tweak this value if your character slides a bit when starting to get up.
		/// </summary>
		public Vector3 getUpOffsetProne;

		[Tooltip("Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a supine pose. Tweak this value if your character slides a bit when starting to get up.")]
		/// <summary>
		/// Offset of the target character (in character rotation space) from the hip bone when initiating getting up animation from a supine pose. Tweak this value if your character slides a bit when starting to get up.
		/// </summary>
		public Vector3 getUpOffsetSupine;

		[LargeHeader("Events")]

		[Tooltip("Called when the character starts getting up from a prone pose (facing down).")]
		/// <summary>
		/// Called when the character starts getting up from a prone pose (facing down).
		/// </summary>
		public PuppetEvent onGetUpProne;

		[Tooltip("Called when the character starts getting up from a supine pose (facing up).")]
		/// <summary>
		/// Called when the character starts getting up from a supine pose (facing up).
		/// </summary>
		public PuppetEvent onGetUpSupine;

		[Tooltip("Called when the character is knocked out (loses balance). Doesn't matter from which state.")]
		/// <summary>
		/// Called when the character is knocked out (loses balance). Doesn't matter from which state.
		/// </summary>
		public PuppetEvent onLoseBalance;

		[Tooltip("Called when the character is knocked out (loses balance) only from the normal Puppet state.")]
		/// <summary>
		/// Called when the character is knocked out (loses balance) only from the normal Puppet state.
		/// </summary>
		public PuppetEvent onLoseBalanceFromPuppet;

		[Tooltip("Called when the character is knocked out (loses balance) only from the GetUp state.")]
		/// <summary>
		/// Called when the character is knocked out (loses balance) only from the GetUp state.
		/// </summary>
		public PuppetEvent onLoseBalanceFromGetUp;

		[Tooltip("Called when the character has fully recovered and switched to the Puppet state.")]
		/// <summary>
		/// Called when the character has fully recovered and switched to the Puppet state.
		/// </summary>
		public PuppetEvent onRegainBalance;

		public delegate void CollisionImpulseDelegate(MuscleCollision m, float impulse);

		/// <summary>
		/// Called when any of the puppet's muscles has had a collision.
		/// </summary>
		public CollisionDelegate OnCollision;

		/// <summary>
		/// Called when any of the puppet's muscles has had a collision and that collision has resulted in a loss of pinning.
		/// </summary>
		public CollisionImpulseDelegate OnCollisionImpulse;

		/// <summary>
		/// Gets the current state of the puppet (Puppet/Unpinned/GetUp).
		/// </summary>
		public State state { get; private set; }

		/// <summary>
		/// If false, BehaviourPuppet will not move the target root while unpinned or getting up. Useful if target root is synced over the network.
		/// </summary>
		[HideInInspector] public bool canMoveTarget = true;

		// Called when the PuppetMaster has been deactivated (by parenting it to an inactive hierarchy or calling SetActive(false)).
		public override void OnReactivate() {
			state = puppetMaster.state == PuppetMaster.State.Alive? State.Puppet: State.Unpinned;
			getUpTimer = 0f;
			unpinnedTimer = 0f;
			getupAnimationBlendWeight = 0f;
			getupAnimationBlendWeightV = 0f;
			getUpTargetFixed = false;

			getupDisabled = puppetMaster.state != RootMotion.Dynamics.PuppetMaster.State.Alive;
			state = puppetMaster.state == RootMotion.Dynamics.PuppetMaster.State.Alive? State.Puppet: State.Unpinned;

			foreach (Muscle m in puppetMaster.muscles) {
				// Pin, muscle weight and damper already set in PuppetMaster
				if (!m.state.isDisconnected) SetColliders(m, state == State.Unpinned);
			}

			//Activate();
			enabled = true;
		}

		/// <summary>
		/// Resets this puppet to the specified position and rotation and normal Puppet state. Use this for respawning existing puppets.
		/// </summary>
		public void Reset(Vector3 position, Quaternion rotation) {
			Debug.LogWarning("BehaviourPuppet.Reset has been deprecated, please use PuppetMaster.Teleport instead.");
		}

		public override void OnTeleport(Quaternion deltaRotation, Vector3 deltaPosition, Vector3 pivot, bool moveToTarget) {
			getUpPosition = pivot + deltaRotation * (getUpPosition - pivot) + deltaPosition;
			if (moveToTarget) {
				getupAnimationBlendWeight = 0f;
				getupAnimationBlendWeightV = 0f;
			}
		}

		#endregion Main Properties

		private float unpinnedTimer;
		private float getUpTimer;
		private Vector3 hipsForward;
		private Vector3 hipsUp;
		private float getupAnimationBlendWeight, getupAnimationBlendWeightV;
		private bool getUpTargetFixed;
		private NormalMode lastNormalMode;
		private int collisions;
		private bool eventsEnabled;
		private float lastKnockOutDistance;
		private float knockOutDistanceSqr;
		private bool getupDisabled;
		private bool hasCollidedSinceGetUp;

		protected override void OnInitiate() {
			foreach (CollisionResistanceMultiplier crm in collisionResistanceMultipliers) {
				if (crm.layers == 0) {
					Debug.LogWarning("BehaviourPuppet has a Collision Resistance Multiplier that's layers is set to Nothing. Please add some layers.", transform);
				}
			}

			/* // What if the puppet needs to be grounded to a collisin object during getting up?
			int[] groundLayerIndexes = LayerMaskExtensions.MaskToNumbers(groundLayers);
			for (int i = 0; i < groundLayerIndexes.Length; i++) {
				if (LayerMaskExtensions.Contains(collisionLayers, groundLayerIndexes[i])) {
					Debug.LogWarning("BehaviourPuppet 'Collision Layers' contains one of the layers in the 'Ground Layers'. This will make the Puppet lose balance when the feet collide with the ground.");
				}
			}
			*/

			foreach (Muscle m in puppetMaster.muscles) {
				if (m.joint.gameObject.layer == puppetMaster.targetRoot.gameObject.layer) {
					Debug.LogWarning("One of the ragdoll bones is on the same layer as the animated character. This might make the ragdoll collide with the character controller.");
				}
					
				if (!Physics.GetIgnoreLayerCollision(m.joint.gameObject.layer, puppetMaster.targetRoot.gameObject.layer)) {
					Debug.LogWarning("The ragdoll layer (" + m.joint.gameObject.layer + ") and the character controller layer (" + puppetMaster.targetRoot.gameObject.layer + ") are not set to ignore each other in Edit/Project Settings/Physics/Layer Collision Matrix. This might cause the ragdoll bones to collide with the character controller.");
				}
			}

			hipsForward = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.forward;
			hipsUp = Quaternion.Inverse(puppetMaster.muscles[0].transform.rotation) * puppetMaster.targetRoot.up;
			
			state = State.Unpinned;

			eventsEnabled = true;
		}

		protected override void OnActivate() {
			// Start with unpinned on puppet state
			bool unpinned = true;
            if (puppetMaster.pinWeight >= 1f)
            {
                foreach (Muscle m in puppetMaster.muscles)
                {
                    if (m.state.pinWeightMlp > 0.5f)
                    {
                        unpinned = false;
                        break;
                    }
                }
            }

			bool e = eventsEnabled;
			eventsEnabled = false;
			if (unpinned) SetState(State.Unpinned);
			else SetState(State.Puppet);
			eventsEnabled = e;
		}

		public override void KillStart() {
			getupDisabled = true;

			foreach (Muscle m in puppetMaster.muscles) {
                if (!m.state.isDisconnected)
                {
                    m.state.pinWeightMlp = 0f;
                    if (hasBoosted) m.state.immunity = 0f;

                    // Change physic materials
                    SetColliders(m, true);
                }
			}
		}

		public override void KillEnd() {
			SetState(State.Unpinned);
		}

		public override void Resurrect() {
			getupDisabled = false;

			if (state == State.Unpinned) {
				getUpTimer = Mathf.Infinity;
				unpinnedTimer = Mathf.Infinity;
				getupAnimationBlendWeight = 0f;
				getupAnimationBlendWeightV = 0f;
				
				foreach (Muscle m in puppetMaster.muscles) {
					m.state.pinWeightMlp = 0f;
				}
			}
		}

		protected override void OnDeactivate() {
			state = State.Unpinned;
		}

		protected override void OnFixedUpdate() {
			collisions = 0;

			if (dropPropFlag) {
				RemovePropMuscles();

                foreach (PropMuscle m in puppetMaster.propMuscles)
                {
                    m.currentProp = null;
                }
				dropPropFlag = false;
			}

            // If the PuppetMaster is not active, make sure the puppet is in the Puppet state and return.
            if (!puppetMaster.isActive && !puppetMaster.isSwitchingMode)
            {
                SetState(State.Puppet);
				return;
			}

			if (!puppetMaster.isAlive) {
				foreach (Muscle m in puppetMaster.muscles) {
                    if (!m.state.isDisconnected)
                    {
                        m.state.pinWeightMlp = 0f;
                        m.state.mappingWeightMlp = Mathf.MoveTowards(m.state.mappingWeightMlp, 1f, Time.deltaTime * 5f);
                    }
				}
				return;
			}

			// Boosting falloff
			if (hasBoosted) {
				foreach (Muscle m in puppetMaster.muscles) {
                    if (!m.state.isDisconnected)
                    {
                        m.state.immunity = Mathf.MoveTowards(m.state.immunity, 0f, Time.deltaTime * boostFalloff);
                        m.state.impulseMlp = Mathf.Lerp(m.state.impulseMlp, 1f, Time.deltaTime * boostFalloff);
                    }
				}
			}

			// Getting up and making sure the puppet stays unpinned and mapped
			if (state == State.Unpinned) {
				unpinnedTimer += Time.deltaTime;

				if (unpinnedTimer >= getUpDelay && canGetUp && !getupDisabled && puppetMaster.muscles[0].rigidbody.velocity.magnitude < maxGetUpVelocity) {
					SetState(State.GetUp);
					return;
				}

                foreach (Muscle m in puppetMaster.muscles) {
                    if (!m.state.isDisconnected)
                    {
                        m.state.pinWeightMlp = 0f;
                        m.state.mappingWeightMlp = Mathf.MoveTowards(m.state.mappingWeightMlp, 1f, Time.deltaTime * masterProps.mappingBlendSpeed);
                    }
				}
			}

			// In PUPPET and GETUP states...
			if (state != State.Unpinned && !puppetMaster.isKilling) {
				if (knockOutDistance != lastKnockOutDistance) {
					knockOutDistanceSqr = Mathf.Sqrt(knockOutDistance);
					lastKnockOutDistance = knockOutDistance;
				}

				foreach (Muscle m in puppetMaster.muscles) {
					var props = GetProps(m.props.group);
					
					float stateMlp = 1f;

					if (state == State.GetUp) {// && getUpTimer < 0.5f) {
						stateMlp = Mathf.Lerp(getUpKnockOutDistanceMlp, stateMlp, m.state.pinWeightMlp);
					}

                    float offset = unpinnedMuscleKnockout? m.positionOffset.sqrMagnitude: m.positionOffset.sqrMagnitude * m.props.pinWeight * puppetMaster.pinWeight;

                    if (puppetMaster.pinWeight < 1f) hasCollidedSinceGetUp = true;

                    float pinW = m.state.pinWeightMlp * m.props.pinWeight * puppetMaster.pinWeight;

                    // [PuppetMaster 0.5 and earlier] if (!puppetMaster.isBlending && m.state.pinWeightMlp < 0.5f && m.positionOffset.sqrMagnitude * m.props.pinWeight > props.knockOutDistance * knockOutDistanceSqr * stateMlp) {
                    // [PuppetMaster 0.6] if (hasCollidedSinceGetUp && !puppetMaster.isBlending && m.state.pinWeightMlp < 1f && m.positionOffset.sqrMagnitude > props.knockOutDistance * knockOutDistanceSqr * stateMlp) {
                    if (hasCollidedSinceGetUp && !m.state.isDisconnected && !puppetMaster.isBlending && offset > 0f && pinW <= pinWeightThreshold && offset > props.knockOutDistance * knockOutDistanceSqr * stateMlp) {
						if (state != State.GetUp || getUpTargetFixed) {
							SetState(State.Unpinned);
						}
						return;
					}

                    if (!m.state.isDisconnected)
                    {
                        m.state.muscleWeightMlp = Mathf.Lerp(unpinnedMuscleWeightMlp, 1f, m.state.pinWeightMlp);

                        if (state == State.GetUp) m.state.muscleDamperAdd = 0f;

                        if (!puppetMaster.isKilling)
                        {
                            float speedF = 1f;
                            if (state == State.GetUp) speedF = Mathf.Lerp(getUpRegainPinSpeedMlp, 1f, m.state.pinWeightMlp);

                            m.state.pinWeightMlp += Time.deltaTime * props.regainPinSpeed * regainPinSpeed * speedF;
                        }
                    }
				}

				// Max pin weight from the legs and feet
				float maxPinWeight = 1f;
				foreach (Muscle m in puppetMaster.muscles) {
					if (m.props.group == Muscle.Group.Leg || m.props.group == Muscle.Group.Foot) {
						if (!m.state.isDisconnected && m.state.pinWeightMlp < maxPinWeight) maxPinWeight = m.state.pinWeightMlp;
					}
				}
				foreach (Muscle m in puppetMaster.muscles) {
					m.state.pinWeightMlp = Mathf.Clamp(m.state.pinWeightMlp, 0, maxPinWeight * 5f);
				}
			}

			// Ending GetUp
			if (state == State.GetUp) {
				getUpTimer += Time.deltaTime;

				if (getUpTimer > minGetUpDuration) {
					getUpTimer = 0f;
					SetState(State.Puppet);
				}
			}
		}

		protected override void OnLateUpdate() {
			// Used in PuppetMasterModes
			forceActive = state != State.Puppet;

			if (!puppetMaster.isAlive) return;

			// Normal mode switching
			if (masterProps.normalMode != lastNormalMode) {
				if (lastNormalMode == NormalMode.Unmapped) {
					foreach (Muscle m in puppetMaster.muscles) m.state.mappingWeightMlp = 1f;
				}

				if (lastNormalMode == NormalMode.Kinematic) {
					if (puppetMaster.mode == PuppetMaster.Mode.Kinematic) puppetMaster.mode = PuppetMaster.Mode.Active;
				}

				lastNormalMode = masterProps.normalMode;
			}

			// Normal modes
			switch(masterProps.normalMode) {
			case NormalMode.Unmapped:
				if (puppetMaster.isActive) {
                    bool to = puppetMaster.pinWeight < 1f;
                        
					for (int i = 0; i < puppetMaster.muscles.Length; i++) {
						BlendMuscleMapping(i, ref to);
					}
				}
				break;
			case NormalMode.Kinematic:
				if (SetKinematic()) {
					puppetMaster.mode = PuppetMaster.Mode.Kinematic;
				}
				break;
			default:break;
			}
		}

		private bool SetKinematic() {
			if (!puppetMaster.isActive) return false;
			if (state != State.Puppet) return false;
			if (puppetMaster.isBlending) return false;
			if (getupAnimationBlendWeight > 0f) return false;
			if (!puppetMaster.isAlive) return false;

			foreach (Muscle m in puppetMaster.muscles) {
				if (!m.state.isDisconnected && m.state.pinWeightMlp < 1f) return false;
			}

			return true;
		}

		// Called when the PuppetMaster reads
		protected override void OnReadBehaviour() {
			if (!enabled) return;

			if (!puppetMaster.isFrozen) {
                if (state == State.Unpinned && puppetMaster.isActive && !puppetMaster.isBlending && !puppetMaster.muscles[0].state.isDisconnected) {
                //if (state == State.Unpinned && puppetMaster.isActive && !puppetMaster.muscles[0].state.isDisconnected) { 

                    MoveTarget(puppetMaster.muscles[0].rigidbody.position);
					GroundTarget(groundLayers);
					getUpPosition = puppetMaster.targetRoot.position;
				}
			}

            // Prevents root motion from snapping the target to another position
            if (state == State.GetUp && getUpTimer < minGetUpDuration * 0.1f)
            {
                Vector3 y = Vector3.Project(puppetMaster.targetRoot.position - getUpPosition, puppetMaster.targetRoot.up);
                getUpPosition += y;
                MoveTarget(getUpPosition);
            }

            if (getupAnimationBlendWeight > 0f) {
				Vector3 y = Vector3.Project(puppetMaster.targetRoot.position - getUpPosition, puppetMaster.targetRoot.up);
				getUpPosition += y;
				MoveTarget(getUpPosition);

				getupAnimationBlendWeight = Mathf.SmoothDamp(getupAnimationBlendWeight, 0f, ref getupAnimationBlendWeightV, blendToAnimationTime);
				if (getupAnimationBlendWeight < 0.01f) getupAnimationBlendWeight = 0f;
				
				// Lerps the target pose to last sampled mapped pose. Starting off from the ragdoll pose
				puppetMaster.FixTargetToSampledState(getupAnimationBlendWeight);
			}

			getUpTargetFixed = true;
		}

		private void BlendMuscleMapping(int muscleIndex, ref bool to) {
			if (puppetMaster.muscles[muscleIndex].state.pinWeightMlp < 1) to = true;

			var props = GetProps(puppetMaster.muscles[muscleIndex].props.group);
			float target = to? (state == State.Puppet? props.maxMappingWeight: 1f): props.minMappingWeight;

			puppetMaster.muscles[muscleIndex].state.mappingWeightMlp = Mathf.MoveTowards(puppetMaster.muscles[muscleIndex].state.mappingWeightMlp, target, Time.deltaTime * masterProps.mappingBlendSpeed);
		}

		public override void OnMuscleAdded(Muscle m) {
			base.OnMuscleAdded(m);

			SetColliders(m, state == State.Unpinned);
		}

		public override void OnMuscleRemoved(Muscle m) {
			base.OnMuscleRemoved(m);

			SetColliders(m, true);
		}

		protected void MoveTarget(Vector3 position) {
			if (!canMoveTarget) return;

			puppetMaster.targetRoot.position = position;
		}

		protected void RotateTarget(Quaternion rotation) {
			if (!canMoveTarget) return;

			puppetMaster.targetRoot.rotation = rotation;
		}

		protected override void GroundTarget(LayerMask layers) {
			if (canMoveTarget) base.GroundTarget (layers);
		}

		void OnDrawGizmosSelected() {
			for (int g = 0; g < groupOverrides.Length; g++) {
				groupOverrides[g].name = string.Empty;

				if (groupOverrides[g].groups.Length > 0) {
					for (int i = 0; i < groupOverrides[g].groups.Length; i++) {
						if (i > 0) groupOverrides[g].name += ", ";
						groupOverrides[g].name += groupOverrides[g].groups[i].ToString();
					}
				}
			}
		}
	}
}
