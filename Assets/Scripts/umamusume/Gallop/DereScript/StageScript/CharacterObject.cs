using Cutt;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

public class CharacterObject : Character3DBase, ILiveTimelineCharactorLocator, ILiveTimelineMSQTarget
{
    public enum OverrideClothParameterType
    {
        Reset,
        Soft,
        Usually,
        Hard,
        SuperHard,
        Suppresion
    }

    public enum IKIndex
    {
        Leg_L,
        Leg_R,
        Wrist_L,
        Wrist_R,
        Max
    }

    public enum IKFixType
    {
        None,
        Position,
        Root,
        IKPositionSelfHeight
    }

    public enum EyeTargetVerticalPos
    {
        Middle,
        Up,
        Down
    }

    public enum RealtimeShadowType
    {
        HardShadow,
        SoftShadow
    }

    private static readonly Vector3 kLiveCharaHeadPositionOffset = new Vector3(0f, 0.1f, 0f);

    private static readonly Vector3 kLiveCharaLeftHandPosition = new Vector3(0.5f, 0f, 0f);

    private static readonly Vector3 kLiveCharaRightHandPosition = new Vector3(-0.5f, 0f, 0f);

    private LiveCharaPosition _liveCharaStandingPosition;

    private Vector3 _liveCharaInitialPosition = Vector3.zero;

    private Transform _leftHandWristTransform;

    private Transform _leftHandAttachTransform;

    private Transform _rightHandWristTransform;

    private Transform _rightHandAttachTransform;

    private float _initialHeadHeight;

    private float _initialWaistHeight;

    private float _initialChestHeight;

    private int _liveCharaHeightLevel;

    private float _liveCharaHeightValue;

    private float _liveCharaHeightRatio = 1f;

    private bool _liveMSQControlled;

    private float _liveMSQCurrentAnimStartTime;

    [SerializeField]
    protected GameObject _eyeTarget;

    protected Transform _eyeTargetTrans;

    protected FacialEyeTrackTarget _curAvertOnesEyeTarget;

    private CySpringRootBone[] _overrideClothParameterList;

    private CySpringRootBone[] _overrideClothParameterListAll;

    private CySpringRootBone[] _overrideClothParameterAccesory;

    private CySpringRootBone[] _overrideClothParameterFurisode;

    protected Master3DCharaData.Behavior _behavior;

    protected fakeShadow[] _shadowR;

    protected fakeShadow[] _shadowL;

    protected FollowSpotLightController[] _followSpotLightController;

    [SerializeField]
    protected GameObject _r_leg;

    [SerializeField]
    protected GameObject _l_leg;

    protected Transform _waistTransform;

    protected Transform _chestTransform;

    private const int IKIndexNum = 4;


    private static readonly BodyParts.eTransform[] IKPoleVectorTable = new BodyParts.eTransform[4]
    {
        BodyParts.eTransform.Pole_Leg_L,
        BodyParts.eTransform.Pole_Leg_R,
        BodyParts.eTransform.Pole_Arm_L,
        BodyParts.eTransform.Pole_Arm_R
    };

    private static readonly BodyParts.eTransform[] IKEffectorTable = new BodyParts.eTransform[4]
    {
        BodyParts.eTransform.Eff_Leg_L,
        BodyParts.eTransform.Eff_Leg_R,
        BodyParts.eTransform.Eff_Wrist_L,
        BodyParts.eTransform.Eff_Wrist_R
    };

    private static readonly LiveCharaPosition[] EyeTrackAvertLivePositionTable = new LiveCharaPosition[52]
    {
        (LiveCharaPosition)(-1),
        (LiveCharaPosition)(-1),
        (LiveCharaPosition)(-1),
        (LiveCharaPosition)(-1),
        (LiveCharaPosition)(-1),
        LiveCharaPosition.Center,
        LiveCharaPosition.Center,
        LiveCharaPosition.Center,
        LiveCharaPosition.Left1,
        LiveCharaPosition.Left1,
        LiveCharaPosition.Left1,
        LiveCharaPosition.Right1,
        LiveCharaPosition.Right1,
        LiveCharaPosition.Right1,
        LiveCharaPosition.Left2,
        LiveCharaPosition.Left2,
        LiveCharaPosition.Left2,
        LiveCharaPosition.Right2,
        LiveCharaPosition.Right2,
        LiveCharaPosition.Right2,
        LiveCharaPosition.Left3,
        LiveCharaPosition.Left3,
        LiveCharaPosition.Left3,
        LiveCharaPosition.Right3,
        LiveCharaPosition.Right3,
        LiveCharaPosition.Right3,
        LiveCharaPosition.Left4,
        LiveCharaPosition.Left4,
        LiveCharaPosition.Left4,
        LiveCharaPosition.Right4,
        LiveCharaPosition.Right4,
        LiveCharaPosition.Right4,
        LiveCharaPosition.Left5,
        LiveCharaPosition.Left5,
        LiveCharaPosition.Left5,
        LiveCharaPosition.Right5,
        LiveCharaPosition.Right5,
        LiveCharaPosition.Right5,
        LiveCharaPosition.Left6,
        LiveCharaPosition.Left6,
        LiveCharaPosition.Left6,
        LiveCharaPosition.Right6,
        LiveCharaPosition.Right6,
        LiveCharaPosition.Right6,
        LiveCharaPosition.Left7,
        LiveCharaPosition.Left7,
        LiveCharaPosition.Left7,
        LiveCharaPosition.Right7,
        LiveCharaPosition.Right7,
        LiveCharaPosition.Right7,
        (LiveCharaPosition)(-1),
        (LiveCharaPosition)(-1)
    };

    protected StageTwoBoneIK[] _ik;

    private Vector3[] _positionOffsetRate;

    private IKIndex[] _positionOffsetTargetEffector;

    private IKFixType _positionOffsetTargetType = IKFixType.Position;

    private Transform _bodyPosition;

    protected FacialEyeTrackTarget _eyeTrackTargetType;

    protected EyeTargetVerticalPos _eyeTrackTargetVPos;

    protected static readonly Vector3 kEyeTargetStageLeftOffset = new Vector3(100f, 0f, 0f);

    protected static readonly Vector3 kEyeTargetStageRightOffset = new Vector3(-100f, 0f, 0f);

    protected Vector3 _eyeTargetStageUpOffset = new Vector3(0f, 3f, 0f);

    protected Vector3 _eyeTargetStageDownOffset = new Vector3(0f, -3f, 0f);

    protected Vector3 _eyeTrackDefaultPos = Vector3.zero;

    protected bool _eyeTrackAvertEnable = true;

    protected bool _eyeTrackAvertFacialEnable = true;

    protected bool _eyeTrackForceAvertEnable;

    protected bool _eyeTrackForceAvertFacialEnable;

    protected readonly bool[] _eyeTrackAvertFlagArray = new bool[52]
    {
        true, true, true, true, true, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, true
    };

    private bool _isEnabledAlterUpdateSelf = true;

    private bool _isRich;

    [SerializeField]
    protected float _motionNoiseTime;

    [SerializeField]
    private float _motionNoiseRate;

    private const float ScaleToUnit = 0.01f;

    private const float IKTargetMinScale = 1f;

    private const float IKTargetMaxScale = 1.4546876f;

    private const float IKTargetDiffScale = 0.4546876f;

    private GameObject _softShadowCamera;

    private GameObject _softShadowReceiver;

    private Transform _softShadowCameraTrans;

    private Transform _softShadowReceiverTrans;

    private RealtimeShadowType _realtimeShadowType;

    private bool _enableRealtimeShadow;

    public const float CySpringSoftValue = 0.05f;

    public const float CySpringUsuallyValue = 0.1f;

    public const float CySpringHardValue = 0.15f;

    public const float CySpringSuperHardValue = 2f;

    private bool _isSuppression;

    private bool _isSpareCharacter;

    private CharacterObject[] _spareCharacters;


    public LiveCharaPosition liveCharaStandingPosition
    {
        get
        {
            return _liveCharaStandingPosition;
        }
        set
        {
            _liveCharaStandingPosition = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._liveCharaStandingPosition = value;
                }
            }
        }
    }

    public Vector3 liveCharaInitialPosition
    {
        get
        {
            return _liveCharaInitialPosition;
        }
        set
        {
            _liveCharaInitialPosition = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._liveCharaInitialPosition = value;
                }
            }
        }
    }

    public Vector3 liveCharaHeadPosition
    {
        get
        {
            if (_bodyNodeHead != null)
            {
                return _bodyNodeHead.position + kLiveCharaHeadPositionOffset;
            }
            return Vector3.zero;
        }
    }

    public Vector3 liveCharaWaistPosition
    {
        get
        {
            if (_waistTransform != null)
            {
                return _waistTransform.position;
            }
            return Vector3.zero;
        }
    }

    public Vector3 liveCharaLeftHandWristPosition
    {
        get
        {
            if (_leftHandWristTransform != null)
            {
                return _leftHandWristTransform.position;
            }
            return liveCharaWaistPosition + kLiveCharaLeftHandPosition;
        }
    }

    public Vector3 liveCharaLeftHandAttachPosition
    {
        get
        {
            if (_leftHandAttachTransform != null)
            {
                return _leftHandAttachTransform.position;
            }
            return liveCharaWaistPosition + kLiveCharaLeftHandPosition;
        }
    }

    public Vector3 liveCharaRightHandAttachPosition
    {
        get
        {
            if (_rightHandAttachTransform != null)
            {
                return _rightHandAttachTransform.position;
            }
            return liveCharaWaistPosition + kLiveCharaRightHandPosition;
        }
    }

    public Vector3 liveCharaRightHandWristPosition
    {
        get
        {
            if (_rightHandWristTransform != null)
            {
                return _rightHandWristTransform.position;
            }
            return liveCharaWaistPosition + kLiveCharaRightHandPosition;
        }
    }

    public Vector3 liveCharaChestPosition
    {
        get
        {
            if (_chestTransform != null)
            {
                return _chestTransform.position;
            }
            return Vector3.zero;
        }
    }

    public Vector3 liveCharaFootPosition
    {
        get
        {
            Vector3 result = liveCharaChestPosition;
            result.y = 0f;
            return result;
        }
    }

    public Vector3 liveCharaConstHeightHeadPosition
    {
        get
        {
            Vector3 result = _liveCharaInitialPosition;
            result.y = _initialHeadHeight;
            return result;
        }
    }

    public Vector3 liveCharaConstHeightWaistPosition
    {
        get
        {
            Vector3 result = _liveCharaInitialPosition;
            result.y = _initialWaistHeight;
            return result;
        }
    }

    public Vector3 liveCharaConstHeightChestPosition
    {
        get
        {
            Vector3 result = _liveCharaInitialPosition;
            result.y = _initialChestHeight;
            return result;
        }
    }

    public Animation liveAnimationComponent => _bodyParts.animation;

    public Transform liveParentTransform
    {
        get
        {
            return _parentTransform;
        }
        set
        {
            _parentTransform = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._parentTransform = value;
                }
            }
        }
    }

    public Transform liveRootTransform => _cachedTransform;

    public int liveCharaHeightLevel
    {
        get
        {
            return _liveCharaHeightLevel;
        }
        set
        {
            _liveCharaHeightLevel = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._liveCharaHeightLevel = value;
                }
            }
        }
    }

    public float liveCharaHeightValue => _liveCharaHeightValue;

    public float liveCharaHeightRatio
    {
        get
        {
            return _liveCharaHeightRatio;
        }
        set
        {
            _liveCharaHeightRatio = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._liveCharaHeightRatio = value;
                }
            }
        }
    }

    public Animation liveMSQAnimation
    {
        get
        {
            if (_bodyParts == null)
            {
                return null;
            }
            return _bodyParts.animation;
        }
    }

    public bool liveMSQControlled
    {
        get
        {
            return _liveMSQControlled;
        }
        set
        {
            _liveMSQControlled = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._liveMSQControlled = value;
                }
            }
        }
    }

    public AnimationState liveMSQCurrentAnimState
    {
        get
        {
            if (_bodyParts == null)
            {
                return null;
            }
            return _bodyParts.animationState;
        }
        set
        {
            if (_bodyParts != null)
            {
                _bodyParts.animationState = value;
            }
            if (_spareCharacters == null)
            {
                return;
            }
            CharacterObject[] array = _spareCharacters;
            foreach (CharacterObject characterObject in array)
            {
                if (characterObject._bodyParts != null)
                {
                    characterObject._bodyParts.animationState = value;
                }
            }
        }
    }

    public float liveMSQCurrentAnimStartTime
    {
        get
        {
            return _liveMSQCurrentAnimStartTime;
        }
        set
        {
            _liveMSQCurrentAnimStartTime = value;
            if (_spareCharacters != null)
            {
                CharacterObject[] array = _spareCharacters;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i]._liveMSQCurrentAnimStartTime = value;
                }
            }
        }
    }

    public ILiveTimelineMSQTarget[] spareTarget => _spareCharacters;

    public LipSyncController faceController => _lipSyncController;

    public Master3DCharaData.Behavior Behavior
    {
        get
        {
            return _behavior;
        }
        set
        {
            _behavior = value;
        }
    }

    public FollowSpotLightController[] followSpotLightController => _followSpotLightController;

    public FacialEyeTrackTarget eyeTrackTargetType
    {
        set
        {
            _eyeTrackTargetType = value;
        }
    }

    public bool eyeTrackAvert
    {
        set
        {
            _eyeTrackAvertEnable = value;
        }
    }

    public bool eyeTrackAvertFacial
    {
        set
        {
            _eyeTrackAvertFacialEnable = value;
        }
    }

    public bool eyeTrackForceAvert
    {
        set
        {
            _eyeTrackForceAvertEnable = value;
        }
    }

    public bool eyeTrackForceAvertFacial
    {
        set
        {
            _eyeTrackForceAvertFacialEnable = value;
        }
    }

    public EyeTargetVerticalPos eyeTrackTargetVPos
    {
        set
        {
            _eyeTrackTargetVPos = value;
        }
    }

    public bool isEnabledAlterUpdateSelf
    {
        get
        {
            return _isEnabledAlterUpdateSelf;
        }
        set
        {
            _isEnabledAlterUpdateSelf = value;
        }
    }

    public static bool disableErrorCheck { get; set; }

    public float motionNoiseTime
    {
        get
        {
            return _motionNoiseTime;
        }
        set
        {
            _motionNoiseTime = value;
        }
    }

    public float motionNoiseRate
    {
        get
        {
            return _motionNoiseRate;
        }
        set
        {
            _motionNoiseRate = value;
        }
    }

    public RealtimeShadowType realtimeShadowType
    {
        get
        {
            return _realtimeShadowType;
        }
        set
        {
            _realtimeShadowType = value;
            if (_softShadowCamera != null)
            {
                BlurOptimized component = _softShadowCamera.GetComponent<BlurOptimized>();
                if (component != null)
                {
                    component.enabled = ((_realtimeShadowType == RealtimeShadowType.SoftShadow) ? true : false);
                }
            }
        }
    }

    public bool enableRealtimeShadow
    {
        get
        {
            return _enableRealtimeShadow;
        }
        set
        {
            if (_enableRealtimeShadow == value)
            {
                return;
            }
            _enableRealtimeShadow = value;
            if (_softShadowCamera != null && _softShadowReceiver != null)
            {
                _softShadowCamera.SetActive(value);
                _softShadowReceiver.SetActive(value);
            }
            if (_shadowL != null)
            {
                for (int i = 0; i < _shadowL.Length; i++)
                {
                    _shadowL[i].Visible = !enableRealtimeShadow;
                }
            }
            if (_shadowR != null)
            {
                for (int j = 0; j < _shadowR.Length; j++)
                {
                    _shadowR[j].Visible = !enableRealtimeShadow;
                }
            }
        }
    }

    public bool isSpareCharacter => _isSpareCharacter;

    public CharacterObject[] spareCharacters
    {
        get
        {
            return _spareCharacters;
        }
        set
        {
            _spareCharacters = value;
        }
    }

    protected override void OnLiveCharaVisible(bool bVisible)
    {
        int num = 0;
        if (enableRealtimeShadow)
        {
            if (_shadowR != null)
            {
                for (num = 0; num < _shadowR.Length; num++)
                {
                    _shadowR[num].Visible = false;
                }
            }
            if (_shadowL != null)
            {
                for (num = 0; num < _shadowL.Length; num++)
                {
                    _shadowL[num].Visible = false;
                }
            }
            return;
        }
        if (_shadowR != null)
        {
            for (num = 0; num < _shadowR.Length; num++)
            {
                _shadowR[num].Visible = bVisible;
            }
        }
        if (_shadowL != null)
        {
            for (num = 0; num < _shadowL.Length; num++)
            {
                _shadowL[num].Visible = bVisible;
            }
        }
    }

    public void LiveTimeline_OnResetCloth()
    {
        ResetCloth();
    }

    public Vector3 getLegR()
    {
        return _r_leg.transform.position;
    }

    public Vector3 getLegL()
    {
        return _l_leg.transform.position;
    }

    public void SetAttachIKParameterType(IKFixType type)
    {
        _positionOffsetTargetType = type;
        for (int i = 0; i < 4; i++)
        {
            _positionOffsetTargetEffector[i] = IKIndex.Max;
        }
    }

    public void SetAttachIKParameter(int index, IKIndex positionIndex, Vector3 positionOffsetRate)
    {
        if (positionIndex != IKIndex.Max)
        {
            _positionOffsetRate[index] = positionOffsetRate;
            _positionOffsetTargetEffector[index] = positionIndex;
        }
    }

    public Transform FindChild(string name, Transform trans, bool isMessage = true)
    {
        return GameObjectUtility.FindChildTransform(name, trans);
    }

    public void EnableFootShadow(bool enable)
    {
        if (_shadowR != null)
        {
            fakeShadow[] shadowR = _shadowR;
            foreach (fakeShadow obj in shadowR)
            {
                obj.Visible = enable;
                obj.enabled = enable;
            }
        }
        if (_shadowL != null)
        {
            fakeShadow[] shadowR = _shadowL;
            foreach (fakeShadow obj2 in shadowR)
            {
                obj2.Visible = enable;
                obj2.enabled = enable;
            }
        }
    }

    public void ActiveFollowSpotLight(bool active)
    {
        if (_followSpotLightController != null)
        {
            FollowSpotLightController[] array = _followSpotLightController;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].active = active;
            }
        }
    }

    public void BindCharaShadow(List<CharaEffectInfo> listShadowEffectInfo)
    {
        if (listShadowEffectInfo == null)
        {
            return;
        }
        _shadowL = new fakeShadow[listShadowEffectInfo.Count];
        _shadowR = new fakeShadow[listShadowEffectInfo.Count];
        for (int i = 0; i < listShadowEffectInfo.Count; i++)
        {
            GameObject gameObject = listShadowEffectInfo[i].CreateInstance(base.transform);
            if (gameObject != null)
            {
                _shadowL[i] = gameObject.GetComponent<fakeShadow>();
                if (_shadowL[i] != null)
                {
                    _shadowL[i].locationTransform = liveRootTransform;
                }
            }
            gameObject = listShadowEffectInfo[i].CreateInstance(base.transform);
            if (gameObject != null)
            {
                _shadowR[i] = gameObject.GetComponent<fakeShadow>();
                if (_shadowR[i] != null)
                {
                    _shadowR[i].locationTransform = liveRootTransform;
                }
            }
        }
        if (_bodyParts == null)
        {
            return;
        }
        Transform transform = _bodyParts.GetTransform(BodyParts.eTransform.Ankle_R);
        if (transform != null)
        {
            _r_leg = transform.gameObject;
            if (_shadowL != null)
            {
                for (int j = 0; j < _shadowL.Length; j++)
                {
                    _shadowL[j].rootObject = transform.gameObject;
                    _shadowL[j].Initialize();
                }
            }
        }
        Transform transform2 = _bodyParts.GetTransform(BodyParts.eTransform.Ankle_L);
        if (!(transform2 != null))
        {
            return;
        }
        _l_leg = transform2.gameObject;
        if (_shadowR != null)
        {
            for (int k = 0; k < _shadowR.Length; k++)
            {
                _shadowR[k].rootObject = transform2.gameObject;
                _shadowR[k].Initialize();
            }
        }
    }

    public void BindCharaEffect(ref string strSpolightParentName)
    {
        Director instance = Director.instance;
        List<CharaEffectInfo> listShadowEffectInfo = instance.listShadowEffectInfo;
        BindCharaShadow(listShadowEffectInfo);
        List<CharaEffectInfo> listSpotLightEffectInfo = instance.listSpotLightEffectInfo;
        if (listSpotLightEffectInfo == null)
        {
            return;
        }
        GameObject gameObject = null;
        _followSpotLightController = new FollowSpotLightController[listSpotLightEffectInfo.Count];
        for (int i = 0; i < listSpotLightEffectInfo.Count; i++)
        {
            gameObject = listSpotLightEffectInfo[i].CreateInstance(base.transform);
            if (_bodyParts == null)
            {
                continue;
            }
            Transform transform = FindChild(strSpolightParentName, _bodyParts.transform);
            if (transform == null)
            {
                transform = FindChild(LiveTimelineData.DEFAULT_SPOTLIGHT_PARENT_NAME, _bodyParts.transform);
            }
            if (transform != null)
            {
                _followSpotLightController[i] = gameObject.GetComponent<FollowSpotLightController>();
                if (_followSpotLightController[i] != null)
                {
                    _followSpotLightController[i].rootObject = transform.gameObject;
                    _followSpotLightController[i].locationTransform = liveRootTransform;
                }
            }
        }
    }

    private void SetupRealtimeShadow()
    {
        GameObject gameObject = new GameObject("Shadow Camera");
        gameObject.transform.Rotate(new Vector3(90f, 0f, 0f));
        gameObject.transform.parent = base.transform;
        gameObject.transform.localPosition = Vector3.zero;
        Camera camera = gameObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Color;
        camera.fieldOfView = 60f;
        camera.backgroundColor = Color.black;
        camera.depth = -100f;
        camera.cullingMask = 1024;
        CastShadowEffect castShadowEffect = gameObject.AddComponent<CastShadowEffect>();
        GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        gameObject2.name = "Shadow Receiver";
        gameObject2.transform.localScale = Vector3.one * 0.3f;
        gameObject2.transform.parent = base.transform;
        gameObject2.transform.localPosition = Vector3.zero;
        Collider component = gameObject2.GetComponent<Collider>();
        if (component != null)
        {
            UnityEngine.Object.Destroy(component);
            component = null;
        }
        MeshRenderer component2 = gameObject2.GetComponent<MeshRenderer>();
        component2.shadowCastingMode = ShadowCastingMode.Off;
        component2.receiveShadows = false;
        component2.reflectionProbeUsage = ReflectionProbeUsage.Off;
        //Shader shadowShader = Shader.Find("Unlit/CastShadow");
        Shader shadowShader = ResourcesManager.instance.GetShader("CastShadow");
        //Shader shadowReciveShader = Shader.Find("Unlit/ReceiveShadow");
        Shader shadowReciveShader = ResourcesManager.instance.GetShader("ReceiveShadow");
        castShadowEffect.Initialize(shadowShader, shadowReciveShader, this, gameObject2.GetComponent<MeshRenderer>(), 10);
        BlurOptimized blurOptimized = gameObject.AddComponent<BlurOptimized>();
        blurOptimized.downsample = 2;
        blurOptimized.blurSize = 2f;
        blurOptimized.blurIterations = 1;
        blurOptimized.blurShader = Shader.Find("Hidden/FastBlur"); //FastBlur‚ÍUnity‚É‚à‚Æ‚à‚Æ‘¶Ý‚µ‚Ä‚¢‚é‚½‚ßA‚»‚Ì‚Ü‚ÜFind‚Å‚«‚é
        //blurOptimized.blurShader = ResourcesManager.instance.GetShader("FastBlur");
        _softShadowCamera = gameObject;
        _softShadowReceiver = gameObject2;
        _softShadowCameraTrans = _softShadowCamera.transform;
        _softShadowReceiverTrans = _softShadowReceiver.transform;
        _enableRealtimeShadow = false;
        _softShadowCamera.SetActive(value: false);
        _softShadowReceiver.SetActive(value: false);
    }

    public void BindResource(CharacterData characterData, int index, bool bindForHotSwap, bool setupShadow)
    {
        if (characterData != null)
        {
            _liveCharaHeightLevel = characterData.heightId;
            _liveCharaHeightValue = characterData.heightWithoutHeel;
            SetEyeTarget(_eyeTarget);
            _eyeTargetTrans = _eyeTarget.transform;
            if (!bindForHotSwap)
            {
                _eyeTrackDefaultPos = _eyeTarget.transform.localPosition;
            }
            if (_bodyParts != null)
            {
                _leftHandWristTransform = _bodyParts.GetTransform(BodyParts.eTransform.Wrist_L);
                _rightHandWristTransform = _bodyParts.GetTransform(BodyParts.eTransform.Wrist_R);
                _leftHandAttachTransform = _bodyParts.GetTransform(BodyParts.eTransform.Hand_Attach_L);
                _rightHandAttachTransform = _bodyParts.GetTransform(BodyParts.eTransform.Hand_Attach_R);
                _waistTransform = _bodyParts.GetTransform(BodyParts.eTransform.Hip);
                _chestTransform = _bodyParts.GetTransform(BodyParts.eTransform.Chest);
                _initialHeadHeight = liveCharaHeadPosition.y;
                _initialWaistHeight = liveCharaWaistPosition.y;
                _initialChestHeight = liveCharaChestPosition.y;
            }
            if (index > 1)
            {
                _motionNoiseTime = UnityEngine.Random.Range(0, 2);
                _motionNoiseRate = 0.00075f;
            }
            if (setupShadow && !bindForHotSwap)
            {
                SetupRealtimeShadow();
            }
            _bodyPosition = _bodyParts.GetTransform(BodyParts.eTransform.Position);
        }
    }

    public virtual void BindResource(int index, bool bindForHotSwap = false)
    {
        _index = Mathf.Clamp(index, 1, 15);
        CharacterData characterData = Director.instance.GetCharacterData(_index);
        BindResource(characterData, _index, bindForHotSwap, setupShadow: true);
    }

    private IEnumerator Start()
    {
        while (!base.isSetting)
        {
            yield return 0;
        }
        //_isRich = LiveUtils.IsRich();
        _isRich = true;
        _motionNoiseTime = UnityEngine.Random.Range(0, 2);
    }

    protected virtual void UpdateMotion()
    {
        if (!base.isSetting)
        {
            return;
        }
        Animation animation = _bodyParts.animation;
        if (!Director.instance.isTimelineControlled)
        {
            float num = (0f - Mathf.Cos(_motionNoiseTime) + 1f) * _motionNoiseRate;
            if (animation != null && _bodyParts.animationState != null)
            {
                float num2 = Director.instance.musicScoreTimeNormalized(_bodyParts.animationState.length);
                num2 += num;
                _bodyParts.animationState.normalizedTime = num2;
            }
        }
        else if (!liveMSQControlled)
        {
            if (animation != null && liveMSQCurrentAnimState != null)
            {
                _motionNoiseTime += Time.smoothDeltaTime * 0.75f;
                float num3 = (Mathf.Cos(_motionNoiseTime) - 1f) * (1f / liveMSQCurrentAnimState.length);
                float num4 = Mathf.Clamp01((Director.instance.musicScoreTime - liveMSQCurrentAnimStartTime) / liveMSQCurrentAnimState.length);
                num4 += num3;
                liveMSQCurrentAnimState.normalizedTime = num4;
            }
            else
            {
                _motionNoiseTime = 0f;
            }
        }
        else
        {
            _motionNoiseTime = 0f;
        }
        if (_isRich && null != liveRootTransform && null != _bodyRoot)
        {
            Vector3 position = _bodyRoot.position;
            if (null != _softShadowCameraTrans)
            {
                position.y = liveRootTransform.position.y + 4f;
                _softShadowCameraTrans.position = position;
            }
            if (null != _softShadowReceiverTrans)
            {
                position.y = liveRootTransform.position.y + 0.01f;
                _softShadowReceiverTrans.position = position;
            }
        }
    }

    public void SetUpOffset(float offset)
    {
        _eyeTargetStageUpOffset = new Vector3(0f, offset, 0f);
    }

    public void SetDownOffset(float offset)
    {
        _eyeTargetStageDownOffset = new Vector3(0f, offset * -1f, 0f);
    }

    private FacialEyeTrackTarget AvertOnesEyes(Transform tmMainCam, Vector3 vecCamDir)
    {
        FacialEyeTrackTarget result = _curAvertOnesEyeTarget;
        float num = _faceNodeHead.position.x - tmMainCam.position.x;
        float distEpsilon = Behavior.AvertOnesEye.DistEpsilon;
        Vector3 normalized = _faceNodeHead.TransformDirection(Vector3.forward).normalized;
        normalized.y = 0f;
        normalized.Normalize();
        vecCamDir.y = 0f;
        vecCamDir.Normalize();
        float num2 = Mathf.Acos(Vector3.Dot(vecCamDir, normalized)) * 57.29578f;
        float degEpsilon = Behavior.AvertOnesEye.DegEpsilon;
        if (num2 >= degEpsilon)
        {
            normalized = _faceNodeHead.TransformDirection(Vector3.forward).normalized;
            if (Vector3.Cross(normalized, vecCamDir).y < 0f)
            {
                if (num < distEpsilon)
                {
                    if (_curAvertOnesEyeTarget != FacialEyeTrackTarget.StageLeftSide)
                    {
                        result = FacialEyeTrackTarget.StageLeftSide;
                    }
                }
                else
                {
                    result = FacialEyeTrackTarget.StageLeftSide;
                }
            }
            else if (num > 0f - distEpsilon)
            {
                if (_curAvertOnesEyeTarget != FacialEyeTrackTarget.StageRightSide)
                {
                    result = FacialEyeTrackTarget.StageRightSide;
                }
            }
            else
            {
                result = FacialEyeTrackTarget.StageRightSide;
            }
        }
        else if (_curAvertOnesEyeTarget != FacialEyeTrackTarget.StageLeftSide && _curAvertOnesEyeTarget != FacialEyeTrackTarget.StageRightSide)
        {
            result = ((!(num < distEpsilon) || !(num > 0f - distEpsilon)) ? FacialEyeTrackTarget.StageLeftSide : FacialEyeTrackTarget.StageRightSide);
        }
        return result;
    }

    public void ResetEyeTrackAvertParameter()
    {
        eyeTrackAvert = true;
        eyeTrackAvertFacial = true;
        eyeTrackForceAvert = false;
        eyeTrackForceAvertFacial = false;
    }

    private void UpdateGimmick()
    {
        LookDownOn();
        GravityControl();
        if (_gimmickController != null)
        {
            if (Director.instance != null)
            {
                _gimmickController.SetPause(Director.instance.IsPauseLive());
            }
            _gimmickController.SetCurrentTime(Time.time);
        }
    }

    private void LookDownOn()
    {
        if (base.index >= 1 && !(_bodyNodeHead == null) && !(_bodyNodeNeck == null) && Behavior != null && Behavior.LookDownOn != null)
        {
            float headAngle = Behavior.LookDownOn.HeadAngle;
            float neckAngle = Behavior.LookDownOn.NeckAngle;
            float forwardUnit = Behavior.LookDownOn.ForwardUnit;
            float backwardUnit = Behavior.LookDownOn.BackwardUnit;
            Vector3 lhs = base.bodyNodeHead.TransformDirection(Vector3.up);
            lhs.x = 0f;
            lhs = lhs.normalized;
            float num = Mathf.Acos(Vector3.Dot(lhs, Vector3.up)) * 57.29578f;
            float num2 = ((base.bodyNodeHead.rotation.x < 0f) ? backwardUnit : forwardUnit);
            float num3 = Mathf.Clamp(1f - num2 * num, 0f, 1f);
            headAngle *= num3;
            neckAngle *= num3;
            Vector3 localEulerAngles = Vector3.zero;
            localEulerAngles = _bodyNodeHead.localEulerAngles;
            localEulerAngles.x += headAngle;
            _bodyNodeHead.localEulerAngles = localEulerAngles;
            localEulerAngles = _bodyNodeNeck.localEulerAngles;
            localEulerAngles.x += neckAngle;
            _bodyNodeNeck.localEulerAngles = localEulerAngles;
        }
    }

    private void GravityControl()
    {
        if (Behavior == null || Behavior.GravityControl == null || base.clothController == null || !(_lipSyncController != null))
        {
            return;
        }
        base.clothController.ExcludeCapability(CySpring.eCapability.GravityControl);
        List<Master3DCharaData.GravityControlData.Record> recordList = Behavior.GravityControl.GetRecordList(Master3DCharaData.GravityControlData.eType.Facial);
        if (recordList != null)
        {
            foreach (Master3DCharaData.GravityControlData.Record item in recordList)
            {
                if (item.CheckContains(_lipSyncController.currentFaceFlag))
                {
                    Vector3 direction = item.gravityDirection;
                    base.clothController.SetCapability(CySpring.eCapability.GravityControl, item.groupdId, bSwitch: true, ref direction);
                }
            }
        }
        List<Master3DCharaData.GravityControlData.Record> recordList2 = Behavior.GravityControl.GetRecordList(Master3DCharaData.GravityControlData.eType.Eye);
        if (recordList2 == null)
        {
            return;
        }
        foreach (Master3DCharaData.GravityControlData.Record item2 in recordList2)
        {
            Vector3 direction2 = item2.gravityDirection;
            if (item2.CheckContains(_lipSyncController.rightEyeFlag) || item2.CheckContains(_lipSyncController.leftEyeFlag))
            {
                base.clothController.SetCapability(CySpring.eCapability.GravityControl, item2.groupdId, bSwitch: true, ref direction2);
            }
        }
    }

    public int GetOverridedFacialId(int srcFacialId)
    {
        int result = srcFacialId;
        if (base.data.isDressGradeDown && _behavior != null && _behavior.FacialOverride != null)
        {
            result = _behavior.FacialOverride.Override(srcFacialId);
        }
        return result;
    }

    public void UpdateShadowFactor(float shadowPower, ref Color color)
    {
        if (_shadowL != null && _shadowR != null)
        {
            int num = _shadowL.Length;
            for (int i = 0; i < num; i++)
            {
                _shadowL[i].maxAlpha = color.a;
                _shadowL[i].shadowPower = shadowPower;
                _shadowL[i].color = color;
            }
            num = _shadowR.Length;
            for (int j = 0; j < num; j++)
            {
                _shadowR[j].maxAlpha = color.a;
                _shadowR[j].shadowPower = shadowPower;
                _shadowR[j].color = color;
            }
        }
    }

    private void OnIKEffectUpdate(int index)
    {
        switch (_positionOffsetTargetType)
        {
            case IKFixType.Position:
                {
                    Transform iKTargetNode3 = GetIKTargetNode((IKIndex)index);
                    Vector3 vector4 = _positionOffsetRate[0];
                    Vector3 localScale2 = iKTargetNode3.localScale;
                    Vector3 localPosition = iKTargetNode3.localPosition;
                    float num4 = localScale2.x * 0.01f;
                    float num5 = localScale2.y * 0.01f;
                    float num6 = localScale2.z * 0.01f;
                    float num7 = 1f + vector4.x * 0.4546876f;
                    localPosition.x *= num7;
                    localPosition.z = num6 * num7 + (localPosition.z - num6);
                    localPosition.y = num5 * num7 + (localPosition.y - num5);
                    iKTargetNode3.localPosition = localPosition;
                    localScale2.y = 0f;
                    localScale2.z = 0f;
                    localScale2.x = (0f - num4) * vector4.x;
                    if (index == (int)_positionOffsetTargetEffector[0])
                    {
                        _bodyPosition.localPosition += _bodyPosition.localRotation * localScale2;
                    }
                    break;
                }
            case IKFixType.Root:
                {
                    Transform iKTargetNode2 = GetIKTargetNode(_positionOffsetTargetEffector[index]);
                    Vector3 vector2 = _positionOffsetRate[index];
                    Vector3 vector3 = iKTargetNode2.localScale * (vector2.x * 0.01f);
                    vector3.x = 0f - vector3.x;
                    _bodyRoot.localPosition += vector3;
                    break;
                }
            case IKFixType.IKPositionSelfHeight:
                {
                    Transform iKTargetNode = GetIKTargetNode(_positionOffsetTargetEffector[index]);
                    Vector3 vector = _positionOffsetRate[index];
                    Vector3 localScale = iKTargetNode.localScale;
                    Vector3 position = iKTargetNode.position;
                    float num = (0f - localScale.x) * 0.01f * vector.x;
                    float num2 = localScale.y * 0.01f * vector.y;
                    float num3 = localScale.z * 0.01f * vector.z;
                    position.x += num;
                    position.y = position.y + num2 + num3;
                    iKTargetNode.position = position;
                    break;
                }
        }
    }

    protected override void OnPreUpdateBodyTransform()
    {
        base.OnPreUpdateBodyTransform();
        switch (_positionOffsetTargetType)
        {
            case IKFixType.Position:
                OnIKEffectUpdate(2);
                OnIKEffectUpdate(3);
                _positionOffsetTargetEffector[0] = IKIndex.Max;
                return;
            case IKFixType.None:
                return;
        }
        for (int i = 0; i < 4 && _positionOffsetTargetEffector[i] != IKIndex.Max; i++)
        {
            OnIKEffectUpdate(i);
            _positionOffsetTargetEffector[i] = IKIndex.Max;
        }
    }

    protected override void OnUpdateController()
    {
        if (Director.instance != null)
        {
            _renderController.SetPause(Director.instance.IsPauseLive());
            base.OnUpdateController();
        }
        StageTwoBoneIK[] ik = _ik;
        for (int i = 0; i < ik.Length; i++)
        {
            ik[i].Solve();
        }
    }

    protected void UpdateEyeBehavior(Transform tmMainCam, ref Vector3 vecCameraEye)
    {
        if (_behavior == null || _behavior.AvertOnesEye == null || (_behavior.AvertOnesEye.IsExclude(base.createInfo.charaData.cardId) && !_eyeTrackForceAvertEnable))
        {
            return;
        }
        if (_behavior.AvertOnesEye.IsApplyedFacialFlag(faceController.currentFaceFlag, out var isFacialFlag))
        {
            bool flag = true;
            if ((isFacialFlag && !_eyeTrackAvertFacialEnable) || (!isFacialFlag && !_eyeTrackAvertEnable))
            {
                flag = false;
            }
            if (flag && _eyeTrackAvertFlagArray[(int)_eyeTrackTargetType])
            {
                _eyeTrackTargetType = AvertOnesEyes(tmMainCam, vecCameraEye);
            }
        }
        _curAvertOnesEyeTarget = _eyeTrackTargetType;
    }

    protected void SetEyeTargetPos_Arena()
    {
        _eyeTargetTrans.localPosition = _eyeTrackDefaultPos + base.eyeTargetOffset;
        switch (_eyeTrackTargetVPos)
        {
            case EyeTargetVerticalPos.Up:
                _eyeTargetTrans.localPosition += _eyeTargetStageUpOffset;
                break;
            case EyeTargetVerticalPos.Down:
                _eyeTargetTrans.localPosition += _eyeTargetStageDownOffset;
                break;
        }
    }

    protected virtual void SetEyeTargetPos_Camera(ref Vector3 vecCameraEye)
    {
        _eyeTargetTrans.position = _faceNodeHead.position + base.eyeTargetOffset + vecCameraEye * 30f;
        SetEyeTrackPosition(3f);
    }

    protected void SetEyeTargetPos_Forward(float length, float verticalRatio)
    {
        Transform eyeTrans = _eyeTraceController._eyeObject[0].eyeTrans;
        _eyeTargetTrans.position = _faceNodeHead.position + base.eyeTargetOffset + eyeTrans.forward * length;
        SetEyeTrackPosition(verticalRatio);
    }

    protected void SetEyeTargetPos_Side(Vector3 offset, float verticalRatio)
    {
        _eyeTargetTrans.localPosition = _eyeTrackDefaultPos + base.eyeTargetOffset + offset;
        SetEyeTrackPosition(verticalRatio);
    }

    protected void SetEyeTargetPos_World()
    {
        _eyeTargetTrans.position = base.eyeTargetOffset;
    }

    protected virtual void UpdateEyeTraceTargetPosition()
    {
        if (_eyeTraceController == null || !_eyeTraceController.enabled)
        {
            return;
        }
        Director instance = Director.instance;
        if (!(instance == null) && !(instance.mainCamera == null))
        {
            Transform transform = instance.mainCamera.transform;
            Vector3 vecCameraEye = (transform.position - _faceNodeHead.position).normalized;
            UpdateEyeBehavior(transform, ref vecCameraEye);
            switch (_eyeTrackTargetType)
            {
                default:
                    SetEyeTargetPos_Arena();
                    break;
                case FacialEyeTrackTarget.Camera:
                    SetEyeTargetPos_Camera(ref vecCameraEye);
                    break;
                case FacialEyeTrackTarget.CharaForward:
                    SetEyeTargetPos_Forward(30f, 3f);
                    break;
                case FacialEyeTrackTarget.StageLeftSide:
                    SetEyeTargetPos_Side(kEyeTargetStageLeftOffset, 10f);
                    break;
                case FacialEyeTrackTarget.StageRightSide:
                    SetEyeTargetPos_Side(kEyeTargetStageRightOffset, 10f);
                    break;
                case FacialEyeTrackTarget.CharaCenter_Head:
                case FacialEyeTrackTarget.CharaLeft1_Head:
                case FacialEyeTrackTarget.CharaRight1_Head:
                case FacialEyeTrackTarget.CharaLeft2_Head:
                case FacialEyeTrackTarget.CharaRight2_Head:
                case FacialEyeTrackTarget.CharaLeft3_Head:
                case FacialEyeTrackTarget.CharaRight3_Head:
                case FacialEyeTrackTarget.CharaLeft4_Head:
                case FacialEyeTrackTarget.CharaRight4_Head:
                case FacialEyeTrackTarget.CharaLeft5_Head:
                case FacialEyeTrackTarget.CharaRight5_Head:
                case FacialEyeTrackTarget.CharaLeft6_Head:
                case FacialEyeTrackTarget.CharaRight6_Head:
                case FacialEyeTrackTarget.CharaLeft7_Head:
                case FacialEyeTrackTarget.CharaRight7_Head:
                    {
                        LiveCharaPosition id3 = EyeTrackAvertLivePositionTable[(int)_eyeTrackTargetType];
                        CharacterObject characterObjectFromPositionID3 = instance.getCharacterObjectFromPositionID(id3);
                        _eyeTargetTrans.position = characterObjectFromPositionID3.liveCharaHeadPosition + base.eyeTargetOffset;
                        SetEyeTrackPosition(3f);
                        break;
                    }
                case FacialEyeTrackTarget.CharaCenter_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft1_Finger_Left:
                case FacialEyeTrackTarget.CharaRight1_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft2_Finger_Left:
                case FacialEyeTrackTarget.CharaRight2_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft3_Finger_Left:
                case FacialEyeTrackTarget.CharaRight3_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft4_Finger_Left:
                case FacialEyeTrackTarget.CharaRight4_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft5_Finger_Left:
                case FacialEyeTrackTarget.CharaRight5_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft6_Finger_Left:
                case FacialEyeTrackTarget.CharaRight6_Finger_Left:
                case FacialEyeTrackTarget.CharaLeft7_Finger_Left:
                case FacialEyeTrackTarget.CharaRight7_Finger_Left:
                    {
                        LiveCharaPosition id2 = EyeTrackAvertLivePositionTable[(int)_eyeTrackTargetType];
                        CharacterObject characterObjectFromPositionID2 = instance.getCharacterObjectFromPositionID(id2);
                        _eyeTargetTrans.position = characterObjectFromPositionID2.liveCharaLeftHandAttachPosition + base.eyeTargetOffset;
                        SetEyeTrackPosition(3f);
                        break;
                    }
                case FacialEyeTrackTarget.CharaCenter_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft1_Finger_Right:
                case FacialEyeTrackTarget.CharaRight1_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft2_Finger_Right:
                case FacialEyeTrackTarget.CharaRight2_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft3_Finger_Right:
                case FacialEyeTrackTarget.CharaRight3_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft4_Finger_Right:
                case FacialEyeTrackTarget.CharaRight4_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft5_Finger_Right:
                case FacialEyeTrackTarget.CharaRight5_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft6_Finger_Right:
                case FacialEyeTrackTarget.CharaRight6_Finger_Right:
                case FacialEyeTrackTarget.CharaLeft7_Finger_Right:
                case FacialEyeTrackTarget.CharaRight7_Finger_Right:
                    {
                        LiveCharaPosition id = EyeTrackAvertLivePositionTable[(int)_eyeTrackTargetType];
                        CharacterObject characterObjectFromPositionID = instance.getCharacterObjectFromPositionID(id);
                        _eyeTargetTrans.position = characterObjectFromPositionID.liveCharaRightHandAttachPosition + base.eyeTargetOffset;
                        SetEyeTrackPosition(3f);
                        break;
                    }
                case FacialEyeTrackTarget.World:
                    SetEyeTargetPos_World();
                    break;
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (base.isSetting)
        {
            UpdateEyeTraceTargetPosition();
        }
    }

    protected override void LateUpdate()
    {
        if (_isEnabledAlterUpdateSelf && (Director.instance == null || !Director.instance.isTimelineControlled))
        {
            AlterLateUpdate();
        }
    }

    public override void AlterLateUpdate()
    {
        UpdateMotion();
        UpdateGimmick();
        base.AlterLateUpdate();
    }

    public void SetRimColorMulti(Color color)
    {
        _renderController.mtrlProp.SetColor(SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.RimColorMulti), color);
    }

    public void SetFootLightParameter(ref CharaFootLightUpdateInfo updateInfo, List<Props> lstProps = null)
    {
        float hightMax = updateInfo.hightMax;
        Color lightColor = updateInfo.lightColor;
        bool alphaBlend = updateInfo.alphaBlend;
        bool inverseAlpha = updateInfo.inverseAlpha;
        SharedShaderParam instance = SharedShaderParam.instance;
        Vector4 value = new Vector4(_bodyPosition.position.y, hightMax, 0f, 0f);
        _renderController.mtrlProp.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.HeightLightParam), value);
        _renderController.mtrlProp.SetColor(instance.getPropertyID(SharedShaderParam.ShaderProperty.HeightLightColor), lightColor);
        if (alphaBlend)
        {
            _renderController.SetShaderKeyword(MaterialPack.eShaderKeyword.TRANSPARENCY, !inverseAlpha);
            _renderController.SetShaderKeyword(MaterialPack.eShaderKeyword.INVERSE_TRANSPARENCY, inverseAlpha);
        }
        else
        {
            _renderController.SetShaderKeyword(MaterialPack.eShaderKeyword.TRANSPARENCY, sw: false);
            _renderController.SetShaderKeyword(MaterialPack.eShaderKeyword.INVERSE_TRANSPARENCY, sw: false);
        }
        if (lstProps == null)
        {
            return;
        }
        foreach (Props lstProp in lstProps)
        {
            Material copyMaterial = lstProp.copyMaterial;
            if (!(copyMaterial == null))
            {
                copyMaterial.DisableKeyword("TRANSPARENCY");
                copyMaterial.DisableKeyword("INVERSE_TRANSPARENCY");
                if (alphaBlend)
                {
                    copyMaterial.EnableKeyword(inverseAlpha ? "INVERSE_TRANSPARENCY" : "TRANSPARENCY");
                    copyMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.HeightLightParam), value);
                    copyMaterial.SetColor(instance.getPropertyID(SharedShaderParam.ShaderProperty.HeightLightColor), lightColor);
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (_isEnabledAlterUpdateSelf && (Director.instance == null || !Director.instance.isTimelineControlled))
        {
            AlterFixedUpdate();
        }
    }

    public override void AlterFixedUpdate()
    {
        base.isClothUpdate = ((!(Director.instance == null) && !Director.instance.IsPauseLive()) ? true : false);
        if (base.isClothUpdate)
        {
            base.AlterFixedUpdate();
        }
    }

    protected override bool CheckSetting()
    {
        if (!base.CheckSetting())
        {
            return false;
        }
        if (Director.instance == null)
        {
            return false;
        }
        return true;
    }

    private void SetEyeTrackPosition(float ratio)
    {
        switch (_eyeTrackTargetVPos)
        {
            case EyeTargetVerticalPos.Up:
                _eyeTargetTrans.localPosition += _eyeTargetStageUpOffset * ratio;
                break;
            case EyeTargetVerticalPos.Down:
                _eyeTargetTrans.localPosition += _eyeTargetStageDownOffset * ratio;
                break;
        }
    }

    protected override void _SetupNotReflectLayer()
    {
        base._SetupNotReflectLayer();
        if (null != _softShadowReceiver)
        {
            GameObjectUtility.SetLayer(27, _softShadowReceiver.transform);
        }
    }

    public void CollectCySpringClothParameter()
    {
        ClothController clothController = base.clothController;
        if (clothController == null)
        {
            _overrideClothParameterList = new CySpringRootBone[0];
            _overrideClothParameterListAll = new CySpringRootBone[0];
            _overrideClothParameterAccesory = new CySpringRootBone[0];
            _overrideClothParameterFurisode = new CySpringRootBone[0];
            return;
        }
        List<CySpringRootBone> list = new List<CySpringRootBone>(32);
        List<CySpringRootBone> list2 = new List<CySpringRootBone>(32);
        List<CySpringRootBone> list3 = new List<CySpringRootBone>(32);
        List<CySpringRootBone> list4 = new List<CySpringRootBone>(32);
        List<CySpringRootBone> list5 = new List<CySpringRootBone>(32);
        int clothCount = clothController.clothCount;
        List<ClothController.Cloth> clothList = clothController.clothList;
        CySpringRootBone[] boneList;
        for (int i = 0; i < clothCount; i++)
        {
            boneList = clothList[i].spring.boneList;
            foreach (CySpringRootBone cySpringRootBone in boneList)
            {
                if (cySpringRootBone.BoneName.Contains("Sp_Ch_AccOP"))
                {
                    list3.Add(cySpringRootBone);
                    continue;
                }
                if (cySpringRootBone.BoneName.Contains("Hair_"))
                {
                    list4.Add(cySpringRootBone);
                }
                if (cySpringRootBone.BoneName.Contains("Sp_El_hurisode"))
                {
                    list5.Add(cySpringRootBone);
                    continue;
                }
                if (cySpringRootBone.BoneName.Contains("Sp_Hi_") || cySpringRootBone.BoneName.Contains("Sp_Th_"))
                {
                    list.Add(cySpringRootBone);
                }
                if (cySpringRootBone.BoneName.IndexOf("Sp_", StringComparison.Ordinal) == 0)
                {
                    list2.Add(cySpringRootBone);
                }
            }
        }
        _overrideClothParameterList = list.ToArray();
        _overrideClothParameterListAll = list2.ToArray();
        _overrideClothParameterAccesory = list3.ToArray();
        _overrideClothParameterFurisode = list5.ToArray();
        int num = 1;
        boneList = _overrideClothParameterListAll;
        foreach (CySpringRootBone obj in boneList)
        {
            obj.IsWind = true;
            obj.WindGroupIndex = (float)Math.PI * 2f / num;
            num++;
            if (num >= 10)
            {
                num = 1;
            }
        }
        foreach (CySpringRootBone item in list4)
        {
            item.IsWind = true;
            item.WindGroupIndex = (float)Math.PI * 2f / num;
            num++;
            if (num >= 10)
            {
                num = 1;
            }
        }
    }

    public void OnMotionChange()
    {
        int clothCount = base.clothController.clothCount;
        List<ClothController.Cloth> clothList = base.clothController.clothList;
        for (int i = 0; i < clothCount; i++)
        {
            clothList[i].spring.SetSuppression(enable: true, 1E-05f);
        }
    }

    private void OverrideCySpringAccClothParameter(float accStiffness)
    {
        CySpringRootBone.OverrideParameter overrideParameter = default(CySpringRootBone.OverrideParameter);
        overrideParameter.enableGravity = false;
        overrideParameter.enableStiffness = true;
        overrideParameter.enableDragForce = false;
        overrideParameter.gravity = 0f;
        overrideParameter.stiffness = accStiffness;
        overrideParameter.dragForce = 0f;
        CySpringRootBone[] overrideClothParameterAccesory = _overrideClothParameterAccesory;
        foreach (CySpringRootBone obj in overrideClothParameterAccesory)
        {
            obj.OverrideClothParameter(ref overrideParameter);
            obj.IsSuppression = false;
            obj.SuppressionLength = 1f;
        }
    }

    private void OverrideCyspringFurisodeClothParameter(float furisodeStiffness)
    {
        CySpringRootBone.OverrideParameter overrideParameter = default(CySpringRootBone.OverrideParameter);
        overrideParameter.enableGravity = false;
        overrideParameter.enableStiffness = true;
        overrideParameter.enableDragForce = false;
        overrideParameter.gravity = 0f;
        overrideParameter.stiffness = furisodeStiffness;
        overrideParameter.dragForce = 0f;
        CySpringRootBone[] overrideClothParameterFurisode = _overrideClothParameterFurisode;
        foreach (CySpringRootBone obj in overrideClothParameterFurisode)
        {
            obj.OverrideClothParameter(ref overrideParameter);
            obj.IsSuppression = false;
            obj.SuppressionLength = 1f;
        }
    }

    public void OverrideCySpringClothParameter(OverrideClothParameterType type, bool isAll, bool isAcc, float accStiffness, bool isFurisode, float furisodeStiffness)
    {
        float num = 0f;
        CySpringRootBone.OverrideParameter overrideParameter = default(CySpringRootBone.OverrideParameter);
        overrideParameter.enableGravity = false;
        overrideParameter.enableStiffness = true;
        overrideParameter.enableDragForce = false;
        bool isSuppression = _isSuppression;
        _isSuppression = false;
        CySpringRootBone[] array = _overrideClothParameterList;
        CySpringRootBone[] overrideClothParameterAccesory = _overrideClothParameterAccesory;
        if (isAll)
        {
            array = _overrideClothParameterListAll;
        }
        CySpringRootBone[] array2;
        switch (type)
        {
            default:
                {
                    array2 = array;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        array2[j].ResetClothParameter();
                    }
                    if (isAcc)
                    {
                        OverrideCySpringAccClothParameter(accStiffness);
                    }
                    else if (isFurisode)
                    {
                        OverrideCyspringFurisodeClothParameter(furisodeStiffness);
                    }
                    else if (isAll)
                    {
                        array2 = overrideClothParameterAccesory;
                        for (int j = 0; j < array2.Length; j++)
                        {
                            array2[j].ResetClothParameter();
                        }
                    }
                    return;
                }
            case OverrideClothParameterType.Soft:
                num = 0.05f;
                break;
            case OverrideClothParameterType.Usually:
                num = 0.1f;
                break;
            case OverrideClothParameterType.Hard:
                num = 0.15f;
                break;
            case OverrideClothParameterType.SuperHard:
                num = 2f;
                break;
            case OverrideClothParameterType.Suppresion:
                _isSuppression = true;
                if (!isSuppression)
                {
                    int clothCount = base.clothController.clothCount;
                    List<ClothController.Cloth> clothList = base.clothController.clothList;
                    for (int i = 0; i < clothCount; i++)
                    {
                        clothList[i].spring.SetSuppression(enable: true, 1f);
                    }
                    array2 = array;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        array2[j].ResetClothParameter();
                    }
                }
                if (isAcc)
                {
                    OverrideCySpringAccClothParameter(accStiffness);
                }
                else if (isFurisode)
                {
                    OverrideCyspringFurisodeClothParameter(furisodeStiffness);
                }
                else if (isAll)
                {
                    array2 = overrideClothParameterAccesory;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        array2[j].ResetClothParameter();
                    }
                }
                return;
        }
        if (isSuppression != _isSuppression)
        {
            int clothCount2 = base.clothController.clothCount;
            List<ClothController.Cloth> clothList2 = base.clothController.clothList;
            for (int k = 0; k < clothCount2; k++)
            {
                clothList2[k].spring.SetSuppression(enable: false, 1f);
            }
        }
        overrideParameter.gravity = 0f;
        overrideParameter.stiffness = num;
        overrideParameter.dragForce = 0f;
        array2 = array;
        for (int j = 0; j < array2.Length; j++)
        {
            array2[j].OverrideClothParameter(ref overrideParameter);
        }
        if (isAcc)
        {
            OverrideCySpringAccClothParameter(accStiffness);
        }
        else if (isFurisode)
        {
            OverrideCyspringFurisodeClothParameter(furisodeStiffness);
        }
        else if (isAll)
        {
            OverrideCySpringAccClothParameter(num);
        }
    }

    public StageTwoBoneIK GetIK(IKIndex index)
    {
        return _ik[(int)index];
    }

    public Transform GetIKTargetNode(IKIndex index)
    {
        return _bodyParts.GetTransform(IKEffectorTable[(int)index]);
    }

    public void InitializeIK()
    {
        _positionOffsetRate = new Vector3[4];
        _positionOffsetTargetEffector = new IKIndex[4];
        int num = 0;
        Transform bodyPosition = _bodyPosition;
        BodyParts.eTransform[] iKEffectorTable = IKEffectorTable;
        for (int i = 0; i < iKEffectorTable.Length; i++)
        {
            BodyParts.eTransform idx = iKEffectorTable[i];
            Transform transform = _bodyParts.GetTransform(idx);
            if (transform == null)
            {
                if (_bodyParts.GetTransform(IKPoleVectorTable[num]) == null)
                {
                    num++;
                    continue;
                }
                GameObject obj = new GameObject(idx.ToString());
                obj.transform.SetParent(bodyPosition, worldPositionStays: false);
                transform = obj.transform;
                _bodyParts.transformCollector.SetTransform<BodyParts.eTransform>(transform, (int)idx);
            }
            num++;
        }
        _ik = new StageTwoBoneIK[4];
        for (int j = 0; j < 4; j++)
        {
            _ik[j] = new StageTwoBoneIK();
            _positionOffsetTargetEffector[j] = IKIndex.Max;
        }
        Transform endEffector1 = _bodyParts.GetTransform(BodyParts.eTransform.Ankle_R);
        Transform endEffector2 = _bodyParts.GetTransform(BodyParts.eTransform.Ankle_L);
        Transform endEffector3 = _bodyParts.GetTransform(BodyParts.eTransform.Wrist_L);
        Transform endEffector4 = _bodyParts.GetTransform(BodyParts.eTransform.Wrist_R);
        if (endEffector2 != null) _ik[0].Initialize(endEffector2, _bodyParts.GetTransform(BodyParts.eTransform.Pole_Leg_L), _cachedTransform);
        if (endEffector1 != null) _ik[1].Initialize(endEffector1, _bodyParts.GetTransform(BodyParts.eTransform.Pole_Leg_R), _cachedTransform);
        if (endEffector3 != null) _ik[2].Initialize(endEffector3, _bodyParts.GetTransform(BodyParts.eTransform.Pole_Arm_L), _cachedTransform);
        if (endEffector4 != null) _ik[3].Initialize(endEffector4, _bodyParts.GetTransform(BodyParts.eTransform.Pole_Arm_R), _cachedTransform);
    }

    public void RegisterOutlineCommandBuffer(Camera camera)
    {
        if (_renderController.GetRenderCommand(RenderCommand.eCategory.Outline, out var renderCommand))
        {
            renderCommand.RegisterCommandBuffer(camera);
        }
    }

    public void UnregisterOutlineCommandBuffer(Camera camera)
    {
        if (_renderController.GetRenderCommand(RenderCommand.eCategory.Outline, out var renderCommand))
        {
            renderCommand.UnregisterCommandBuffer(camera);
        }
    }

    public void SetDistanceForCheekLOD(float distance)
    {
        _headParts.SetDistanceForCheekLOD(distance);
    }

    public void EnableCheekLOD(bool enable)
    {
        _headParts.EnableCheekLOD(enable);
    }

    public void SetCySpringForceScale(Vector3 cySpringForceScale)
    {
        base.clothController.clothParameter.forceScale = cySpringForceScale;
    }

    public void SetWind(ClothController.WindCalcMode mode, Vector3 windPower, float loopTime)
    {
        base.clothController.SetWind(mode, windPower, loopTime);
    }

    public virtual void RemoveComponentsForHotSwap()
    {
        if (_eyeTraceController != null)
        {
            EyeTraceObject[] eyeObject = _eyeTraceController._eyeObject;
            for (int i = 0; i < eyeObject.Length; i++)
            {
                eyeObject[i].body = null;
            }
        }
        _eyeTrackAvertEnable = true;
        _eyeTrackAvertFacialEnable = true;
        ResetResource();
        if (_shadowR != null)
        {
            for (int j = 0; j < _shadowR.Length; j++)
            {
                if (!(_shadowR[j] == null))
                {
                    UnityEngine.Object.DestroyImmediate(_shadowR[j].gameObject);
                }
            }
            _shadowR = null;
        }
        if (_shadowL != null)
        {
            for (int k = 0; k < _shadowL.Length; k++)
            {
                if (!(_shadowL[k] == null))
                {
                    UnityEngine.Object.DestroyImmediate(_shadowL[k].gameObject);
                }
            }
            _shadowL = null;
        }
        if (_followSpotLightController == null)
        {
            return;
        }
        for (int l = 0; l < _followSpotLightController.Length; l++)
        {
            if (!(_followSpotLightController[l] == null))
            {
                UnityEngine.Object.DestroyImmediate(_followSpotLightController[l].gameObject);
            }
        }
        _followSpotLightController = null;
    }

    public void AppointSpareCharacter(bool isSpare)
    {
        _isSpareCharacter = isSpare;
        base.liveCharaVisible = !isSpare;
        if (_followSpotLightController != null)
        {
            FollowSpotLightController[] array = _followSpotLightController;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].active = !isSpare;
            }
        }
        if (_renderController != null)
        {
            _renderController.Update();
        }
    }
}
