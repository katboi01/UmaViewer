using UnityEngine;

namespace Cutt
{
    public struct CharaWindUpdateInfo
    {
        public LiveCharaPosition selfPosition;

        public Vector3 cySpringForceScale;

        public Vector3 windPower;

        public float loopTime;

        public bool enable;

        public LiveTimelineKeyCharaWindData.WindMode windMode;
    }
}
