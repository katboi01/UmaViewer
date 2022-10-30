using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveCameraLookAtType
    {
        Direct = 0,
        Character = 1
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraLookAtData : LiveTimelineKeyWithInterpolate
    {
        public LiveCameraLookAtType lookAtType;
        public Vector3 position;
        public LiveCharaPositionFlag lookAtCharaPos;
        public LiveCameraCharaParts lookAtCharaParts;
        public Vector3 charaPos;
        public Vector3[] bezierPoints;
        public float traceSpeed;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraLookAtDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraLookAtData>
    {

    }
}

