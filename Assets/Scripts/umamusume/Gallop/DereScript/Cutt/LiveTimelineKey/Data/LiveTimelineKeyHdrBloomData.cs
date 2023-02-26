using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyHdrBloomData : LiveTimelineKeyWithInterpolate
    {
        public float intensity;

        public float blurSpread;

        public bool enable = true;
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.HdrBloom;
    }
}
