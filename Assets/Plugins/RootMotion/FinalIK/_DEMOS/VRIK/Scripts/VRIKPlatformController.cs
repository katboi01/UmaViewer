using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos
{

    public class VRIKPlatformController : MonoBehaviour
    {

        public VRIK ik;
        public Transform trackingSpace;
        public Transform platform;
        public bool moveToPlatform = true;

        private Transform lastPlatform;
        private Vector3 lastPosition;
        private Quaternion lastRotation = Quaternion.identity;

        void LateUpdate()
        {
            if (platform != lastPlatform)
            {
                if (platform != null)
                {
                    // Moves the character to the position/rotation of the new platform
                    if (moveToPlatform)
                    {
                        lastPosition = ik.transform.position;
                        lastRotation = ik.transform.rotation;
                        ik.transform.position = platform.position;
                        ik.transform.rotation = platform.rotation;
                        trackingSpace.position = platform.position;
                        trackingSpace.rotation = platform.rotation;
                        ik.solver.AddPlatformMotion(platform.position - lastPosition, platform.rotation * Quaternion.Inverse(lastRotation), platform.position);
                    }

                    lastPosition = platform.position;
                    lastRotation = platform.rotation;
                }

                ik.transform.parent = platform;
                trackingSpace.parent = platform;

                lastPlatform = platform;
            }

            if (platform != null)
            {
                // Adding the motion of this Transform to VRIK
                ik.solver.AddPlatformMotion(platform.position - lastPosition, platform.rotation * Quaternion.Inverse(lastRotation), platform.position);

                lastRotation = platform.rotation;
                lastPosition = platform.position;
            }
        }
    }
}
