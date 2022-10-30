using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMultiCameraColorCorrectionData : LiveTimelineKeyColorCorrectionData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraColorCorrectionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraColorCorrectionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraColorCorrectionData : LiveTimelineMultiCameraPostEffect
    {
        private const string default_name = "ColorCorrection";
        public LiveTimelineKeyMultiCameraColorCorrectionDataList keys;
    }
}
