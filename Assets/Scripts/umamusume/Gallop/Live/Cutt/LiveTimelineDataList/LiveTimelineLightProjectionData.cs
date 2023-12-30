using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyLightProjectionData : LiveTimelineKeyWithInterpolate
    {
        [System.Serializable]
        public class AnimationData
        {
            public int TextureId;
            public int DivisionNumberX;
            public int DivisionNumberY;
            public int MaxCut;
            public float AnimationTime;
            public Vector2 ScaleUV;
            public Vector2 OffsetUV;
        }

        public const int UNUSED_ANIMATION_TEXTURE_ID = -1;
        public bool IsEnable;
        public int TextureId;
        public Color Color;
        public Vector3 Position;
        public Vector3 Angle;
        public Vector3 Scale;
        public bool Orthographic;
        public float OrthographicSize;
        public float NearClipPlane;
        public float FarClipPlane;
        public float FieldOfView;
        public float ColorPower;
        public bool CharacterAttach;
        public LiveCharaPosition CharacterAttachPosition;
        public Quaternion Rotation;
        public LiveTimelineKeyLightProjectionData.AnimationData AnimationParam;
    }

    [System.Serializable]
    public class LiveTimelineKeyLightProjectionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyLightProjectionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineLightProjectionData : ILiveTimelineGroupDataWithName
    {
        public const string DEFAULT_NAME = "projector_";
        public LiveTimelineKeyLightProjectionDataList keys;
    }
}