using System;
using UnityEngine;

namespace Cutt
{
    [Serializable]
    public class LiveTimelineKeyEffectData : LiveTimelineKeyWithInterpolate
    {
        private enum eEffectStatus
        {
            Play,
            Stop
        }

        private const int FLAG_LOOP = 262144;

        private const int FLAG_PLAY = 524288;

        private const int FLAG_CLEAR = 1048576;

        private const int FLAG_STAY_PRS = 2097152;

        public eEffectBlendMode blendMode;

        public Color color = Color.white;

        public float colorPower = 1f;

        public eEffectOwner owner = eEffectOwner.World;

        public eEffectOccurrenceSpot occurrenceSpot = eEffectOccurrenceSpot.Hand_Attach_L;

        public Vector3 offset = Vector3.zero;

        public Vector3 offsetAngle = Vector3.zero;

        public override LiveTimelineKeyDataType dataType => LiveTimelineKeyDataType.Effect;

        public bool IsPlay()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)FLAG_PLAY);
        }

        public bool IsLoop()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)FLAG_LOOP);
        }

        public bool IsClear()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)FLAG_CLEAR);
        }

        public bool IsStayPRS()
        {
            return attribute.hasFlag((LiveTimelineKeyAttribute)FLAG_STAY_PRS);
        }
    }
}
