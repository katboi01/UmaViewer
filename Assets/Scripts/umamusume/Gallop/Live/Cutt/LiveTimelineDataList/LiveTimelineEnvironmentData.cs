using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyStageEnvironmentData : LiveTimelineKey
    {
        public bool isValidMirror;
        public bool isMirror;
        public bool isBgMirror;
        public bool IsMirrorBg3d;
        public LiveCharaPositionFlag characterMirror;
        public LiveCharaPositionFlag CharacterMirrorHead;
        public float mirrorReflectionRate;
        public bool isValidShadow;
        public LiveCharaPositionFlag characterShadow;
        public bool isSoftShadow;
        public bool IsToonMirror;
        public bool isValidWaterReflection;
        public float waterReflection;
        public float waveScale;
        public float waterDistortion;
        public float waterUCross;
        public float waterVCross;
        public float waterUSpeed;
        public float waterVSpeed;
        public float waterNormalPower;
        public float waveDistortionPower;
        public float waveClearly;
        public float waveDiffusion;
        public float waveDecline;
        public Color waterColor;
        public float waterVOffset;
        public bool IsValidStageFovShift;
        public float BaseFov;
        public float ShiftPower;
        public bool IsShiftY;
        private const int ATTR_ENABLE_MIRROR_CHARACTER_HEAD = 65536;
        public bool _isValidMirror;
        public bool _mirror;
        public bool _bgMirror;
        public LiveCharaPositionFlag _characterMirror;
        public float _mirrorReflectionRate;
    }

    [System.Serializable]
    public class LiveTimelineKeyStageEnvironmentDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyStageEnvironmentData>
    {

    }

    [System.Serializable]
    public class LiveTimelineEnvironmentData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Environment";
        public LiveTimelineKeyStageEnvironmentDataList keys;
    }
}
