using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCrossFadeCameraRollData : LiveTimelineKeyCameraRollData
    {
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CrossFadeCameraRoll;
    }
}
