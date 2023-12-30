using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyAnimationData : LiveTimelineKeyWithInterpolate
    {
        public int animationID;
        public WrapMode wrapMode;
        public float speed;
        public float offsetTime;
    }

    [System.Serializable]
    public class LiveTimelineKeyAnimationDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyAnimationData>
    {

    }


    [System.Serializable]
    public class LiveTimelineAnimationData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyAnimationDataList keys;
    }
}