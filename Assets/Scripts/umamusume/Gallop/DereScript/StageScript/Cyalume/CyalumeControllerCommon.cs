using System;
using UnityEngine;

namespace Stage.Cyalume
{
    public class CyalumeControllerCommon
    {
        public struct ChoreographyInfo
        {
            public ChoreographyCyalume.ChoreographyCyalumeData choreographyData;

            public float localTime;

            public float laps;

            public static readonly ChoreographyInfo dummyData;
        }

        private const string CYALUME_PATH = "MusicScores/m{0:d3}/m{0:d3}_cyalume";

        private const string CYALUME_REPLACE_PATH = "MusicScores/m{0:d3}/m{1:d3}_cyalume";

        private MasterCyalumeColorData _masterCyalumeColorData;

        private ChoreographyCyalume _choreographyCyalume;

        private float _choreographyTime;

        private int _musicId;
        public ChoreographyCyalume choreographyCyalume => _choreographyCyalume;

        public float choreographyTime
        {
            get
            {
                return _choreographyTime;
            }
            set
            {
                _choreographyTime = value;
            }
        }

        public int musicId => _musicId;

        public bool LoadChoreography(int id, int beforeId)
        {
            _masterCyalumeColorData = SingletonMonoBehaviour<MasterDBManager>.instance.GetCyalumeColorData();
            _choreographyCyalume = new ChoreographyCyalume(_masterCyalumeColorData);
            string strPath = string.Format("MusicScores/m{0:d3}/m{1:d3}_cyalume", beforeId, id);
            bool num = _choreographyCyalume.Load(strPath);
            if (!num)
            {
                id = 1;
                strPath = string.Format("MusicScores/m{0:d3}/m{0:d3}_cyalume", id, id);
                _choreographyCyalume.Load(strPath);
            }
            _musicId = id;
            return num;
        }

        public void Start()
        {
            _choreographyTime = 0f;
        }

        public void Update()
        {
        }

        public ChoreographyInfo getNowChoreographyCyalumeData()
        {
            ChoreographyInfo result = default(ChoreographyInfo);
            result.choreographyData = _choreographyCyalume.getChoreographyDataFromTime(_choreographyTime);
            if (result.choreographyData != null)
            {
                result.localTime = _choreographyTime - result.choreographyData._startTime;
                if (result.localTime > 0f)
                {
                    if (result.choreographyData._playSpeed == 0f)
                    {
                        result.laps = 0f;
                    }
                    else
                    {
                        result.laps = result.localTime / ChoreographyCyalume.PlaySpeedToAnimationTime(result.choreographyData._playSpeed);
                    }
                }
                else
                {
                    result.laps = 0f;
                }
            }
            else
            {
                result.localTime = 0f;
                result.laps = 0f;
            }
            return result;
        }

        public ChoreographyInfo getChoreographyCyalumeDataFromNo(int no)
        {
            ChoreographyInfo dummyData = ChoreographyInfo.dummyData;
            if (_choreographyCyalume != null)
            {
                dummyData.choreographyData = _choreographyCyalume.getChoreographyDataFromNo(no);
            }
            return dummyData;
        }

        public Color GetMobColor()
        {
            ChoreographyInfo dummyData = ChoreographyInfo.dummyData;
            if (_choreographyCyalume != null)
            {
                dummyData.choreographyData = _choreographyCyalume.getChoreographyDataFromNo(_choreographyCyalume.GetDataNum() - 1);
            }
            if (dummyData.choreographyData._colorPattern == ChoreographyCyalume.ColorPattern.SetMobColor)
            {
                return dummyData.choreographyData._colorData[0]._inColor;
            }
            return MasterCyalumeColorData._defaultColor._inColor;
        }
    }
}
