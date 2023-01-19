using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyFacialNoiseData : LiveTimelineKey
    {
        private const int kAttrDisable = 65536;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.FacialNoise;

        public bool IsNoiseDisable()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrDisable);
        }
    }
}
