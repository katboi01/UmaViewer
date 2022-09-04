using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCharaFootLightData : LiveTimelineKeyWithInterpolate
    {
        public LiveCharaPositionFlag positionFlag;
        public float[] hightMax;
        public Color[] lightColor;
    }

    [System.Serializable]
    public class LiveTimelineKeyCharaFootLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaFootLightData>
    {

    }
}
