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
    }
}