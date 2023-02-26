using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DLightingData : LiveTimelineKeyWithInterpolate
    {
        public const float DEFAULT_GRADIATION = 0.05f;

        public const float DEFAULT_RIMLIGHT = 6f;

        public const float DEFAULT_BLEND_RANGE = 20f;

        public float gradiation = 0.05f;

        public float rimlight = 6f;

        public float blendRange = 20f;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DLighting;
    }
}
