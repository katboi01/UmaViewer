using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public interface ILiveTimelineMSQTarget
    {
        //public abstract LiveModelController[] LiveModelControllerArray { get; }
        public abstract bool liveMSQControlled { get; set; }
        public abstract AnimationState liveMSQCurrentAnimState { get; set; }
        public abstract float liveMSQCurrentAnimStartTime { get; set; }
        public abstract float heightRate { get; }
        //public abstract LiveIK IKCtrl { get; }
    }
}
