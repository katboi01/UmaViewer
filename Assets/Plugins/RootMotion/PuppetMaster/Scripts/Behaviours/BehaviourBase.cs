using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// The base abstract class for all Puppet Behaviours.
    /// </summary>
    public abstract class BehaviourBase : MonoBehaviour
    {

        /// <summary>
        /// Gets the PuppetMaster associated with this behaviour. Returns null while the behaviour is not initiated by the PuppetMaster.
        /// </summary>
        [HideInInspector] public PuppetMaster puppetMaster;

        public delegate void BehaviourDelegate();
        public delegate void HitDelegate(MuscleHit hit);
        public delegate void CollisionDelegate(MuscleCollision collision);

        public abstract void OnReactivate();

        public BehaviourDelegate OnPreActivate;
        public BehaviourDelegate OnPreInitiate;
        public BehaviourDelegate OnPreFixedUpdate;
        public BehaviourDelegate OnPreUpdate;
        public BehaviourDelegate OnPreLateUpdate;
        //public BehaviourDelegate OnPreDisable;
        public BehaviourDelegate OnPreDeactivate;
        public BehaviourDelegate OnPreFixTransforms;
        public BehaviourDelegate OnPreRead;
        public BehaviourDelegate OnPreWrite;
        public HitDelegate OnPreMuscleHit;
        public CollisionDelegate OnPreMuscleCollision;
        public CollisionDelegate OnPreMuscleCollisionExit;
        public BehaviourDelegate OnHierarchyChanged;

        public virtual void Resurrect() { }
        public virtual void Freeze() { }
        public virtual void Unfreeze() { }
        public virtual void KillStart() { }
        public virtual void KillEnd() { }
        public virtual void OnTeleport(Quaternion deltaRotation, Vector3 deltaPosition, Vector3 pivot, bool moveToTarget) { }
        public virtual void OnMuscleDisconnected(Muscle m) { }
        public virtual void OnMuscleReconnected(Muscle m) { }

        public virtual void OnMuscleAdded(Muscle m)
        {
            if (OnHierarchyChanged != null) OnHierarchyChanged();
        }

        public virtual void OnMuscleRemoved(Muscle m)
        {
            if (OnHierarchyChanged != null) OnHierarchyChanged();
        }

        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }
        protected virtual void OnInitiate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        //protected virtual void OnDisableBehaviour() {}
        protected virtual void OnDrawGizmosBehaviour() { }
        protected virtual void OnFixTransformsBehaviour() { }
        protected virtual void OnReadBehaviour() { }
        protected virtual void OnWriteBehaviour() { }
        protected virtual void OnMuscleHitBehaviour(MuscleHit hit) { }
        protected virtual void OnMuscleCollisionBehaviour(MuscleCollision collision) { }
        protected virtual void OnMuscleCollisionExitBehaviour(MuscleCollision collision) { }
        
        public BehaviourDelegate OnPostActivate;
        public BehaviourDelegate OnPostInitiate;
        public BehaviourDelegate OnPostFixedUpdate;
        public BehaviourDelegate OnPostUpdate;
        public BehaviourDelegate OnPostLateUpdate;
        //public BehaviourDelegate OnPostDisable;
        public BehaviourDelegate OnPostDeactivate;
        public BehaviourDelegate OnPostDrawGizmos;
        public BehaviourDelegate OnPostFixTransforms;
        public BehaviourDelegate OnPostRead;
        public BehaviourDelegate OnPostWrite;
        public HitDelegate OnPostMuscleHit;
        public CollisionDelegate OnPostMuscleCollision;
        public CollisionDelegate OnPostMuscleCollisionExit;

        [HideInInspector] public bool deactivated;
        public bool forceActive { get; protected set; }

        private bool initiated = false;

        public void Initiate()
        {
            initiated = true;

            if (OnPreInitiate != null) OnPreInitiate();

            OnInitiate();

            if (OnPostInitiate != null) OnPostInitiate();
        }

        public void OnFixTransforms()
        {
            if (!initiated) return;
            if (!enabled) return;

            if (OnPreFixTransforms != null) OnPreFixTransforms();

            OnFixTransformsBehaviour();

            if (OnPostFixTransforms != null) OnPostFixTransforms();
        }

        public void OnRead()
        {
            if (!initiated) return;
            if (!enabled) return;

            if (OnPreRead != null) OnPreRead();

            OnReadBehaviour();

            if (OnPostRead != null) OnPostRead();
        }

        public void OnWrite()
        {
            if (!initiated) return;
            if (!enabled) return;

            if (OnPreWrite != null) OnPreWrite();

            OnWriteBehaviour();

            if (OnPostWrite != null) OnPostWrite();
        }

        public void OnMuscleHit(MuscleHit hit)
        {
            if (!initiated) return;
            if (OnPreMuscleHit != null) OnPreMuscleHit(hit);

            OnMuscleHitBehaviour(hit);

            if (OnPostMuscleHit != null) OnPostMuscleHit(hit);
        }

        public void OnMuscleCollision(MuscleCollision collision)
        {
            if (!initiated) return;
            if (OnPreMuscleCollision != null) OnPreMuscleCollision(collision);

            OnMuscleCollisionBehaviour(collision);

            if (OnPostMuscleCollision != null) OnPostMuscleCollision(collision);
        }

        public void OnMuscleCollisionExit(MuscleCollision collision)
        {
            if (!initiated) return;
            if (OnPreMuscleCollisionExit != null) OnPreMuscleCollisionExit(collision);

            OnMuscleCollisionExitBehaviour(collision);

            if (OnPostMuscleCollisionExit != null) OnPostMuscleCollisionExit(collision);
        }

        void OnEnable()
        {
            if (!initiated)
            {
                // Discarding Unity's initial OnEnable call, because the starting behaviour will be determined by PuppetMaster
                return;
            }

            Activate();
        }

        public void Activate()
        {
            foreach (BehaviourBase b in puppetMaster.behaviours)
            {
                b.enabled = b == this;
            }

            if (OnPreActivate != null) OnPreActivate();

            OnActivate();

            if (OnPostActivate != null) OnPostActivate();
        }

        void OnDisable()
        {
            if (!initiated) return;
            if (OnPreDeactivate != null) OnPreDeactivate();

            OnDeactivate();

            if (OnPostDeactivate != null) OnPostDeactivate();
        }

        public void FixedUpdateB()
        {
            if (!initiated) return;
            if (!enabled) return;
            if (puppetMaster.muscles.Length <= 0) return;

            if (OnPreFixedUpdate != null && enabled) OnPreFixedUpdate();

            OnFixedUpdate();

            if (OnPostFixedUpdate != null && enabled) OnPostFixedUpdate();
        }

        public void UpdateB()
        {
            if (!initiated) return;
            if (!enabled) return;
            if (puppetMaster.muscles.Length <= 0) return;

            if (OnPreUpdate != null && enabled) OnPreUpdate();

            OnUpdate();

            if (OnPostUpdate != null && enabled) OnPostUpdate();
        }

        public void LateUpdateB()
        {
            if (!initiated) return;
            if (!enabled) return;
            if (puppetMaster.muscles.Length <= 0) return;

            if (OnPreLateUpdate != null && enabled) OnPreLateUpdate();

            OnLateUpdate();

            if (OnPostLateUpdate != null && enabled) OnPostLateUpdate();
        }

        protected virtual void OnDrawGizmos()
        {
            if (!initiated) return;
            OnDrawGizmosBehaviour();

            if (OnPostDrawGizmos != null) OnPostDrawGizmos();
        }

        /// <summary>
        /// Defines actions taken on certain events defined by the Puppet Behaviours.
        /// </summary>
        [System.Serializable]
        public struct PuppetEvent
        {
            [TooltipAttribute("Another Puppet Behaviour to switch to on this event. This must be the exact Type of the the Behaviour, careful with spelling.")]
            /// <summary>
            /// Another Puppet Behaviour to switch to on this event. This must be the exact Type of the the Behaviour, careful with spelling.
            /// </summary>
            public string switchToBehaviour;

            [TooltipAttribute("Animations to cross-fade to on this event. This is separate from the UnityEvent below because UnityEvents can't handle calls with more than one parameter such as Animator.CrossFade.")]
            /// <summary>
            /// Animations to cross-fade to on this event. This is separate from the UnityEvent below because UnityEvents can't handle calls with more than one parameter such as Animator.CrossFade.
            /// </summary>
            public AnimatorEvent[] animations;

            [TooltipAttribute("The UnityEvent to invoke on this event.")]
            /// <summary>
            /// The UnityEvent to invoke on this event.
            /// </summary>
            public UnityEvent unityEvent;

            public bool switchBehaviour
            {
                get
                {
                    return switchToBehaviour != string.Empty && switchToBehaviour != empty;
                }
            }
            private const string empty = "";

            public void Trigger(PuppetMaster puppetMaster, bool switchBehaviourEnabled = true)
            {
                unityEvent.Invoke();
                foreach (AnimatorEvent animatorEvent in animations) animatorEvent.Activate(puppetMaster.targetAnimator, puppetMaster.targetAnimation);

                if (switchBehaviour)
                {
                    bool found = false;

                    foreach (BehaviourBase behaviour in puppetMaster.behaviours)
                    {
                        //if (behaviour != null && behaviour.GetType() == System.Type.GetType(switchToBehaviour)) {
                        if (behaviour != null && behaviour.GetType().ToString() == "RootMotion.Dynamics." + switchToBehaviour)
                        {
                            found = true;
                            //behaviour.Activate();
                            behaviour.enabled = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Debug.LogWarning("No Puppet Behaviour of type '" + switchToBehaviour + "' was found. Can not switch to the behaviour, please check the spelling (also for empty spaces).");
                    }
                }
            }
        }

        /// <summary>
        /// Cross-fades to an animation state. UnityEvent can not be used for cross-fading, it requires multiple parameters.
        /// </summary>
        [System.Serializable]
        public class AnimatorEvent
        {

            /// <summary>
            /// The name of the animation state
            /// </summary>
            public string animationState;
            /// <summary>
            /// The crossfading time
            /// </summary>
            public float crossfadeTime = 0.3f;
            /// <summary>
            /// The layer of the animation state (if using Legacy, the animation state will be forced to this layer)
            /// </summary>
            public int layer;
            /// <summary>
            ///  Should the animation always start from 0 normalized time?
            /// </summary>
            public bool resetNormalizedTime;

            private const string empty = "";

            // Activate the animation
            public void Activate(Animator animator, Animation animation)
            {
                if (animator != null) Activate(animator);
                if (animation != null) Activate(animation);
            }

            // Activate a Mecanim animation
            private void Activate(Animator animator)
            {
                if (animationState == empty) return;

                if (resetNormalizedTime)
                {
                    if (crossfadeTime > 0f) animator.CrossFadeInFixedTime(animationState, crossfadeTime, layer, 0f);
                    else animator.Play(animationState, layer, 0f);
                }
                else
                {
                    if (crossfadeTime > 0f)
                    {
                        animator.CrossFadeInFixedTime(animationState, crossfadeTime, layer);
                    }
                    else animator.Play(animationState, layer);
                }
            }

            // Activate a Legacy animation
            private void Activate(Animation animation)
            {
                if (animationState == empty) return;

                if (resetNormalizedTime) animation[animationState].normalizedTime = 0f;

                animation[animationState].layer = layer;

                animation.CrossFade(animationState, crossfadeTime);
            }
        }

        protected void RotateTargetToRootMuscle()
        {
            Vector3 hipsForward = Quaternion.Inverse(puppetMaster.muscles[0].target.rotation) * puppetMaster.targetRoot.forward;
            Vector3 forward = puppetMaster.muscles[0].rigidbody.rotation * hipsForward;
            forward.y = 0f;
            puppetMaster.targetRoot.rotation = Quaternion.LookRotation(forward);
        }

        protected void TranslateTargetToRootMuscle(float maintainY)
        {
            puppetMaster.muscles[0].target.position = new Vector3(
                puppetMaster.muscles[0].transform.position.x,
                Mathf.Lerp(puppetMaster.muscles[0].transform.position.y, puppetMaster.muscles[0].target.position.y, maintainY),
                puppetMaster.muscles[0].transform.position.z);
        }

        protected void RemovePropMuscles()
        {
            while (ContainsRemovablePropMuscle())
            {
                for (int i = 0; i < puppetMaster.muscles.Length; i++)
                {
                    if (puppetMaster.muscles[i].props.group == Muscle.Group.Prop && !puppetMaster.muscles[i].isPropMuscle)
                    {
                        puppetMaster.RemoveMuscleRecursive(puppetMaster.muscles[i].joint, true);
                        break;
                    }
                }
            }
        }

        protected virtual void GroundTarget(LayerMask layers)
        {
            Ray ray = new Ray(puppetMaster.targetRoot.position + puppetMaster.targetRoot.up, -puppetMaster.targetRoot.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 4f, layers))
            {
                if (!float.IsNaN(hit.point.x) && !float.IsNaN(hit.point.y) && !float.IsNaN(hit.point.z))
                {
                    puppetMaster.targetRoot.position = hit.point;
                }
                else
                {
                    Debug.LogWarning("Raycasting against a large collider has produced a NaN hit point.", transform);
                }
            }
        }

        protected bool ContainsRemovablePropMuscle()
        {
            foreach (Muscle m in puppetMaster.muscles)
            {
                if (m.props.group == Muscle.Group.Prop && !m.isPropMuscle) return true;
            }
            return false;
        }
    }
}
