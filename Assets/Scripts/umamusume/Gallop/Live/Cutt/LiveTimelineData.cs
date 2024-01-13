using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{

    [System.Serializable]
    public class LiveTimelineCharacterSettings
    {
        public int[] motionSequenceIndices;
        public bool[] useHighPolygonModel;
        public bool[] useHighPolygonModelForLightMode;
        public bool[] useWetTexture;
        public int[] EyeReflectionTextureSizeRateArray;
    }

    [System.Serializable]
    public class LiveTimelinePropsSettings
    {
        public enum PropsConditionType
        {
            Default = 0,
            CharaPosition = 1,
            CharaId = 2,
            DressId = 3
        }

        [System.Serializable]
        public class PropsConditionData
        {
            public LiveTimelinePropsSettings.PropsConditionType Type;
            public int Value;
        }

        [System.Serializable]
        public class PropsConditionGroup
        {
            public LiveTimelinePropsSettings.PropsConditionData[] propsConditionData;
            public int propsConditionCount;
            public bool satisfiesAllConditions;
            public bool IsInvalid;
        }

        [System.Serializable]
        public class PropsDataGroup
        {
            public string propsName;
            public bool isCharaProps;
            public int charaPropsMajorId;
            public int charaPropsMinorId;
            public string[] attachJointNames;
            public int attachJointNameCount;
            public bool hasShadow;
            public LiveTimelinePropsSettings.PropsConditionGroup[] propsConditionGroup;
            public int propsConditionGroupCount;
        }

        public LiveTimelinePropsSettings.PropsDataGroup[] propsDataGroup;
        public int propsDataGroupCount;
    }

    [System.Serializable]
    public class LiveTimelineSunShaftsSettings
    {
        //public SunShaftsParam.SunShaftsResolution resolution; // 0x10
        //public SunShaftsParam.SunShaftsScreenBlendMode screenBlendMode; // 0x14
        public Color sunColor; // 0x18
        public float sunPower; // 0x28
        public float sunCenterBrightness; // 0x2C
        public float sunCenterMultiplex; // 0x30
        public float intensity; // 0x34
        public float fadeStart; // 0x38
        public float fadeMix; // 0x3C
        public float blackLevel; // 0x40
        public float komorebiRate; // 0x44
        public float blurRadius; // 0x48
        public int blurIterations;
        public bool isEnabledBorderClear;
    }

    [System.Serializable]
    public class LiveTimelineHdrBloomSettings
    {
        public int bloomBlurIterations;
    }

    [System.Serializable]
    public class LiveTimelineCharacterOptionSettings
    {
        public string[] optionName;
    }

    [System.Serializable]
    public class LiveTimelineIndirectLightShaftsSettings
    {
        public enum ShaftType
        {
            Default = 0,
            Vr03 = 1
        }
        public int id;
        public LiveTimelineIndirectLightShaftsSettings.ShaftType shaftType;
        public int shaftTextureNameLength;
        public string[] shaftTextureNames;
    }

    [System.Serializable]
    public class LiveTimelineA2USettings
    {
        [System.Serializable]
        public class Prefab
        {
            public string name;
            public string path;
        }

        public int flickRandomSeed; // 0x10
        public uint flickCount; // 0x14
        public float flickStepSec; // 0x18
        public uint flickMin; // 0x1C
        public uint flickMax; // 0x20
        public LiveTimelineA2USettings.Prefab[] prefabs; // 0x28
        public string[] sprites; // 0x30
        public int assetNo; // 0x38
    }

    [System.Serializable]
    public class LiveTimelineMultiCameraSettings
    {
        public int cameraNum;
    }

    [System.Serializable]
    public class LiveTimelineMonitorCameraSettings
    {
        public bool IsEnabledTextureWidthRate;
        public float TextureWidthRate;
    }

    [System.Serializable]
    public class LiveTimelineLensFlareSetting
    {
        [System.Serializable]
        public class FlareDataGroup
        {
            public int objectId;
            public bool isHqOnly;
        }

        public LiveTimelineLensFlareSetting.FlareDataGroup[] flareDataGroup; // 0x10
        public int flareDataGroupCount; // 0x18
        public float lightStandard; // 0x1C
        public float underLimit; // 0x20
    }

    [System.Serializable]
    public class LiveTimelineStageObjectsSettings
    {
        [System.Serializable]
        public class Object
        {
            public int replaceQuality;
            public int objectId;
            public string objectName;
        }

        public int objectCount; // 0x10
        public LiveTimelineStageObjectsSettings.Object[] objectData;
    }

    [System.Serializable]
    public class LiveTimelineDebugSettings
    {
        public bool isCheckCharacterPosition;
    }

    public class LiveTimelineData : ScriptableObject
    {
        public enum CharacterPositionMode
        {
            Immobility = 0,
            Relative = 1,
            None = 2
        }

        public enum FacialTimelineType
        {
            Motion = 0,
            Character = 1
        }

        public string version;
        public int timeLength;
        public LiveTimelineData.CharacterPositionMode characterPositionMode;
        public string[] spotLightPrefabNames;
        public string spotLightParentName;
        public string[] shadowPrefabNames;
        public LiveTimelineCharacterSettings characterSettings;
        public LiveTimelinePropsSettings propsSettings;
        public LiveTimelineSunShaftsSettings sunShaftsSettings;
        public LiveTimelineHdrBloomSettings hdrBloomSettings;
        public LiveTimelineCharacterOptionSettings characterOptionSettings;
        public LiveTimelineIndirectLightShaftsSettings indirectLightShaftsSettings;
        public LiveTimelineA2USettings a2uSettings;
        public LiveTimelineMultiCameraSettings multiCameraSettings;
        public LiveTimelineMonitorCameraSettings MonitorCameraSettings;
        public List<LiveTimelineWorkSheet> worksheetList;
        public LiveTimelineData.FacialTimelineType FacialLineType;
        public bool isUseHQParticle;
        public string[] particlePrefabNames;
        public string[] mirrorScanLightBodyPrefabNames;
        public LiveTimelineLensFlareSetting lensFlareSetting;
        public float maxForcalSize;
        public bool isUseMirrorScanMotionDictionary;
        public bool isUseGameSettingToParticle;
        public LiveTimelineStageObjectsSettings stageObjectsSettings;
        public LiveTimelineDebugSettings debugSettings;
    }

}

