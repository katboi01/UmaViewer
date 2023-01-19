using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCrossFadeCameraAlphaData : LiveTimelineKeyWithInterpolate
    {
        public float alpha = 1f;
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CrossFadeCameraAlpha;
    }
}
