using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyData_MultiLightShadow : LiveTimelineKeyWithInterpolate
    {
        public bool IsUseParam;
        public Vector3 ShadowCenterPosition;
        public Vector3 ShadowForward;
        public float ShadowFadeStart;
        public float ShadowFadeLength;
        public bool IsUseShadowFront;
        public bool IsShadowNearStart;
        public Color ShadowStartColor;
        public Color ShadowEndColor;
        public float ShadowDistance;
    }

    [System.Serializable]
    public class LiveTimelineKeyMultiLightShadowDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyData_MultiLightShadow>
    {

    }
}