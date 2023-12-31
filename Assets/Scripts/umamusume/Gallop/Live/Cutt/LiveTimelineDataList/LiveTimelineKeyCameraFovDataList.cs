using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveCameraFovType
    {
        Direct = 0
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraFovData : LiveTimelineKeyWithInterpolate
    {
        public LiveCameraFovType fovType;
        public float fov;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraFovDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraFovData>
    {

    }
}
