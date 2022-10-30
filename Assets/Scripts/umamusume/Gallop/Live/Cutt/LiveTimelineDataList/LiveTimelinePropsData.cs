using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyPropsData : LiveTimelineKeyWithInterpolate
    {
        public int settingFlags;
        public int propsID;
        public bool rendererEnable;
        public Color color;
        public Color rootColor;
        public Color tipColor;
        public float colorPower;
    }

    [System.Serializable]
    public class LiveTimelineKeyPropsDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyPropsData>
    {

    }

    [System.Serializable]
    public class LiveTimelinePropsData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyPropsDataList keys;
    }
}