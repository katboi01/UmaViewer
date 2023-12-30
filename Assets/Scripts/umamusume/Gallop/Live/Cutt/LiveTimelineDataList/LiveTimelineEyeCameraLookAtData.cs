namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyEyeCameraLookAtData : LiveTimelineKeyCameraLookAtData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyEyeCameraLookAtDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyEyeCameraLookAtData>
    {

    }

    [System.Serializable]
    public class LiveTimelineEyeCameraLookAtData : ILiveTimelineGroupDataWithName
    {
        private const string DEFAULT_NAME = "EyeCameraLookAt";
        public LiveTimelineKeyEyeCameraLookAtDataList keys;
    }
}
