using System;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineGamePlaySettings
    {
        public bool isFootShadowOff;

        public bool isRichFootShadowOff;

        public float distanceOutlineLOD = -1f;

        public float distanceRichOutlineLOD = -1f;

        public float distanceCheekLOD = -1f;

        public float distanceRichCheekLOD = -1f;

        public bool isDofBloomOff;

        public bool isRichDofBloomOff;

        public bool isMirrorScanOff;

        public bool isMonitorCameraOff;

        public bool isSunShaftOff;
    }
}
