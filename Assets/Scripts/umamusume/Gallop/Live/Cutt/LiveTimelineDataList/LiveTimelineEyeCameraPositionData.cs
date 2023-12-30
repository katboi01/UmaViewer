using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyEyeCameraPositionData : LiveTimelineKeyCameraPositionData
    {
        public bool IsEnabled;
        public float Power;
        public float Roll;
        public float Fov;
        public LiveCameraCullingLayer CullingMask;
        public Texture2D MaskTexture;
    }

    [System.Serializable]
    public class LiveTimelineKeyEyeCameraPositionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyEyeCameraPositionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineEyeCameraPositionData : ILiveTimelineGroupDataWithName
    {
        private const string DEFAULT_NAME = "EyeCameraPos";
        public LiveTimelineKeyEyeCameraPositionDataList keys;
        [SerializeField]
        private int _characterIndex;
    }
}
