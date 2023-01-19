using System.Collections.Generic;

namespace Cutt
{
    public interface ILiveTimelineKeyDataList
    {
        LiveTimelineKey this[int index] { get; set; }

        int Count { get; }

        LiveTimelineKeyDataListAttr attribute { get; set; }

        TimelineKeyPlayMode playMode { get; set; }

        int depthCounter { get; }

        void Insert(int index, LiveTimelineKey item);

        IEnumerator<LiveTimelineKey> GetEnumerator();

        void Add(LiveTimelineKey item);

        void Clear();

        bool Remove(LiveTimelineKey item);

        void RemoveAt(int index);

        IEnumerable<LiveTimelineKey> ToEnumerable();

        LiveTimelineKey At(int index);

        LiveTimelineKey[] ToArray();

        List<LiveTimelineKey> ToList();

        bool HasAttribute(LiveTimelineKeyDataListAttr attr);

        bool EnablePlayModeTimeline(TimelinePlayerMode playerMode);

        FindKeyResult FindKeyCached(int frame, bool ignoreCache);

        FindKeyResult FindCurrentKey(int frame);

        FindKeyResult FindCurrentKeyNeighbor(int frame, int baseIndex);

        List<LiveTimelineKey> GetRange(int index, int count);
    }
}
