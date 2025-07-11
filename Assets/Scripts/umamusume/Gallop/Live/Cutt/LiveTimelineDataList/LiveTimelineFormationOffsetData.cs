using Gallop.Live.Cutt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Gallop.Live.Cutt
{
	[System.Serializable]
	public class LiveTimelineKeyFormationOffsetData : LiveTimelineKeyWithInterpolate
	{
		private enum IKTargetChara
		{
			_1st = 0,
			_2nd = 1,
			_3rd = 2,
			_4th = 3,
			_5th = 4,
			_6th = 5,
			_7th = 6,
			_8th = 7,
			_9th = 8,
			_10th = 9,
			_11th = 10,
			_12th = 11,
			_13th = 12,
			_14th = 13,
			_15th = 14,
			_16th = 15,
			_17th = 16,
			_18th = 17
		}

		public enum IKSystemType
		{
			None = 0,
			HoldHands3p_Center = 1,
			HoldHands3p_Outside = 2,
			Limb = 3,
			MicStand = 4,
			HoldHandsMix = 5
		}

		public enum IKSystemHoldHandTarget
		{
			None = 0,
			HandAttachL = 1,
			HandAttachR = 2
		}

        public Vector3 Position;
        public float RotationY;
        public float LocalRotationY;
        public bool visible;
        public int DressIndex;
        public bool warmUpCySpring;
        public bool isEnabledOffset;
        public Vector3 offsetMaxPosition;
        public Vector3 offsetMinPosition;
        public bool isEnabledOffsetCameraTargetChara;
        public bool IsWorldSpace;
        public Vector3 WorldSpaceOrigin;
        public float WorldRotationY;
        public bool IsLookAtWorldOrigin;
        public bool IsPositionAddParentNode;
        public string ParentObjectName;
        public bool IsCastShadow;
        public float CySpringRate;
        public bool IsLayerIndex;
        public int LayerIndex;
        public bool IsEmissiveColor;
        public Color EmissiveColor;
        public float EmissiveScrollTimeScale;
        public float EmissiveScrollEnergyScale;
        private static readonly string[] IKSYSTEM_HOLD_HAND_TARGET_NODE_NAME_ARRAY;
        public LiveTimelineKeyFormationOffsetData.IKSystemType IKSystem;
        public int IKSystemParam1;
        public int IKSystemParam2;
        public int IKSystemParam3;
        public int IKSystemParam4;
        public int PositionPriority;
        public bool IsEnabledIKMicStandLOffset;
        public bool IsEnabledIKMicStandROffset;
        public Vector3 IKMicStandLOffsetHigh;
        public Vector3 IKMicStandLOffsetLow;
        public Vector3 IKMicStandROffsetHigh;
        public Vector3 IKMicStandROffsetLow;
        [SerializeField]
        private int ver;
        [SerializeField]
        private Vector2 posXZ;
        [SerializeField]
        private float posY;
        [SerializeField]
        private Vector3 position;
        [SerializeField]
        private float rotY;
        private const int ATTR_RESET_CLOTH = 65536;
        private const int ATTR_PARENT_OBJECT = 131072;
        private int _parentObjectNameHash;
        private bool _isUpdatedParentObject;
        private Transform _parentObjectTransform;
        private int _parentObjectTargetCount;

        public Transform GetParentObjectTransform(LiveTimelineControl liveTimelineControl)
        {
            if (_parentObjectTransform != null)
            {
                return _parentObjectTransform;
            }

            if (liveTimelineControl.StageObjectMap.TryGetValue(ParentObjectName, out GameObject gameObject))
            {
                _parentObjectTransform = gameObject.transform;
                return _parentObjectTransform;
            }

            return null;
        }
    }

	[System.Serializable]
	public class LiveTimelineKeyFormationOffsetDataList : LiveTimelineKeyDataListTemplate<LiveTimelineKeyFormationOffsetData>
	{

	}

	[System.Serializable]
	public class LiveTimelineFormationOffsetData
    {

        private const int DATA_LIST_SIZE = 20;
        public LiveTimelineKeyFormationOffsetDataList centerKeys; // 0x10
        public LiveTimelineKeyFormationOffsetDataList left1Keys; // 0x18
        public LiveTimelineKeyFormationOffsetDataList right1Keys; // 0x20
        public LiveTimelineKeyFormationOffsetDataList left2Keys; // 0x28
        public LiveTimelineKeyFormationOffsetDataList right2Keys; // 0x30
        public LiveTimelineKeyFormationOffsetDataList place06Keys; // 0x38
        public LiveTimelineKeyFormationOffsetDataList place07Keys; // 0x40
        public LiveTimelineKeyFormationOffsetDataList place08Keys; // 0x48
        public LiveTimelineKeyFormationOffsetDataList place09Keys; // 0x50
        public LiveTimelineKeyFormationOffsetDataList place10Keys; // 0x58
        public LiveTimelineKeyFormationOffsetDataList place11Keys; // 0x60
        public LiveTimelineKeyFormationOffsetDataList place12Keys; // 0x68
        public LiveTimelineKeyFormationOffsetDataList place13Keys; // 0x70
        public LiveTimelineKeyFormationOffsetDataList place14Keys; // 0x78
        public LiveTimelineKeyFormationOffsetDataList place15Keys; // 0x80
        public LiveTimelineKeyFormationOffsetDataList place16Keys; // 0x88
        public LiveTimelineKeyFormationOffsetDataList place17Keys; // 0x90
        public LiveTimelineKeyFormationOffsetDataList place18Keys; // 0x98
        public LiveTimelineKeyFormationOffsetDataList place19Keys; // 0xA0
        public LiveTimelineKeyFormationOffsetDataList place20Keys; // 0xA8
        private readonly ILiveTimelineKeyDataList[] _cacheDataList; // 0xB0

        public List<LiveTimelineKeyFormationOffsetDataList> Init()
        {
            return new List<LiveTimelineKeyFormationOffsetDataList> { centerKeys, left1Keys , right1Keys, left2Keys, right2Keys, place06Keys, place07Keys, place08Keys, place09Keys, place10Keys, place11Keys, place12Keys, place13Keys, place14Keys, place15Keys, place16Keys, place17Keys, place18Keys, place19Keys, place20Keys };
        }
    }
}
