using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyColorCorrectionData : LiveTimelineKey
    {
        public bool enable = true;

        public float saturation = 1f;

        public AnimationCurve redCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public AnimationCurve greenCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public AnimationCurve blueCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public bool selective;

        public Color keyColor = Color.white;

        public Color targetColor = Color.white;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.ColorCorrection;

        public override void OnLoad(LiveTimelineControl timelineControl)
        {
            base.OnLoad(timelineControl);
        }
    }
}
