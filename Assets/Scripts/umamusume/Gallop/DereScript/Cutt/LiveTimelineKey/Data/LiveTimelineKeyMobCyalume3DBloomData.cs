using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DBloomData : LiveTimelineKeyWithInterpolate
    {
        public const float DEFAULT_HORIZONTAL_OFFSET = 2.4f;

        public const float DEFAULT_VERTICAL_OFFSET = 0.6f;

        public const float DEFAULT_THRESHOLD = 1f;

        public const float DEFAULT_GROW_POWER = 1.4f;

        public float horizontalOffset = 2.4f;

        public float verticalOffset = 0.6f;

        public float threshold = 1f;

        public float growPower = 1.4f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DBloom;
    }
}
