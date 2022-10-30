using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live.Cutt
{
	[System.Serializable]
	public class LiveTimelineKeySpotlight3dData : LiveTimelineKeyWithInterpolate
	{
		public enum TargetCameraType
		{
			Main = 0,
			Multi = 1
		}

		public bool isActive;
		public Color color;
		public float colorPower;
		public float localHeight;
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;
		public Vector3 characterPosition;
		public LiveTimelineKeySpotlight3dData.TargetCameraType targetCameraType;
		public int targetCameraIndex;
		public string assetName;
		public int characterIndex;
		private const int ATTR_DISABLE_BILLBOARD = 65536;
	}

	[System.Serializable]
	public class LiveTimelineKeySpotlight3dDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeySpotlight3dData>
	{

	}

	[System.Serializable]
    public class LiveTimelineSpotlight3dData : ILiveTimelineGroupDataWithName
    {
		private const string DEFAULT_NAME = "Spotlight3d";
		public LiveTimelineKeySpotlight3dDataList keys; // 0x28
		public const string ASSET_NAME = "spotlight3d";
		[SerializeField]
		private int _characterIndex;
		[SerializeField]
		private int _assetId;
		private bool _isUpdateController;
		private int _currentCharacterIndex;
	}
}
