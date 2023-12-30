namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyNodeScaleData : LiveTimelineKeyWithInterpolate
    {
        public int characterFlag;
        public int targetFlag;
        public int sizeType;
        public int scaleRatePer;
    }

    [System.Serializable]
    public class LiveTimelineKeyNodeScaleDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyNodeScaleData>
    {

    }

    [System.Serializable]
    public class LiveTimelineNodeScaleData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "NodeScale";
        public LiveTimelineKeyNodeScaleDataList keys;
    }
}
