using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics
{

    [CreateAssetMenu(fileName = "PuppetMaster Humanoid Config", menuName = "PuppetMaster/Humanoid Config", order = 1)]
    public class PuppetMasterHumanoidConfig : ScriptableObject
    {

        [System.Serializable]
        public class HumanoidMuscle
        {
            [SerializeField] [HideInInspector] public string name;
            public HumanBodyBones bone;
            public Muscle.Props props;
        }

        [LargeHeader("Simulation")]

        public PuppetMaster.State state;
        public PuppetMaster.StateSettings stateSettings = PuppetMaster.StateSettings.Default;
        public PuppetMaster.Mode mode;
        public float blendTime = 0.1f;
        public bool fixTargetTransforms = true;
        public int solverIterationCount = 6;
        public bool visualizeTargetPose = true;

        [LargeHeader("Master Weights")]

        [Range(0f, 1f)] public float mappingWeight = 1f;
        [Range(0f, 1f)] public float pinWeight = 1f;
        [Range(0f, 1f)] public float muscleWeight = 1f;

        [LargeHeader("Joint and Muscle Settings")]

        public float muscleSpring = 100f;
        public float muscleDamper = 0f;
        [Range(1f, 8f)] public float pinPow = 4f;
        [Range(0f, 100f)] public float pinDistanceFalloff = 5;
        public bool angularPinning;
        public bool updateJointAnchors = true;
        public bool supportTranslationAnimation;
        public bool angularLimits;
        public bool internalCollisions;

        [LargeHeader("Individual Muscle Settings")]

        public HumanoidMuscle[] muscles = new HumanoidMuscle[0];

        /// <summary>
        /// Applies this config to the specified PuppetMaster.
        /// </summary>
        /// <param name="p">P.</param>
        public void ApplyTo(PuppetMaster p)
        {
            if (p.targetRoot == null)
            {
                Debug.LogWarning("Please assign 'Target Root' for PuppetMaster using a Humanoid Config.", p.transform);
                return;
            }

            if (p.targetAnimator == null)
            {
                Debug.LogError("PuppetMaster 'Target Root' does not have an Animator component. Can not use Humanoid Config.", p.transform);
                return;
            }

            if (!p.targetAnimator.isHuman)
            {
                Debug.LogError("PuppetMaster target is not a Humanoid. Can not use Humanoid Config.", p.transform);
                return;
            }

            p.state = state;
            p.stateSettings = stateSettings;
            p.mode = mode;
            p.blendTime = blendTime;
            p.fixTargetTransforms = fixTargetTransforms;
            p.solverIterationCount = solverIterationCount;
            p.visualizeTargetPose = visualizeTargetPose;
            p.mappingWeight = mappingWeight;
            p.pinWeight = pinWeight;
            p.muscleWeight = muscleWeight;
            p.muscleSpring = muscleSpring;
            p.muscleDamper = muscleDamper;
            p.pinPow = pinPow;
            p.pinDistanceFalloff = pinDistanceFalloff;
            p.angularPinning = angularPinning;
            p.updateJointAnchors = updateJointAnchors;
            p.supportTranslationAnimation = supportTranslationAnimation;
            p.angularLimits = angularLimits;
            p.internalCollisions = internalCollisions;

            foreach (HumanoidMuscle h in muscles)
            {
                var m = GetMuscle(h.bone, p.targetAnimator, p);
                if (m != null)
                {
                    m.props.group = h.props.group;
                    m.props.mappingWeight = h.props.mappingWeight;
                    m.props.mapPosition = h.props.mapPosition;
                    m.props.muscleDamper = h.props.muscleDamper;
                    m.props.muscleWeight = h.props.muscleWeight;
                    m.props.pinWeight = h.props.pinWeight;
                }
            }
        }

        private Muscle GetMuscle(HumanBodyBones boneId, Animator animator, PuppetMaster puppetMaster)
        {
            Transform bone = animator.GetBoneTransform(boneId);
            if (bone == null) return null;

            foreach (Muscle m in puppetMaster.muscles)
            {
                if (m.target == bone) return m;
            }

            return null;
        }
    }
}