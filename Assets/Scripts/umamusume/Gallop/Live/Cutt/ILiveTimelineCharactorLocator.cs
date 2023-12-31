using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public interface ILiveTimelineCharactorLocator
    {
        public abstract bool liveCharaVisible { get; set; }
        public abstract LiveCharaPosition liveCharaStandingPosition { get; set; }
        public abstract Vector3 liveCharaInitialPosition { get; set; }
        public abstract Vector3 liveCharaPosition { get; }
        public abstract Quaternion liveCharaPositionLocalRotation { get; set; }
        public abstract Vector3 liveCharaHeadPosition { get; }
        public abstract Vector3 liveCharaWaistPosition { get; }
        public abstract Vector3 liveCharaLeftHandWristPosition { get; }
        public abstract Vector3 liveCharaLeftHandAttachPosition { get; }
        public abstract Vector3 liveCharaRightHandAttachPosition { get; }
        public abstract Vector3 liveCharaRightHandWristPosition { get; }
        public abstract Vector3 liveCharaChestPosition { get; }
        public abstract Vector3 liveCharaFootPosition { get; }
        public abstract Vector3 liveCharaConstHeightHeadPosition { get; }
        public abstract Vector3 liveCharaConstHeightWaistPosition { get; }
        public abstract Vector3 liveCharaConstHeightChestPosition { get; }
        public abstract Vector3 liveCharaInitialHeightHeadPosition { get; }
        public abstract Vector3 liveCharaInitialHeightWaistPosition { get; }
        public abstract Vector3 liveCharaInitialHeightChestPosition { get; }
        public abstract Vector3 liveCharaScale { get; }
        public abstract Transform liveParentDefaultTransform { get; set; }
        public abstract Transform liveParentTransform { get; set; }
        public abstract Transform liveRootTransform { get; }
        public abstract int liveCharaHeightLevel { get; set; }
        public abstract float liveCharaHeightValue { get; }
        public abstract float liveCharaHeightRatioBase { get; set; }
        public abstract float liveCharaHeightRatio { get; set; }
        public abstract Vector3 liveCharaFormationHeightRateOffset { get; set; }
        public abstract bool IsPositionNodePositionAddParent { get; set; }
        public abstract bool IsCastShadow { get; set; }
        public abstract float CySpringRate { get; set; }
        public abstract int LayerIndex { set; }
        public abstract Color EmissiveColor { set; }
    }
}
