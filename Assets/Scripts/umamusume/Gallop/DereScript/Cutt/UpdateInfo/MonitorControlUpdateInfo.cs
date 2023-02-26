using UnityEngine;

namespace Cutt
{
    public struct MonitorControlUpdateInfo
    {
        public float progressTime;

        public LiveTimelineMonitorControlData data;

        public Vector2 pos;

        public Vector2 size;

        public bool multiFlag;

        public LiveTimelineKeyMonitorControlData.eInputSource inputSource;

        public int dispID;

        public int subDispID;

        public float speed;

        public string textureLabel;

        public bool reversePlayFlag;

        public int playStartOffsetFrame;

        public float blendFactor;

        public float colorPower;
    }
}
