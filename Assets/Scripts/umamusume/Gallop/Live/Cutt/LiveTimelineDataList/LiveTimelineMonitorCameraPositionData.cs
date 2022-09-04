using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMonitorCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public bool enable;
        public float fov;
        public float roll;
        public LiveCameraCullingLayer cullingMask;
    }

    [System.Serializable]
    public class LiveTimelineKeyMonitorCameraPositionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMonitorCameraPositionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMonitorCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "MonitorCameraPos";
        public LiveTimelineKeyMonitorCameraPositionDataList keys;
    }
}
