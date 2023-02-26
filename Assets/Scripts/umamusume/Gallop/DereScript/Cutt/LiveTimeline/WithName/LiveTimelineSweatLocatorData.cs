using System;
using System.Collections.Generic;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineSweatLocatorData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "SweatLocator";

        public LiveTimelineKeySweatLocatorDataList keys = new LiveTimelineKeySweatLocatorDataList();

        [NonSerialized]
        public List<int> randomVisibleIndex = new List<int>();

        public LiveTimelineSweatLocatorData() : base("SweatLocator") { }

        public override ILiveTimelineKeyDataList GetKeyList() { return keys; }
    }
}
