namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMultiCameraTiltShiftData : LiveTimelineKeyTiltShiftData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraTiltShiftDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraTiltShiftData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraTiltShiftData : LiveTimelineMultiCameraPostEffect
    {
        private const string default_name = "TiltShift";
        public LiveTimelineKeyMultiCameraTiltShiftDataList keys;
    }
}