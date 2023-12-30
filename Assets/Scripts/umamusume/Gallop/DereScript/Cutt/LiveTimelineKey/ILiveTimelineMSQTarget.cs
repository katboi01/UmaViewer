using UnityEngine;

namespace Cutt
{
    public interface ILiveTimelineMSQTarget
    {
        Animation liveMSQAnimation { get; }

        bool liveMSQControlled { get; set; }

        AnimationState liveMSQCurrentAnimState { get; set; }

        float liveMSQCurrentAnimStartTime { get; set; }

        ILiveTimelineMSQTarget[] spareTarget { get; }

        void OnMotionChange();
    }
}
