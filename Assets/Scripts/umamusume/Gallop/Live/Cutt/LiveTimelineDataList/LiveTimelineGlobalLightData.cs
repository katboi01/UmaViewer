using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyGlobalLightData : LiveTimelineKeyWithInterpolate
    {
		public static readonly Color DefaultRimColor;
		public Vector3 lightDir;
		public Color rimColor;
		public float rimStep;
		public float rimFeather;
		public float rimSpecRate;
		public float globalRimShadowRate;
		public bool cameraFollow; 
		public float RimHorizonOffset;
		public float RimVerticalOffset;
		public float RimHorizonOffset2;
		public float RimVerticalOffset2;
		public Color rimColor2;
		public float rimStep2;
		public float rimFeather2;
		public float rimSpecRate2;
		public float globalRimShadowRate2;
		public LiveCharaPositionFlag flags;
		public LiveTimelineKeyLoopType loopType;
		public int loopCount;
		public int loopExecutedCount;
		public int loopIntervalFrame;
		public bool isPasteLoopUnit;
		public bool isChangeLoopInterpolate;

	}

	[System.Serializable]
    public class LiveTimelineKeyGlobalLightDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyGlobalLightData>
    {

    }

    [System.Serializable]
    public class LiveTimelineGlobalLightData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "GlobalLight";
        public LiveTimelineKeyGlobalLightDataList keys;
    }
}
