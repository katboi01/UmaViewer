using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyRipSyncData : LiveTimelineKey, ILiveTimelineKeyHasMouthType
    {
        private enum Attr
        {
            SmallMouth = 0x10000
        }

        public LiveCharaPositionFlag character = LiveCharaPositionFlag_Helper.Default5;

        public LiveTimelineDefine.MouthType vowel = LiveTimelineDefine.MouthType.N;

        public int mouthSpeed;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.LipSync;

        public bool IsSmallMouth()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)Attr.SmallMouth);
        }

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }

        public LiveTimelineDefine.MouthType GetMouthType()
        {
            return vowel;
        }
    }
}
