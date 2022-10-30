using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gallop.ImageEffect;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyColorCorrectionData : LiveTimelineKey
    {
        public bool enable;
        public float saturation;
        public ColorCorrectionParam.ColorCorrectionMode mode;
        public AnimationCurve redCurve;
        public AnimationCurve greenCurve;
        public AnimationCurve blueCurve;
        public AnimationCurve depthRedCurve;
        public AnimationCurve depthGreenCurve;
        public AnimationCurve depthBlueCurve;
        public AnimationCurve blendCurve;
        public bool selective;
        public Color keyColor;
        public Color targetColor;
    }

    [System.Serializable]
    public class LiveTimelineKeyColorCorrectionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyColorCorrectionData>
    {

    }

    [System.Serializable]
    public class LiveTimelineColorCorrectionData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "ColorCorrection";
        public LiveTimelineKeyColorCorrectionDataList keys;
    }
}
