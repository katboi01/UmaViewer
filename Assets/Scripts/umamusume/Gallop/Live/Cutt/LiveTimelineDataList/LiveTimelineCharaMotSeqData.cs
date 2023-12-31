using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class ILiveTimelineGroupData
    {

    }

    [System.Serializable]
    public class LiveTimelineKeyCharaMotionData : LiveTimelineKey
    {
        public string motionName;
        public string motionName2;
        public string motionName3;
        public AnimationClip clip;
        public AnimationClip clip2;
        public AnimationClip clip3;
        public int overrideMotionSysTextId;
        public bool useOverrideMotionFacial;
        public int motionHeadFrame;
        public int playFrameLength;
        public float playSpeed;
        public bool loop;
        public bool isMotionHeadFrameAll;
        public int[] motionHeadFrameSeparetes;
    }

    [System.Serializable]
    public class LiveTimelineKeyCharaMotionSeqDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaMotionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineCharaMotSeqData : ILiveTimelineGroupData
    {
        public LiveTimelineKeyCharaMotionSeqDataList keys;
        [SerializeField]
        private bool _existsOverrideMotionSet;
    }
}
