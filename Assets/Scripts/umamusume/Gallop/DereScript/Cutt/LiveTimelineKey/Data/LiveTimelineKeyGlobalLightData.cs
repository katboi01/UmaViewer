using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyGlobalLightData : LiveTimelineKeyWithInterpolate
    {
        public Vector3 lightDir = new Vector3(-75f, 0f, 0f);

        public float globalRimRate = 1f;

        public float globalRimShadowRate;

        public float globalRimSpecularRate = 1f;

        public float globalToonRate = 1f;

        public bool cameraFollow;
        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.GlobalLight;
    }
}
