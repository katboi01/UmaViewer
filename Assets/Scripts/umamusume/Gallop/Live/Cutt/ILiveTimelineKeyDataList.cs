using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public interface ILiveTimelineKeyDataList
    {
        //public abstract LiveTimelineKey Item { get; set; }
        public abstract int Count { get; }
        //public abstract LiveTimelineKeyDataListAttr attribute { get; set; }
        //public abstract TimelineKeyPlayMode playMode { get; set; }
        //public abstract Color BaseColor { get; set; }
        //public abstract string Description { get; set; }
        //public abstract int depthCounter { get; }

        public abstract LiveTimelineKeyIndex TimeKeyIndex { get; }

        LiveTimelineKeyIndex FindCurrentKey(float currentTime);

        LiveTimelineKeyIndex UpdateCurrentKey(float currentTime);

        LiveTimelineKey this[int index] { get; set; }

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

        FindKeyResult FindKeyCached(float frame, bool ignoreCache);

        FindKeyResult FindCurrentKey(int frame);

        FindKeyResult FindCurrentKeyNeighbor(float frame, int baseIndex);

        List<LiveTimelineKey> GetRange(int index, int count);
    }
}