using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPostEffectDOFData : LiveTimelineKeyPostEffectDOFData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraPostEffectDOFDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraPostEffectDOFData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraPostEffectDOFData : LiveTimelineMultiCameraPostEffect
    {
        private const string default_name = "DOF";
        public LiveTimelineKeyMultiCameraPostEffectDOFDataList keys;
    }
}
