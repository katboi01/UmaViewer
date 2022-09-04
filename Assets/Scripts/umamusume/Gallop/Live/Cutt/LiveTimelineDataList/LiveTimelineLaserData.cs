using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
	public enum LaserFormation
	{
		Off = 0,
		One = 1,
		Linear_2 = 2,
		Linear_3 = 3,
		Linear_4 = 4,
		Linear_5 = 5,
		LightOnly = 6,
		Circle_2 = 8,
		Circle_3 = 9,
		Circle_4 = 10,
		Circle_5 = 11
	}

	public enum LaserBlink
	{
		None = 0,
		All = 1,
		Random = 2,
		AscendingOn = 3,
		DescendingOn = 4,
		AscendingOff = 5,
		DescendingOff = 6,
		Max = 7
	}

	[System.Serializable]
	public class LiveTimelineKeyLaserData : LiveTimelineKeyWithInterpolate
	{
		public Vector3 objectPosition;
		public Vector3 objectRotate;
		public Vector3 objectScale;
		public LaserFormation formation;
		public Vector3 rotate;
		public float degRootYaw;
		public float degLaserPitch;
		public float posInterval;
		public LaserBlink blink;
		public float blinkPeriod;
		public float RaycastDistance;
		private const int ATTR_ENABLE_RENDER = 65536;
		private const int ATTR_ENABLE_RAYCAST = 131072;
		private const int ATTR_DISABLE_ROOT_LIGHT = 262144;
	}

	[System.Serializable]
	public class LiveTimelineKeyLaserDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyLaserData>
	{

	}

	[System.Serializable]
	public class LiveTimelineLaserData : ILiveTimelineGroupDataWithName
    {
		private const string DEFAULT_NAME = "Laser Object";
		public LiveTimelineKeyLaserDataList keys;
		[SerializeField]
		private int _objectIndex;
		[SerializeField]
		private int _materialIndex;
	}
}
