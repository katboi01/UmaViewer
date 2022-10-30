using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyProjectorData : LiveTimelineKeyWithInterpolate
    {
        public int motionID;
        public int materialID;
        public float speed;
        public Color color1;
        public float power;
        public Vector3 position;
        public float rotate;
        public Vector2 size;
        public LiveTimelineKeyLoopType loopType;
        public int loopCount;
        public int loopExecutedCount;
        public int loopIntervalFrame;
        public bool isPasteLoopUnit;
        public bool isChangeLoopInterpolate;
    }

    [System.Serializable]
    public class LiveTimelineKeyProjectorDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyProjectorData>
    {

    }

    [System.Serializable]
    public class LiveTimelineProjectorData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyProjectorDataList keys;
    }
}
