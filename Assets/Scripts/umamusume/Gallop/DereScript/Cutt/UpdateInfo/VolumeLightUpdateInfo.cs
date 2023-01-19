using UnityEngine;

namespace Cutt
{
    public struct VolumeLightUpdateInfo
    {
        public float progressTime;

        public LiveTimelineVolumeLightData data;

        public Vector3 sunPosition;

        public Color color1;

        public float power;

        public float komorebi;

        public float blurRadius;

        public bool enable;

        public bool isEnabledBorderClear;
    }
}
