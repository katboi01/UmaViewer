using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyFacialNoiseData : LiveTimelineKey
    {
        public int EnableCharacterBitFlag;
    }

    [System.Serializable]
    public class LiveTimelineKeyFacialNoiseDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialNoiseData>
    {

    }
}