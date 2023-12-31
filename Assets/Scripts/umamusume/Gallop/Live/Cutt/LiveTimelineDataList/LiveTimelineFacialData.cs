using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
	public class LiveTimelineKeyFacialFaceData : LiveTimelineKey
	{
		public int facialId;
		public int weight;
		public int speed;
		public int time;
		public DrivenKeyComponent.InterpolateType interpolateType;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialFaceDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialFaceData>
	{
		
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialMouthData : LiveTimelineKey
	{
		public int facialId;
		public int weight;
		public int speed;
		public int time;
		public FacialPartsData[] facialPartsDataArray;
		public DrivenKeyComponent.InterpolateType interpolateType;
		public int type;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialMouthDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialMouthData>
	{

	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEyeData : LiveTimelineKey
	{
		public int facialId;
		public int weight;
		public int speed;
		public int time;
		public FacialPartsData[] facialPartsDataArrayL;
		public FacialPartsData[] facialPartsDataArrayR;
		public DrivenKeyComponent.InterpolateType interpolateType;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEyeDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialEyeData>
	{

	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEyebrowData : LiveTimelineKey
	{
		public int facialId;
		public int weight;
		public int speed;
		public int time;
		public FacialPartsData[] facialPartsDataArrayL;
		public FacialPartsData[] facialPartsDataArrayR;
		public DrivenKeyComponent.InterpolateType interpolateType;
		public bool usePartsScale;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEyebrowDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialEyebrowData>
	{

	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEyeTrackData : LiveTimelineKey
	{
		public LiveTimelineDefine.FacialEyeTrackTargetType targetType;
		public int verticalRatePer;
		public int horizontalRatePer;
		public int speedRatePer;
		public int speed;
		public int time;
		public DrivenKeyComponent.InterpolateType interpolateType;
		public Vector3 DirectPosition;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEyeTrackDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialEyeTrackData>
	{

	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEarData : LiveTimelineKey
	{
		public int facialId;
		public int weight;
		public int speed;
		public int time;
		public int facialEarIdL;
		public int facialEarIdR;
		public DrivenKeyComponent.InterpolateType interpolateType;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEarDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialEarData>
	{

	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEffectData : LiveTimelineKey
	{
		[System.Serializable]
		public class TeardropConfig
		{
			public int teardropIndex;
			public int teardropSlot;
			public bool isReversed;
			public float Alpha;
			public float Speed;
			public Color Color;
		}

		public int cheekType;
		public int tearyType;
		public int tearfulType;
		public TeardropConfig[] teardropConfigs;
		public int mangameIndex;

		public const int kAttrCheek = 65536;
		public const int kAttrTeary = 131072;
		public const int kAttrTearful = 262144;
		public const int kAttrTeardrop = 524288;
		public const int kAttrMangame = 2097152;
		public const int kAttrFaceShadow = 4194304;
		public const int kAttrFaceShadowVisible = 8388608;
	}

	[System.Serializable]
	public class LiveTimelineKeyFacialEffectDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFacialEffectData>
	{

	}

	[System.Serializable]
	public class LiveTimelineFacialData
    {
		public LiveTimelineKeyFacialFaceDataList faceKeys; // 0x10
		public LiveTimelineKeyFacialMouthDataList mouthKeys; // 0x18
		public LiveTimelineKeyFacialEyeDataList eyeKeys; // 0x20
		public LiveTimelineKeyFacialEyebrowDataList eyebrowKeys; // 0x28
		public LiveTimelineKeyFacialEyeTrackDataList eyeTrackKeys; // 0x30
		public LiveTimelineKeyFacialEarDataList earKeys; // 0x38
		public LiveTimelineKeyFacialEffectDataList effectKeys; // 0x40
	}
}