using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public enum MuscleRemoveMode {
		Sever, // Severs the body part disconnecting the first joit
		Explode, // Explodes the body part disconnecting all joints
		Numb, // Removes the muscles, keeps the joints connected, but disables spring and damper forces
	}

    [System.Serializable]
    public enum MuscleDisconnectMode
    {
        Sever,
        Explode
    }
	
	// Contains high level API calls for changing the PuppetMaster's muscle structure.
	public partial class PuppetMaster: MonoBehaviour {

        #region Public API

        /// <summary>
        /// Adds a PropMuscle to the puppet at runtime. If Vector3.zero passed for additionalPinOffset, additional pin will not be added.
        /// </summary>
        public bool AddPropMuscle(ConfigurableJoint addPropMuscleTo, Vector3 position, Quaternion rotation, Vector3 additionalPinOffset, Transform targetParent = null, PuppetMasterProp initiateWithProp = null)
        {
            if (!initiated)
            {
                Debug.LogError("Can not add Prop Muscle to PuppetMaster that has not been initiated! Please use Start() instead of Awake() or PuppetMaster.OnPostInitiate delegate to call AddPropMuscle.", transform);
                return false;
            }

            if (addPropMuscleTo != null)
            {
                bool isFlat = HierarchyIsFlat();

                var addToMuscle = GetMuscle(addPropMuscleTo);
                if (addToMuscle != null)
                {
                    GameObject go = new GameObject("Prop Muscle " + addPropMuscleTo.name);
                    go.layer = addPropMuscleTo.gameObject.layer;
                    go.transform.parent = isFlat ? transform : addPropMuscleTo.transform;
                    go.transform.position = position;
                    go.transform.rotation = rotation;

                    go.AddComponent<Rigidbody>();

                    GameObject target = new GameObject("Prop Muscle Target " + addPropMuscleTo.name);
                    target.gameObject.layer = addToMuscle.target.gameObject.layer;
                    target.transform.parent = targetParent != null? targetParent: addToMuscle.target;
                    target.transform.position = go.transform.position;
                    target.transform.rotation = go.transform.rotation;

                    ConfigurableJoint joint = go.AddComponent<ConfigurableJoint>();
                    joint.xMotion = ConfigurableJointMotion.Locked;
                    joint.yMotion = ConfigurableJointMotion.Locked;
                    joint.zMotion = ConfigurableJointMotion.Locked;
                    joint.angularXMotion = ConfigurableJointMotion.Locked;
                    joint.angularYMotion = ConfigurableJointMotion.Locked;
                    joint.angularZMotion = ConfigurableJointMotion.Locked;

                    Muscle.Props props = new Muscle.Props();
                    props.group = Muscle.Group.Prop;

                    AddMuscle(joint, target.transform, addPropMuscleTo.GetComponent<Rigidbody>(), targetParent != null ? targetParent : addToMuscle.target, props, false, true);

                    muscles[muscles.Length - 1].isPropMuscle = true;

                    var propMuscle = go.AddComponent<PropMuscle>();
                    propMuscle.puppetMaster = this;
                    propMuscle.additionalPinOffset = additionalPinOffset;
                    propMuscle.currentProp = initiateWithProp;
                    if (additionalPinOffset != Vector3.zero) propMuscle.AddAdditionalPin();

                    Array.Resize(ref propMuscles, propMuscles.Length + 1);
                    propMuscles[propMuscles.Length - 1] = propMuscle;
                    propMuscle.OnInitiate();
                    
                    return true;
                }
                else
                {
                    Debug.LogError("Can't add Prop Muscle to a ConfigurableJoint that is not in the list of PuppetMaster.muscles.", transform);
                    return false;
                }
            }
            else
            {
                Debug.LogError("Please assign the ConfigurableJoint of the muscle you wish to add the Prop Muscle to.", transform);
                return false;
            }
        }

        /// <summary>
        /// Is the muscle scheduled for disconnecting?
        /// </summary>
        public bool IsDisconnecting(int muscleIndex)
        {
            return disconnectMuscleFlags[muscleIndex];
        }

        /// <summary>
        /// Is the muscle scheduled for reconnecting
        /// </summary>
        public bool IsReconnecting(int muscleIndex)
        {
            return reconnectMuscleFlags[muscleIndex];
        }

        /// <summary>
        /// Disconnects muscle from index. In Sever mode, the muscle and it's children will be disconnected in one piece (cutting limb off). In Explode mode the muscle and all it's children will be cut off. If Deactivate is true, the disconnected muscle GameObjects will be deactivated.
        /// </summary>
        public void DisconnectMuscleRecursive(int index, MuscleDisconnectMode disconnectMode = MuscleDisconnectMode.Sever, bool deactivate = false)
        {
            if (index < 0 || index >= muscles.Length)
            {
                Debug.LogError("PuppetMaster.DisconnectMuscleRecursive() called with out of range index: " + index, transform);
                return;
            }

            disconnectMuscleFlags[index] = true;
            muscleDisconnectModes[index] = disconnectMode;
            disconnectDeactivateFlags[index] = deactivate;
        }

        /// <summary>
        /// Reconnects all muscles starting from the specified index.
        /// </summary>
        public void ReconnectMuscleRecursive(int index)
        {
            if (index < 0 || index >= muscles.Length)
            {
                Debug.LogError("PuppetMaster.ReconnectMuscleRecursive() called with out of range index: " + index, transform);
                return;
            }

            if (index > 0) index = GetHighestDisconnectedParentIndex(index);

            reconnectMuscleFlags[index] = true;

            // Disable reconnecting muscles, so they could be reset on actual reconnect
            if (muscles[index].state.resetFlag) muscles[index].joint.gameObject.SetActive(false);

            for (int i = 0; i < muscles[index].childIndexes.Length; i++)
            {
                int childIndex = muscles[index].childIndexes[i];
                if (muscles[childIndex].state.resetFlag) muscles[childIndex].joint.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// NB! Make sure to call this from FixedUpdate!
        /// Creates a new muscle for the specified "joint" and targets it to the "target". The joint will be connected to the specified "connectTo" Muscle.
        /// Note that the joint will be binded to it's current position and rotation relative to the "connectTo", so make sure the added object is positioned correctly when calling this.
        /// This method allocates memory, avoid using it each frame.
        /// </summary>
        public void AddMuscle(ConfigurableJoint joint, Transform target, Rigidbody connectTo, Transform targetParent, Muscle.Props muscleProps = null, bool forceTreeHierarchy = false, bool forceLayers = true) {
			if (!CheckIfInitiated()) return;

			if (!initiated) {
				Debug.LogWarning("PuppetMaster has not been initiated.", transform);
				return;
			}
			
			if (ContainsJoint(joint)) {
				Debug.LogWarning("Joint " + joint.name + " is already used by a Muscle", transform);
				return;
			}
			
			if (target == null) {
				Debug.LogWarning("AddMuscle was called with a null 'target' reference.", transform);
				return;
			}
			
			if (connectTo == joint.GetComponent<Rigidbody>()) {
				Debug.LogWarning("ConnectTo is the joint's own Rigidbody, can not add muscle.", transform);
				return;
			}

			if (activeMode == Mode.Disabled) {
				Debug.LogWarning("Adding muscles to disabled PuppetMasters is not currently supported.", transform);
				return;
			}
			
			if (muscleProps == null) muscleProps = new Muscle.Props();
			
			Muscle muscle = new Muscle();
			muscle.props = muscleProps;
			muscle.joint = joint;
			muscle.target = target;
			muscle.joint.transform.parent = (hierarchyIsFlat || connectTo == null) && !forceTreeHierarchy? transform: connectTo.transform;

			var animationBlocker = target.GetComponent<AnimationBlocker>();
			if (animationBlocker != null) Destroy(animationBlocker);

			if (forceLayers) {
				joint.gameObject.layer = gameObject.layer; //@todo what if collider is on a child gameobject?
				target.gameObject.layer = targetRoot.gameObject.layer;
			}

			if (connectTo != null) {
				muscle.target.parent = targetParent;
				
				Vector3 relativePosition = GetMuscle(connectTo).transform.InverseTransformPoint(muscle.target.position);
				Quaternion relativeRotation = Quaternion.Inverse(GetMuscle(connectTo).transform.rotation) * muscle.target.rotation;
				
				joint.transform.position = connectTo.transform.TransformPoint(relativePosition);
				joint.transform.rotation = connectTo.transform.rotation * relativeRotation;
				
				joint.connectedBody = connectTo;

				joint.xMotion = ConfigurableJointMotion.Locked;
				joint.yMotion = ConfigurableJointMotion.Locked;
				joint.zMotion = ConfigurableJointMotion.Locked;
			}
			
			muscle.Initiate(muscles);
			
			if (connectTo != null) {
				muscle.rigidbody.velocity = connectTo.velocity;
				muscle.rigidbody.angularVelocity = connectTo.angularVelocity;
			}
			
			// Ignore internal collisions
			if (!internalCollisions) {
				for (int i = 0; i < muscles.Length; i++) {
					muscle.IgnoreInternalCollisions(muscles[i]);
				}
			}
			
			Array.Resize(ref muscles, muscles.Length + 1);
			muscles[muscles.Length - 1] = muscle;
            muscle.index = muscles.Length - 1;
			
			// Update angular limit ignoring
			muscle.IgnoreAngularLimits(!angularLimits);

			if (behaviours.Length > 0) {
				muscle.broadcaster = muscle.joint.gameObject.AddComponent<MuscleCollisionBroadcaster>();
				muscle.broadcaster.puppetMaster = this;
				muscle.broadcaster.muscleIndex = muscles.Length - 1;
			}

			muscle.jointBreakBroadcaster = muscle.joint.gameObject.AddComponent<JointBreakBroadcaster>();
			muscle.jointBreakBroadcaster.puppetMaster = this;
			muscle.jointBreakBroadcaster.muscleIndex = muscles.Length - 1;

			UpdateHierarchies();
			CheckMassVariation(100f, true);

			foreach (BehaviourBase b in behaviours) b.OnMuscleAdded(muscle);
		}

		/// <summary>
		/// Complete rebuild of this puppet's muscle hierarchy to it's default state as it was at first initiation (required that none of the original components have been destroyed).
		/// </summary>
		public void Rebuild() {
			rebuildFlag = true;
		}

        /// <summary>
        /// Removes the muscle with the specified joint and all muscles connected to it from PuppetMaster management. This will not destroy the body part/prop, but just release it from following the target.
        /// If you call RemoveMuscleRecursive on an upper arm muscle, the entire arm will be disconnected from the rest of the body.
        /// </summary>
        /// <param name="joint">The joint of the muscle (and the muscles connected to it) to remove.</param>
        /// <param name="attachTarget">If set to <c>true</c> , the target Transform of the first muscle will be parented to the disconnected limb.</param>
        /// <param name="blockTargetAnimation">If set to <c>true</c>, will add AnimationBlocker.cs to the removed target bones. That will override animation that would otherwise still be writing on those bones.</param>
        /// <param name="removeMode">Remove mode. Sever cuts the body part by disconnecting the first joint. Explode explodes the body part disconnecting all joints. Numb removes the muscles from PuppetMaster management, keeps the joints connected and disables spring and damper forces.</param>
        public void RemoveMuscleRecursive(ConfigurableJoint joint, bool attachTarget, bool blockTargetAnimation = false, MuscleRemoveMode removeMode = MuscleRemoveMode.Sever)
        {
            if (!CheckIfInitiated()) return;

            if (joint == null)
            {
                Debug.LogWarning("RemoveMuscleRecursive was called with a null 'joint' reference.", transform);
                return;
            }

            if (!ContainsJoint(joint))
            {
                Debug.LogWarning("No Muscle with the specified joint was found, can not remove muscle.", transform);
                return;
            }

            int index = GetMuscleIndex(joint);
            Muscle[] newMuscles = new Muscle[muscles.Length - (muscles[index].childIndexes.Length + 1)];

            int added = 0;
            for (int i = 0; i < muscles.Length; i++)
            {
                if (i != index && !muscles[index].childFlags[i])
                {
                    newMuscles[added] = muscles[i];
                    added++;
                }
                else
                {
                    if (muscles[i].broadcaster != null)
                    {
                        muscles[i].broadcaster.enabled = false;
                        Destroy(muscles[i].broadcaster);
                    }
                    if (muscles[i].jointBreakBroadcaster != null)
                    {
                        muscles[i].jointBreakBroadcaster.enabled = false;
                        Destroy(muscles[i].jointBreakBroadcaster);
                    }
                }
            }

            switch (removeMode)
            {
                case MuscleRemoveMode.Sever:
                    DisconnectJoint(muscles[index].joint);

                    for (int i = 0; i < muscles[index].childIndexes.Length; i++)
                    {
                        KillJoint(muscles[muscles[index].childIndexes[i]].joint);
                    }
                    break;
                case MuscleRemoveMode.Explode:
                    DisconnectJoint(muscles[index].joint);

                    for (int i = 0; i < muscles[index].childIndexes.Length; i++)
                    {
                        DisconnectJoint(muscles[muscles[index].childIndexes[i]].joint);
                    }
                    break;
                case MuscleRemoveMode.Numb:
                    KillJoint(muscles[index].joint);

                    for (int i = 0; i < muscles[index].childIndexes.Length; i++)
                    {
                        KillJoint(muscles[muscles[index].childIndexes[i]].joint);
                    }
                    break;
            }

            muscles[index].transform.parent = null;

            for (int i = 0; i < muscles[index].childIndexes.Length; i++)
            {
                if (removeMode == MuscleRemoveMode.Explode || muscles[muscles[index].childIndexes[i]].transform.parent == transform)
                {
                    muscles[muscles[index].childIndexes[i]].transform.parent = null;
                }
            }


            foreach (BehaviourBase b in behaviours)
            {
                b.OnMuscleRemoved(muscles[index]);

                for (int i = 0; i < muscles[index].childIndexes.Length; i++)
                {
                    var c = muscles[muscles[index].childIndexes[i]];
                    b.OnMuscleRemoved(c);
                }
            }

            if (attachTarget)
            {
                muscles[index].target.parent = muscles[index].transform;
                muscles[index].target.position = muscles[index].transform.position;
                muscles[index].target.rotation = muscles[index].transform.rotation * muscles[index].targetRotationRelative;

                for (int i = 0; i < muscles[index].childIndexes.Length; i++)
                {
                    var c = muscles[muscles[index].childIndexes[i]];
                    c.target.parent = c.transform;
                    c.target.position = c.transform.position;
                    c.target.rotation = c.transform.rotation;
                }
            }

            if (blockTargetAnimation)
            {
                var blocker = muscles[index].target.gameObject.GetComponent<AnimationBlocker>();
                if (blocker == null) blocker = muscles[index].target.gameObject.AddComponent<AnimationBlocker>();
                
                for (int i = 0; i < muscles[index].childIndexes.Length; i++)
                {
                    var c = muscles[muscles[index].childIndexes[i]];

                    blocker = c.target.gameObject.GetComponent<AnimationBlocker>();
                    if (blocker == null) blocker = c.target.gameObject.AddComponent<AnimationBlocker>();
                }
            }

            if (OnMuscleRemoved != null) OnMuscleRemoved(muscles[index]);
            for (int i = 0; i < muscles[index].childIndexes.Length; i++)
            {
                var c = muscles[muscles[index].childIndexes[i]];
                if (OnMuscleRemoved != null) OnMuscleRemoved(c);
            }

            // Enable collisions between the new muscles and the removed colliders
            if (!internalCollisionsEnabled)
            {
                foreach (Muscle newMuscle in newMuscles)
                {
                    newMuscle.ResetInternalCollisions(muscles[index], false);

                    for (int childMuscleIndex = 0; childMuscleIndex < muscles[index].childIndexes.Length; childMuscleIndex++)
                    {
                        newMuscle.ResetInternalCollisions(muscles[childMuscleIndex], false);
                    }
                }
            }

            muscles = newMuscles;

            UpdateHierarchies();
        }

		/// <summary>
		/// NB! Make sure to call this from FixedUpdate!
		/// Replaces a muscle with a new one. This can be used to replace props, 
		/// but in most cases it would be faster and more efficient to do it by maintaining the muscle (the Joint and the Rigidbody) and just replacing the colliders and the graphical model.
		/// This method allocates memory, avoid using it each frame.
		/// </summary>
		public void ReplaceMuscle(ConfigurableJoint oldJoint, ConfigurableJoint newJoint) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}
		
		/// <summary>
		/// NB! Make sure to call this from FixedUpdate!
		/// Completely replaces the muscle structure. Make sure the new muscle objects are positioned and rotated correctly relative to their targets.
		/// This method allocates memory, avoid using it each frame.
		/// </summary>
		public void SetMuscles(Muscle[] newMuscles) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}
		
		/// <summary>
		/// Disables the muscle with the specified joint and all muscles connected to it. This is a faster and more efficient alternative to RemoveMuscleRecursive,
		/// as it will not require reinitiating the muscles.
		/// </summary>
		public void DisableMuscleRecursive(ConfigurableJoint joint) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}
		
		/// <summary>
		/// Re-enables a previously disabled muscle and the muscles connected to it.
		/// </summary>
		public void EnableMuscleRecursive(ConfigurableJoint joint) {
			if (!CheckIfInitiated()) return;
			
			// @todo
			Debug.LogWarning("@todo", transform);
		}

		/// <summary>
		/// Flattens the ragdoll hierarchy so that all muscles are parented to the PuppetMaster.
		/// </summary>
		[ContextMenu("Flatten Muscle Hierarchy")]
		public void FlattenHierarchy() {
			foreach (Muscle m in muscles) {
				if (m.joint != null) m.joint.transform.parent = transform;
			}

			hierarchyIsFlat = true;
		}

		/// <summary>
		/// Builds a hierarchy tree from the muscles.
		/// </summary>
		[ContextMenu("Tree Muscle Hierarchy")]
		public void TreeHierarchy() {
			foreach (Muscle m in muscles) {
				if (m.joint != null) {
					m.joint.transform.parent = m.joint.connectedBody != null? m.joint.connectedBody.transform: transform;
				}
			}

			hierarchyIsFlat = false;
		}

		/// <summary>
		/// Moves all muscles to the positions of their targets.
		/// </summary>
		[ContextMenu("Fix Muscle Positions")]
		public void FixMusclePositions() {
			foreach (Muscle m in muscles) {
				if (m.joint != null && m.target != null) {
					m.joint.transform.position = m.target.position;
				}
			}
		}

        /// <summary>
		/// Moves all muscles to the positions and rotations of their targets.
		/// </summary>
		[ContextMenu("Fix Muscle Positions and Rotations")]
        public void FixMusclePositionsAndRotations()
        {
            foreach (Muscle m in muscles)
            {
                if (m.joint != null && m.target != null)
                {
                    m.joint.transform.position = m.target.position;
                    m.joint.transform.rotation = m.target.rotation;
                }
            }
        }

        /// <summary>
        /// Are all the muscles parented to the PuppetMaster Transform?
        /// </summary>
        public bool HierarchyIsFlat()
        {
            foreach (Muscle m in muscles)
            {
                if (m.joint.transform.parent != transform) return false;
            }
            return true;
        }

        #endregion Public API

        private int GetHighestDisconnectedParentIndex(int index)
        {
            for (int i = muscles[index].parentIndexes.Length - 1; i > -1; i--)
            {
                int parentIndex = muscles[index].parentIndexes[i];
                if (muscles[parentIndex].state.isDisconnected) return parentIndex;
            }

            return index;
        }

        private void ProcessDisconnects()
        {
            for (int i = 0; i < disconnectMuscleFlags.Length; i++)
            {
                if (disconnectMuscleFlags[i]) OnDisconnectMuscleRecursive(i, muscleDisconnectModes[i], disconnectDeactivateFlags[i]);
            }

            for (int i = 0; i < disconnectMuscleFlags.Length; i++)
            {
                disconnectMuscleFlags[i] = false;
                disconnectDeactivateFlags[i] = false;
            }
        }

        private void ProcessReconnects()
        {
            for (int i = 0; i < reconnectMuscleFlags.Length; i++)
            {
                if (reconnectMuscleFlags[i]) OnReconnectMuscleRecursive(i);
            }

            for (int i = 0; i < reconnectMuscleFlags.Length; i++)
            {
                reconnectMuscleFlags[i] = false;
            }
        }

        private void OnDisconnectMuscleRecursive(int index, MuscleDisconnectMode disconnectMode = MuscleDisconnectMode.Sever, bool deactivate = false)
        {
            // Reset flags
            if (!muscles[index].joint.gameObject.activeInHierarchy || deactivate) muscles[index].state.resetFlag = true;
            for (int i = 0; i < muscles[index].childIndexes.Length; i++)
            {
                int childIndex = muscles[index].childIndexes[i];
                if (!muscles[childIndex].joint.gameObject.activeInHierarchy || deactivate) muscles[childIndex].state.resetFlag = true;
            }

            // Disconnect individual muscles
            DisconnectMuscle(muscles[index], true, deactivate);

            for (int i = 0; i < muscles[index].childIndexes.Length; i++)
            {
                int childIndex = muscles[index].childIndexes[i];
                bool alreadyDone = disconnectMode == MuscleDisconnectMode.Sever && muscles[childIndex].state.isDisconnected;
                if (disconnectMode == MuscleDisconnectMode.Explode && muscles[childIndex].joint.xMotion != ConfigurableJointMotion.Free) alreadyDone = false;

                if (!alreadyDone)
                {
                    DisconnectMuscle(muscles[childIndex], disconnectMode == MuscleDisconnectMode.Explode, deactivate);
                }
            }

            // Disconnect the last muscle too if all others removed
            if (!muscles[0].state.isDisconnected)
            {
                bool lastMuscle = true;

                for (int i = 1; i < muscles.Length; i++)
                {
                    if (!muscles[i].state.isDisconnected)
                    {
                        lastMuscle = false;
                        break;
                    }

                    if (lastMuscle)
                    {
                        DisconnectMuscleRecursive(0, MuscleDisconnectMode.Sever);
                    }
                }
            }
        }

        private void DisconnectMuscle(Muscle m, bool sever, bool deactivate)
        {
            m.state.pinWeightMlp = 0f;
            m.state.muscleWeightMlp = 0f;
            m.state.muscleDamperAdd = 0f;
            m.state.muscleDamperMlp = 0f;
            m.state.mappingWeightMlp = 0f;
            m.state.maxForceMlp = 0f;
            m.state.immunity = 0f;
            m.state.impulseMlp = 1f;

            if (sever)
            {
                m.joint.xMotion = ConfigurableJointMotion.Free;
                m.joint.yMotion = ConfigurableJointMotion.Free;
                m.joint.zMotion = ConfigurableJointMotion.Free;

                m.IgnoreAngularLimits(true);

                if (!hierarchyIsFlat)
                {
                    m.joint.transform.parent = transform;
                }
            }
            else
            {
                m.IgnoreAngularLimits(false);
            }

            bool applyMappedVelocity = !m.joint.gameObject.activeInHierarchy || m.rigidbody.isKinematic;
            if (activeState == State.Frozen) applyMappedVelocity = false;

            // In case disconnecting in disabled mode
            if (!m.joint.gameObject.activeInHierarchy && !deactivate)
            {
                m.MoveToTarget();
                m.joint.gameObject.SetActive(true);
            }

            m.SetKinematic(false);
            JointDrive slerpDrive = new JointDrive();
            slerpDrive.positionSpring = 0f;
            slerpDrive.maximumForce = 0f;
            slerpDrive.positionDamper = 0f;
            m.joint.slerpDrive = slerpDrive;

            // Enable internal collisions with the disconnected muscle
            if (!deactivate)
            {
                for (int i = 0; i < muscles.Length; i++)
                {
                    if (muscles[i] != m && !muscles[i].state.isDisconnected)
                    {
                        foreach (Collider c1 in m.colliders)
                        {
                            foreach (Collider c2 in muscles[i].colliders)
                            {
                                if (c1.enabled && c2.enabled) Physics.IgnoreCollision(c1, c2, false);
                            }
                        }
                    }
                }

                if (applyMappedVelocity)
                {
                    m.rigidbody.velocity = m.mappedVelocity;
                    m.rigidbody.angularVelocity = m.mappedAngularVelocity;
                }
            }
            else
            {
                m.joint.gameObject.SetActive(false);
            }

            if (m.isPropMuscle)
            {
                var propMuscle = m.joint.GetComponent<PropMuscle>();
                if (propMuscle.activeProp != null) propMuscle.currentProp = null;
            }

            m.state.isDisconnected = true;

            foreach (BehaviourBase b in behaviours) b.OnMuscleDisconnected(m);
            if (OnMuscleDisconnected != null) OnMuscleDisconnected(m);
        }

        private void OnReconnectMuscleRecursive(int index)
        {
            // Special case for reconnecting the entire puppet
            if (index == 0)
            {
                state = State.Alive;

                foreach (Muscle m in muscles)
                {
                    m.state.isDisconnected = false;
                    m.FixTargetTransforms();
                }

                foreach (Muscle m in muscles)
                {
                    m.Reset();
                    m.Read();
                    m.ClearVelocities();
                }
            }

            ReconnectMuscle(muscles[index]);

            for (int i = 0; i < muscles[index].childIndexes.Length; i++)
            {
                int childIndex = muscles[index].childIndexes[i];
                ReconnectMuscle(muscles[childIndex]);
            }
        }

        private void ReconnectMuscle(Muscle m)
        {
            m.state.isDisconnected = false;

            if (activeState != State.Frozen && !m.isPropMuscle)
            {
                m.target.position = m.targetAnimatedPosition;
                m.target.rotation = m.targetAnimatedWorldRotation;
            }

            if (m != muscles[0])
            {
                m.joint.xMotion = ConfigurableJointMotion.Locked;
                m.joint.yMotion = ConfigurableJointMotion.Locked;
                m.joint.zMotion = ConfigurableJointMotion.Locked;

                if (!hierarchyIsFlat && m.joint.connectedBody != null)
                {
                    m.transform.parent = m.joint.connectedBody.transform;
                }
            }

            bool disable = false;
            if (m.joint.connectedBody != null && !m.joint.connectedBody.gameObject.activeInHierarchy) disable = true;
            if (m.joint.connectedBody == null)
            {
                if (activeMode == Mode.Disabled || activeState == State.Frozen) disable = true;
            }

            if (disable)
            {
                m.joint.gameObject.SetActive(false);
            }
            else
            {
                if (!m.joint.gameObject.activeInHierarchy || m.state.resetFlag)
                {
                    m.Reset();
                    m.joint.gameObject.SetActive(true);
                }
                else
                {
                    if (activeState != State.Frozen) m.MoveToTarget();
                }
            }

            if (activeMode == Mode.Kinematic) m.SetKinematic(true);

            if (activeState == State.Dead)
            {
                m.ResetTargetLocalPosition();
                m.SetMuscleRotation(muscleWeight * stateSettings.deadMuscleWeight, muscleSpring, muscleDamper + stateSettings.deadMuscleDamper);
            }

            m.state.resetFlag = false;
            m.ClearVelocities();

            m.state.pinWeightMlp = 1;
            m.state.muscleWeightMlp = 1;
            m.state.muscleDamperMlp = 1;
            m.state.maxForceMlp = 1;
            m.state.mappingWeightMlp = 1f;

            UpdateInternalCollisions(m);
            m.IgnoreAngularLimits(!angularLimits);

            foreach (BehaviourBase b in behaviours) b.OnMuscleReconnected(m);
            if (OnMuscleReconnected != null) OnMuscleReconnected(m);
        }

        private void AddIndexesRecursive(int index, ref int[] indexes) {
			int l = indexes.Length;
			Array.Resize(ref indexes, indexes.Length + 1 + muscles[index].childIndexes.Length);
			indexes[l] = index;
			
			if (muscles[index].childIndexes.Length == 0) return;
			
			for (int i = 0; i < muscles[index].childIndexes.Length; i++) {
				AddIndexesRecursive(muscles[index].childIndexes[i], ref indexes);
			}
		}

		// Disconnects a joint without destroying it
		private void DisconnectJoint(ConfigurableJoint joint) {
			if (mode == Mode.Disabled) joint.gameObject.SetActive(true); 

			joint.connectedBody = null;
			
			KillJoint(joint);

			joint.xMotion = ConfigurableJointMotion.Free;
			joint.yMotion = ConfigurableJointMotion.Free;
			joint.zMotion = ConfigurableJointMotion.Free;
			joint.angularXMotion = ConfigurableJointMotion.Free;
			joint.angularYMotion = ConfigurableJointMotion.Free;
			joint.angularZMotion = ConfigurableJointMotion.Free;
		}

		// Disables joint target rotation, position spring and damper
		private void KillJoint(ConfigurableJoint joint) {
			joint.targetRotation = Quaternion.identity;
			JointDrive j = new JointDrive();
			j.positionSpring = 0f;
			j.positionDamper = 0f;
			
			#if UNITY_5_2
			j.mode = JointDriveMode.None;
			#endif
			
			joint.slerpDrive = j;
		}
	}
}
