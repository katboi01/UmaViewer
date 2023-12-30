namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyRendererData : LiveTimelineKeyWithInterpolate
    {
        public bool renderEnable;
    }

    [System.Serializable]
    public class LiveTimelineKeyRendererDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyRendererData>
    {

    }

    [System.Serializable]
    public class LiveTimelineRendererData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyRendererDataList keys;
    }
}
