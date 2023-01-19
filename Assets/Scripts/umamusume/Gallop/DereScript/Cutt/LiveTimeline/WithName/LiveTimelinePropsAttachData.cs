using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelinePropsAttachData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "PropsAttach";

        public LiveTimelineKeyPropsAttachDataList keys = new LiveTimelineKeyPropsAttachDataList();

        [NonSerialized]
        public int updatedKeyFrame = -1;

        public LiveTimelinePropsAttachData() : base("PropsAttach") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
