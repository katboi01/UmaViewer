using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyPostEffectDOFData : LiveTimelineKeyWithInterpolate
    {
		public float forcalSize; // 0x30
		public float blurSpread; // 0x34
		public LiveCharaPositionFlag charactor; // 0x38
		public DepthBlurAndBloom.DofBlur dofBlurType; // 0x3C
		public DepthBlurAndBloom.DofQuality dofQuality; // 0x40
		public float dofForegroundSize; // 0x44
		public float dofFgBlurSpread; // 0x48
		public float dofFocalPoint; // 0x4C
		public float dofSmoothness; // 0x50
		public float BallBlurPowerFactor; // 0x54
		public float BallBlurBrightnessThreshhold; // 0x58
		public float BallBlurBrightnessIntensity; // 0x5C
		public float BallBlurSpread; // 0x60
	}

    [System.Serializable]
    public class LiveTimelineKeyPostEffectDOFDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyPostEffectDOFData>
    {

    }
}
