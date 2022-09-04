using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyCharaMotionNoiseData : LiveTimelineKey
    {
		public enum GravityType
		{
			None = 0,
			Scale = 1,
			AddValue = 2,
			SetValue = 3
		}

		public float sideChrMotNoiseBaseBias;
		public float sideChrMotNoiseRange;
		public float sideChrMotNoiseFrequency;
		public float backChrMotNoiseBaseBias;
		public float backChrMotNoiseRange;
		public float backChrMotNoiseFrequency;
		public bool isNegativeCheck;
		public LiveTimelineKeyCharaMotionNoiseData.GravityType[] cySpringGravityScaleType;
		public float[] cySpringGravityScale;
		public string[] cySpringGravityScaleBoneName;
		public float[] cySpringExpressionKneeCollisionRadius;
		public float[] cySpringExpressionAnkleCollisionRadius;
		public float[] cySpringExpressionInfluenceAngle;
		public float[] cySpringExpressionInfluenceMaxAngle;
	}


    [System.Serializable]
    public class LiveTimelineKeyCharaMotionNoiseDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyCharaMotionNoiseData>
    {

    }
}
