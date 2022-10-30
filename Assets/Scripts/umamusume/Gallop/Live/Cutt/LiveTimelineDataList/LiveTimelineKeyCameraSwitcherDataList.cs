using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCameraSwitcherData : LiveTimelineKey
    {
        public int cameraIndex;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraSwitcherDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraSwitcherData>
    {

    }
}
