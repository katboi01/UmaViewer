using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaMotionNoiseData : LiveTimelineKey
    {
        private const int kAttrDisable = 65536;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaMotionNoise;

        public bool IsNoiseDisable()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDisable);
        }
    }
}
