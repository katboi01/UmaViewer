using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyFadeData : LiveTimelineKeyWithInterpolate
    {
        public Color fadeColor;
    }

    [System.Serializable]
    public class LiveTimelineKeyFadeDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFadeData>
    {

    }
}
