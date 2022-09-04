using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos
{

    public class SkeletonDisconnector : MonoBehaviour
    {

        public BehaviourPuppet puppet;
        public Skeleton skeleton;
        public MuscleDisconnectMode disconnectMuscleMode;
        public LayerMask layers;
        public float unpin = 10f;
        public float force = 10f;
        public ParticleSystem particles;

        public PropMuscle propMuscle;
        public PuppetMasterProp prop;

        // Update is called once per frame
        void Update()
        {
            // Switching modes
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (disconnectMuscleMode == MuscleDisconnectMode.Sever) disconnectMuscleMode = MuscleDisconnectMode.Explode;
                else disconnectMuscleMode = MuscleDisconnectMode.Sever;
            }

            // Pick up prop
            if (Input.GetKeyDown(KeyCode.P))
            {
                propMuscle.currentProp = prop;

                // If skeleton is dead, need to resurrect it, as attaching prop also reconnects all parent muscles
                if (puppet.puppetMaster.muscles[0].state.isDisconnected) skeleton.OnRebuild();
            }

            // Drop prop
            if (Input.GetKeyDown(KeyCode.D))
            {
                propMuscle.currentProp = null;
            }

            // Shooting
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Raycast to find a ragdoll collider
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit, 100f, layers))
                {
                    var broadcaster = hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();

                    // If is a muscle...
                    if (broadcaster != null)
                    {
                        broadcaster.Hit(unpin, ray.direction * force, hit.point);

                        // Remove the muscle and its children
                        broadcaster.puppetMaster.DisconnectMuscleRecursive(broadcaster.muscleIndex, disconnectMuscleMode);
                    }
                    else
                    {
                        // Add force
                        hit.collider.attachedRigidbody.AddForceAtPosition(ray.direction * force, hit.point);
                    }

                    // Particle FX
                    particles.transform.position = hit.point;
                    particles.transform.rotation = Quaternion.LookRotation(-ray.direction);
                    particles.Emit(5);
                }
            }

            // Reattach all the missing muscles
            if (Input.GetKeyDown(KeyCode.R))
            {
                puppet.puppetMaster.ReconnectMuscleRecursive(0);
                skeleton.OnRebuild();
            }
        }
    }
}