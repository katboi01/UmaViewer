using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Cutt
{
    public class LiveTimelineControl : MonoBehaviour
    {
        public struct FindTimelineConfig
        {
            public enum KeyType
            {
                KeyDirect,
                CurrentFrame
            }

            public KeyType keyType;

            public ILiveTimelineKeyDataList posKeys;

            public ILiveTimelineKeyDataList lookAtKeys;

            public LiveTimelineKey curKey;

            public LiveTimelineKey nextKey;

            public int extraCameraIndex;
        }

        private class ProjectorWork
        {
            public int lineHash;

            public int startFrame = -1;

            public float progressTime;
        }

        public static class Drawing
        {
            private static Texture2D aaLineTex;

            private static Texture2D lineTex;

            private static Material blitMaterial;

            private static Material blendMaterial;

            private static Rect lineRect;

            public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
            {
                if (!lineTex)
                {
                    Initialize();
                }
                float num = pointB.x - pointA.x;
                float num2 = pointB.y - pointA.y;
                float num3 = Mathf.Sqrt(num * num + num2 * num2);
                if (!(num3 < 0.001f))
                {
                    Texture2D texture;
                    Material mat;
                    if (antiAlias)
                    {
                        width *= 3f;
                        texture = aaLineTex;
                        mat = blendMaterial;
                    }
                    else
                    {
                        texture = lineTex;
                        mat = blitMaterial;
                    }
                    float num4 = width * num2 / num3;
                    float num5 = width * num / num3;
                    Matrix4x4 identity = Matrix4x4.identity;
                    identity.m00 = num;
                    identity.m01 = 0f - num4;
                    identity.m03 = pointA.x + 0.5f * num4;
                    identity.m10 = num2;
                    identity.m11 = num5;
                    identity.m13 = pointA.y - 0.5f * num5;
                    GL.PushMatrix();
                    GL.MultMatrix(identity);
                    Graphics.DrawTexture(lineRect, texture, lineRect, 0, 0, 0, 0, color, mat);
                    GL.PopMatrix();
                }
            }

            public static void DrawBezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments)
            {
                Vector2 pointA = CubeBezier(start, startTangent, end, endTangent, 0f);
                for (int i = 1; i < segments; i++)
                {
                    Vector2 vector = CubeBezier(start, startTangent, end, endTangent, i / (float)segments);
                    DrawLine(pointA, vector, color, width, antiAlias);
                    pointA = vector;
                }
            }

            private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
            {
                float num = 1f - t;
                return num * num * num * s + 3f * num * num * t * st + 3f * num * t * t * et + t * t * t * e;
            }

            static Drawing()
            {
                aaLineTex = null;
                lineTex = null;
                blitMaterial = null;
                blendMaterial = null;
                lineRect = new Rect(0f, 0f, 1f, 1f);
                Initialize();
            }

            private static void Initialize()
            {
                if (lineTex == null)
                {
                    lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
                    lineTex.SetPixel(0, 1, Color.white);
                    lineTex.Apply();
                }
                if (aaLineTex == null)
                {
                    aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, mipChain: false);
                    aaLineTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
                    aaLineTex.SetPixel(0, 1, Color.white);
                    aaLineTex.SetPixel(0, 2, new Color(1f, 1f, 1f, 0f));
                    aaLineTex.Apply();
                }
                blitMaterial = (Material)typeof(GUI).GetMethod("get_blitMaterial", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                blendMaterial = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            }
        }

        private const int BG_COLOR3_COLOR_NUM = 10;

        private const float DefaultCameraAspectRatio = 1.77777779f;

        public LiveTimelineData data;

        private bool isDataLoaded;

        public const int kTargetFps = 60;

        public const float kTargetFpsF = 60f;

        public const float kFrameToSec = 0.0166666675f;

        public Transform cameraPositionLocatorsRoot;

        public Transform cameraLookAtLocatorsRoot;

        public Transform[] characterStandPosLocators = new Transform[liveCharaPositionMax];

        private int _numLaserParts = 5;

        private Vector3[] _arrLaserPosition = new Vector3[5];

        private Vector3[] _arrLaserRotation = new Vector3[5];

        private Vector3[] _arrLaserScale = new Vector3[5];

        private bool _isNowAlterUpdate;

        private float _currentLiveTime;

        private float _oldLiveTime;

        private int _currentFrame;

        private int _oldFrame;

        private float _deltaTime;

        private float _deltaTimeRatio;

        private bool _isEnablePostEffect = true;

        private Vector3 _cameraLayerOffset = Vector3.zero;

        private Vector3 _extraCameraLayerOffset = Vector3.zero;

        private bool _isExtraCameraLayer;

        private const float kCameraLayerOffsetMinCharaHeight = 127f;

        private const float kCameraLayerOffsetMaxCharaHeight = 182f;

        private const float kCameraLayerOffsetBaseDiff = 55f;

        private Dictionary<int, bool>[] _ignoreLaserHashDic;

        private float _facialNoiseLimitTime = 0.5f;

        private float[] _facialNoiseLimitTimeArray = new float[14];

        private int[] _facialNoiseFrameArray = new int[14];

        private TimelinePlayerMode _playMode = TimelinePlayerMode.Default;

        private BgColor3UpdateInfo _bgColor3UpdateInfo;

        public float _baseCameraAspectRatio = DefaultCameraAspectRatio;

        public bool _limitFovForWidth;

        private bool _isUnloadAssetWorkSheet = true;

        private string _animationClipLoadDir;

        private int _activeSheetIndex = -1;

        private bool _isActiveSheetIndexAlt;

        private float _chrMotNoiseBaseBias;

        private float _chrMotNoiseRange = 0.07f;

        private float _chrMotNoiseFrequency = 10f;

        private float[] _charaMotNoiseTimeArray = new float[liveCharaPositionMax];

        private Vector3[] _arrMobCyalume3DLookAtPosition = new Vector3[11];

        private Vector3[] _arrMobCyalume3DPosition = new Vector3[11];

        private const int PartsMotionNum = 4;

        private Vector3[] _heightMotionOffsetRate = new Vector3[4];

        private LiveTimelineKeyCharaIKData.IKPart[] _heightMotionOffsetTargetPart = new LiveTimelineKeyCharaIKData.IKPart[4];

        private CacheCamera[] _cameraArray = new CacheCamera[3];

        private LiveTimelineCamera[] _cameraScriptArray = new LiveTimelineCamera[3];

        private Dictionary<string, Transform> _cameraPositionLocatorDict;

        private Dictionary<string, Transform> _cameraLookAtLocatorDict;

        private ILiveTimelineCharactorLocator[] _liveCharactorLocators = new ILiveTimelineCharactorLocator[liveCharaPositionMax];

        private static int _liveCharaPositionMax = -1;

        public const int liveCharaPositionEditorDefault = 5;

        private Vector3 _liveStageCenterPos = Vector3.zero;

        private LiveTimelineEventPublisher _eventPublisher;

        private LiveTimelineMotionSequence[] _motionSequenceArray = new LiveTimelineMotionSequence[liveCharaPositionMax];

        private static Func<LiveTimelineKeyCameraPositionData, LiveTimelineControl, FindTimelineConfig, Vector3> fnGetCameraPosValue = GetCameraPosValue;

        private static Func<LiveTimelineKeyCameraLookAtData, LiveTimelineControl, Vector3, FindTimelineConfig, Vector3> fnGetCameraLookAtValue = GetCameraLookAtValue;

        private int lastTriggeredEventFrame = -1;

        private ProjectorWork[] _projectorWorks;

        private int[] _formationOffsetStartFrameArray = new int[liveCharaPositionMax];

        private LiveTimelineKeyFormationOffsetData[] _formationOffsetLateUpdateKey = new LiveTimelineKeyFormationOffsetData[liveCharaPositionMax * 2];

        private bool _drawDeltaTimeGraph;

        private const int kDeltaTimeArrayLength = 300;

        private float[] _deltaTimeArrayForGraph = new float[kDeltaTimeArrayLength];

        private int _headIndexOfDeltaTimeArray;

        private float graphXMargin = 200f;

        private float graphYMargin;

        private float graphWidth;

        private float graphHeight = 50f;

        private LiveCharaPositionFlag _enviromentShadowCharacterPositionFlag = (LiveCharaPositionFlag)(-1);

        private bool _enviromentSoftShadow;

        private int _colorCorrectionCameraIndex = -1;

        private SweatLocatorUpdateInfo _sweatLocatorUpdateInfo = new SweatLocatorUpdateInfo(5);

        private List<int> _sweatLocatorRandomIndex = new List<int>();

        private CacheCamera _crossFadeCameraCache;

        private CacheCamera[] _arrMonitorCameraCache;

        private float[] _arrMonitorCameraRoll;

        private static Func<LiveTimelineKeyCameraPositionData, LiveTimelineControl, FindTimelineConfig, Vector3> fnGetMonitorCameraPositionValue = GetMonitorCameraPositionValue;

        private static Func<LiveTimelineKeyCameraLookAtData, LiveTimelineControl, Vector3, FindTimelineConfig, Vector3> fnGetMonitorCameraLookAtValue = GetMonitorCameraLookAtValue;

        private CacheCamera[] _multiCameraCache;

        private MultiCamera[] _multiCamera;

        private MultiCameraManager _multiCameraManager;

        private bool _isMultiCameraEnable;

        private static Func<LiveTimelineKeyCameraPositionData, LiveTimelineControl, FindTimelineConfig, Vector3> fnGetMultiCameraPositionValueFunc = GetMultiCameraPositionValue;

        private static Func<LiveTimelineKeyCameraLookAtData, LiveTimelineControl, Vector3, FindTimelineConfig, Vector3> fnGetMultiCameraLookAtValueFunc = GetMultiCameraLookAtValue;

        public Vector3 cameraLayerOffset => _cameraLayerOffset;

        public float facialNoiseLimitTime
        {
            get
            {
                return _facialNoiseLimitTime;
            }
            set
            {
                float num = _facialNoiseLimitTime;
                _facialNoiseLimitTime = value;
                if (_facialNoiseLimitTime != num)
                {
                    InitFacialNoise();
                }
            }
        }

        public TimelinePlayerMode playMode
        {
            set
            {
                _playMode = value;
            }
        }

        private float kChrMotNoiseCurveRate => (float)Math.PI * 2f / _chrMotNoiseFrequency;

        private static bool availableFindKeyCache => true;

        public float currentLiveTime
        {
            get
            {
                return _currentLiveTime;
            }
            private set
            {
                _currentLiveTime = value;
            }
        }

        public bool isEnablePostEffect
        {
            get
            {
                return _isEnablePostEffect;
            }
            set
            {
                _isEnablePostEffect = value;
            }
        }

        public CacheCamera[] cameraArray
        {
            get
            {
                return _cameraArray;
            }
            private set
            {
                _cameraArray = value;
            }
        }

        private LiveTimelineCamera[] cameraScriptArray => _cameraScriptArray;

        private Dictionary<string, Transform> cameraPositionLocatorDict
        {
            get
            {
                if (_cameraPositionLocatorDict == null)
                {
                    _cameraPositionLocatorDict = new Dictionary<string, Transform>();
                    if (cameraPositionLocatorsRoot != null)
                    {
                        Transform[] componentsInChildren = cameraPositionLocatorsRoot.GetComponentsInChildren<Transform>();
                        foreach (Transform transform in componentsInChildren)
                        {
                            _cameraPositionLocatorDict[transform.name] = transform;
                        }
                    }
                }
                return _cameraPositionLocatorDict;
            }
        }

        private Dictionary<string, Transform> cameraLookAtLocatorDict
        {
            get
            {
                if (_cameraLookAtLocatorDict == null)
                {
                    _cameraLookAtLocatorDict = new Dictionary<string, Transform>();
                    if (cameraLookAtLocatorsRoot != null)
                    {
                        Transform[] componentsInChildren = cameraLookAtLocatorsRoot.GetComponentsInChildren<Transform>();
                        foreach (Transform transform in componentsInChildren)
                        {
                            _cameraLookAtLocatorDict[transform.name] = transform;
                        }
                    }
                }
                return _cameraLookAtLocatorDict;
            }
        }

        public ILiveTimelineCharactorLocator[] liveCharactorLocators => _liveCharactorLocators;

        public static int liveCharaPositionMax
        {
            get
            {
                if (_liveCharaPositionMax < 0)
                {
                    _liveCharaPositionMax = Enum.GetValues(typeof(LiveCharaPosition)).Length;
                }
                return _liveCharaPositionMax;
            }
        }

        public Vector3 liveStageCenterPos
        {
            get
            {
                return _liveStageCenterPos;
            }
            set
            {
                _liveStageCenterPos = value;
            }
        }

        public LiveTimelineEventPublisher eventPublisher
        {
            get
            {
                if (_eventPublisher == null)
                {
                    _eventPublisher = new LiveTimelineEventPublisher();
                }
                return _eventPublisher;
            }
        }
        public event Action OnStartUpdate;

        public event Action OnPreUpdateAllTimeline;

        public event Action OnPostUpdateAllTimeline;

        public event CameraPosUpdateInfoDelegate OnUpdateCameraPos;

        public event BgColor1UpdateInfoDelegate OnUpdateBgColor1;

        public event BgColor2UpdateInfoDelegate OnUpdateBgColor2;

        public event BgColor3UpdateInfoDelegate OnUpdateBgColor3;

        public event VolumeLightUpdateInfoDelegate OnUpdateVolumeLight;

        public event HdrBloomUpdateInfoDelegate OnUpdateHdrBloom;

        public event MonitorControlUpdateInfoDelegate OnUpdateMonitorControl;

        public event Action<int> OnUpdateCameraSwitcher;

        public event Action<LiveTimelineKeyRipSyncData, float> OnUpdateLipSync;

        public event PostEffectUpdateInfoDelegate OnUpdatePostEffect;

        public event ScreenOverlayUpdateInfoDelegate OnUpdatePostFilm;

        public event ScreenOverlayUpdateInfoDelegate OnUpdatePostFilm2;

        public event ProjectorUpdateInfoDelegate OnUpdateProjector;

        public event AnimationUpdateInfoDelegate OnUpdateAnimation;

        public event TextureAnimationUpdateInfoDelegate OnUpdateTextureAnimation;

        public event TransformUpdateInfoDelegate OnUpdateTransform;

        public event RendererUpdateInfoDelegate OnUpdateRenderer;

        public event ObjectUpdateInfoDelegate OnUpdateObject;

        public event GazingObjectUpdateInfoDelegate OnUpdateGazingObject;

        public event ScreenFadeUpdateInfoDelegate OnUpdateScreenFade;

        public event PropsUpdateInfoDelegate OnUpdateProps;

        public event PropsAttachUpdateInfoDelegate OnUpdatePropsAttach;

        public event Action<FacialDataUpdateInfo, float, LiveCharaPosition> OnUpdateFacial;

        public event ParticleUpdateInfoDelegate OnUpdateParticle;

        public event ParticleGroupUpdateInfoDelegate OnUpdateParticleGroup;

        public event LaserUpdateInfoDelegate OnUpdateLaser;

        public event EffectUpdateInfoDelegate OnUpdateEffect;

        public event FormationOffsetUpdateInfoDelegate OnUpdateFormationOffset;

        public event EnvironmentCharacterShadowDelegate OnEnvironmentCharacterShadow;

        public event EnvironmentMirrorDelegate OnEnvironmentMirror;

        public event EnvironmentGlobalLightDelegate OnEnvironmentGlobalLight;

        public event GlobalFogDelegate OnGlobalFog;

        public event TiltShiftDelegate OnUpdateTiltShift;

        public event LightShuftDelegate OnLightShuft;

        public event ColorCorrectionDelegate OnColorCorrection;

        public event SweatLocatorUpdateInfoDelegate OnUpdateSweatLocator;

        public event LensFlareUpdateInfoDelegate OnUpdateLensFlare;

        public event A2UConfigUpdateInfoDelegate OnUpdateA2UConfig;

        public event A2UUpdateInfoDelegate OnUpdateA2U;

        public event MonitorCameraUpdateDelegate OnUpdateMonitorCamera;

        public event GlassUpdateInfoDelegate OnUpdateGlass;

        public event ShaderControlUpdateInfoDelegate OnUpdateShaderControl;

        public event CySpringUpdateInfoDelegate OnUpdateCySpring;

        public event CharaIKUpdateInfoDelegate OnUpdateIK;

        public event CharaFootLightUpdateInfoDelegate OnUpdateCharaFootLight;

        public event StageGazeControlUpdateInfoDelegate OnUpdateStageGazeControl;

        public event MobCyalumeUpdateInfoDelegate OnUpdateMobCyalume;

        public event MobCyalume3DUpdateInfoDelegate OnUpdateMobCyalume3D;

        public event CrossFadeCameraUpdateInfoDelegate OnUpdateCrossFadeCamera;

        public event MultiCameraUpdateInfoDelegate OnUpdateMultiCamera;

        public event CharaHeightMotionUpdateInfoDelegate OnCharaHeightMotionUpdateInfo;

        public event CharaWindUpdateInfoDelegate OnCharaWindUpdate;

        public event DressChangeUpdateInfoDelegate OnUpdateDressChange;

        public Vector3 GetExtraCameraLayerOffset(Quaternion worldRotation)
        {
            if (!_isExtraCameraLayer)
            {
                return _cameraLayerOffset;
            }
            return worldRotation * _extraCameraLayerOffset;
        }

        public void SetUnloadAssetWorkSheet(bool b)
        {
            _isUnloadAssetWorkSheet = b;
        }

        public void SetActiveSheetIndex(int index, bool isAlt = false)
        {
            _activeSheetIndex = index;
            _activeSheetIndex = index;
            _isActiveSheetIndexAlt = isAlt;
        }
        public CacheCamera GetCamera(int index)
        {
            if (index < 0 || index >= _cameraArray.Length)
            {
                return null;
            }
            return _cameraArray[index];
        }

        public LiveTimelineCamera GetCameraScript(int index)
        {
            if (index < 0 || index >= _cameraScriptArray.Length)
            {
                return null;
            }
            return _cameraScriptArray[index];
        }

        public Vector3 GetCameraPos(int index)
        {
            return GetCamera(index)?.cacheTransform.position ?? Vector3.zero;
        }

        public Transform FindPositionLocator(string name)
        {
            if (cameraPositionLocatorsRoot == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (!cameraPositionLocatorDict.TryGetValue(name, out var value))
            {
                return null;
            }
            return value;
        }

        public Transform FindLookAtLocator(string name)
        {
            if (cameraLookAtLocatorsRoot == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (!cameraLookAtLocatorDict.TryGetValue(name, out var value))
            {
                return null;
            }
            return value;
        }

        public bool SetCharactorParentLocator(int id, Transform parentTransform)
        {
            if (_liveCharactorLocators.Length <= id)
            {
                return false;
            }
            if (_liveCharactorLocators[id] == null)
            {
                return false;
            }
            _liveCharactorLocators[id].liveParentTransform = parentTransform;
            return true;
        }

        public void SetCharactorLocator(LiveCharaPosition position, ILiveTimelineCharactorLocator locator)
        {
            if (locator == null)
            {
                liveCharactorLocators[(int)position] = null;
            }
            else
            {
                locator.liveCharaStandingPosition = position;
                liveCharactorLocators[(int)position] = locator;
            }
            InitCharaMotionSequence(position);
            if (locator != null)
            {
                if ((int)position < characterStandPosLocators.Length && characterStandPosLocators[(int)position] != null && locator.liveRootTransform != null)
                {
                    locator.liveRootTransform.position = characterStandPosLocators[(int)position].position;
                    locator.liveCharaInitialPosition = characterStandPosLocators[(int)position].position;
                }
                float num = locator.liveCharaHeightValue - kCameraLayerOffsetMinCharaHeight;
                locator.liveCharaHeightRatio = num / kCameraLayerOffsetBaseDiff;
                if (Mathf.Abs(locator.liveCharaHeightRatio) < 0.001f)
                {
                    locator.liveCharaHeightRatio = 0f;
                }
            }
        }

        public void ChangeCharacterLocator(LiveCharaPosition position, ILiveTimelineCharactorLocator locator)
        {
            if (locator == null)
            {
                liveCharactorLocators[(int)position] = null;
            }
            else
            {
                locator.liveCharaStandingPosition = position;
                liveCharactorLocators[(int)position] = locator;
            }
            ChangeMotionTarget(position);
        }

        public void SetTimelineCamera(Camera cam, int index)
        {
            if (index < _cameraArray.Length)
            {
                if (_cameraArray[index] == null)
                {
                    _cameraArray[index] = new CacheCamera(cam);
                }
                else
                {
                    _cameraArray[index].Set(cam);
                }
                LiveTimelineCamera liveTimelineCamera = cam.gameObject.GetComponent<LiveTimelineCamera>();
                if (liveTimelineCamera == null)
                {
                    liveTimelineCamera = cam.gameObject.AddComponent<LiveTimelineCamera>();
                }
                _cameraScriptArray[index] = liveTimelineCamera;
                if (liveTimelineCamera != null)
                {
                    liveTimelineCamera.AlterAwake();
                }
            }
        }

        public void InitCharaMotionSequence(LiveCharaPosition position)
        {
            if (data == null)
            {
                return;
            }
            if (liveCharactorLocators[(int)position] == null)
            {
                return;
            }
            List<LiveTimelineCharaMotSeqData> charaMotSeqList = data.GetWorkSheetList()[0].charaMotSeqList;
            List<LiveTimelineCharaOverrideMotSeqData> charaMotOverwriteList = data.GetWorkSheetList()[0].charaMotOverwriteList;
            LiveTimelineCharaMotSeqData liveTimelineCharaMotSeqData = null;
            LiveTimelineCharaOverrideMotSeqData liveTimelineCharaOverrideMotSeqData = null;
            List<LiveTimelineCharaHeightMotSeqData> charaHeightMotList = data.GetWorkSheetList()[0].charaHeightMotList;
            LiveTimelineCharaHeightMotSeqData liveTimelineCharaHeightMotSeqData = null;
            if ((int)position < data.characterSettings.motionSequenceIndices.Length)
            {
                int num = data.characterSettings.motionSequenceIndices[(int)position];
                if (num >= 0 && num < charaMotSeqList.Count)
                {
                    liveTimelineCharaMotSeqData = charaMotSeqList[num];
                }
            }
            if ((int)position < data.characterSettings.motionOverwriteIndices.Length)
            {
                int num2 = data.characterSettings.motionOverwriteIndices[(int)position];
                if (num2 >= 0 && num2 < charaMotOverwriteList.Count)
                {
                    liveTimelineCharaOverrideMotSeqData = charaMotOverwriteList[num2];
                }
            }
            foreach (LiveTimelineCharaHeightMotSeqData item in charaHeightMotList)
            {
                if (item.charaPosition == position)
                {
                    liveTimelineCharaHeightMotSeqData = item;
                    break;
                }
            }
            if (liveTimelineCharaMotSeqData != null && liveCharactorLocators[(int)position] is ILiveTimelineMSQTarget)
            {
                if (_motionSequenceArray[(int)position] == null)
                {
                    _motionSequenceArray[(int)position] = new LiveTimelineMotionSequence();
                }
                ILiveTimelineMSQTarget liveTimelineMSQTarget = liveCharactorLocators[(int)position] as ILiveTimelineMSQTarget;
                LiveTimelineMotionSequence.Context context = default(LiveTimelineMotionSequence.Context);
                context.motionKeys = liveTimelineCharaMotSeqData.keys;
                context.overwriteMotionKeys = liveTimelineCharaOverrideMotSeqData?.keys;
                context.heightMotionKeys = liveTimelineCharaHeightMotSeqData?.keys;
                context.timelineControl = this;
                if (liveTimelineMSQTarget.spareTarget != null)
                {
                    ILiveTimelineMSQTarget[] spareTarget = liveTimelineMSQTarget.spareTarget;
                    for (int i = 0; i < spareTarget.Length; i++)
                    {
                        ILiveTimelineMSQTarget liveTimelineMSQTarget2 = (context.target = spareTarget[i]);
                        _motionSequenceArray[(int)position].Initialize(ref context);
                        _motionSequenceArray[(int)position].AlterUpdate(0f, 60f);
                    }
                }
                context.target = liveTimelineMSQTarget;
                _motionSequenceArray[(int)position].Initialize(ref context);
            }
            else
            {
                _motionSequenceArray[(int)position] = null;
            }
        }

        public void ChangeMotionTarget(LiveCharaPosition position)
        {
            if (liveCharactorLocators[(int)position] != null)
            {
                ILiveTimelineMSQTarget liveTimelineMSQTarget = liveCharactorLocators[(int)position] as ILiveTimelineMSQTarget;
                if (liveTimelineMSQTarget != null && _motionSequenceArray[(int)position] != null)
                {
                    _motionSequenceArray[(int)position].ChangeTarget(liveTimelineMSQTarget);
                }
                else
                {
                    _motionSequenceArray[(int)position] = null;
                }
            }
        }

        private void Awake()
        {
            currentLiveTime = 0f;
            _bgColor3UpdateInfo.colorArray = new Color[BG_COLOR3_COLOR_NUM];
            for (int i = 0; i < _bgColor3UpdateInfo.colorArray.Length; i++)
            {
                _bgColor3UpdateInfo.colorArray[i] = default(Color);
            }
        }

        private void OnDestroy()
        {
            Terminate();
        }

        private void Start()
        {
        }

        public void Initialize(string charaMotionPath = null)
        {
            if (charaMotionPath == null)
            {
                _animationClipLoadDir = "3D/Chara/Motion/Legacy/";
            }
            else
            {
                _animationClipLoadDir = Regex.Replace(charaMotionPath, "[^/]+$", "");
            }
            if (LoadData() == null)
            {
                return;
            }
            eventPublisher.BeginPublish();
            InitFacialNoise();
            InitCharaMotionNoise();
            InitFormationOffsetWork();
            CreateIgnoreLaserHashDic();
            eventPublisher.Subscribe(LiveTimelineEventID.CameraHandShake, delegate (LiveTimelineKeyEventData.EventData eventData)
            {
                for (int i = 0; i < _cameraScriptArray.Length; i++)
                {
                    LiveTimelineCamera liveTimelineCamera = _cameraScriptArray[i];
                    if (!(liveTimelineCamera == null))
                    {
                        CuttEventParam_CameraHandShake parameter = eventData.GetParameter<CuttEventParam_CameraHandShake>();
                        if (parameter != null)
                        {
                            liveTimelineCamera.applyNoise = parameter.isEnable;
                            if (parameter.isOverwrite)
                            {
                                liveTimelineCamera.positionAmount = parameter.positionAmount;
                                liveTimelineCamera.rotationAmount = parameter.rotationAmount;
                                liveTimelineCamera.positionComponents = (Vector3)parameter.positionComponents;
                                liveTimelineCamera.rotationComponents = (Vector3)parameter.rotationComponents;
                            }
                        }
                    }
                }
            });
        }

        private void Terminate()
        {
            eventPublisher.EndPublish();
            UnloadData();
        }

        public LiveTimelineData LoadData()
        {
            if (!isDataLoaded && data != null)
            {
                data.OnLoad(this);
                isDataLoaded = true;
            }
            return data;
        }

        private void UnloadData()
        {
            if (!(data == null))
            {
                if (_isUnloadAssetWorkSheet)
                {
                    data.UnloadSheets();
                    Resources.UnloadAsset(data);
                }
                data = null;
                isDataLoaded = false;
            }
        }

        public AnimationClip LoadAnimationClip(LiveTimelineKeyCharaMotionData key)
        {
            if (key.clip != null && key.clip.name != key.motionName)
            {
                key.clip = null;
            }
            if (key.clip != null)
            {
                //キャラモーションのFPSを指定
                key.clip.frameRate = Application.targetFrameRate;
                return key.clip;
            }
            if (string.IsNullOrEmpty(key.motionName))
            {
                return null;
            }
            string text = GetAnimationClipLoadDir() + key.motionName;
            /*
            if (Application.isEditor && !Application.isPlaying)
            {
                return Resources.Load<AnimationClip>(text);
            }
            */

            //キャラモーションのFPSを指定
            AnimationClip clip = (AnimationClip)ResourcesManager.instance.LoadObject(text);
            clip.frameRate = Application.targetFrameRate;
            return clip;
        }

        public string GetAnimationClipLoadDir()
        {
            return _animationClipLoadDir;
        }

        /// <summary>
        /// 口パクにノイズを載せる
        /// </summary>

        private void InitFacialNoise()
        {
            float[] array = new float[14]
            {
            _facialNoiseLimitTime * -1f,
            _facialNoiseLimitTime * -0.857f,
            _facialNoiseLimitTime * -0.714f,
            _facialNoiseLimitTime * -0.571f,
            _facialNoiseLimitTime * -0.428f,
            _facialNoiseLimitTime * -0.285f,
            _facialNoiseLimitTime * -0.142f,
            _facialNoiseLimitTime * 0.142f,
            _facialNoiseLimitTime * 0.285f,
            _facialNoiseLimitTime * 0.428f,
            _facialNoiseLimitTime * 0.571f,
            _facialNoiseLimitTime * 0.714f,
            _facialNoiseLimitTime * 0.857f,
            _facialNoiseLimitTime * 1f
            };
            int num = array.Length;
            for (int i = 0; i < _facialNoiseLimitTimeArray.Length; i++)
            {
                int num2 = num - i - 1;
                int num3 = ((num2 != 0) ? UnityEngine.Random.Range(0, num2) : 0);
                _facialNoiseLimitTimeArray[i] = array[num3];
                array[num3] = array[num2];
            }
        }

        private float GetFacialNoiseCurrentTime(int index)
        {
            return currentLiveTime + _facialNoiseLimitTimeArray[index];
        }

        /// <summary>
        /// キャラのモーションにノイズを載せる
        /// </summary>

        public void InitCharaMotionNoise()
        {
            float[] array = new float[14]
            {
            0f, 0.142f, 0.285f, 0.428f, 0.571f, 0.714f, 0.857f, 1f, 0.2f, 0.3f,
            0.4f, 0.6f, 0.8f, 1f
            };
            int num = array.Length;
            for (int i = 0; i < liveCharaPositionMax - 1; i++)
            {
                int num2 = num - i - 1;
                int num3 = ((num2 != 0) ? UnityEngine.Random.Range(0, num2) : 0);
                _charaMotNoiseTimeArray[i + 1] = _chrMotNoiseFrequency * array[num3];
                array[num3] = array[num2];
            }
        }

        public void StartUpdate()
        {
            if (this.OnStartUpdate != null)
            {
                this.OnStartUpdate();
            }
            AlterUpdate(0f);
        }

        public void AlterUpdate(float liveTime)
        {
            if (data == null)
            {
                return;
            }
            _isNowAlterUpdate = true;
            if (this.OnPreUpdateAllTimeline != null)
            {
                this.OnPreUpdateAllTimeline();
            }
            _oldLiveTime = currentLiveTime;
            currentLiveTime = liveTime;
            _currentFrame = Mathf.RoundToInt(currentLiveTime * 60f);
            _oldFrame = Mathf.RoundToInt(_oldLiveTime * 60f);
            _deltaTime = currentLiveTime - _oldLiveTime;
            _deltaTimeRatio = _deltaTime / 0.0166666675f;
            for (int i = 0; i < _facialNoiseLimitTimeArray.Length; i++)
            {
                float facialNoiseCurrentTime = GetFacialNoiseCurrentTime(i);
                _facialNoiseFrameArray[i] = Mathf.RoundToInt(facialNoiseCurrentTime * 60f);
            }
            if (_drawDeltaTimeGraph)
            {
                UpdateDeltaTimeGraph();
                EnqueueDeltaTimeForGraph(_deltaTime);
            }
            int count = data.GetWorkSheetList().Count;
            for (int j = 0; j < count; j++)
            {
                LiveTimelineWorkSheet workSheet = data.GetWorkSheet(j);
                if (workSheet.IsEnable())
                {
                    AlterUpdate_Event(workSheet, _currentFrame, _oldFrame);
                    AlterUpdate_ShaderControl(workSheet, _currentFrame);
                    if (j == 0)
                    {
                        AlterUpdate_CharaHeightMotion(workSheet, _currentFrame);
                        AlterUpdate_CharaMotionSequence(workSheet, _currentFrame);
                    }
                    AlterUpdate_LipSync(workSheet, _currentFrame);
                    if ((_activeSheetIndex != -1 && _isActiveSheetIndexAlt && data.switchSheetTargetTimelineSettings.Get(LiveTimelineSwitchSheetTargetTimelineSetting.Timeline.Facial)) ? (_activeSheetIndex == j) : (j == 0))
                    {
                        AlterUpdate_FacialData(workSheet, _currentFrame);
                    }
                    if (j == 0)
                    {
                        AlterUpdate_CharaWind(workSheet, _currentFrame);
                    }
                    AlterUpdate_BgColor1(workSheet, _currentFrame);
                    AlterUpdate_BgColor2(workSheet, _currentFrame);
                    AlterUpdate_BgColor3(workSheet, _currentFrame);
                    AlterUpdate_VolumeLight(workSheet, _currentFrame);
                    AlterUpdate_LightShuft(workSheet, _currentFrame);
                    AlterUpdate_HdrBloom(workSheet, _currentFrame);
                    AlterUpdate_ColorCorrection(workSheet, _currentFrame);
                    AlterUpdate_MonitorControl(workSheet, _currentFrame);
                    AlterUpdate_Projector(workSheet, _currentFrame);
                    AlterUpdate_AnimationControl(workSheet, _currentFrame);
                    AlterUpdate_TextureAnimationControl(workSheet, _currentFrame);
                    AlterUpdate_TransformControl(workSheet, _currentFrame);
                    AlterUpdate_RendererControl(workSheet, _currentFrame);
                    AlterUpdate_ObjectControl(workSheet, _currentFrame);
                    AlterUpdate_GazingObjectControl(workSheet, _currentFrame);
                    AlterUpdate_StageGazeControl(workSheet, _currentFrame);
                    if (_activeSheetIndex == -1 || _activeSheetIndex == j)
                    {
                        AlterUpdate_FormationOffset(workSheet, _currentFrame);
                    }
                    AlterUpdate_PropsControl(workSheet, _currentFrame);
                    if (!_isActiveSheetIndexAlt || _activeSheetIndex == -1 || _activeSheetIndex == j)
                    {
                        AlterUpdate_PropsAttachControl(workSheet, _currentFrame);
                    }
                    AlterUpdate_Particle(workSheet, _currentFrame);
                    AlterUpdate_ParticleGroup(workSheet, _currentFrame);
                    AlterUpdate_Laser(workSheet, _currentFrame, j);
                    AlterUpdate_Effect(workSheet, _currentFrame);
                    AlterUpdate_Glass(workSheet, _currentFrame);
                    AlterUpdate_Environment(workSheet, _currentFrame);
                    AlterUpdate_GlobalLight(workSheet, _currentFrame);
                    AlterUpdate_GlobalFog(workSheet, _currentFrame);
                    AlterUpdate_TiltShift(workSheet, _currentFrame);
                    AlterUpdate_SweatLocator(workSheet, _currentFrame);
                    AlterUpdate_LensFlare(workSheet, _currentFrame);
                    AlterUpdate_A2UConfig(workSheet, _currentFrame);
                    AlterUpdate_A2U(workSheet, _currentFrame);
                    AlterUpdate_IK(workSheet, _currentFrame);
                    AlterUpdate_MobCyalume(workSheet, _currentFrame);
                    AlterUpdate_MobCyalume3D(workSheet, _currentFrame);
                    AlterUpdate_CameraSwitcher(workSheet, _currentFrame);
                    AlterUpdate_DressChange(workSheet, _currentFrame);
                }
            }
            if (this.OnPostUpdateAllTimeline != null)
            {
                this.OnPostUpdateAllTimeline();
            }
            _isNowAlterUpdate = false;
        }

        public void AlterLateUpdate()
        {
            if (data == null)
            {
                return;
            }
            _isMultiCameraEnable = false;
            _isNowAlterUpdate = true;
            int count = data.GetWorkSheetList().Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineWorkSheet workSheet = data.GetWorkSheet(i);
                if (workSheet.IsEnable())
                {
                    Vector3 outLookAt = Vector3.zero;
                    if (_activeSheetIndex == -1 || _activeSheetIndex == i)
                    {
                        AlterLateUpdate_FormationOffset(workSheet, _currentFrame);
                        AlterUpdate_CameraLayer(workSheet, _currentFrame, ref _cameraLayerOffset, ref _extraCameraLayerOffset);
                        AlterUpdate_CharaFootLight(workSheet, _currentFrame);
                        AlterUpdate_CameraPos(workSheet, _currentFrame, i);
                        AlterUpdate_CameraLookAt(workSheet, _currentFrame, ref outLookAt);
                        AlterUpdate_CameraFov(workSheet, _currentFrame);
                        AlterUpdate_CameraRoll(workSheet, _currentFrame);
                    }
                    if (i == 0)
                    {
                        AlterUpdate_CrossFadeCamera(workSheet, _currentFrame);
                        AlterUpdate_MultiCamera(workSheet, _currentFrame);
                    }
                    AlterUpdate_MonitorCamera(workSheet, _currentFrame);
                    if (isEnablePostEffect && (_activeSheetIndex == -1 || _activeSheetIndex == i))
                    {
                        AlterUpdate_PostEffect(workSheet, _currentFrame, outLookAt);
                    }
                    AlterUpdate_PostFilm(workSheet.postFilmKeys, _currentFrame, this.OnUpdatePostFilm);
                    AlterUpdate_PostFilm(workSheet.postFilm2Keys, _currentFrame, this.OnUpdatePostFilm2);
                    AlterUpdate_ScreenFade(workSheet.screenFadeKeys, _currentFrame);
                }
            }
            for (int j = 0; j < _cameraScriptArray.Length; j++)
            {
                if (_cameraScriptArray[j] != null)
                {
                    _cameraScriptArray[j].AlterUpdate(currentLiveTime);
                }
            }
            if (this.OnUpdateMultiCamera != null)
            {
                MultiCameraUpdateInfo updateInfo = default(MultiCameraUpdateInfo);
                updateInfo.isEnable = _isMultiCameraEnable;
                this.OnUpdateMultiCamera(ref updateInfo);
            }
            _isNowAlterUpdate = false;
        }

        private static float LerpWithoutClamp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static Vector2 LerpWithoutClamp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
        }

        private static Vector3 LerpWithoutClamp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        private static Vector4 LerpWithoutClamp(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
        }

        private static Color LerpWithoutClamp(Color a, Color b, float t)
        {
            return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
        }

        public static void FindTimelineKey(out LiveTimelineKey curKey, out LiveTimelineKey nextKey, ILiveTimelineKeyDataList keys, int curFrame)
        {
            FindKeyResult findKeyResult = keys.FindKeyCached(curFrame, availableFindKeyCache);
            curKey = findKeyResult.key;
            if (curKey != null)
            {
                nextKey = keys.At(findKeyResult.index + 1);
            }
            else
            {
                nextKey = null;
            }
        }

        public static void FindTimelineKeyCurrent(out LiveTimelineKey curKey, ILiveTimelineKeyDataList keys, int curFrame)
        {
            LiveTimelineKey nextKey;
            FindTimelineKey(out curKey, out nextKey, keys, curFrame);
        }

        public static void FindTimelineKeyTrigger(out LiveTimelineKey triggeredKey, ILiveTimelineKeyDataList keys, int curFrame, int oldFrame)
        {
            triggeredKey = null;
            LiveTimelineKey liveTimelineKey = null;
            liveTimelineKey = keys.FindKeyCached(curFrame, availableFindKeyCache).key;
            if (liveTimelineKey != null && oldFrame <= liveTimelineKey.frame && liveTimelineKey.frame <= curFrame)
            {
                triggeredKey = liveTimelineKey;
            }
        }

        public static void FindTimelineKeyPrevious(out LiveTimelineKey previousKey, ILiveTimelineKeyDataList keys, int curFrame)
        {
            previousKey = null;
            FindKeyResult findKeyResult = keys.FindKeyCached(curFrame, availableFindKeyCache);
            if (findKeyResult.key != null)
            {
                previousKey = keys.At(findKeyResult.index - 1);
            }
        }

        public static void FindTimelineKeyNext(out LiveTimelineKey nextKey, ILiveTimelineKeyDataList keys, int curFrame)
        {
            nextKey = null;
            LiveTimelineKey curKey;
            FindTimelineKey(out curKey, out nextKey, keys, curFrame);
        }

        private static float LinearInterpolateKeyframes(LiveTimelineKey from, LiveTimelineKey to, float curFrame)
        {
            int num = to.frame - from.frame;
            return Mathf.Clamp01((curFrame - from.frame) / num);
        }

        private static float CurveInterpolateKeyframes(LiveTimelineKey from, LiveTimelineKey to, float curFrame)
        {
            LiveTimelineKeyWithInterpolate liveTimelineKeyWithInterpolate = from as LiveTimelineKeyWithInterpolate;
            LiveTimelineKeyWithInterpolate liveTimelineKeyWithInterpolate2 = to as LiveTimelineKeyWithInterpolate;
            if (liveTimelineKeyWithInterpolate == null)
            {
                return 0f;
            }
            if (liveTimelineKeyWithInterpolate2 == null)
            {
                return 0f;
            }
            int num = to.frame - from.frame;
            float time = Mathf.Clamp01((curFrame - from.frame) / num);
            return liveTimelineKeyWithInterpolate2.curve.Evaluate(time);
        }

        private static float EaseInterpolateKeyframes(LiveTimelineKey from, LiveTimelineKey to, float curFrame)
        {
            LiveTimelineKeyWithInterpolate liveTimelineKeyWithInterpolate = from as LiveTimelineKeyWithInterpolate;
            LiveTimelineKeyWithInterpolate liveTimelineKeyWithInterpolate2 = to as LiveTimelineKeyWithInterpolate;
            if (liveTimelineKeyWithInterpolate == null)
            {
                return 0f;
            }
            if (liveTimelineKeyWithInterpolate2 == null)
            {
                return 0f;
            }
            int num = to.frame - from.frame;
            return LiveTimelineEasing.GetValue(liveTimelineKeyWithInterpolate2.easingType, curFrame - from.frame, 0f, 1f, num);
        }

        public float GetCharacterHeight(LiveCharaPosition position)
        {
            return liveCharactorLocators[(int)position].liveCharaHeightValue;
        }

        public Vector3 GetPositionWithCharacters(LiveCharaPositionFlag posFlags, LiveCameraLookAtCharaParts parts, Vector3 cameraOffset)
        {
            Vector3 retPos = Vector3.zero;
            Vector3 tmpPos = Vector3.zero;
            if (posFlags == 0)
            {
                retPos = liveStageCenterPos;
                tmpPos = liveStageCenterPos;
            }
            else
            {
                int num = 0;
                switch (parts)
                {
                    case LiveCameraLookAtCharaParts.Face:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaHeadPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.Waist:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaWaistPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.LeftHandWrist:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaLeftHandWristPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.LeftHandAttach:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaLeftHandAttachPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.RightHandWrist:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaRightHandWristPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.RightHandAttach:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaRightHandAttachPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.Chest:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaChestPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.Foot:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaFootPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.ConstHeightFace:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaConstHeightHeadPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.ConstHeightWaist:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaConstHeightWaistPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.ConstHeightChest:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaConstHeightChestPosition;
                                    retPos += cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.AxisLockedFace:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaHeadPosition;
                                    tmpPos += liveCharactorLocators[i].liveCharaConstHeightHeadPosition;
                                    Vector3 vector3 = cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    retPos += vector3;
                                    tmpPos += vector3;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.AxisLockedChest:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaChestPosition;
                                    tmpPos += liveCharactorLocators[i].liveCharaConstHeightChestPosition;
                                    Vector3 vector2 = cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    retPos += vector2;
                                    tmpPos += vector2;
                                    num++;
                                }
                            }
                            break;
                        }
                    case LiveCameraLookAtCharaParts.AxisLockedWaist:
                        {
                            for (int i = 0; i < liveCharaPositionMax; i++)
                            {
                                if (posFlags.hasFlag((LiveCharaPosition)i) && liveCharactorLocators[i] != null)
                                {
                                    retPos += liveCharactorLocators[i].liveCharaWaistPosition;
                                    tmpPos += liveCharactorLocators[i].liveCharaConstHeightWaistPosition;
                                    Vector3 vector = cameraOffset * liveCharactorLocators[i].liveCharaHeightRatio;
                                    retPos += vector;
                                    tmpPos += vector;
                                    num++;
                                }
                            }
                            break;
                        }
                }
                bool flag = num > 1;
                if (flag)
                {
                    retPos /= num;
                }
                if ((uint)(parts - 11) <= 2u)
                {
                    if (flag)
                    {
                        tmpPos /= num;
                    }
                    retPos.y = tmpPos.y;
                }
            }
            return retPos;
        }

        public Vector3 GetPositionWithCharacters(LiveCharaPositionFlag posFlags, LiveCameraLookAtCharaParts parts)
        {
            return GetPositionWithCharacters(posFlags, parts, _cameraLayerOffset);
        }

        public static float CalculateInterpolationValue(LiveTimelineKey curKey, LiveTimelineKeyWithInterpolate nextKey, int frame)
        {
            float result = 0f;
            switch (nextKey.interpolateType)
            {
                case LiveCameraInterpolateType.Linear:
                    result = LinearInterpolateKeyframes(curKey, nextKey, frame);
                    break;
                case LiveCameraInterpolateType.Curve:
                    result = CurveInterpolateKeyframes(curKey, nextKey, frame);
                    break;
                case LiveCameraInterpolateType.Ease:
                    result = EaseInterpolateKeyframes(curKey, nextKey, frame);
                    break;
            }
            return result;
        }

        private void AlterUpdate_CameraPos(LiveTimelineWorkSheet sheet, int currentFrame, int sheetIndex)
        {
            if (sheet.cameraPosKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.cameraPosKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            if (camera == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.cameraPosKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyCameraPositionData liveTimelineKeyCameraPositionData = curKey as LiveTimelineKeyCameraPositionData;
            camera.camera.nearClipPlane = liveTimelineKeyCameraPositionData.nearClip;
            camera.camera.farClipPlane = liveTimelineKeyCameraPositionData.farClip;
            if (CalculateCameraPos(out var pos, sheet, curKey, nextKey, currentFrame))
            {
                camera.cacheTransform.position = pos;
                int num = liveTimelineKeyCameraPositionData.GetCullingMask();
                if (num == 0)
                {
                    num = LiveTimelineKeyCameraPositionData.GetDefaultCullingMask();
                }
                camera.camera.cullingMask = num;
                if (this.OnUpdateCameraPos != null)
                {
                    CameraPosUpdateInfo updateInfo = default(CameraPosUpdateInfo);
                    updateInfo.outlineZOffset = liveTimelineKeyCameraPositionData.outlineZOffset;
                    updateInfo.characterLODMask = (int)liveTimelineKeyCameraPositionData.characterLODMask;
                    this.OnUpdateCameraPos(ref updateInfo);
                }
            }
        }

        private static Vector3 GetCameraPosValue(LiveTimelineKeyCameraPositionData keyData, LiveTimelineControl timelineControl, FindTimelineConfig config)
        {
            return keyData.GetValue(timelineControl);
        }

        public bool CalculateCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = null;
            config.nextKey = null;
            config.keyType = FindTimelineConfig.KeyType.CurrentFrame;
            config.posKeys = sheet.cameraPosKeys;
            config.lookAtKeys = null;
            config.extraCameraIndex = 0;
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            return CalculateCameraPos(out pos, sheet, currentFrame, camera, ref config, ref fnGetCameraPosValue);
        }

        public bool CalculateCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, LiveTimelineKey curKey, LiveTimelineKey nextKey, int currentFrame)
        {
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = curKey;
            config.nextKey = nextKey;
            config.keyType = FindTimelineConfig.KeyType.KeyDirect;
            config.posKeys = sheet.cameraPosKeys;
            config.lookAtKeys = null;
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            config.extraCameraIndex = 0;
            return CalculateCameraPos(out pos, sheet, currentFrame, camera, ref config, ref fnGetCameraPosValue);
        }

        public bool CalculateCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, int currentFrame, CacheCamera targetCamera, ref FindTimelineConfig config, ref Func<LiveTimelineKeyCameraPositionData, LiveTimelineControl, FindTimelineConfig, Vector3> getFunc)
        {
            pos = Vector3.zero;
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            if (config.posKeys == null)
            {
                return false;
            }
            if (config.keyType == FindTimelineConfig.KeyType.CurrentFrame)
            {
                FindTimelineKey(out curKey, out nextKey, config.posKeys, currentFrame);
            }
            else
            {
                curKey = config.curKey;
                nextKey = config.nextKey;
            }
            if (curKey == null)
            {
                return false;
            }
            LiveTimelineKeyCameraPositionData liveTimelineKeyCameraPositionData = curKey as LiveTimelineKeyCameraPositionData;
            LiveTimelineKeyCameraPositionData liveTimelineKeyCameraPositionData2 = nextKey as LiveTimelineKeyCameraPositionData;
            if (liveTimelineKeyCameraPositionData2 != null && liveTimelineKeyCameraPositionData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyCameraPositionData, liveTimelineKeyCameraPositionData2, currentFrame);
                int bezierPointCount = liveTimelineKeyCameraPositionData2.GetBezierPointCount();
                if (bezierPointCount == 0)
                {
                    pos = LerpWithoutClamp(getFunc(liveTimelineKeyCameraPositionData, this, config), getFunc(liveTimelineKeyCameraPositionData2, this, config), t);
                }
                else if (liveTimelineKeyCameraPositionData2.necessaryToUseNewBezierCalcMethod)
                {
                    BezierCalcWork.cameraPos.Set(getFunc(liveTimelineKeyCameraPositionData, this, config), getFunc(liveTimelineKeyCameraPositionData2, this, config), bezierPointCount);
                    BezierCalcWork.cameraPos.UpdatePoints(liveTimelineKeyCameraPositionData2, this);
                    BezierCalcWork.cameraPos.Calc(bezierPointCount, t, out pos);
                }
                else
                {
                    Vector3 end = getFunc(liveTimelineKeyCameraPositionData2, this, config);
                    Vector3 cp = liveTimelineKeyCameraPositionData2.GetBezierPoint(0, this);
                    Vector3 cp2 = liveTimelineKeyCameraPositionData2.GetBezierPoint(1, this);
                    Vector3 cp3 = liveTimelineKeyCameraPositionData2.GetBezierPoint(2, this);
                    switch (liveTimelineKeyCameraPositionData2.GetBezierPointCount())
                    {
                        default:
                            pos = LerpWithoutClamp(getFunc(liveTimelineKeyCameraPositionData, this, config), getFunc(liveTimelineKeyCameraPositionData2, this, config), t);
                            break;
                        case 1:
                            {
                                Vector3 start3 = getFunc(liveTimelineKeyCameraPositionData, this, config);
                                BezierUtil.Calc(ref start3, ref end, ref cp, t, out pos);
                                break;
                            }
                        case 2:
                            {
                                Vector3 start2 = getFunc(liveTimelineKeyCameraPositionData, this, config);
                                BezierUtil.Calc(ref start2, ref end, ref cp, ref cp2, t, out pos);
                                break;
                            }
                        case 3:
                            {
                                Vector3 start = getFunc(liveTimelineKeyCameraPositionData, this, config);
                                BezierUtil.Calc(ref start, ref end, ref cp, ref cp2, ref cp3, t, out pos);
                                break;
                            }
                    }
                }
            }
            else
            {
                pos = getFunc(liveTimelineKeyCameraPositionData, this, config);
            }
            if (_isNowAlterUpdate && liveTimelineKeyCameraPositionData.attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayEnable) && (_oldFrame >= liveTimelineKeyCameraPositionData.frame || currentFrame < liveTimelineKeyCameraPositionData.frame || liveTimelineKeyCameraPositionData.attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayInherit)))
            {
                if (targetCamera == null)
                {
                    return false;
                }
                float t2 = liveTimelineKeyCameraPositionData.traceSpeed * _deltaTimeRatio;
                pos = Vector3.Slerp(targetCamera.cacheTransform.position, pos, t2);
            }
            return true;
        }

        private void AlterUpdate_CameraLookAt(LiveTimelineWorkSheet sheet, int currentFrame, ref Vector3 outLookAt)
        {
            if (!sheet.cameraLookAtKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && sheet.cameraLookAtKeys.EnablePlayModeTimeline(_playMode))
            {
                CacheCamera camera = GetCamera(sheet.targetCameraIndex);
                if (camera != null && CalculateCameraLookAt(out var lookAtPos, sheet, currentFrame))
                {
                    camera.cacheTransform.LookAt(lookAtPos, Vector3.up);
                    outLookAt = lookAtPos;
                }
            }
        }

        private static Vector3 GetCameraLookAtValue(LiveTimelineKeyCameraLookAtData keyData, LiveTimelineControl timelineControl, Vector3 camPos, FindTimelineConfig config)
        {
            return keyData.GetValue(timelineControl, camPos);
        }

        public bool CalculateCameraLookAt(out Vector3 lookAtPos, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = null;
            config.nextKey = null;
            config.keyType = FindTimelineConfig.KeyType.CurrentFrame;
            config.posKeys = sheet.cameraPosKeys;
            config.lookAtKeys = sheet.cameraLookAtKeys;
            config.extraCameraIndex = 0;
            return CalculateCameraLookAt(out lookAtPos, sheet, currentFrame, camera, ref config, ref fnGetCameraLookAtValue, ref fnGetCameraPosValue);
        }

        private bool CalculateCameraLookAt(out Vector3 lookAtPos, LiveTimelineWorkSheet sheet, int currentFrame, CacheCamera targetCamera, ref FindTimelineConfig config, ref Func<LiveTimelineKeyCameraLookAtData, LiveTimelineControl, Vector3, FindTimelineConfig, Vector3> getLookAtValueFunc, ref Func<LiveTimelineKeyCameraPositionData, LiveTimelineControl, FindTimelineConfig, Vector3> getPosValueFunc)
        {
            lookAtPos = Vector3.zero;
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            if (camera == null || config.lookAtKeys == null || config.posKeys == null)
            {
                return false;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, config.lookAtKeys, currentFrame);
            if (curKey == null)
            {
                return false;
            }
            Vector3 position = camera.cacheTransform.position;
            LiveTimelineKeyCameraLookAtData liveTimelineKeyCameraLookAtData = curKey as LiveTimelineKeyCameraLookAtData;
            LiveTimelineKeyCameraLookAtData liveTimelineKeyCameraLookAtData2 = nextKey as LiveTimelineKeyCameraLookAtData;
            if (liveTimelineKeyCameraLookAtData2 != null && liveTimelineKeyCameraLookAtData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyCameraLookAtData, liveTimelineKeyCameraLookAtData2, currentFrame);
                int bezierPointCount = liveTimelineKeyCameraLookAtData2.GetBezierPointCount();
                if (bezierPointCount == 0)
                {
                    lookAtPos = LerpWithoutClamp(getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config), getLookAtValueFunc(liveTimelineKeyCameraLookAtData2, this, position, config), t);
                }
                else if (liveTimelineKeyCameraLookAtData2.necessaryToUseNewBezierCalcMethod)
                {
                    Vector3 zero = Vector3.zero;
                    BezierCalcWork.cameraLookAt.Set(getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config), getLookAtValueFunc(liveTimelineKeyCameraLookAtData2, this, zero, config), bezierPointCount);
                    BezierCalcWork.cameraLookAt.UpdatePoints(liveTimelineKeyCameraLookAtData2, this, zero);
                    BezierCalcWork.cameraLookAt.Calc(bezierPointCount, t, out lookAtPos);
                }
                else
                {
                    Vector3 zero2 = Vector3.zero;
                    Vector3 end = getLookAtValueFunc(liveTimelineKeyCameraLookAtData2, this, zero2, config);
                    Vector3 cp = liveTimelineKeyCameraLookAtData2.GetBezierPoint(0, this, zero2);
                    Vector3 cp2 = liveTimelineKeyCameraLookAtData2.GetBezierPoint(1, this, zero2);
                    Vector3 cp3 = liveTimelineKeyCameraLookAtData2.GetBezierPoint(2, this, zero2);
                    switch (liveTimelineKeyCameraLookAtData2.GetBezierPointCount())
                    {
                        default:
                            lookAtPos = LerpWithoutClamp(getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config), getLookAtValueFunc(liveTimelineKeyCameraLookAtData2, this, position, config), t);
                            break;
                        case 1:
                            {
                                Vector3 start3 = getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config);
                                BezierUtil.Calc(ref start3, ref end, ref cp, t, out lookAtPos);
                                break;
                            }
                        case 2:
                            {
                                Vector3 start2 = getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config);
                                BezierUtil.Calc(ref start2, ref end, ref cp, ref cp2, t, out lookAtPos);
                                break;
                            }
                        case 3:
                            {
                                Vector3 start = getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config);
                                BezierUtil.Calc(ref start, ref end, ref cp, ref cp2, ref cp3, t, out lookAtPos);
                                break;
                            }
                    }
                }
            }
            else
            {
                lookAtPos = getLookAtValueFunc(liveTimelineKeyCameraLookAtData, this, position, config);
            }
            if (_isNowAlterUpdate && liveTimelineKeyCameraLookAtData.attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayEnable) && (_oldFrame >= liveTimelineKeyCameraLookAtData.frame || currentFrame < liveTimelineKeyCameraLookAtData.frame || liveTimelineKeyCameraLookAtData.attribute.hasFlag(LiveTimelineKeyAttribute.CameraDelayInherit)))
            {
                Vector3 b = lookAtPos - camera.cacheTransform.position;
                float magnitude = b.magnitude;
                if (magnitude >= float.Epsilon)
                {
                    b /= magnitude;
                    float t2 = liveTimelineKeyCameraLookAtData.traceSpeed * _deltaTimeRatio;
                    lookAtPos = position + Vector3.Slerp(camera.cacheTransform.forward, b, t2) * magnitude;
                }
            }
            return true;
        }
        private void AlterUpdate_CameraFov(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (sheet.cameraFovKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.cameraFovKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            if (camera == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.cameraFovKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyCameraFovData liveTimelineKeyCameraFovData = curKey as LiveTimelineKeyCameraFovData;
            LiveTimelineKeyCameraFovData liveTimelineKeyCameraFovData2 = nextKey as LiveTimelineKeyCameraFovData;
            float num = 80f;
            if (liveTimelineKeyCameraFovData2 != null && liveTimelineKeyCameraFovData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyCameraFovData, liveTimelineKeyCameraFovData2, currentFrame);
                num = LerpWithoutClamp(liveTimelineKeyCameraFovData.fov, liveTimelineKeyCameraFovData2.fov, t);
            }
            else if (liveTimelineKeyCameraFovData.fovType == LiveCameraFovType.Direct)
            {
                num = liveTimelineKeyCameraFovData.fov;
            }
            if (_limitFovForWidth)
            {
                float num2 = camera.camera.pixelWidth / (float)camera.camera.pixelHeight;
                if (num2 > _baseCameraAspectRatio)
                {
                    float num3 = num2 / _baseCameraAspectRatio;
                    num /= num3;
                }
            }
            camera.camera.fieldOfView = num;
        }

        private void AlterUpdate_CameraRoll(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (sheet.cameraRollKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.cameraRollKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            CacheCamera camera = GetCamera(sheet.targetCameraIndex);
            if (camera == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.cameraRollKeys, currentFrame);
            if (curKey != null)
            {
                LiveTimelineKeyCameraRollData liveTimelineKeyCameraRollData = curKey as LiveTimelineKeyCameraRollData;
                LiveTimelineKeyCameraRollData liveTimelineKeyCameraRollData2 = nextKey as LiveTimelineKeyCameraRollData;
                float num = 80f;
                if (liveTimelineKeyCameraRollData2 != null && liveTimelineKeyCameraRollData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyCameraRollData, liveTimelineKeyCameraRollData2, currentFrame);
                    num = LerpWithoutClamp(liveTimelineKeyCameraRollData.degree, liveTimelineKeyCameraRollData2.degree, t);
                }
                else
                {
                    num = liveTimelineKeyCameraRollData.degree;
                }
                camera.cacheTransform.Rotate(0f, 0f, num);
            }
        }

        private void AlterUpdate_CharaMotionSequence(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (_motionSequenceArray == null)
            {
                return;
            }
            bool flag = true;
            LiveTimelineKeyCharaMotionNoiseData liveTimelineKeyCharaMotionNoiseData = sheet.charaMotionNoiseKeys.FindKeyCached(currentFrame, availableFindKeyCache).key as LiveTimelineKeyCharaMotionNoiseData;
            if (liveTimelineKeyCharaMotionNoiseData != null)
            {
                flag = !liveTimelineKeyCharaMotionNoiseData.IsNoiseDisable();
            }
            LiveTimelineKeyCharaCySpring liveTimelineKeyCharaCySpring = sheet.charaCySpringKeys.FindKeyCached(currentFrame, availableFindKeyCache).key as LiveTimelineKeyCharaCySpring;
            bool flag2 = false;
            if (liveTimelineKeyCharaCySpring != null && !liveTimelineKeyCharaCySpring._isExecute)
            {
                flag2 = true;
                liveTimelineKeyCharaCySpring._isExecute = true;
            }
            if (this.OnUpdateCySpring == null)
            {
                flag2 = false;
            }
            float num = currentFrame * 0.0166666675f;
            CySpringUpdateInfo updateInfo = default(CySpringUpdateInfo);
            for (int i = 0; i < liveCharaPositionMax; i++)
            {
                if (_motionSequenceArray[i] != null)
                {
                    float num2 = num;
                    if (i > 0 && flag)
                    {
                        _charaMotNoiseTimeArray[i] += _deltaTime;
                        num2 += _chrMotNoiseBaseBias;
                        float f = _charaMotNoiseTimeArray[i] * kChrMotNoiseCurveRate;
                        num2 += _chrMotNoiseRange * Mathf.Cos(f);
                    }
                    if (num2 < 0f)
                    {
                        num2 = 0f;
                    }
                    _motionSequenceArray[i].AlterUpdate(num2, 60f);
                }
                if (flag2)
                {
                    updateInfo.position = (LiveCharaPosition)i;
                    updateInfo.resetCySpring = liveTimelineKeyCharaCySpring.IsResetCharaFlag((LiveCharaPosition)i);
                    if (liveTimelineKeyCharaCySpring.IsOverrideParameterCharaFlag((LiveCharaPosition)i))
                    {
                        updateInfo.overrideParameter = true;
                        updateInfo.overrideParameterAllBone = liveTimelineKeyCharaCySpring._overrideAllBone[i];
                        updateInfo.overrideType = liveTimelineKeyCharaCySpring._overrideParameterType[i];
                        updateInfo.overrideParameterAcc = liveTimelineKeyCharaCySpring._overrideAccBone[i];
                        updateInfo.overrideParameterAccSitffness = liveTimelineKeyCharaCySpring._overrideAccStiffness[i];
                        updateInfo.overrideParameterFurisode = liveTimelineKeyCharaCySpring._overrideFurisodeBone[i];
                        updateInfo.overrideParameterFurisodeStiffness = liveTimelineKeyCharaCySpring._overrideFurisodeStiffness[i];
                    }
                    else
                    {
                        updateInfo.overrideParameter = false;
                        updateInfo.overrideParameterAllBone = true;
                        updateInfo.overrideParameterAcc = false;
                        updateInfo.overrideParameterAccSitffness = 0f;
                        updateInfo.overrideParameterFurisode = false;
                        updateInfo.overrideParameterFurisodeStiffness = 0f;
                        updateInfo.overrideType = LiveTimelineKeyCharaCySpring.ParameterType.Reset;
                    }
                    this.OnUpdateCySpring(ref updateInfo);
                }
            }
        }

        private void SetCharaIKUpdateInfo(out CharaIKUpdateInfo.PartUpdateInfo partUpdateInfo, LiveCharaPosition position, int index, LiveTimelineWorkSheet sheet, LiveTimelineKeyCharaIKData curData, float blendRate)
        {
            if (curData.enableIk[index])
            {
                partUpdateInfo.blendRate = blendRate;
                partUpdateInfo.targetPosition = Vector3.zero;
                partUpdateInfo.targetPositionType = StageTwoBoneIK.TargetType.Transform;
            }
            else
            {
                partUpdateInfo.blendRate = 0f;
                partUpdateInfo.targetPosition = Vector3.zero;
                partUpdateInfo.targetPositionType = StageTwoBoneIK.TargetType.Position;
            }
        }

        private void AlterUpdate_IK(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateIK == null)
            {
                return;
            }
            LiveTimelineKeyCharaIKDataList[] iKKeyListArray = sheet.charaIkSet.GetIKKeyListArray();
            int num = 0;
            LiveTimelineKeyCharaIKDataList[] array = iKKeyListArray;
            CharaIKUpdateInfo updateInfo = default(CharaIKUpdateInfo);
            foreach (LiveTimelineKeyCharaIKDataList liveTimelineKeyCharaIKDataList in array)
            {
                if (liveTimelineKeyCharaIKDataList.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    updateInfo = default(CharaIKUpdateInfo);
                    updateInfo.position = (LiveCharaPosition)num;
                    this.OnUpdateIK(ref updateInfo);
                    num++;
                }
                else
                {
                    if (liveCharactorLocators[num] == null)
                    {
                        continue;
                    }
                    if (!liveTimelineKeyCharaIKDataList.EnablePlayModeTimeline(_playMode))
                    {
                        num++;
                        continue;
                    }
                    LiveTimelineKey curKey = null;
                    LiveTimelineKey nextKey = null;
                    FindTimelineKey(out curKey, out nextKey, liveTimelineKeyCharaIKDataList, currentFrame);
                    if (curKey == null)
                    {
                        num++;
                        continue;
                    }
                    LiveTimelineKeyCharaIKData liveTimelineKeyCharaIKData = curKey as LiveTimelineKeyCharaIKData;
                    LiveTimelineKeyCharaIKData liveTimelineKeyCharaIKData2 = nextKey as LiveTimelineKeyCharaIKData;
                    LiveCharaPosition position = (updateInfo.position = (LiveCharaPosition)num);
                    if (liveTimelineKeyCharaIKData2 != null && liveTimelineKeyCharaIKData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyCharaIKData, liveTimelineKeyCharaIKData2, currentFrame);
                        float blendRate = LerpWithoutClamp(liveTimelineKeyCharaIKData.blendRate[0], liveTimelineKeyCharaIKData2.blendRate[0], t);
                        SetCharaIKUpdateInfo(out updateInfo.leg_Left, position, 0, sheet, liveTimelineKeyCharaIKData, blendRate);
                        blendRate = LerpWithoutClamp(liveTimelineKeyCharaIKData.blendRate[1], liveTimelineKeyCharaIKData2.blendRate[1], t);
                        SetCharaIKUpdateInfo(out updateInfo.leg_Right, position, 1, sheet, liveTimelineKeyCharaIKData, blendRate);
                        blendRate = LerpWithoutClamp(liveTimelineKeyCharaIKData.blendRate[2], liveTimelineKeyCharaIKData2.blendRate[2], t);
                        SetCharaIKUpdateInfo(out updateInfo.arm_Left, position, 2, sheet, liveTimelineKeyCharaIKData, blendRate);
                        blendRate = LerpWithoutClamp(liveTimelineKeyCharaIKData.blendRate[3], liveTimelineKeyCharaIKData2.blendRate[3], t);
                        SetCharaIKUpdateInfo(out updateInfo.arm_Right, position, 3, sheet, liveTimelineKeyCharaIKData, blendRate);
                    }
                    else
                    {
                        SetCharaIKUpdateInfo(out updateInfo.leg_Left, position, 0, sheet, liveTimelineKeyCharaIKData, liveTimelineKeyCharaIKData.blendRate[0]);
                        SetCharaIKUpdateInfo(out updateInfo.leg_Right, position, 1, sheet, liveTimelineKeyCharaIKData, liveTimelineKeyCharaIKData.blendRate[1]);
                        SetCharaIKUpdateInfo(out updateInfo.arm_Left, position, 2, sheet, liveTimelineKeyCharaIKData, liveTimelineKeyCharaIKData.blendRate[2]);
                        SetCharaIKUpdateInfo(out updateInfo.arm_Right, position, 3, sheet, liveTimelineKeyCharaIKData, liveTimelineKeyCharaIKData.blendRate[3]);
                    }
                    this.OnUpdateIK(ref updateInfo);
                    num++;
                }
            }
        }

        private void AlterUpdate_CharaFootLight(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateCharaFootLight == null)
            {
                return;
            }
            CharaFootLightUpdateInfo updateInfo = default(CharaFootLightUpdateInfo);
            if (sheet.charaFootLightKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
            {
                for (int i = 0; i < 15; i++)
                {
                    updateInfo.position = (LiveCharaPosition)i;
                    updateInfo.hightMax = 1f;
                    updateInfo.lightColor = Color.black;
                    updateInfo.alphaBlend = false;
                    updateInfo.inverseAlpha = false;
                    this.OnUpdateCharaFootLight(ref updateInfo);
                }
                return;
            }
            FindTimelineKey(out var curKey, out var nextKey, sheet.charaFootLightKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyCharaFootLightData liveTimelineKeyCharaFootLightData = curKey as LiveTimelineKeyCharaFootLightData;
            LiveTimelineKeyCharaFootLightData liveTimelineKeyCharaFootLightData2 = nextKey as LiveTimelineKeyCharaFootLightData;
            if (liveTimelineKeyCharaFootLightData2 != null && liveTimelineKeyCharaFootLightData2.interpolateType != 0)
            {
                for (int j = 0; j < 15; j++)
                {
                    updateInfo.position = (LiveCharaPosition)j;
                    if (liveTimelineKeyCharaFootLightData.positionFlag.hasFlag((LiveCharaPosition)j))
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyCharaFootLightData, liveTimelineKeyCharaFootLightData2, currentFrame);
                        updateInfo.hightMax = LerpWithoutClamp(liveTimelineKeyCharaFootLightData.hightMax[j], liveTimelineKeyCharaFootLightData2.hightMax[j], t);
                        updateInfo.lightColor = LerpWithoutClamp(liveTimelineKeyCharaFootLightData.lightColor[j], liveTimelineKeyCharaFootLightData2.lightColor[j], t);
                        updateInfo.alphaBlend = liveTimelineKeyCharaFootLightData.alphaBlend[j];
                        updateInfo.inverseAlpha = liveTimelineKeyCharaFootLightData.inverseAlpha[j];
                    }
                    else
                    {
                        updateInfo.hightMax = 1f;
                        updateInfo.lightColor.r = 0f;
                        updateInfo.lightColor.g = 0f;
                        updateInfo.lightColor.b = 0f;
                        updateInfo.lightColor.a = 1f;
                        updateInfo.alphaBlend = false;
                        updateInfo.inverseAlpha = false;
                    }
                    this.OnUpdateCharaFootLight(ref updateInfo);
                }
                return;
            }
            for (int k = 0; k < 15; k++)
            {
                updateInfo.position = (LiveCharaPosition)k;
                if (liveTimelineKeyCharaFootLightData.positionFlag.hasFlag((LiveCharaPosition)k))
                {
                    updateInfo.hightMax = liveTimelineKeyCharaFootLightData.hightMax[k];
                    updateInfo.alphaBlend = liveTimelineKeyCharaFootLightData.alphaBlend[k];
                    updateInfo.inverseAlpha = liveTimelineKeyCharaFootLightData.inverseAlpha[k];
                    updateInfo.lightColor = liveTimelineKeyCharaFootLightData.lightColor[k];
                }
                else
                {
                    updateInfo.hightMax = 1f;
                    updateInfo.lightColor.r = 0f;
                    updateInfo.lightColor.g = 0f;
                    updateInfo.lightColor.b = 0f;
                    updateInfo.lightColor.a = 1f;
                    updateInfo.alphaBlend = false;
                    updateInfo.inverseAlpha = false;
                }
                this.OnUpdateCharaFootLight(ref updateInfo);
            }
        }

        private void AlterUpdate_StageGazeControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateStageGazeControl == null)
            {
                return;
            }
            int count = sheet.stageGazeList.Count;
            StageGazeControlUpdateInfo updateInfo = default(StageGazeControlUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineStageGazeData liveTimelineStageGazeData = sheet.stageGazeList[i];
                LiveTimelineKeyStageGazeDataList keys = liveTimelineStageGazeData.keys;
                if (!keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && keys.EnablePlayModeTimeline(_playMode))
                {
                    LiveTimelineKey curKey = null;
                    LiveTimelineKey nextKey = null;
                    FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
                    if (curKey == null)
                    {
                        break;
                    }
                    LiveTimelineKeyStageGazeData liveTimelineKeyStageGazeData = curKey as LiveTimelineKeyStageGazeData;
                    LiveTimelineKeyStageGazeData liveTimelineKeyStageGazeData2 = nextKey as LiveTimelineKeyStageGazeData;
                    updateInfo.isEnable = liveTimelineKeyStageGazeData.isEnable;
                    updateInfo.groupNo = liveTimelineStageGazeData.groupNo;
                    if (liveTimelineKeyStageGazeData2 != null && liveTimelineKeyStageGazeData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyStageGazeData, liveTimelineKeyStageGazeData2, currentFrame);
                        updateInfo.targetPosition = LerpWithoutClamp(liveTimelineKeyStageGazeData.targetPosition, liveTimelineKeyStageGazeData2.targetPosition, t);
                    }
                    else
                    {
                        updateInfo.targetPosition = liveTimelineKeyStageGazeData.targetPosition;
                    }
                    this.OnUpdateStageGazeControl(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_MobCyalume(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateMobCyalume == null)
            {
                return;
            }
            MobCyalumeUpdateInfo updateInfo = default(MobCyalumeUpdateInfo);
            int num = Math.Min(sheet.mobCyalumeDataList.Count, 11);
            for (int i = 0; i < num; i++)
            {
                updateInfo.groupNo = i;
                LiveTimelineKeyMobCyalumeDataList keys = sheet.mobCyalumeDataList[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    updateInfo.translate = Vector3.zero;
                    updateInfo.rotate = Vector3.zero;
                    updateInfo.scale = Vector3.one;
                    updateInfo.isVisibleMob = true;
                    updateInfo.isVisibleCyalume = true;
                    this.OnUpdateMobCyalume(ref updateInfo);
                    continue;
                }
                FindTimelineKey(out var curKey, out var nextKey, keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyMobCyalumeData liveTimelineKeyMobCyalumeData = curKey as LiveTimelineKeyMobCyalumeData;
                    LiveTimelineKeyMobCyalumeData liveTimelineKeyMobCyalumeData2 = nextKey as LiveTimelineKeyMobCyalumeData;
                    updateInfo.isVisibleMob = liveTimelineKeyMobCyalumeData.isMobVisible;
                    updateInfo.isVisibleCyalume = liveTimelineKeyMobCyalumeData.isCyalumeVisible;
                    if (liveTimelineKeyMobCyalumeData2 != null && liveTimelineKeyMobCyalumeData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyMobCyalumeData, liveTimelineKeyMobCyalumeData2, currentFrame);
                        updateInfo.translate = LerpWithoutClamp(liveTimelineKeyMobCyalumeData.translate, liveTimelineKeyMobCyalumeData2.translate, t);
                        updateInfo.rotate = LerpWithoutClamp(liveTimelineKeyMobCyalumeData.rotate, liveTimelineKeyMobCyalumeData2.rotate, t);
                        updateInfo.scale = LerpWithoutClamp(liveTimelineKeyMobCyalumeData.scale, liveTimelineKeyMobCyalumeData2.scale, t);
                    }
                    else
                    {
                        updateInfo.translate = liveTimelineKeyMobCyalumeData.translate;
                        updateInfo.rotate = liveTimelineKeyMobCyalumeData.rotate;
                        updateInfo.scale = liveTimelineKeyMobCyalumeData.scale;
                    }
                    this.OnUpdateMobCyalume(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_MobCyalume3D(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateMobCyalume3D == null)
            {
                return;
            }
            MobCyalume3DUpdateInfo updateInfo = default(MobCyalume3DUpdateInfo);
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.mobCyalume3DRootKeys, currentFrame);
            if (curKey == null)
            {
                updateInfo.rootTranslate = Vector3.zero;
                updateInfo.rootRotate = Vector3.zero;
                updateInfo.rootScale = Vector3.one;
                updateInfo.isVisibleMob = true;
                updateInfo.isVisibleCyalume = true;
                updateInfo.isEnableMotionMultiSample = false;
                updateInfo.motionTimeOffset = 0f;
                updateInfo.motionTimeInterval = 1f;
                updateInfo.waveMode = LiveMobCyalume3DWaveMode.None;
                updateInfo.waveBasePosition = Vector3.zero;
                updateInfo.waveWidth = 5f;
                updateInfo.waveHeight = 1f;
                updateInfo.waveRoughness = 0f;
                updateInfo.waveProgress = 0f;
                updateInfo.waveColorBasePower = 1f;
                updateInfo.waveColorGainPower = 0f;
            }
            else
            {
                LiveTimelineKeyMobCyalume3DRootData liveTimelineKeyMobCyalume3DRootData = curKey as LiveTimelineKeyMobCyalume3DRootData;
                LiveTimelineKeyMobCyalume3DRootData liveTimelineKeyMobCyalume3DRootData2 = nextKey as LiveTimelineKeyMobCyalume3DRootData;
                if (liveTimelineKeyMobCyalume3DRootData2 != null && liveTimelineKeyMobCyalume3DRootData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyMobCyalume3DRootData, liveTimelineKeyMobCyalume3DRootData2, currentFrame);
                    updateInfo.rootTranslate = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.translate, liveTimelineKeyMobCyalume3DRootData2.translate, t);
                    updateInfo.rootRotate = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.rotate, liveTimelineKeyMobCyalume3DRootData2.rotate, t);
                    updateInfo.rootScale = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.scale, liveTimelineKeyMobCyalume3DRootData2.scale, t);
                    updateInfo.motionTimeOffset = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.motionTimeOffset, liveTimelineKeyMobCyalume3DRootData2.motionTimeOffset, t);
                    updateInfo.motionTimeInterval = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.motionTimeInterval, liveTimelineKeyMobCyalume3DRootData2.motionTimeInterval, t);
                    updateInfo.waveBasePosition = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveBasePosition, liveTimelineKeyMobCyalume3DRootData2.waveBasePosition, t);
                    updateInfo.waveWidth = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveWidth, liveTimelineKeyMobCyalume3DRootData2.waveWidth, t);
                    updateInfo.waveHeight = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveHeight, liveTimelineKeyMobCyalume3DRootData2.waveHeight, t);
                    updateInfo.waveRoughness = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveRoughness, liveTimelineKeyMobCyalume3DRootData2.waveRoughness, t);
                    updateInfo.waveProgress = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveProgress, liveTimelineKeyMobCyalume3DRootData2.waveProgress, t);
                    updateInfo.waveColorBasePower = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveColorBasePower, liveTimelineKeyMobCyalume3DRootData2.waveColorBasePower, t);
                    updateInfo.waveColorGainPower = LerpWithoutClamp(liveTimelineKeyMobCyalume3DRootData.waveColorGainPower, liveTimelineKeyMobCyalume3DRootData2.waveColorGainPower, t);
                }
                else
                {
                    updateInfo.rootTranslate = liveTimelineKeyMobCyalume3DRootData.translate;
                    updateInfo.rootRotate = liveTimelineKeyMobCyalume3DRootData.rotate;
                    updateInfo.rootScale = liveTimelineKeyMobCyalume3DRootData.scale;
                    updateInfo.motionTimeOffset = liveTimelineKeyMobCyalume3DRootData.motionTimeOffset;
                    updateInfo.motionTimeInterval = liveTimelineKeyMobCyalume3DRootData.motionTimeInterval;
                    updateInfo.waveBasePosition = liveTimelineKeyMobCyalume3DRootData.waveBasePosition;
                    updateInfo.waveWidth = liveTimelineKeyMobCyalume3DRootData.waveWidth;
                    updateInfo.waveHeight = liveTimelineKeyMobCyalume3DRootData.waveHeight;
                    updateInfo.waveRoughness = liveTimelineKeyMobCyalume3DRootData.waveRoughness;
                    updateInfo.waveProgress = liveTimelineKeyMobCyalume3DRootData.waveProgress;
                    updateInfo.waveColorBasePower = liveTimelineKeyMobCyalume3DRootData.waveColorBasePower;
                    updateInfo.waveColorGainPower = liveTimelineKeyMobCyalume3DRootData.waveColorGainPower;
                }
                updateInfo.isVisibleMob = liveTimelineKeyMobCyalume3DRootData.isVisibleMob;
                updateInfo.isVisibleCyalume = liveTimelineKeyMobCyalume3DRootData.isVisibleCyalume;
                updateInfo.isEnableMotionMultiSample = liveTimelineKeyMobCyalume3DRootData.isEnableMotionMultiSample;
                updateInfo.waveMode = liveTimelineKeyMobCyalume3DRootData.waveMode;
            }
            LiveTimelineKey curKey2 = null;
            LiveTimelineKey nextKey2 = null;
            FindTimelineKey(out curKey2, out nextKey2, sheet.mobCyalume3DLightingKeys, currentFrame);
            if (curKey2 == null)
            {
                updateInfo.gradiation = 0.05f;
                updateInfo.rimlight = 6f;
                updateInfo.blendRange = 20f;
            }
            else
            {
                LiveTimelineKeyMobCyalume3DLightingData liveTimelineKeyMobCyalume3DLightingData = curKey2 as LiveTimelineKeyMobCyalume3DLightingData;
                LiveTimelineKeyMobCyalume3DLightingData liveTimelineKeyMobCyalume3DLightingData2 = nextKey2 as LiveTimelineKeyMobCyalume3DLightingData;
                if (liveTimelineKeyMobCyalume3DLightingData2 != null && liveTimelineKeyMobCyalume3DLightingData2.interpolateType != 0)
                {
                    float t2 = CalculateInterpolationValue(liveTimelineKeyMobCyalume3DLightingData, liveTimelineKeyMobCyalume3DLightingData2, currentFrame);
                    updateInfo.gradiation = LerpWithoutClamp(liveTimelineKeyMobCyalume3DLightingData.gradiation, liveTimelineKeyMobCyalume3DLightingData2.gradiation, t2);
                    updateInfo.rimlight = LerpWithoutClamp(liveTimelineKeyMobCyalume3DLightingData.rimlight, liveTimelineKeyMobCyalume3DLightingData2.rimlight, t2);
                    updateInfo.blendRange = LerpWithoutClamp(liveTimelineKeyMobCyalume3DLightingData.blendRange, liveTimelineKeyMobCyalume3DLightingData2.blendRange, t2);
                }
                else
                {
                    updateInfo.gradiation = liveTimelineKeyMobCyalume3DLightingData.gradiation;
                    updateInfo.rimlight = liveTimelineKeyMobCyalume3DLightingData.rimlight;
                    updateInfo.blendRange = liveTimelineKeyMobCyalume3DLightingData.blendRange;
                }
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int num = -1;
            LiveTimelineKeyMobCyalume3DColorDataList mobCyalume3DColorKeys = sheet.mobCyalume3DColorKeys;
            FindKeyResult findKeyResult = mobCyalume3DColorKeys.FindKeyCached(currentFrame, availableFindKeyCache);
            liveTimelineKey = findKeyResult.key;
            if (liveTimelineKey != null)
            {
                num = findKeyResult.index;
                liveTimelineKey2 = mobCyalume3DColorKeys.At(num + 1);
            }
            else
            {
                liveTimelineKey2 = null;
            }
            if (liveTimelineKey == null)
            {
                updateInfo.paletteScrollSection = 0f;
            }
            else
            {
                int frame = liveTimelineKey.frame;
                int num2 = liveTimelineKey2?.frame ?? frame;
                bool num3 = liveTimelineKey2?.IsInterpolateKey() ?? false;
                float num4 = 0f;
                num4 = ((num3 || (frame != num2 && frame + 120 > num2)) ? (num + (currentFrame - frame) / (float)(num2 - frame) - 0.5f) : ((currentFrame < frame + 60) ? (num + (currentFrame - frame) / 120f - 0.5f) : ((frame == num2 || num2 - 60 >= currentFrame) ? num : (num + 1 - (num2 - currentFrame) / 120f - 0.5f))));
                if (num4 < 0f)
                {
                    num4 = 0f;
                }
                updateInfo.paletteScrollSection = num4;
            }
            LiveTimelineKey curKey3 = null;
            LiveTimelineKey nextKey3 = null;
            FindTimelineKey(out curKey3, out nextKey3, sheet.mobCyalume3DBloomKeys, currentFrame);
            if (curKey3 == null)
            {
                updateInfo.horizontalOffset = 2.4f;
                updateInfo.verticalOffset = 0.6f;
                updateInfo.threshold = 1f;
                updateInfo.growPower = 1.4f;
            }
            else
            {
                LiveTimelineKeyMobCyalume3DBloomData liveTimelineKeyMobCyalume3DBloomData = curKey3 as LiveTimelineKeyMobCyalume3DBloomData;
                LiveTimelineKeyMobCyalume3DBloomData liveTimelineKeyMobCyalume3DBloomData2 = nextKey3 as LiveTimelineKeyMobCyalume3DBloomData;
                if (liveTimelineKeyMobCyalume3DBloomData2 != null && liveTimelineKeyMobCyalume3DBloomData2.interpolateType != 0)
                {
                    float t3 = CalculateInterpolationValue(liveTimelineKeyMobCyalume3DBloomData, liveTimelineKeyMobCyalume3DBloomData2, currentFrame);
                    updateInfo.horizontalOffset = LerpWithoutClamp(liveTimelineKeyMobCyalume3DBloomData.horizontalOffset, liveTimelineKeyMobCyalume3DBloomData2.horizontalOffset, t3);
                    updateInfo.verticalOffset = LerpWithoutClamp(liveTimelineKeyMobCyalume3DBloomData.verticalOffset, liveTimelineKeyMobCyalume3DBloomData2.verticalOffset, t3);
                    updateInfo.threshold = LerpWithoutClamp(liveTimelineKeyMobCyalume3DBloomData.threshold, liveTimelineKeyMobCyalume3DBloomData2.threshold, t3);
                    updateInfo.growPower = LerpWithoutClamp(liveTimelineKeyMobCyalume3DBloomData.growPower, liveTimelineKeyMobCyalume3DBloomData2.growPower, t3);
                }
                else
                {
                    updateInfo.horizontalOffset = liveTimelineKeyMobCyalume3DBloomData.horizontalOffset;
                    updateInfo.verticalOffset = liveTimelineKeyMobCyalume3DBloomData.verticalOffset;
                    updateInfo.threshold = liveTimelineKeyMobCyalume3DBloomData.threshold;
                    updateInfo.growPower = liveTimelineKeyMobCyalume3DBloomData.growPower;
                }
            }
            LiveTimelineKey curKey4 = null;
            LiveTimelineKey nextKey4 = null;
            FindTimelineKey(out curKey4, out nextKey4, sheet.mobCyalume3DLookAtModeKeys, currentFrame);
            if (curKey4 == null)
            {
                updateInfo.lookAtMode = LiveMobCyalume3DLookAtMode.Locator;
            }
            else
            {
                LiveTimelineKeyMobCyalume3DLookAtModeData liveTimelineKeyMobCyalume3DLookAtModeData = curKey4 as LiveTimelineKeyMobCyalume3DLookAtModeData;
                updateInfo.lookAtMode = liveTimelineKeyMobCyalume3DLookAtModeData.lookAtMode;
            }
            int num5 = (updateInfo.lookAtPositionCount = Math.Min(sheet.mobCyalume3DLookAtPositionDataList.Count, 11));
            updateInfo.lookAtPositionList = _arrMobCyalume3DLookAtPosition;
            for (int i = 0; i < num5; i++)
            {
                LiveTimelineKeyMobCyalume3DLookAtPositionDataList keys = sheet.mobCyalume3DLookAtPositionDataList[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    updateInfo.lookAtPositionList[i] = Vector3.zero;
                    continue;
                }
                FindTimelineKey(out var curKey5, out var nextKey5, keys, currentFrame);
                if (curKey5 != null)
                {
                    LiveTimelineKeyMobCyalume3DLookAtPositionData liveTimelineKeyMobCyalume3DLookAtPositionData = curKey5 as LiveTimelineKeyMobCyalume3DLookAtPositionData;
                    LiveTimelineKeyMobCyalume3DLookAtPositionData liveTimelineKeyMobCyalume3DLookAtPositionData2 = nextKey5 as LiveTimelineKeyMobCyalume3DLookAtPositionData;
                    if (liveTimelineKeyMobCyalume3DLookAtPositionData2 != null && liveTimelineKeyMobCyalume3DLookAtPositionData2.interpolateType != 0)
                    {
                        float t4 = CalculateInterpolationValue(liveTimelineKeyMobCyalume3DLookAtPositionData, liveTimelineKeyMobCyalume3DLookAtPositionData2, currentFrame);
                        updateInfo.lookAtPositionList[i] = LerpWithoutClamp(liveTimelineKeyMobCyalume3DLookAtPositionData.translate, liveTimelineKeyMobCyalume3DLookAtPositionData2.translate, t4);
                    }
                    else
                    {
                        updateInfo.lookAtPositionList[i] = liveTimelineKeyMobCyalume3DLookAtPositionData.translate;
                    }
                }
            }
            num5 = (updateInfo.positionCount = Math.Min(sheet.mobCyalume3DPositionDataList.Count, 11));
            updateInfo.positionList = _arrMobCyalume3DPosition;
            for (int j = 0; j < num5; j++)
            {
                LiveTimelineKeyMobCyalume3DPositionDataList keys2 = sheet.mobCyalume3DPositionDataList[j].keys;
                if (keys2.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    updateInfo.positionList[j] = Vector3.zero;
                    continue;
                }
                FindTimelineKey(out var curKey6, out var nextKey6, keys2, currentFrame);
                if (curKey6 != null)
                {
                    LiveTimelineKeyMobCyalume3DPositionData liveTimelineKeyMobCyalume3DPositionData = curKey6 as LiveTimelineKeyMobCyalume3DPositionData;
                    LiveTimelineKeyMobCyalume3DPositionData liveTimelineKeyMobCyalume3DPositionData2 = nextKey6 as LiveTimelineKeyMobCyalume3DPositionData;
                    if (liveTimelineKeyMobCyalume3DPositionData2 != null && liveTimelineKeyMobCyalume3DPositionData2.interpolateType != 0)
                    {
                        float t5 = CalculateInterpolationValue(liveTimelineKeyMobCyalume3DPositionData, liveTimelineKeyMobCyalume3DPositionData2, currentFrame);
                        updateInfo.positionList[j] = LerpWithoutClamp(liveTimelineKeyMobCyalume3DPositionData.translate, liveTimelineKeyMobCyalume3DPositionData2.translate, t5);
                    }
                    else
                    {
                        updateInfo.positionList[j] = liveTimelineKeyMobCyalume3DPositionData.translate;
                    }
                }
            }
            this.OnUpdateMobCyalume3D(ref updateInfo);
        }

        private void AlterUpdate_Event(LiveTimelineWorkSheet sheet, int currentFrame, int oldFrame)
        {
            if (sheet.eventKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.eventKeys.EnablePlayModeTimeline(_playMode) || currentFrame <= oldFrame)
            {
                return;
            }
            LiveTimelineKey triggeredKey = null;
            FindTimelineKeyTrigger(out triggeredKey, sheet.eventKeys, currentFrame, oldFrame);
            if (triggeredKey != null && triggeredKey.attribute.hasFlag(LiveTimelineKeyAttribute.Disable))
            {
                triggeredKey = null;
            }
            if (triggeredKey == null)
            {
                return;
            }
            if (oldFrame <= lastTriggeredEventFrame && lastTriggeredEventFrame <= currentFrame)
            {
                lastTriggeredEventFrame = -1;
                return;
            }
            LiveTimelineKeyEventData liveTimelineKeyEventData = triggeredKey as LiveTimelineKeyEventData;
            if (liveTimelineKeyEventData != null)
            {
                lastTriggeredEventFrame = triggeredKey.frame;
                eventPublisher.FireEvent(liveTimelineKeyEventData);
            }
        }

        private void AlterUpdate_BgColor1(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateBgColor1 == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            BgColor1UpdateInfo updateInfo = default(BgColor1UpdateInfo);
            for (int i = 0; i < sheet.bgColor1List.Count; i++)
            {
                LiveTimelineBgColor1Data liveTimelineBgColor1Data = sheet.bgColor1List[i];
                if (liveTimelineBgColor1Data.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineBgColor1Data.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineBgColor1Data.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyBgColor1Data liveTimelineKeyBgColor1Data = curKey as LiveTimelineKeyBgColor1Data;
                    LiveTimelineKeyBgColor1Data liveTimelineKeyBgColor1Data2 = nextKey as LiveTimelineKeyBgColor1Data;
                    if (liveTimelineKeyBgColor1Data2 != null && liveTimelineKeyBgColor1Data2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyBgColor1Data, liveTimelineKeyBgColor1Data2, currentFrame);
                        updateInfo.color = LerpWithoutClamp(liveTimelineKeyBgColor1Data.color, liveTimelineKeyBgColor1Data2.color, t);
                        updateInfo.value = LerpWithoutClamp(liveTimelineKeyBgColor1Data.f32, liveTimelineKeyBgColor1Data2.f32, t);
                    }
                    else
                    {
                        updateInfo.color = liveTimelineKeyBgColor1Data.color;
                        updateInfo.value = liveTimelineKeyBgColor1Data.f32;
                    }
                    updateInfo.data = liveTimelineBgColor1Data;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyBgColor1Data.frame) * 0.0166666675f;
                    updateInfo.flags = liveTimelineKeyBgColor1Data.flags;
                    this.OnUpdateBgColor1(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_BgColor2(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateBgColor2 == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            BgColor2UpdateInfo updateInfo = default(BgColor2UpdateInfo);
            for (int i = 0; i < sheet.bgColor2List.Count; i++)
            {
                LiveTimelineBgColor2Data liveTimelineBgColor2Data = sheet.bgColor2List[i];
                if (liveTimelineBgColor2Data.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineBgColor2Data.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineBgColor2Data.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyBgColor2Data liveTimelineKeyBgColor2Data = curKey as LiveTimelineKeyBgColor2Data;
                    LiveTimelineKeyBgColor2Data liveTimelineKeyBgColor2Data2 = nextKey as LiveTimelineKeyBgColor2Data;
                    if (liveTimelineKeyBgColor2Data2 != null && liveTimelineKeyBgColor2Data2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyBgColor2Data, liveTimelineKeyBgColor2Data2, currentFrame);
                        updateInfo.color1 = LerpWithoutClamp(liveTimelineKeyBgColor2Data.color1, liveTimelineKeyBgColor2Data2.color1, t);
                        updateInfo.color2 = LerpWithoutClamp(liveTimelineKeyBgColor2Data.color2, liveTimelineKeyBgColor2Data2.color2, t);
                        updateInfo.value = LerpWithoutClamp(liveTimelineKeyBgColor2Data.f32, liveTimelineKeyBgColor2Data2.f32, t);
                        updateInfo.enableManualAngle = liveTimelineKeyBgColor2Data.manualAngle;
                        updateInfo.manualRotation = Quaternion.Lerp(liveTimelineKeyBgColor2Data.manualRotation, liveTimelineKeyBgColor2Data2.manualRotation, t);
                    }
                    else
                    {
                        updateInfo.color1 = liveTimelineKeyBgColor2Data.color1;
                        updateInfo.color2 = liveTimelineKeyBgColor2Data.color2;
                        updateInfo.value = liveTimelineKeyBgColor2Data.f32;
                        updateInfo.enableManualAngle = liveTimelineKeyBgColor2Data.manualAngle;
                        updateInfo.manualRotation = liveTimelineKeyBgColor2Data.manualRotation;
                    }
                    updateInfo.data = liveTimelineBgColor2Data;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyBgColor2Data.frame) * 0.0166666675f;
                    updateInfo.rndValueIdx = liveTimelineKeyBgColor2Data.RandomTableIndex();
                    updateInfo.groupIndex = liveTimelineBgColor2Data.groupNo;
                    this.OnUpdateBgColor2(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_BgColor3(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateBgColor3 == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            for (int i = 0; i < sheet.bgColor3List.Count; i++)
            {
                LiveTimelineBgColor3Data liveTimelineBgColor3Data = sheet.bgColor3List[i];
                if (liveTimelineBgColor3Data.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineBgColor3Data.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineBgColor3Data.keys, currentFrame);
                if (curKey == null)
                {
                    continue;
                }
                LiveTimelineKeyBgColor3Data liveTimelineKeyBgColor3Data = curKey as LiveTimelineKeyBgColor3Data;
                LiveTimelineKeyBgColor3Data liveTimelineKeyBgColor3Data2 = nextKey as LiveTimelineKeyBgColor3Data;
                if (liveTimelineKeyBgColor3Data2 != null && liveTimelineKeyBgColor3Data2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyBgColor3Data, liveTimelineKeyBgColor3Data2, currentFrame);
                    for (int j = 0; j < _bgColor3UpdateInfo.colorArray.Length; j++)
                    {
                        _bgColor3UpdateInfo.colorArray[j] = LerpWithoutClamp(liveTimelineKeyBgColor3Data._colorArray[j], liveTimelineKeyBgColor3Data2._colorArray[j], t);
                    }
                }
                else
                {
                    for (int k = 0; k < _bgColor3UpdateInfo.colorArray.Length; k++)
                    {
                        _bgColor3UpdateInfo.colorArray[k] = liveTimelineKeyBgColor3Data._colorArray[k];
                    }
                }
                _bgColor3UpdateInfo.data = liveTimelineBgColor3Data;
                _bgColor3UpdateInfo.progressTime = (currentFrame - liveTimelineKeyBgColor3Data.frame) * 0.0166666675f;
                this.OnUpdateBgColor3(ref _bgColor3UpdateInfo);
            }
        }

        /// <summary>
        /// VolumeLight(Sunshuft)用AlterUpdate
        /// </summary>
        private void AlterUpdate_VolumeLight(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateVolumeLight == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            float num = 0.0166666675f;
            VolumeLightUpdateInfo updateInfo = default(VolumeLightUpdateInfo);
            for (int i = 0; i < sheet.VolumeLightKeys.Count; i++)
            {
                LiveTimelineVolumeLightData liveTimelineVolumeLightData = sheet.VolumeLightKeys[i];
                if (liveTimelineVolumeLightData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineVolumeLightData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyVolumeLightData liveTimelineKeyVolumeLightData = curKey as LiveTimelineKeyVolumeLightData;
                    LiveTimelineKeyVolumeLightData liveTimelineKeyVolumeLightData2 = nextKey as LiveTimelineKeyVolumeLightData;
                    if (liveTimelineKeyVolumeLightData2 != null && liveTimelineKeyVolumeLightData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyVolumeLightData, liveTimelineKeyVolumeLightData2, currentFrame);
                        updateInfo.sunPosition = LerpWithoutClamp(liveTimelineKeyVolumeLightData.sunPosition, liveTimelineKeyVolumeLightData2.sunPosition, t);
                        updateInfo.color1 = LerpWithoutClamp(liveTimelineKeyVolumeLightData.color1, liveTimelineKeyVolumeLightData2.color1, t);
                        updateInfo.power = LerpWithoutClamp(liveTimelineKeyVolumeLightData.power, liveTimelineKeyVolumeLightData2.power, t);
                        updateInfo.blurRadius = LerpWithoutClamp(liveTimelineKeyVolumeLightData.blurRadius, liveTimelineKeyVolumeLightData2.blurRadius, t);
                    }
                    else
                    {
                        updateInfo.sunPosition = liveTimelineKeyVolumeLightData.sunPosition;
                        updateInfo.color1 = liveTimelineKeyVolumeLightData.color1;
                        updateInfo.power = liveTimelineKeyVolumeLightData.power;
                        updateInfo.blurRadius = liveTimelineKeyVolumeLightData.blurRadius;
                    }
                    updateInfo.enable = liveTimelineKeyVolumeLightData.enable;
                    updateInfo.komorebi = liveTimelineKeyVolumeLightData.komorebi;
                    updateInfo.isEnabledBorderClear = liveTimelineKeyVolumeLightData.isEnabledBorderClear;
                    updateInfo.data = liveTimelineVolumeLightData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyVolumeLightData.frame) * num;
                    this.OnUpdateVolumeLight(ref updateInfo);
                }
            }
        }

        /// <summary>
        /// HdrBloom用AlterUpdate
        /// </summary>
        private void AlterUpdate_HdrBloom(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateHdrBloom == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            float num = 0.0166666675f;
            HdrBloomUpdateInfo updateInfo = default(HdrBloomUpdateInfo);
            for (int i = 0; i < sheet.HdrBloomKeys.Count; i++)
            {
                LiveTimelineHdrBloomData liveTimelineHdrBloomData = sheet.HdrBloomKeys[i];
                if (liveTimelineHdrBloomData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineHdrBloomData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyHdrBloomData liveTimelineKeyHdrBloomData = curKey as LiveTimelineKeyHdrBloomData;
                    LiveTimelineKeyHdrBloomData liveTimelineKeyHdrBloomData2 = nextKey as LiveTimelineKeyHdrBloomData;
                    if (liveTimelineKeyHdrBloomData2 != null && liveTimelineKeyHdrBloomData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyHdrBloomData, liveTimelineKeyHdrBloomData2, currentFrame);
                        updateInfo.intensity = LerpWithoutClamp(liveTimelineKeyHdrBloomData.intensity, liveTimelineKeyHdrBloomData2.intensity, t);
                        updateInfo.blurSpread = LerpWithoutClamp(liveTimelineKeyHdrBloomData.blurSpread, liveTimelineKeyHdrBloomData2.blurSpread, t);
                    }
                    else
                    {
                        updateInfo.intensity = liveTimelineKeyHdrBloomData.intensity;
                        updateInfo.blurSpread = liveTimelineKeyHdrBloomData.blurSpread;
                    }
                    updateInfo.enable = liveTimelineKeyHdrBloomData.enable;
                    updateInfo.data = liveTimelineHdrBloomData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyHdrBloomData.frame) * num;
                    this.OnUpdateHdrBloom(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_MonitorControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateMonitorControl == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            MonitorControlUpdateInfo updateInfo = default(MonitorControlUpdateInfo);
            for (int i = 0; i < sheet.monitorControlList.Count; i++)
            {
                LiveTimelineMonitorControlData liveTimelineMonitorControlData = sheet.monitorControlList[i];
                if (liveTimelineMonitorControlData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineMonitorControlData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineMonitorControlData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyMonitorControlData liveTimelineKeyMonitorControlData = curKey as LiveTimelineKeyMonitorControlData;
                    LiveTimelineKeyMonitorControlData liveTimelineKeyMonitorControlData2 = nextKey as LiveTimelineKeyMonitorControlData;
                    if (liveTimelineKeyMonitorControlData2 != null && liveTimelineKeyMonitorControlData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyMonitorControlData, liveTimelineKeyMonitorControlData2, currentFrame);
                        updateInfo.pos = LerpWithoutClamp(liveTimelineKeyMonitorControlData.position, liveTimelineKeyMonitorControlData2.position, t);
                        updateInfo.size = LerpWithoutClamp(liveTimelineKeyMonitorControlData.size, liveTimelineKeyMonitorControlData2.size, t);
                        updateInfo.speed = LerpWithoutClamp(liveTimelineKeyMonitorControlData.speed, liveTimelineKeyMonitorControlData2.speed, t);
                        updateInfo.blendFactor = LerpWithoutClamp(liveTimelineKeyMonitorControlData.blendFactor, liveTimelineKeyMonitorControlData2.blendFactor, t);
                        updateInfo.colorPower = LerpWithoutClamp(liveTimelineKeyMonitorControlData.colorPower, liveTimelineKeyMonitorControlData2.colorPower, t);
                    }
                    else
                    {
                        updateInfo.pos = liveTimelineKeyMonitorControlData.position;
                        updateInfo.size = liveTimelineKeyMonitorControlData.size;
                        updateInfo.speed = liveTimelineKeyMonitorControlData.speed;
                        updateInfo.blendFactor = liveTimelineKeyMonitorControlData.blendFactor;
                        updateInfo.colorPower = liveTimelineKeyMonitorControlData.colorPower;
                    }
                    updateInfo.multiFlag = liveTimelineKeyMonitorControlData.attribute.hasFlag((LiveTimelineKeyAttribute)65536);
                    updateInfo.dispID = liveTimelineKeyMonitorControlData.dispID;
                    updateInfo.subDispID = liveTimelineKeyMonitorControlData.subDispID;
                    updateInfo.reversePlayFlag = liveTimelineKeyMonitorControlData.attribute.hasFlag((LiveTimelineKeyAttribute)131072);
                    updateInfo.inputSource = liveTimelineKeyMonitorControlData.inputSource;
                    updateInfo.data = liveTimelineMonitorControlData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyMonitorControlData.frame) * 0.0166666675f;
                    updateInfo.textureLabel = liveTimelineKeyMonitorControlData.outputTextureLabel;
                    updateInfo.playStartOffsetFrame = liveTimelineKeyMonitorControlData.playStartOffsetFrame;
                    this.OnUpdateMonitorControl(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_AnimationControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateAnimation == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            AnimationUpdateInfo updateInfo = default(AnimationUpdateInfo);
            for (int i = 0; i < sheet.animationList.Count; i++)
            {
                LiveTimelineAnimationData liveTimelineAnimationData = sheet.animationList[i];
                if (liveTimelineAnimationData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineAnimationData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineAnimationData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyAnimationData liveTimelineKeyAnimationData = curKey as LiveTimelineKeyAnimationData;
                    LiveTimelineKeyAnimationData liveTimelineKeyAnimationData2 = nextKey as LiveTimelineKeyAnimationData;
                    if (liveTimelineKeyAnimationData2 != null && liveTimelineKeyAnimationData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyAnimationData, liveTimelineKeyAnimationData2, currentFrame);
                        updateInfo.speed = LerpWithoutClamp(liveTimelineKeyAnimationData.speed, liveTimelineKeyAnimationData2.speed, t);
                    }
                    else
                    {
                        updateInfo.speed = liveTimelineKeyAnimationData.speed;
                    }
                    updateInfo.animationID = liveTimelineKeyAnimationData.animationID;
                    updateInfo.wrapMode = liveTimelineKeyAnimationData.wrapMode;
                    updateInfo.speed = liveTimelineKeyAnimationData.speed;
                    updateInfo.offsetTime = liveTimelineKeyAnimationData.offsetTime;
                    updateInfo.data = liveTimelineAnimationData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyAnimationData.frame) * 0.0166666675f;
                    this.OnUpdateAnimation(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_TextureAnimationControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateTextureAnimation == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            TextureAnimationUpdateInfo updateInfo = default(TextureAnimationUpdateInfo);
            for (int i = 0; i < sheet.textureAnimationList.Count; i++)
            {
                LiveTimelineTextureAnimationData liveTimelineTextureAnimationData = sheet.textureAnimationList[i];
                if (liveTimelineTextureAnimationData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineTextureAnimationData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineTextureAnimationData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyTextureAnimationData liveTimelineKeyTextureAnimationData = curKey as LiveTimelineKeyTextureAnimationData;
                    LiveTimelineKeyTextureAnimationData liveTimelineKeyTextureAnimationData2 = nextKey as LiveTimelineKeyTextureAnimationData;
                    if (liveTimelineKeyTextureAnimationData2 != null && liveTimelineKeyTextureAnimationData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyTextureAnimationData, liveTimelineKeyTextureAnimationData2, currentFrame);
                        updateInfo.scrollSpeed = LerpWithoutClamp(liveTimelineKeyTextureAnimationData.scrollSpeed, liveTimelineKeyTextureAnimationData2.scrollSpeed, t);
                        updateInfo.scrollInterval = LerpWithoutClamp(liveTimelineKeyTextureAnimationData.scrollInterval, liveTimelineKeyTextureAnimationData2.scrollInterval, t);
                    }
                    else
                    {
                        updateInfo.scrollSpeed = liveTimelineKeyTextureAnimationData.scrollSpeed;
                        updateInfo.scrollInterval = liveTimelineKeyTextureAnimationData.scrollInterval;
                    }
                    updateInfo.textureName = liveTimelineKeyTextureAnimationData.textureName;
                    updateInfo.textureNameEmpty = liveTimelineKeyTextureAnimationData.textureNameEmpty;
                    updateInfo.offset = liveTimelineKeyTextureAnimationData.offset;
                    updateInfo.tiling = liveTimelineKeyTextureAnimationData.tiling;
                    updateInfo.textureID = liveTimelineKeyTextureAnimationData.textureID;
                    updateInfo.data = liveTimelineTextureAnimationData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyTextureAnimationData.frame) * 0.0166666675f;
                    this.OnUpdateTextureAnimation(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_TransformControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateTransform == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            TransformUpdateInfo updateInfo = default(TransformUpdateInfo);
            for (int i = 0; i < sheet.transformList.Count; i++)
            {
                LiveTimelineTransformData liveTimelineTransformData = sheet.transformList[i];
                if (liveTimelineTransformData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineTransformData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineTransformData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyTransformData liveTimelineKeyTransformData = curKey as LiveTimelineKeyTransformData;
                    LiveTimelineKeyTransformData liveTimelineKeyTransformData2 = nextKey as LiveTimelineKeyTransformData;
                    Vector3 euler;
                    if (liveTimelineKeyTransformData2 != null && liveTimelineKeyTransformData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyTransformData, liveTimelineKeyTransformData2, currentFrame);
                        updateInfo.updateData.position = LerpWithoutClamp(liveTimelineKeyTransformData.position, liveTimelineKeyTransformData2.position, t);
                        updateInfo.updateData.scale = LerpWithoutClamp(liveTimelineKeyTransformData.scale, liveTimelineKeyTransformData2.scale, t);
                        euler = LerpWithoutClamp(liveTimelineKeyTransformData.rotate, liveTimelineKeyTransformData2.rotate, t);
                    }
                    else
                    {
                        updateInfo.updateData.position = liveTimelineKeyTransformData.position;
                        updateInfo.updateData.scale = liveTimelineKeyTransformData.scale;
                        euler = liveTimelineKeyTransformData.rotate;
                    }
                    updateInfo.updateData.rotation = Quaternion.Euler(euler);
                    updateInfo.data = liveTimelineTransformData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyTransformData.frame) * 0.0166666675f;
                    this.OnUpdateTransform(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_RendererControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateRenderer == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            RendererUpdateInfo updateInfo = default(RendererUpdateInfo);
            for (int i = 0; i < sheet.rendererList.Count; i++)
            {
                LiveTimelineRendererData liveTimelineRendererData = sheet.rendererList[i];
                if (!liveTimelineRendererData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && liveTimelineRendererData.keys.EnablePlayModeTimeline(_playMode))
                {
                    FindTimelineKey(out curKey, out nextKey, liveTimelineRendererData.keys, currentFrame);
                    if (curKey != null)
                    {
                        LiveTimelineKeyRendererData liveTimelineKeyRendererData = curKey as LiveTimelineKeyRendererData;
                        updateInfo.renderEnable = liveTimelineKeyRendererData.renderEnable;
                        updateInfo.data = liveTimelineRendererData;
                        updateInfo.progressTime = (currentFrame - liveTimelineKeyRendererData.frame) * 0.0166666675f;
                        this.OnUpdateRenderer(ref updateInfo);
                    }
                }
            }
        }

        private void AlterUpdate_ObjectControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateObject == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            ObjectUpdateInfo updateInfo = default(ObjectUpdateInfo);
            for (int i = 0; i < sheet.objectList.Count; i++)
            {
                LiveTimelineObjectData liveTimelineObjectData = sheet.objectList[i];
                if (liveTimelineObjectData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineObjectData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineObjectData.keys, currentFrame);
                if (curKey == null)
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineObjectData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyObjectData liveTimelineKeyObjectData = curKey as LiveTimelineKeyObjectData;
                    LiveTimelineKeyObjectData liveTimelineKeyObjectData2 = nextKey as LiveTimelineKeyObjectData;
                    Vector3 euler;
                    if (liveTimelineKeyObjectData2 != null && liveTimelineKeyObjectData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyObjectData, liveTimelineKeyObjectData2, currentFrame);
                        updateInfo.updateData.position = LerpWithoutClamp(liveTimelineKeyObjectData.position, liveTimelineKeyObjectData2.position, t);
                        updateInfo.updateData.scale = LerpWithoutClamp(liveTimelineKeyObjectData.scale, liveTimelineKeyObjectData2.scale, t);
                        euler = LerpWithoutClamp(liveTimelineKeyObjectData.rotate, liveTimelineKeyObjectData2.rotate, t);
                        updateInfo.color = LerpWithoutClamp(liveTimelineKeyObjectData.color, liveTimelineKeyObjectData2.color, t);
                    }
                    else
                    {
                        updateInfo.updateData.position = liveTimelineKeyObjectData.position;
                        updateInfo.updateData.scale = liveTimelineKeyObjectData.scale;
                        updateInfo.colorEnable = liveTimelineKeyObjectData.colorEnable;
                        euler = liveTimelineKeyObjectData.rotate;
                        updateInfo.color = liveTimelineKeyObjectData.color;
                    }
                    updateInfo.renderEnable = liveTimelineKeyObjectData.renderEnable;
                    updateInfo.data = liveTimelineObjectData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyObjectData.frame) * 0.0166666675f;
                    updateInfo.updateData.rotation = Quaternion.Euler(euler);
                    updateInfo.reflectionEnable = liveTimelineKeyObjectData.reflectionEnable;
                    updateInfo.colorEnable = liveTimelineKeyObjectData.colorEnable;
                    this.OnUpdateObject(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_GazingObjectControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateGazingObject == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            GazingObjectUpdateInfo updateInfo = default(GazingObjectUpdateInfo);
            for (int i = 0; i < sheet.gazingObjectList.Count; i++)
            {
                LiveTimelineGazingObjectData liveTimelineGazingObjectData = sheet.gazingObjectList[i];
                if (liveTimelineGazingObjectData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineGazingObjectData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineGazingObjectData.keys, currentFrame);
                if (curKey == null)
                {
                    continue;
                }
                LiveTimelineKeyGazingObjectData liveTimelineKeyGazingObjectData = curKey as LiveTimelineKeyGazingObjectData;
                LiveTimelineKeyGazingObjectData liveTimelineKeyGazingObjectData2 = nextKey as LiveTimelineKeyGazingObjectData;
                if (liveTimelineKeyGazingObjectData.lookAtCharaFlags != 0)
                {
                    if (liveTimelineKeyGazingObjectData2 != null && liveTimelineKeyGazingObjectData2.interpolateType != 0 && liveTimelineKeyGazingObjectData2.lookAtCharaFlags != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyGazingObjectData, liveTimelineKeyGazingObjectData2, currentFrame);
                        updateInfo.lookAtPos = Vector3.LerpUnclamped(GetPositionWithCharacters(liveTimelineKeyGazingObjectData.lookAtCharaFlags, liveTimelineKeyGazingObjectData.lookAtCharaParts, Vector3.zero), GetPositionWithCharacters(liveTimelineKeyGazingObjectData2.lookAtCharaFlags, liveTimelineKeyGazingObjectData2.lookAtCharaParts, Vector3.zero), t) + Vector3.LerpUnclamped(liveTimelineKeyGazingObjectData.lookAtOffset, liveTimelineKeyGazingObjectData2.lookAtOffset, t);
                    }
                    else
                    {
                        updateInfo.lookAtPos = GetPositionWithCharacters(liveTimelineKeyGazingObjectData.lookAtCharaFlags, liveTimelineKeyGazingObjectData.lookAtCharaParts, Vector3.zero) + liveTimelineKeyGazingObjectData.lookAtOffset;
                    }
                    updateInfo.data = liveTimelineGazingObjectData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyGazingObjectData.frame) * 0.0166666675f;
                    this.OnUpdateGazingObject(ref updateInfo);
                }
            }
        }

        private void SetupCharaHeightMotionParameter(LiveTimelineKeyCharaHeightMotionData curData, LiveCharaPosition charaPosition, float blendRate, float targetBlendRate, LiveTimelineMotionSequence motionSequence, out CharaHeightMotionUpdateInfo updateInfo)
        {
            float characterHeight = GetCharacterHeight(charaPosition);
            float num;
            switch (curData.blendType)
            {
                default:
                    num = (characterHeight - kCameraLayerOffsetMinCharaHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                    updateInfo.offsetRate.x = (characterHeight - kCameraLayerOffsetMinCharaHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                    updateInfo.offsetRate.y = 0f;
                    updateInfo.offsetRate.z = 0f;
                    updateInfo.offsetRateExtend = null;
                    updateInfo.offsetPositionEffectorExtend = null;
                    updateInfo.extendNum = 0;
                    break;
                case LiveTimelineKeyCharaHeightMotionData.BlendType.Self:
                    num = (characterHeight - kCameraLayerOffsetMinCharaHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                    updateInfo.offsetRate = Vector3.zero;
                    updateInfo.offsetRateExtend = null;
                    updateInfo.offsetPositionEffectorExtend = null;
                    updateInfo.extendNum = 0;
                    break;
                case LiveTimelineKeyCharaHeightMotionData.BlendType.Target:
                    {
                        num = (GetCharacterHeight(curData.targetPosition) - characterHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                        float x = (characterHeight - kCameraLayerOffsetMinCharaHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                        updateInfo.offsetRate.x = x;
                        updateInfo.offsetRate.y = 0f;
                        updateInfo.offsetRate.z = 0f;
                        if (num < 0f)
                        {
                            num = 0f;
                        }
                        updateInfo.offsetRateExtend = null;
                        updateInfo.offsetPositionEffectorExtend = null;
                        updateInfo.extendNum = 0;
                        break;
                    }
                case LiveTimelineKeyCharaHeightMotionData.BlendType.TargetParts:
                    {
                        float characterHeight2 = GetCharacterHeight(curData.targetPosition);
                        float characterHeight3 = GetCharacterHeight(curData.target2Position);
                        characterHeight2 = Mathf.Max(0f, (characterHeight2 - characterHeight) / kCameraLayerOffsetBaseDiff) * blendRate;
                        characterHeight3 = Mathf.Max(0f, (characterHeight3 - characterHeight) / kCameraLayerOffsetBaseDiff) * blendRate;
                        num = Mathf.Lerp(characterHeight2, characterHeight3, targetBlendRate);
                        updateInfo.offsetRate = Vector3.zero;
                        updateInfo.offsetRateExtend = _heightMotionOffsetRate;
                        updateInfo.offsetPositionEffectorExtend = _heightMotionOffsetTargetPart;
                        updateInfo.extendNum = 0;
                        if (curData.partsMotion == null)
                        {
                            break;
                        }
                        int num2 = curData.partsMotion.Length;
                        if (num2 > 4)
                        {
                            num2 = 4;
                        }
                        updateInfo.extendNum = num2;
                        for (int i = 0; i < num2; i++)
                        {
                            float num3 = (characterHeight - kCameraLayerOffsetMinCharaHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                            float characterHeight4 = GetCharacterHeight(curData.partsMotion[i].targetPosition);
                            _heightMotionOffsetRate[i].y = num3;
                            float rate;
                            if (characterHeight4 > characterHeight)
                            {
                                _heightMotionOffsetRate[i].x = 0f;
                                _heightMotionOffsetRate[i].z = 0f;
                                rate = (characterHeight4 - characterHeight) / kCameraLayerOffsetBaseDiff * blendRate;
                            }
                            else
                            {
                                _heightMotionOffsetRate[i].x = num3;
                                _heightMotionOffsetRate[i].z = (characterHeight - characterHeight4) / kCameraLayerOffsetBaseDiff * blendRate;
                                rate = 0f;
                            }
                            _heightMotionOffsetTargetPart[i] = curData.partsMotion[i].positionOffset;
                            motionSequence.SetHeightMotionBlend(i + 1, rate);
                        }
                        break;
                    }
            }
            motionSequence.SetHeightMotionKey(curData);
            motionSequence.SetHeightMotionBlend(0, num);
            updateInfo.selfPosition = charaPosition;
            updateInfo.blendType = curData.blendType;
            updateInfo.offsetPositionEffector = curData.positionOffset;
        }

        private void AlterUpdate_CharaHeightMotion(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            for (int i = 0; i < sheet.charaHeightMotList.Count; i++)
            {
                LiveTimelineCharaHeightMotSeqData liveTimelineCharaHeightMotSeqData = sheet.charaHeightMotList[i];
                int charaPosition = (int)liveTimelineCharaHeightMotSeqData.charaPosition;
                LiveTimelineMotionSequence liveTimelineMotionSequence = _motionSequenceArray[charaPosition];
                if (liveTimelineMotionSequence == null)
                {
                    continue;
                }
                if (liveTimelineCharaHeightMotSeqData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    liveTimelineMotionSequence.SetHeightMotionKey(null);
                    continue;
                }
                if (!liveTimelineCharaHeightMotSeqData.keys.EnablePlayModeTimeline(_playMode))
                {
                    liveTimelineMotionSequence.SetHeightMotionKey(null);
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineCharaHeightMotSeqData.keys, currentFrame);
                if (curKey == null)
                {
                    liveTimelineMotionSequence.SetHeightMotionKey(null);
                    continue;
                }
                LiveTimelineKeyCharaHeightMotionData liveTimelineKeyCharaHeightMotionData = curKey as LiveTimelineKeyCharaHeightMotionData;
                LiveTimelineKeyCharaHeightMotionData liveTimelineKeyCharaHeightMotionData2 = nextKey as LiveTimelineKeyCharaHeightMotionData;
                float num = curKey.frame * 0.0166666675f;
                if (currentFrame * 0.0166666675f > num + liveTimelineKeyCharaHeightMotionData.playLength)
                {
                    liveTimelineMotionSequence.SetHeightMotionKey(null);
                    continue;
                }
                float blendRate;
                float targetBlendRate;
                if (liveTimelineKeyCharaHeightMotionData2 != null && liveTimelineKeyCharaHeightMotionData2.interpolate)
                {
                    float t = LinearInterpolateKeyframes(liveTimelineKeyCharaHeightMotionData, liveTimelineKeyCharaHeightMotionData2, currentFrame);
                    blendRate = LerpWithoutClamp(liveTimelineKeyCharaHeightMotionData.blendRate, liveTimelineKeyCharaHeightMotionData2.blendRate, t);
                    targetBlendRate = LerpWithoutClamp(liveTimelineKeyCharaHeightMotionData.targetBlendRate, liveTimelineKeyCharaHeightMotionData2.targetBlendRate, t);
                }
                else
                {
                    blendRate = liveTimelineKeyCharaHeightMotionData.blendRate;
                    targetBlendRate = liveTimelineKeyCharaHeightMotionData.targetBlendRate;
                }
                SetupCharaHeightMotionParameter(liveTimelineKeyCharaHeightMotionData, liveTimelineCharaHeightMotSeqData.charaPosition, blendRate, targetBlendRate, liveTimelineMotionSequence, out var updateInfo);
                this.OnCharaHeightMotionUpdateInfo(ref updateInfo);
            }
        }

        private void AlterUpdate_PropsControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateProps == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            PropsUpdateInfo updateInfo = default(PropsUpdateInfo);
            updateInfo.currentTime = currentFrame * 0.0166666675f;
            for (int i = 0; i < sheet.propsList.Count; i++)
            {
                LiveTimelinePropsData liveTimelinePropsData = sheet.propsList[i];
                if (liveTimelinePropsData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelinePropsData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelinePropsData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyPropsData liveTimelineKeyPropsData = curKey as LiveTimelineKeyPropsData;
                    LiveTimelineKeyPropsData liveTimelineKeyPropsData2 = nextKey as LiveTimelineKeyPropsData;
                    if (liveTimelineKeyPropsData2 != null && liveTimelineKeyPropsData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyPropsData, liveTimelineKeyPropsData2, currentFrame);
                        updateInfo.color = LerpWithoutClamp(liveTimelineKeyPropsData.color, liveTimelineKeyPropsData2.color, t);
                        updateInfo.colorPower = LerpWithoutClamp(liveTimelineKeyPropsData.colorPower, liveTimelineKeyPropsData2.colorPower, t);
                        updateInfo.rootColor = LerpWithoutClamp(liveTimelineKeyPropsData.rootColor, liveTimelineKeyPropsData2.rootColor, t);
                        updateInfo.tipColor = LerpWithoutClamp(liveTimelineKeyPropsData.tipColor, liveTimelineKeyPropsData2.tipColor, t);
                        updateInfo.lightPosition = LerpWithoutClamp(liveTimelineKeyPropsData.lightPos, liveTimelineKeyPropsData2.lightPos, t);
                        updateInfo.specularPower = LerpWithoutClamp(liveTimelineKeyPropsData.specularPower, liveTimelineKeyPropsData2.specularPower, t);
                        updateInfo.specularColor = LerpWithoutClamp(liveTimelineKeyPropsData.specularColor, liveTimelineKeyPropsData2.specularColor, t);
                        updateInfo.luminousColor = LerpWithoutClamp(liveTimelineKeyPropsData.luminousColor, liveTimelineKeyPropsData2.luminousColor, t);
                    }
                    else
                    {
                        updateInfo.color = liveTimelineKeyPropsData.color;
                        updateInfo.colorPower = liveTimelineKeyPropsData.colorPower;
                        updateInfo.rootColor = liveTimelineKeyPropsData.rootColor;
                        updateInfo.tipColor = liveTimelineKeyPropsData.tipColor;
                        updateInfo.lightPosition = liveTimelineKeyPropsData.lightPos;
                        updateInfo.specularPower = liveTimelineKeyPropsData.specularPower;
                        updateInfo.specularColor = liveTimelineKeyPropsData.specularColor;
                        updateInfo.luminousColor = liveTimelineKeyPropsData.luminousColor;
                    }
                    updateInfo.data = liveTimelinePropsData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyPropsData.frame) * 0.0166666675f;
                    updateInfo.renderEnable = liveTimelineKeyPropsData.rendererEnable;
                    updateInfo.settingFlags = liveTimelineKeyPropsData.settingFlags;
                    updateInfo.propsID = liveTimelineKeyPropsData.propsID;
                    updateInfo.drawAfterImage = liveTimelineKeyPropsData.IsDrawAfterImage();
                    updateInfo.useLocalAxis = liveTimelineKeyPropsData.IsUseLocalAxis();
                    this.OnUpdateProps(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_PropsAttachControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdatePropsAttach == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            PropsAttachUpdateInfo updateInfo = default(PropsAttachUpdateInfo);
            for (int i = 0; i < sheet.propsAttachList.Count; i++)
            {
                LiveTimelinePropsAttachData liveTimelinePropsAttachData = sheet.propsAttachList[i];
                if (liveTimelinePropsAttachData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelinePropsAttachData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelinePropsAttachData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyPropsAttachData liveTimelineKeyPropsAttachData = curKey as LiveTimelineKeyPropsAttachData;
                    LiveTimelineKeyPropsAttachData liveTimelineKeyPropsAttachData2 = nextKey as LiveTimelineKeyPropsAttachData;
                    if (liveTimelineKeyPropsAttachData2 != null && liveTimelineKeyPropsAttachData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyPropsAttachData, liveTimelineKeyPropsAttachData2, currentFrame);
                        updateInfo.offsetPosition = LerpWithoutClamp(liveTimelineKeyPropsAttachData.offsetPosition, liveTimelineKeyPropsAttachData2.offsetPosition, t);
                        updateInfo.rotation = LerpWithoutClamp(liveTimelineKeyPropsAttachData.rotation, liveTimelineKeyPropsAttachData2.rotation, t);
                    }
                    else
                    {
                        updateInfo.offsetPosition = liveTimelineKeyPropsAttachData.offsetPosition;
                        updateInfo.rotation = liveTimelineKeyPropsAttachData.rotation;
                    }
                    updateInfo.data = liveTimelinePropsAttachData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyPropsAttachData.frame) * 0.0166666675f;
                    updateInfo.settingFlags = liveTimelineKeyPropsAttachData.settingFlags;
                    updateInfo.attachJointHash = liveTimelineKeyPropsAttachData.attachJointHash;
                    updateInfo.propsID = liveTimelineKeyPropsAttachData.propsID;
                    updateInfo.animationId = liveTimelineKeyPropsAttachData.animationId;
                    updateInfo.animationSpeed = liveTimelineKeyPropsAttachData.animationSpeed;
                    updateInfo.animationOffset = liveTimelineKeyPropsAttachData.animationOffset;
                    updateInfo.changeAnimation = false;
                    if (liveTimelinePropsAttachData.updatedKeyFrame != liveTimelineKeyPropsAttachData.frame && liveTimelineKeyPropsAttachData.isAnimationChange)
                    {
                        updateInfo.changeAnimation = true;
                    }
                    liveTimelinePropsAttachData.updatedKeyFrame = liveTimelineKeyPropsAttachData.frame;
                    this.OnUpdatePropsAttach(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_CameraSwitcher(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (sheet.cameraSwitcherKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.cameraSwitcherKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            FindTimelineKeyCurrent(out curKey, sheet.cameraSwitcherKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyCameraSwitcherData liveTimelineKeyCameraSwitcherData = curKey as LiveTimelineKeyCameraSwitcherData;
            if (this.OnUpdateCameraSwitcher != null)
            {
                this.OnUpdateCameraSwitcher(liveTimelineKeyCameraSwitcherData.cameraIndex);
            }
            else
            {
                if (liveTimelineKeyCameraSwitcherData.cameraIndex >= cameraArray.Length)
                {
                    return;
                }
                for (int i = 0; i < cameraArray.Length; i++)
                {
                    if (cameraArray[i] == null)
                    {
                        continue;
                    }
                    if (i == liveTimelineKeyCameraSwitcherData.cameraIndex)
                    {
                        if (!cameraArray[i].camera.enabled)
                        {
                            cameraArray[i].camera.enabled = true;
                            cameraScriptArray[i].enabled = true;
                        }
                    }
                    else if (cameraArray[i].camera.enabled)
                    {
                        cameraArray[i].camera.enabled = false;
                        cameraScriptArray[i].enabled = false;
                    }
                }
            }
        }

        private void AlterUpdate_LipSync(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (!sheet.ripSyncKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && sheet.ripSyncKeys.EnablePlayModeTimeline(_playMode) && this.OnUpdateLipSync != null)
            {
                LiveTimelineKey curKey = null;
                FindTimelineKeyCurrent(out curKey, sheet.ripSyncKeys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyRipSyncData arg = curKey as LiveTimelineKeyRipSyncData;
                    this.OnUpdateLipSync(arg, currentLiveTime);
                }
            }
        }

        /// <summary>
        /// PostEffect用AlterUpdate
        /// </summary>
        private void AlterUpdate_PostEffect(LiveTimelineWorkSheet sheet, int currentFrame, Vector3 cameraLookAt)
        {
            if (sheet.postEffectKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.postEffectKeys.EnablePlayModeTimeline(_playMode) || this.OnUpdatePostEffect == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.postEffectKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            PostEffectUpdateInfo updateInfo = default(PostEffectUpdateInfo);
            LiveTimelineKeyPostEffectData liveTimelineKeyPostEffectData = curKey as LiveTimelineKeyPostEffectData;
            LiveTimelineKeyPostEffectData liveTimelineKeyPostEffectData2 = nextKey as LiveTimelineKeyPostEffectData;
            updateInfo.dofBlurType = liveTimelineKeyPostEffectData.dofBlurType;
            updateInfo.dofQuality = liveTimelineKeyPostEffectData.dofQuality;
            updateInfo.isUseFocalPoint = liveTimelineKeyPostEffectData.IsUseFocalPoint();
            if (liveTimelineKeyPostEffectData2 != null && liveTimelineKeyPostEffectData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyPostEffectData, liveTimelineKeyPostEffectData2, currentFrame);
                updateInfo.forcalSize = LerpWithoutClamp(liveTimelineKeyPostEffectData.forcalSize, liveTimelineKeyPostEffectData2.forcalSize, t);
                updateInfo.blurSpread = LerpWithoutClamp(liveTimelineKeyPostEffectData.blurSpread, liveTimelineKeyPostEffectData2.blurSpread, t);
                updateInfo.bloomDofWeight = LerpWithoutClamp(liveTimelineKeyPostEffectData.bloomDofWeight, liveTimelineKeyPostEffectData2.bloomDofWeight, t);
                updateInfo.threshold = LerpWithoutClamp(liveTimelineKeyPostEffectData.threshold, liveTimelineKeyPostEffectData2.threshold, t);
                updateInfo.intensity = LerpWithoutClamp(liveTimelineKeyPostEffectData.intensity, liveTimelineKeyPostEffectData2.intensity, t);
                if (liveTimelineKeyPostEffectData.IsUseLookAt() && liveTimelineKeyPostEffectData2.IsUseLookAt())
                {
                    updateInfo.forcalPosition = cameraLookAt;
                }
                else
                {
                    updateInfo.forcalPosition = LerpWithoutClamp(liveTimelineKeyPostEffectData.IsUseLookAt() ? cameraLookAt : GetPositionWithCharacters(liveTimelineKeyPostEffectData.charactor, LiveCameraLookAtCharaParts.Face), liveTimelineKeyPostEffectData2.IsUseLookAt() ? cameraLookAt : GetPositionWithCharacters(liveTimelineKeyPostEffectData2.charactor, LiveCameraLookAtCharaParts.Face), t);
                }
                updateInfo.dofForegroundSize = LerpWithoutClamp(liveTimelineKeyPostEffectData.dofForegroundSize, liveTimelineKeyPostEffectData2.dofForegroundSize, t);
                updateInfo.dofFocalPoint = LerpWithoutClamp(liveTimelineKeyPostEffectData.dofFocalPoint, liveTimelineKeyPostEffectData2.dofFocalPoint, t);
                updateInfo.filterIntensity = LerpWithoutClamp(liveTimelineKeyPostEffectData.dofMVFilterIntensity, liveTimelineKeyPostEffectData2.dofMVFilterIntensity, t);
                float num = LerpWithoutClamp(liveTimelineKeyPostEffectData.dofMVFilterSpd, liveTimelineKeyPostEffectData2.dofMVFilterSpd, t);
                updateInfo.filterTime = (currentFrame - liveTimelineKeyPostEffectData.frame) * 0.0166666675f * num;
            }
            else
            {
                updateInfo.forcalSize = liveTimelineKeyPostEffectData.forcalSize;
                updateInfo.blurSpread = liveTimelineKeyPostEffectData.blurSpread;
                updateInfo.bloomDofWeight = liveTimelineKeyPostEffectData.bloomDofWeight;
                updateInfo.threshold = liveTimelineKeyPostEffectData.threshold;
                updateInfo.intensity = liveTimelineKeyPostEffectData.intensity;
                updateInfo.forcalPosition = (liveTimelineKeyPostEffectData.IsUseLookAt() ? cameraLookAt : GetPositionWithCharacters(liveTimelineKeyPostEffectData.charactor, LiveCameraLookAtCharaParts.Face));
                updateInfo.dofForegroundSize = liveTimelineKeyPostEffectData.dofForegroundSize;
                updateInfo.dofFocalPoint = liveTimelineKeyPostEffectData.dofFocalPoint;
                updateInfo.dofMVFilterType = liveTimelineKeyPostEffectData.dofMVFilterType;
                updateInfo.filterResId = liveTimelineKeyPostEffectData.dofMVFilterResId;
                updateInfo.filterIntensity = liveTimelineKeyPostEffectData.dofMVFilterIntensity;
                updateInfo.filterTime = (currentFrame - liveTimelineKeyPostEffectData.frame) * 0.0166666675f * liveTimelineKeyPostEffectData.dofMVFilterSpd;
                updateInfo.disableDOFBlur = liveTimelineKeyPostEffectData.IsDisableDOFBlur();
            }
            this.OnUpdatePostEffect(ref updateInfo);
        }

        /// <summary>
        /// PostFilm用AlterUpdate
        /// </summary>
        private void AlterUpdate_PostFilm(LiveTimelineKeyPostFilmDataList postFilmKeys, int currentFrame, ScreenOverlayUpdateInfoDelegate fnUpdatePostFilm)
        {
            if (fnUpdatePostFilm == null || postFilmKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !postFilmKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, postFilmKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            PostFilmUpdateInfo updateInfo = default(PostFilmUpdateInfo);
            LiveTimelineKeyPostFilmData liveTimelineKeyPostFilmData = curKey as LiveTimelineKeyPostFilmData;
            LiveTimelineKeyPostFilmData liveTimelineKeyPostFilmData2 = nextKey as LiveTimelineKeyPostFilmData;
            if (liveTimelineKeyPostFilmData2 != null && liveTimelineKeyPostFilmData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyPostFilmData, liveTimelineKeyPostFilmData2, currentFrame);
                updateInfo.filmPower = LerpWithoutClamp(liveTimelineKeyPostFilmData.filmPower, liveTimelineKeyPostFilmData2.filmPower, t);
                updateInfo.filmOffsetParam = LerpWithoutClamp(liveTimelineKeyPostFilmData.filmOffsetParam, liveTimelineKeyPostFilmData2.filmOffsetParam, t);
                updateInfo.filmOptionParam = LerpWithoutClamp(liveTimelineKeyPostFilmData.filmOptionParam, liveTimelineKeyPostFilmData2.filmOptionParam, t);
                switch (liveTimelineKeyPostFilmData.colorType)
                {
                    case PostColorType.ColorAll:
                        updateInfo.color0 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color0, liveTimelineKeyPostFilmData2.color0, t);
                        updateInfo.color1 = (updateInfo.color2 = (updateInfo.color3 = updateInfo.color0));
                        break;
                    case PostColorType.Color2TopBottom:
                        updateInfo.color0 = (updateInfo.color1 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color0, liveTimelineKeyPostFilmData2.color0, t));
                        updateInfo.color2 = (updateInfo.color3 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color1, liveTimelineKeyPostFilmData2.color1, t));
                        break;
                    case PostColorType.Color2LeftRight:
                        updateInfo.color0 = (updateInfo.color2 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color0, liveTimelineKeyPostFilmData2.color0, t));
                        updateInfo.color1 = (updateInfo.color3 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color1, liveTimelineKeyPostFilmData2.color1, t));
                        break;
                    case PostColorType.Color4:
                        updateInfo.color0 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color0, liveTimelineKeyPostFilmData2.color0, t);
                        updateInfo.color1 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color1, liveTimelineKeyPostFilmData2.color1, t);
                        updateInfo.color2 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color2, liveTimelineKeyPostFilmData2.color2, t);
                        updateInfo.color3 = LerpWithoutClamp(liveTimelineKeyPostFilmData.color3, liveTimelineKeyPostFilmData2.color3, t);
                        break;
                }
                updateInfo.colorBlendFactor = LerpWithoutClamp(liveTimelineKeyPostFilmData.colorBlendFactor, liveTimelineKeyPostFilmData2.colorBlendFactor, t);
                float num = LerpWithoutClamp(liveTimelineKeyPostFilmData.movieSpeed, liveTimelineKeyPostFilmData2.movieSpeed, t);
                updateInfo.movieTime = (currentFrame - liveTimelineKeyPostFilmData.frame) * 0.0166666675f * num;
                updateInfo.screenCircleDir = LerpWithoutClamp(liveTimelineKeyPostFilmData.screenCircleDir, liveTimelineKeyPostFilmData2.screenCircleDir, t);
            }
            else
            {
                updateInfo.filmPower = liveTimelineKeyPostFilmData.filmPower;
                updateInfo.filmOffsetParam = liveTimelineKeyPostFilmData.filmOffsetParam;
                updateInfo.filmOptionParam = liveTimelineKeyPostFilmData.filmOptionParam;
                switch (liveTimelineKeyPostFilmData.colorType)
                {
                    case PostColorType.ColorAll:
                        updateInfo.color0 = liveTimelineKeyPostFilmData.color0;
                        updateInfo.color1 = (updateInfo.color2 = (updateInfo.color3 = updateInfo.color0));
                        break;
                    case PostColorType.Color2TopBottom:
                        updateInfo.color0 = (updateInfo.color1 = liveTimelineKeyPostFilmData.color0);
                        updateInfo.color2 = (updateInfo.color3 = liveTimelineKeyPostFilmData.color1);
                        break;
                    case PostColorType.Color2LeftRight:
                        updateInfo.color0 = (updateInfo.color2 = liveTimelineKeyPostFilmData.color0);
                        updateInfo.color1 = (updateInfo.color3 = liveTimelineKeyPostFilmData.color1);
                        break;
                    case PostColorType.Color4:
                        updateInfo.color0 = liveTimelineKeyPostFilmData.color0;
                        updateInfo.color1 = liveTimelineKeyPostFilmData.color1;
                        updateInfo.color2 = liveTimelineKeyPostFilmData.color2;
                        updateInfo.color3 = liveTimelineKeyPostFilmData.color3;
                        break;
                }
                updateInfo.colorBlendFactor = liveTimelineKeyPostFilmData.colorBlendFactor;
                updateInfo.movieTime = (currentFrame - liveTimelineKeyPostFilmData.frame) * 0.0166666675f * liveTimelineKeyPostFilmData.movieSpeed;
                updateInfo.screenCircleDir = liveTimelineKeyPostFilmData.screenCircleDir;
            }
            updateInfo.inverseVignette = liveTimelineKeyPostFilmData.isInverseVignette();
            updateInfo.screenCircle = liveTimelineKeyPostFilmData.isScreenCircle;
            updateInfo.layerMode = liveTimelineKeyPostFilmData.layerMode;
            updateInfo.movieResId = liveTimelineKeyPostFilmData.movieResId;
            updateInfo.movieFrameOffset = liveTimelineKeyPostFilmData.movieFrameOffset;
            updateInfo.movieReverse = liveTimelineKeyPostFilmData.isReverseUVMovie();
            updateInfo.colorBlend = liveTimelineKeyPostFilmData.colorBlend;
            updateInfo.filmMode = liveTimelineKeyPostFilmData.filmMode;
            updateInfo.colorType = liveTimelineKeyPostFilmData.colorType;
            fnUpdatePostFilm(ref updateInfo);
        }

        /// <summary>
        /// タイムラインにてスクリーンフェードを管理する
        /// 実行するかを管理し、実際の処理はAlterUpdate_ScreenFadeMainにて行う
        /// </summary>
        private void AlterUpdate_ScreenFade(LiveTimelineKeyScreenFadeDataList keys, int currentFrame)
        {
            if (this.OnUpdateScreenFade != null && keys.Count != 0 && !keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && keys.EnablePlayModeTimeline(_playMode))
            {
                ScreenFadeUpdateInfo updateInfo = default(ScreenFadeUpdateInfo);
                AlterUpdate_ScreenFadeMain(keys, currentFrame, ref updateInfo);
                this.OnUpdateScreenFade(ref updateInfo);
            }
        }

        /// <summary>
        /// OnUpdateScreenFadeに送るScreenFadeUpdateInfoの値を生成する
        /// </summary>
        private void AlterUpdate_ScreenFadeMain(LiveTimelineKeyScreenFadeDataList keys, int currentFrame, ref ScreenFadeUpdateInfo updateInfo)
        {
            updateInfo.enable = false;
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
            if (curKey != null)
            {
                LiveTimelineKeyScreenFadeData liveTimelineKeyScreenFadeData = curKey as LiveTimelineKeyScreenFadeData;
                LiveTimelineKeyScreenFadeData liveTimelineKeyScreenFadeData2 = nextKey as LiveTimelineKeyScreenFadeData;
                if (liveTimelineKeyScreenFadeData2 != null && liveTimelineKeyScreenFadeData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyScreenFadeData, liveTimelineKeyScreenFadeData2, currentFrame);
                    updateInfo.color = Color.LerpUnclamped(liveTimelineKeyScreenFadeData.color, liveTimelineKeyScreenFadeData2.color, t);
                }
                else
                {
                    updateInfo.color = liveTimelineKeyScreenFadeData.color;
                }
                updateInfo.enable = updateInfo.color.a > 0f;
                updateInfo.onlyQuality3DLight = liveTimelineKeyScreenFadeData.onlyQuality3DLight;
            }
        }

        private void AlterUpdate_CameraLayer(LiveTimelineWorkSheet sheet, int currentFrame, ref Vector3 layerOffset, ref Vector3 extraLayerOffset)
        {
            if (sheet.cameraLayerKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.cameraLayerKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.cameraLayerKeys, currentFrame);
            if (curKey == null)
            {
                layerOffset = Vector3.zero;
                return;
            }
            LiveTimelineKeyCameraLayerData liveTimelineKeyCameraLayerData = curKey as LiveTimelineKeyCameraLayerData;
            LiveTimelineKeyCameraLayerData liveTimelineKeyCameraLayerData2 = nextKey as LiveTimelineKeyCameraLayerData;
            if (liveTimelineKeyCameraLayerData2 != null && liveTimelineKeyCameraLayerData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyCameraLayerData, liveTimelineKeyCameraLayerData2, currentFrame);
                layerOffset = LerpWithoutClamp(liveTimelineKeyCameraLayerData.layerOffset, liveTimelineKeyCameraLayerData2.layerOffset, t);
                _isExtraCameraLayer = liveTimelineKeyCameraLayerData.extraCamera;
                if (liveTimelineKeyCameraLayerData.extraCamera)
                {
                    extraLayerOffset = LerpWithoutClamp(liveTimelineKeyCameraLayerData.extraCameraLayerOffset, liveTimelineKeyCameraLayerData2.extraCameraLayerOffset, t);
                }
            }
            else
            {
                layerOffset = liveTimelineKeyCameraLayerData.layerOffset;
                _isExtraCameraLayer = liveTimelineKeyCameraLayerData.extraCamera;
                if (liveTimelineKeyCameraLayerData.extraCamera)
                {
                    extraLayerOffset = liveTimelineKeyCameraLayerData.extraCameraLayerOffset;
                }
            }
        }

        private void AlterUpdate_Projector(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateProjector == null)
            {
                return;
            }
            if (sheet.projecterList.Count > 0 && (_projectorWorks == null || _projectorWorks.Length < sheet.projecterList.Count))
            {
                _projectorWorks = new ProjectorWork[sheet.projecterList.Count];
                for (int i = 0; i < _projectorWorks.Length; i++)
                {
                    _projectorWorks[i] = new ProjectorWork();
                }
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            ProjectorUpdateInfo updateInfo = default(ProjectorUpdateInfo);
            for (int j = 0; j < sheet.projecterList.Count; j++)
            {
                LiveTimelineProjectorData liveTimelineProjectorData = sheet.projecterList[j];
                if (liveTimelineProjectorData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineProjectorData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineProjectorData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                liveTimelineKey2 = ((liveTimelineKey == null) ? null : liveTimelineProjectorData.keys.At(findKeyResult.index + 1));
                if (liveTimelineKey == null)
                {
                    continue;
                }
                LiveTimelineKeyProjectorData liveTimelineKeyProjectorData = liveTimelineKey as LiveTimelineKeyProjectorData;
                LiveTimelineKeyProjectorData liveTimelineKeyProjectorData2 = liveTimelineKey2 as LiveTimelineKeyProjectorData;
                if (liveTimelineKeyProjectorData2 != null && liveTimelineKeyProjectorData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyProjectorData, liveTimelineKeyProjectorData2, currentFrame);
                    updateInfo.motionID = liveTimelineKeyProjectorData.motionID;
                    updateInfo.materialID = liveTimelineKeyProjectorData.materialID;
                    updateInfo.speed = LerpWithoutClamp(liveTimelineKeyProjectorData.speed, liveTimelineKeyProjectorData2.speed, t);
                    updateInfo.color1 = LerpWithoutClamp(liveTimelineKeyProjectorData.color1, liveTimelineKeyProjectorData2.color1, t);
                    updateInfo.power = LerpWithoutClamp(liveTimelineKeyProjectorData.power, liveTimelineKeyProjectorData2.power, t);
                    updateInfo.position = LerpWithoutClamp(liveTimelineKeyProjectorData.position, liveTimelineKeyProjectorData2.position, t);
                    updateInfo.rotate = LerpWithoutClamp(liveTimelineKeyProjectorData.rotate, liveTimelineKeyProjectorData2.rotate, t);
                    updateInfo.rotateXZ = LerpWithoutClamp(liveTimelineKeyProjectorData.rotateXZ, liveTimelineKeyProjectorData2.rotateXZ, t);
                    updateInfo.size = LerpWithoutClamp(liveTimelineKeyProjectorData.size, liveTimelineKeyProjectorData2.size, t);
                }
                else
                {
                    updateInfo.motionID = liveTimelineKeyProjectorData.motionID;
                    updateInfo.materialID = liveTimelineKeyProjectorData.materialID;
                    updateInfo.speed = liveTimelineKeyProjectorData.speed;
                    updateInfo.color1 = liveTimelineKeyProjectorData.color1;
                    updateInfo.power = liveTimelineKeyProjectorData.power;
                    updateInfo.position = liveTimelineKeyProjectorData.position;
                    updateInfo.rotate = liveTimelineKeyProjectorData.rotate;
                    updateInfo.rotateXZ = liveTimelineKeyProjectorData.rotateXZ;
                    updateInfo.size = liveTimelineKeyProjectorData.size;
                }
                updateInfo.data = liveTimelineProjectorData;
                ProjectorWork projectorWork = _projectorWorks[j];
                float num = (currentFrame - liveTimelineKeyProjectorData.frame) * 0.0166666675f;
                if (liveTimelineKeyProjectorData.IsAnimContinuous())
                {
                    if (liveTimelineKeyProjectorData.frame != projectorWork.startFrame || projectorWork.lineHash != liveTimelineProjectorData.nameHash)
                    {
                        projectorWork.lineHash = liveTimelineProjectorData.nameHash;
                        int frame = liveTimelineKeyProjectorData.frame;
                        for (int num2 = findKeyResult.index - 1; num2 >= 0; num2--)
                        {
                            LiveTimelineKey liveTimelineKey3 = liveTimelineProjectorData.keys[num2];
                            frame = liveTimelineKey3.frame;
                            if (!(liveTimelineKey3 as LiveTimelineKeyProjectorData).IsAnimContinuous())
                            {
                                break;
                            }
                        }
                        projectorWork.progressTime = (liveTimelineKeyProjectorData.frame - frame) * 0.0166666675f;
                    }
                    num += projectorWork.progressTime;
                }
                projectorWork.startFrame = liveTimelineKeyProjectorData.frame;
                updateInfo.progressTime = num;
                this.OnUpdateProjector(ref updateInfo);
            }
        }

        private void AlterUpdate_CharaWind(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnCharaWindUpdate == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            CharaWindUpdateInfo updateInfo = default(CharaWindUpdateInfo);
            for (int i = 0; i < sheet.charaWindList.Count; i++)
            {
                LiveTimelineCharaWindData liveTimelineCharaWindData = sheet.charaWindList[i];
                if (liveTimelineCharaWindData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineCharaWindData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineCharaWindData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyCharaWindData liveTimelineKeyCharaWindData = curKey as LiveTimelineKeyCharaWindData;
                    LiveTimelineKeyCharaWindData liveTimelineKeyCharaWindData2 = nextKey as LiveTimelineKeyCharaWindData;
                    Vector3 cySpringForceScale;
                    Vector3 windPower;
                    if (liveTimelineKeyCharaWindData2 != null && liveTimelineKeyCharaWindData2.IsInterpolateKey())
                    {
                        float t = LinearInterpolateKeyframes(liveTimelineKeyCharaWindData, liveTimelineKeyCharaWindData2, currentFrame);
                        cySpringForceScale = LerpWithoutClamp(liveTimelineKeyCharaWindData.cySpringForceScale, liveTimelineKeyCharaWindData2.cySpringForceScale, t);
                        windPower = LerpWithoutClamp(liveTimelineKeyCharaWindData.windPower, liveTimelineKeyCharaWindData2.windPower, t);
                    }
                    else
                    {
                        cySpringForceScale = liveTimelineKeyCharaWindData.cySpringForceScale;
                        windPower = liveTimelineKeyCharaWindData.windPower;
                    }
                    updateInfo.windMode = liveTimelineKeyCharaWindData.windMode;
                    updateInfo.loopTime = liveTimelineKeyCharaWindData.loopTime;
                    updateInfo.enable = liveTimelineKeyCharaWindData.enable;
                    updateInfo.selfPosition = liveTimelineCharaWindData.charaPosition;
                    updateInfo.cySpringForceScale = cySpringForceScale;
                    updateInfo.windPower = windPower;
                    this.OnCharaWindUpdate(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_FacialData(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateFacial == null)
            {
                return;
            }
            FacialDataUpdateInfo updateInfo = default(FacialDataUpdateInfo);
            if (sheet.facial1Set != null)
            {
                SetupFacialUpdateInfo_Face(ref updateInfo, sheet.facial1Set.faceKeys, currentFrame);
                SetupFacialUpdateInfo_Mouth(ref updateInfo, sheet.facial1Set.mouthKeys, currentFrame);
                SetupFacialUpdateInfo_Eye(ref updateInfo, sheet.facial1Set.eyeKeys, currentFrame);
                SetupFacialUpdateInfo_EyeTrack(ref updateInfo, sheet.facial1Set.eyeTrackKeys, currentFrame);
                this.OnUpdateFacial(updateInfo, currentLiveTime, LiveCharaPosition.Center);
            }
            if (sheet.other4EyeTrackKeys != null)
            {
                SetupFacialUpdateInfo_EyeTrack(ref updateInfo, sheet.other4EyeTrackKeys, currentFrame);
            }
            bool flag = false;
            LiveTimelineKeyFacialNoiseData liveTimelineKeyFacialNoiseData = sheet.facialNoiseKeys.FindKeyCached(currentFrame, availableFindKeyCache).key as LiveTimelineKeyFacialNoiseData;
            if (liveTimelineKeyFacialNoiseData != null)
            {
                flag = liveTimelineKeyFacialNoiseData.IsNoiseDisable();
            }
            for (int i = 1; i < liveCharaPositionMax; i++)
            {
                bool flag2 = false;
                int num = currentFrame;
                if (i <= sheet.other4FacialArray.Length)
                {
                    LiveTimelineOther4FacialData liveTimelineOther4FacialData = sheet.other4FacialArray[i - 1];
                    num = (flag ? currentFrame : _facialNoiseFrameArray[i - 1]);
                    SetupFacialUpdateInfo_Face(ref updateInfo, liveTimelineOther4FacialData.faceKeys, num);
                    SetupFacialUpdateInfo_Mouth(ref updateInfo, liveTimelineOther4FacialData.mouthKeys, num);
                    SetupFacialUpdateInfo_Eye(ref updateInfo, liveTimelineOther4FacialData.eyeKeys, num);
                    updateInfo.eyeTrack = null;
                    LiveTimelineKey curKey = null;
                    LiveTimelineKey nextKey = null;
                    FindTimelineKey(out curKey, out nextKey, liveTimelineOther4FacialData.eyeTrackKeys, num);
                    if (curKey != null && !(curKey as LiveTimelineKeyFacialEyeTrackData).IsNop())
                    {
                        SetupFacialUpdateInfo_EyeTrack(ref updateInfo, liveTimelineOther4FacialData.eyeTrackKeys, num);
                        flag2 = true;
                    }
                }
                if (!flag2 && sheet.other4EyeTrackKeys != null)
                {
                    LiveTimelineKey curKey2 = null;
                    LiveTimelineKey nextKey2 = null;
                    FindTimelineKey(out curKey2, out nextKey2, sheet.other4EyeTrackKeys, num);
                    if (curKey2 != null)
                    {
                        SetupFacialUpdateInfo_EyeTrack(ref updateInfo, sheet.other4EyeTrackKeys, num);
                    }
                }
                float facialNoiseCurrentTime = GetFacialNoiseCurrentTime(i - 1);
                this.OnUpdateFacial(updateInfo, facialNoiseCurrentTime, (LiveCharaPosition)i);
            }
        }

        private void SetupFacialUpdateInfo_Face(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialFaceDataList keys, int frame)
        {
            LiveTimelineKey liveTimelineKey = null;
            if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
            {
                updateInfo.face = null;
                return;
            }
            FindKeyResult findKeyResult = keys.FindKeyCached(frame, availableFindKeyCache);
            liveTimelineKey = findKeyResult.key;
            updateInfo.face = liveTimelineKey as LiveTimelineKeyFacialFaceData;
            updateInfo.faceKeyIndex = findKeyResult.index;
        }

        private void SetupFacialUpdateInfo_Mouth(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialFaceMouthList keys, int frame)
        {
            LiveTimelineKey liveTimelineKey = null;
            if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
            {
                updateInfo.mouth = null;
                return;
            }
            FindKeyResult findKeyResult = keys.FindKeyCached(frame, availableFindKeyCache);
            liveTimelineKey = findKeyResult.key;
            updateInfo.mouth = liveTimelineKey as LiveTimelineKeyFacialMouthData;
            updateInfo.mouthKeyIndex = findKeyResult.index;
        }

        private void SetupFacialUpdateInfo_Eye(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialEyeDataList keys, int frame)
        {
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
            {
                updateInfo.eyeCur = null;
                updateInfo.eyeNext = null;
                return;
            }
            FindKeyResult findKeyResult = keys.FindKeyCached(frame, availableFindKeyCache);
            liveTimelineKey = findKeyResult.key;
            liveTimelineKey2 = keys.At(findKeyResult.index + 1);
            updateInfo.eyeCur = liveTimelineKey as LiveTimelineKeyFacialEyeData;
            updateInfo.eyeNext = liveTimelineKey2 as LiveTimelineKeyFacialEyeData;
            updateInfo.eyeKeyIndex = findKeyResult.index;
        }

        private void SetupFacialUpdateInfo_EyeTrack(ref FacialDataUpdateInfo updateInfo, LiveTimelineKeyFacialEyeTrackDataList keys, int frame)
        {
            LiveTimelineKeyFacialEyeTrackData liveTimelineKeyFacialEyeTrackData = null;
            LiveTimelineKeyFacialEyeTrackData liveTimelineKeyFacialEyeTrackData2 = null;
            if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
            {
                updateInfo.eyeTrack = null;
                updateInfo.eyeTrackOffset = Vector3.zero;
                return;
            }
            keys.FindKeyCached(frame, availableFindKeyCache, out var current, out var next);
            liveTimelineKeyFacialEyeTrackData = current.key as LiveTimelineKeyFacialEyeTrackData;
            liveTimelineKeyFacialEyeTrackData2 = next.key as LiveTimelineKeyFacialEyeTrackData;
            if (liveTimelineKeyFacialEyeTrackData2 != null && liveTimelineKeyFacialEyeTrackData2.IsInterpolateKey())
            {
                float t = CalculateInterpolationValue(liveTimelineKeyFacialEyeTrackData, liveTimelineKeyFacialEyeTrackData2, frame);
                if (liveTimelineKeyFacialEyeTrackData != null)
                {
                    updateInfo.eyeTrackOffset = LerpWithoutClamp(liveTimelineKeyFacialEyeTrackData.position, liveTimelineKeyFacialEyeTrackData2.position, t);
                }
            }
            else if (liveTimelineKeyFacialEyeTrackData != null)
            {
                updateInfo.eyeTrackOffset = liveTimelineKeyFacialEyeTrackData.position;
            }
            updateInfo.eyeTrack = liveTimelineKeyFacialEyeTrackData;
            updateInfo.eyeTrackKeyIndex = current.index;
        }

        private void InitFormationOffsetWork()
        {
            for (int i = 0; i < _formationOffsetStartFrameArray.Length; i++)
            {
                _formationOffsetStartFrameArray[i] = -1;
            }
        }

        private void AlterLateUpdate_FormationOffset(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (data.characterSettings.isCharaAxisRotateLateUpdate)
            {
                ILiveTimelineKeyDataList[] keyListArray = sheet.formationOffsetSet.GetKeyListArray();
                for (int i = 0; i < keyListArray.Length; i++)
                {
                    LateUpdateFormationOffset_Transform(i, currentFrame);
                }
                for (int j = 0; j < sheet.formationOffsetAdditionalList.Count; j++)
                {
                    LateUpdateFormationOffset_Transform(keyListArray.Length + j, currentFrame);
                }
            }
        }

        private bool UpdateFormationOffset_CharacterVisible(LiveTimelineKeyFormationOffsetData curData, int iList)
        {
            bool visible = curData.visible;
            bool flag = false;
            if (_liveCharactorLocators[iList] != null)
            {
                flag = _liveCharactorLocators[iList].liveCharaVisible != visible;
                _liveCharactorLocators[iList].liveCharaVisible = visible;
            }
            if ((_formationOffsetStartFrameArray[iList] != curData.frame || flag) && this.OnUpdateFormationOffset != null)
            {
                FormationOffsetUpdateInfo updateInfo = default(FormationOffsetUpdateInfo);
                updateInfo.characterPosition = iList;
                updateInfo.isVisible = visible;
                updateInfo.isEffectClear = curData.IsEffectClear();
                this.OnUpdateFormationOffset(ref updateInfo);
            }
            if (!visible)
            {
                return false;
            }
            return true;
        }

        private void LateUpdateFormationOffset_Transform(int iList, int currentFrame)
        {
            int num = iList * 2;
            if (_formationOffsetLateUpdateKey[num] != null)
            {
                LiveTimelineKeyFormationOffsetData curData = _formationOffsetLateUpdateKey[num];
                LiveTimelineKeyFormationOffsetData nextData = _formationOffsetLateUpdateKey[num + 1];
                UpdateFormationOffset_Transform(iList, curData, nextData, currentFrame);
                _formationOffsetLateUpdateKey[num] = null;
                _formationOffsetLateUpdateKey[num + 1] = null;
            }
        }

        private void UpdateFormationOffset_Transform(int iList, LiveTimelineKeyFormationOffsetData curData, LiveTimelineKeyFormationOffsetData nextData, int currentFrame)
        {
            Vector2 zero = Vector2.zero;
            float num = 0f;
            float num2 = 0f;
            Vector3 zero2 = Vector3.zero;
            bool isWorldSpace = curData.isWorldSpace;
            bool isLookAtWorldOrigin = curData.isLookAtWorldOrigin;
            Vector3 vector = Vector3.zero;
            float y = 0f;
            bool useLateRotate = curData.useLateRotate;
            Vector3 lateCharaRotate = curData.lateCharaRotate;
            if (nextData != null && nextData.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(curData, nextData, currentFrame);
                zero = LerpWithoutClamp(curData.posXZ, nextData.posXZ, t);
                num = LerpWithoutClamp(curData.posY, nextData.posY, t);
                num2 = LerpWithoutClamp(curData.rotY, nextData.rotY, t);
                zero2 = LerpWithoutClamp(curData.charaAxisRotate, nextData.charaAxisRotate, t);
                lateCharaRotate = LerpWithoutClamp(curData.lateCharaRotate, nextData.lateCharaRotate, t);
                if (isWorldSpace)
                {
                    vector = LerpWithoutClamp(curData.worldSpaceOrigin, nextData.worldSpaceOrigin, t);
                    y = LerpWithoutClamp(curData.worldRotationY, nextData.worldRotationY, t);
                }
            }
            else
            {
                zero = curData.posXZ;
                num = curData.posY;
                num2 = curData.rotY;
                zero2 = curData.charaAxisRotate;
                lateCharaRotate = curData.lateCharaRotate;
                if (isWorldSpace)
                {
                    vector = curData.worldSpaceOrigin;
                    y = curData.worldRotationY;
                }
            }
            Vector3 vector2 = new Vector3(zero.x, num, zero.y);
            Quaternion quaternion = Quaternion.AngleAxis(num2, Vector3.up);
            ILiveTimelineCharactorLocator liveTimelineCharactorLocator = _liveCharactorLocators[iList];
            if (liveTimelineCharactorLocator != null && liveTimelineCharactorLocator.liveRootTransform != null)
            {
                if (liveTimelineCharactorLocator.liveParentTransform == null)
                {
                    Vector3 vector3;
                    if (!useLateRotate)
                    {
                        if (zero2.x != 0f || zero2.y != 0f || zero2.z != 0f)
                        {
                            liveTimelineCharactorLocator.liveRootTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, zero2.z));
                            Vector3 reverseOffsetPosition = GetReverseOffsetPosition(liveTimelineCharactorLocator.liveCharaWaistPosition, liveTimelineCharactorLocator.liveRootTransform.position, zero2.z, Vector3.back);
                            liveTimelineCharactorLocator.liveRootTransform.position = liveTimelineCharactorLocator.liveCharaInitialPosition + vector2 + reverseOffsetPosition;
                            liveTimelineCharactorLocator.liveRootTransform.localRotation = Quaternion.Euler(new Vector3(zero2.x, 0f, zero2.z));
                            Vector3 reverseOffsetPosition2 = GetReverseOffsetPosition(liveTimelineCharactorLocator.liveCharaWaistPosition, liveTimelineCharactorLocator.liveRootTransform.position, zero2.x, Vector3.left);
                            liveTimelineCharactorLocator.liveRootTransform.position += reverseOffsetPosition2;
                            liveTimelineCharactorLocator.liveRootTransform.localRotation = Quaternion.Euler(new Vector3(zero2.x, zero2.y, zero2.z));
                            Vector3 reverseOffsetPosition3 = GetReverseOffsetPosition(liveTimelineCharactorLocator.liveCharaWaistPosition, liveTimelineCharactorLocator.liveRootTransform.position, zero2.y, Vector3.down);
                            liveTimelineCharactorLocator.liveRootTransform.position += reverseOffsetPosition3;
                            vector3 = liveTimelineCharactorLocator.liveRootTransform.position;
                            quaternion = liveTimelineCharactorLocator.liveRootTransform.localRotation;
                        }
                        else
                        {
                            vector3 = liveTimelineCharactorLocator.liveCharaInitialPosition + vector2;
                        }
                        if (isWorldSpace)
                        {
                            Vector3 vector4 = vector3 - vector;
                            vector3 = vector + Quaternion.Euler(0f, y, 0f) * vector4;
                            if (isLookAtWorldOrigin)
                            {
                                quaternion = Quaternion.LookRotation(vector - vector3);
                            }
                        }
                    }
                    else
                    {
                        vector3 = liveTimelineCharactorLocator.liveCharaInitialPosition + vector2;
                        liveTimelineCharactorLocator.liveRootTransform.localRotation = Quaternion.Euler(lateCharaRotate);
                        quaternion = liveTimelineCharactorLocator.liveRootTransform.localRotation;
                    }
                    liveTimelineCharactorLocator.liveRootTransform.position = vector3;
                    liveTimelineCharactorLocator.liveRootTransform.localRotation = quaternion;
                }
                else
                {
                    Matrix4x4 localToWorldMatrix = liveTimelineCharactorLocator.liveParentTransform.localToWorldMatrix;
                    Vector3 vector5 = localToWorldMatrix.MultiplyPoint3x4(liveTimelineCharactorLocator.liveCharaInitialPosition + vector2);
                    quaternion = localToWorldMatrix.rotation * quaternion;
                    if (!useLateRotate)
                    {
                        if (isWorldSpace)
                        {
                            Vector3 vector6 = vector5 - vector;
                            vector5 = vector + Quaternion.Euler(0f, y, 0f) * vector6;
                            if (isLookAtWorldOrigin)
                            {
                                quaternion = Quaternion.LookRotation(vector - vector5);
                            }
                        }
                    }
                    else
                    {
                        liveTimelineCharactorLocator.liveRootTransform.localRotation = Quaternion.Euler(lateCharaRotate);
                        quaternion = liveTimelineCharactorLocator.liveRootTransform.localRotation;
                    }
                    liveTimelineCharactorLocator.liveRootTransform.position = vector5;
                    liveTimelineCharactorLocator.liveRootTransform.rotation = quaternion;
                }
            }
            _formationOffsetStartFrameArray[iList] = curData.frame;
        }

        private void UpdateFormationOffset(ILiveTimelineKeyDataList keys, int iList, int currentFrame)
        {
            if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode) || keys.Count <= 0)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyFormationOffsetData liveTimelineKeyFormationOffsetData = curKey as LiveTimelineKeyFormationOffsetData;
            if (!UpdateFormationOffset_CharacterVisible(liveTimelineKeyFormationOffsetData, iList))
            {
                _formationOffsetStartFrameArray[iList] = liveTimelineKeyFormationOffsetData.frame;
                return;
            }
            int num = iList * 2;
            LiveTimelineKeyFormationOffsetData liveTimelineKeyFormationOffsetData2 = nextKey as LiveTimelineKeyFormationOffsetData;
            if (data.characterSettings.isCharaAxisRotateLateUpdate)
            {
                _formationOffsetLateUpdateKey[num] = liveTimelineKeyFormationOffsetData;
                _formationOffsetLateUpdateKey[num + 1] = liveTimelineKeyFormationOffsetData2;
            }
            else
            {
                UpdateFormationOffset_Transform(iList, liveTimelineKeyFormationOffsetData, liveTimelineKeyFormationOffsetData2, currentFrame);
                _formationOffsetLateUpdateKey[num] = null;
                _formationOffsetLateUpdateKey[num + 1] = null;
            }
        }

        private void AlterUpdate_FormationOffset(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            ILiveTimelineKeyDataList[] keyListArray = sheet.formationOffsetSet.GetKeyListArray();
            for (int i = 0; i < keyListArray.Length; i++)
            {
                ILiveTimelineKeyDataList keys = keyListArray[i];
                UpdateFormationOffset(keys, i, currentFrame);
            }
            for (int j = 0; j < sheet.formationOffsetAdditionalList.Count; j++)
            {
                LiveTimelineKeyFormationOffsetDataList keys2 = sheet.formationOffsetAdditionalList[j];
                UpdateFormationOffset(keys2, keyListArray.Length + j, currentFrame);
            }
        }

        private Vector3 GetReverseOffsetPosition(Vector3 targetPosition, Vector3 parentPosition, float angle, Vector3 axis)
        {
            Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
            Vector3 vector = targetPosition - parentPosition;
            return parentPosition + (quaternion * vector - targetPosition);
        }

        private void AlterUpdate_Particle(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (data.particleControllMode != LiveTimelineData.ParticleControllMode.Timeline || this.OnUpdateParticle == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            ParticleUpdateInfo updateInfo = default(ParticleUpdateInfo);
            for (int i = 0; i < sheet.particleList.Count; i++)
            {
                LiveTimelineParticleData liveTimelineParticleData = sheet.particleList[i];
                if (liveTimelineParticleData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineParticleData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineParticleData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey != null)
                {
                    liveTimelineKey2 = liveTimelineParticleData.keys.At(findKeyResult.index + 1);
                    LiveTimelineKeyParticleData liveTimelineKeyParticleData = liveTimelineKey as LiveTimelineKeyParticleData;
                    LiveTimelineKeyParticleData liveTimelineKeyParticleData2 = liveTimelineKey2 as LiveTimelineKeyParticleData;
                    if (liveTimelineKeyParticleData2 != null && liveTimelineKeyParticleData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyParticleData, liveTimelineKeyParticleData2, currentFrame);
                        updateInfo.emissionRate = LerpWithoutClamp(liveTimelineKeyParticleData.emissionRate, liveTimelineKeyParticleData2.emissionRate, t);
                    }
                    else
                    {
                        updateInfo.emissionRate = liveTimelineKeyParticleData.emissionRate;
                    }
                    updateInfo.data = liveTimelineParticleData;
                    this.OnUpdateParticle(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_ParticleGroup(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateParticleGroup == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            ParticleGroupUpdateInfo updateInfo = default(ParticleGroupUpdateInfo);
            for (int i = 0; i < sheet.particleGroupList.Count; i++)
            {
                LiveTimelineParticleGroupData liveTimelineParticleGroupData = sheet.particleGroupList[i];
                if (liveTimelineParticleGroupData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineParticleGroupData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineParticleGroupData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey != null)
                {
                    liveTimelineKey2 = liveTimelineParticleGroupData.keys.At(findKeyResult.index + 1);
                    LiveTimelineKeyParticleGroupData liveTimelineKeyParticleGroupData = liveTimelineKey as LiveTimelineKeyParticleGroupData;
                    LiveTimelineKeyParticleGroupData liveTimelineKeyParticleGroupData2 = liveTimelineKey2 as LiveTimelineKeyParticleGroupData;
                    if (liveTimelineKeyParticleGroupData2 != null && liveTimelineKeyParticleGroupData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyParticleGroupData, liveTimelineKeyParticleGroupData2, currentFrame);
                        updateInfo.lerpColor = LerpWithoutClamp(liveTimelineKeyParticleGroupData.lerpColor, liveTimelineKeyParticleGroupData2.lerpColor, t);
                        updateInfo.lerpColorRate = LerpWithoutClamp(liveTimelineKeyParticleGroupData.colorLerpRate, liveTimelineKeyParticleGroupData2.colorLerpRate, t);
                    }
                    else
                    {
                        updateInfo.lerpColor = liveTimelineKeyParticleGroupData.lerpColor;
                        updateInfo.lerpColorRate = liveTimelineKeyParticleGroupData.colorLerpRate;
                    }
                    updateInfo.data = liveTimelineParticleGroupData;
                    this.OnUpdateParticleGroup(ref updateInfo);
                }
            }
        }

        /// <summary>
        /// レーザーのUpdateInfoを実装に渡す前処理
        /// </summary>
        public void AlterUpdate_Laser(LiveTimelineWorkSheet sheet, int currentFrame, int sheetIdx)
        {
            if (this.OnUpdateLaser == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = sheet.laserList.Count;
            LaserUpdateInfo updateInfo = default(LaserUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineLaserData liveTimelineLaserData = sheet.laserList[i];
                if (liveTimelineLaserData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineLaserData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineLaserData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey == null)
                {
                    continue;
                }
                liveTimelineKey2 = liveTimelineLaserData.keys.At(findKeyResult.index + 1);
                LiveTimelineKeyLaserData liveTimelineKeyLaserData = liveTimelineKey as LiveTimelineKeyLaserData;
                LiveTimelineKeyLaserData liveTimelineKeyLaserData2 = liveTimelineKey2 as LiveTimelineKeyLaserData;
                int num = liveTimelineKeyLaserData.joints.Length;
                if (_numLaserParts != num)
                {
                    _numLaserParts = num;
                    _arrLaserPosition = new Vector3[num];
                    _arrLaserRotation = new Vector3[num];
                    _arrLaserScale = new Vector3[num];
                }
                updateInfo.localPos = _arrLaserPosition;
                updateInfo.localRot = _arrLaserRotation;
                updateInfo.localScale = _arrLaserScale;
                if (liveTimelineKeyLaserData2 != null && liveTimelineKeyLaserData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyLaserData, liveTimelineKeyLaserData2, currentFrame);
                    updateInfo.degRootYaw = LerpWithoutClamp(liveTimelineKeyLaserData.degRootYaw, liveTimelineKeyLaserData2.degRootYaw, t);
                    updateInfo.degLaserPitch = LerpWithoutClamp(liveTimelineKeyLaserData.degLaserPitch, liveTimelineKeyLaserData2.degLaserPitch, t);
                    updateInfo.posInterval = LerpWithoutClamp(liveTimelineKeyLaserData.posInterval, liveTimelineKeyLaserData2.posInterval, t);
                    updateInfo.blinkPeroid = LerpWithoutClamp(liveTimelineKeyLaserData.blinkPeriod, liveTimelineKeyLaserData2.blinkPeriod, t);
                    updateInfo.rootPos = LerpWithoutClamp(liveTimelineKeyLaserData.rootTransform.localPos, liveTimelineKeyLaserData2.rootTransform.localPos, t);
                    updateInfo.rootRot = LerpWithoutClamp(liveTimelineKeyLaserData.rootTransform.localRot, liveTimelineKeyLaserData2.rootTransform.localRot, t);
                    updateInfo.rootScale = LerpWithoutClamp(liveTimelineKeyLaserData.rootTransform.localScale, liveTimelineKeyLaserData2.rootTransform.localScale, t);
                    for (int j = 0; j < num; j++)
                    {
                        updateInfo.localPos[j] = LerpWithoutClamp(liveTimelineKeyLaserData.joints[j].localPos, liveTimelineKeyLaserData2.joints[j].localPos, t);
                        updateInfo.localRot[j] = LerpWithoutClamp(liveTimelineKeyLaserData.joints[j].localRot, liveTimelineKeyLaserData2.joints[j].localRot, t);
                        updateInfo.localScale[j] = LerpWithoutClamp(liveTimelineKeyLaserData.joints[j].localScale, liveTimelineKeyLaserData2.joints[j].localScale, t);
                    }
                }
                else
                {
                    updateInfo.degRootYaw = liveTimelineKeyLaserData.degRootYaw;
                    updateInfo.degLaserPitch = liveTimelineKeyLaserData.degLaserPitch;
                    updateInfo.posInterval = liveTimelineKeyLaserData.posInterval;
                    updateInfo.blinkPeroid = liveTimelineKeyLaserData.blinkPeriod;
                    updateInfo.rootPos = liveTimelineKeyLaserData.rootTransform.localPos;
                    updateInfo.rootRot = liveTimelineKeyLaserData.rootTransform.localRot;
                    updateInfo.rootScale = liveTimelineKeyLaserData.rootTransform.localScale;
                    for (int k = 0; k < num; k++)
                    {
                        updateInfo.localPos[k] = liveTimelineKeyLaserData.joints[k].localPos;
                        updateInfo.localRot[k] = liveTimelineKeyLaserData.joints[k].localRot;
                        updateInfo.localScale[k] = liveTimelineKeyLaserData.joints[k].localScale;
                    }
                }
                updateInfo.data = liveTimelineLaserData;
                updateInfo.formation = liveTimelineKeyLaserData.formation;
                updateInfo.isBlink = liveTimelineKeyLaserData.IsBlink();
                this.OnUpdateLaser(ref updateInfo, _ignoreLaserHashDic[sheetIdx]);
            }
        }

        /// <summary>
        /// PostFilm用AlterUpdate
        /// </summary>
        private void AlterUpdate_Effect(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateEffect == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = sheet.effectList.Count;
            EffectUpdateInfo updateInfo = default(EffectUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineEffectData liveTimelineEffectData = sheet.effectList[i];
                if (liveTimelineEffectData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineEffectData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineEffectData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey != null)
                {
                    liveTimelineKey2 = liveTimelineEffectData.keys.At(findKeyResult.index + 1);
                    LiveTimelineKeyEffectData liveTimelineKeyEffectData = liveTimelineKey as LiveTimelineKeyEffectData;
                    LiveTimelineKeyEffectData liveTimelineKeyEffectData2 = liveTimelineKey2 as LiveTimelineKeyEffectData;
                    if (liveTimelineKeyEffectData2 != null && liveTimelineKeyEffectData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyEffectData, liveTimelineKeyEffectData2, currentFrame);
                        updateInfo.color = LerpWithoutClamp(liveTimelineKeyEffectData.color, liveTimelineKeyEffectData2.color, t);
                        updateInfo.colorPower = LerpWithoutClamp(liveTimelineKeyEffectData.colorPower, liveTimelineKeyEffectData2.colorPower, t);
                        updateInfo.offset = LerpWithoutClamp(liveTimelineKeyEffectData.offset, liveTimelineKeyEffectData2.offset, t);
                        updateInfo.offsetAngle = LerpWithoutClamp(liveTimelineKeyEffectData.offsetAngle, liveTimelineKeyEffectData2.offsetAngle, t);
                    }
                    else
                    {
                        updateInfo.color = liveTimelineKeyEffectData.color;
                        updateInfo.colorPower = liveTimelineKeyEffectData.colorPower;
                        updateInfo.offset = liveTimelineKeyEffectData.offset;
                        updateInfo.offsetAngle = liveTimelineKeyEffectData.offsetAngle;
                    }
                    if (liveTimelineKeyEffectData.owner == eEffectOwner.World)
                    {
                        updateInfo.isEnable = true;
                    }
                    else
                    {
                        updateInfo.isEnable = _liveCharactorLocators[eEffectOwnerUtil.ToCharaPosIndex(liveTimelineKeyEffectData.owner)].liveCharaVisible;
                    }
                    updateInfo.isClear = liveTimelineEffectData.updatedKeyFrame != liveTimelineKeyEffectData.frame && liveTimelineKeyEffectData.IsClear();
                    updateInfo.blendMode = liveTimelineKeyEffectData.blendMode;
                    updateInfo.isPlay = liveTimelineKeyEffectData.IsPlay();
                    updateInfo.isLoop = liveTimelineKeyEffectData.IsLoop();
                    updateInfo.isStayPRS = liveTimelineKeyEffectData.IsStayPRS();
                    updateInfo.owner = liveTimelineKeyEffectData.owner;
                    updateInfo.occurrenceSpot = liveTimelineKeyEffectData.occurrenceSpot;
                    updateInfo.data = liveTimelineEffectData;
                    this.OnUpdateEffect(ref updateInfo);
                    liveTimelineEffectData.updatedKeyFrame = liveTimelineKeyEffectData.frame;
                }
            }
        }

        public bool IsDrawDeltaTimeGraph()
        {
            return _drawDeltaTimeGraph;
        }

        public void SetDrawDeltaTimeGraph(bool draw)
        {
            _drawDeltaTimeGraph = draw;
        }

        public void EnqueueDeltaTimeForGraph(float dt)
        {
            _headIndexOfDeltaTimeArray = (_headIndexOfDeltaTimeArray + 1) % kDeltaTimeArrayLength;
            _deltaTimeArrayForGraph[_headIndexOfDeltaTimeArray] = dt;
        }

        private void UpdateDeltaTimeGraph()
        {
            graphWidth = Screen.width - graphXMargin * 2f;
            graphYMargin = Screen.height / 2 - graphHeight;
        }

        private void OnGUI()
        {
            if (!_drawDeltaTimeGraph)
            {
                return;
            }
            float num = graphWidth / kDeltaTimeArrayLength;
            Drawing.DrawLine(new Vector2(graphXMargin, graphYMargin - graphHeight), new Vector2(graphXMargin + graphWidth, graphYMargin - graphHeight), Color.blue, 1f, antiAlias: false);
            Drawing.DrawLine(new Vector2(graphXMargin, graphYMargin), new Vector2(graphXMargin + graphWidth, graphYMargin), Color.white, 1f, antiAlias: false);
            Drawing.DrawLine(new Vector2(graphXMargin, graphYMargin + graphHeight), new Vector2(graphXMargin + graphWidth, graphYMargin + graphHeight), Color.yellow, 1f, antiAlias: false);
            int num2 = _headIndexOfDeltaTimeArray;
            int num3 = 0;
            for (int i = 0; i < 299; i++)
            {
                num3 = num2 - 1;
                if (num3 < 0)
                {
                    num3 = 299;
                }
                float num4 = _deltaTimeArrayForGraph[num2];
                float num5 = _deltaTimeArrayForGraph[num3];
                float num6 = num4 / 0.0166666675f;
                float num7 = num5 / 0.0166666675f;
                Vector2 pointA = new Vector2(graphXMargin + graphWidth - num * i, graphYMargin - graphHeight * num6);
                Vector2 pointB = new Vector2(graphXMargin + graphWidth - num * (i + 1), graphYMargin - graphHeight * num7);
                Drawing.DrawLine(pointA, pointB, Color.green, 2f, antiAlias: false);
                num2 = num3;
            }
        }

        /// <summary>
        /// 環境光を更新する
        /// </summary>
        private void AlterUpdate_Environment(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            int count = sheet.environmentDataLists.Count;
            EnvironmentCharacterShadowUpdateInfo updateInfo = default(EnvironmentCharacterShadowUpdateInfo);
            EnvironmentMirrorUpdateInfo updateInfo2 = default(EnvironmentMirrorUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyStageEnvironmentDataList keys = sheet.environmentDataLists[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                LiveTimelineKey curKey = null;
                FindTimelineKeyCurrent(out curKey, keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyStageEnvironmentData liveTimelineKeyStageEnvironmentData = curKey as LiveTimelineKeyStageEnvironmentData;
                    if (this.OnEnvironmentCharacterShadow != null && (liveTimelineKeyStageEnvironmentData.characterShadow != _enviromentShadowCharacterPositionFlag || liveTimelineKeyStageEnvironmentData.softShadow != _enviromentSoftShadow))
                    {
                        _enviromentShadowCharacterPositionFlag = liveTimelineKeyStageEnvironmentData.characterShadow;
                        _enviromentSoftShadow = liveTimelineKeyStageEnvironmentData.softShadow;
                        updateInfo.positionFlag = liveTimelineKeyStageEnvironmentData.characterShadow;
                        updateInfo.softShadow = liveTimelineKeyStageEnvironmentData.softShadow;
                        this.OnEnvironmentCharacterShadow(ref updateInfo);
                    }
                    if (this.OnEnvironmentMirror != null)
                    {
                        updateInfo2.mirror = liveTimelineKeyStageEnvironmentData.mirror;
                        updateInfo2.mirrorReflectionRate = liveTimelineKeyStageEnvironmentData.mirrorReflectionRate;
                        this.OnEnvironmentMirror(ref updateInfo2);
                    }
                }
            }
        }

        /// <summary>
        /// GlobalLight用AlterUpdate
        /// </summary>
        private void AlterUpdate_GlobalLight(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnEnvironmentGlobalLight == null)
            {
                return;
            }
            int count = sheet.globalLightDataLists.Count;
            EnvironmentGlobalLightUpdateInfo updateInfo = default(EnvironmentGlobalLightUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyGlobalLightDataList keys = sheet.globalLightDataLists[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                LiveTimelineKey curKey = null;
                LiveTimelineKey nextKey = null;
                FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyGlobalLightData liveTimelineKeyGlobalLightData = curKey as LiveTimelineKeyGlobalLightData;
                    LiveTimelineKeyGlobalLightData liveTimelineKeyGlobalLightData2 = nextKey as LiveTimelineKeyGlobalLightData;
                    Quaternion quaternion = Quaternion.identity;
                    if (liveTimelineKeyGlobalLightData.cameraFollow)
                    {
                        quaternion = GetCamera(sheet.targetCameraIndex).cacheTransform.rotation;
                    }
                    Vector3 lightDirection;
                    float globalRimRate;
                    float globalRimShadowRate;
                    float globalRimSpecularRate;
                    float globalToonRate;
                    if (liveTimelineKeyGlobalLightData2 != null && liveTimelineKeyGlobalLightData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyGlobalLightData, liveTimelineKeyGlobalLightData2, currentFrame);
                        Quaternion a = Quaternion.Euler(liveTimelineKeyGlobalLightData.lightDir);
                        Quaternion b = Quaternion.Euler(liveTimelineKeyGlobalLightData2.lightDir);
                        lightDirection = Quaternion.Lerp(a, b, t) * quaternion * Vector3.forward;
                        globalRimRate = LerpWithoutClamp(liveTimelineKeyGlobalLightData.globalRimRate, liveTimelineKeyGlobalLightData2.globalRimRate, t);
                        globalRimShadowRate = LerpWithoutClamp(liveTimelineKeyGlobalLightData.globalRimShadowRate, liveTimelineKeyGlobalLightData2.globalRimShadowRate, t);
                        globalRimSpecularRate = LerpWithoutClamp(liveTimelineKeyGlobalLightData.globalRimSpecularRate, liveTimelineKeyGlobalLightData2.globalRimSpecularRate, t);
                        globalToonRate = LerpWithoutClamp(liveTimelineKeyGlobalLightData.globalToonRate, liveTimelineKeyGlobalLightData2.globalToonRate, t);
                    }
                    else
                    {
                        lightDirection = Quaternion.Euler(liveTimelineKeyGlobalLightData.lightDir) * quaternion * Vector3.forward;
                        globalRimRate = liveTimelineKeyGlobalLightData.globalRimRate;
                        globalRimShadowRate = liveTimelineKeyGlobalLightData.globalRimShadowRate;
                        globalRimSpecularRate = liveTimelineKeyGlobalLightData.globalRimSpecularRate;
                        globalToonRate = liveTimelineKeyGlobalLightData.globalToonRate;
                    }
                    updateInfo.lightDirection = lightDirection;
                    updateInfo.globalRimRate = globalRimRate;
                    updateInfo.globalRimShadowRate = globalRimShadowRate;
                    updateInfo.globalRimSpecularRate = globalRimSpecularRate;
                    updateInfo.globalToonRate = globalToonRate;
                    updateInfo.data = sheet.globalLightDataLists[i];
                    this.OnEnvironmentGlobalLight(ref updateInfo);
                }
            }
        }

        /// <summary>
        /// GlobalFog用AlterUpdate
        /// </summary>
        private void AlterUpdate_GlobalFog(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            int count = sheet.globalFogDataLists.Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyGlobalFogDataList keys = sheet.globalFogDataLists[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                LiveTimelineKey curKey = null;
                LiveTimelineKey nextKey = null;
                FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
                if (curKey == null)
                {
                    continue;
                }
                LiveTimelineKeyGlobalFogData liveTimelineKeyGlobalFogData = curKey as LiveTimelineKeyGlobalFogData;
                LiveTimelineKeyGlobalFogData liveTimelineKeyGlobalFogData2 = nextKey as LiveTimelineKeyGlobalFogData;
                if (this.OnGlobalFog != null)
                {
                    GlobalFogUpdateInfo updateInfo = default(GlobalFogUpdateInfo);
                    bool isDistance;
                    bool isHeight;
                    float startDistance;
                    float height;
                    float heightDensity;
                    Color color;
                    FogMode fogMode;
                    float expDensity;
                    float start;
                    float end;
                    bool useRadialDistance;
                    Vector4 heightOption;
                    Vector4 distanceOption;
                    if (liveTimelineKeyGlobalFogData2 != null && liveTimelineKeyGlobalFogData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyGlobalFogData, liveTimelineKeyGlobalFogData2, currentFrame);
                        isDistance = liveTimelineKeyGlobalFogData2.isDistance;
                        isHeight = liveTimelineKeyGlobalFogData2.isHeight;
                        startDistance = LerpWithoutClamp(liveTimelineKeyGlobalFogData.startDistance, liveTimelineKeyGlobalFogData2.startDistance, t);
                        height = LerpWithoutClamp(liveTimelineKeyGlobalFogData.height, liveTimelineKeyGlobalFogData2.height, t);
                        heightDensity = LerpWithoutClamp(liveTimelineKeyGlobalFogData.heightDensity, liveTimelineKeyGlobalFogData2.heightDensity, t);
                        color = LerpWithoutClamp(liveTimelineKeyGlobalFogData.color, liveTimelineKeyGlobalFogData2.color, t);
                        fogMode = liveTimelineKeyGlobalFogData2.fogMode;
                        expDensity = LerpWithoutClamp(liveTimelineKeyGlobalFogData.expDensity, liveTimelineKeyGlobalFogData2.expDensity, t);
                        start = LerpWithoutClamp(liveTimelineKeyGlobalFogData.start, liveTimelineKeyGlobalFogData2.start, t);
                        end = LerpWithoutClamp(liveTimelineKeyGlobalFogData.end, liveTimelineKeyGlobalFogData2.end, t);
                        useRadialDistance = liveTimelineKeyGlobalFogData2.useRadialDistance;
                        heightOption = LerpWithoutClamp(liveTimelineKeyGlobalFogData.heightOption, liveTimelineKeyGlobalFogData2.heightOption, t);
                        distanceOption = LerpWithoutClamp(liveTimelineKeyGlobalFogData.distanceOption, liveTimelineKeyGlobalFogData2.distanceOption, t);
                    }
                    else
                    {
                        isDistance = liveTimelineKeyGlobalFogData.isDistance;
                        isHeight = liveTimelineKeyGlobalFogData.isHeight;
                        startDistance = liveTimelineKeyGlobalFogData.startDistance;
                        height = liveTimelineKeyGlobalFogData.height;
                        heightDensity = liveTimelineKeyGlobalFogData.heightDensity;
                        color = liveTimelineKeyGlobalFogData.color;
                        fogMode = liveTimelineKeyGlobalFogData.fogMode;
                        expDensity = liveTimelineKeyGlobalFogData.expDensity;
                        start = liveTimelineKeyGlobalFogData.start;
                        end = liveTimelineKeyGlobalFogData.end;
                        useRadialDistance = liveTimelineKeyGlobalFogData.useRadialDistance;
                        heightOption = liveTimelineKeyGlobalFogData.heightOption;
                        distanceOption = liveTimelineKeyGlobalFogData.distanceOption;
                    }
                    updateInfo.isDistance = isDistance;
                    updateInfo.isHeight = isHeight;
                    updateInfo.startDistance = startDistance;
                    updateInfo.height = height;
                    updateInfo.heightDensity = heightDensity;
                    updateInfo.color = color;
                    updateInfo.fogMode = fogMode;
                    updateInfo.expDensity = expDensity;
                    updateInfo.start = start;
                    updateInfo.end = end;
                    updateInfo.useRadialDistance = useRadialDistance;
                    updateInfo.heightOption = heightOption;
                    updateInfo.distanceOption = distanceOption;
                    this.OnGlobalFog(ref updateInfo);
                }
            }
        }

        /// <summary>
        /// TiltShift用AlterUpdate
        /// </summary>
        private void AlterUpdate_TiltShift(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateTiltShift == null || sheet.tiltShiftKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.tiltShiftKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.tiltShiftKeys, currentFrame);
            if (curKey != null)
            {
                LiveTimelineKeyTiltShiftData liveTimelineKeyTiltShiftData = curKey as LiveTimelineKeyTiltShiftData;
                LiveTimelineKeyTiltShiftData liveTimelineKeyTiltShiftData2 = nextKey as LiveTimelineKeyTiltShiftData;
                TilsShiftUpdateInfo updateInfo = default(TilsShiftUpdateInfo);
                updateInfo.mode = liveTimelineKeyTiltShiftData.mode;
                updateInfo.quality = liveTimelineKeyTiltShiftData.quality;
                updateInfo.downsample = liveTimelineKeyTiltShiftData.downsample;
                if (liveTimelineKeyTiltShiftData2 != null && liveTimelineKeyTiltShiftData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyTiltShiftData, liveTimelineKeyTiltShiftData2, currentFrame);
                    updateInfo.blurArea = LerpWithoutClamp(liveTimelineKeyTiltShiftData.blurArea, liveTimelineKeyTiltShiftData2.blurArea, t);
                    updateInfo.maxBlurSize = LerpWithoutClamp(liveTimelineKeyTiltShiftData.maxBlurSize, liveTimelineKeyTiltShiftData2.maxBlurSize, t);
                    updateInfo.offset = LerpWithoutClamp(liveTimelineKeyTiltShiftData.offset, liveTimelineKeyTiltShiftData2.offset, t);
                    updateInfo.roll = LerpWithoutClamp(liveTimelineKeyTiltShiftData.roll, liveTimelineKeyTiltShiftData2.roll, t);
                    updateInfo.blurDir = LerpWithoutClamp(liveTimelineKeyTiltShiftData.blurDir, liveTimelineKeyTiltShiftData2.blurDir, t);
                    updateInfo.blurAreaDir = LerpWithoutClamp(liveTimelineKeyTiltShiftData.blurAreaDir, liveTimelineKeyTiltShiftData2.blurAreaDir, t);
                }
                else
                {
                    updateInfo.blurArea = liveTimelineKeyTiltShiftData.blurArea;
                    updateInfo.maxBlurSize = liveTimelineKeyTiltShiftData.maxBlurSize;
                    updateInfo.offset = liveTimelineKeyTiltShiftData.offset;
                    updateInfo.roll = liveTimelineKeyTiltShiftData.roll;
                    updateInfo.blurDir = liveTimelineKeyTiltShiftData.blurDir;
                    updateInfo.blurAreaDir = liveTimelineKeyTiltShiftData.blurAreaDir;
                }
                this.OnUpdateTiltShift(ref updateInfo);
            }
        }

        /// <summary>
        /// LightShuft用AlterUpdate
        /// </summary>
        private void AlterUpdate_LightShuft(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            if (this.OnLightShuft == null)
            {
                return;
            }
            LightShuftUpdateInfo updateInfo = default(LightShuftUpdateInfo);
            for (int i = 0; i < sheet.lightShuftKeys.Count; i++)
            {
                LiveTimelineLightShuftData liveTimelineLightShuftData = sheet.lightShuftKeys[i];
                if (liveTimelineLightShuftData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineLightShuftData.keys, currentFrame);
                if (curKey == null)
                {
                    continue;
                }
                LiveTimelineKeyLightShuftData liveTimelineKeyLightShuftData = curKey as LiveTimelineKeyLightShuftData;
                LiveTimelineKeyLightShuftData liveTimelineKeyLightShuftData2 = nextKey as LiveTimelineKeyLightShuftData;
                if (liveTimelineKeyLightShuftData2 != null && liveTimelineKeyLightShuftData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyLightShuftData, liveTimelineKeyLightShuftData2, currentFrame);
                    updateInfo._enable = liveTimelineKeyLightShuftData.enabled;
                    updateInfo._offset = LerpWithoutClamp(liveTimelineKeyLightShuftData.offset, liveTimelineKeyLightShuftData2.offset, t);
                    updateInfo._scale = LerpWithoutClamp(liveTimelineKeyLightShuftData.scale, liveTimelineKeyLightShuftData2.scale, t);
                    updateInfo._speed = LerpWithoutClamp(liveTimelineKeyLightShuftData.speed, liveTimelineKeyLightShuftData2.speed, t);
                    updateInfo._alpha = LerpWithoutClamp(liveTimelineKeyLightShuftData.alpha, liveTimelineKeyLightShuftData2.alpha, t);
                    updateInfo._alpha2 = LerpWithoutClamp(liveTimelineKeyLightShuftData.alpha2, liveTimelineKeyLightShuftData2.alpha2, t);
                    updateInfo._maskAlpha = LerpWithoutClamp(liveTimelineKeyLightShuftData.maskAlpha, liveTimelineKeyLightShuftData2.maskAlpha, t);
                    updateInfo._angle = LerpWithoutClamp(liveTimelineKeyLightShuftData.angle, liveTimelineKeyLightShuftData2.angle, t);
                }
                else
                {
                    updateInfo._enable = liveTimelineKeyLightShuftData.enabled;
                    updateInfo._offset = liveTimelineKeyLightShuftData.offset;
                    updateInfo._scale = liveTimelineKeyLightShuftData.scale;
                    updateInfo._speed = liveTimelineKeyLightShuftData.speed;
                    updateInfo._angle = liveTimelineKeyLightShuftData.angle;
                    updateInfo._alpha = liveTimelineKeyLightShuftData.alpha;
                    updateInfo._alpha2 = liveTimelineKeyLightShuftData.alpha2;
                    updateInfo._maskAlpha = liveTimelineKeyLightShuftData.maskAlpha;
                }
                float num = Mathf.RoundToInt(liveTimelineKeyLightShuftData.maskAnimeTime * 60f);
                if (num <= 0f)
                {
                    updateInfo._alpha.z = liveTimelineKeyLightShuftData.maskAlphaRange.x;
                }
                else
                {
                    float num2 = liveTimelineKeyLightShuftData.maskAlphaRange.y - liveTimelineKeyLightShuftData.maskAlphaRange.x;
                    float num3 = (currentFrame - liveTimelineKeyLightShuftData.frame) % num;
                    num /= 2f;
                    if (num3 >= num)
                    {
                        num3 -= num;
                        updateInfo._alpha.z = liveTimelineKeyLightShuftData.maskAlphaRange.x + (num2 - num2 * num3 / num);
                    }
                    else
                    {
                        updateInfo._alpha.z = liveTimelineKeyLightShuftData.maskAlphaRange.x + num2 * num3 / num;
                    }
                }
                this.OnLightShuft(ref updateInfo);
            }
        }

        private void AlterUpdate_ColorCorrection(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnColorCorrection == null)
            {
                return;
            }
            int count = sheet.colorCorrectionDataLists.Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyColorCorrectionDataList keys = sheet.colorCorrectionDataLists[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                LiveTimelineKey curKey = null;
                LiveTimelineKey nextKey = null;
                FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyColorCorrectionData liveTimelineKeyColorCorrectionData = curKey as LiveTimelineKeyColorCorrectionData;
                    bool flag = false;
                    if (_oldFrame < curKey.frame || _colorCorrectionCameraIndex != sheet.targetCameraIndex)
                    {
                        _colorCorrectionCameraIndex = sheet.targetCameraIndex;
                        flag = true;
                    }
                    if (nextKey != null && _oldFrame >= nextKey.frame)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        ColorCorrectionUpdateInfo updateInfo = default(ColorCorrectionUpdateInfo);
                        updateInfo._enable = liveTimelineKeyColorCorrectionData.enable;
                        updateInfo._saturation = liveTimelineKeyColorCorrectionData.saturation;
                        updateInfo._redCurve = liveTimelineKeyColorCorrectionData.redCurve;
                        updateInfo._greenCurve = liveTimelineKeyColorCorrectionData.greenCurve;
                        updateInfo._blueCurve = liveTimelineKeyColorCorrectionData.blueCurve;
                        updateInfo._selective = liveTimelineKeyColorCorrectionData.selective;
                        updateInfo._keyColor = liveTimelineKeyColorCorrectionData.keyColor;
                        updateInfo._targetColor = liveTimelineKeyColorCorrectionData.targetColor;
                        updateInfo._cameraIndex = sheet.targetCameraIndex;
                        this.OnColorCorrection(ref updateInfo);
                    }
                }
            }
        }

        private void AlterUpdate_SweatLocator(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateSweatLocator == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = sheet.sweatLocatorList.Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineSweatLocatorData liveTimelineSweatLocatorData = sheet.sweatLocatorList[i];
                if (liveTimelineSweatLocatorData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineSweatLocatorData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineSweatLocatorData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey == null)
                {
                    continue;
                }
                liveTimelineKey2 = liveTimelineSweatLocatorData.keys.At(findKeyResult.index + 1);
                LiveTimelineKeySweatLocatorData liveTimelineKeySweatLocatorData = liveTimelineKey as LiveTimelineKeySweatLocatorData;
                LiveTimelineKeySweatLocatorData liveTimelineKeySweatLocatorData2 = liveTimelineKey2 as LiveTimelineKeySweatLocatorData;
                if (liveTimelineKeySweatLocatorData2 != null && liveTimelineKeySweatLocatorData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeySweatLocatorData, liveTimelineKeySweatLocatorData2, currentFrame);
                    _sweatLocatorUpdateInfo.alpha = LerpWithoutClamp(liveTimelineKeySweatLocatorData.alpha, liveTimelineKeySweatLocatorData2.alpha, t);
                    _sweatLocatorUpdateInfo.locatorInfo[0].offset = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator0_offset, liveTimelineKeySweatLocatorData2.locator0_offset, t);
                    _sweatLocatorUpdateInfo.locatorInfo[0].offsetAngle = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator0_offsetAngle, liveTimelineKeySweatLocatorData2.locator0_offsetAngle, t);
                    _sweatLocatorUpdateInfo.locatorInfo[1].offset = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator1_offset, liveTimelineKeySweatLocatorData2.locator1_offset, t);
                    _sweatLocatorUpdateInfo.locatorInfo[1].offsetAngle = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator1_offsetAngle, liveTimelineKeySweatLocatorData2.locator1_offsetAngle, t);
                    _sweatLocatorUpdateInfo.locatorInfo[2].offset = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator2_offset, liveTimelineKeySweatLocatorData2.locator2_offset, t);
                    _sweatLocatorUpdateInfo.locatorInfo[2].offsetAngle = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator2_offsetAngle, liveTimelineKeySweatLocatorData2.locator2_offsetAngle, t);
                    _sweatLocatorUpdateInfo.locatorInfo[3].offset = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator3_offset, liveTimelineKeySweatLocatorData2.locator3_offset, t);
                    _sweatLocatorUpdateInfo.locatorInfo[3].offsetAngle = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator3_offsetAngle, liveTimelineKeySweatLocatorData2.locator3_offsetAngle, t);
                    _sweatLocatorUpdateInfo.locatorInfo[4].offset = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator4_offset, liveTimelineKeySweatLocatorData2.locator4_offset, t);
                    _sweatLocatorUpdateInfo.locatorInfo[4].offsetAngle = LerpWithoutClamp(liveTimelineKeySweatLocatorData.locator4_offsetAngle, liveTimelineKeySweatLocatorData2.locator4_offsetAngle, t);
                }
                else
                {
                    _sweatLocatorUpdateInfo.alpha = liveTimelineKeySweatLocatorData.alpha;
                    _sweatLocatorUpdateInfo.locatorInfo[0].offset = liveTimelineKeySweatLocatorData.locator0_offset;
                    _sweatLocatorUpdateInfo.locatorInfo[0].offsetAngle = liveTimelineKeySweatLocatorData.locator0_offsetAngle;
                    _sweatLocatorUpdateInfo.locatorInfo[1].offset = liveTimelineKeySweatLocatorData.locator1_offset;
                    _sweatLocatorUpdateInfo.locatorInfo[1].offsetAngle = liveTimelineKeySweatLocatorData.locator1_offsetAngle;
                    _sweatLocatorUpdateInfo.locatorInfo[2].offset = liveTimelineKeySweatLocatorData.locator2_offset;
                    _sweatLocatorUpdateInfo.locatorInfo[2].offsetAngle = liveTimelineKeySweatLocatorData.locator2_offsetAngle;
                    _sweatLocatorUpdateInfo.locatorInfo[3].offset = liveTimelineKeySweatLocatorData.locator3_offset;
                    _sweatLocatorUpdateInfo.locatorInfo[3].offsetAngle = liveTimelineKeySweatLocatorData.locator3_offsetAngle;
                    _sweatLocatorUpdateInfo.locatorInfo[4].offset = liveTimelineKeySweatLocatorData.locator4_offset;
                    _sweatLocatorUpdateInfo.locatorInfo[4].offsetAngle = liveTimelineKeySweatLocatorData.locator4_offsetAngle;
                }
                _sweatLocatorUpdateInfo.owner = liveTimelineKeySweatLocatorData.owner;
                _sweatLocatorUpdateInfo.locatorInfo[0].isVisible = liveTimelineKeySweatLocatorData.locator0_isVisible;
                _sweatLocatorUpdateInfo.locatorInfo[1].isVisible = liveTimelineKeySweatLocatorData.locator1_isVisible;
                _sweatLocatorUpdateInfo.locatorInfo[2].isVisible = liveTimelineKeySweatLocatorData.locator2_isVisible;
                _sweatLocatorUpdateInfo.locatorInfo[3].isVisible = liveTimelineKeySweatLocatorData.locator3_isVisible;
                _sweatLocatorUpdateInfo.locatorInfo[4].isVisible = liveTimelineKeySweatLocatorData.locator4_isVisible;
                if (liveTimelineSweatLocatorData.randomVisibleIndex.Count < liveTimelineKeySweatLocatorData.randomVisibleCount)
                {
                    _sweatLocatorRandomIndex.Clear();
                    for (int j = 0; j < _sweatLocatorUpdateInfo.locatorInfo.Length; j++)
                    {
                        _sweatLocatorRandomIndex.Add(j);
                    }
                    do
                    {
                        int index = UnityEngine.Random.Range(0, _sweatLocatorRandomIndex.Count);
                        int item = _sweatLocatorRandomIndex[index];
                        if (!liveTimelineSweatLocatorData.randomVisibleIndex.Contains(item))
                        {
                            liveTimelineSweatLocatorData.randomVisibleIndex.Add(item);
                        }
                        _sweatLocatorRandomIndex.RemoveAt(index);
                    }
                    while (liveTimelineSweatLocatorData.randomVisibleIndex.Count < liveTimelineKeySweatLocatorData.randomVisibleCount && 0 < _sweatLocatorRandomIndex.Count);
                }
                else if (liveTimelineKeySweatLocatorData.randomVisibleCount < liveTimelineSweatLocatorData.randomVisibleIndex.Count)
                {
                    do
                    {
                        int index2 = UnityEngine.Random.Range(0, liveTimelineSweatLocatorData.randomVisibleIndex.Count);
                        liveTimelineSweatLocatorData.randomVisibleIndex.RemoveAt(index2);
                    }
                    while (liveTimelineKeySweatLocatorData.randomVisibleCount < liveTimelineSweatLocatorData.randomVisibleIndex.Count);
                }
                if (0 < liveTimelineSweatLocatorData.randomVisibleIndex.Count)
                {
                    for (int k = 0; k < _sweatLocatorUpdateInfo.locatorInfo.Length; k++)
                    {
                        _sweatLocatorUpdateInfo.locatorInfo[k].isVisible = false;
                        _sweatLocatorUpdateInfo.locatorInfo[k].offset = Vector3.zero;
                        _sweatLocatorUpdateInfo.locatorInfo[k].offsetAngle = Vector3.zero;
                    }
                    for (int l = 0; l < liveTimelineSweatLocatorData.randomVisibleIndex.Count; l++)
                    {
                        _sweatLocatorUpdateInfo.locatorInfo[liveTimelineSweatLocatorData.randomVisibleIndex[l]].isVisible = true;
                    }
                }
                this.OnUpdateSweatLocator(ref _sweatLocatorUpdateInfo);
            }
        }

        /// <summary>
        /// レンズフレアのパラメータを更新する
        /// </summary>
        public void AlterUpdate_LensFlare(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateLensFlare == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = sheet.lensFlareList.Count;
            LensFlareUpdateInfo updateInfo = default(LensFlareUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineLensFlareData liveTimelineLensFlareData = sheet.lensFlareList[i];
                if (liveTimelineLensFlareData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineLensFlareData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineLensFlareData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey != null)
                {
                    liveTimelineKey2 = liveTimelineLensFlareData.keys.At(findKeyResult.index + 1);
                    LiveTimelineKeyLensFlareData liveTimelineKeyLensFlareData = liveTimelineKey as LiveTimelineKeyLensFlareData;
                    LiveTimelineKeyLensFlareData liveTimelineKeyLensFlareData2 = liveTimelineKey2 as LiveTimelineKeyLensFlareData;
                    if (liveTimelineKeyLensFlareData2 != null && liveTimelineKeyLensFlareData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyLensFlareData, liveTimelineKeyLensFlareData2, currentFrame);
                        updateInfo.color = LerpWithoutClamp(liveTimelineKeyLensFlareData.color, liveTimelineKeyLensFlareData2.color, t);
                        updateInfo.offset = LerpWithoutClamp(liveTimelineKeyLensFlareData.offset, liveTimelineKeyLensFlareData2.offset, t);
                        updateInfo.brightness = LerpWithoutClamp(liveTimelineKeyLensFlareData.brightness, liveTimelineKeyLensFlareData2.brightness, t);
                        updateInfo.fadeSpeed = LerpWithoutClamp(liveTimelineKeyLensFlareData.fadeSpeed, liveTimelineKeyLensFlareData2.fadeSpeed, t);
                    }
                    else
                    {
                        updateInfo.color = liveTimelineKeyLensFlareData.color;
                        updateInfo.offset = liveTimelineKeyLensFlareData.offset;
                        updateInfo.brightness = liveTimelineKeyLensFlareData.brightness;
                        updateInfo.fadeSpeed = liveTimelineKeyLensFlareData.fadeSpeed;
                    }
                    updateInfo.data = liveTimelineLensFlareData;
                    this.OnUpdateLensFlare(ref updateInfo);
                }
            }
        }

        private void AlterUpdate_A2UConfig(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateA2UConfig == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            A2UConfigUpdateInfo updateInfo = default(A2UConfigUpdateInfo);
            float num = 0.0166666675f;
            for (int i = 0; i < sheet.a2uConfigList.Count; i++)
            {
                LiveTimelineA2UConfigData liveTimelineA2UConfigData = sheet.a2uConfigList[i];
                if (!liveTimelineA2UConfigData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    FindTimelineKey(out curKey, out nextKey, liveTimelineA2UConfigData.keys, currentFrame);
                    if (curKey != null)
                    {
                        LiveTimelineKeyA2UConfigData liveTimelineKeyA2UConfigData = curKey as LiveTimelineKeyA2UConfigData;
                        updateInfo.blend = liveTimelineKeyA2UConfigData.blend;
                        updateInfo.order = liveTimelineKeyA2UConfigData.order;
                        updateInfo.enable = liveTimelineKeyA2UConfigData.enable;
                        updateInfo.data = liveTimelineA2UConfigData;
                        updateInfo.progressTime = (currentFrame - liveTimelineKeyA2UConfigData.frame) * num;
                        this.OnUpdateA2UConfig(ref updateInfo);
                    }
                }
            }
        }

        private void AlterUpdate_A2U(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateA2U == null)
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            A2UUpdateInfo updateInfo = default(A2UUpdateInfo);
            float num = 0.0166666675f;
            for (int i = 0; i < sheet.a2uList.Count; i++)
            {
                LiveTimelineA2UData liveTimelineA2UData = sheet.a2uList[i];
                if (liveTimelineA2UData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    continue;
                }
                FindTimelineKey(out curKey, out nextKey, liveTimelineA2UData.keys, currentFrame);
                if (curKey != null)
                {
                    LiveTimelineKeyA2UData liveTimelineKeyA2UData = curKey as LiveTimelineKeyA2UData;
                    LiveTimelineKeyA2UData liveTimelineKeyA2UData2 = nextKey as LiveTimelineKeyA2UData;
                    if (liveTimelineKeyA2UData2 != null && liveTimelineKeyA2UData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyA2UData, liveTimelineKeyA2UData2, currentFrame);
                        updateInfo.position = LerpWithoutClamp(liveTimelineKeyA2UData.position, liveTimelineKeyA2UData2.position, t);
                        updateInfo.scale = LerpWithoutClamp(liveTimelineKeyA2UData.scale, liveTimelineKeyA2UData2.scale, t);
                        updateInfo.rotationZ = LerpWithoutClamp(liveTimelineKeyA2UData.rotationZ, liveTimelineKeyA2UData2.rotationZ, t);
                        updateInfo.spriteScale = LerpWithoutClamp(liveTimelineKeyA2UData.spriteScale, liveTimelineKeyA2UData2.spriteScale, t);
                        updateInfo.spriteOpacity = LerpWithoutClamp(liveTimelineKeyA2UData.spriteOpacity, liveTimelineKeyA2UData2.spriteOpacity, t);
                        updateInfo.speed = LerpWithoutClamp(liveTimelineKeyA2UData.speed, liveTimelineKeyA2UData2.speed, t);
                    }
                    else
                    {
                        updateInfo.position = liveTimelineKeyA2UData.position;
                        updateInfo.scale = liveTimelineKeyA2UData.scale;
                        updateInfo.rotationZ = liveTimelineKeyA2UData.rotationZ;
                        updateInfo.spriteScale = liveTimelineKeyA2UData.spriteScale;
                        updateInfo.spriteOpacity = liveTimelineKeyA2UData.spriteOpacity;
                        updateInfo.speed = liveTimelineKeyA2UData.speed;
                    }
                    updateInfo.spriteColor = liveTimelineKeyA2UData.spriteColor;
                    updateInfo.textureIndex = (uint)Math.Max(0, liveTimelineKeyA2UData.textureIndex);
                    updateInfo.appearanceRandomSeed = liveTimelineKeyA2UData.appearanceRandomSeed;
                    updateInfo.spriteAppearance = liveTimelineKeyA2UData.spriteAppearance;
                    updateInfo.slopeRandomSeed = liveTimelineKeyA2UData.slopeRandomSeed;
                    updateInfo.spriteMinSlope = liveTimelineKeyA2UData.spriteMinSlope;
                    updateInfo.spriteMaxSlope = liveTimelineKeyA2UData.spriteMaxSlope;
                    updateInfo.startSec = Mathf.Max(0f, liveTimelineKeyA2UData.startSec);
                    updateInfo.isFlick = liveTimelineKeyA2UData.isFlick;
                    updateInfo.enable = liveTimelineKeyA2UData.enable;
                    updateInfo.data = liveTimelineA2UData;
                    updateInfo.progressTime = (currentFrame - liveTimelineKeyA2UData.frame) * num;
                    this.OnUpdateA2U(ref updateInfo);
                }
            }
        }

        public void InitializeCrossFadeCamera(CacheCamera camera)
        {
            _crossFadeCameraCache = camera;
        }

        public Quaternion GetCrossCameraWorldRotation()
        {
            if (_crossFadeCameraCache == null)
            {
                return Quaternion.identity;
            }
            return _crossFadeCameraCache.cacheTransform.rotation;
        }

        public CacheCamera GetCrossFadeCameraCache()
        {
            return _crossFadeCameraCache;
        }

        public bool CalculateCrossFadeCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = null;
            config.nextKey = null;
            config.keyType = FindTimelineConfig.KeyType.CurrentFrame;
            config.lookAtKeys = null;
            config.posKeys = sheet.crossFadeCameraPosKeys;
            config.extraCameraIndex = 0;
            CacheCamera crossFadeCameraCache = _crossFadeCameraCache;
            return CalculateCameraPos(out pos, sheet, currentFrame, crossFadeCameraCache, ref config, ref fnGetCameraPosValue);
        }

        public bool CalculateCrossFadeCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, LiveTimelineKey curKey, LiveTimelineKey nextKey, int currentFrame)
        {
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = curKey;
            config.nextKey = nextKey;
            config.keyType = FindTimelineConfig.KeyType.KeyDirect;
            config.posKeys = sheet.crossFadeCameraPosKeys;
            config.lookAtKeys = null;
            CacheCamera crossFadeCameraCache = _crossFadeCameraCache;
            config.extraCameraIndex = 0;
            return CalculateCameraPos(out pos, sheet, currentFrame, crossFadeCameraCache, ref config, ref fnGetCameraPosValue);
        }

        public bool CalculateCrossFadeCameraLookAt(out Vector3 lookAtPos, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            CacheCamera crossFadeCameraCache = GetCrossFadeCameraCache();
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = null;
            config.nextKey = null;
            config.keyType = FindTimelineConfig.KeyType.CurrentFrame;
            config.posKeys = sheet.crossFadeCameraPosKeys;
            config.lookAtKeys = sheet.crossFadeCameraLookAtKeys;
            config.extraCameraIndex = 0;
            return CalculateCameraLookAt(out lookAtPos, sheet, currentFrame, crossFadeCameraCache, ref config, ref fnGetCameraLookAtValue, ref fnGetCameraPosValue);
        }

        private void AlterUpdate_CrossFadeCameraPosition(CacheCamera cam, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateCrossFadeCamera == null)
            {
                return;
            }
            CrossFadeCameraUpdateInfo updateInfo = default(CrossFadeCameraUpdateInfo);
            updateInfo.isEnable = false;
            updateInfo.alpha = 0f;
            if (sheet.crossFadeCameraPosKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
            {
                this.OnUpdateCrossFadeCamera(ref updateInfo);
                return;
            }
            if (!sheet.crossFadeCameraPosKeys.EnablePlayModeTimeline(_playMode))
            {
                this.OnUpdateCrossFadeCamera(ref updateInfo);
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.crossFadeCameraPosKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyCrossFadeCameraPositionData liveTimelineKeyCrossFadeCameraPositionData = curKey as LiveTimelineKeyCrossFadeCameraPositionData;
            AlterUpdate_CrossFadeCameraAlpha(cam, sheet, currentFrame, out var alpha);
            updateInfo.isEnable = liveTimelineKeyCrossFadeCameraPositionData.isEnable;
            updateInfo.alpha = alpha;
            this.OnUpdateCrossFadeCamera(ref updateInfo);
            if (updateInfo.isEnable)
            {
                cam.camera.nearClipPlane = liveTimelineKeyCrossFadeCameraPositionData.nearClip;
                cam.camera.farClipPlane = liveTimelineKeyCrossFadeCameraPositionData.farClip;
                if (CalculateCrossFadeCameraPos(out var pos, sheet, curKey, nextKey, currentFrame))
                {
                    cam.cacheTransform.position = pos;
                    int cullingMask = liveTimelineKeyCrossFadeCameraPositionData.GetCullingMask();
                    cam.camera.cullingMask = cullingMask;
                }
            }
        }

        private void AlterUpdate_CrossFadeCameraAlpha(CacheCamera cam, LiveTimelineWorkSheet sheet, int currentFrame, out float alpha)
        {
            alpha = 0f;
            if (sheet.crossFadeCameraAlphaKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.crossFadeCameraAlphaKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.crossFadeCameraAlphaKeys, currentFrame);
            if (curKey != null)
            {
                LiveTimelineKeyCrossFadeCameraAlphaData liveTimelineKeyCrossFadeCameraAlphaData = curKey as LiveTimelineKeyCrossFadeCameraAlphaData;
                LiveTimelineKeyCrossFadeCameraAlphaData liveTimelineKeyCrossFadeCameraAlphaData2 = nextKey as LiveTimelineKeyCrossFadeCameraAlphaData;
                if (liveTimelineKeyCrossFadeCameraAlphaData2 != null && liveTimelineKeyCrossFadeCameraAlphaData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyCrossFadeCameraAlphaData, liveTimelineKeyCrossFadeCameraAlphaData2, currentFrame);
                    alpha = LerpWithoutClamp(liveTimelineKeyCrossFadeCameraAlphaData.alpha, liveTimelineKeyCrossFadeCameraAlphaData2.alpha, t);
                }
                else
                {
                    alpha = liveTimelineKeyCrossFadeCameraAlphaData.alpha;
                }
            }
        }

        private void AlterUpdate_CrossFadeCameraLookAt(CacheCamera cam, LiveTimelineWorkSheet sheet, int currentFrame, ref Vector3 outLookAt)
        {
            if (!sheet.crossFadeCameraLookAtKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && sheet.crossFadeCameraLookAtKeys.EnablePlayModeTimeline(_playMode) && CalculateCrossFadeCameraLookAt(out var lookAtPos, sheet, currentFrame))
            {
                cam.cacheTransform.LookAt(lookAtPos, Vector3.up);
                outLookAt = lookAtPos;
            }
        }

        private void AlterUpdate_CrossFadeCameraFov(CacheCamera cam, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (sheet.crossFadeCameraFovKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.crossFadeCameraFovKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.crossFadeCameraFovKeys, currentFrame);
            if (curKey == null)
            {
                return;
            }
            LiveTimelineKeyCrossFadeCameraFovData liveTimelineKeyCrossFadeCameraFovData = curKey as LiveTimelineKeyCrossFadeCameraFovData;
            LiveTimelineKeyCrossFadeCameraFovData liveTimelineKeyCrossFadeCameraFovData2 = nextKey as LiveTimelineKeyCrossFadeCameraFovData;
            float num = 80f;
            if (liveTimelineKeyCrossFadeCameraFovData2 != null && liveTimelineKeyCrossFadeCameraFovData2.interpolateType != 0)
            {
                float t = CalculateInterpolationValue(liveTimelineKeyCrossFadeCameraFovData, liveTimelineKeyCrossFadeCameraFovData2, currentFrame);
                num = LerpWithoutClamp(liveTimelineKeyCrossFadeCameraFovData.fov, liveTimelineKeyCrossFadeCameraFovData2.fov, t);
            }
            else if (liveTimelineKeyCrossFadeCameraFovData.fovType == LiveCameraFovType.Direct)
            {
                num = liveTimelineKeyCrossFadeCameraFovData.fov;
            }
            if (_limitFovForWidth)
            {
                float num2 = cam.camera.pixelWidth / (float)cam.camera.pixelHeight;
                if (num2 > _baseCameraAspectRatio)
                {
                    float num3 = num2 / _baseCameraAspectRatio;
                    num /= num3;
                }
            }
            cam.camera.fieldOfView = num;
        }

        private void AlterUpdate_CrossFadeCameraRoll(CacheCamera cam, LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (sheet.crossFadeCameraRollKeys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !sheet.crossFadeCameraRollKeys.EnablePlayModeTimeline(_playMode))
            {
                return;
            }
            LiveTimelineKey curKey = null;
            LiveTimelineKey nextKey = null;
            FindTimelineKey(out curKey, out nextKey, sheet.crossFadeCameraRollKeys, currentFrame);
            if (curKey != null)
            {
                LiveTimelineKeyCameraRollData liveTimelineKeyCameraRollData = curKey as LiveTimelineKeyCameraRollData;
                LiveTimelineKeyCameraRollData liveTimelineKeyCameraRollData2 = nextKey as LiveTimelineKeyCameraRollData;
                float num = 80f;
                if (liveTimelineKeyCameraRollData2 != null && liveTimelineKeyCameraRollData2.interpolateType != 0)
                {
                    float t = CalculateInterpolationValue(liveTimelineKeyCameraRollData, liveTimelineKeyCameraRollData2, currentFrame);
                    num = LerpWithoutClamp(liveTimelineKeyCameraRollData.degree, liveTimelineKeyCameraRollData2.degree, t);
                }
                else
                {
                    num = liveTimelineKeyCameraRollData.degree;
                }
                cam.cacheTransform.Rotate(0f, 0f, num);
            }
        }

        private void AlterUpdate_CrossFadeCamera(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (_crossFadeCameraCache != null)
            {
                AlterUpdate_CrossFadeCameraPosition(_crossFadeCameraCache, sheet, currentFrame);
                if (_crossFadeCameraCache.camera.enabled)
                {
                    Vector3 outLookAt = Vector3.zero;
                    AlterUpdate_CrossFadeCameraLookAt(_crossFadeCameraCache, sheet, currentFrame, ref outLookAt);
                    AlterUpdate_CrossFadeCameraFov(_crossFadeCameraCache, sheet, currentFrame);
                    AlterUpdate_CrossFadeCameraRoll(_crossFadeCameraCache, sheet, currentFrame);
                }
            }
        }

        public Quaternion GetMonitorCameraWorldRotation(int index)
        {
            if (_arrMonitorCameraCache == null || _arrMonitorCameraCache.Length <= index)
            {
                return Quaternion.identity;
            }
            return _arrMonitorCameraCache[index].cacheTransform.rotation;
        }

        public bool ExistsMonitroCamera(int index)
        {
            if (_arrMonitorCameraCache != null && index < _arrMonitorCameraCache.Length)
            {
                return _arrMonitorCameraCache[index] != null;
            }
            return false;
        }

        /// <summary>
        /// モニターカメラをセットする
        /// </summary>
        public void SetMonitorCameras(MonitorCamera[] monitorCameras)
        {
            if (monitorCameras != null && monitorCameras.Length != 0)
            {
                int num = monitorCameras.Length;
                _arrMonitorCameraCache = new CacheCamera[num];
                _arrMonitorCameraRoll = new float[num];
                for (int i = 0; i < num; i++)
                {
                    _arrMonitorCameraCache[i] = new CacheCamera(monitorCameras[i].targetCamera);
                    _arrMonitorCameraRoll[i] = 0f;
                }
            }
            else
            {
                _arrMonitorCameraCache = null;
            }
        }

        /// <summary>
        /// モニターカメラを更新する
        /// </summary>
        private void AlterUpdate_MonitorCamera(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (_arrMonitorCameraCache != null && _arrMonitorCameraCache.Length != 0)
            {
                AlterUpdate_MonitorCameraPosition(sheet, currentFrame);
                AlterUpdate_MonitorCameraLookAt(sheet, currentFrame);
                for (int i = 0; i < _arrMonitorCameraCache.Length; i++)
                {
                    float zAngle = _arrMonitorCameraRoll[i];
                    _arrMonitorCameraCache[i].cacheTransform.Rotate(0f, 0f, zAngle);
                }
            }
        }

        private static Vector3 GetMonitorCameraPositionValue(LiveTimelineKeyCameraPositionData keyData, LiveTimelineControl timelineControl, FindTimelineConfig config)
        {
            return (keyData as LiveTimelineKeyMonitorCameraPositionData).GetValue(timelineControl, config.extraCameraIndex);
        }

        public bool CalculateMonitorCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, int currentFrame, int timelineIndex = 0)
        {
            if (sheet.monitorCameraPosKeys.Count <= timelineIndex || _arrMonitorCameraCache == null || (_arrMonitorCameraCache != null && _arrMonitorCameraCache.Length <= timelineIndex))
            {
                pos = Vector3.zero;
                return false;
            }
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = null;
            config.nextKey = null;
            config.keyType = FindTimelineConfig.KeyType.CurrentFrame;
            config.lookAtKeys = null;
            config.posKeys = sheet.monitorCameraPosKeys[timelineIndex].keys;
            config.extraCameraIndex = timelineIndex;
            return CalculateCameraPos(out pos, sheet, currentFrame, _arrMonitorCameraCache[timelineIndex], ref config, ref fnGetMonitorCameraPositionValue);
        }

        public bool CalculateMonitorCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, int currentFrame, LiveTimelineKey curKey, LiveTimelineKey nextKey, int timelineIndex = 0)
        {
            if (sheet.monitorCameraPosKeys.Count <= timelineIndex || _arrMonitorCameraCache == null || (_arrMonitorCameraCache != null && _arrMonitorCameraCache.Length <= timelineIndex))
            {
                pos = Vector3.zero;
                return false;
            }
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = curKey;
            config.nextKey = nextKey;
            config.keyType = FindTimelineConfig.KeyType.KeyDirect;
            config.lookAtKeys = null;
            config.posKeys = sheet.monitorCameraPosKeys[timelineIndex].keys;
            config.extraCameraIndex = timelineIndex;
            return CalculateCameraPos(out pos, sheet, currentFrame, _arrMonitorCameraCache[timelineIndex], ref config, ref fnGetMonitorCameraPositionValue);
        }

        /// <summary>
        /// モニターカメラのポジションを更新する
        /// カメラのパラメータもここで更新
        /// </summary>
        private void AlterUpdate_MonitorCameraPosition(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (_arrMonitorCameraCache == null || (_arrMonitorCameraCache != null && _arrMonitorCameraCache.Length == 0))
            {
                return;
            }
            int count = sheet.monitorCameraPosKeys.Count;
            for (int i = 0; i < count; i++)
            {
                DereLiveTimelineKeyMonitorCameraPositionDataList keys = sheet.monitorCameraPosKeys[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode) || _arrMonitorCameraCache.Length <= i)
                {
                    continue;
                }
                CacheCamera cacheCamera = _arrMonitorCameraCache[i];
                LiveTimelineKey curKey = null;
                LiveTimelineKey nextKey = null;
                FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
                if (curKey != null)
                {
                    if (CalculateMonitorCameraPos(out var pos, sheet, currentFrame, curKey, nextKey, i))
                    {
                        cacheCamera.cacheTransform.position = pos;
                    }
                    LiveTimelineKeyMonitorCameraPositionData liveTimelineKeyMonitorCameraPositionData = curKey as LiveTimelineKeyMonitorCameraPositionData;
                    LiveTimelineKeyMonitorCameraPositionData liveTimelineKeyMonitorCameraPositionData2 = nextKey as LiveTimelineKeyMonitorCameraPositionData;
                    float num = 80f;
                    float num2 = 0f;
                    if (liveTimelineKeyMonitorCameraPositionData2 != null && liveTimelineKeyMonitorCameraPositionData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyMonitorCameraPositionData, liveTimelineKeyMonitorCameraPositionData2, currentFrame);
                        num = LerpWithoutClamp(liveTimelineKeyMonitorCameraPositionData.fov, liveTimelineKeyMonitorCameraPositionData2.fov, t);
                        num2 = LerpWithoutClamp(liveTimelineKeyMonitorCameraPositionData.roll, liveTimelineKeyMonitorCameraPositionData2.roll, t);
                    }
                    else
                    {
                        num = liveTimelineKeyMonitorCameraPositionData.fov;
                        num2 = liveTimelineKeyMonitorCameraPositionData.roll;
                    }
                    _arrMonitorCameraRoll[i] = num2;
                    cacheCamera.camera.fieldOfView = num;
                    cacheCamera.camera.nearClipPlane = liveTimelineKeyMonitorCameraPositionData.nearClip;
                    cacheCamera.camera.farClipPlane = liveTimelineKeyMonitorCameraPositionData.farClip;
                    if (liveTimelineKeyMonitorCameraPositionData.enable)
                    {
                        cacheCamera.camera.cullingMask = liveTimelineKeyMonitorCameraPositionData.GetCullingMask();
                    }
                    if (this.OnUpdateMonitorCamera != null)
                    {
                        MonitorCameraUpdateInfo updateInfo = default(MonitorCameraUpdateInfo);
                        updateInfo.index = i;
                        updateInfo.enable = liveTimelineKeyMonitorCameraPositionData.enable;
                        updateInfo.visibleCharaFlag = liveTimelineKeyMonitorCameraPositionData.visibleCharaPositionFlag;
                        this.OnUpdateMonitorCamera(ref updateInfo);
                    }
                }
            }
        }

        private static Vector3 GetMonitorCameraLookAtValue(LiveTimelineKeyCameraLookAtData keyData, LiveTimelineControl timelineControl, Vector3 camPos, FindTimelineConfig config)
        {
            return (keyData as LiveTimelineKeyMonitorCameraLookAtData).GetValue(timelineControl, camPos, config.extraCameraIndex);
        }

        /// <summary>
        /// モニターカメラの視点を計算する
        /// </summary>
        public bool CalculateMonitorCameraLookAt(out Vector3 lookAtPos, LiveTimelineWorkSheet sheet, int currentFrame, int timelineIndex = 0)
        {
            if (sheet.monitorLookAtPosKeys.Count <= timelineIndex || _arrMonitorCameraCache == null || (_arrMonitorCameraCache != null && _arrMonitorCameraCache.Length <= timelineIndex))
            {
                lookAtPos = Vector3.zero;
                return false;
            }
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = null;
            config.nextKey = null;
            config.keyType = FindTimelineConfig.KeyType.CurrentFrame;
            if (sheet.monitorCameraPosKeys.Count <= timelineIndex || sheet.monitorLookAtPosKeys.Count <= timelineIndex)
            {
                lookAtPos = Vector3.zero;
                return false;
            }
            config.posKeys = sheet.monitorCameraPosKeys[timelineIndex].keys;
            config.lookAtKeys = sheet.monitorLookAtPosKeys[timelineIndex].keys;
            config.extraCameraIndex = timelineIndex;
            return CalculateCameraLookAt(out lookAtPos, sheet, currentFrame, _arrMonitorCameraCache[timelineIndex], ref config, ref fnGetMonitorCameraLookAtValue, ref fnGetMonitorCameraPositionValue);
        }

        /// <summary>
        /// モニターカメラの視点を更新する
        /// </summary>
        private void AlterUpdate_MonitorCameraLookAt(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (_arrMonitorCameraCache == null || (_arrMonitorCameraCache != null && _arrMonitorCameraCache.Length == 0))
            {
                return;
            }
            int count = sheet.monitorLookAtPosKeys.Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyMonitorCameraLookAtDataList keys = sheet.monitorLookAtPosKeys[i].keys;
                if (!keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && keys.EnablePlayModeTimeline(_playMode) && _arrMonitorCameraCache.Length > i)
                {
                    CacheCamera cacheCamera = _arrMonitorCameraCache[i];
                    if (!CalculateMonitorCameraLookAt(out var lookAtPos, sheet, currentFrame, i))
                    {
                        break;
                    }
                    cacheCamera.cacheTransform.LookAt(lookAtPos);
                }
            }
        }

        public MultiCameraManager GetMultiCameraManager()
        {
            return _multiCameraManager;
        }

        public Quaternion GetMultiCameraWorldRotation(int index)
        {
            if (_multiCameraCache == null || _multiCameraCache.Length <= index)
            {
                return Quaternion.identity;
            }
            return _multiCameraCache[index].cacheTransform.rotation;
        }

        public bool ExistsMultiCamera(int index)
        {
            if (_multiCameraCache != null && index < _multiCameraCache.Length)
            {
                return _multiCameraCache[index] != null;
            }
            return false;
        }

        public void SetMultiCamera(MultiCameraManager manager, MultiCamera[] multiCamera)
        {
            _multiCamera = multiCamera;
            _multiCameraManager = manager;
            if (multiCamera != null)
            {
                _multiCameraCache = new CacheCamera[multiCamera.Length];
                for (int i = 0; i < multiCamera.Length; i++)
                {
                    _multiCameraCache[i] = new CacheCamera(multiCamera[i].GetCamera());
                }
            }
        }

        private static Vector3 GetMultiCameraPositionValue(LiveTimelineKeyCameraPositionData keyData, LiveTimelineControl timelineControl, FindTimelineConfig config)
        {
            return (keyData as LiveTimelineKeyMultiCameraPositionData).GetValue(timelineControl, config.extraCameraIndex);
        }

        public bool CalculateMultiCameraPos(out Vector3 pos, LiveTimelineWorkSheet sheet, LiveTimelineKey curKey, LiveTimelineKey nextKey, int currentFrame, int timelineIndex)
        {
            if (sheet.multiCameraPositionKeys.Count <= timelineIndex)
            {
                pos = Vector3.zero;
                return false;
            }
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = curKey;
            config.nextKey = nextKey;
            config.keyType = FindTimelineConfig.KeyType.KeyDirect;
            config.posKeys = sheet.multiCameraPositionKeys[timelineIndex].keys;
            config.lookAtKeys = null;
            config.extraCameraIndex = timelineIndex;
            return CalculateCameraPos(out pos, sheet, currentFrame, _multiCameraCache[timelineIndex], ref config, ref fnGetMultiCameraPositionValueFunc);
        }

        private void AlterUpdate_MultiCameraPosition(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            int count = sheet.multiCameraPositionKeys.Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyMultiCameraPositionDataList keys = sheet.multiCameraPositionKeys[i].keys;
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode) || i >= _multiCameraCache.Length)
                {
                    continue;
                }
                LiveTimelineKey curKey = null;
                LiveTimelineKey nextKey = null;
                FindTimelineKey(out curKey, out nextKey, keys, currentFrame);
                if (!CalculateMultiCameraPos(out var pos, sheet, curKey, nextKey, currentFrame, i))
                {
                    continue;
                }
                CacheCamera cacheCamera = _multiCameraCache[i];
                Camera camera = cacheCamera.camera;
                if (curKey == null)
                {
                    continue;
                }
                LiveTimelineKeyMultiCameraPositionData liveTimelineKeyMultiCameraPositionData = curKey as LiveTimelineKeyMultiCameraPositionData;
                LiveTimelineKeyMultiCameraPositionData liveTimelineKeyMultiCameraPositionData2 = nextKey as LiveTimelineKeyMultiCameraPositionData;
                camera.enabled = liveTimelineKeyMultiCameraPositionData.enableMultiCamera;
                if (liveTimelineKeyMultiCameraPositionData.enableMultiCamera)
                {
                    _isMultiCameraEnable = true;
                    camera.cullingMask = 0x10000000 | liveTimelineKeyMultiCameraPositionData.GetCullingMask();
                    float fieldOfView;
                    Vector3 maskPosition;
                    Quaternion maskRotation;
                    Vector3 maskScale;
                    if (liveTimelineKeyMultiCameraPositionData2 != null && liveTimelineKeyMultiCameraPositionData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyMultiCameraPositionData, liveTimelineKeyMultiCameraPositionData2, currentFrame);
                        fieldOfView = LerpWithoutClamp(liveTimelineKeyMultiCameraPositionData.fov, liveTimelineKeyMultiCameraPositionData2.fov, t);
                        maskPosition = LerpWithoutClamp(liveTimelineKeyMultiCameraPositionData.maskPosition, liveTimelineKeyMultiCameraPositionData2.maskPosition, t);
                        maskRotation = Quaternion.Lerp(liveTimelineKeyMultiCameraPositionData.maskRotation, liveTimelineKeyMultiCameraPositionData2.maskRotation, t);
                        maskScale = LerpWithoutClamp(liveTimelineKeyMultiCameraPositionData.maskScale, liveTimelineKeyMultiCameraPositionData2.maskScale, t);
                    }
                    else
                    {
                        fieldOfView = liveTimelineKeyMultiCameraPositionData.fov;
                        maskPosition = liveTimelineKeyMultiCameraPositionData.maskPosition;
                        maskRotation = liveTimelineKeyMultiCameraPositionData.maskRotation;
                        maskScale = liveTimelineKeyMultiCameraPositionData.maskScale;
                    }
                    camera.nearClipPlane = liveTimelineKeyMultiCameraPositionData.nearClip;
                    camera.farClipPlane = liveTimelineKeyMultiCameraPositionData.farClip;
                    camera.fieldOfView = fieldOfView;
                    cacheCamera.cacheTransform.localPosition = pos;
                    if (_multiCamera[i].maskIndex != liveTimelineKeyMultiCameraPositionData.maskIndex)
                    {
                        _multiCameraManager.AttachMask(i, liveTimelineKeyMultiCameraPositionData.maskIndex);
                    }
                    if (_multiCamera[i].maskIndex >= 0)
                    {
                        _multiCamera[i].maskPosition = maskPosition;
                        _multiCamera[i].maskRotation = maskRotation;
                        _multiCamera[i].maskScale = maskScale;
                    }
                }
            }
        }

        private static Vector3 GetMultiCameraLookAtValue(LiveTimelineKeyCameraLookAtData keyData, LiveTimelineControl timelineControl, Vector3 camPos, FindTimelineConfig config)
        {
            return (keyData as LiveTimelineKeyMultiCameraLookAtData).GetValue(timelineControl, camPos, config.extraCameraIndex);
        }

        public bool CalculateMultiCameraLookAt(out Vector3 pos, LiveTimelineWorkSheet sheet, LiveTimelineKey curKey, LiveTimelineKey nextKey, int currentFrame, int timelineIndex = 0)
        {
            if (sheet.multiCameraPositionKeys.Count <= timelineIndex || sheet.multiCameraLookAtKeys.Count <= timelineIndex)
            {
                pos = Vector3.zero;
                return false;
            }
            FindTimelineConfig config = default(FindTimelineConfig);
            config.curKey = curKey;
            config.nextKey = nextKey;
            config.keyType = FindTimelineConfig.KeyType.KeyDirect;
            config.posKeys = sheet.multiCameraPositionKeys[timelineIndex].keys;
            config.lookAtKeys = sheet.multiCameraLookAtKeys[timelineIndex].keys;
            config.extraCameraIndex = timelineIndex;
            return CalculateCameraLookAt(out pos, sheet, currentFrame, _multiCameraCache[timelineIndex], ref config, ref fnGetMultiCameraLookAtValueFunc, ref fnGetMultiCameraPositionValueFunc);
        }


        private void AlterUpdate_MultiCameraLookAt(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            int count = sheet.multiCameraLookAtKeys.Count;
            for (int i = 0; i < count; i++)
            {
                LiveTimelineKeyMultiCameraLookAtDataList keys = sheet.multiCameraLookAtKeys[i].keys;
                if (i >= _multiCameraCache.Length)
                {
                    break;
                }
                if (keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindTimelineKey(out var curKey, out var nextKey, keys, currentFrame);
                if (curKey != null && CalculateMultiCameraLookAt(out var pos, sheet, curKey, nextKey, currentFrame, i))
                {
                    LiveTimelineKeyMultiCameraLookAtData liveTimelineKeyMultiCameraLookAtData = curKey as LiveTimelineKeyMultiCameraLookAtData;
                    LiveTimelineKeyMultiCameraLookAtData liveTimelineKeyMultiCameraLookAtData2 = nextKey as LiveTimelineKeyMultiCameraLookAtData;
                    float zAngle;
                    if (liveTimelineKeyMultiCameraLookAtData2 != null && liveTimelineKeyMultiCameraLookAtData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyMultiCameraLookAtData, liveTimelineKeyMultiCameraLookAtData2, currentFrame);
                        zAngle = LerpWithoutClamp(liveTimelineKeyMultiCameraLookAtData.roll, liveTimelineKeyMultiCameraLookAtData2.roll, t);
                    }
                    else
                    {
                        zAngle = liveTimelineKeyMultiCameraLookAtData.roll;
                    }
                    Transform cacheTransform = _multiCameraCache[i].cacheTransform;
                    cacheTransform.LookAt(pos);
                    cacheTransform.Rotate(0f, 0f, zAngle);
                }
            }
        }

        private void AlterUpdate_MultiCamera(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (_multiCameraCache != null && !(_multiCameraManager == null) && _multiCameraManager.isInitialize)
            {
                AlterUpdate_MultiCameraPosition(sheet, currentFrame);
                AlterUpdate_MultiCameraLookAt(sheet, currentFrame);
            }
        }


        /// <summary>
        /// Glass用AlterUpdate
        /// </summary>
        public void AlterUpdate_Glass(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateGlass == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = sheet.glassList.Count;
            GlassUpdateInfo updateInfo = default(GlassUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineGlassData liveTimelineGlassData = sheet.glassList[i];
                if (liveTimelineGlassData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineGlassData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineGlassData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey != null)
                {
                    liveTimelineKey2 = liveTimelineGlassData.keys.At(findKeyResult.index + 1);
                    LiveTimelineKeyGlassData liveTimelineKeyGlassData = liveTimelineKey as LiveTimelineKeyGlassData;
                    LiveTimelineKeyGlassData liveTimelineKeyGlassData2 = liveTimelineKey2 as LiveTimelineKeyGlassData;
                    if (liveTimelineKeyGlassData2 != null && liveTimelineKeyGlassData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyGlassData, liveTimelineKeyGlassData2, currentFrame);
                        updateInfo.transparent = LerpWithoutClamp(liveTimelineKeyGlassData.transparent, liveTimelineKeyGlassData2.transparent, t);
                        updateInfo.specularColor = LerpWithoutClamp(liveTimelineKeyGlassData.specularColor, liveTimelineKeyGlassData2.specularColor, t);
                        updateInfo.specularPower = LerpWithoutClamp(liveTimelineKeyGlassData.specularPower, liveTimelineKeyGlassData2.specularPower, t);
                        updateInfo.specularColor = LerpWithoutClamp(liveTimelineKeyGlassData.specularColor, liveTimelineKeyGlassData2.specularColor, t);
                        updateInfo.lightPosition = LerpWithoutClamp(liveTimelineKeyGlassData.lightPosition, liveTimelineKeyGlassData2.lightPosition, t);
                    }
                    else
                    {
                        updateInfo.transparent = liveTimelineKeyGlassData.transparent;
                        updateInfo.specularColor = liveTimelineKeyGlassData.specularColor;
                        updateInfo.specularPower = liveTimelineKeyGlassData.specularPower;
                        updateInfo.specularColor = liveTimelineKeyGlassData.specularColor;
                        updateInfo.lightPosition = liveTimelineKeyGlassData.lightPosition;
                    }
                    updateInfo.data = liveTimelineGlassData;
                    updateInfo.sortOrderOffset = liveTimelineKeyGlassData.sortOrderOffset;
                    this.OnUpdateGlass(ref updateInfo);
                }
            }
        }

        public void AlterUpdate_ShaderControl(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (this.OnUpdateShaderControl == null)
            {
                return;
            }
            LiveTimelineKey liveTimelineKey = null;
            LiveTimelineKey liveTimelineKey2 = null;
            int count = sheet.shdCtrlList.Count;
            ShaderControlUpdateInfo updateInfo = default(ShaderControlUpdateInfo);
            for (int i = 0; i < count; i++)
            {
                LiveTimelineShaderControlData liveTimelineShaderControlData = sheet.shdCtrlList[i];
                if (liveTimelineShaderControlData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable) || !liveTimelineShaderControlData.keys.EnablePlayModeTimeline(_playMode))
                {
                    continue;
                }
                FindKeyResult findKeyResult = liveTimelineShaderControlData.keys.FindKeyCached(currentFrame, availableFindKeyCache);
                liveTimelineKey = findKeyResult.key;
                if (liveTimelineKey != null)
                {
                    liveTimelineKey2 = liveTimelineShaderControlData.keys.At(findKeyResult.index + 1);
                    LiveTimelineKeyShaderControlData liveTimelineKeyShaderControlData = liveTimelineKey as LiveTimelineKeyShaderControlData;
                    LiveTimelineKeyShaderControlData liveTimelineKeyShaderControlData2 = liveTimelineKey2 as LiveTimelineKeyShaderControlData;
                    if (liveTimelineShaderControlData.updatedKeyFrame == liveTimelineKeyShaderControlData.frame)
                    {
                        break;
                    }
                    liveTimelineShaderControlData.updatedKeyFrame = liveTimelineKeyShaderControlData.frame;
                    updateInfo.condition = liveTimelineKeyShaderControlData.condition;
                    updateInfo.conditionParam = liveTimelineKeyShaderControlData.conditionParam;
                    updateInfo.targetFlags = liveTimelineKeyShaderControlData.targetFlags;
                    updateInfo.behaviorFlags = liveTimelineKeyShaderControlData.behaviorFlags;
                    updateInfo.useVtxClrB = liveTimelineKeyShaderControlData.useVtxClrB;
                    if (liveTimelineKeyShaderControlData2 != null && liveTimelineKeyShaderControlData2.interpolateType != 0)
                    {
                        float t = CalculateInterpolationValue(liveTimelineKeyShaderControlData, liveTimelineKeyShaderControlData2, currentFrame);
                        updateInfo.lerpDiffuse = LerpWithoutClamp(liveTimelineKeyShaderControlData.lerpDiffuse, liveTimelineKeyShaderControlData2.lerpDiffuse, t);
                        updateInfo.lerpGradation = LerpWithoutClamp(liveTimelineKeyShaderControlData.lerpGradetion, liveTimelineKeyShaderControlData2.lerpGradetion, t);
                    }
                    else
                    {
                        updateInfo.lerpDiffuse = liveTimelineKeyShaderControlData.lerpDiffuse;
                        updateInfo.lerpGradation = liveTimelineKeyShaderControlData.lerpGradetion;
                    }
                    updateInfo.data = liveTimelineShaderControlData;
                    this.OnUpdateShaderControl(ref updateInfo);
                }
            }
        }

        public void AlterUpdate_DressChange(LiveTimelineWorkSheet sheet, int currentFrame)
        {
            if (!sheet.dressChangeDataList.HasAttribute(LiveTimelineKeyDataListAttr.Disable) && sheet.dressChangeDataList.EnablePlayModeTimeline(_playMode) && this.OnUpdateDressChange != null)
            {
                LiveTimelineKey curKey = null;
                LiveTimelineKey nextKey = null;
                FindTimelineKey(out curKey, out nextKey, sheet.dressChangeDataList, currentFrame);
                if (curKey != null)
                {
                    DressChangeUpdateInfo updateInfo = default(DressChangeUpdateInfo);
                    LiveTimelineKeyDressChangeData liveTimelineKeyDressChangeData = curKey as LiveTimelineKeyDressChangeData;
                    updateInfo.targetFlags = liveTimelineKeyDressChangeData.targetFlags;
                    updateInfo.dressType = liveTimelineKeyDressChangeData.dressType;
                    this.OnUpdateDressChange(ref updateInfo);
                }
            }
        }

        public void CreateIgnoreLaserHashDic()
        {
            int count = data.GetWorkSheetList().Count;
            _ignoreLaserHashDic = new Dictionary<int, bool>[count];
            for (int i = 0; i < count; i++)
            {
                int[] controlledGameObjectHashes = GetControlledGameObjectHashes(i);
                Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
                for (int j = 0; j < controlledGameObjectHashes.Length; j++)
                {
                    dictionary[controlledGameObjectHashes[j]] = true;
                }
                _ignoreLaserHashDic[i] = dictionary;
            }
        }

        private int[] GetControlledGameObjectHashes(int sheetIdx)
        {
            if (data == null || data.GetWorkSheetList().Count <= sheetIdx)
            {
                return new int[0];
            }
            List<int> list = new List<int>();
            LiveTimelineWorkSheet workSheet = data.GetWorkSheet(sheetIdx);
            for (int i = 0; i < workSheet.objectList.Count; i++)
            {
                LiveTimelineObjectData liveTimelineObjectData = workSheet.objectList[i];
                if (!liveTimelineObjectData.keys.HasAttribute(LiveTimelineKeyDataListAttr.Disable))
                {
                    list.Add(liveTimelineObjectData.nameHash);
                }
            }
            return list.ToArray();
        }
    }
}
