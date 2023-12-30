using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyFluctuationData : LiveTimelineKeyWithInterpolate
    {
        public bool IsEnable;
        public Vector2 MoveDirection;
        public float MovePower;
        public float Power;
        public float DepthClip;
    }

    [System.Serializable]
    public class LiveTimelineKeyFluctuationDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFluctuationData>
    {

    }
}
