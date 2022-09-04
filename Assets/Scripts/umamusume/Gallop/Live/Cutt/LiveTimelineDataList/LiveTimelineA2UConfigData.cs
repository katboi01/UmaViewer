using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyA2UConfigData : LiveTimelineKeyWithInterpolate
    {
        public A2U.Blend blend;
        public A2U.Order order;
        public bool enable;
    }

    [System.Serializable]
    public class LiveTimelineKeyA2UConfigDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyA2UConfigData>
    {

    }

    [System.Serializable]
    public class LiveTimelineA2UConfigData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "A2UConfig";
        public LiveTimelineKeyA2UConfigDataList keys;
    }
}
