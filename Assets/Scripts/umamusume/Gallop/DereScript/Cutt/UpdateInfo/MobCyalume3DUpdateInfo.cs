using UnityEngine;

namespace Cutt
{
    public struct MobCyalume3DUpdateInfo
    {
        public Vector3 rootTranslate;

        public Vector3 rootRotate;

        public Vector3 rootScale;

        public bool isVisibleMob;

        public bool isVisibleCyalume;

        public bool isEnableMotionMultiSample;

        public float motionTimeOffset;

        public float motionTimeInterval;

        public LiveMobCyalume3DWaveMode waveMode;

        public Vector3 waveBasePosition;

        public float waveWidth;

        public float waveHeight;

        public float waveRoughness;

        public float waveProgress;

        public float waveColorBasePower;

        public float waveColorGainPower;

        public float gradiation;

        public float rimlight;

        public float blendRange;

        public float paletteScrollSection;

        public float horizontalOffset;

        public float verticalOffset;

        public float threshold;

        public float growPower;

        public LiveMobCyalume3DLookAtMode lookAtMode;

        public int lookAtPositionCount;

        public Vector3[] lookAtPositionList;

        public int positionCount;

        public Vector3[] positionList;
    }
}
