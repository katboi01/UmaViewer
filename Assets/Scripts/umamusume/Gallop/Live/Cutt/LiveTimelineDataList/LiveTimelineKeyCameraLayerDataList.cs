using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCameraLayerData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 offsetMaxPosition;
        public Vector3 offsetMinPosition;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraLayerDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraLayerData>
    {

    }
}
