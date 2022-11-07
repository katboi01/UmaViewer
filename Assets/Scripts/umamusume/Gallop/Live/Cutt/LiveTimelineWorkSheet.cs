using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    public enum LiveTimelineKeyAttribute
	{
		Disable = 1,
		CameraDelayEnable = 2,
		CameraDelayInherit = 4,
		KeyCommonBitMask = 32768
	}

	public enum LiveTimelineKeyDataListAttr
	{
		Disable = 1
	}

	public enum TimelineKeyPlayMode
	{
		Always = 0,
		LightOnly = 1,
		DefaultOver = 2
	}

	public enum LiveCameraInterpolateType
	{
		None = 0,
		Linear = 1,
		Curve = 2,
		Ease = 3
	}

	public enum LiveCameraPositionType
	{
		Direct = 0,
		Character = 1
	}

	public enum LiveCharaPositionFlag
	{
		Place01 = 1,
		Place02 = 2,
		Place03 = 4,
		Place04 = 8,
		Place05 = 16,
		Place06 = 32,
		Place07 = 64,
		Place08 = 128,
		Place09 = 256,
		Place10 = 512,
		Place11 = 1024,
		Place12 = 2048,
		Place13 = 4096,
		Place14 = 8192,
		Place15 = 16384,
		Place16 = 32768,
		Place17 = 65536,
		Place18 = 131072,
		Center = 1,
		Left = 2,
		Right = 4,
		Side = 6,
		Back = 262136,
		Other = 262142,
		All = 262143
	}

	public enum LiveCameraCharaParts
	{
		Face = 0,
		Waist = 1,
		LeftHandWrist = 2,
		RightHandAttach = 3,
		Chest = 4,
		Foot = 5,
		InitFaceHeight = 6,
		InitWaistHeight = 7,
		InitChestHeight = 8,
		RightHandWrist = 9,
		LeftHandAttach = 10,
		ConstFaceHeight = 11,
		ConstChestHeight = 12,
		ConstWaistHeight = 13,
		ConstFootHeight = 14,
		Position = 15,
		PositionWithoutOffset = 16,
		InitialHeightFace = 17,
		InitialHeightChest = 18,
		InitialHeightWaist = 19,
		Max = 20
	}

	public enum LiveCameraCullingLayer
	{
		None = 0,
		TransparentFX = 1,
		Background3d_NotReflect = 2,
		Background3d = 4,
		Character3d = 8,
		Character3d_0 = 16,
		Character3d_1 = 32,
		Character3d_NotReflect = 64,
		NotLayerDefault = 128,
		NotLayer3d = 256,
		Effect = 512
	}

	public enum LiveCameraBgColorType
	{
		Direct = 0,
		CharacterImageColorMain = 1,
		CharacterImageColorSub = 2,
		CharacterUIColorMain = 3,
		CharacterUIColorSub = 4
	}

	[System.Serializable]
	public class LiveTimelineKeyDataListTemplate<T>
	{
		[SerializeField]
		private LiveTimelineKeyDataListAttr _attribute;
		[SerializeField]
		private TimelineKeyPlayMode _playMode;
		public List<T> thisList;
	}

	[System.Serializable]
	public abstract class LiveTimelineKey
	{
		public int frame;
		public LiveTimelineKeyAttribute attribute;
	}

	[System.Serializable]
	public class LiveTimelineKeyTimescaleData : LiveTimelineKey
	{
		public float Timescale;
	}

	[System.Serializable]
	public class LiveTimelineKeyTimescaleDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyTimescaleData>
	{

	}

	[System.Serializable]
	public static class LiveTimelineEasing
	{
		public enum Type
		{
			Linear = 0,
			ExpoEaseOut = 1,
			ExpoEaseIn = 2,
			ExpoEaseInOut = 3,
			ExpoEaseOutIn = 4,
			CircEaseOut = 5,
			CircEaseIn = 6,
			CircEaseInOut = 7,
			CircEaseOutIn = 8,
			QuadEaseOut = 9,
			QuadEaseIn = 10,
			QuadEaseInOut = 11,
			QuadEaseOutIn = 12,
			SineEaseOut = 13,
			SineEaseIn = 14,
			SineEaseInOut = 15,
			SineEaseOutIn = 16,
			CubicEaseOut = 17,
			CubicEaseIn = 18,
			CubicEaseInOut = 19,
			CubicEaseOutIn = 20,
			QuartEaseOut = 21,
			QuartEaseIn = 22,
			QuartEaseInOut = 23,
			QuartEaseOutIn = 24,
			QuintEaseOut = 25,
			QuintEaseIn = 26,
			QuintEaseInOut = 27,
			QuintEaseOutIn = 28,
			ElasticEaseOut = 29,
			ElasticEaseIn = 30,
			ElasticEaseInOut = 31,
			ElasticEaseOutIn = 32,
			BounceEaseOut = 33,
			BounceEaseIn = 34,
			BounceEaseInOut = 35,
			BounceEaseOutIn = 36,
			BackEaseOut = 37,
			BackEaseIn = 38,
			BackEaseInOut = 39,
			BackEaseOutIn = 40
		}
	}

	[System.Serializable]
	public abstract class LiveTimelineKeyWithInterpolate : LiveTimelineKey
	{
		public LiveCameraInterpolateType interpolateType;
		public AnimationCurve curve;
		public LiveTimelineEasing.Type easingType;
	}


	//正在处理这里
	[System.Serializable]
	public class LiveTimelineKeyCameraPositionData : LiveTimelineKeyWithInterpolate
	{
		public LiveCameraPositionType setType;
		public Vector3 position;
		public Vector3 charaPos;
		public Vector3[] bezierPoints;
		public LiveCharaPositionFlag charaRelativeBase;
		public LiveCameraCharaParts charaRelativeParts;
		public float traceSpeed;
		public float nearClip;
		public float farClip;
		public LiveCameraCullingLayer cullingLayer;
		public LiveCameraBgColorType BgColorType;
		public Color BgColor;
		public int BgColorTargetCharacterIndex;
	}

	[System.Serializable]
	public class LiveTimelineKeyCameraPositionDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCameraPositionData>
	{

	}


	public class LiveTimelineWorkSheet : ScriptableObject
    {
		public string version;
		public int targetCameraIndex;
		public bool enableAtRuntime;
		public bool enableAtEdit;
		public float TotalTimeLength;
		public bool Lyrics;
		public LiveTimelineDefine.SheetIndex SheetType;
		public LiveTimelineKeyTimescaleDataList timescaleKeys;
		public LiveTimelineKeyCameraPositionDataList cameraPosKeys;

		//[SerializeField]用于该类在别的脚本里定义的时候
		[SerializeField] public LiveTimelineKeyCameraLookAtDataList cameraLookAtKeys;
		[SerializeField] public LiveTimelineKeyCameraFovDataList cameraFovKeys;
		[SerializeField] public LiveTimelineKeyCameraRollDataList cameraRollKeys;
		[SerializeField] public LiveTimelineKeyCameraMotionDataList cameraMotionKeys;
		[SerializeField] public LiveTimelineKeyHandShakeCameraDataList handShakeCameraKeys;
		[SerializeField] public LiveTimelineKeyEventDataList eventKeys;

		[SerializeField] public List<LiveTimelineCharaMotSeqData> charaMotSeqList;
		[SerializeField] public List<LiveTimelineBgColor1Data> bgColor1List;
		[SerializeField] public List<LiveTimelineBgColor2Data> bgColor2List;
		[SerializeField] public List<LiveTimelineMonitorControlData> monitorControlList;
		[SerializeField] public List<LiveTimelineAnimationData> animationList;
		[SerializeField] public List<LiveTimelineTextureAnimationData> textureAnimationList;
		[SerializeField] public List<LiveTimelineTransformData> transformList;
		[SerializeField] public List<LiveTimelineRendererData> rendererList;
		[SerializeField] public List<LiveTimelineObjectData> objectList;
		[SerializeField] public List<LiveTimelineWaveObjectData> waveObjectList;
		[SerializeField] public List<LiveTimelineAudienceData> audienceList;
		[SerializeField] public List<LiveTimelinePropsData> propsList;
		[SerializeField] public List<LiveTimelinePropsAttachData> propsAttachList;

		[SerializeField] public LiveTimelineKeyCameraSwitcherDataList cameraSwitcherKeys;
		[SerializeField] public LiveTimelineKeyLipSyncDataList ripSyncKeys;
		[SerializeField] public LiveTimelineKeyLipSyncDataList ripSync2Keys;
		[SerializeField] public LiveTimelineKeyPostEffectDOFDataList postEffectDOFKeys;
		[SerializeField] public LiveTimelineKeyPostEffectBloomDiffusionDataList postEffectBloomDiffusionKeys;
		[SerializeField] public LiveTimelineKeyRadialBlurDataList radialBlurKeys;
		[SerializeField] public LiveTimelineKeyPostFilmDataList postFilmKeys;
		[SerializeField] public LiveTimelineKeyPostFilmDataList postFilm2Keys;
		[SerializeField] public LiveTimelineKeyPostFilmDataList postFilm3Keys;
		[SerializeField] public LiveTimelineKeyFluctuationDataList FluctuationKeys;
		[SerializeField] public LiveTimelineKeyVortexDataList VortexKeys;
		[SerializeField] public LiveTimelineKeyFadeDataList fadeKeys;
		[SerializeField] public LiveTimelineKeyCameraLayerDataList cameraLayerKeys;

		[SerializeField] public List<LiveTimelineProjectorData> projecterList;

		[SerializeField] public LiveTimelineFacialData facial1Set;
		[SerializeField] public LiveTimelineKeyFacialEyeTrackDataList other4EyeTrackKeys;
		[SerializeField] public LiveTimelineKeyToneCurveDataList ToneCurveKeys;
		[SerializeField] public LiveTimelineKeyExposureDataList ExposureKeys;
		[SerializeField] public LiveTimelineFacialData[] other4FacialArray;
		[SerializeField] public LiveTimelineKeyFacialNoiseDataList facialNoiseKeys;
		[SerializeField] public LiveTimelineKeyCharaMotionNoiseDataList charaMotionNoiseKeys;
		[SerializeField] public LiveTimelineFormationOffsetData formationOffsetSet;

		[SerializeField] public List<LiveTimelineVolumeLightData> volumeLightKeys;
		[SerializeField] public List<LiveTimelineHdrBloomData> hdrBloomKeys;
		[SerializeField] public List<LiveTimelineParticleData> particleList;
		[SerializeField] public List<LiveTimelineParticleGroupData> particleGroupList;
		[SerializeField] public List<LiveTimelineWashLightData> WashLightList;
		[SerializeField] public List<LiveTimelineLaserData> laserList;
		[SerializeField] public List<LiveTimelineBlinkLightData> blinkLightList;
		[SerializeField] public List<LiveTimelineUVScrollLightData> uvScrollLightList;

		[SerializeField] public LiveTimelineFacialToonData facialToonSet;

		[SerializeField] public List<LiveTimelineGlobalLightData> globalLightDataLists;
		[SerializeField] public List<LiveTimelineGlobalFogData> globalFogDataLists;
		[SerializeField] public List<LiveTimelineColorCorrectionData> colorCorrectionDataLists;
		[SerializeField] public List<LiveTimelineLightShaftsData> lightShaftsKeysLine;
		[SerializeField] public List<LiveTimelineMonitorCameraPositionData> monitorCameraPosKeys;
		[SerializeField] public List<LiveTimelineMonitorCameraLookAtData> monitorCameraLookAtKeys;
		[SerializeField] public List<LiveTimelineMultiCameraPositionData> multiCameraPosKeys;
		[SerializeField] public List<LiveTimelineMultiCameraLookAtData> multiCameraLookAtKeys;
		[SerializeField] public List<LiveTimelineLensFlareData> lensFlareList;
		[SerializeField] public List<LiveTimelineEnvironmentData> environmentDataLists;
		[SerializeField] public List<LiveTimelineSweatLocatorData> sweatLocatorList;
		[SerializeField] public List<LiveTimelineEffectData> effectList;

		[SerializeField] public LiveTimelineKeyTiltShiftDataList tiltShiftKeys;

		[SerializeField] public List<LiveTimelineA2UConfigData> a2uConfigList;
		[SerializeField] public List<LiveTimelineA2UData> a2uList;

		[SerializeField] public LiveTimelineKeyTitleDataList titleKeys;

		[SerializeField] public List<LiveTimelineSpotlight3dData> spotlight3dList;
		[SerializeField] public List<LiveTimelineNodeScaleData> nodeScaleList;

		[SerializeField] public LiveTimelineKeyCharaFootLightDataList charaFootLightKeys;

		[SerializeField] public List<LiveTimelineChromaticAberrationData> chromaticAberrationList;
		[SerializeField] public List<LiveTimelineLightProjectionData> lightProjectionList;
		[SerializeField] public List<LiveTimelineBillboardData> billboardList;
		[SerializeField] public List<LiveTimelineMultiCameraPostFilmData> postFilm1MultiCameraKeys;
		[SerializeField] public List<LiveTimelineMultiCameraPostEffectBloomDiffusionData> postEffectBloomDiffusionMultiCameraKeys;
		[SerializeField] public List<LiveTimelineMultiCameraColorCorrectionData> multiCameraColorCorrectionDataLists;
		[SerializeField] public List<LiveTimelineMultiCameraTiltShiftData> multiCameraTiltShiftDataLists;
		[SerializeField] public List<LiveTimelineMultiCameraRadialBlurData> multiCameraRadialBlurDataLists;
		[SerializeField] public List<LiveTimelineMultiCameraPostEffectDOFData> postEffectDOFMultiCameraKeys;
		[SerializeField] public List<LiveTimelineAdditionalLight> AdditionalLightList;

		[SerializeField] public LiveTimelineKeyMultiLightShadowDataList MultiLightShadowKeys;

		[SerializeField] public List<LiveTimelineMobControlData> MobControlKeys;
		[SerializeField] public List<LiveTimelineCyalumeControlData> CyalumeControlKeys;

		[SerializeField] public LiveTimelineCharaWindData charaWind;
		[SerializeField] public LiveTimelineCharaPartsData CharaPartsKeys;

		[SerializeField] public List<LiveTimelineCharaCollisionData> CharaCollisionDataList;
		[SerializeField] public List<LiveTimelineEyeCameraPositionData> EyeCameraPosList;
		[SerializeField] public List<LiveTimelineEyeCameraLookAtData> EyeCameraLookAtList;




		/*
		//终于可以调用AB包了，虽然后面发现没什么用...说不定什么时候能用到
		private void Start()
		{
			LoadCharaMotion();
		}

		public void LoadCharaMotion()
		{
			foreach(LiveTimelineCharaMotSeqData liveCharaData in charaMotSeqList)
			{
				foreach(LiveTimelineKeyCharaMotionData charaMotionData in liveCharaData.keys.thisList)
				{
					foreach(var motionname in UmaViewerMain.Instance.AbList.Where(a => a.Name.StartsWith("3d/motion/live/body") && a.Name.EndsWith(charaMotionData.motionName)))
					{
						//UmaViewerBuilder.Instance.LoadComponent(motionname);
					}
				}
			}
		}
		*/
	}
}

