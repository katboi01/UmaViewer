using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyA2UConfigData : LiveTimelineKeyWithInterpolate
    {
        public A2U.Blend blend;

        public A2U.Order order = A2U.Order.PostImageEffect;

        public bool enable = true;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.A2UConfig;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
