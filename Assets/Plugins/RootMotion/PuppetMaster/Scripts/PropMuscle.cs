using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// Muscle that PuppetMasterProps can be attached to.
    /// </summary>
    public class PropMuscle : MonoBehaviour
    {
        [SerializeField] [HideInInspector] public PuppetMaster puppetMaster;

        [Tooltip("The PuppetMasterProp currently held by this Prop Muscle. To pick up a prop, just assign it as propMuscle.currentProp. To drop, set propMuscle.currentProp to null. Replacing this value with another prop drops any previously held props.")]
        /// <summary>
        /// The PuppetMasterProp currently held by this Prop Muscle. To pick up a prop, just assign it as propMuscle.currentProp. To drop, set propMuscle.currentProp to null. Replacing this value with another prop drops any previously held props.
        /// </summary>
        public PuppetMasterProp currentProp;

        [LargeHeader("Additional Pin")]

        [Tooltip("Offset of the additional pin from this Prop Muscle in local space.")]
        /// <summary>
        /// Offset of the additional pin from this Prop Muscle in local space.
        /// </summary>
        public Vector3 additionalPinOffset = Vector3.forward;

        public Muscle muscle
        {
            get
            {
                if (_muscle == null) _muscle = puppetMaster.GetMuscle(GetComponent<ConfigurableJoint>());
                return _muscle;
            }
        }

        /// <summary>
        /// The prop that is actually connected to this PropMuscle. Null if none.
        /// </summary>
        public PuppetMasterProp activeProp { get; private set; }

        /// <summary>
        /// Delegate used when picking up and dropping props.
        /// </summary>
        /// <param name="prop"></param>
        public delegate void PropDelegate(PuppetMasterProp prop);
        
        /// <summary>
        /// Called after a prop has been picked up by this PropMuscle.
        /// </summary>
        public PropDelegate OnPickUpProp;

        /// <summary>
        /// Called after a prop is dropped by this PropMuscle.
        /// </summary>
        public PropDelegate OnDropProp;

        private Muscle _muscle;
        private PuppetMasterProp lastProp;
        private Vector3 targetDefaultLocalPos;
        private Vector3 lastAdditionalPinOffset;

        /// <summary>
        /// Adding additional pin at runtime.
        /// </summary>
        public bool AddAdditionalPin()
        {
            if (!puppetMaster.initiated)
            {
                Debug.LogError("Can not call AddAdditionalPin on an uninitiated PuppetMaster.", transform);
                return false;
            }

            if (muscle.additionalPin != null) return false;

            var addGO = new GameObject("Additional Pin");
            addGO.gameObject.layer = gameObject.layer;
            addGO.transform.parent = transform;
            addGO.transform.localPosition = additionalPinOffset;
            addGO.transform.localRotation = Quaternion.identity;
            lastAdditionalPinOffset = additionalPinOffset;

            addGO.AddComponent<Rigidbody>();

            var joint = addGO.AddComponent<ConfigurableJoint>();
            joint.connectedBody = muscle.joint.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = additionalPinOffset;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            var addTargetGO = new GameObject("Additional Pin Target");
            addTargetGO.layer = muscle.target.gameObject.layer;
            addTargetGO.transform.parent = muscle.target;
            addTargetGO.transform.position = addGO.transform.position;
            addTargetGO.transform.rotation = addGO.transform.rotation;

            muscle.additionalPin = joint;
            muscle.additionalPinTarget = addTargetGO.transform;
            muscle.InitiateAdditionalPin();

            //muscle.additionalPin.gameObject.hideFlags = HideFlags.HideInHierarchy;
            //muscle.additionalPinTarget.gameObject.hideFlags = HideFlags.HideInHierarchy;

            return true;
        }

        /// <summary>
        /// Removing additional pin at runtime
        /// </summary>
        public bool RemoveAdditionalPin()
        {
            if (!puppetMaster.initiated)
            {
                Debug.LogError("Can not call RemoveAdditionalPin on an uninitiated PuppetMaster.", transform);
                return false;
            }

            if (muscle.additionalPin == null) return false;

            //muscle.additionalPin.gameObject.hideFlags = HideFlags.None;
            //muscle.additionalPinTarget.gameObject.hideFlags = HideFlags.None;

            Destroy(muscle.additionalPin.gameObject);
            Destroy(muscle.additionalPinTarget.gameObject);

            muscle.additionalPin = null;
            muscle.additionalPinTarget = null;

            return true;
        }

        public void OnInitiate()
        {
            muscle.isPropMuscle = true;

            if (currentProp == null && activeProp == null) puppetMaster.DisconnectMuscleRecursive(muscle.index, MuscleDisconnectMode.Sever, true);
        }

        public void TakeOver()
        {
            currentProp = null;
            lastProp = null;
            activeProp = null;
            puppetMaster.DisconnectMuscleRecursive(muscle.index, MuscleDisconnectMode.Sever, true);
        }

        public void OnUpdate()
        {
            if (currentProp != lastProp && !puppetMaster.IsDisconnecting(muscle.index) && !puppetMaster.IsReconnecting(muscle.index))
            {
                // Drop any previously held props
                if (lastProp != null)
                {
                    lastProp.Drop(puppetMaster, muscle.index);

                    activeProp = null;

                    if (OnDropProp != null) OnDropProp(lastProp);
                }

                if (currentProp != null)
                {
                    // Take over if currentProp is held by anoth prop muscle
                    foreach (PropMuscle otherPropMuscle in puppetMaster.propMuscles)
                    {
                        if (otherPropMuscle != this)
                        {
                            if (otherPropMuscle.currentProp == currentProp) otherPropMuscle.TakeOver();
                        }
                    }

                    // Reconnect Prop Muscle
                    if (muscle.state.isDisconnected)
                    {
                        puppetMaster.ReconnectMuscleRecursive(muscle.index);
                    }

                    // Pick up the new prop
                    currentProp.PickUp(puppetMaster, muscle.index);
                    muscle.rigidbody.ResetInertiaTensor();
                    activeProp = currentProp;

                    if (OnPickUpProp != null) OnPickUpProp(currentProp);
                }
                else
                {
                    // If no current prop, disconnect this prop muscle.
                    puppetMaster.DisconnectMuscleRecursive(muscle.index, MuscleDisconnectMode.Sever, true);
                }

                lastProp = currentProp;
            }

            if (currentProp != null)
            {
                muscle.rigidbody.mass = currentProp.mass;

                if (muscle.additionalPin != null)
                {
                    muscle.additionalPinWeight = currentProp.additionalPinWeight;
                    muscle.additionalRigidbody.mass = currentProp.additionalPinMass;// * Mathf.Max(muscle.additionalPinWeight, currentProp.mass * 0.1f);
                    muscle.additionalRigidbody.drag = muscle.rigidbody.drag;
                    muscle.additionalRigidbody.angularDrag = muscle.rigidbody.angularDrag;
                    muscle.additionalRigidbody.useGravity = muscle.rigidbody.useGravity;

                    muscle.additionalRigidbody.inertiaTensor = Vector3.one * 0.00001f;
                    //muscle.additionalRigidbody.ResetInertiaTensor();

                    Vector3 f = additionalPinOffset + currentProp.additionalPinOffsetAdd;
                    if (lastAdditionalPinOffset != f)
                    {
                        muscle.additionalPinTarget.localPosition = f;
                        muscle.additionalPin.connectedAnchor = f;

                        lastAdditionalPinOffset = f;
                    }
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            if (muscle.target == null) return;
            //muscle.target.gameObject.hideFlags = HideFlags.None;
            muscle.target.position = transform.position;
            muscle.target.rotation = transform.rotation;

            if (muscle.additionalPin != null && muscle.additionalPinTarget != null)
            {
                //muscle.additionalPin.gameObject.hideFlags = HideFlags.None;
                //muscle.additionalPinTarget.gameObject.hideFlags = HideFlags.None;

                muscle.additionalPin.transform.localPosition = additionalPinOffset;
                muscle.additionalPin.transform.localRotation = Quaternion.identity;

                muscle.additionalPinTarget.position = muscle.additionalPin.transform.position;
                muscle.additionalPinTarget.rotation = muscle.additionalPin.transform.rotation;
            }
        }
    }
}
