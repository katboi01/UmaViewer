using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCameraRollData : LiveTimelineKeyWithInterpolate
    {
        public float degree;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraRollDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraRollData>
    {

    }
}
