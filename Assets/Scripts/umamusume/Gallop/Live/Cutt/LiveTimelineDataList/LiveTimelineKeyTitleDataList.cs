namespace Gallop.Live.Cutt
{
    public enum LiveTitleActionType
    {
        None = 0,
        FadeIn = 1,
        FadeOut = 2,
        Max = 3
    }

    [System.Serializable]
    public class LiveTimelineKeyTitleData : LiveTimelineKey
    {
        public LiveTitleActionType _actionType;
        public int _actionFrame;

    }

    [System.Serializable]
    public class LiveTimelineKeyTitleDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTitleData>
    {

    }
}
