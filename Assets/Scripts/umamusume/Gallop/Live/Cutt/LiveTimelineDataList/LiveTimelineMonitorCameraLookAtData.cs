using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMonitorCameraLookAtData : LiveTimelineKeyCameraLookAtData 
    { 

    }

    [System.Serializable]
    public class LiveTimelineKeyMonitorCameraLookAtDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMonitorCameraLookAtData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMonitorCameraLookAtData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorCameraLookAt";
        public LiveTimelineKeyMonitorCameraLookAtDataList keys;
    }
}
