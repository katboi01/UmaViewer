using System;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [Serializable]
    public class LiveTimelineKeyTransformDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTransformData>
    {

    }

    [Serializable]
    public class LiveTimelineKeyTransformData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 position;
        public Vector3 rotate;
        public Vector3 scale;
     }

    [Serializable]
    public class LiveTimelineTransformData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Transform";
        public LiveTimelineKeyTransformDataList keys;
        public bool enablePosition;
        public bool enableRotate;
        public bool enableScale;
    }
}

