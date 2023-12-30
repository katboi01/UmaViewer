using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum EffectOccurrenceSpot
    {
        Head = 0,
        Eye_middle_01_L = 1,
        Eye_bottom_01_L = 2,
        Eye_middle_01_R = 3,
        Eye_bottom_01_R = 4,
        Neck = 5,
        Chest = 6,
        Waist = 7,
        Hand_Attach_L = 8,
        Thumb_03_L = 9,
        Index_03_L = 10,
        Middle_03_L = 11,
        Ring_03_L = 12,
        Pinky_03_L = 13,
        Hand_Attach_R = 14,
        Thumb_03_R = 15,
        Index_03_R = 16,
        Middle_03_R = 17,
        Ring_03_R = 18,
        Pinky_03_R = 19,
        Ankle_L = 20,
        Toe_L = 21,
        Ankle_R = 22,
        Toe_R = 23,
        Position = 24,
        Max = 25
    }

    [System.Serializable]
    public class LiveTimelineKeyEffectData : LiveTimelineKeyWithInterpolate
    {
        public enum ShaderColorProperty
        {
            Color = 0,
            MulColor0_ColorPower = 1
        }

        public enum EffectStatus
        {
            Play = 0,
            Stop = 1
        }

        [System.Serializable]
        public class ParticleParam
        {
            public enum SimulationSpace
            {
                Local = 0,
                World = 1
            }

            public string ObjectName;
            public int ObjectNameHash;
            public bool IsEnabled;
            public float MainDuration;
            public bool MainLooping;
            public bool IsUpdateMainSimulationSpace;
            public bool IsSetAllMainSimulationSpace;
            public LiveTimelineKeyEffectData.ParticleParam.SimulationSpace MainSimulationSpace;
            public Color MainStartColorMin;
            public Color MainStartColorMax;
            public float EmissionRateOverTimeMultiplier;
            public Vector3 ShapeScale;
            public bool IsUpdateRandomSeed;
            public bool IsSetAllRandomSeed;
            public int RandomSeed;
        }


        private const int FLAG_LOOP = 262144;
        private const int FLAG_PLAY = 524288;
        private const int FLAG_CLEAR = 1048576;
        private const int FLAG_STAY_PRS = 2097152;
        private const int FLAG_TRANSPARENT_FX = 4194304;
        private const int FLAG_COLOR_POWER_RGB = 8388608;
        private const int FLAG_PARTICLE_PARAM = 16777216;
        public Color color; // 0x30
        public float colorPower; // 0x40
        public LiveTimelineKeyEffectData.ShaderColorProperty ColorProperty;
        public LiveCharaPosition owner;
        public EffectOccurrenceSpot occurrenceSpot;
        public Vector3 offset;
        public Vector3 offsetAngle;
        public Vector3 offsetScale;
        public LiveTimelineKeyEffectData.ParticleParam[] ParticleParamArray;
    }

    [System.Serializable]
    public class LiveTimelineKeyEffectDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyEffectData>
    {

    }

    [System.Serializable]
    public class LiveTimelineEffectData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Effect";
        public LiveTimelineKeyEffectDataList keys;
        //Mono√ª”–
        public int updatedKeyFrame;
    }
}
