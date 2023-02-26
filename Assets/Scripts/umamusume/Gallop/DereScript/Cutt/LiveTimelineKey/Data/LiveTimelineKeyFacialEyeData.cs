using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyFacialEyeData : LiveTimelineKey
    {
        public int eyeRFlag;

        public int eyeLFlag;

        public int eyeSpeed;

        public const int kAttrNop = 65536;

        public const int kAttrBlink = 131072;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.FacialEye;

        public bool IsNop()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrNop);
        }

        public bool IsBlink()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrBlink);
        }
    }
}
