using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaMotionData : LiveTimelineKey
    {
        public string motionName = "";

        public AnimationClip clip;

        public float motionHeadTime;

        public float playLength = 1f;

        public float playSpeed = 1f;

        public bool loop;

        [NonSerialized]
        public int motionNameHash;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaMotionSequecne;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
            MotionNameChangeOnLoad(timelineControl);
        }

        protected virtual void MotionNameChangeOnLoad(LiveTimelineControl timelineControl)
        {
            motionNameHash = FNVHash.FNV_OFFSET_BASIS_I32;

            if (string.IsNullOrEmpty(motionName))
            {
                return;
            }
            MotionNameChangeAlt(timelineControl);
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


        protected virtual void MotionNameChangeAlt(LiveTimelineControl timelineControl)
        {
            if (motionName.EndsWith("_alt_legacy", StringComparison.Ordinal))
            {
                motionName = motionName.Replace("_alt", string.Empty);
            }
            bool flag = false;
            if (timelineControl.data.alterAnimationMode == LiveTimelineData.AlterAnimationMode.LeftHanded)
            {
                int num = -1;
                List<LiveTimelineCharaMotSeqData> charaMotSeqList = timelineControl.data.GetWorkSheetList()[0].charaMotSeqList;
                for (int i = 0; i < charaMotSeqList.Count; i++)
                {
                    if (charaMotSeqList[i].keys.thisList.IndexOf(this) >= 0)
                    {
                        num = i;
                        break;
                    }
                }
                if (num >= 0)
                {
                    num = Array.IndexOf(timelineControl.data.characterSettings.motionSequenceIndices, num);
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
    }
}
