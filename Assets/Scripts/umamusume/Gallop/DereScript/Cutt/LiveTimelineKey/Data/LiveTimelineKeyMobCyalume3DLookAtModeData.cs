using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DLookAtModeData : LiveTimelineKeyWithInterpolate
    {
        public LiveMobCyalume3DLookAtMode lookAtMode = LiveMobCyalume3DLookAtMode.Locator;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DLookAtMode;
    }
}
