using Gallop.Live.Cutt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
	[System.Serializable]
	public class LiveTimelineKeyFacialToonData : LiveTimelineKeyWithInterpolate
	{
		public float CheekPretenseThreshold;
		public float NosePretenseThreshold;
		public float CylinderBlend;
		public float HairNormalBlend;
		public LiveTimelineDefine.FacialToonLightType UseOriginalDirectionalLight;
		public Vector3 OriginalDirectionalLightDir;
		public float EyeToonStep;
		public float EyeToonFeather;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialToonDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialToonData>
	{

	}

	[System.Serializable]
    public class LiveTimelineFacialToonData
    {
        public const int DATA_LIST_SIZE = 20;
        public LiveTimelineKeyFacialToonDataList centerKeys; // 0x10
        public LiveTimelineKeyFacialToonDataList left1Keys; // 0x18
        public LiveTimelineKeyFacialToonDataList right1Keys; // 0x20
        public LiveTimelineKeyFacialToonDataList left2Keys; // 0x28
        public LiveTimelineKeyFacialToonDataList right2Keys; // 0x30
        public LiveTimelineKeyFacialToonDataList motion5Keys; // 0x38
        public LiveTimelineKeyFacialToonDataList motion6Keys; // 0x40
        public LiveTimelineKeyFacialToonDataList motion7Keys; // 0x48
        public LiveTimelineKeyFacialToonDataList motion8Keys; // 0x50
        public LiveTimelineKeyFacialToonDataList motion9Keys; // 0x58
        public LiveTimelineKeyFacialToonDataList motion10Keys; // 0x60
        public LiveTimelineKeyFacialToonDataList motion11Keys; // 0x68
        public LiveTimelineKeyFacialToonDataList motion12Keys; // 0x70
        public LiveTimelineKeyFacialToonDataList motion13Keys; // 0x78
        public LiveTimelineKeyFacialToonDataList motion14Keys; // 0x80
        public LiveTimelineKeyFacialToonDataList motion15Keys; // 0x88
        public LiveTimelineKeyFacialToonDataList motion16Keys; // 0x90
        public LiveTimelineKeyFacialToonDataList motion17Keys; // 0x98
        public LiveTimelineKeyFacialToonDataList motion18Keys; // 0xA0
        public LiveTimelineKeyFacialToonDataList motion19Keys; // 0xA8
        private ILiveTimelineKeyDataList[] _cacheDataList; // 0xB0
    }
}
