using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyVolumeLightData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 sunPosition;
        public Color color1;
        public float power;
        public float komorebi;
        public float blurRadius;
        public bool enable;
        public bool isEnabledBorderClear;
    }

    [System.Serializable]
    public class LiveTimelineKeyVolumeLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyVolumeLightData>
    {

    }

    [System.Serializable]
    public class LiveTimelineVolumeLightData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyVolumeLightDataList keys;
    }
}
