using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineMultiCameraPostEffect : ILiveTimelineGroupDataWithName
    {
        public int MultiCameraNo;
    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPostFilmData : LiveTimelineKeyPostFilmData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPostFilmDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraPostFilmData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraPostFilmData : LiveTimelineMultiCameraPostEffect
    {
        private const string default_name = "Overlay";
        public LiveTimelineKeyMultiCameraPostFilmDataList keys;
    }
}
