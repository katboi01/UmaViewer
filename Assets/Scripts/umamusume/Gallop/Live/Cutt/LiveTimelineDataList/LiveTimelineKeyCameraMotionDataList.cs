using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveCameraMotionType
    {
        Direct = 0,
        Character = 1
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraMotionData : LiveTimelineKey
    {
        public bool IsEnable;
        public LiveCameraMotionType MotionType;
        public AnimationClip Clip;
        public float MotionHeadTime;
        public float PlaySpeed;
        public LiveCharaPositionFlag CharaRelativeBase;
        public LiveCameraCharaParts CharaRelativeParts;
        public Vector3 Offset;
        public Vector3 CharaPos;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraMotionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraMotionData>
    {

    }
}
