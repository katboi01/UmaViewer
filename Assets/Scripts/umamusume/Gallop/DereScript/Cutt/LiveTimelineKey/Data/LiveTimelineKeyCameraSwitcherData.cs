using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCameraSwitcherData : LiveTimelineKey
    {
        public int cameraIndex;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CameraSwitcher;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
