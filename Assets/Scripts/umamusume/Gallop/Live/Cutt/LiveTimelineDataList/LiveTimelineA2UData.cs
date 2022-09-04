using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
    [System.Serializable]
    public class LiveTimelineKeyA2UData : LiveTimelineKeyWithInterpolate
    {
		private static readonly float MinOpacity;
		private static readonly float MaxOpacity;
		public Color spriteColor;
		public Vector2 position;
		public Vector2 scale;
		public float rotationZ;
		public int textureIndex;
		public int appearanceRandomSeed;
		public float spriteAppearance;
		public int slopeRandomSeed;
		public float spriteMinSlope;
		public float spriteMaxSlope;
		public float spriteScale;
		public float spriteOpacity;
		public float startSec;
		public float speed;
		public bool isFlick;
		public bool enable;
	}

    [System.Serializable]
    public class LiveTimelineKeyA2UDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyA2UData>
    {

    }

    [System.Serializable]
    public class LiveTimelineA2UData : ILiveTimelineGroupDataWithName
    {
        private const string default_name = "A2U";
        public LiveTimelineKeyA2UDataList keys;
    }
}
