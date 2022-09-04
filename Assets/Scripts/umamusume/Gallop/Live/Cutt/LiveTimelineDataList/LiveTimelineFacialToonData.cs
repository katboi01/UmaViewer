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
		public const int DATA_LIST_SIZE = 18;
		public LiveTimelineKeyFacialToonDataList centerKeys;
		public LiveTimelineKeyFacialToonDataList left1Keys;
		public LiveTimelineKeyFacialToonDataList right1Keys;
		public LiveTimelineKeyFacialToonDataList left2Keys;
		public LiveTimelineKeyFacialToonDataList right2Keys;
		public LiveTimelineKeyFacialToonDataList motion5Keys; 
		public LiveTimelineKeyFacialToonDataList motion6Keys; 
		public LiveTimelineKeyFacialToonDataList motion7Keys; 
		public LiveTimelineKeyFacialToonDataList motion8Keys; 
		public LiveTimelineKeyFacialToonDataList motion9Keys; 
		public LiveTimelineKeyFacialToonDataList motion10Keys;
		public LiveTimelineKeyFacialToonDataList motion11Keys;
		public LiveTimelineKeyFacialToonDataList motion12Keys;
		public LiveTimelineKeyFacialToonDataList motion13Keys;
		public LiveTimelineKeyFacialToonDataList motion14Keys;
		public LiveTimelineKeyFacialToonDataList motion15Keys;
		public LiveTimelineKeyFacialToonDataList motion16Keys;
		public LiveTimelineKeyFacialToonDataList motion17Keys;
	}
}
