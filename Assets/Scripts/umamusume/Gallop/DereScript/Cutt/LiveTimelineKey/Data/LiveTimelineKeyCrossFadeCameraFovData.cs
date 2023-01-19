using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCrossFadeCameraFovData : LiveTimelineKeyCameraFovData
    {
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CrossFadeCameraFov;
    }
}
