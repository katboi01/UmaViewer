using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyData_AdditionalLight : LiveTimelineKeyWithInterpolate
    {
        public enum MyLightType
        {
            Directional = 0,
            Spot = 1,
            Point = 2
        }

        public Vector3 Position;
        public Vector3 Rotate;
        public bool IsEnable;
        public LiveTimelineKeyData_AdditionalLight.MyLightType Type;
        public float Range;
        public int SpotAngle;
        public int IndirectMultiplier;
        public LightShadows ShadowType;
        public float Strength;
        public float Bias;
        public float NormalBias;
        public float NearPlane;
    }

    [System.Serializable]
    public class LiveTimelineKeyAdditionalLightList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyData_AdditionalLight>
    {

    }

    [System.Serializable]
    public class LiveTimelineAdditionalLight : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "Light";
        public LiveTimelineKeyAdditionalLightList keys;
    }
}
