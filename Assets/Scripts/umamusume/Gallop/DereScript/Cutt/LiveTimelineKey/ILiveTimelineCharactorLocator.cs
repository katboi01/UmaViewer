using UnityEngine;

namespace Cutt
{
    public interface ILiveTimelineCharactorLocator
    {
        LiveCharaPosition liveCharaStandingPosition { get; set; }

        Vector3 liveCharaInitialPosition { get; set; }

        Vector3 liveCharaHeadPosition { get; }

        Vector3 liveCharaWaistPosition { get; }

        Vector3 liveCharaLeftHandWristPosition { get; }

        Vector3 liveCharaLeftHandAttachPosition { get; }

        Vector3 liveCharaRightHandAttachPosition { get; }

        Vector3 liveCharaRightHandWristPosition { get; }

        Vector3 liveCharaChestPosition { get; }

        Vector3 liveCharaFootPosition { get; }

        Vector3 liveCharaConstHeightHeadPosition { get; }

        Vector3 liveCharaConstHeightWaistPosition { get; }

        Vector3 liveCharaConstHeightChestPosition { get; }

        Animation liveAnimationComponent { get; }

        Transform liveParentTransform { get; set; }

        Transform liveRootTransform { get; }

        int liveCharaHeightLevel { get; set; }

        float liveCharaHeightValue { get; }

        float liveCharaHeightRatio { get; set; }

        bool liveCharaVisible { get; set; }

        void LiveTimeline_OnResetCloth();
    }
}
