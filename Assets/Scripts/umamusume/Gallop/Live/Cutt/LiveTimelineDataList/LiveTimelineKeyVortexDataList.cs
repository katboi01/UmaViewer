using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyVortexData : LiveTimelineKeyWithInterpolate
    {
        public bool IsEnable; // 0x30
        public Vector4 Area; // 0x34
        public float RotVolume; // 0x44
        public float DepthClip; // 0x48
    }

    [System.Serializable]
    public class LiveTimelineKeyVortexDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyVortexData>
    {

    }
}
