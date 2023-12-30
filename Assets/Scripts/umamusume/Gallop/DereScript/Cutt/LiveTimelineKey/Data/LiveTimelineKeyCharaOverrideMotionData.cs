using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaOverrideMotionData : LiveTimelineKeyCharaMotionData
    {
        public bool enableRandomPlay;

        public int randomCount;

        public AnimationClip[] randomClips;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaMotionOverwrite;

        protected override void MotionNameChangeOnLoad(LiveTimelineControl timelineControl)
        {
            motionNameHash = FNVHash.FNV_OFFSET_BASIS_I32;

            if (string.IsNullOrEmpty(motionName))
            {
                return;
            }
            MotionNameChangeAlt(timelineControl);
            MotionNameChangeRandomPlay();
            if (!Director.instance || (!Director.instance.IsExtendMotionLoadForConditionMotionChange() && !Director.instance.IsSwapMotionNameForConditionMotionChange(motionName)))
            {
                clip = timelineControl.LoadAnimationClip(this);
                if (clip == null && motionName.EndsWith("_alt_legacy", StringComparison.Ordinal))
                {
                    motionName = motionName.Replace("_alt", string.Empty);
                    clip = timelineControl.LoadAnimationClip(this);
                }
                motionNameHash = FNVHash.Generate(motionName);
            }
        }

        protected override void MotionNameChangeAlt(LiveTimelineControl timelineControl)
        {
            if (motionName.EndsWith("_alt_legacy", StringComparison.Ordinal))
            {
                motionName = motionName.Replace("_alt", string.Empty);
            }
            bool flag = false;
            if (timelineControl.data.alterAnimationMode == LiveTimelineData.AlterAnimationMode.LeftHanded)
            {
                int num = -1;
                List<LiveTimelineCharaOverrideMotSeqData> charaMotOverwriteList = timelineControl.data.GetWorkSheetList()[0].charaMotOverwriteList;
                for (int i = 0; i < charaMotOverwriteList.Count; i++)
                {
                    if (charaMotOverwriteList[i].keys.thisList.IndexOf(this) >= 0)
                    {
                        num = i;
                        break;
                    }
                }
                if (num >= 0)
                {
                    num = Array.IndexOf(timelineControl.data.characterSettings.motionOverwriteIndices, num);
                    int num2 = num;
                    int charaId = Director.instance.GetCharacterData(num2 + 1).charaId;
                    if (MasterDBManager.instance.masterCharaData.Get(charaId).hand == 3002)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                motionName = motionName.Replace("_legacy", "_alt_legacy");
            }
        }

        private void MotionNameChangeRandomPlay()
        {
            if (enableRandomPlay)
            {
                int num = UnityEngine.Random.Range(0, randomCount);
                AnimationClip animationClip = randomClips[num];
                if (animationClip != null)
                {
                    motionName = animationClip.name;
                }
            }
        }
    }
}