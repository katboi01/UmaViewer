namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMultiCameraRadialBlurData : LiveTimelineKeyRadialBlurData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyMultiCameraRadialBlurDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMultiCameraRadialBlurData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMultiCameraRadialBlurData : LiveTimelineMultiCameraPostEffect
    {
        private const string default_name = "MultiCamera-RadialBlur";
        public LiveTimelineKeyMultiCameraRadialBlurDataList keys;
    }
}
