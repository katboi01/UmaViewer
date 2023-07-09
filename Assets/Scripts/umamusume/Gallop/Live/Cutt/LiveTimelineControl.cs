using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.FinalIK.IKSolverVR;
using System.Linq;
using System.IO;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Gallop.Live.Cutt
{
    public class LiveTimelineControl : MonoBehaviour
    {
        public LiveTimelineData data;
        public const int kTargetFps = 60;
        public const float kTargetFpsF = 60;
        public const float kFrameToSec = 0.016666668f;
        public Transform cameraPositionLocatorsRoot;
        public Transform cameraLookAtLocatorsRoot;
        [SerializeField]
        private Transform[] characterStandPosLocators;
        public const float BaseCharaHeight = 158;
        public const float BaseCharaHeightMin = 130;
        public const float BaseCharaHeightMax = 190;
        public const float BaseCharaHeightDiff = 60;

        private LiveTimelineMotionSequence[] _motionSequenceArray;

        public LiveTimelineKeyCharaMotionSeqDataList[] _keyArray;

        public event Action<LiveTimelineKeyLipSyncData, float> OnUpdateLipSync;

        public event Action<FacialDataUpdateInfo, float, int> OnUpdateFacial;

        public void CopyValues<T>(T from, T to)
        {
            var json = JsonUtility.ToJson(from);
            JsonUtility.FromJsonOverwrite(json, to);
        }

        private void Awake()
        {
            InitializeTimeLineData();
            if (Director.instance)
            {
                Director.instance._liveTimelineControl = this;
            }
        }

        public void InitializeTimeLineData()
        {
            //var LoadData = gameObject.AddComponent<LiveTimelineData>();
            //CopyValues(data, LoadData);
            //foreach(LiveTimelineWorkSheet worksheet in LoadData.worksheetList)
            //{
            //    var LoadSheet = gameObject.AddComponent<LiveTimelineWorkSheet>();
            //    CopyValues(worksheet, LoadSheet);
            //}
        }

        public void InitCharaMotionSequence(int[] motionSequence) 
        {
            //Create Animation
            foreach (var obj in Director.instance.charaObjs)
            {
                var container = obj.GetComponentInChildren<UmaContainer>();
                if (container)
                {
                    var animation = container.gameObject.AddComponent<Animation>();
                    Director.instance.charaAnims.Add(animation);
                }
            }

            //Get KeyArray
            var listCount = data.worksheetList[0].charaMotSeqList.Count;

            _keyArray = new LiveTimelineKeyCharaMotionSeqDataList[listCount];

            for (int i = 0; i < listCount; i++)
            {
                _keyArray[i] = data.worksheetList[0].charaMotSeqList[i].keys;
            }

            if (Director.instance.liveMode == 1)
            {
                //Set Motion
                int CharaPositionMax = Director.instance.allowCount;

                _motionSequenceArray = new LiveTimelineMotionSequence[CharaPositionMax];


                for (int i = 0; i < CharaPositionMax; i++)
                {
                    Debug.Log(i);
                    _motionSequenceArray[i] = new LiveTimelineMotionSequence();
                    _motionSequenceArray[i].Initialize(Director.instance.charaObjs[i], i, motionSequence[i], this);
                }
            }
            else if(Director.instance.liveMode == 0)
            {
                List<AnimationClip> Anims = new List<AnimationClip>();
                //Get MotionList
                foreach (var motion in UmaViewerMain.Instance.AbMotions.Where(a => a.Name.StartsWith($"3d/motion/live/body/son{Director.instance.live.MusicId}") && Path.GetFileName(a.Name).Split('_').Length == 4))
                {
                    AssetBundle motionAB = UmaViewerBuilder.LoadOrGet(motion);
                    AnimationClip motionAnim = motionAB.LoadAsset<AnimationClip>(Path.GetFileName(motion.Name).Split('.')[0]);
                    Anims.Add(motionAnim);
                }

                //Set Motion
                int CharaPositionMax = Director.instance.allowCount;

                _motionSequenceArray = new LiveTimelineMotionSequence[CharaPositionMax];


                for (int i = 0; i < CharaPositionMax; i++)
                {
                    _motionSequenceArray[i] = new LiveTimelineMotionSequence();
                    _motionSequenceArray[i].Initialize(Director.instance.charaObjs[i], i, motionSequence[i], this, Anims);
                }
            }
  
        }

        public void AlterUpdate(float liveTime)
        {
            AlterUpdate_CharaMotionSequence(liveTime);
            AlterUpdate_FacialData(liveTime);
            AlterUpdate_LipSync(liveTime);
            //AlterLateUpdate_FormationOffset(liveTime);
        }

        public void AlterUpdate_CharaMotionSequence(float liveTime)
        {
            foreach (var motion in _motionSequenceArray)
            {
                motion.AlterUpdate(liveTime);
            }
        }

        public void AlterUpdate_FacialData(float liveTime)
        {
            var facialDataList = data.worksheetList[0].facial1Set;
            FacialDataUpdateInfo updateInfo = default(FacialDataUpdateInfo);
            if (facialDataList != null)
            {
                SetupFacialUpdateInfo_Mouth(ref updateInfo, facialDataList.mouthKeys, liveTime);
                SetupFacialUpdateInfo_Eye(ref updateInfo, facialDataList.eyeKeys, liveTime);
                SetupFacialUpdateInfo_Eyebrow(ref updateInfo, facialDataList.eyebrowKeys, liveTime);
                SetupFacialUpdateInfo_Ear(ref updateInfo, facialDataList.earKeys, liveTime);
                this.OnUpdateFacial(updateInfo, liveTime, 0);
            }

            var otherFacialDataList = data.worksheetList[0].other4FacialArray;
            for(int i = 0; i < otherFacialDataList.Length && i < Director.instance.characterCount - 1; i++)
            {
                SetupFacialUpdateInfo_Mouth(ref updateInfo, otherFacialDataList[i].mouthKeys, liveTime);
                SetupFacialUpdateInfo_Eye(ref updateInfo, otherFacialDataList[i].eyeKeys, liveTime);
                SetupFacialUpdateInfo_Eyebrow(ref updateInfo, otherFacialDataList[i].eyebrowKeys, liveTime);
                SetupFacialUpdateInfo_Ear(ref updateInfo, otherFacialDataList[i].earKeys, liveTime);
                this.OnUpdateFacial(updateInfo, liveTime, i+1);
            }
        }

        private void SetupFacialUpdateInfo_Mouth(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialMouthDataList keys, float time)
        {
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;

            LiveTimelineKeyIndex curKey = AlterUpdate_Key(keys, time);
            if (curKey != null)
            {
                liveTimelineKey = curKey.key;
                liveTimelineKey2 = curKey.nextKey;
                updateInfo.mouthCur = liveTimelineKey as LiveTimelineKeyFacialMouthData;
                updateInfo.mouthNext = liveTimelineKey2 as LiveTimelineKeyFacialMouthData;
                updateInfo.mouthKeyIndex = curKey.index;
            }
        }

        private void SetupFacialUpdateInfo_Eye(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialEyeDataList keys, float time)
        {
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;

            LiveTimelineKeyIndex curKey = AlterUpdate_Key(keys, time);
            if (curKey != null)
            {
                liveTimelineKey = curKey.key;
                liveTimelineKey2 = curKey.nextKey;
                updateInfo.eyeCur = liveTimelineKey as LiveTimelineKeyFacialEyeData;
                updateInfo.eyeNext = liveTimelineKey2 as LiveTimelineKeyFacialEyeData;
                updateInfo.eyeKeyIndex = curKey.index;
            }
        }

        private void SetupFacialUpdateInfo_Eyebrow(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialEyebrowDataList keys, float time)
        {
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;

            LiveTimelineKeyIndex curKey = AlterUpdate_Key(keys, time);
            if (curKey != null)
            {
                liveTimelineKey = curKey.key;
                liveTimelineKey2 = curKey.nextKey;
                updateInfo.eyebrowCur = liveTimelineKey as LiveTimelineKeyFacialEyebrowData;
                updateInfo.eyebrowNext = liveTimelineKey2 as LiveTimelineKeyFacialEyebrowData;
                updateInfo.eyebrowKeyIndex = curKey.index;
            }
        }

        private void SetupFacialUpdateInfo_Ear(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialEarDataList keys, float time)
        {
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;

            LiveTimelineKeyIndex curKey = AlterUpdate_Key(keys, time);
            if (curKey != null)
            {
                liveTimelineKey = curKey.key;
                liveTimelineKey2 = curKey.nextKey;
                updateInfo.earCur = liveTimelineKey as LiveTimelineKeyFacialEarData;
                updateInfo.earNext = liveTimelineKey2 as LiveTimelineKeyFacialEarData;
                updateInfo.earKeyIndex = curKey.index;
            }
        }

        public void AlterUpdate_LipSync(float liveTime)
        {
            var lipDataList = data.worksheetList[0].ripSyncKeys;

            LiveTimelineKeyIndex curKey = AlterUpdate_Key(lipDataList, liveTime);

            if (curKey != null && curKey.index != -1)
            {
                LiveTimelineKeyLipSyncData arg = curKey.key as LiveTimelineKeyLipSyncData;
                this.OnUpdateLipSync(arg, liveTime);
            }
        }

        private void AlterLateUpdate_FormationOffset(float liveTime)
        {

            var formationList = data.worksheetList[0].formationOffsetSet.Init();

            for (int i = 0; i < Director.instance.characterCount; i++)
            {
                LiveTimelineKeyIndex curKey = AlterUpdate_Key(formationList[i], liveTime);

                if (curKey != null && curKey.index != -1)
                {
                    LateUpdateFormationOffset_Transform(i, curKey, liveTime);
                }
            }
        }

        public void LateUpdateFormationOffset_Transform(int targetIndex, LiveTimelineKeyIndex curKeyIndex, float time)
        {
            LiveTimelineKeyFormationOffsetData curKey = curKeyIndex.key as LiveTimelineKeyFormationOffsetData;
            LiveTimelineKeyFormationOffsetData nextKey = curKeyIndex.nextKey as LiveTimelineKeyFormationOffsetData;

            Director.instance.charaObjs[targetIndex].position = curKey.Position;
        }



        public static void FindTimelineKeyCurrent(out LiveTimelineKeyIndex curKey, ILiveTimelineKeyDataList keys, float curTime)
        {
            curKey = keys.FindCurrentKey(curTime);
        }

        public static void UpdateTimelineKeyCurrent(out LiveTimelineKeyIndex curKey, ILiveTimelineKeyDataList keys, float curTime)
        {
            curKey = keys.UpdateCurrentKey(curTime);
        }

        public static LiveTimelineKeyIndex AlterUpdate_Key(ILiveTimelineKeyDataList keys, float curTime)
        {
            LiveTimelineKeyIndex curKey = keys.TimeKeyIndex;

            if (curKey.index == -1)
            {
                FindTimelineKeyCurrent(out curKey, keys, curTime);
            }
            else
            {
                UpdateTimelineKeyCurrent(out curKey, keys, curTime);
            }

            return curKey;
        }
    }

    
}

