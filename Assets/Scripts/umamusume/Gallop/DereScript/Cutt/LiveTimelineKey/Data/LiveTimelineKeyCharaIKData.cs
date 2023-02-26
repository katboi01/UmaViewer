using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaIKData : LiveTimelineKeyWithInterpolate
    {
        public enum IKPart
        {
            Leg_L,
            Leg_R,
            Arm_L,
            Arm_R,
            Max
        }

        private const int IKPartNum = 4;

        public float[] blendRate = new float[IKPartNum];

        public bool[] enableIk = new bool[IKPartNum];
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaIK;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}

