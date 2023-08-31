using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    public class LiveTimelineWorkSheet : ScriptableObject
    {
        public static string[] sVersionList = new string[1] { "0.9" };

        public string version = sVersionList[0];

        public int targetCameraIndex;

        public bool enableAtRuntime = true;

        public bool enableAtEdit = true;

        public LiveTimelineKeyCameraPositionDataList cameraPosKeys = new LiveTimelineKeyCameraPositionDataList();

        public LiveTimelineKeyCameraLookAtDataList cameraLookAtKeys = new LiveTimelineKeyCameraLookAtDataList();

        public LiveTimelineKeyCameraFovDataList cameraFovKeys = new LiveTimelineKeyCameraFovDataList();

        public LiveTimelineKeyCameraRollDataList cameraRollKeys = new LiveTimelineKeyCameraRollDataList();

        public LiveTimelineKeyEventDataList eventKeys = new LiveTimelineKeyEventDataList();

        public List<LiveTimelineCharaMotSeqData> charaMotSeqList = new List<LiveTimelineCharaMotSeqData>();

        public List<LiveTimelineCharaOverrideMotSeqData> charaMotOverwriteList = new List<LiveTimelineCharaOverrideMotSeqData>();

        public List<LiveTimelineBgColor1Data> bgColor1List = new List<LiveTimelineBgColor1Data>();

        public List<LiveTimelineBgColor2Data> bgColor2List = new List<LiveTimelineBgColor2Data>();

        public List<LiveTimelineBgColor3Data> bgColor3List = new List<LiveTimelineBgColor3Data>();

        public List<LiveTimelineMonitorControlData> monitorControlList = new List<LiveTimelineMonitorControlData>();

        public List<LiveTimelineAnimationData> animationList = new List<LiveTimelineAnimationData>();

        public List<LiveTimelineTextureAnimationData> textureAnimationList = new List<LiveTimelineTextureAnimationData>();

        public List<LiveTimelineTransformData> transformList = new List<LiveTimelineTransformData>();

        public List<LiveTimelineRendererData> rendererList = new List<LiveTimelineRendererData>();

        public List<LiveTimelineObjectData> objectList = new List<LiveTimelineObjectData>();

        public List<LiveTimelineGazingObjectData> gazingObjectList = new List<LiveTimelineGazingObjectData>();

        public List<LiveTimelinePropsData> propsList = new List<LiveTimelinePropsData>();

        public List<LiveTimelinePropsAttachData> propsAttachList = new List<LiveTimelinePropsAttachData>();

        public LiveTimelineKeyCameraSwitcherDataList cameraSwitcherKeys = new LiveTimelineKeyCameraSwitcherDataList();

        public LiveTimelineKeyRipSyncDataList ripSyncKeys = new LiveTimelineKeyRipSyncDataList();

        public LiveTimelineKeyPostEffectDataList postEffectKeys = new LiveTimelineKeyPostEffectDataList();

        public LiveTimelineKeyPostFilmDataList postFilmKeys = new LiveTimelineKeyPostFilmDataList();

        public LiveTimelineKeyPostFilmDataList postFilm2Keys = new LiveTimelineKeyPostFilmDataList();

        public LiveTimelineKeyScreenFadeDataList screenFadeKeys = new LiveTimelineKeyScreenFadeDataList();

        public LiveTimelineKeyCrossFadeCameraPositionDataList crossFadeCameraPosKeys = new LiveTimelineKeyCrossFadeCameraPositionDataList();

        public LiveTimelineKeyCrossFadeCameraLookAtDataList crossFadeCameraLookAtKeys = new LiveTimelineKeyCrossFadeCameraLookAtDataList();

        public LiveTimelineKeyCrossFadeCameraFovDataList crossFadeCameraFovKeys = new LiveTimelineKeyCrossFadeCameraFovDataList();

        public LiveTimelineKeyCrossFadeCameraRollDataList crossFadeCameraRollKeys = new LiveTimelineKeyCrossFadeCameraRollDataList();

        public LiveTimelineKeyCrossFadeCameraAlphaDataList crossFadeCameraAlphaKeys = new LiveTimelineKeyCrossFadeCameraAlphaDataList();

        public LiveTimelineKeyCameraLayerDataList cameraLayerKeys = new LiveTimelineKeyCameraLayerDataList();

        public List<LiveTimelineProjectorData> projecterList = new List<LiveTimelineProjectorData>();

        public LiveTimelineFacialData facial1Set = new LiveTimelineFacialData();

        public LiveTimelineKeyFacialEyeTrackDataList other4EyeTrackKeys = new LiveTimelineKeyFacialEyeTrackDataList();

        public LiveTimelineOther4FacialData[] other4FacialArray = new LiveTimelineOther4FacialData[8]
        {
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData(),
        new LiveTimelineOther4FacialData()
        };

        public LiveTimelineKeyFacialNoiseDataList facialNoiseKeys = new LiveTimelineKeyFacialNoiseDataList();

        public LiveTimelineKeyCharaMotionNoiseDataList charaMotionNoiseKeys = new LiveTimelineKeyCharaMotionNoiseDataList();

        public LiveTimelineKeyCharaCySpringDataList charaCySpringKeys = new LiveTimelineKeyCharaCySpringDataList();

        public LiveTimelineFormationOffsetData formationOffsetSet = new LiveTimelineFormationOffsetData();

        public List<LiveTimelineKeyFormationOffsetDataList> formationOffsetAdditionalList = new List<LiveTimelineKeyFormationOffsetDataList>();

        public List<LiveTimelineVolumeLightData> VolumeLightKeys = new List<LiveTimelineVolumeLightData>();

        public List<LiveTimelineHdrBloomData> HdrBloomKeys = new List<LiveTimelineHdrBloomData>();

        public List<LiveTimelineParticleData> particleList = new List<LiveTimelineParticleData>();

        public List<LiveTimelineParticleGroupData> particleGroupList = new List<LiveTimelineParticleGroupData>();

        public List<LiveTimelineLaserData> laserList = new List<LiveTimelineLaserData>();

        public List<LiveTimelineEffectData> effectList = new List<LiveTimelineEffectData>();

        public List<LiveTimelineEnvironmentData> environmentDataLists = new List<LiveTimelineEnvironmentData>();

        public List<LiveTimelineGlobalLightData> globalLightDataLists = new List<LiveTimelineGlobalLightData>();

        public List<LiveTimelineGlobalFogData> globalFogDataLists = new List<LiveTimelineGlobalFogData>();

        public List<LiveTimelineColorCorrectionData> colorCorrectionDataLists = new List<LiveTimelineColorCorrectionData>();

        public LiveTimelineKeyTiltShiftDataList tiltShiftKeys = new LiveTimelineKeyTiltShiftDataList();

        public List<LiveTimelineLightShuftData> lightShuftKeys = new List<LiveTimelineLightShuftData>();

        public List<LiveTimelineSweatLocatorData> sweatLocatorList = new List<LiveTimelineSweatLocatorData>();

        public List<LiveTimelineLensFlareData> lensFlareList = new List<LiveTimelineLensFlareData>();

        public List<DereLiveTimelineMonitorCameraPositionData> monitorCameraPosKeys = new List<DereLiveTimelineMonitorCameraPositionData>();

        public List<LiveTimelineMonitorCameraLookAtData> monitorLookAtPosKeys = new List<LiveTimelineMonitorCameraLookAtData>();

        public List<LiveTimelineMultiCameraPositionData> multiCameraPositionKeys = new List<LiveTimelineMultiCameraPositionData>();

        public List<LiveTimelineMultiCameraLookAtData> multiCameraLookAtKeys = new List<LiveTimelineMultiCameraLookAtData>();

        public List<LiveTimelineA2UConfigData> a2uConfigList = new List<LiveTimelineA2UConfigData>();

        public List<LiveTimelineA2UData> a2uList = new List<LiveTimelineA2UData>();

        public List<LiveTimelineGlassData> glassList = new List<LiveTimelineGlassData>();

        public List<LiveTimelineShaderControlData> shdCtrlList = new List<LiveTimelineShaderControlData>();

        public LiveTimelineCharaIKData charaIkSet = new LiveTimelineCharaIKData();

        public LiveTimelineKeyCharaFootLightDataList charaFootLightKeys = new LiveTimelineKeyCharaFootLightDataList();

        public List<LiveTimelineStageGazeData> stageGazeList = new List<LiveTimelineStageGazeData>();

        public List<LiveTimelineMobCyalumeData> mobCyalumeDataList = new List<LiveTimelineMobCyalumeData>();

        public List<LiveTimelineCharaHeightMotSeqData> charaHeightMotList = new List<LiveTimelineCharaHeightMotSeqData>();

        public List<LiveTimelineCharaWindData> charaWindList = new List<LiveTimelineCharaWindData>();

        public LiveTimelineKeyMobCyalume3DRootDataList mobCyalume3DRootKeys = new LiveTimelineKeyMobCyalume3DRootDataList();

        public LiveTimelineKeyMobCyalume3DBloomDataList mobCyalume3DBloomKeys = new LiveTimelineKeyMobCyalume3DBloomDataList();

        public LiveTimelineKeyMobCyalume3DColorDataList mobCyalume3DColorKeys = new LiveTimelineKeyMobCyalume3DColorDataList();

        public LiveTimelineKeyMobCyalume3DLightingDataList mobCyalume3DLightingKeys = new LiveTimelineKeyMobCyalume3DLightingDataList();

        public LiveTimelineKeyMobCyalume3DLookAtModeDataList mobCyalume3DLookAtModeKeys = new LiveTimelineKeyMobCyalume3DLookAtModeDataList();

        public List<LiveTimelineMobCyalume3DLookAtPositionData> mobCyalume3DLookAtPositionDataList = new List<LiveTimelineMobCyalume3DLookAtPositionData>();

        public List<LiveTimelineMobCyalume3DPositionData> mobCyalume3DPositionDataList = new List<LiveTimelineMobCyalume3DPositionData>();

        public LiveTimelineKeyDressChangeDataList dressChangeDataList = new LiveTimelineKeyDressChangeDataList();

        private ILiveTimelineKeyDataList[] _keysArray;

        public ILiveTimelineKeyDataList[] keysArray
        {
            get
            {
                if (_keysArray == null)
                {
                    List<ILiveTimelineKeyDataList> list = new List<ILiveTimelineKeyDataList>();
                    list.Add(cameraPosKeys);
                    list.Add(cameraLookAtKeys);
                    list.Add(cameraFovKeys);
                    list.Add(cameraRollKeys);
                    list.Add(eventKeys);
                    list.Add(cameraSwitcherKeys);
                    list.Add(ripSyncKeys);
                    list.Add(postEffectKeys);
                    list.Add(cameraLayerKeys);
                    list.Add(other4EyeTrackKeys);
                    list.Add(facialNoiseKeys);
                    list.Add(charaMotionNoiseKeys);
                    list.Add(charaCySpringKeys);
                    list.Add(tiltShiftKeys);
                    list.Add(charaFootLightKeys);
                    list.Add(crossFadeCameraPosKeys);
                    list.Add(crossFadeCameraLookAtKeys);
                    list.Add(crossFadeCameraFovKeys);
                    list.Add(crossFadeCameraRollKeys);
                    list.Add(crossFadeCameraAlphaKeys);
                    list.Add(mobCyalume3DBloomKeys);
                    list.Add(mobCyalume3DColorKeys);
                    list.Add(mobCyalume3DLightingKeys);
                    list.Add(mobCyalume3DLookAtModeKeys);
                    list.Add(dressChangeDataList);

                    list.AddRange(GetKeysInGroup(charaMotSeqList));
                    list.AddRange(GetKeysInGroup(bgColor1List));
                    list.AddRange(GetKeysInGroup(bgColor2List));
                    list.AddRange(GetKeysInGroup(monitorControlList));
                    list.AddRange(GetKeysInGroup(projecterList));
                    list.AddRange(facial1Set.GetKeyListArray());
                    list.AddRange(GetKeysInGroup(particleList));
                    list.AddRange(GetKeysInGroup(particleGroupList));
                    list.AddRange(GetKeysInGroup(laserList));
                    list.AddRange(GetKeysInGroup(effectList));
                    list.AddRange(GetKeysInGroup(sweatLocatorList));
                    list.AddRange(GetKeysInGroup(environmentDataLists));
                    list.AddRange(GetKeysInGroup(globalLightDataLists));
                    list.AddRange(GetKeysInGroup(globalFogDataLists));
                    list.AddRange(GetKeysInGroup(colorCorrectionDataLists));
                    list.AddRange(GetKeysInGroup(a2uConfigList));
                    list.AddRange(GetKeysInGroup(a2uList));
                    list.AddRange(GetKeysInGroup(glassList));
                    list.AddRange(GetKeysInGroup(shdCtrlList));
                    list.AddRange(GetKeysInGroup(charaMotOverwriteList));
                    LiveTimelineOther4FacialData[] array = other4FacialArray;
                    foreach (LiveTimelineOther4FacialData liveTimelineOther4FacialData in array)
                    {
                        list.AddRange(liveTimelineOther4FacialData.GetKeyListArray());
                    }
                    list.AddRange(formationOffsetSet.GetKeyListArray());
                    list.AddRange(GetKeysInGroup(bgColor3List));
                    list.AddRange(charaIkSet.GetKeyListArray());
                    list.AddRange(GetKeysInGroup(stageGazeList));
                    list.AddRange(GetKeysInGroup(mobCyalumeDataList));
                    list.AddRange(GetKeysInGroup(charaHeightMotList));
                    list.AddRange(GetKeysInGroup(monitorCameraPosKeys));
                    list.AddRange(GetKeysInGroup(monitorLookAtPosKeys));
                    list.AddRange(GetKeysInGroup(multiCameraPositionKeys));
                    list.AddRange(GetKeysInGroup(multiCameraLookAtKeys));
                    list.AddRange(GetKeysInGroup(lightShuftKeys));
                    list.AddRange(GetKeysInGroup(lensFlareList));
                    list.AddRange(GetKeysInGroup(charaWindList));
                    list.AddRange(GetKeysInGroup(mobCyalume3DLookAtPositionDataList));
                    list.AddRange(GetKeysInGroup(mobCyalume3DPositionDataList));

                    _keysArray = list.ToArray();
                }
                return _keysArray;
            }
        }

        public bool IsEnable()
        {
            return enableAtRuntime;
        }

        private ILiveTimelineKeyDataList[] GetKeysInGroup<T>(List<T> groups) where T : ILiveTimelineGroupData
        {
            List<ILiveTimelineKeyDataList> list = new List<ILiveTimelineKeyDataList>();
            foreach (T group in groups)
            {
                list.Add(group.GetKeyList());
            }
            return list.ToArray();
        }

        private void UpdateStatusInGroup<T>(List<T> groups) where T : ILiveTimelineGroupData
        {
            foreach (T group in groups)
            {
                (group as ILiveTimelineGroupDataWithName).UpdateStatus();
            }
        }

        public void OnLoad(LiveTimelineControl timelineControl)
        {
            for (int i = 0; i < keysArray.Length; i++)
            {
                ILiveTimelineKeyDataList liveTimelineKeyDataList = keysArray[i];
                int count = liveTimelineKeyDataList.Count;
                for (int j = 0; j < count; j++)
                {
                    liveTimelineKeyDataList[j].OnLoad(timelineControl);
                }
            }
            UpdateStatusInGroup(bgColor1List);
            UpdateStatusInGroup(bgColor2List);
            UpdateStatusInGroup(monitorControlList);
            UpdateStatusInGroup(projecterList);
            UpdateStatusInGroup(animationList);
            UpdateStatusInGroup(textureAnimationList);
            UpdateStatusInGroup(transformList);
            UpdateStatusInGroup(rendererList);
            UpdateStatusInGroup(objectList);
            UpdateStatusInGroup(propsList);
            UpdateStatusInGroup(propsAttachList);
            UpdateStatusInGroup(particleList);
            UpdateStatusInGroup(particleGroupList);
            UpdateStatusInGroup(laserList);
            UpdateStatusInGroup(effectList);
            UpdateStatusInGroup(sweatLocatorList);
            UpdateStatusInGroup(environmentDataLists);
            UpdateStatusInGroup(globalLightDataLists);
            UpdateStatusInGroup(globalFogDataLists);
            UpdateStatusInGroup(colorCorrectionDataLists);
            UpdateStatusInGroup(a2uConfigList);
            UpdateStatusInGroup(a2uList);
            UpdateStatusInGroup(glassList);
            UpdateStatusInGroup(shdCtrlList);
            UpdateStatusInGroup(bgColor3List);
            UpdateStatusInGroup(stageGazeList);
            UpdateStatusInGroup(multiCameraPositionKeys);
            UpdateStatusInGroup(multiCameraLookAtKeys);
            UpdateStatusInGroup(monitorCameraPosKeys);
            UpdateStatusInGroup(monitorLookAtPosKeys);
            UpdateStatusInGroup(lightShuftKeys);
            UpdateStatusInGroup(lensFlareList);
            UpdateStatusInGroup(gazingObjectList);
        }

        public int FindTimelineIndexInGroup(LiveTimelineKey key)
        {
            if (key == null)
            {
                return -1;
            }
            return key.dataType switch
            {
                LiveTimelineKeyDataType.MonitorCameraPos => monitorCameraPosKeys.FindIndex((DereLiveTimelineMonitorCameraPositionData x) => x.keys.Contains(key)),
                LiveTimelineKeyDataType.MonitorCameraLookAt => monitorLookAtPosKeys.FindIndex((LiveTimelineMonitorCameraLookAtData x) => x.keys.Contains(key)),
                LiveTimelineKeyDataType.MultiCameraPos => multiCameraPositionKeys.FindIndex((LiveTimelineMultiCameraPositionData x) => x.keys.Contains(key)),
                LiveTimelineKeyDataType.MultiCameraLookAt => multiCameraLookAtKeys.FindIndex((LiveTimelineMultiCameraLookAtData x) => x.keys.Contains(key)),
                _ => -1,
            };
        }
    }
}
