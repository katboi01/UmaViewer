using UnityEngine;

namespace Cutt
{
    public struct EffectUpdateInfo
    {
        public LiveTimelineEffectData data;

        public bool isEnable;

        public bool isPlay;

        public bool isLoop;

        public bool isClear;

        public eEffectOwner owner;

        public eEffectOccurrenceSpot occurrenceSpot;

        public Vector3 offset;

        public bool isStayPRS;

        public eEffectBlendMode blendMode;

        public Color color;

        public float colorPower;

        public Vector3 offsetAngle;
    }
}
