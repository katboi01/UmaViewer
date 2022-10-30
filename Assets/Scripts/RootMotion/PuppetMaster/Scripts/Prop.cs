using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Automated prop picking up/dropping for the PuppetMaster.
	/// </summary>
	public abstract class Prop : MonoBehaviour {

		#region User Interface

		[Tooltip("This has no other purpose but helping you distinguish props by PropRoot.currentProp.propType.")]
		/// <summary>
		/// This has no other purpose but helping you distinguish PropRoot.currentProp by type.
		/// </summary>
		public int propType;

		[LargeHeader("Muscle")]

		[Tooltip("The Muscle of this prop.")]
		/// <summary>
		/// The Muscle of this prop.
		/// </summary>
		public ConfigurableJoint muscle;

        /*
        [Tooltip("Ignore collisions with these bones (Humanoid only)")]
        /// <summary>
        /// Ignore collisions with these Humanoid bones. This will automatically find the Muscles for the Muscle Props Internal Collision Ignores below when the prop is picked up.
        /// </summary>
        public HumanBodyBones[] internalCollisionIgnores;
        */

        [Tooltip("The muscle properties that will be applied to the Muscle on pickup.")]
		/// <summary>
		/// The muscle properties that will be applied to the Muscle.
		/// </summary>
		public Muscle.Props muscleProps = new Muscle.Props();

		[Tooltip("If true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.")]
		/// <summary>
		/// If true, this prop's layer will be forced to PuppetMaster layer and target's layer forced to PuppetMaster's Target Root's layer when the prop is picked up.
		/// </summary>
		public bool forceLayers = true;

		[LargeHeader("Additional Pin")]

		[Tooltip("Optinal additional pin, useful for long melee weapons that would otherwise require a lot of muscle force and solver iterations to be swinged quickly. Should normally be without any colliders attached. The position of the pin, it's mass and the pin weight will effect how the prop will handle.")]
		/// <summary>
		/// Optinal additional pin, useful for long melee weapons that would otherwise require a lot of muscle force and solver iterations to be swinged quickly. Should normally be without any colliders attached. The position of the pin, it's mass and the pin weight will effect how the prop will handle.
		/// </summary>
		public ConfigurableJoint additionalPin;
		
		[Tooltip("Target Transform for the additional pin.")]
		/// <summary>
		/// Target Transform for the additional pin.
		/// </summary>
		public Transform additionalPinTarget;
		
		[Tooltip("The pin weight of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.")]
		/// <summary>
		/// The pin weight of the additional pin. Increasing this weight will make the prop follow animation better, but will increase jitter when colliding with objects.
		/// </summary>
		[Range(0f, 1f)] public float additionalPinWeight = 1f;

        [LargeHeader("Materials")]

        [Tooltip("If assigned, sets prop colliders to this PhysicMaterial when picked up.")]
        /// <summary>
        /// If assigned, sets prop colliders to this PhysicMaterial when picked up.
        /// </summary>
        public PhysicMaterial pickedUpMaterial;

        [Tooltip("If assigned, sets prop colliders to this PhysicMaterial when dropped.")]
        /// <summary>
        /// If assigned, sets prop colliders to this PhysicMaterial when dropped.
        /// </summary>
        public PhysicMaterial droppedMaterial;

        /// <summary>
        /// Is this prop picked up and connected to a PropRoot?
        /// </summary>
        public bool isPickedUp { get { return propRoot != null; }}

		/// <summary>
		/// Returns the PropRoot that this prop is connected to if it is picked up. If this returns null, the prop is not picked up.
		/// </summary>
		public PropRoot propRoot { get; private set; }

        #endregion User Interface

        // Picking up/dropping props is done by simply changing PropRoot.currentProp
        public void PickUp(PropRoot propRoot) {
			muscle.xMotion = xMotion;
			muscle.yMotion = yMotion;
			muscle.zMotion = zMotion;
			muscle.angularXMotion = angularXMotion;
			muscle.angularYMotion = angularYMotion;
			muscle.angularZMotion = angularZMotion;
			
			this.propRoot = propRoot;

			muscle.gameObject.layer = propRoot.puppetMaster.gameObject.layer;
			foreach (Collider c in colliders) {
				if (!c.isTrigger) c.gameObject.layer = muscle.gameObject.layer;
			}

            SetMaterial(pickedUpMaterial);

            OnPickUp(propRoot);
		}

		// Picking up/dropping props is done by simply changing PropRoot.currentProp
		public void Drop() {
			this.propRoot = null;

            SetMaterial(droppedMaterial);

            OnDrop();
		}

		public void StartPickedUp(PropRoot propRoot) {
			this.propRoot = propRoot;
		}

        public bool initiated { get; private set; }

        protected virtual void OnPickUp(PropRoot propRoot) {}
		protected virtual void OnDrop() {}
		protected virtual void OnStart() {}
	
		private ConfigurableJointMotion xMotion, yMotion, zMotion, angularXMotion, angularYMotion, angularZMotion;
		private Collider[] colliders = new Collider[0];

        void Start() {
            Debug.LogWarning("PropRoot and Prop system is deprecated. Please see the 'Prop' demo to learn about the new easier and much more performance-efficient PropMuscle and PuppetMasterProp system.", transform);

            if (transform.position != muscle.transform.position) {
				Debug.LogError("Prop target position must match exactly with it's muscle's position!", transform);
			}

			xMotion = muscle.xMotion;
			yMotion = muscle.yMotion;
			zMotion = muscle.zMotion;
			angularXMotion = muscle.angularXMotion;
			angularYMotion = muscle.angularYMotion;
			angularZMotion = muscle.angularZMotion;

			colliders = muscle.GetComponentsInChildren<Collider>();

			if (!isPickedUp) ReleaseJoint();

			OnStart();

            initiated = true;
		}

		private void ReleaseJoint() {
			muscle.connectedBody = null;
			muscle.targetRotation = Quaternion.identity;
			
			JointDrive j = new JointDrive();
			j.positionSpring = 0f;
			#if UNITY_5_2
			j.mode = JointDriveMode.None;
			#endif
			muscle.slerpDrive = j;
			
			muscle.xMotion = ConfigurableJointMotion.Free;
			muscle.yMotion = ConfigurableJointMotion.Free;
			muscle.zMotion = ConfigurableJointMotion.Free;
			muscle.angularXMotion = ConfigurableJointMotion.Free;
			muscle.angularYMotion = ConfigurableJointMotion.Free;
			muscle.angularZMotion = ConfigurableJointMotion.Free;
		}

        private void SetMaterial(PhysicMaterial material)
        {
            if (material == null) return;

            foreach (Collider c in colliders)
            {
                c.sharedMaterial = material;
            }
        }

        // Just making sure this GameObject's position and rotation matches with the muscle's in the Editor.
        void OnDrawGizmos() {
			if (muscle == null) return;
			if (Application.isPlaying) return;

			transform.position = muscle.transform.position;
			transform.rotation = muscle.transform.rotation;

			if (additionalPinTarget != null && additionalPin != null) {
				additionalPinTarget.position = additionalPin.transform.position;
			}

			muscleProps.group = Muscle.Group.Prop;
		}
	}
}
