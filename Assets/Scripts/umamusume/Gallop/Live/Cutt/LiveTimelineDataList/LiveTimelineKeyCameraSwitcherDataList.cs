namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCameraSwitcherData : LiveTimelineKey
    {
        public int cameraIndex;
    }

    [System.Serializable]
    public class LiveTimelineKeyCameraSwitcherDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraSwitcherData>
    {

    }
}
