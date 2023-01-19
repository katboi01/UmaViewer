using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyVolumeLightData : LiveTimelineKeyWithInterpolate, ILiveTimelineKeyHasColor
    {
        public Vector3 sunPosition = Vector3.zero;

        public Color color1 = Color.white;

        public float power;

        public float komorebi;

        public float blurRadius;

        public bool enable = true;

        public bool isEnabledBorderClear;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.VolumeLight;

        public Color GetRepresentativeColor()
        {
            return color1;
        }
    }
}
