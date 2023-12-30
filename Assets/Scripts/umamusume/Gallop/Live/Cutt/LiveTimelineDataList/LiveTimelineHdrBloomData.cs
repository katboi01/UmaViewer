namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyHdrBloomData : LiveTimelineKeyWithInterpolate
    {
        public float intensity;
        public float blurSpread;
        public bool enable;

    }

    [System.Serializable]
    public class LiveTimelineKeyHdrBloomDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyHdrBloomData>
    {

    }

    [System.Serializable]
    public class LiveTimelineHdrBloomData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyHdrBloomDataList keys;
    }
}
