using UnityEngine;

namespace Cutt
{
    public struct LaserUpdateInfo
    {
        public LiveTimelineLaserData data;

        public eLaserForm formation;

        public float degRootYaw;

        public float degLaserPitch;

        public float posInterval;

        public float blinkPeroid;

        public bool isBlink;

        public Vector3 rootPos;

        public Vector3 rootRot;

        public Vector3 rootScale;

        public Vector3[] localPos;

        public Vector3[] localRot;

        public Vector3[] localScale;
    }
}

