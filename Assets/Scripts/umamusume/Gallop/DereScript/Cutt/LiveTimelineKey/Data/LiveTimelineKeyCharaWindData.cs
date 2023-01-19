using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyCharaWindData : LiveTimelineKeyWithInterpolate
    {
        public enum WindMode
        {
            None,
            Sin,
            Wind
        }

        public bool enable;

        public Vector3 cySpringForceScale = Vector3.one;

        public Vector3 windPower;

        public float loopTime = 1f;

        public WindMode windMode = WindMode.Sin;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.CharaWind;
    }
}
