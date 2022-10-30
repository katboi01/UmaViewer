using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyGlobalFogData : LiveTimelineKeyWithInterpolate
    {
        public bool isDistance;
        public float startDistance;
        public bool isHeight;
        public float height;
        public float heightDensity;
        public Color color;
        public FogMode fogMode;
        public float expDensity;
        public float start;
        public float end;
        public bool useRadialDistance;
    }

    [System.Serializable]
    public class LiveTimelineKeyGlobalFogDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyGlobalFogData>
    {

    }

    [System.Serializable]
    public class LiveTimelineGlobalFogData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "GlobalFog";
        public LiveTimelineKeyGlobalFogDataList keys;
    }
}
