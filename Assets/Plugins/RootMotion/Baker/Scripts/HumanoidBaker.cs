using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

namespace RootMotion
{
    /// <summary>
    /// Baker for Humanoid animation.
    /// </summary>
    public class HumanoidBaker : Baker
    {
        /// <summary>
        /// Should the hand IK curves be added to the animation? Disable this if the original hand positions are not important when using the clip on another character via Humanoid retargeting.
        /// </summary>
        [Tooltip("Should the hand IK curves be added to the animation? Disable this if the original hand positions are not important when using the clip on another character via Humanoid retargeting.")]
        public bool bakeHandIK = true;

        /// <summary>
        /// Max keyframe reduction error for the Root.Q/T, LeftFoot IK and RightFoot IK channels. Having a larger error value for 'Key Reduction Error' and a smaller one for this enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.
        /// </summary>
        [Tooltip("Max keyframe reduction error for the Root.Q/T, LeftFoot IK and RightFoot IK channels. Having a larger error value for 'Key Reduction Error' and a smaller one for this enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.")]
        [Range(0f, 0.1f)] public float IKKeyReductionError;

        /// <summary>
        /// Frame rate divider for the muscle curves. If you had 'Frame Rate' set to 30, and this value set to 3, the muscle curves will be baked at 10 fps. Only the Root Q/T and Hand and Foot IK curves will be baked at 30. This enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.
        /// </summary>
        [Tooltip("Frame rate divider for the muscle curves. If you have 'Frame Rate' set to 30, and this value set to 3, the muscle curves will be baked at 10 fps. Only the Root Q/T and Hand and Foot IK curves will be baked at 30. This enables you to optimize clip data size without the floating feet effect by enabling 'Foot IK' in the Animator.")]
        [Range(1, 9)] public int muscleFrameRateDiv = 1;

        private BakerMuscle[] bakerMuscles;
        private BakerHumanoidQT rootQT;
        private BakerHumanoidQT leftFootQT;
        private BakerHumanoidQT rightFootQT;
        private BakerHumanoidQT leftHandQT;
        private BakerHumanoidQT rightHandQT;

        private float[] muscles = new float[0];
        private HumanPose pose = new HumanPose();
        private HumanPoseHandler handler;
        private Vector3 bodyPosition;
        private Quaternion bodyRotation = Quaternion.identity;
        private int mN = 0;
        private Quaternion lastBodyRotation = Quaternion.identity;

        void Awake()
        {
            animator = GetComponent<Animator>();
            director = GetComponent<PlayableDirector>();

            if (mode == Mode.AnimationStates || mode == Mode.AnimationClips)
            {
                if (animator == null || !animator.isHuman)
                {
                    Debug.LogError("HumanoidBaker GameObject does not have a Humanoid Animator component, can not bake.");
                    enabled = false;
                    return;
                }

                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            else if (mode == Mode.PlayableDirector)
            {
                if (director == null)
                {
                    Debug.LogError("HumanoidBaker GameObject does not have a PlayableDirector component, can not bake.");
                }
            }

            muscles = new float[HumanTrait.MuscleCount];
            bakerMuscles = new BakerMuscle[HumanTrait.MuscleCount];
            for (int i = 0; i < bakerMuscles.Length; i++)
            {
                bakerMuscles[i] = new BakerMuscle(i);
            }

            rootQT = new BakerHumanoidQT("Root");

            leftFootQT = new BakerHumanoidQT(animator.GetBoneTransform(HumanBodyBones.LeftFoot), AvatarIKGoal.LeftFoot, "LeftFoot");
            rightFootQT = new BakerHumanoidQT(animator.GetBoneTransform(HumanBodyBones.RightFoot), AvatarIKGoal.RightFoot, "RightFoot");
            leftHandQT = new BakerHumanoidQT(animator.GetBoneTransform(HumanBodyBones.LeftHand), AvatarIKGoal.LeftHand, "LeftHand");
            rightHandQT = new BakerHumanoidQT(animator.GetBoneTransform(HumanBodyBones.RightHand), AvatarIKGoal.RightHand, "RightHand");

            handler = new HumanPoseHandler(animator.avatar, animator.transform);
        }

        protected override Transform GetCharacterRoot()
        {
            return animator.transform;
        }

        protected override void OnStartBaking()
        {

            rootQT.Reset();
            leftFootQT.Reset();
            rightFootQT.Reset();
            leftHandQT.Reset();
            rightHandQT.Reset();

            for (int i = 0; i < bakerMuscles.Length; i++)
            {
                bakerMuscles[i].Reset();
            }

            mN = muscleFrameRateDiv;

            lastBodyRotation = Quaternion.identity;
        }

        protected override void OnSetLoopFrame(float time)
        {
            for (int i = 0; i < bakerMuscles.Length; i++) bakerMuscles[i].SetLoopFrame(time);

            rootQT.MoveLastKeyframes(time);

            leftFootQT.SetLoopFrame(time);
            rightFootQT.SetLoopFrame(time);
            leftHandQT.SetLoopFrame(time);
            rightHandQT.SetLoopFrame(time);
        }

        protected override void OnSetCurves(ref AnimationClip clip)
        {
            float length = bakerMuscles[0].curve.keys[bakerMuscles[0].curve.keys.Length - 1].time;
            float lengthMlp = mode != Mode.Realtime ? clipLength / length : 1f;

            for (int i = 0; i < bakerMuscles.Length; i++) bakerMuscles[i].SetCurves(ref clip, keyReductionError, lengthMlp);

            rootQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
            leftFootQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
            rightFootQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);

            if (bakeHandIK)
            {
                leftHandQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
                rightHandQT.SetCurves(ref clip, IKKeyReductionError, lengthMlp);
            }
        }

        protected override void OnSetKeyframes(float time, bool lastFrame)
        {
            // Skip muscle frames
            mN++;
            bool updateMuscles = true;
            if (mN < muscleFrameRateDiv && !lastFrame)
            {
                updateMuscles = false;
            }
            if (mN >= muscleFrameRateDiv) mN = 0;

            UpdateHumanPose();

            if (updateMuscles)
            {
                for (int i = 0; i < bakerMuscles.Length; i++) bakerMuscles[i].SetKeyframe(time, muscles);
            }

            rootQT.SetKeyframes(time, bodyPosition, bodyRotation);

            Vector3 bodyPositionScaled = bodyPosition * animator.humanScale;
            leftFootQT.SetIKKeyframes(time, animator.avatar, animator.humanScale, bodyPositionScaled, bodyRotation);
            rightFootQT.SetIKKeyframes(time, animator.avatar, animator.humanScale, bodyPositionScaled, bodyRotation);

            leftHandQT.SetIKKeyframes(time, animator.avatar, animator.humanScale, bodyPositionScaled, bodyRotation);
            rightHandQT.SetIKKeyframes(time, animator.avatar, animator.humanScale, bodyPositionScaled, bodyRotation);
        }

        private void UpdateHumanPose()
        {
            handler.GetHumanPose(ref pose);

            bodyPosition = pose.bodyPosition;
            bodyRotation = pose.bodyRotation;

            bodyRotation = BakerUtilities.EnsureQuaternionContinuity(lastBodyRotation, bodyRotation);
            lastBodyRotation = bodyRotation;

            for (int i = 0; i < pose.muscles.Length; i++)
            {
                muscles[i] = pose.muscles[i];
            }
        }

#if UNITY_EDITOR
        protected override void SetClipSettings(AnimationClip clip, UnityEditor.AnimationClipSettings settings)
        {
            settings.loopBlendOrientation = true;
            settings.loopBlendPositionY = true;
            settings.keepOriginalOrientation = true;
            settings.keepOriginalPositionY = true;
        }
#endif
    }
}
