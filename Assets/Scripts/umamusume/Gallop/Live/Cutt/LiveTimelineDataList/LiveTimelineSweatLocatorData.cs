using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeySweatLocatorData : LiveTimelineKeyWithInterpolate
    {
        public enum LocatorType
        {
            Local = 0,
            World = 1
        }

        public LiveCharaPosition owner;
        public float alpha;
        public int randomVisibleCount;
        public bool locator0_isVisible;
        public Vector3 locator0_offset;
        public Vector3 locator0_offsetAngle;
        public LiveTimelineKeySweatLocatorData.LocatorType locator0_offsetType;
        public bool locator1_isVisible;
        public Vector3 locator1_offset;
        public Vector3 locator1_offsetAngle;
        public LiveTimelineKeySweatLocatorData.LocatorType locator1_offsetType;
        public bool locator2_isVisible;
        public Vector3 locator2_offset;
        public Vector3 locator2_offsetAngle;
        public LiveTimelineKeySweatLocatorData.LocatorType locator2_offsetType;
        public bool locator3_isVisible;
        public Vector3 locator3_offset;
        public Vector3 locator3_offsetAngle;
        public LiveTimelineKeySweatLocatorData.LocatorType locator3_offsetType;
        public bool locator4_isVisible;
        public Vector3 locator4_offset;
        public Vector3 locator4_offsetAngle;
        public LiveTimelineKeySweatLocatorData.LocatorType locator4_offsetType;
    }

    [System.Serializable]
    public class LiveTimelineKeySweatLocatorDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeySweatLocatorData>
    {

    }

    [System.Serializable]
    public class LiveTimelineSweatLocatorData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "SweatLocator";
        public LiveTimelineKeySweatLocatorDataList keys;
        //为什么Mono里没有这个
        public List<int> randomVisibleIndex;
    }
}
