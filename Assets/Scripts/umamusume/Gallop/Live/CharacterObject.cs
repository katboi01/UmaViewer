using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gallop.Live.Cutt;
using System.Runtime.CompilerServices;

namespace Gallop.Live
{
    public class CharacterObject : MonoBehaviour, ILiveTimelineCharactorLocator, ILiveTimelineMSQTarget
    {
        public enum RealtimeShadowType
        {
            HardShadow = 0,
            SoftShadow = 1
        }

        public const int DRESS_COUNT = 2;
        public const int DRESS_MAIN_INDEX = 0;
        public const int DRESS_SUB_INDEX = 1;
        private int _reserveDressModelCount; // 0x18
        private int _activeModelIndex; // 0x1C
        private GameObject[] _modelObjectArray; // 0x20
        private Transform[] _modelTransformArray; // 0x28
        //private LiveModelController[] _liveModelControllerArray; // 0x30
        //private GallopSweatController[] _sweatControllerArray; // 0x38
        //private EyeTraceController[] _eyeTraceControllerArray; // 0x40
        //private LiveModelController.EyeTargetPositionSetter[] _eyeTargetSetter; // 0x48
        private const int EffectAttachNum = 26;
        private Transform[,] _effectAttachTransformArray; // 0x50
        public int prevFrameWarmUpCyspring; // 0x58
        public int CurrentFrameEyeTrack; // 0x5C
        private List<Collider[]> _colliderListArray; // 0x60
        private bool _liveMSQControlled; // 0x68
        private float _liveMSQCurrentAnimStartTime; // 0x6C
        private bool _bVisible; // 0x70
        private LiveCharaPosition _liveCharaStandingPosition; // 0x74
        private Vector3 _liveCharaInitialPosition; // 0x78
        private const float kLiveCharaHeadPositionOffsetHeight = 0.1f;
        private int _liveCharaHeightLevel; // 0x84
        private float _liveCharaHeightValue; // 0x88
        private float _liveCharaHeightRatioBase; // 0x8C
        private float _liveCharaHeightRatio; // 0x90
        private Vector3 _liveCharaFormationHeightRateOffset; // 0x94
        private bool _isPositionNodePositionAddParent; // 0xA0
        private LiveTimelineDefine.FacialEyeTrackTargetType _eyeTrackTargetType; // 0xA4
        private GameObject _softShadowCamera; // 0xC0
        private GameObject _softShadowReceiver; // 0xC8
        private Transform _softShadowCameraTrans; // 0xD0
        private Transform _softShadowReceiverTrans; // 0xD8
        private const string SoftShadowObjectName = "Shadow Camera";
        private CharacterObject.RealtimeShadowType _realtimeShadowType; // 0xE0
        private bool _enableRealtimeShadow; // 0xE4
        private bool _isMirror; // 0xE5
        //private CastShadowEffect _dynamicShadow; // 0xE8
        private int _characterDataIndex; // 0xF0
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _motionNoiseTime; // 0xF4
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _motionNoiseRate; // 0xF8
        private bool _isUpdateMotionNoiseTime; // 0xFC
        protected bool _isLod; // 0xFD
        protected Transform _parentDefaultTransform; // 0x100
        protected Transform _parentTransform; // 0x108
        protected AnimationState _bodyAnimationState; // 0x110
        private bool _isSetting; // 0x118
        protected bool _isMeshActiveAuto; // 0x119
        private readonly Dictionary<int, Props> _dicProps; // 0x120
        private readonly List<int> _lstPropHashs; // 0x128
        //private FakeShadowController[] _shadowL; // 0x130
        //private FakeShadowController[] _shadowR; // 0x138
        //private FollowSpotLightController[] _followSpotLightControllers; // 0x140
        //private List<Spotlight3dController> _spotlight3dControllerList; // 0x148
        private Vector3 _eyeTrackDefaultPos; // 0x150

        public int ReserveDressModelCount { get; }
        public int ActiveModelIndex { get; }
        public int LayerIndex { get; set; }
        public Color EmissiveColor { get; set; }
        public GameObject[] ModelObjectArray { get; }
        public GameObject CurrentModelObject { get; }
        public Transform[] ModelTransformArray { get; }
        public Transform CurrentModelTransform { get; }
        //public LiveModelController[] LiveModelControllerArray { get; }
        //public LiveModelController CurrentLiveModelController { get; }
        public EyeTraceController[] EyeTraceControllerArray { get; }
        public Animation liveMSQAnimation { get; }
        public bool liveMSQControlled { get; set; }
        public AnimationState liveMSQCurrentAnimState { get; set; }
        public float liveMSQCurrentAnimStartTime { get; set; }
        public float heightRate { get; }
        //public LiveIK IKCtrl { get; }
        public bool liveCharaVisible { get; set; }
        public LiveCharaPosition liveCharaStandingPosition { get; set; }
        public Vector3 liveCharaInitialPosition { get; set; }
        public Vector3 liveCharaPosition { get; }
        public Quaternion liveCharaPositionLocalRotation { get; set; }
        public Vector3 liveCharaHeadPosition { get; }
        public Vector3 liveCharaWaistPosition { get; }
        public Vector3 liveCharaLeftHandWristPosition { get; }
        public Vector3 liveCharaLeftHandAttachPosition { get; }
        public Vector3 liveCharaRightHandWristPosition { get; }
        public Vector3 liveCharaRightHandAttachPosition { get; }
        public Vector3 liveCharaChestPosition { get; }
        public Vector3 liveCharaFootPosition { get; }
        public Vector3 liveCharaConstHeightHeadPosition { get; }
        public Vector3 liveCharaConstHeightWaistPosition { get; }
        public Vector3 liveCharaConstHeightChestPosition { get; }
        public Vector3 liveCharaInitialHeightHeadPosition { get; }
        public Vector3 liveCharaInitialHeightWaistPosition { get; }
        public Vector3 liveCharaInitialHeightChestPosition { get; }
        public Vector3 liveCharaScale { get; }
        public Transform liveParentDefaultTransform { get; set; }
        public Transform liveParentTransform { get; set; }
        public Transform liveRootTransform { get; }
        public int liveCharaHeightLevel { get; set; }
        public float liveCharaHeightValue { get; }
        public float liveCharaHeightRatioBase { get; set; }
        public float liveCharaHeightRatio { get; set; }
        public Vector3 liveCharaFormationHeightRateOffset { get; set; }
        public bool IsPositionNodePositionAddParent { get; set; }
        public bool IsCastShadow { get; set; }
        public float CySpringRate { get; set; }
        public LiveTimelineDefine.FacialEyeTrackTargetType EyeTrackTargetType { get; set; }
        public float EyeTrackVerticalRate { get; set; }
        public float EyeTrackHorizontalRate { get; set; }
        public Vector3 EyeTrackDirectWorldPosition { get; set; }
        public CharacterObject.RealtimeShadowType realtimeShadowType { get; set; }
        public bool enableRealtimeShadow { get; set; }
        public bool IsMirror { get; set; }
        public int CharacterDataIndex { get; set; }
        public bool IsUpdateMotionNoiseTime { get; set; }
        public bool IsSetting { get; }
        public static string FollowSpotLightParentName { get; set; }
        public FollowSpotLightController[] FollowSpotLightControllers { get; }
    }
}
