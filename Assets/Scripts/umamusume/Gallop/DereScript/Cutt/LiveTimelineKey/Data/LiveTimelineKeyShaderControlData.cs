using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyShaderControlData : LiveTimelineKeyWithInterpolate
    {
        public enum eTarget
        {
            Center,
            Left1,
            Right1,
            Left2,
            Right2,
            Left3,
            Right3,
            Left4,
            Right4,
            Left5,
            Right5,
            Left6,
            Right6,
            Left7,
            Right7,
            StageGimmick
        }

        public enum eTargetFlag
        {
            Center = 1,
            Left1 = 2,
            Right1 = 4,
            Left2 = 8,
            Right2 = 0x10,
            Left3 = 0x20,
            Right3 = 0x40,
            Left4 = 0x80,
            Right4 = 0x100,
            Left5 = 0x200,
            Right5 = 0x400,
            Left6 = 0x800,
            Right6 = 0x1000,
            Left7 = 0x2000,
            Right7 = 0x4000,
            StageGimmick = 0x8000,
            MainUnit = 0x1F,
            ExtUnit = 32736,
            AllUnit = 0x7FFF
        }

        public enum eCondition
        {
            None,
            CharaId,
            DressId
        }

        public enum eBehavior
        {
            Luminous
        }

        public enum eBehaviorFlag
        {
            None = 0,
            Luminous = 1,
            Invalid = 0x40000000
        }

        public eCondition condition;

        public int conditionParam;

        public int targetFlags;

        public int behaviorFlags;

        public bool useVtxClrB = true;

        public float lerpDiffuse;

        public float lerpGradetion = 5f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.ShaderControl;

        protected int GetBehaviorFlag(eBehavior behavior)
        {
            return 1 << (int)behavior;
        }

        protected int GetTargetFlag(eTarget target)
        {
            return 1 << (int)target;
        }

        public bool CheckTargetFlag(eTarget target)
        {
            int targetFlag = GetTargetFlag(target);
            return (targetFlags & targetFlag) == targetFlag;
        }

        public void SetTargetFlag(eTarget target, bool sw)
        {
            int targetFlag = GetTargetFlag(target);
            targetFlags |= targetFlag;
            if (!sw)
            {
                targetFlags ^= targetFlag;
            }
        }

        public bool CheckBehaviorFlag(eBehavior behavior)
        {
            int behaviorFlag = GetBehaviorFlag(behavior);
            return (behaviorFlags & behaviorFlag) == behaviorFlag;
        }

        public void SetBehaviorFlag(eBehavior behavior, bool sw)
        {
            int behaviorFlag = GetBehaviorFlag(behavior);
            behaviorFlags |= behaviorFlag;
            if (!sw)
            {
                behaviorFlags ^= behaviorFlag;
            }
        }
    }
}
