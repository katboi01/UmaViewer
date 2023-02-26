using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCameraRollData : LiveTimelineKeyWithInterpolate
    {
        public float degree;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CameraRoll;
    }
}
