using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos
{
    public class NavMeshPuppet : MonoBehaviour
    {
        public BehaviourPuppet puppet;
        public UnityEngine.AI.NavMeshAgent agent;
        public Transform target;
        public Animator animator;

        void Update()
        {
            // Keep the agent disabled while the puppet is unbalanced.
            agent.enabled = puppet.state == BehaviourPuppet.State.Puppet;

            // Update agent destination and Animator
            if (agent.enabled)
            {
                agent.SetDestination(target.position);

                animator.SetFloat("Forward", agent.velocity.magnitude * 0.25f);
            }
        }
    }
}
