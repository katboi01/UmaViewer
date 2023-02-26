using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyStageEnvironmentData : LiveTimelineKey
    {
        public LiveCharaPositionFlag characterShadow = LiveCharaPositionFlag_Helper.Default5;

        public bool mirror = true;

        public float mirrorReflectionRate = 1f;

        public bool softShadow;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Environment;
    }
}
