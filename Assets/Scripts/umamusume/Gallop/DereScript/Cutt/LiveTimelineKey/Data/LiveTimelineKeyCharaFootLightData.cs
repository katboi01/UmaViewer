using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaFootLightData : LiveTimelineKeyWithInterpolate
    {
        public LiveCharaPositionFlag positionFlag;

        public float[] hightMax = new float[15];

        public Color[] lightColor = new Color[15];

        public bool[] alphaBlend = new bool[15];

        public bool[] inverseAlpha = new bool[15];

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaFootLight;
    }
}
