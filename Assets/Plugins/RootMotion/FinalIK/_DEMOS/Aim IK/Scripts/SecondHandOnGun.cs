using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos
{
    // Using LimbIK on the left arm to put the left hand back to where it was relative to the right hand before solving AimIK on the spine and right arm bones.
    public class SecondHandOnGun : MonoBehaviour
    {
        public AimIK aim;
        public LimbIK leftArmIK;
        public Transform leftHand, rightHand;

        public Vector3 leftHandPositionOffset;
        public Vector3 leftHandRotationOffset;

        private Vector3 leftHandPosRelToRight;
        private Quaternion leftHandRotRelToRight;

        void Start()
        {
            // Disable the IK components to take control over the updating of their solvers
            aim.enabled = false;
            leftArmIK.enabled = false;
        }

        void LateUpdate()
        {
            // Store the position and rotation of the left hand relative to the right as animated
            leftHandPosRelToRight = rightHand.InverseTransformPoint(leftHand.position);
            leftHandRotRelToRight = Quaternion.Inverse(rightHand.rotation) * leftHand.rotation;

            // Update AimIK
            aim.solver.Update();

            // AimIK has moved the right arm, so the hand hand needs to be put back to how it was relative to the right hand (the inverse of the above)
            leftArmIK.solver.IKPosition = rightHand.TransformPoint(leftHandPosRelToRight + leftHandPositionOffset);
            leftArmIK.solver.IKRotation = rightHand.rotation * Quaternion.Euler(leftHandRotationOffset) * leftHandRotRelToRight;

            // Update Left arm IK
            leftArmIK.solver.Update();
        }
    }
}
