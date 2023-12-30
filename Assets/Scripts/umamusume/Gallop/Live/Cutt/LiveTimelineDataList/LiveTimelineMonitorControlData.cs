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

        public enum ExtraContent
        {
            None = 0,
            ChampionsMeetingLogo = 1,
            ChampionsMeetingGoal = 2
        }

        public const int CHANGE_UV_SETTING_MAX = 1;
        public Vector2 position;
        public Vector2 size;
        public int dispID;
        public float speed;
        public string outputTextureLabel;
        public int playStartOffsetFrame;
        public float blendFactor;
        public Color colorFade;
        public Color BaseColor;
        public int RenderQueueNo;
        public bool IsRenderQueue;
        public int DispID2;
        public float CrossFadeRate;
        public int LightImageNo;
        public int LightImageNo2;
        public LiveTimelineKeyMonitorControlData.ChangeUVSetting[] ChangeUVSettingArray;
        public LiveTimelineKeyMonitorControlData.ExtraContent extraContent;
        private const int ATTR_MULTI = 65536;
        private const int ATTR_PLAY_REVERSE = 131072;
        private const int ATTR_USE_MONITOR_CAMERA = 262144;
        private const int ATTR_FORCED_USE_MONITOR_CAMERA = 524288;
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
