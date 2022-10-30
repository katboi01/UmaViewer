using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyTransformData : LiveTimelineKeyWithInterpolate
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
    public class LiveTimelineKeyTransformDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTransformData>
    {

    }

    [System.Serializable]
    public class LiveTimelineTransformData : ILiveTimelineGroupDataWithName
    {
        [System.Serializable]
        public class TimelineTransformUpdateDelegate
        {

        }

        public LiveTimelineKeyTransformDataList keys;
        public bool enablePosition;
        public bool enableRotate;
        public bool enableScale;
        public LiveTimelineTransformData.TimelineTransformUpdateDelegate OnUpdateTimelineTransform;
    }
}

