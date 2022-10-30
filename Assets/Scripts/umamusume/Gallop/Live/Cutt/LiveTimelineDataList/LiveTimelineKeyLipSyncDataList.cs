using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class FacialPartsData
    {
        public int FacialPartsId;
        public int WeightPer;
    }

    [System.Serializable]
    public class LiveTimelineKeyLipSyncData : LiveTimelineKey
    {
        public LiveCharaPositionFlag character;
        public int facialId;
        public int weight;
        public int speed;
        public int time;
        public FacialPartsData[] facialPartsDataArray; // 0x30
        public DrivenKeyComponent.InterpolateType interpolateType; // 0x38
    }

    [System.Serializable]
    public class LiveTimelineKeyLipSyncDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyLipSyncData>
    {

    }
}
