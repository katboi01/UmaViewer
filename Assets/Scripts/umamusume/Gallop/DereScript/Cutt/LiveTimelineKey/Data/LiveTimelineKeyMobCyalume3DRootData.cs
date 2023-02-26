using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyMobCyalume3DRootData : LiveTimelineKeyWithInterpolate
    {
        public const float DEFAULT_MOTION_TIME_INTERVAL = 1f;

        public const LiveMobCyalume3DWaveMode DEFAULT_WAVE_MODE = LiveMobCyalume3DWaveMode.None;

        public const float DEFAULT_WAVE_WIDTH = 5f;

        public const float DEFAULT_WAVE_HEIGHT = 1f;

        public Vector3 translate = Vector3.zero;

        public Vector3 rotate = Vector3.zero;

        public Vector3 scale = Vector3.one;

        public bool isVisibleMob = true;

        public bool isVisibleCyalume = true;

        public bool isEnableMotionMultiSample;

        public float motionTimeOffset;

        public float motionTimeInterval = 1f;

        public LiveMobCyalume3DWaveMode waveMode;

        public Vector3 waveBasePosition = Vector3.zero;

        public float waveWidth = 5f;

        public float waveHeight = 1f;

        public float waveRoughness;

        public float waveProgress;

        public float waveColorBasePower = 1f;

        public float waveColorGainPower;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.MobCyalume3DRoot;
    }
}
