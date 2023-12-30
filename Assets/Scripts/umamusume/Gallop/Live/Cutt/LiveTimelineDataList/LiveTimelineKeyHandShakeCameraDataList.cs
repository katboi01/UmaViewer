namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyHandShakeCameraData : LiveTimelineKeyWithInterpolate
    {
        public float power;
        public float frequency;
    }

    [System.Serializable]
    public class LiveTimelineKeyHandShakeCameraDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyHandShakeCameraData>
    {

    }
}
