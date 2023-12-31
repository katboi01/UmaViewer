using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace RootMotion
{
    public class TQ
    {
        public TQ(Vector3 translation, Quaternion rotation)
        {
            t = translation;
            q = rotation;
        }

        public Vector3 t;
        public Quaternion q;
    }

    /*
    Written with the kind help of the one commonly known as Mecanim-Dev.
    */
    public class AvatarUtility
    {

        public static Quaternion GetPostRotation(Avatar avatar, AvatarIKGoal avatarIKGoal)
        {
            int humanId = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
            if (humanId == (int)HumanBodyBones.LastBone) throw new InvalidOperationException("Invalid human id.");

            MethodInfo methodGetPostRotation = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetPostRotation == null) throw new InvalidOperationException("Cannot find GetPostRotation method.");

            return (Quaternion)methodGetPostRotation.Invoke(avatar, new object[] { humanId });
        }

        /// <summary>
        /// Get IK position and rotation for foot/hand bone position/rotation.
        /// </summary>
        public static TQ GetIKGoalTQ(Avatar avatar, float humanScale, AvatarIKGoal avatarIKGoal, TQ bodyPositionRotation, TQ boneTQ)
        {
            int humanId = (int)HumanIDFromAvatarIKGoal(avatarIKGoal);
            if (humanId == (int)HumanBodyBones.LastBone) throw new InvalidOperationException("Invalid human id.");

            MethodInfo methodGetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetAxisLength == null) throw new InvalidOperationException("Cannot find GetAxisLength method.");

            MethodInfo methodGetPostRotation = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetPostRotation == null) throw new InvalidOperationException("Cannot find GetPostRotation method.");

            Quaternion postRotation = (Quaternion)methodGetPostRotation.Invoke(avatar, new object[] { humanId });

            var goalTQ = new TQ(boneTQ.t, boneTQ.q * postRotation);


            if (avatarIKGoal == AvatarIKGoal.LeftFoot || avatarIKGoal == AvatarIKGoal.RightFoot)
            {
                // Here you could use animator.leftFeetBottomHeight or animator.rightFeetBottomHeight rather than GetAxisLenght
                // Both are equivalent but GetAxisLength is the generic way and work for all human bone
                float axislength = (float)methodGetAxisLength.Invoke(avatar, new object[] { humanId });
                Vector3 footBottom = new Vector3(axislength, 0, 0);
                goalTQ.t += (goalTQ.q * footBottom);
            }

            // IK goal are in avatar body local space
            Quaternion invRootQ = Quaternion.Inverse(bodyPositionRotation.q);
            goalTQ.t = invRootQ * (goalTQ.t - bodyPositionRotation.t);
            goalTQ.q = invRootQ * goalTQ.q;
            goalTQ.t /= humanScale;

            goalTQ.q = Quaternion.LookRotation(goalTQ.q * Vector3.forward, goalTQ.q * Vector3.up);

            return goalTQ;
        }

        public static HumanBodyBones HumanIDFromAvatarIKGoal(AvatarIKGoal avatarIKGoal)
        {
            HumanBodyBones humanId = HumanBodyBones.LastBone;
            switch (avatarIKGoal)
            {
                case AvatarIKGoal.LeftFoot: humanId = HumanBodyBones.LeftFoot; break;
                case AvatarIKGoal.RightFoot: humanId = HumanBodyBones.RightFoot; break;
                case AvatarIKGoal.LeftHand: humanId = HumanBodyBones.LeftHand; break;
                case AvatarIKGoal.RightHand: humanId = HumanBodyBones.RightHand; break;
            }
            return humanId;
        }
    }
}