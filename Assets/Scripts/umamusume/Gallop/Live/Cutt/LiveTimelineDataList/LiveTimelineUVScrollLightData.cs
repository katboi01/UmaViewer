using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    
    [System.Serializable]
    public class LiveTimelineKeyUVScrollLightData : LiveTimelineKeyWithInterpolate
    {
        
        public Color mulColor0;
        public Color mulColor1;
        public float colorPower;
        public float scrollOffsetX;
        public float scrollOffsetY;
        public float scrollSpeedX;
        public float scrollSpeedY;
        public Texture2D texture;
        public LiveTimelineKeyLoopType loopType;
        public int loopCount;
        public int loopExecutedCount;
        public int loopIntervalFrame;
        public bool isPasteLoopUnit;
        public bool isChangeLoopInterpolate;
        private const int ENABLE_TEXTURE = 65536;
        
    }

    [System.Serializable]
    public class LiveTimelineKeyUVScrollLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyUVScrollLightData>
    {

    }
    

    [System.Serializable]
    public class LiveTimelineUVScrollLightData : ILiveTimelineGroupDataWithName
    {
        private const string DEFAULT_NAME = "UVScrollLight";
        public LiveTimelineKeyUVScrollLightDataList keys;
    }
}
