using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveTimelineKeyLoopType
    {
        None = 0,
        CopyStart = 1,
        CopyEnd = 2,
        Paste = 3,
        Max = 4
    }

    [System.Serializable]
    public class ILiveTimelineGroupDataWithName : ILiveTimelineGroupData
    {
        public string name;
    }
}
