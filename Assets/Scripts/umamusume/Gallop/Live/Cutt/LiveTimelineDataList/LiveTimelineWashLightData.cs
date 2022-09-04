using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyWashLightData : LiveTimelineKeyWithInterpolate 
    {
        public float RaycastDistance;
        private const int ATTR_ENABLE_RAYCAST = 65536;
    }

    [System.Serializable]
    public class LiveTimelineKeyWashLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyWashLightData>
    {

    }

    [System.Serializable]
    public class LiveTimelineWashLightData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyWashLightDataList keys;
    }
}
