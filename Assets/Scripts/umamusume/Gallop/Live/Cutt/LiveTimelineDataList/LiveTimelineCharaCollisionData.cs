using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCharaCollisionData : LiveTimelineKeyWithInterpolate
    {
        public static readonly Quaternion PLANE_UP_ROTATION;
        public bool IsEnable;
        public Vector3 Position;
        public Vector3 Angle;
        public CySpringCollisionData.CollisionType CollisionType;
        public float Radius;
        public Vector3 Offset;
        public Vector3 Offset2;
        public bool IsInner;
        public bool IsApplyHead;
        public bool IsApplyTail;
        public LiveCharaPositionFlag CharacterFlag;
        public Quaternion Rotation;
        private const int ATTR_PLANE_UP = 65536;
    }

    [System.Serializable]
    public class LiveTimelineKeyCharaCollisionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaCollisionData>
    {

    }


    [System.Serializable]
    public class LiveTimelineCharaCollisionData : ILiveTimelineGroupDataWithName
    {
        private const string DEFAULT_NAME = "EnvCollision";
        public LiveTimelineKeyCharaCollisionDataList keys;
    }
}
