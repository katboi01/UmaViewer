using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineEffectData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Effect";

        public LiveTimelineKeyEffectDataList keys = new LiveTimelineKeyEffectDataList();

        [NonSerialized]
        public int updatedKeyFrame;

        public LiveTimelineEffectData() : base("Effect")
        {
        }

        public override ILiveTimelineKeyDataList GetKeyList()
        {
            return keys;
        }
    }
}