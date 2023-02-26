using System.Collections.Generic;
using UnityEngine;

namespace Cutt
{
    /// <summary>
    /// TimelineDataÉtÉ@ÉCÉãÇÃê›íË
    /// </summary>
    public class LiveTimelineData : ScriptableObject
    {
        public enum CharacterPositionMode
        {
            Immobility,
            Relative,
            None
        }

        public enum ParticleControllMode
        {
            Score,
            Timeline
        }

        public enum AlterAnimationMode
        {
            None,
            LeftHanded
        }

        public enum AlterSheetMode
        {
            None,
            LeftHanded
        }

        public static readonly string DEFAULT_SPOTLIGHT_PARENT_NAME = "Position";

        public const float MAX_FORCAL_SIZE_MIN = 30f;

        public const float MAX_FORCAL_SIZE_MAX = 200f;

        public static string[] sVersionList = new string[1] { "0.9" };

        public string version = sVersionList[0];

        public int timeLength = 140;

        public CharacterPositionMode characterPositionMode;

        public string[] spotLightPrefabNames = new string[0];

        public string spotLightParentName = DEFAULT_SPOTLIGHT_PARENT_NAME;

        public string[] shadowPrefabNames = new string[0];

        public LiveTimelineCharacterSettings characterSettings = new LiveTimelineCharacterSettings();

        public LiveTimelinePropsSettings propsSetteings = new LiveTimelinePropsSettings();

        public LiveTimelineSunshaftsSettings sunshaftsSettings = new LiveTimelineSunshaftsSettings();

        public LiveTimelineHdrBloomSettings hdrBloomSettings = new LiveTimelineHdrBloomSettings();

        public LiveTimelineCharacterOptionSettings characterOptionSettings = new LiveTimelineCharacterOptionSettings();

        public LiveTimelineIndirectLightShuftSettings indirectLightShuftSettings = new LiveTimelineIndirectLightShuftSettings();

        public LiveTimelineA2USettings a2uSettings = new LiveTimelineA2USettings();

        public LiveTimelineMonitorCameraSettings monitorCameraSettings = new LiveTimelineMonitorCameraSettings();

        public LiveTimelineMultiCameraSettings multiCameraSettings = new LiveTimelineMultiCameraSettings();

        public LiveTimelineMobCyalume3DSettings mobCyalume3DSettings = new LiveTimelineMobCyalume3DSettings();

        public LiveTimelineSwitchSheetSetting switchSheetSettings = new LiveTimelineSwitchSheetSetting();

        public LiveTimelineSwitchSheetAltSetting switchSheetAltSettings = new LiveTimelineSwitchSheetAltSetting();

        public LiveTimelineSwitchSheetTargetTimelineSetting switchSheetTargetTimelineSettings = new LiveTimelineSwitchSheetTargetTimelineSetting();

        public LiveTimelineTransparentCameraSettings transparentFXCameraSettings = new LiveTimelineTransparentCameraSettings();

        [SerializeField]
        private List<LiveTimelineWorkSheet> worksheetList = new List<LiveTimelineWorkSheet>();

        public LiveTimelineMultiCameraMaskSettings multiCameraMaskSettings = new LiveTimelineMultiCameraMaskSettings();

        public ParticleControllMode particleControllMode = ParticleControllMode.Timeline;

        public bool isUseHQParticle;

        public string[] particlePrefabNames = new string[0];

        public string[] mirrorScanLightBodyPrefabNames = new string[0];

        public LiveTimelineLensFlareSetting lensFlareSetting = new LiveTimelineLensFlareSetting();

        [Range(30f, 200f)]
        public float maxForcalSize = 30f;

        public bool isUseMirrorScanMotionDictionary;

        public bool isUseGameSettingToParticle;

        public LiveTimelineStageObjectsSettings stageObjectsSettings = new LiveTimelineStageObjectsSettings();

        public LiveTimelineGamePlaySettings gamePlaySettings = new LiveTimelineGamePlaySettings();

        [HideInInspector]
        public LiveTimelineAlterAnimationSettings alterAnimationSettings = new LiveTimelineAlterAnimationSettings();

        public AlterAnimationMode alterAnimationMode;

        public LiveTimelineMonitorSettings monitorSettings = new LiveTimelineMonitorSettings();

        public bool isHighPolygonModel(int charaPos, bool bLightMode)
        {
            return (bLightMode ? characterSettings.useHighPolygonModelForLightMode : characterSettings.useHighPolygonModel)[charaPos];
        }

        public List<LiveTimelineWorkSheet> GetWorkSheetList()
        {
            return worksheetList;
        }

        public LiveTimelineWorkSheet GetWorkSheet(int index)
        {
            return worksheetList[index];
        }

        public void UnloadSheets()
        {
            for (int i = 0; i < worksheetList.Count; i++)
            {
                if (worksheetList[i] != null)
                {
                    Resources.UnloadAsset(worksheetList[i]);
                }
            }
        }

        public void OnLoad(LiveTimelineControl timelineControl)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < worksheetList.Count; i++)
            {
                if (worksheetList[i] == null)
                {
                    list.Add(i);
                }
                else
                {
                    worksheetList[i].OnLoad(timelineControl);
                }
            }
            list.Reverse();
            foreach (int item in list)
            {
                worksheetList.RemoveAt(item);
            }
        }

        public int FindTimelineIndexInGroup(LiveTimelineKey key)
        {
            if (key == null)
            {
                return -1;
            }
            for (int i = 0; i < worksheetList.Count; i++)
            {
                LiveTimelineWorkSheet liveTimelineWorkSheet = worksheetList[i];
                if (!(liveTimelineWorkSheet == null))
                {
                    int num = liveTimelineWorkSheet.FindTimelineIndexInGroup(key);
                    if (num >= 0)
                    {
                        return num;
                    }
                }
            }
            return -1;
        }
    }
}
