using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveTimelineEventID
    {
        NOT_AVAILABLE = 0,
        SwitchCamera = 100,
        CameraHandShake = 101,
        SwitchMonitor = 102,
        ChangeEyeTrackTarget = 103
    }

    [System.Serializable]
    public class LiveTimelineKeyEventData : LiveTimelineKey
    {
        [System.Serializable]
        public class EventData
        {
            public LiveTimelineEventID eventId;
            public string serializedParamter;
        }

        public List<LiveTimelineKeyEventData.EventData> eventList;
    }


    [System.Serializable]
    public class LiveTimelineKeyEventDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyEventData>
    {

    }
}