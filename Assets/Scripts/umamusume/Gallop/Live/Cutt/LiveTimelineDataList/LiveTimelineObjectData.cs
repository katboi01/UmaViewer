using System;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveCharaPosition
    {
        Place01 = 0,
        Place02 = 1,
        Place03 = 2,
        Place04 = 3,
        Place05 = 4,
        Place06 = 5,
        Place07 = 6,
        Place08 = 7,
        Place09 = 8,
        Place10 = 9,
        Place11 = 10,
        Place12 = 11,
        Place13 = 12,
        Place14 = 13,
        Place15 = 14,
        Place16 = 15,
        Place17 = 16,
        Place18 = 17,
        World = 18,
        Max = 18,
        Camera = 19,
        Center = 0,
        Left = 1,
        Right = 2,
        CharacterMax = 17
    }

    public enum AttachType
    {
        None = 0,
        Camera = 1,
        Character = 2,
    }

    public enum OffsetType
    {
        Direct = 0,
        Add = 1,
    }


    public enum LayerType
    {
        None = 0,
        Normal = 1,
        NotReflect = 2,
        Projector = 3,
    }

    [Serializable]
    public class LiveTimelineKeyObjectDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyObjectData>
    {

    }

    [Serializable]
    public class LiveTimelineKeyObjectData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 position;
        public Vector3 rotate;
        public Vector3 scale;
        public bool renderEnable;
        public AttachType AttachTarget;
        public int CharacterPosition;
        public int MultiCameraIndex;
        public OffsetType OffsetValueType;
        public LayerType LayerTypeValue;
        public bool IsLayerTypeRecursively;
    }

    [Serializable]
    public class LiveTimelineObjectData : ILiveTimelineGroupDataWithName
    {
        // Fields
        private const string default_name = "Object";
        public LiveTimelineKeyObjectDataList keys;
        public bool enablePosition;
        public bool enableRotate;
        public bool enableScale;
    }
}
