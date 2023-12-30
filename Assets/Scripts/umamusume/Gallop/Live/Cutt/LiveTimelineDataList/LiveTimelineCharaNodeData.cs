using System;
using System.Collections.Generic;

namespace Gallop.Live.Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaNodeData : LiveTimelineKeyWithInterpolate
    {
        public LiveCharaPositionFlag PositionFlag;
        public bool EnableHeadCySpring;
        public bool EnableEarCySpring;
        public bool EnableBodyCySpring;
        public bool EnableSkirtCySpring;
        public bool EnableTailCySpring;
        public List<string> TargetCySpringBornNameList;
        public List<bool> TargetCySpringBornEnableList;
    }

    [Serializable]
    public class LiveTimelineKeyCharaNodeDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaNodeData>
    {

    }

    [Serializable]
    public class LiveTimelineCharaNodeData : ILiveTimelineGroupDataWithName
    {
        private const string DEFAULT_NAME = "CharaNode";
        public LiveTimelineKeyCharaNodeDataList keys;
    }
}
