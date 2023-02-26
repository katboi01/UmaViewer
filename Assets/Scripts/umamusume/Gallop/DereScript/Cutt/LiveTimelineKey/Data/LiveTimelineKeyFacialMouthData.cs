using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyFacialMouthData : LiveTimelineKey, ILiveTimelineKeyHasMouthType
    {
        public const int kAttrNop = 65536;

        public int mouthFlag;

        public int mouthSizeFlag;

        public int mouthSpeed;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.FacialMouth;

        public bool IsNop()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)kAttrNop);
        }

        public LiveTimelineDefine.MouthType GetMouthType()
        {
            return (LiveTimelineDefine.MouthType)mouthFlag;
        }
    }
}
