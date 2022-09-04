using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyMonitorControlData : LiveTimelineKeyWithInterpolate
    {
        [System.Serializable]
        public class ChangeUVSetting
        {
            [System.Serializable]
            public class ChangeUVSettingCondition
            {
                public bool IsCheck;
                public int CharaId;
                public int DressId;
            }

            public bool IsChangeUVSetting;
            public int DispID;
            public LiveTimelineKeyMonitorControlData.ChangeUVSetting.ChangeUVSettingCondition[] ConditionArray;
        }

        public Vector2 position;
        public Vector2 size;
        public int dispID;
        public float speed;
        public string outputTextureLabel;
        public int playStartOffsetFrame;
        public float blendFactor;
        public Color colorFade;
        public int RenderQueueNo;
        public bool IsRenderQueue;
        public int DispID2;
        public float CrossFadeRate;
        public int LightImageNo; 
        public int LightImageNo2;
        public LiveTimelineKeyMonitorControlData.ChangeUVSetting[] ChangeUVSettingArray;
    }


    [System.Serializable]
    public class LiveTimelineKeyMonitorControlDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyMonitorControlData>
    {

    }

    [System.Serializable]
    public class LiveTimelineMonitorControlData : ILiveTimelineGroupDataWithName
    {
        public LiveTimelineKeyMonitorControlDataList keys;
    }
}
