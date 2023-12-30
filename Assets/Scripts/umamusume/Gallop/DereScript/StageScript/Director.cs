using Cute;
using Cutt;
using Stage;
using Stage.Cyalume;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Director : MonoBehaviour
{
    [SerializeField]
    public MusicManager musicManager;

    public enum QualityLevel
    {
        Auto = 0,
        Lv1_Outline75 = 1,
        Lv2_Outline = 2,
        Lv3_OutlineBloom50 = 3,
        Lv4_OutlineBloom75 = 4,
        Lv5_OutlineBloom100 = 5,
        Lv6_OutlineDof75 = 6,
        Lv7_OutlineDof1280 = 7,
        Lv8_OutlineDof = 8,
        Lv9_Rich = 9,
        Max = 9
    }

    public class RenderTarget
    {
        public enum eCapability
        {
            None = 0,
            Color = 1,
            Depth = 2,
            Monitor = 4,
            PostEffect = 8,
            ColorAndDepth = 3
        }

        private eCapability _capability;

        private RenderTexture _color;

        private RenderTexture _depth;

        private RenderTexture _result;

        private RenderTexture _monitor;

        public eCapability capability => _capability;

        public RenderTexture color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }

        public RenderTexture depth
        {
            get
            {
                return _depth;
            }
            set
            {
                _depth = value;
            }
        }

        public RenderTexture result
        {
            get
            {
                return _result ?? _color;
            }
            set
            {
                _result = value;
            }
        }

        public RenderTexture monitor
        {
            get
            {
                return _monitor;
            }
            set
            {
                _monitor = value;
            }
        }

        public RenderTarget(int width, int height, eQualityType qualityType)
        {
            bool flag = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            Func<string, RenderTextureFormat, int, RenderTexture> func = delegate (string propertyName, RenderTextureFormat format, int depth)
            {
                RenderTexture renderTexture2 = new RenderTexture(width, height, depth, format);
                if (renderTexture2 != null)
                {
                    renderTexture2.Create();
                    renderTexture2.name = propertyName;
                    Shader.SetGlobalTexture(propertyName, renderTexture2);
                }
                return renderTexture2;
            };
            Func<float, RenderTexture> func2 = delegate (float scale)
            {
                int width2 = (int)(width * scale);
                int height2 = (int)(height * scale);
                RenderTexture renderTexture = new RenderTexture(width2, height2, 0);
                if (renderTexture != null)
                {
                    renderTexture.Create();
                    renderTexture.filterMode = FilterMode.Bilinear;
                }
                return renderTexture;
            };
            switch (qualityType)
            {
                case eQualityType.Quality3D_Light:
                    _color = func("_gColorTexture", RenderTextureFormat.ARGB32, 24);
                    _capability |= eCapability.Color;
                    break;
                case eQualityType.Quality3D:
                case eQualityType.Quality3D_Rich:
                    if (flag)
                    {
                        _color = func("_gColorTexture", RenderTextureFormat.ARGB32, 0);
                        _depth = func("_CameraDepthTexture", RenderTextureFormat.Depth, 24);
                        _result = func("_gResultTexture", RenderTextureFormat.ARGB32, 0);
                        _capability |= eCapability.ColorAndDepth;
                        _capability |= eCapability.PostEffect;
                    }
                    else
                    {
                        _color = func("_gColorTexture", RenderTextureFormat.ARGB32, 24);
                        _capability |= eCapability.Color;
                    }
                    //_monitor = func2(0.25f);
                    _monitor = func2(0.25f);
                    _capability |= eCapability.Monitor;
                    break;
            }
        }

        public void Release()
        {
            ReleaseRT(ref _result);
            ReleaseRT(ref _monitor);
        }

        public bool CheckCapability(eCapability caps)
        {
            return (_capability & caps) != 0;
        }

        private static void ReleaseRT(ref RenderTexture rt)
        {
            if (rt != null)
            {
                rt.Release();
            }
            rt = null;
        }

        public void ResizeScreen(int width, int height)
        {
            if (_color != null)
            {
                _color.Release();
                _color.width = width;
                _color.height = height;
                _color.Create();
            }
            if (_depth != null)
            {
                _depth.Release();
                _depth.width = width;
                _depth.height = height;
                _depth.Create();
            }
            if (_result != null)
            {
                _result.Release();
                _result.width = width;
                _result.height = height;
                _result.Create();
            }
        }
    }

    private enum CameraIndex
    {
        Motion,
        Timeline1,
        Timeline2,
        Timeline3
    }

    public class Live3DSettings
    {
        public int _id;

        public int _beforeId;

        public int _debugSongId;

        public string _cuttName = "";

        public string _cuttAssetBundleName = "";

        public string _bgName = "";

        public string _bgAssetsBundleName = "";

        public float _delayTime;

        public string _autoLipName = "";

        public string _cameraName = "";

        public string _charaMotion = "";

        public string _postEffect = "";

        public const string CuttName = "3d_cutt_";

        public int _charaMotionNum;

        public int _overrideMotionNum;

        public int _charaHeightMotionNum;

        public string _overrideMotionAssetBundleName = "";

        public string _charaHeightMotionAssetBundleName = "";

        public string _autoLipAssetBundleName = "";

        public string _cameraAssetBundleName = "";

        public string _charaMotionAssetBundleName = "";

        public int _uvMovieAssetBundleCount;

        public string[] _uvMovieAssetBundleNames = new string[16];

        public string[] _uvMovieAssetBundleLabels = new string[16];

        public string[] _uvMovieCharaAssetBundleNames = new string[16];

        public string[] _imgResourcesLabels;

        public string[] _mirrorScanMateriaLabels;

        public string[] _mirrorScanAssetBundleNames;

        public Dictionary<string, bool> _useHqParticlesDic = new Dictionary<string, bool>();

        public int _mobCyalume3DResourceID;

        public int _mobCyalume3DMotionID;

        public void setData(Master3dLive.Live3dData info, int charaId)
        {
            if (info == null)
            {
                return;
            }
            if (info.uvMovies != null)
            {
                for (int i = 0; i < info.uvMovies.Length; i++)
                {
                    if (info.uvMovies[i] != null && info.uvMovies[i].Length != 0)
                    {
                        _uvMovieAssetBundleNames[_uvMovieAssetBundleCount] = "3d_uvm_" + info.uvMovies[i] + ".unity3d";
                        _uvMovieAssetBundleLabels[_uvMovieAssetBundleCount] = info.uvMovies[i];
                        _uvMovieCharaAssetBundleNames[_uvMovieAssetBundleCount] = "3d_uvm_" + info.uvMovies[i] + "_chr{0:D3}.unity3d";
                        _uvMovieAssetBundleCount++;
                    }
                }
            }
            if (info.imgResources != null)
            {
                _imgResourcesLabels = new string[info.imgResources.Length];
                for (int i = 0; i < info.imgResources.Length; i++)
                {
                    _imgResourcesLabels[i] = "3D/UVMovie/Texture/" + info.imgResources[i];
                }
            }
            if (info.mirrorScanMatIDs != null && info.mirrorScanMatIDs.Length != 0)
            {
                _mirrorScanMateriaLabels = new string[info.mirrorScanMatIDs.Length];
                for (int i = 0; i < info.mirrorScanMatIDs.Length; i++)
                {
                    _mirrorScanMateriaLabels[i] = string.Format("3D/Stage/stg_{0:0000}/Materials/mt_stg_{0:0000}_MirrorScanLight_{1:000}", info.bg, info.mirrorScanMatIDs[i]);
                }
            }
            else if (info.mirrorScanMatNames != null && info.mirrorScanMatNames.Length != 0)
            {
                int num = info.mirrorScanMatNames.Length;
                _mirrorScanMateriaLabels = new string[num];
                _mirrorScanAssetBundleNames = new string[num];
                for (int i = 0; i < num; i++)
                {
                    _mirrorScanAssetBundleNames[i] = "3d_" + info.mirrorScanMatNames[i] + ".unity3d";
                    _mirrorScanMateriaLabels[i] = "3D/MirrorScanLight/Pattern/" + info.mirrorScanMatNames[i] + "/mt_" + info.mirrorScanMatNames[i];
                }
            }
            _charaMotionNum = info.charaMotionNum;
            _overrideMotionNum = info.overrideMotionNum;
            _charaHeightMotionNum = info.charaHeightMotionNum;
            string text2 = info.charaMotion.Replace("01_legacy", "").Replace("{0:D2}_legacy", "");
            _overrideMotionAssetBundleName = "3d_cutt_" + text2 + "o{0:D3}_legacy.unity3d";
            _charaHeightMotionAssetBundleName = "3d_cutt_" + text2 + "ch{0:D3}_legacy.unity3d";
            _mobCyalume3DResourceID = info.mobCyalume3DResourceID;
            _mobCyalume3DMotionID = info.mobCyalume3DMotionID;
        }
    }

    private enum IndirectLightShuftTextureType
    {
        ShaftA,
        ShaftB,
        Mask,
        ShaftC
    }

    public enum eAssetTaskState
    {
        Idle,
        WorkDownload,
        DoneDonwload,
        WorkLoad,
        Done
    }

    public enum eSoundType
    {
        None = 0,
        Solo = 1,
        Another = 2,
        Call = 3,
        SE = 4,
        Collab = 5,
        TempMax = 6,
        SoloOnly3D = 1001,
        AnotherOnly3D = 1002,
        CallOnly3D = 1003,
        SEOnly3D = 1004,
        CollabOnly3D = 1005
    }

    public enum eQualityType
    {
        Quality2D_Light = 0,
        Quality2D = 1,
        Quality3D_Light = 2,
        Quality3D = 3,
        Quality3D_Rich = 4,
        Quality2D_Rich = 5,
        QualityMovie = 6,
        Quality_Unknown = -1
    }

    private static Director _instance = null;

    private static int s_SeasonID = 0;

    public static int sBootDirectLiveID = 0;

    private static int _screenOriginWidth = 0;

    private static eSoundType _soundType = eSoundType.None;

    private eQualityType _qualityType = eQualityType.Quality_Unknown;

    public static QualityLevel _debugQualityType = QualityLevel.Auto;

    public static Character3DBase.CharacterData[] _debugCharacterSet = null;

    private const int INVALID_LIVE_ID = -1;

    private const float MAXIMUM_MUSIC_SCORE_TIME = 99999f;

    public const int CharacterMax = 15;

    public const float _cySpringAttenuation = 1.7f;

    public const float _cyGravityAttenuation = 2.05f;

    private readonly float kSmoothingTargetDeltaTime = 0.0166666675f;

    private readonly float kSmoothingDeltaTimeThreshold = 0.0003f;

    private readonly float kSmoothingOvertimeClamp = 71f / (904f * (float)Math.PI);

    [SerializeField]
    private GameObject[] _prefabs;

    [SerializeField]
    private bool _isEnableBloom = true;

    [SerializeField]
    private float _lipDelayTime = 0.1f;

    [SerializeField]
    private QualityLevel _forceQualityLevel;

    [SerializeField]
    private static int _screenOriginHeight = 0;

    [SerializeField]
    private Vector4 _GlobalLightDir = new Vector4(0f, 0.965f, 0.258f, 0f);

    [SerializeField]
    private Color _GlobalEnvColor = Color.white;

    private bool _isBootDirect;

    private int _bootDirectLiveID = -1;

    private float _initializeTimer;

    private bool _isInitialized;

    private int _charaClothResetTimer = 10;

    private bool _isHaveHqStage;

    private QualityLevel _qualityLevel;

    private RandomTable<float> _randomTable = new RandomTable<float>();

    private LyricsManager _lyricsManager;

    private PropsManager _propsManager = new PropsManager();

    private StageController _stageController;

    private List<CharacterObject> _characterObjects = new List<CharacterObject>();

    private Dictionary<int, CharacterObject[]> _mapSpareCharacter = new Dictionary<int, CharacterObject[]>();

    //CuttMotionVmRunでのみ使用
    private Character3DBase.CharacterData[] _characterData = new Character3DBase.CharacterData[15];

    private List<CharaEffectInfo> _listShadowEffectInfo = new List<CharaEffectInfo>();

    private List<CharaEffectInfo> _listSpotLightEffectInfo = new List<CharaEffectInfo>();

    private int _numMemberUnit = 15;

    private bool _enableSmoothMusicTime = true;

    private float _smoothMusicScoreTime;

    private float _smoothingWorkTime;

    private float _lastMusicScoreWithoutDelay;

    private float _lastMusicScoreTime;

    private float _GlobalEnvRate = 1f;

    private float _GlobalRimRate = 1f;

    private float _GlobalRimShadowRate;

    private float _GlobalSpecularRate = 1f;

    private float _GlobalToonRate = 1f;

    private Texture _GlobalEnvTex;

    private bool _enableTimelineGlobalRimParam = true;

    public bool _existsMobCyalume3DImitation;

    private readonly int[] kTimelineCameraIndices = new int[3] { 1, 2, 3 };

    private const int kMotionCameraIndex = 0;

    private const string TransparentCameraObjectName = "TransparentCamera";

    private const int TransparentCameraDepthOffset = 10;

    private const int CrossFadeCameraDepth = -30;

    private const int MultiCameraDepth = -30;

    [SerializeField]
    private GameObject[] _cameraNodes;

    [SerializeField]
    private CameraLookAt _cameraLookAt;

    [SerializeField]
    private Animator _cameraAnimator;

    [SerializeField]
    private Camera _uiCamera;

    private RenderTarget _renderTarget;

    private bool _useCalcedCameraRect;

    private Rect _mainCameraRect = new Rect(0f, 0f, 1f, 1f);

    private int _activeCameraIndex;

    private RuntimeAnimatorController _cameraRuntimeAnimator;

    private Camera[] _cameraObjects;

    private Transform[] _cameraTransforms;

    private Camera _mainCameraObject;

    private Transform _mainCameraTransform;

    private Camera _transparentCamera;

    private Transform _transparentCameraTransform;

    private bool _isEnableTransparentCamera;

    private LiveCharaPositionFlag _monitorCameraVisiblePositionFlag = LiveCharaPositionFlag.All;

    private LiveCharaPositionFlag _restoreCharacterVisibleFlag;

    private CrossFadeCamera _crossFadeCamera;

    private MultiCameraManager _multiCameraManager;

    private MonitorCameraManager _monitorCameraManager = new MonitorCameraManager();

    private GameObject _initializeCamera;

    private Transform _tmInitializeCamera;

    private FinalizeCamera _finalizeCamera;

    private bool _isDisplayGUI = true;

    private bool _isDisplayGUIChangeButton = true;

    private bool _isDisplayOnlyFPS;

    [SerializeField]
    private bool _isDebugPose;

    private static readonly string[] s_addNames = new string[6] { "", "_Solo", "_Another", "_Call", "_Se", "" };

    private Live3DSettings _live3DSettings;

    private static readonly string IndirectLightShuftResourcePath = "3D/InDirectLightShuft/lightshuft_{0:0000}/{1}";

    private static readonly string[] _indirectLightShuftResourceNames = new string[3] { "tx_indirectLightShuft_def_A_hq", "tx_indirectLightShuft_def_B_hq", "tx_indirectLightShuft_sun_hq" };

    private bool _isBloom;

    private bool _isDof;

    private bool _isForcePostEffectDisable;

    private bool _isForceLOD;

    private List<PostEffectLive3D> _imageEffectLive3dList = new List<PostEffectLive3D>();

    private ScreenFade[] _screenFadeArray;

    private PostEffectLive3D.ColorCorrection[] _colorCorrectionCurves;

    private PostEffectLive3D.ColorCorrectionCurvesParameter _colorCorrectionParameter;

    private PostEffectLive3D.IndirectLightShafts[] _indirectLightShuft;

    private Texture2D[] _indirectLightShuftTexture;

    public const int UVMOIVE_COUNT = 16;

    public const int IMAGERESOURCE_COUNT = 8;

    public const int MIRRORSCANLIGHTMAT_COUNT = 8;

    public const string k3DCharaDataAsset = "3d_chara_data";

    public const string k3DCharaDataAssetFile = "3d_chara_data.unity3d";

    public const string k3DCharaDataCSV = "3D/Live/3d_chara_data";

    public static readonly string LIVE3D_SETTINGS_CSV_PATH = "3D/Live/3d_live_data";

    public static readonly string LIVE3D_SEASON_SETTINGS_CSV_PATH = "3D/Live/season/3d_live_data_season_{0:D03}";

    public static readonly string LIVE3D_ASSET_3D_LIVE_SEASON_FORAMT = "3d_live_data_season_{0:D03}.unity3d";

    public static readonly string LIVE3D_SETTINGS_CSV_CLIENT_PATH = "DebugMaster/live/3d_live_data";

    public static readonly string LIVE_SETTINGS_CSV_PATH = "Master/live/live_data";

    public static readonly string LIVE_SEASON_DATA_CSV_PATH = "DebugMaster/Live/live_season_data";

    public const string LIVE3D_SWAPLOCATION_CSV_PATH = "DebugMaster/Live/musicvideo_swap_location";

    //private bool _isUseAsssetBundle;

    private eAssetTaskState _assetTaskState;

    private List<string> _lstAssetBundleName = new List<string>();

    private Queue<Func<IEnumerator>> _qAssetTask = new Queue<Func<IEnumerator>>();

    private Master3DCharaData _master3DCharaData;

    private Master3DCharaData.CharaDataWork[] _charaDataWorks = new Master3DCharaData.CharaDataWork[15];

    private int[] _uvMovieCharaIdArray;

    private static readonly LiveCharaPositionFlag[] _characterFlags = new LiveCharaPositionFlag[15]
    {
        LiveCharaPositionFlag.Center,
        LiveCharaPositionFlag.Left1,
        LiveCharaPositionFlag.Left2,
        LiveCharaPositionFlag.Right1,
        LiveCharaPositionFlag.Right2,
        LiveCharaPositionFlag.Left3,
        LiveCharaPositionFlag.Right3,
        LiveCharaPositionFlag.Left4,
        LiveCharaPositionFlag.Right4,
        LiveCharaPositionFlag.Left5,
        LiveCharaPositionFlag.Right5,
        LiveCharaPositionFlag.Left6,
        LiveCharaPositionFlag.Right6,
        LiveCharaPositionFlag.Left7,
        LiveCharaPositionFlag.Right7
    };

    private static Dictionary<int, int> sPropsTimelineDataDictionary = new Dictionary<int, int>();

    private CuttMotionChange _cuttMotionChange;

    private CuttDressChange _cuttDressChange;

    private CuttMotionVmRun _cuttVmrun;

    private SharedShaderParam _sharedShaderParam;

    private LiveTimelineControl _liveTimelineControl;

    private bool _switchSheetCondition = true;

    private bool _switchSheetEnable;

    private int _switchSheetAltSheetNumber = -1;

    public static Director instance => _instance;

    public static int sSeasonID
    {
        get
        {
            return s_SeasonID;
        }
        set
        {
            s_SeasonID = value;
        }
    }

    public static eSoundType soundType
    {
        get
        {
            return _soundType;
        }
        set
        {
            _soundType = value;
        }
    }

    public static bool forceUseAssetBundle { get; set; }

    public QualityLevel qualityLevel
    {
        get
        {
            return _qualityLevel;
        }
        set
        {
            if (_qualityLevel != value)
            {
                _qualityLevel = value;
                ApplyQualityLevel();
                ApplyShaderQualityLevel();
            }
        }
    }

    public bool isHaveHqStage => _isHaveHqStage;

    public bool isForceLOD => _isForceLOD;

    public RandomTable<float> randomTable => _randomTable;

    public int bootDirectLiveID => _bootDirectLiveID;

    public bool isBootDirect => _isBootDirect;

    public List<CharacterObject> characterObjects => _characterObjects;

    public List<CharaEffectInfo> listShadowEffectInfo => _listShadowEffectInfo;

    public List<CharaEffectInfo> listSpotLightEffectInfo => _listSpotLightEffectInfo;

    public int MemberUnitNumber
    {
        get
        {
            return _numMemberUnit;
        }
        set
        {
            _numMemberUnit = value;
        }
    }

    public float lipDelayTime => _lipDelayTime;

    public float lastMusicScoreTime => _lastMusicScoreTime;

    private float smoothMusicScoreTime => _smoothMusicScoreTime;

    public float musicScoreTime => Mathf.Clamp(smoothMusicScoreTime, 0f, 99999f);

    private float directMusicScoreTime
    {
        get
        {
            /*
            if (SingletonMonoBehaviour<LiveController>.IsInstanceEmpty())
            {
                return 0f;
            }
            return Mathf.Clamp(SingletonMonoBehaviour<LiveController>.instance.GetMusicTimeIgnoredTimingAdjust(), 0f, 99999f);
            */
            return musicManager.currentTime;
        }
    }
    public RenderTarget renderTarget => _renderTarget;

    public bool useCalcedCameraRect => _useCalcedCameraRect;

    public Rect mainCameraRect => _mainCameraRect;

    public Camera mainCamera => _mainCameraObject;

    public Transform mainCameraTransform => _mainCameraTransform;

    public MonitorCameraManager monitorCameraManager => _monitorCameraManager;

    public bool isDisplayGUI
    {
        get
        {
            return _isDisplayGUI;
        }
        set
        {
            _isDisplayGUI = value;
        }
    }

    public bool isDisplayGUIChangeButton
    {
        get
        {
            return _isDisplayGUIChangeButton;
        }
        set
        {
            _isDisplayGUIChangeButton = value;
        }
    }

    public bool isDisplayOnlyFPS
    {
        get
        {
            return _isDisplayOnlyFPS;
        }
        set
        {
            _isDisplayOnlyFPS = value;
        }
    }

    public bool isDebugPose
    {
        get
        {
            return _isDebugPose;
        }
        set
        {
            _isDebugPose = value;
        }
    }

    public Live3DSettings live3DSettings => _live3DSettings;

    public List<PostEffectLive3D> imageEffectLive3dList => _imageEffectLive3dList;

    public ScreenFade[] screenFadeArray => _screenFadeArray;

    //public bool isUseAsssetBundle => _isUseAsssetBundle;

    public List<string> lstAssetBundleName => _lstAssetBundleName;

    public int assetTaskCount => _qAssetTask.Count;

    public eAssetTaskState assetTaskState => _assetTaskState;

    public bool isTimelineControlled
    {
        get
        {
            if (_liveTimelineControl != null)
            {
                return _liveTimelineControl.data != null;
            }
            return false;
        }
    }

    public int liveTotalTime
    {
        get
        {
            int result = 180;
            if (isTimelineControlled)
            {
                result = _liveTimelineControl.data.timeLength + 5;
            }
            return result;
        }
    }

    /// <summary>
    /// DereViewer用に追加
    /// </summary>
    private CharaDirector[] _charaDirector = new CharaDirector[15];

    /// <summary>
    /// DereViewer用に追加
    /// </summary>
    private LiveDirector _liveDirector;

    /// <summary>
    /// 追加
    /// リッチモード
    /// </summary>
    private bool _isRich = true;

    public bool isRich
    {
        get
        {
            return _isRich;
        }
    }

    private Vector2Int CalcResolusion()
    {
        Vector2Int vector2 = default(Vector2Int);

        int resRatio = SaveManager.GetInt("ResolutionRatio", 0);
        float ratio = 1f;
        switch (resRatio)
        {
            case 0:
                ratio = 1.0f;
                break;
            case 1:
                ratio = 1.4142f;
                break;
            case 2:
                ratio = 1.7320f;
                break;
            case 3:
                ratio = 2.0f;
                break;
        }

        float width = Screen.width * ratio;
        float height = Screen.height * ratio;

        vector2.x = Mathf.FloorToInt(width);
        vector2.y = Mathf.FloorToInt(height);
        /*
        //大きいほうを取得
        float wide = width > height ? width : height;

        //4Kの横サイズより大きければリサイズ
        float resizeRatio = 1f;
        if (wide > 3840f)
        {
            resizeRatio = 3840f / wide;
            resizeRatio = resizeRatio < 1 ? resizeRatio : 1f; //デフォルトで4K以上のモニターなら1で…
            vector2.x = Mathf.FloorToInt(width * resizeRatio);
            vector2.y = Mathf.FloorToInt(height * resizeRatio);
        }
        else
        {
            vector2.x = Mathf.FloorToInt(width);
            vector2.y = Mathf.FloorToInt(height);
        }
        */
        return vector2;
    }

    /// <summary>
    /// セーブからフレームレートの設定を行う
    /// </summary>
    private void SettingFrameRate()
    {
        //フレームレートの設定を行う
        int DontSync = SaveManager.GetInt("vSyncSetting", 1); //デフォルトは垂直同期無し
        if (DontSync == 1)
        {
            QualitySettings.vSyncCount = 0;
            int FrameRate = SaveManager.GetInt("FrameRate", 1); //デフォルト60
            switch (FrameRate)
            {
                case 0:
                    Application.targetFrameRate = 30;
                    break;
                case 1:
                    //Every VBlank(60fps)
                    Application.targetFrameRate = 60;
                    break;
                case 2:
                    //Every VBlank(120fps)
                    Application.targetFrameRate = 120;
                    break;
            }
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            int FrameRate = SaveManager.GetInt("FrameRate", 1); //デフォルト60
            switch (FrameRate)
            {
                case 0:
                    //Every Second VBlank(60fpsの半分)
                    QualitySettings.vSyncCount = 2;
                    Application.targetFrameRate = 60;
                    break;
                case 1:
                    //Every VBlank(60fps)
                    Application.targetFrameRate = 60;
                    break;
                case 2:
                    //Every VBlank(120fps)
                    Application.targetFrameRate = 120;
                    break;
            }
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Object.Destroy(this);
            return;
        }

        if (_screenOriginWidth == 0 || _screenOriginHeight == 0)
        {
            var vec = CalcResolusion();
            _screenOriginWidth = vec.x;
            _screenOriginHeight = vec.y;
        }

        _isBootDirect = true;

        //フレームレートの設定
        SettingFrameRate();

        DecideQualityLevel();
        ApplyQualityLevel();
        if (_isBootDirect)
        {
            CharacterObject.disableErrorCheck = false;
        }
        else
        {
            /*
            CharacterObject.disableErrorCheck = true;
            MasterDataManager masterDataManager = SingletonMonoBehaviour<MasterDataManager>.instance;
            SingletonMonoBehaviour<TempData>.instance.dressTempData.CreateDressLists(this, masterDataManager.masterDressData, masterDataManager.masterDressTarget2);
            */
        }

        _instance = this;
    }
    private IEnumerator Start()
    {
        CameraSharedCachePool.Initialize();

        if (_randomTable != null)
        {
            _randomTable.Build(RandomTable.DEFAULT_BUCKET_COUNT, RandomTable.DEFAULT_STACK_SIZE, (int idxBucket, int idxStack) => UnityEngine.Random.value);
        }
        PostEffectLive3D.enableScreenScale = false;

        _mainCameraRect = new Rect(0f, 0f, 1f, 1f);
        _useCalcedCameraRect = false;

        yield return fnLoadAssetBundlePrepareTimelineData();

        yield return StartCoroutine(LoadLiveDirectTask());

        GC.Collect();
    }

    private void Update()
    {
        if (assetTaskState != eAssetTaskState.Done)
        {
            return;
        }
        //_isInitializedがFales＝読み込みまちのとき
        if (!_isInitialized)
        {
            _mainCameraObject.gameObject.SetActive(value: false);
            if (_charaClothResetTimer > 0)
            {
                switch (_charaClothResetTimer)
                {
                    case 10:
                        {
                            bool flag = true;
                            if (base.gameObject.activeSelf && flag && _useCalcedCameraRect)
                            {
                                SetCameraRect(_mainCameraRect);
                            }
                            break;
                        }
                    case 9:
                        _tmInitializeCamera.position = Vector3.forward * 50f;
                        _tmInitializeCamera.LookAt(Vector3.zero);
                        break;
                    case 8:
                        _tmInitializeCamera.position = Vector3.up * 50f;
                        _tmInitializeCamera.LookAt(Vector3.zero);
                        break;
                    case 7:
                        _tmInitializeCamera.position = Vector3.right * 50f;
                        _tmInitializeCamera.LookAt(Vector3.zero);
                        break;
                    case 6:
                        _tmInitializeCamera.position = Vector3.left * 50f;
                        _tmInitializeCamera.LookAt(Vector3.zero);
                        break;
                    case 5:
                        _tmInitializeCamera.position = Vector3.down * 50f;
                        _tmInitializeCamera.LookAt(Vector3.zero);
                        _liveTimelineControl.StartUpdate();
                        base.gameObject.GetComponentInChildren<StageController>().SetStartDisableRenderer();
                        break;
                    case 4:
                        _initializeCamera.SetActive(value: false);
                        UnityEngine.Object.Destroy(_initializeCamera);
                        _initializeCamera = null;
                        _tmInitializeCamera = null;
                        break;
                    case 2:
                        {
                            int playSongId = GetPlaySongId();
                            int targetFrameRate = Application.targetFrameRate;

                            CySpring.SetAttenuationFactor(targetFrameRate, 1.7f, 2.05f); //物理演算にFPSを入力
                            for (int i = 0; i < _characterObjects.Count; i++)
                            {
                                if (_characterObjects[i] != null)
                                {
                                    _characterObjects[i].CheckAndSetSpringValue(playSongId, targetFrameRate, 2.05f);
                                    _characterObjects[i].CySpringForceUpdate(0.2f, 1.8f);
                                    _characterObjects[i].isEnabledAlterUpdateSelf = false;
                                }
                            }
                            foreach (KeyValuePair<int, CharacterObject[]> item in _mapSpareCharacter)
                            {
                                CharacterObject[] value = item.Value;
                                foreach (CharacterObject obj in value)
                                {
                                    obj.CheckAndSetSpringValue(playSongId, targetFrameRate, 2.05f);
                                    obj.CySpringForceUpdate(0.2f, 1.8f);
                                    obj.isEnabledAlterUpdateSelf = false;
                                }
                            }
                            break;
                        }
                }
                _charaClothResetTimer--;
                return;
            }

            _initializeTimer += Time.deltaTime; //0.3秒待機

            if (_initializeTimer > 0.3f)
            {
                /*
                if (!SingletonMonoBehaviour<LiveController>.IsInstanceEmpty())
                {
                    SingletonMonoBehaviour<LiveController>.instance.LoadComplete(is2D: false);
                }
                */
                _isInitialized = true; //ここで読み込み完了
            }
        }
        else
        {
            if (_randomTable != null && !IsPauseLive())
            {
                _randomTable.Update();
            }
            UpdateSmoothMusicScoreTime();
            if (_liveTimelineControl != null)
            {
                _liveTimelineControl.isEnablePostEffect = _isBloom || _isDof;
                _liveTimelineControl.AlterUpdate(musicScoreTime);
            }
            if (_uiCamera != null)
            {
                _uiCamera.clearFlags = CameraClearFlags.Depth;
            }
            UpdateMainCamera();
            if (_cameraLookAt != null && isTimelineControlled)
            {
                _cameraLookAt.AlterUpdate();
            }
            if (_lyricsManager != null)
            {
                _lyricsManager.UpdateTime(instance.musicScoreTime);
            }
        }
    }

    private void LateUpdate()
    {
        if (assetTaskState != eAssetTaskState.Done)
        {
            return;
        }
        if (CameraSharedCachePool.instance != null)
        {
            CameraSharedCachePool.instance.Fetch();
        }
        for (int i = 0; i < _characterObjects.Count; i++)
        {
            CharacterObject characterObject = _characterObjects[i];
            if (characterObject.spareCharacters != null)
            {
                CharacterObject[] spareCharacters = characterObject.spareCharacters;
                foreach (CharacterObject obj in spareCharacters)
                {
                    obj.bodyRoot.position = characterObject.bodyRoot.position;
                    obj.AlterLateUpdate();
                }
            }
            characterObject.AlterLateUpdate();
        }
        if (_isInitialized)
        {
            if ((bool)mainCamera)
            {
                mainCamera.clearFlags = CameraClearFlags.Color;
            }
            if (_liveTimelineControl != null)
            {
                _liveTimelineControl.AlterLateUpdate();
            }
            Shader.SetGlobalColor(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalEnvColor), _GlobalEnvColor);
            Shader.SetGlobalTexture(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalEnvTex), _GlobalEnvTex);
            Shader.SetGlobalFloat(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalEnvRate), _GlobalEnvRate);
            Shader.SetGlobalVector(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalLightDir), _GlobalLightDir.normalized);
            Shader.SetGlobalFloat(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalRimRate), _GlobalRimRate);
            Shader.SetGlobalFloat(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalRimShadowRate), _GlobalRimShadowRate);
            Shader.SetGlobalFloat(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalSpecRate), _GlobalSpecularRate);
            Shader.SetGlobalFloat(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.GlobalToonRate), _GlobalToonRate);
            Physics.SyncTransforms();
        }
    }

    private void FixedUpdate()
    {
        if (assetTaskState != eAssetTaskState.Done || !_isInitialized)
        {
            return;
        }
        for (int i = 0; i < _characterObjects.Count; i++)
        {
            _characterObjects[i].AlterFixedUpdate();
        }
        foreach (KeyValuePair<int, CharacterObject[]> item in _mapSpareCharacter)
        {
            CharacterObject[] value = item.Value;
            for (int j = 0; j < value.Length; j++)
            {
                value[j].AlterFixedUpdate();
            }
        }
    }

    private void OnDestroy()
    {
        CameraSharedCachePool.Terminate();
        if (_colorCorrectionParameter != null)
        {
            _colorCorrectionParameter.Destroy();
            _colorCorrectionParameter = null;
        }
        DeleteTimelineDelegetes();
        if (_multiCameraManager != null)
        {
            UnityEngine.Object.Destroy(_multiCameraManager.gameObject);
            _multiCameraManager = null;
        }
        UninitializeTransparentCamera();
        UninitializeCrossFadeCamera();
        if (ResourcesManager.instance != null)
        {
            foreach (string item in _lstAssetBundleName)
            {
                //ResourcesManager.instance.RemoveAsset(item);
            }
        }
        _instance = null;
        /*
        if (!SingletonMonoBehaviour<TempData>.IsInstanceEmpty())
        {
            SingletonMonoBehaviour<TempData>.instance.dressTempData.DestroyDressData(this);
        }
        */
        if (_renderTarget != null)
        {
            _renderTarget.Release();
            _renderTarget = null;
        }
    }

    public void SetEnableTimelineGlobalRimParam(bool enable)
    {
        _enableTimelineGlobalRimParam = enable;
    }

    public void SetGlobalLightingParam(float globalRimRate, float globalRimShadowRate, float globalSpecularRate, float globalToonRate, float globalEnvRate)
    {
        _GlobalRimRate = globalRimRate;
        _GlobalRimShadowRate = globalRimShadowRate;
        _GlobalSpecularRate = globalSpecularRate;
        _GlobalToonRate = globalToonRate;
        _GlobalEnvRate = globalEnvRate;
    }

    public void SetGlobalEnvTex(Texture globalEnvTex)
    {
        _GlobalEnvTex = globalEnvTex;
    }


    public eQualityType GetQualityType()
    {
        if (_qualityType != eQualityType.Quality_Unknown)
        {
            return _qualityType;
        }
        _qualityType = eQualityType.Quality3D_Rich;

        return _qualityType;
    }

    private QualityLevel DecideQualityLevel()
    {
        if (_debugQualityType != 0)
        {
            _qualityLevel = _debugQualityType;
            return _qualityLevel;
        }
        _qualityLevel = QualityLevel.Lv9_Rich;
        return _qualityLevel;
    }

    private QualityLevel DecideDofQualityLevel()
    {
        QualityLevel result = QualityLevel.Lv8_OutlineDof;
        if (_screenOriginWidth > 1920 || _screenOriginHeight > 1920)
        {
            result = QualityLevel.Lv7_OutlineDof1280;
        }
        else if (_screenOriginWidth > 1280 || _screenOriginHeight > 1280)
        {
            result = QualityLevel.Lv7_OutlineDof1280;
        }
        return result;
    }

    private void ApplyShaderQualityLevel()
    {
        QualityLevel qualityLevel = _qualityLevel;
        if (qualityLevel != QualityLevel.Lv1_Outline75)
        {
            _ = 2;
        }
    }

    private PostEffectLive3D.FilterType GetFilter()
    {
        /*
        TempData.LiveTempData liveTemp = SingletonMonoBehaviour<TempData>.instance.liveTemp;
        LocalData localData = SingletonMonoBehaviour<LocalData>.instance;
        LocalData.OptionData.eQualityType qualityTypeForStage = GetQualityTypeForStage();
        PostEffectLive3D.FilterType result = (liveTemp.isMV ? localData.option.mv_image_filter : ((qualityTypeForStage == LocalData.OptionData.eQualityType.Quality3D || qualityTypeForStage == LocalData.OptionData.eQualityType.Quality3D_Rich) ? PostEffectLive3D.FilterType.DofBloom : PostEffectLive3D.FilterType.NoEffect));
        if (!liveTemp.isMV && _liveTimelineControl != null)
        {
            if (LiveUtils.IsRich())
            {
                if (_liveTimelineControl.data.gamePlaySettings.isRichDofBloomOff)
                {
                    result = PostEffectLive3D.FilterType.NoEffect;
                }
            }
            else if (_liveTimelineControl.data.gamePlaySettings.isDofBloomOff)
            {
                result = PostEffectLive3D.FilterType.NoEffect;
            }
        }
        */
        PostEffectLive3D.FilterType result = PostEffectLive3D.FilterType.DofBloom;

        return result;
    }

    private void ApplyQualityLevel()
    {
        /*
        switch (_qualityLevel)
        {
            case QualityLevel.Lv1_Outline75:
                _isBloom = false;
                _isDof = false;
                break;
            case QualityLevel.Lv2_Outline:
                _isBloom = false;
                _isDof = false;
                break;
            case QualityLevel.Lv3_OutlineBloom50:
                _isBloom = _isEnableBloom;
                _isDof = false;
                break;
            case QualityLevel.Lv4_OutlineBloom75:
                _isBloom = _isEnableBloom;
                _isDof = false;
                break;
            case QualityLevel.Lv5_OutlineBloom100:
                _isBloom = _isEnableBloom;
                _isDof = false;
                break;
            case QualityLevel.Lv6_OutlineDof75:
                _isBloom = _isEnableBloom;
                _isDof = true;
                break;
            case QualityLevel.Lv7_OutlineDof1280:
                _isBloom = _isEnableBloom;
                _isDof = true;
                break;
            case QualityLevel.Lv8_OutlineDof:
            case QualityLevel.Lv9_Rich:
                _isBloom = _isEnableBloom;
                _isDof = true;
                break;
            default:
                _isBloom = false;
                _isDof = false;
                break;
        }
        */
        _isBloom = _isEnableBloom;
        _isDof = true;
        _isForcePostEffectDisable = false;
        /*
        if (QualityManager.GetGPUQualityLevel() == QualityManager.GPUQualityLevel.Level_1)
        {
            _isDof = false;
            _isBloom = false;
            if (_isBootDirect)
            {
                _isForcePostEffectDisable = false;
            }
            else
            {
                _isForcePostEffectDisable = true;
            }
        }
        int num = 1 | LayerMask.GetMask("Background");
        num |= StageUtil.Background3dAllLayers() | StageUtil.CharaAllLayers();        
        */

        int num = 1; //default
        num |= LayerMask.GetMask("Background");
        num |= StageUtil.Background3dAllLayers();
        num |= StageUtil.CharaAllLayers();

        if (_cameraObjects == null)
        {
            _cameraObjects = new Camera[_cameraNodes.Length + 1];
            _cameraTransforms = new Transform[_cameraNodes.Length + 1];
            for (int i = 0; i < _cameraNodes.Length; i++)
            {
                GameObject gameObject = _cameraNodes[i];
                Camera camera = gameObject.GetComponent<Camera>();
                if (camera == null)
                {
                    camera = gameObject.GetComponentInChildren<Camera>();
                }
                camera.cullingMask = num;
                _cameraObjects[i] = camera;
                _cameraTransforms[i] = camera.transform;
            }
        }
        UpdateMainCamera();
        if (_mainCameraObject != null)
        {
            List<PostEffectLive3D> list = new List<PostEffectLive3D> { _mainCameraObject.GetComponent<PostEffectLive3D>() };
            PostEffectLive3D postEffect = null;

            _indirectLightShuft = new PostEffectLive3D.IndirectLightShafts[kTimelineCameraIndices.Length];
            _colorCorrectionCurves = new PostEffectLive3D.ColorCorrection[kTimelineCameraIndices.Length];
            _colorCorrectionParameter = new PostEffectLive3D.ColorCorrectionCurvesParameter();

            for (int j = 0; j < kTimelineCameraIndices.Length; j++)
            {
                int num2 = kTimelineCameraIndices[j];
                if (num2 < _cameraObjects.Length)
                {
                    PostEffectLive3D component = _cameraObjects[num2].GetComponent<PostEffectLive3D>();
                    list.Add(component);
                    _indirectLightShuft[j] = component.indirectLightShafts;
                    _colorCorrectionCurves[j] = component.colorCorrection;
                    component.colorCorrection.parameter = _colorCorrectionParameter;
                    _indirectLightShuft[j].enabled = false;
                    component.colorCorrection.enabled = false;
                }
            }
            for (int k = 0; k < list.Count; k++)
            {
                PostEffectLive3D postEffectLive3D = list[k];
                if (postEffectLive3D != null && postEffectLive3D.isMainCamera)
                {
                    postEffect = postEffectLive3D;
                    break;
                }
            }
            PostEffectLive3D.FilterType filter = GetFilter();
            for (int l = 0; l < list.Count; l++)
            {
                PostEffectLive3D postEffectLive3D = list[l];
                if (postEffectLive3D != null)
                {
                    postEffectLive3D.InitParam(postEffect);
                    postEffectLive3D.filterType = filter;
                    _imageEffectLive3dList.Add(postEffectLive3D);
                    postEffectLive3D.isDof = _isDof;
                    postEffectLive3D.isEnableBloom = _isBloom;
                    postEffectLive3D.enabled = _isDof || _isBloom;
                }
            }
            if (_crossFadeCamera != null)
            {
                SetupCrossFadeCameraImageEffect();
            }
        }
        if (Application.isPlaying)
        {
            _initializeCamera = new GameObject("initialize camera");
            Camera camera2 = _initializeCamera.AddComponent<Camera>();
            camera2.transform.SetParent(base.transform, worldPositionStays: false);
            camera2.cullingMask = num;
            _mainCameraObject.gameObject.SetActive(value: false);
            _tmInitializeCamera = _initializeCamera.transform;
        }
    }

    private eQualityType GetUseReplaceQuality(int replaceQuality)
    {
        eQualityType qualityTypeForStage = eQualityType.Quality3D_Rich;
        replaceQuality <<= 2;
        if ((replaceQuality & (1 << (int)qualityTypeForStage)) > 0)
        {
            return qualityTypeForStage;
        }
        return eQualityType.Quality3D;
    }
    public float musicScoreTimeNormalized(float time)
    {
        return musicScoreTime / time;
    }

    public float CalcFrameJustifiedMusicTime()
    {
        if (isTimelineControlled)
        {
            return Mathf.RoundToInt(musicScoreTime * 60f) / 60f;
        }
        return musicScoreTime;
    }

    public enum eScoreRank
    {
        None,
        Rank_C,
        Rank_B,
        Rank_A,
        Rank_S,
        Max
    }

    private void UpdateSmoothMusicScoreTime()
    {
        float num = directMusicScoreTime - _lastMusicScoreWithoutDelay;
        _lastMusicScoreWithoutDelay = directMusicScoreTime;
        _lastMusicScoreTime = musicScoreTime;
        float f = _smoothMusicScoreTime - directMusicScoreTime;
        bool flag = _enableSmoothMusicTime;
        if (flag && Mathf.Abs(f) > 0.1f)
        {
            flag = false;
        }
        if (flag)
        {
            float num2 = num;
            float num3 = Mathf.Clamp(num - kSmoothingTargetDeltaTime, 0f - kSmoothingOvertimeClamp, kSmoothingOvertimeClamp);
            if (Mathf.Abs(num3) > kSmoothingDeltaTimeThreshold)
            {
                if (num3 > 0f)
                {
                    if (_smoothingWorkTime < kSmoothingDeltaTimeThreshold)
                    {
                        _smoothingWorkTime += num3;
                        num2 = num - num3;
                    }
                }
                else if (_smoothingWorkTime > 0f - kSmoothingDeltaTimeThreshold)
                {
                    _smoothingWorkTime += num3;
                    num2 = num - num3;
                }
            }
            _smoothMusicScoreTime += num2;
        }
        else
        {
            _smoothMusicScoreTime = directMusicScoreTime;
            _smoothingWorkTime = 0f;
        }
    }

    public Character3DBase.CharacterData GetCharacterData(int index)
    {
        if (index <= 0)
        {
            return null;
        }
        if (index > MemberUnitNumber)
        {
            return null;
        }
        return _charaDirector[index - 1].characterData;
    }

    public CharacterObject getCharacterObjectFromPositionID(LiveCharaPosition id)
    {
        if ((int)id >= _characterObjects.Count)
        {
            return null;
        }
        return _characterObjects[(int)id];
    }

    public int GetCharaFaceType(int charaIndex, int srcType, int faceSlotIdx, out Master3DCharaData.CharaDataSpecialFace.eCheek cheek)
    {
        if (_master3DCharaData == null)
        {
            cheek = Master3DCharaData.CharaDataSpecialFace.eCheek.Default;
            return 0;
        }
        Master3DCharaData.CharaDataWork charaDataWork = _charaDataWorks[charaIndex];
        if (charaDataWork == null)
        {
            cheek = Master3DCharaData.CharaDataSpecialFace.eCheek.Default;
            return 0;
        }
        if (charaDataWork.cacheFindId == srcType && charaDataWork.cacheFaceSlotIdx == faceSlotIdx)
        {
            cheek = charaDataWork.cacheCheek;
            return charaDataWork.cacheFaceId;
        }
        int num = 0;
        int[] value = null;
        cheek = Master3DCharaData.CharaDataSpecialFace.eCheek.Default;
        Master3DCharaData.CharaDataSpecialFace.eCheek[] value2 = null;
        if (charaDataWork.cacheSplFaceDictionary != null)
        {
            if (charaDataWork.cacheSplFaceDictionary.TryGetValue(srcType, out value))
            {
                num = value[faceSlotIdx];
            }
            if (num == 0)
            {
                num = _master3DCharaData.GetDefaultFaceID(srcType);
                cheek = Master3DCharaData.CharaDataSpecialFace.eCheek.Default;
            }
            else if (charaDataWork.cacheSplCheekDictionary.TryGetValue(srcType, out value2))
            {
                cheek = value2[faceSlotIdx];
            }
        }
        charaDataWork.cacheFindId = srcType;
        charaDataWork.cacheFaceSlotIdx = faceSlotIdx;
        charaDataWork.cacheFaceId = num;
        charaDataWork.cacheCheek = cheek;
        return num;
    }

    public int GetOverridedFacialId(int charaIndex, int faceId)
    {
        return _characterObjects[charaIndex].GetOverridedFacialId(faceId);
    }

    public bool GetCharaCheekFlag(int charaIndex, int dressId)
    {
        if (_charaDataWorks[charaIndex] == null)
        {
            return false;
        }
        return _charaDataWorks[charaIndex].CheckUsableCheek(dressId);
    }

    public void CharacterRequest<REQUEST>(CharacterObject charaObject, Action<REQUEST, Character3DBase.IRequestExecutor> fnInscription) where REQUEST : Character3DBase.IRequest
    {
        if (charaObject != null && fnInscription != null)
        {
            charaObject.Request(fnInscription);
        }
    }

    public void CharacterRequest<REQUEST>(int charaIndex, Action<REQUEST, Character3DBase.IRequestExecutor> fnInscription) where REQUEST : Character3DBase.IRequest
    {
        if (_characterObjects[charaIndex] != null)
        {
            CharacterRequest(_characterObjects[charaIndex], fnInscription);
        }
    }

    public void CharacterRequest<REQUEST>(LiveCharaPosition id, Action<REQUEST, Character3DBase.IRequestExecutor> fnInscription) where REQUEST : Character3DBase.IRequest
    {
        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID(id);
        CharacterRequest(characterObjectFromPositionID, fnInscription);
    }

    public void CharacterRequest<REQUEST>(Action<REQUEST, Character3DBase.IRequestExecutor> fnInscription) where REQUEST : Character3DBase.IRequest
    {
        foreach (CharacterObject characterObject in _characterObjects)
        {
            CharacterRequest(characterObject, fnInscription);
        }
    }

    /// <summary>
    /// 楽曲IDを取得する
    /// </summary>
    public int GetPlaySongId()
    {
        int num = ViewLauncher.instance.liveDirector.MusicID;
        /*
        if (_isBootDirect)
        {
            return _live3DSettings._debugSongId;
        }
        TempData.LiveTempData liveTemp = SingletonMonoBehaviour<TempData>.instance.liveTemp;
        MasterLiveData.LiveData liveData = ((!GameDefine.REPLACE_MV_ENABLE) ? MasterDBManager.instance.masterLiveData.dictionary[liveTemp._liveId] : MasterDBManager.instance.masterLiveData.dictionary[SingletonMonoBehaviour<LiveController>.instance.GetLiveId(LiveDefine.eReplaceLiveIdType.After)]);
        return liveData.musicDataId;
        */
        return num;
    }

    public bool IsPauseLive()
    {
        //print("IsPauseLive");
        //return true;
        //return SingletonMonoBehaviour<AudioManager>.IsInstanceEmpty() || SingletonMonoBehaviour<AudioManager>.instance.IsPauseSong();
        return musicManager.isPause;
    }

    public void EffectBind(bool bBind, int iCharaPos)
    {
        List<EffectController> effectControllerList = _stageController.effectControllerList;
        if (effectControllerList != null)
        {
            for (int i = 0; i < effectControllerList.Count; i++)
            {
                effectControllerList[i].Bind(bBind, iCharaPos);
            }
        }
    }

    public void SwapSpareCharacter(int charaIndex, Character3DBase.CharacterData.eDressType dressType)
    {
        if (charaIndex < 0 || _characterObjects.Count <= charaIndex)
        {
            return;
        }
        CharacterObject characterObject = _characterObjects[charaIndex];
        CharacterObject[] value = null;
        if (!_mapSpareCharacter.TryGetValue(charaIndex, out value) || value == null || characterObject.data.dressType == dressType)
        {
            return;
        }
        int num = -1;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i].data.dressType == dressType)
            {
                num = i;
            }
        }
        if (num != -1)
        {
            _propsManager.ResetAttach(charaIndex);
            EffectBind(bBind: false, charaIndex);
            _characterObjects[charaIndex] = value[num];
            _mapSpareCharacter[charaIndex][num] = characterObject;
            characterObject = _characterObjects[charaIndex];
            CharacterObject obj = _mapSpareCharacter[charaIndex][num];
            obj.AppointSpareCharacter(isSpare: true);
            bool enable = true;
            characterObject.EnableFootShadow(enable);
            obj.EnableFootShadow(enable: false);
            _propsManager.Reattach(charaIndex, characterObject.gameObject, characterObject, characterObject.createInfo.charaData.heightId);
            _propsManager.SetScale(charaIndex, characterObject.bodyScaleSubScale);
            _liveTimelineControl.ChangeCharacterLocator(characterObject.liveCharaStandingPosition, characterObject);
            EffectBind(bBind: true, charaIndex);
            characterObject.AppointSpareCharacter(isSpare: false);
        }
    }


    public eScoreRank GetCurrentScoreRank()
    {
        /*
        if (!SingletonMonoBehaviour<LiveController>.IsInstanceEmpty())
        {
            return SingletonMonoBehaviour<LiveController>.instance.GetCurrentScoreRank();
        }
        */
        return eScoreRank.Rank_S;
    }

    public void SetCameraRect(Rect rect)
    {
        if (_cameraNodes == null)
        {
            return;
        }
        if (_finalizeCamera != null)
        {
            _finalizeCamera.rect = rect;
            return;
        }
        for (int i = 0; i < _cameraNodes.Length; i++)
        {
            Camera component = _cameraNodes[i].GetComponent<Camera>();
            if (!(component == null))
            {
                component.rect = rect;
            }
        }
    }

    public List<Camera> MakeCameraList()
    {
        List<Camera> list = new List<Camera>(8);
        Camera[] cameraObjects = _cameraObjects;
        foreach (Camera camera in cameraObjects)
        {
            if (camera != null)
            {
                list.Add(camera);
            }
        }
        if (_crossFadeCamera != null)
        {
            list.Add(_crossFadeCamera.GetCamera());
        }
        if (_multiCameraManager != null && _multiCameraManager.MultiCameraArray != null)
        {
            MultiCamera[] multiCameraArray = _multiCameraManager.MultiCameraArray;
            foreach (MultiCamera multiCamera in multiCameraArray)
            {
                list.Add(multiCamera.GetCamera());
            }
        }
        return list;
    }

    private IEnumerator InitMainCameraRenderTarget()
    {
        if (_finalizeCamera != null)
        {
            yield break;
        }
        /*
        int width = (int)((float)Screen.width * _mainCameraRect.width);
        int height = (int)((float)Screen.height * _mainCameraRect.height);
        */
        int width = _screenOriginWidth;
        int height = _screenOriginHeight;
        _renderTarget = new RenderTarget(width, height, GetQualityType());
        _finalizeCamera = base.gameObject.AddComponent<FinalizeCamera>();
        yield return null;
        foreach (PostEffectLive3D imageEffectLive3d in _imageEffectLive3dList)
        {
            if (imageEffectLive3d != null)
            {
                imageEffectLive3d.enabled = _renderTarget.CheckCapability(RenderTarget.eCapability.PostEffect) && imageEffectLive3d.enabled;
                imageEffectLive3d.SetCameraRenderTarget(_renderTarget.color, _renderTarget.depth, _renderTarget.result, _renderTarget.monitor);
            }
        }

        float dimmerAlpha = 0.5f;
        FinalizeCamera.Param param = new FinalizeCamera.Param
        {
            //useDimmer = ((bool)isInsertDimmer && !isMV),
            useDimmer = false,
            usePostEffect = _renderTarget.CheckCapability(RenderTarget.eCapability.PostEffect),
            useMonitor = _renderTarget.CheckCapability(RenderTarget.eCapability.Monitor)
        };
        yield return _finalizeCamera.Initialize(_renderTarget, param, dimmerAlpha);
        yield return SetupMultiCamera();
        SetupCrossFadeCamera();
    }

    public void SetScreenShotTexture(RenderTexture color, RenderTexture depth, RenderTexture result)
    {
        Shader.SetGlobalTexture("_gColorTexture", color);
        Shader.SetGlobalTexture("_CameraDepthTexture", depth);
        Shader.SetGlobalTexture("_gResultTexture", result);
        foreach (var posteffect in _imageEffectLive3dList)
        {
            posteffect.SetCameraRenderTarget(color, depth, result, _renderTarget.monitor);
        }
    }

    public void ReSetRenderTexture()
    {
        Shader.SetGlobalTexture("_gColorTexture", _renderTarget.color);
        Shader.SetGlobalTexture("_CameraDepthTexture", _renderTarget.depth);
        Shader.SetGlobalTexture("_gResultTexture", _renderTarget.result);
        foreach (var tmp in _imageEffectLive3dList)
        {
            tmp.SetCameraRenderTarget(_renderTarget.color, _renderTarget.depth, _renderTarget.result, _renderTarget.monitor);
        }
    }


    public bool IsMonitorCameraOn()
    {
        /*
        if (!LiveUtils.IsRich())
        {
            return false;
        }
        LiveTimelineGamePlaySettings gamePlaySettings = _liveTimelineControl.data.gamePlaySettings;
        if (!LiveUtils.IsMV() && gamePlaySettings.isMonitorCameraOff)
        {
            return false;
        }
        */
        return true;
    }

    public bool IsMirrorScanOn()
    {
        /*
        LiveTimelineGamePlaySettings gamePlaySettings = _liveTimelineControl.data.gamePlaySettings;
        if (!LiveUtils.IsMV() && gamePlaySettings.isMirrorScanOff)
        {
            return false;
        }
        */
        return true;
    }

    public IEnumerator InitializeMonitorCamera()
    {
        if (IsMonitorCameraOn())
        {
            if (_monitorCameraManager == null)
            {
                _monitorCameraManager = new MonitorCameraManager();
            }
            LiveTimelineMonitorCameraSettings monitorCameraSettings = _liveTimelineControl.data.monitorCameraSettings;
            int cntCamera = monitorCameraSettings.cntCamera;
            float renderTextureWidth = monitorCameraSettings.renderTextureWidth;
            float renderTextureHeight = monitorCameraSettings.renderTextureHeight;
            //Vector2 rtResolution = StageUtil.GetQuarterResolution();
            Vector2 rtResolution = CalcResolusion();
            rtResolution.x = rtResolution.x * 0.5f;
            rtResolution.y = rtResolution.y * 0.5f;
            if (renderTextureWidth >= 128f && renderTextureHeight == 0f)
            {
                //rtResolution = StageUtil.CalcRTResolution(renderTextureWidth);
                float num = rtResolution.x / rtResolution.y;
                float y = renderTextureWidth / num;
                rtResolution = new Vector2(renderTextureWidth, y);
            }
            else if (renderTextureWidth >= 128f && renderTextureHeight >= 128f)
            {
                if (renderTextureWidth > rtResolution.x) rtResolution.x = renderTextureWidth;
                if (renderTextureHeight > rtResolution.y) rtResolution.y = renderTextureHeight;
                //rtResolution.x = renderTextureWidth;
                //rtResolution.y = renderTextureHeight;
            }
            _liveTimelineControl.SetMonitorCameras(null);
            if (_monitorCameraManager.monitorCameras != null)
            {
                foreach (CharacterObject characterObject in _characterObjects)
                {
                    Character3DBase.RenderController renderController = characterObject.renderController;
                    MonitorCamera[] monitorCameras = _monitorCameraManager.monitorCameras;
                    foreach (MonitorCamera monitorCamera in monitorCameras)
                    {
                        renderController.UnregisterCamera(monitorCamera.targetCamera);
                    }
                }
            }
            yield return StartCoroutine(_monitorCameraManager.CreateMonitorCamera(cntCamera, base.transform, rtResolution));
            _liveTimelineControl.SetMonitorCameras(_monitorCameraManager.monitorCameras);
            if (_monitorCameraManager.monitorCameras != null)
            {
                MonitorCamera.MonitorCameraRenderCallback callback = OnMonitorRenderCallback;
                MonitorCamera[] monitorCameras = _monitorCameraManager.monitorCameras;
                foreach (MonitorCamera monitorCamera2 in monitorCameras)
                {
                    monitorCamera2.AddMonitorCameraCallback(callback);
                    foreach (CharacterObject characterObject2 in _characterObjects)
                    {
                        characterObject2.renderController.RegisterCamera(monitorCamera2.targetCamera);
                    }
                }
            }
        }
        _liveTimelineControl.OnUpdateMonitorCamera += delegate (ref MonitorCameraUpdateInfo info)
        {
            if (IsMonitorCameraOn() && _monitorCameraManager.countMonitorCamera > info.index)
            {
                _monitorCameraManager.SetCameraEnable(info.index, info.enable);
                _monitorCameraVisiblePositionFlag = info.visibleCharaFlag;
            }
        };
    }

    /// <summary>
    /// マルチカメラの初期化を行う
    /// </summary>
    public IEnumerator InitializeMultiCamera(int cameraNum)
    {
        if (_multiCameraManager == null)
        {
            GameObject multiCameraManager = new GameObject("MultiCameraManager");
            multiCameraManager.transform.SetParent(base.transform, worldPositionStays: false);
            _multiCameraManager = multiCameraManager.AddComponent<MultiCameraManager>();
            _multiCameraManager.BaseCameraDepth = -30;
        }
        yield return _multiCameraManager.Initialize(cameraNum);
    }

    public IEnumerator SetupMultiCamera()
    {
        if (!(_multiCameraManager == null))
        {
            yield return _multiCameraManager.Setup(_renderTarget.color, _renderTarget.depth);
            _liveTimelineControl.SetMultiCamera(_multiCameraManager, _multiCameraManager.MultiCameraArray);
            _multiCameraManager.InitializeFinish();
        }
    }

    /// <summary>
    /// 透過用カメラの初期化を行う
    /// </summary>
    private void InitializeTransparentCamera()
    {
        GameObject gameObject = new GameObject(TransparentCameraObjectName, typeof(Camera));
        Transform transform = gameObject.transform;
        transform.SetParent(base.transform, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        _transparentCameraTransform = transform;
        _transparentCamera = gameObject.GetComponent<Camera>();
        _transparentCamera.cullingMask = StageUtil.LayerToCullingMask(StageUtil.StageLayers.TransparentFX);
        _transparentCamera.clearFlags = CameraClearFlags.Nothing;
        _isEnableTransparentCamera = true;
    }

    private void UninitializeTransparentCamera()
    {
        if (_transparentCamera != null)
        {
            UnityEngine.Object.Destroy(_transparentCamera.gameObject);
            _transparentCamera = null;
        }
        _transparentCameraTransform = null;
        _isEnableTransparentCamera = false;
    }


    private void SetupCrossFadeCameraImageEffect()
    {
        PostEffectLive3D.FilterType filter = GetFilter();
        int count = _imageEffectLive3dList.Count;
        for (int i = 0; i < count; i++)
        {
            PostEffectLive3D postEffectLive3D = _imageEffectLive3dList[i];
            if (postEffectLive3D != null && postEffectLive3D.isMainCamera)
            {
                PostEffectLive3D postEffectLive3D2 = _crossFadeCamera.postEffectLive3D;
                postEffectLive3D2.InitParam(postEffectLive3D);
                postEffectLive3D2.filterType = filter;
                postEffectLive3D2.isDof = _isDof;
                postEffectLive3D2.isEnableBloom = _isBloom;
                postEffectLive3D2.enabled = _isDof || _isBloom;
            }
        }
    }

    private void UninitializeCrossFadeCamera()
    {
        if (_crossFadeCamera != null)
        {
            UnityEngine.Object.Destroy(_crossFadeCamera.gameObject);
            _crossFadeCamera = null;
        }
    }

    /// <summary>
    /// クロスフェードカメラの初期化を行う
    /// </summary>
    private void InitializeCrossFadeCamera()
    {
        GameObject gameObject = new GameObject("CrossFadeCamera");
        Camera camera = gameObject.AddComponent<Camera>();
        _crossFadeCamera = gameObject.AddComponent<CrossFadeCamera>();
        if (!_crossFadeCamera.Initialize())
        {
            UninitializeCrossFadeCamera();
            return;
        }
        gameObject.transform.SetParent(base.transform, worldPositionStays: false);
        camera.depth = -30f;
        camera.enabled = false;
        camera.cullingMask = StageUtil.Background3dAllLayers() | StageUtil.CharaAllLayers();
    }

    private void SetupCrossFadeCamera()
    {
        if (!(_crossFadeCamera == null))
        {
            _crossFadeCamera.Setup(_renderTarget.color, _renderTarget.depth, _renderTarget.monitor);
            Camera camera = _crossFadeCamera.GetCamera();
            _liveTimelineControl.InitializeCrossFadeCamera(new CacheCamera(camera));
            SetupCrossFadeCameraImageEffect();
        }
    }
    private void ResetCrossFadeCamera()
    {
        if (_liveTimelineControl != null)
        {
            _liveTimelineControl.InitializeCrossFadeCamera(null);
        }
        int count = _imageEffectLive3dList.Count;
        for (int i = 0; i < count; i++)
        {
            _imageEffectLive3dList[i].SetCrossFadeBlendTexture(null, 0f);
        }
    }

    private void UpdateMainCamera()
    {
        for (int i = 0; i < _cameraNodes.Length; i++)
        {
            bool activeSelf = _cameraNodes[i].activeSelf;
            bool flag = i == _activeCameraIndex;
            _cameraNodes[i].SetActive(flag);
            if (i == 0 && activeSelf != flag && flag && _cameraLookAt != null)
            {
                _cameraLookAt.ActivationUpdate();
            }
        }
        _mainCameraObject = _cameraObjects[_activeCameraIndex];
        _mainCameraTransform = _cameraTransforms[_activeCameraIndex];
        if (_isEnableTransparentCamera)
        {
            Camera transparentCamera = _transparentCamera;
            if (_activeCameraIndex == 0)
            {
                _transparentCameraTransform.localPosition = _mainCameraTransform.position;
                _transparentCameraTransform.localRotation = _mainCameraTransform.rotation;
            }
            else
            {
                _transparentCameraTransform.localPosition = _mainCameraTransform.localPosition;
                _transparentCameraTransform.localRotation = _mainCameraTransform.localRotation;
            }
            transparentCamera.CopyFrom(_mainCameraObject);
            transparentCamera.cullingMask = StageUtil.LayerToCullingMask(StageUtil.StageLayers.TransparentFX);
            transparentCamera.clearFlags = CameraClearFlags.Nothing;
            transparentCamera.depth = _mainCameraObject.depth + 10f;
        }
        for (int j = 0; j < _characterObjects.Count; j++)
        {
            if (!(_characterObjects[j] == null))
            {
                _characterObjects[j].mainCamera = _mainCameraObject;
            }
        }
        if (_stageController != null)
        {
            _stageController.ResetStageObjectsCamera(_mainCameraObject);
        }
    }

    private void OnMonitorRenderCallback(MonitorCamera monitorCamera, MonitorCamera.RenderCallbackType type)
    {
        switch (type)
        {
            case MonitorCamera.RenderCallbackType.PreCull:
                {
                    _restoreCharacterVisibleFlag = 0;
                    for (int j = 0; j < 15; j++)
                    {
                        LiveCharaPositionFlag liveCharaPositionFlag2 = (LiveCharaPositionFlag)(1 << j);
                        CharacterObject characterObjectFromPositionID2 = getCharacterObjectFromPositionID((LiveCharaPosition)j);
                        if (!(characterObjectFromPositionID2 == null) && characterObjectFromPositionID2.liveCharaVisible && (_monitorCameraVisiblePositionFlag & liveCharaPositionFlag2) == 0)
                        {
                            _restoreCharacterVisibleFlag |= liveCharaPositionFlag2;
                            characterObjectFromPositionID2.liveCharaVisible = false;
                            characterObjectFromPositionID2.UnregisterOutlineCommandBuffer(monitorCamera.targetCamera);
                        }
                    }
                    break;
                }
            case MonitorCamera.RenderCallbackType.PostRender:
                {
                    for (int i = 0; i < 15; i++)
                    {
                        LiveCharaPositionFlag liveCharaPositionFlag = (LiveCharaPositionFlag)(1 << i);
                        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID((LiveCharaPosition)i);
                        if (!(characterObjectFromPositionID == null) && (_restoreCharacterVisibleFlag & liveCharaPositionFlag) != 0)
                        {
                            characterObjectFromPositionID.liveCharaVisible = true;
                            characterObjectFromPositionID.RegisterOutlineCommandBuffer(monitorCamera.targetCamera);
                        }
                    }
                    break;
                }
        }
    }

    public int GetMotionCameraAnimeMemorySize()
    {
        if (_cameraRuntimeAnimator == null)
        {
            return 0;
        }
        long num = 0L;
        if (_cameraRuntimeAnimator.animationClips == null)
        {
            return 0;
        }
        num += Profiler.GetRuntimeMemorySizeLong(_cameraRuntimeAnimator);
        for (int i = 0; i < _cameraRuntimeAnimator.animationClips.Length; i++)
        {
            num += Profiler.GetRuntimeMemorySizeLong(_cameraRuntimeAnimator.animationClips[i]);
        }
        return (int)num;
    }

    private string getCuttName(string cutt_name, int liveId = 0)
    {
        string text = s_addNames[(int)_soundType];
        if (liveId != 0)
        {
            MasterLiveData.LiveData liveData = MasterDBManager.instance.masterLiveData.Get(liveId);
            if (liveData != null)
            {
                int num = liveData.soundType;
                if ((uint)(num - 1001) <= 4u)
                {
                    text = "";
                }
            }
        }
        return cutt_name + text;
    }

    /// <summary>
    /// Live3DSettingの読み込みを行う
    /// </summary>
    private void MakeLiveSettingInternal()
    {
        //ライブメンバ
        int liveNum = ViewLauncher.instance.liveDirector.stageMemberNumber;

        MemberUnitNumber = liveNum;

        MasterDBManager masterDataManager = MasterDBManager.instance;
        MasterCardData masterCardData = masterDataManager.masterCardData;
        MasterCharaData masterCharaData = masterDataManager.masterCharaData;

        int num2 = 0;
        for (int i = 0; i < liveNum; i++)
        {
            var cDirector = ViewLauncher.instance.charaDirector[i];
            /*
            int dressID = liveIdolData._dressID;
            int unitCardId = liveIdolData._unitCardId;
            MasterCardData.CardData cardData = masterCardData.Get(unitCardId);
            MasterCharaData.CharaData charaData = null;
            if (cardData == null)
            {
                continue;
            }
            charaData = masterCharaData.Get(cardData.charaId);
            MasterCardData.CardData cardData2 = masterCardData.Get(dressID);
            if (cardData2 != null)
            {
                MasterCharaData.CharaData charaData2 = null;
                charaData2 = masterCharaData.Get(cardData2.charaId);
                if ((int)charaData.charaId != (int)charaData2.charaId)
                {
                    dressID = 0;
                }
            }
            _characterData[num2] = new Character3DBase.CharacterData(cardData, charaData, dressID);
            */
            _charaDirector[num2] = cDirector;
            _characterData[num2] = cDirector.characterData;

            num2++;
        }

        _live3DSettings = null;

        int liveID = ViewLauncher.instance.liveDirector.LiveID; //LiveIDをViewLauncherから取得する
        _bootDirectLiveID = liveID;

        int beforeId = 0;
        if (liveID > 0)
        {
            int charaId = GetCharacterData(1).charaId;
            if (!MasterDBManager.IsInstanceEmpty())
            {
                int master3dliveID = liveID;
                Master3dLive.Live3dData live3dData = MasterDBManager.instance.master3dLive.dictionary[master3dliveID];
                string cuttName = getCuttName(live3dData.cutt, master3dliveID);
                string text = ("3d_" + cuttName).ToLower();
                string verticaltext = "";
                if (ViewLauncher.instance.liveDirector.isVertical && ResourcesManager.instance.ExistsAssetBundleManifest(text + "_vertical.unity3d"))
                {
                    verticaltext = "_vertical";
                }
                _live3DSettings = new Live3DSettings();
                _live3DSettings._id = liveID;
                _live3DSettings._beforeId = beforeId;
                _live3DSettings._cuttName = cuttName + verticaltext;
                _live3DSettings._cuttAssetBundleName = text + verticaltext + ".unity3d";
                _live3DSettings._bgName = string.Format("3D/Stage/stg_{0:0000}/Prefab/Stage{0:0000}_hq", live3dData.bg);
                _live3DSettings._bgAssetsBundleName = $"3d_stage_{live3dData.bg:0000}_hq.unity3d";
                //HQファイルがない場合
                if (!ResourcesManager.instance.ExistsAssetBundleManifest(_live3DSettings._bgAssetsBundleName))
                {
                    _isHaveHqStage = false;
                    _live3DSettings._bgName = string.Format("3D/Stage/stg_{0:0000}/Prefab/Stage{0:0000}", live3dData.bg);
                    _live3DSettings._bgAssetsBundleName = $"3d_stage_{live3dData.bg:0000}.unity3d";
                }
                else
                {
                    _isHaveHqStage = true;
                }
                _live3DSettings._delayTime = live3dData.delay;
                if (live3dData.autolip.Length > 0)
                {
                    _live3DSettings._autoLipName = "3D/Lip/" + live3dData.autolip;
                    _live3DSettings._autoLipAssetBundleName = "3d_cutt_" + live3dData.autolip + ".unity3d";
                }
                if (live3dData.camera.Length > 0)
                {
                    _live3DSettings._cameraName = string.Format("3D/Camera/{0}/ac_{0}", live3dData.camera);
                    _live3DSettings._cameraAssetBundleName = "3d_cutt_ac_" + live3dData.camera + ".unity3d";
                }
                if (live3dData.charaMotion.Length > 0)
                {
                    _live3DSettings._charaMotion = $"3D/Chara/Motion/Legacy/{live3dData.charaMotion}";
                    _live3DSettings._charaMotionAssetBundleName = "3d_cutt_" + live3dData.charaMotion + ".unity3d";
                    _live3DSettings._charaMotionAssetBundleName = _live3DSettings._charaMotionAssetBundleName.Replace(' ', '_');
                }
                _live3DSettings.setData(live3dData, charaId);
            }
        }
        sBootDirectLiveID = 0;
    }

    /*
     * 
    private TempData.LiveTempData.LiveIdolData[] GetSwapCharacterListFromLocation()
    {
        TempData.LiveTempData liveTemp = SingletonMonoBehaviour<TempData>.instance.liveTemp;
        TempData.LiveTempData.LiveIdolData[] liveIdolDataList = liveTemp._liveIdolDataList;
        MasterMusicvideoSwapLocation masterMusicvideoSwapLocation = null;
        if (!_isBootDirect && !SingletonMonoBehaviour<MasterDataManager>.IsInstanceEmpty())
        {
            masterMusicvideoSwapLocation = SingletonMonoBehaviour<MasterDataManager>.instance.masterMusicvideoSwapLocation;
        }
        if (masterMusicvideoSwapLocation == null)
        {
            return liveIdolDataList;
        }
        MasterMusicvideoSwapLocation.eMVMode mvmode = MasterMusicvideoSwapLocation.eMVMode.wide;
        switch (liveTemp._playMode)
        {
        case TempData.LiveTempData.eLivePlayMode.grand_live:
        case TempData.LiveTempData.eLivePlayMode.musicvideo_grand:
        case TempData.LiveTempData.eLivePlayMode.carnival_grand:
            mvmode = MasterMusicvideoSwapLocation.eMVMode.grand;
            break;
        case TempData.LiveTempData.eLivePlayMode.musicvideo_playlist:
        {
            WorkMVPlayListData.MVPlayListLiveData currentLiveData = SingletonMonoBehaviour<WorkDataManager>.instance.mvPlayListData.GetCurrentLiveData();
            if ((bool)currentLiveData.isGrand && !currentLiveData.isOfficial)
            {
                mvmode = MasterMusicvideoSwapLocation.eMVMode.grand;
            }
            break;
        }
        }
        int liveId = 0;
        if (GameDefine.REPLACE_MV_ENABLE)
        {
            if (!SingletonMonoBehaviour<LiveController>.IsInstanceEmpty() && SingletonMonoBehaviour<LiveController>.instance.GetLiveId(LiveDefine.eReplaceLiveIdType.After) > 0)
            {
                liveId = SingletonMonoBehaviour<LiveController>.instance.GetLiveId(LiveDefine.eReplaceLiveIdType.After);
            }
            else if (!SingletonMonoBehaviour<TempData>.IsInstanceEmpty() && (int)liveTemp._liveId > 0)
            {
                liveId = liveTemp._liveId;
            }
            else if (sBootDirectLiveID > 0)
            {
                liveId = sBootDirectLiveID;
            }
        }
        else if (!SingletonMonoBehaviour<TempData>.IsInstanceEmpty() && (int)liveTemp._liveId > 0)
        {
            liveId = liveTemp._liveId;
        }
        else if (sBootDirectLiveID > 0)
        {
            liveId = sBootDirectLiveID;
        }
        if (!masterMusicvideoSwapLocation.GetMVSwapLocation(liveId, MasterMusicvideoSwapLocation.eQuality.live3d, mvmode, out var swapList))
        {
            return liveIdolDataList;
        }
        TempData.LiveTempData.LiveIdolData[] array = new TempData.LiveTempData.LiveIdolData[liveIdolDataList.Length];
        for (int i = 0; i < swapList.Length && i < liveIdolDataList.Length; i++)
        {
            if (swapList[i] > 0)
            {
                array[i] = liveIdolDataList[swapList[i] - 1];
            }
        }
        return array;
    }

    
    private void SwapWideCharacterData()
    {
        TempData.LiveTempData liveTemp = SingletonMonoBehaviour<TempData>.instance.liveTemp;
        switch (liveTemp._playMode)
        {
        case TempData.LiveTempData.eLivePlayMode.grand_live:
        case TempData.LiveTempData.eLivePlayMode.musicvideo_grand:
        case TempData.LiveTempData.eLivePlayMode.carnival_grand:
            return;
        case TempData.LiveTempData.eLivePlayMode.musicvideo_playlist:
        {
            WorkMVPlayListData.MVPlayListLiveData currentLiveData = SingletonMonoBehaviour<WorkDataManager>.instance.mvPlayListData.GetCurrentLiveData();
            if ((bool)currentLiveData.isGrand && !currentLiveData.isOfficial)
            {
                return;
            }
            break;
        }
        }
        MasterMusicvideoSwapLocation masterMusicvideoSwapLocation = null;
        if (!_isBootDirect && !SingletonMonoBehaviour<MasterDataManager>.IsInstanceEmpty())
        {
            masterMusicvideoSwapLocation = SingletonMonoBehaviour<MasterDataManager>.instance.masterMusicvideoSwapLocation;
        }
        if (masterMusicvideoSwapLocation == null)
        {
            return;
        }
        int liveId = 0;
        if (GameDefine.REPLACE_MV_ENABLE)
        {
            if (!SingletonMonoBehaviour<LiveController>.IsInstanceEmpty() && SingletonMonoBehaviour<LiveController>.instance.GetLiveId(LiveDefine.eReplaceLiveIdType.After) > 0)
            {
                liveId = SingletonMonoBehaviour<LiveController>.instance.GetLiveId(LiveDefine.eReplaceLiveIdType.After);
            }
            else if (!SingletonMonoBehaviour<TempData>.IsInstanceEmpty() && (int)liveTemp._liveId > 0)
            {
                liveId = liveTemp._liveId;
            }
            else if (sBootDirectLiveID > 0)
            {
                liveId = sBootDirectLiveID;
            }
        }
        else if (!SingletonMonoBehaviour<TempData>.IsInstanceEmpty() && (int)liveTemp._liveId > 0)
        {
            liveId = liveTemp._liveId;
        }
        else if (sBootDirectLiveID > 0)
        {
            liveId = sBootDirectLiveID;
        }
        if (!masterMusicvideoSwapLocation.GetMVSwapLocation(liveId, MasterMusicvideoSwapLocation.eQuality.live3d, MasterMusicvideoSwapLocation.eMVMode.wide, out var swapList))
        {
            return;
        }
        Character3DBase.CharacterData[] array = new Character3DBase.CharacterData[15];
        for (int i = 0; i < _characterData.Length && i < _characterData.Length; i++)
        {
            if (swapList[i] > 0)
            {
                array[i] = _characterData[swapList[i] - 1];
            }
        }
        _characterData = array;
    }
     * 
     */


    private string[] GetIndirectLightShaftResourceNames()
    {
        return _indirectLightShuftResourceNames;
    }

    private int GetActiveCameraIndex()
    {
        int activeCameraIndex = _activeCameraIndex;
        if (activeCameraIndex >= _imageEffectLive3dList.Count)
        {
            return -1;
        }
        return activeCameraIndex;
    }
    public PostEffectLive3D GetActivePostEffect()
    {
        int activeCameraIndex = GetActiveCameraIndex();
        if (activeCameraIndex < 0)
        {
            return null;
        }
        if (activeCameraIndex < _imageEffectLive3dList.Count)
        {
            return _imageEffectLive3dList[activeCameraIndex];
        }
        return null;
    }

    public void InitializeA2UManager(bool instatialOnly)
    {
        if (_stageController != null && _liveTimelineControl != null && A2U.manager == null)
        {
            int num = Screen.width;
            int num2 = Screen.height;
            Director director = instance;
            if (director != null && director.useCalcedCameraRect)
            {
                num = (int)(num * director.mainCameraRect.width);
                num2 = (int)(num2 * director.mainCameraRect.height);
            }
            LiveTimelineA2USettings a2uSettings = _liveTimelineControl.data.a2uSettings;
            if (a2uSettings.prefabs != null && a2uSettings.prefabs.Length != 0)
            {
                StageA2UManager obj = (StageA2UManager)A2U.Init(_stageController.gameObject, num, num2);
                StageA2UManager.InitContext initContext = default(StageA2UManager.InitContext);
                initContext.timelineControl = _liveTimelineControl;
                initContext.rtWidth = num;
                initContext.rtHeight = num2;
                obj.Init(ref initContext);
            }
        }
    }

    /// <summary>
    /// キャラクタの初期位置を取得する
    /// </summary>
    /// <param name="i">ポジションID</param>
    /// <returns></returns>
    private Vector3 MakeCharaInitPos(int i)
    {
        return new Vector3((i % 2 - 0.5f) * 2f * Mathf.RoundToInt(i / 2) * 1.5f, 0f, i / 2 * -0.5f);
    }

    /// <summary>
    /// 対象のCharaDataWorksを取得する
    /// </summary>
    private void BuildCharaData()
    {
        if (_master3DCharaData == null)
        {
            return;
        }
        for (int i = 0; i < MemberUnitNumber; i++)
        {
            _charaDataWorks[i] = null;
        }
        for (int j = 0; j < MemberUnitNumber; j++)
        {
            if (_charaDirector[j] != null && _charaDirector[j].characterData != null)
            {
                _charaDataWorks[j] = _master3DCharaData.GetCharaData(_charaDirector[j].characterData.charaId);
            }
        }
    }

    private void Load3DCharaDataCSV(out Master3DCharaData data, string csvPath, bool isServer = false)
    {
        TextAsset textAsset = ResourcesManager.instance.LoadObject<TextAsset>(csvPath);
        if (textAsset != null)
        {
            ArrayList records = Utility.ConvertCSV(textAsset.ToString());
            data = new Master3DCharaData(records);
        }
        else
        {
            data = null;
        }
    }

    /// <summary>
    /// 3DCharaDataの読み込みを行う
    /// </summary>
    private void MakeFaceTypeConvInternal()
    {
        if (_live3DSettings != null && _live3DSettings._id > 0)
        {
            if (!MasterDBManager.IsInstanceEmpty())
            {
                _master3DCharaData = MasterDBManager.instance.master3DCharaData;
            }
            BuildCharaData();
        }
    }

    public static readonly string[] _3dStageValiableAssetBundleSuffix = new string[3] { "_variable_low", "_variable", "_variable_hq" };

    public void LoadVariableObject()
    {
        int objectCount = _liveTimelineControl.data.stageObjectsSettings.objectCount;
        for (int i = 0; i < objectCount; i++)
        {
            LiveTimelineStageObjectsSettings.Object @object = _liveTimelineControl.data.stageObjectsSettings.objectData[i];
            if (@object == null)
            {
                continue;
            }
            int num = (int)(GetUseReplaceQuality(@object.replaceQuality) - 2);
            string arg = $"{@object.objectId}_{@object.objectName}{_3dStageValiableAssetBundleSuffix[num]}";
            string path = $"3D/Stage/stg_{@object.objectId}/Prefab/pf_stg_{arg}";
            GameObject gameObject = LoadResource<GameObject>(path);
            if (!(gameObject == null))
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
                if (!(gameObject2 == null))
                {
                    gameObject2.transform.SetParent(_stageController.gameObject.transform);
                    _stageController.AddObjectWorkInfo(gameObject2.GetComponentsInChildren<Transform>());
                }
            }
        }
    }

    public bool SearchPrefabNames(string[] prefabNames, Func<string, bool> fnCondition = null)
    {
        if (prefabNames == null)
        {
            return false;
        }
        if (fnCondition != null)
        {
            for (int i = 0; i < prefabNames.Length; i++)
            {
                if (fnCondition(prefabNames[i]))
                {
                    return true;
                }
            }
        }
        else
        {
            for (int j = 0; j < prefabNames.Length; j++)
            {
                if (!string.IsNullOrEmpty(prefabNames[j]))
                {
                    return true;
                }
            }
        }
        return false;
    }


    public bool SearchParticlePrefabName()
    {
        return SearchPrefabNames(_liveTimelineControl.data.particlePrefabNames);
    }

    public bool SearchParticleConfettiPrefabName()
    {
        Func<string, bool> fnCondition = (string strPrefabName) => (StageController.GetParticleType(strPrefabName) == StageController.eParticleType.Confetti) ? true : false;
        return SearchPrefabNames(_liveTimelineControl.data.particlePrefabNames, fnCondition);
    }

    public bool SearchMirrorScanPrefabName()
    {
        return SearchPrefabNames(_liveTimelineControl.data.mirrorScanLightBodyPrefabNames);
    }

    public void Preload(List<string> lstDownload)
    {
        if (_live3DSettings == null)
        {
            MakeLiveSettingInternal();
        }
        if (_master3DCharaData == null)
        {
            MakeFaceTypeConvInternal();
        }
        if (lstDownload == null)
        {
            return;
        }
        Action<string> fnAddAssetBundle = delegate (string bundleName)
        {
            if (!string.IsNullOrEmpty(bundleName) & !lstDownload.Contains(bundleName))
            {
                lstDownload.Add(bundleName);
            }
        };
        Func<string, bool, bool> func = delegate (string assetBundleName, bool isVertical)
        {
            string newValue = "_legacy_vertical";
            string text = assetBundleName.Replace("_legacy", newValue);
            if (isVertical && ResourcesManager.instance.ExistsAssetBundleManifest(text))
            {
                fnAddAssetBundle(text);
            }
            else
            {
                fnAddAssetBundle(assetBundleName);
            }
            return true;
        };
        //fnAddAssetBundle(Cabinet.ASSET_BUNDLE_NAME);
        if (_live3DSettings._charaMotion.Length > 0)
        {
            bool num = _live3DSettings._charaMotionAssetBundleName.Contains("{0:D2}_legacy");
            /*bool verticalFlag = LiveUtils.IsVerticalLive() || LiveUtils.IsVerticalMV()*/
            bool verticalFlag = ViewLauncher.instance.liveDirector.isVertical;
            if (num)
            {
                for (int i = 0; i < _live3DSettings._charaMotionNum; i++)
                {
                    string baseName = string.Format(_live3DSettings._charaMotionAssetBundleName, i + 1);
                    AddMotionAssetBundleBaseOrAlt(func, baseName, verticalFlag);
                }
            }
            else if (_live3DSettings._charaMotionAssetBundleName.IndexOf("/", StringComparison.Ordinal) < 0)
            {
                AddMotionAssetBundleBaseOrAlt(func, _live3DSettings._charaMotionAssetBundleName, verticalFlag);
            }
            for (int j = 0; j < _live3DSettings._overrideMotionNum; j++)
            {
                AddMotionAssetBundleBaseOrAlt(func, string.Format(_live3DSettings._overrideMotionAssetBundleName, j + 1), verticalFlag);
            }
            for (int k = 0; k < _live3DSettings._charaHeightMotionNum; k++)
            {
                func(string.Format(_live3DSettings._charaHeightMotionAssetBundleName, k + 1), verticalFlag);
            }
        }
        fnAddAssetBundle(_live3DSettings._cuttAssetBundleName);
    }


    private void AddMotionAssetBundleBaseOrAlt(Func<string, bool, bool> fnAddMotionAssetBundle, string baseName, bool isVertical)
    {
        string arg = baseName.Replace("_legacy", "_alt_legacy");
        if (_liveTimelineControl == null)
        {
            fnAddMotionAssetBundle(baseName, isVertical);
            fnAddMotionAssetBundle(arg, isVertical);
            return;
        }
        bool flag = false;
        bool flag2 = false;
        if (_liveTimelineControl.data.alterAnimationMode == LiveTimelineData.AlterAnimationMode.LeftHanded)
        {
            List<int> motionUseCharaPosition = _liveTimelineControl.data.alterAnimationSettings.GetMotionUseCharaPosition(baseName);
            for (int i = 0; i < motionUseCharaPosition.Count; i++)
            {
                int charaId = instance.GetCharacterData(motionUseCharaPosition[i] + 1).charaId;
                if (MasterDBManager.instance.masterCharaData.Get(charaId).hand == 3002)
                {
                    flag2 = true;
                }
                else
                {
                    flag = true;
                }
            }
        }
        else
        {
            flag = true;
        }
        if (!flag && !flag2)
        {
            flag = true;
        }
        if (flag)
        {
            fnAddMotionAssetBundle(baseName, isVertical);
        }
        if (flag2)
        {
            fnAddMotionAssetBundle(arg, isVertical);
        }
    }

    /// <summary>
    /// AssetBundleからリソースを読み込む
    /// </summary>
    private T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        return (T)ResourcesManager.instance.LoadObject(path);
    }

    private void AddAssetBundleList(string assetName)
    {
        if (!_lstAssetBundleName.Contains(assetName) && !string.IsNullOrEmpty(assetName))
        {
            _lstAssetBundleName.Add(assetName);
        }
    }

    private void PushAssetTask(Func<IEnumerator> fn)
    {
        _qAssetTask.Enqueue(fn);
    }

    /// <summary>
    /// Assetのとりまとめタスクのステータスを変更する
    /// </summary>
    /// <param name="prepareState">タスクステータス</param>
    private void PrepareAssetTask(eAssetTaskState prepareState = eAssetTaskState.Idle)
    {
        _qAssetTask.Clear();
        _assetTaskState = prepareState;
    }

    private IEnumerator ProcessAssetTaskQueue(eAssetTaskState doneState, Action fnFinished = null)
    {
        while (_qAssetTask.Count > 0)
        {
            Func<IEnumerator> func = _qAssetTask.Dequeue();
            yield return func();
        }
        fnFinished?.Invoke();
        _assetTaskState = doneState;
    }

    /// <summary>
    /// Timelineの初期化を行う
    /// </summary>
    /// <returns></returns>
    private IEnumerator fnPrepareTimelineData()
    {
        if (_liveTimelineControl == null)
        {
            //ない場合はダミー
            string text = "Cutt_oneshin";
            if (_live3DSettings != null && !string.IsNullOrEmpty(_live3DSettings._cuttName))
            {
                text = _live3DSettings._cuttName;
            }
            InstanciateTimeline($"Cutt/{text}/{text}");
            _liveTimelineControl._limitFovForWidth = false;
            _liveTimelineControl._baseCameraAspectRatio = 1.77777779f;
            _liveTimelineControl._limitFovForWidth = true;
        }
        bool unloadAssetWorkSheet = true;
        if (_liveTimelineControl != null)
        {
            _liveTimelineControl.SetUnloadAssetWorkSheet(unloadAssetWorkSheet);
            LiveTimelineMonitorSettings monitorSettings = _liveTimelineControl.data.monitorSettings;
            if (monitorSettings.charaTextureNum > 0)
            {
                _uvMovieCharaIdArray = new int[monitorSettings.charaTextureNum];
                for (int i = 0; i < monitorSettings.charaTextureNum; i++)
                {
                    int index = (int)(monitorSettings.charaTexturePosition[i] + 1);
                    _uvMovieCharaIdArray[i] = GetCharacterData(index).charaId;
                }
            }
        }
        if (_liveTimelineControl != null)
        {
            CreateSpareDressInCharacterData();
        }
        yield break;
    }

    private void CreateSpareDressInCharacterData()
    {
        if (_cuttDressChange == null)
        {
            return;
        }
        int playSongId = GetPlaySongId();
        DressCabinet cabinet = Cabinet.GetCabinet<DressCabinet>();
        for (int i = 0; i < MemberUnitNumber; i++)
        {
            if (_characterData[i] == null)
            {
                continue;
            }
            int[] array = _cuttDressChange.CheckAndRun(_characterData[i], i);
            int num = _characterData[i].changeDressId;
            if (array != null && 1 <= array.Length)
            {
                num = array[0];
                _characterData[i].changeDressId = num;
            }
            if (array == null || array.Length <= 1)
            {
                DressCabinet.SpareDressRecord record = null;
                cabinet.GetSpareDressData(playSongId, num, out record);
                if (record != null)
                {
                    Character3DBase.CharacterData.eDressType eDressType = Character3DBase.CharacterData.eDressType.DressB;
                    int spareDressId = record.GetSpareDressId(eDressType);
                    _characterData[i].CreateSpare(spareDressId, spareDressId, eDressType);
                }
            }
            else
            {
                int num2 = array.Length - 1;
                _characterData[i].InitializeSpare(num2);
                for (int j = 0; j < num2; j++)
                {
                    int num3 = array[j + 1];
                    _characterData[i].CreateSpare(num3, num3, (Character3DBase.CharacterData.eDressType)(j + 1));
                }
            }
        }
        _cuttDressChange.WriteLogText();
    }

    public IEnumerator MakeAssetBundleListTask(Action fnFinished = null)
    {
        PrepareAssetTask(eAssetTaskState.WorkDownload);
        _lstAssetBundleName.Clear();
        PushAssetTask(fnPrepareTimelineData);
        PushAssetTask(fnMakeAssetBundleList);
        yield return ProcessAssetTaskQueue(eAssetTaskState.DoneDonwload, fnFinished);
    }

    public IEnumerator LoadAssetBundleTask(Action fnFinished = null)
    {
        PrepareAssetTask(eAssetTaskState.WorkLoad);
        PushAssetTask(fnLoadAssetBundle);
        yield return ProcessAssetTaskQueue(eAssetTaskState.Done, fnFinished);
    }

    /// <summary>
    /// 直接MVを再生する
    /// デレステではライブの途中再開等にあたる
    /// </summary>
    public IEnumerator LoadLiveDirectTask(Action fnFinished = null)
    {
        PrepareAssetTask();
        _lstAssetBundleName.Clear();
        PushAssetTask(fnMakeLiveSetting);
        PushAssetTask(fnPrepareTimelineData);
        PushAssetTask(fnMakeAssetBundleList);
        PushAssetTask(fnPrepareHQParticle);
        PushAssetTask(fnLoadAssetBundle);
        yield return ProcessAssetTaskQueue(eAssetTaskState.Done, fnFinished);
    }

    /// <summary>
    /// アセットファイルのとりまとめを行う
    /// </summary>
    private IEnumerator fnMakeAssetBundleList()
    {
        AddShaderAssetBundle();
        AddStageAssetBundle();
        AddStageEffectAssetBundle();
        AddStageObjectAssetBundle();
        AddStageVariableAssetBundle();
        AddCharacterAssetBundle();
        AddPropsAssetBundle();
        AddMultiCameraMaskAssetBundle();
        yield break;
    }

    private void AddShaderAssetBundle()
    {
        /* ShaderはResouceManagerで管理する
        int num = 0;
        num = ((SingletonMonoBehaviour<TempData>.instance.liveTemp._playMode == TempData.LiveTempData.eLivePlayMode.commu_musicvideo) ? 1 : 0);
        List<string> usableShaderAssets = ShaderBank.GetUsableShaderAssets(num);
        for (int i = 0; i < usableShaderAssets.Count; i++)
        {
            string text = usableShaderAssets[i];
            if (LiveUtils.IsRich())
            {
                AddAssetBundleList(text);
            }
            else if (string.Compare(text, "3d_shader02.unity3d") != 0)
            {
                AddAssetBundleList(text);
            }
        }
        */
    }

    private void AddStageAssetBundle()
    {
        AddAssetBundleList("3d_stage_common.unity3d");
        AddAssetBundleList("3d_uvm_texture.unity3d");
        string text = "";
        AddAssetBundleList("3d_stage_common_hq.unity3d");
        text = $"3d_cyalume_m{live3DSettings._id:D03}_hq.unity3d";
        /*
        if ((bool)SingletonMonoBehaviour<LocalData>.instance.option.isVisibleMobShadow)
        {
            AddAssetBundleList("3d_stage_mob_common.unity3d");
        }
        */
        AddAssetBundleList("3d_stage_mob_common.unity3d");

        AddAssetBundleList(text);
        AddAssetBundleList(_live3DSettings._bgAssetsBundleName);
        _ = ResourcesManager.instance;
        for (int i = 0; i < _live3DSettings._uvMovieAssetBundleNames.Length; i++)
        {
            if (_live3DSettings._uvMovieAssetBundleNames[i] != null && _live3DSettings._uvMovieAssetBundleNames[i].Length != 0)
            {
                AddAssetBundleList(_live3DSettings._uvMovieAssetBundleNames[i]);
                if (_uvMovieCharaIdArray != null && i < _uvMovieCharaIdArray.Length)
                {
                    AddAssetBundleList(string.Format(_live3DSettings._uvMovieCharaAssetBundleNames[i], _uvMovieCharaIdArray[i]));
                }
            }
        }
        if (_live3DSettings._cameraAssetBundleName.Length > 0)
        {
            AddAssetBundleList(_live3DSettings._cameraAssetBundleName);
        }
    }

    private void AddStageEffectAssetBundle()
    {
        LiveTimelineIndirectLightShuftSettings indirectLightShuftSettings = _liveTimelineControl.data.indirectLightShuftSettings;
        AddAssetBundleList($"3d_lightshuft_{indirectLightShuftSettings.id:D4}.unity3d");
        LiveTimelineA2USettings a2uSettings = _liveTimelineControl.data.a2uSettings;
        if (a2uSettings != null && a2uSettings.prefabs != null && a2uSettings.prefabs.Length != 0)
        {
            AddAssetBundleList($"3d_a2u_{a2uSettings.assetNo:D4}.unity3d");
        }
        int flareDataGroupCount = _liveTimelineControl.data.lensFlareSetting.flareDataGroupCount;
        if (flareDataGroupCount > 0)
        {
            for (int i = 0; i < flareDataGroupCount; i++)
            {
                LiveTimelineLensFlareSetting.FlareDataGroup flareDataGroup = _liveTimelineControl.data.lensFlareSetting.flareDataGroup[i];
                if (flareDataGroup.objectId > 0 && (!flareDataGroup.isHqOnly))
                {
                    string assetName = $"3d_flare_{flareDataGroup.objectId:D4}.unity3d";
                    AddAssetBundleList(assetName);
                }
            }
        }
        else if (_isHaveHqStage)
        {
            AddAssetBundleList("3d_flare_0001.unity3d");
        }
    }

    private void AddStageObjectAssetBundle()
    {
        Action<string[], string, bool, string> obj = delegate (string[] prefabNames, string objectTypeName, bool isDirectAssetName, string suffix)
        {
            if (prefabNames != null && prefabNames.Length != 0)
            {
                for (int i = 0; i < prefabNames.Length; i++)
                {
                    if (!string.IsNullOrEmpty(prefabNames[i]))
                    {
                        string assetName = (isDirectAssetName ? prefabNames[i] : ("3d_" + objectTypeName + "_" + prefabNames[i] + suffix + ".unity3d"));
                        if (objectTypeName == "particle")
                        {
                            bool value = false;
                            if (suffix == "_hq")
                            {
                                value = true;
                            }
                            if (!ResourcesManager.instance.ExistsAssetBundleManifest(assetName))
                            {
                                value = false;
                                assetName = "3d_" + objectTypeName + "_" + prefabNames[i] + ".unity3d";
                            }
                            if (_live3DSettings._useHqParticlesDic.ContainsKey(prefabNames[i]))
                            {
                                _live3DSettings._useHqParticlesDic[prefabNames[i]] = value;
                            }
                            else
                            {
                                _live3DSettings._useHqParticlesDic.Add(prefabNames[i], value);
                            }
                        }
                        AddAssetBundleList(assetName);
                    }
                }
            }
        };
        obj(arg4: (_liveTimelineControl.data.isUseHQParticle) ? "_hq" : "", arg1: _liveTimelineControl.data.particlePrefabNames, arg2: "particle", arg3: false);
        obj(_live3DSettings._mirrorScanAssetBundleNames, "", arg3: true, "");
        obj(_liveTimelineControl.data.mirrorScanLightBodyPrefabNames, "mirrorscan_body", arg3: false, "");

        //mobCyalume3DResourceIDが0でなければ3D
        if (ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DResourceID != 0)
        {
            MobController.RegisterDownload(_lstAssetBundleName);
        }

    }

    private void AddStageVariableAssetBundle()
    {
        string[] _3dStageValiableAssetBundleSuffix = new string[3] { "_variable_low", "_variable", "_variable_hq" };

        if (_liveTimelineControl.data.stageObjectsSettings.objectCount != 0)
        {
            for (int i = 0; i < _liveTimelineControl.data.stageObjectsSettings.objectCount; i++)
            {
                LiveTimelineStageObjectsSettings.Object @object = _liveTimelineControl.data.stageObjectsSettings.objectData[i];
                int num = (int)(GetUseReplaceQuality(@object.replaceQuality) - 2);
                string assetName = $"3d_stage_{@object.objectId}{_3dStageValiableAssetBundleSuffix[num]}.unity3d";
                AddAssetBundleList(assetName);
            }
        }
    }

    private void AddCharacterAssetBundle()
    {
        int songId = GetPlaySongId();
        Action<int, Character3DBase.CharacterData> action = delegate (int index, Character3DBase.CharacterData charaData)
        {
            //SubIDが存在するか確認
            Character3DBase.eHeadLoadType headLoadType;
            int headIndexWithHeadSelector = StageUtil.GetHeadIndexWithHeadSelector(charaData, _master3DCharaData, songId, index == 0, out headLoadType);
            int headTextureIndex = StageUtil.GetHeadTextureIndex(charaData.charaId, charaData.activeDressId);
            bool isSubHeadIndex = headLoadType == Character3DBase.eHeadLoadType.Default;

            //BoundsBoxを作成
            _master3DCharaData.GetBoundsBoxSizeData(out var outBoundsBoxSizeData, charaData.charaId, charaData.activeDressId, headIndexWithHeadSelector);
            charaData.boundsBoxSizeData = outBoundsBoxSizeData;

            //_lstAssetBundleNameへ使用するアセットファイルを追加
            charaData.GetLoadCharacterAssetBundle(_lstAssetBundleName, Character3DBase.eResourceQuality.Rich, headIndexWithHeadSelector, headTextureIndex, isSubHeadIndex, null);
        };

        for (int i = 0; i < MemberUnitNumber; i++)
        {
            if (_characterData[i] == null)
            {
                continue;
            }
            Character3DBase.CharacterData characterData = _characterData[i];
            action(i, characterData);
            if (characterData.spareDataArray != null)
            {
                for (int j = 0; j < characterData.spareDataArray.Length; j++)
                {
                    action(i, characterData.spareDataArray[j]);
                }
            }
        }
        if (_live3DSettings._autoLipAssetBundleName.Length > 0)
        {
            AddAssetBundleList(_live3DSettings._autoLipAssetBundleName);
        }
        CharaEffectInfo.AddAssetBundleList(ref _lstAssetBundleName, _liveTimelineControl);
        /*
        if (!LiveUtils.IsRich())
        {
            return;
        }
        */
        int num = _liveTimelineControl.data.characterOptionSettings.optionName.Length;
        for (int k = 0; k < num; k++)
        {
            int result = 0;
            int.TryParse(_liveTimelineControl.data.characterOptionSettings.optionName[k], out result);
            if (result > 0)
            {
                AddAssetBundleList(string.Format(CharaDirector.AssetBundle.GetOptionalDataName(result)));
            }
        }
    }

    private void AddPropsAssetBundle()
    {
        List<string> outListPropsNames = null;
        for (int i = 0; i < MemberUnitNumber; i++)
        {
            if (_charaDirector[i].characterData != null && _charaDirector[i].characterData != null)
            {
                _charaDirector[i].characterData.GetCharacterPropsList(i, _liveTimelineControl, ref outListPropsNames, null, isCheckOverlap: true, _charaDirector[i].characterData.cardId);
            }
        }
        if (outListPropsNames == null)
        {
            return;
        }
        for (int j = 0; j < outListPropsNames.Count; j++)
        {
            if (!string.IsNullOrEmpty(outListPropsNames[j]))
            {
                string assetName = "3d_props_" + outListPropsNames[j] + ".unity3d";
                AddAssetBundleList(assetName);
            }
        }
    }

    /// <summary>
    /// Assetの読み込み
    /// </summary>
    /// <returns></returns>
    private IEnumerator fnLoadAssetBundle()
    {
        ProjectorController.ResetSortingOrderOffset();
        ShaderWarmup();
        yield return fnCreateRichCamera();
        yield return fnCreateCharacterObjects();
        yield return fnCreateCharacterResources();
        yield return fnCreateCharacterEffect();
        yield return fnCreateStage();
        yield return fnCreateStageEffect();
        yield return fnCreateProps();
        yield return fnCreateCamera();
        OverrideMotionClipForConditionMotionChange();
        SetupCharacterLocator();
        ApplyShaderQualityLevel();
        CheckSwitchSheetCharacter(_liveTimelineControl);
        CheckSwitchSheetAltCharacter(_liveTimelineControl, null);
        if (_isBootDirect)
        {
            _lyricsManager = LyricsManager.getCompornent();
            if (_lyricsManager != null)
            {
                _lyricsManager.CreateLyricsObject(_live3DSettings._id);
            }
        }
    }

    private void ShaderWarmup()
    {
        //ShaderはResouceManagerで管理する
        /*
        bool isServerResource = !CustomPreference.isLocalAssetBundles;
        Action<string, string> action = delegate (string assetName, string objName)
        {
            ShaderVariantCollection shaderVariantCollection = ResourcesManager.instance.LoadObject<ShaderVariantCollection>(assetName, objName, isServerResource);
            if (!(shaderVariantCollection == null) && !shaderVariantCollection.isWarmedUp)
            {
                shaderVariantCollection.WarmUp();
            }
        };
        if (GetQualityType() > LocalData.OptionData.eQualityType.Quality3D_Light)
        {
            action("3d_shader03.unity3d", "Shader/ShaderVariant_Standard");
            if (LiveUtils.IsRich())
            {
                action("3d_shader02.unity3d", "Shader/ShaderVariant_Rich");
            }
            if (LiveUtils.IsMV())
            {
                action("3d_shader03.unity3d", "Shader/ShaderVariant_MV");
            }
        }
        */
    }

    /// <summary>
    /// キャラクターオブジェクトを作成する
    /// </summary>
    /// <returns></returns>
    private IEnumerator fnCreateCharacterObjects()
    {
        //デリゲートを生成
        Func<int, Vector3, CharaDirector, CharacterObject> createCharacterObjectFunction = delegate (int index, Vector3 pos, CharaDirector charaData)
        {
            GameObject obj = UnityEngine.Object.Instantiate(_prefabs[0]); //プレハブにテンプレを入れておく
            obj.transform.position = pos;
            obj.transform.SetParent(base.transform, worldPositionStays: false);
            CharacterObject characterObject = obj.GetComponent<CharacterObject>();
            if (characterObject != null)
            {
                Character3DBase.eResourceQuality resourceQuality = Character3DBase.eResourceQuality.Rich;

                /* リッチで固定する
                Character3DBase.eResourceQuality resourceQuality = Character3DBase.eResourceQuality.HighPolygon;
                if (LiveUtils.IsCharacterRich(MemberUnitNumber))
                {
                    resourceQuality = Character3DBase.eResourceQuality.Rich;
                }
                else
                {
                    bool bLightMode = GetQualityType() == LocalData.OptionData.eQualityType.Quality3D_Light;
                    if (!_liveTimelineControl.data.isHighPolygonModel(index, bLightMode) | isForceLOD)
                    {
                        resourceQuality = Character3DBase.eResourceQuality.LowPolygon;
                    }
                }
                */
                characterObject.Initialize(charaData, null, resourceQuality);
            }
            return characterObject;
        };

        for (int i = 0; i < MemberUnitNumber; i++)
        {
            Vector3 characterPosition = MakeCharaInitPos(i);

            CharacterObject characterObject = createCharacterObjectFunction(i, characterPosition, _charaDirector[i]);
            if (characterObject == null)
            {
                continue;
            }
            _characterObjects.Add(characterObject);

            //Cabinetが何かよくわからない。要調査
            /*
            if (_characterData[i].spareDataArray == null || _mapSpareCharacter.ContainsKey(i))
            {
                continue;
            }
            _mapSpareCharacter.Add(i, new CharacterObject[_characterData[i].spareDataArray.Length]);
            int num = _characterData[i].spareDataArray.Length;
            for (int j = 0; j < num; j++)
            {
                CharacterObject characterObject2 = func(i, arg, _characterData[i].spareDataArray[j]);
                if (characterObject2 != null)
                {
                    _mapSpareCharacter[i][j] = characterObject2;
                }
            }
            characterObject.spareCharacters = new CharacterObject[num];
            for (int k = 0; k < num; k++)
            {
                characterObject.spareCharacters[k] = _mapSpareCharacter[i][k];
                characterObject.spareCharacters[k].spareCharacters = new CharacterObject[num];
                for (int l = 0; l < num; l++)
                {
                    characterObject.spareCharacters[k].spareCharacters[l] = ((k == l) ? characterObject : _mapSpareCharacter[i][l]);
                }
            }
            */
        }
        yield break;
    }

    /// <summary>
    /// キャラクタのリソースを作成
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator fnCreateCharacterResources()
    {
        CySpring.ClearUnionCollision();
        //TextureComposite.Manifest.Open();
        List<Camera> lstTargetCamera = MakeCameraList();
        int songId = GetPlaySongId();
        Action<int, CharacterObject> fnMakeCreateInfo = delegate (int index, CharacterObject charaObject)
        {
            Character3DBase.CharacterData data = charaObject.data;
            if (data != null)
            {
                Character3DBase.eHeadLoadType headLoadType = Character3DBase.eHeadLoadType.Default;
                int headIndexWithHeadSelector = StageUtil.GetHeadIndexWithHeadSelector(data, _master3DCharaData, songId, index == 0, out headLoadType);
                int headTextureIndex = StageUtil.GetHeadTextureIndex(data.charaId, data.activeDressId);
                bool isSubHeadIndex = headLoadType == Character3DBase.eHeadLoadType.Default;
                _master3DCharaData.GetBoundsBoxSizeData(out var outBoundsBoxSizeData, data.charaId, data.activeDressId, headIndexWithHeadSelector);
                data.boundsBoxSizeData = outBoundsBoxSizeData;
                Master3DCharaData.Behavior behavior = null;
                _master3DCharaData.GetBehavior(data.charaId, data.cardId, out behavior);
                charaObject.Behavior = behavior;
                Character3DBase.CreateInfo createInfo = charaObject.createInfo;
                createInfo.Reset();
                createInfo.index = index;
                createInfo.isBootDirect = _isBootDirect;
                //createInfo.useAssetBundle = _isUseAsssetBundle;
                createInfo.mergeMaterial = !_isBootDirect;
                createInfo.headIndex = headIndexWithHeadSelector; //subID
                createInfo.headTextureIndex = headTextureIndex;
                createInfo.isSubHeadIndex = isSubHeadIndex; //subIDがあるかどうか
                createInfo.positionMode = _liveTimelineControl.data.characterPositionMode;
                /* ドレスコーデ(TextureComposite)用のため実装見送り
                int charaId = data.charaId;
                int activeDressId = data.activeDressId;
                int activeAccessoryId = data.activeAccessoryId;
                TempData.DressCustomize dressCustomize = SingletonMonoBehaviour<TempData>.instance.dressCustomize;
                List<string> partsCodeList = dressCustomize.GetPartsCodeList(charaId, activeDressId, activeAccessoryId, !_isBootDirect);
                TextureComposite.Meta meta = dressCustomize.GetMeta(charaId, activeDressId, activeAccessoryId);
                createInfo.SetPartsCode(partsCodeList);
                createInfo.textureCompositeMeta = meta;
                */
                createInfo.cyspringPurpose = CySpringCollisionComponent.ePurpose.Union;
                if (_liveTimelineControl.data.characterSettings.isCreateGroundCollision)
                {
                    createInfo.cyspringPurpose |= CySpringCollisionComponent.ePurpose.Ground;
                }
                createInfo.SetRenderTargetCameraList(lstTargetCamera);
            }

        };

        //ここでキャラクタのリソースを読み込む（早く読み込むと勝手に解放されるため直前で読み込む）
        yield return ResourcesManager.instance.LoadAssetGroup(ViewLauncher.instance.GetCharaAssetList());
        for (int k = 0; k < MemberUnitNumber; k++)
        {
            if (!(_characterObjects[k] == null))
            {
                CharacterObject chara = _characterObjects[k].GetComponent<CharacterObject>();
                fnMakeCreateInfo(k, chara); //createInfoを作成
                yield return chara.CreateResource(); //リソースを作成
                BindResourceCharacter(k, chara); //スクリプトとUnityObjectを割り当てる
            }
        }
        foreach (KeyValuePair<int, CharacterObject[]> item in _mapSpareCharacter)
        {
            CharacterObject[] value = item.Value;
            CharacterObject[] array = value;
            foreach (CharacterObject chara in array)
            {
                fnMakeCreateInfo(chara.index, chara);
                yield return chara.CreateResource();
                BindResourceCharacter(chara.index, chara);
                CharacterObject[] spareCharacters = chara.spareCharacters;
                foreach (CharacterObject characterObject in spareCharacters)
                {
                    chara.motionNoiseTime = characterObject.motionNoiseTime;
                    chara.motionNoiseRate = characterObject.motionNoiseRate;
                    chara.AppointSpareCharacter(isSpare: true);
                }
            }
        }
        float distanceForOutlineLOD = _liveTimelineControl.data.characterSettings.distanceForOutlineLOD;
        float distanceForCheekLOD = _liveTimelineControl.data.characterSettings.distanceForCheekLOD;
        /*
        if (!LiveUtils.IsMV())
        {
            LiveTimelineGamePlaySettings gamePlaySettings = _liveTimelineControl.data.gamePlaySettings;
            if (LiveUtils.IsRich())
            {
                if (gamePlaySettings.distanceRichOutlineLOD >= 0f)
                {
                    distanceForOutlineLOD = gamePlaySettings.distanceRichOutlineLOD;
                }
                if (gamePlaySettings.distanceRichCheekLOD >= 0f)
                {
                    distanceForCheekLOD = gamePlaySettings.distanceRichCheekLOD;
                }
            }
            else
            {
                if (gamePlaySettings.distanceOutlineLOD >= 0f)
                {
                    distanceForOutlineLOD = gamePlaySettings.distanceOutlineLOD;
                }
                if (gamePlaySettings.distanceCheekLOD >= 0f)
                {
                    distanceForCheekLOD = gamePlaySettings.distanceCheekLOD;
                }
            }
        }
        */

        Action<CharacterObject> fnSetOtherInfo = delegate (CharacterObject objChara)
        {
            Character3DBase.RenderController renderController = objChara.renderController;
            objChara.InitializeIK();
            renderController.distanceOutlineLOD = distanceForOutlineLOD;
            renderController.distanceShaderLOD = _liveTimelineControl.data.characterSettings.distanceForShaderLOD;
            objChara.SetDistanceForCheekLOD(distanceForCheekLOD);
        };

        foreach (CharacterObject chara in _characterObjects)
        {
            fnSetOtherInfo(chara);
            yield return chara.SetCySpringController();
            chara.CollectCySpringClothParameter();
        }
        foreach (KeyValuePair<int, CharacterObject[]> item2 in _mapSpareCharacter)
        {
            CharacterObject[] value2 = item2.Value;
            CharacterObject[] array = value2;
            foreach (CharacterObject chara in array)
            {
                fnSetOtherInfo(chara);
                yield return chara.SetCySpringController();
                chara.CollectCySpringClothParameter();
            }
        }
        //TextureComposite.Manifest.Close(bSave: false);
    }

    /// <summary>
    /// キャラエフェクトを生成する
    /// </summary>
    /// <returns></returns>
    private IEnumerator fnCreateCharacterEffect()
    {
        int num = _liveTimelineControl.data.characterOptionSettings.optionName.Length;
        for (int i = 0; i < num; i++)
        {
            int result = 0;
            int.TryParse(_liveTimelineControl.data.characterOptionSettings.optionName[i], out result);
            if (result <= 0)
            {
                continue;
            }
            GameObject gameObject = ResourcesManager.instance.LoadObject<GameObject>(CharaDirector.Asset.GetOptionalDataPath(result));
            if (null == gameObject)
            {
                continue;
            }
            for (int j = 0; j < _characterObjects.Count; j++)
            {
                if (!(_characterObjects[j] == null))
                {
                    _characterObjects[j].CreateCharacterOptionResources(gameObject);
                }
            }
            foreach (KeyValuePair<int, CharacterObject[]> item in _mapSpareCharacter)
            {
                CharacterObject[] value = item.Value;
                for (int k = 0; k < value.Length; k++)
                {
                    value[k].CreateCharacterOptionResources(gameObject);
                }
            }
        }
        CharaEffectInfo.SettingData settingData = default(CharaEffectInfo.SettingData);
        settingData.sourcePrefabNames = _liveTimelineControl.data.shadowPrefabNames;
        settingData.loadFormatPath = "3D/Shadow/{0}/Prefab/pf_stg_{0}";
        settingData.renderQueue = 2500;
        settingData.defaultName = "shadow_0001";
        CharaEffectInfo.LoadCharaEffectObjects(ref _listShadowEffectInfo, settingData);
        settingData.sourcePrefabNames = _liveTimelineControl.data.spotLightPrefabNames;
        settingData.loadFormatPath = "3D/SpotLight/{0}/Prefab/pf_stg_{0}";
        settingData.renderQueue = 2500;
        settingData.defaultName = "spotlight_0001";
        CharaEffectInfo.LoadCharaEffectObjects(ref _listSpotLightEffectInfo, settingData);
        string strSpotlightParentName = _liveTimelineControl.data.spotLightParentName;
        if (string.IsNullOrEmpty(strSpotlightParentName))
        {
            strSpotlightParentName = LiveTimelineData.DEFAULT_SPOTLIGHT_PARENT_NAME;
        }
        bool enableFootShadow = true;
        /*
        if (!LiveUtils.IsMV())
        {
            arg = ((!LiveUtils.IsRich()) ? (!_liveTimelineControl.data.gamePlaySettings.isFootShadowOff) : (!_liveTimelineControl.data.gamePlaySettings.isRichFootShadowOff));
        }
        */
        Action<CharacterObject, bool> action = delegate (CharacterObject charaObject, bool isEnable)
        {
            if (!(charaObject == null))
            {
                charaObject.BindCharaEffect(ref strSpotlightParentName);
                charaObject.EnableFootShadow(isEnable);
                charaObject.ActiveFollowSpotLight(isEnable);
            }
        };
        for (int l = 0; l < _characterObjects.Count; l++)
        {
            if (_characterData[l] != null)
            {
                action(_characterObjects[l], enableFootShadow);
            }
        }
        foreach (KeyValuePair<int, CharacterObject[]> item2 in _mapSpareCharacter)
        {
            CharacterObject[] value = item2.Value;
            foreach (CharacterObject arg2 in value)
            {
                action(arg2, arg2: false);
            }
        }
        yield break;
    }

    /// <summary>
    /// StageControllerの初期化を行う
    /// </summary>
    /// <returns></returns>
    private IEnumerator fnCreateStage()
    {
        _liveDirector = ViewLauncher.instance.liveDirector;
        _liveDirector.LoadMusicScoreCyalume();
        _liveDirector.LoadMusicScoreLyrics();
        _liveDirector.LoadMusicScore();

        GameObject bgName = LoadResource<GameObject>(_live3DSettings._bgName);
        if (bgName == null)
        {
            yield break;
        }
        GameObject bgNameObject = UnityEngine.Object.Instantiate(bgName);
        if (bgNameObject == null)
        {
            yield break;
        }
        bgNameObject.transform.SetParent(base.transform, worldPositionStays: false);
        for (int i = 0; i < _imageEffectLive3dList.Count; i++)
        {
            PostEffectLive3D postEffectLive3D = _imageEffectLive3dList[i];
            if (postEffectLive3D != null)
            {
                postEffectLive3D.setFocalTransform(bgNameObject.transform);
            }
        }
        _stageController = bgNameObject.GetComponentInChildren<StageController>();
        _stageController.charaIdArray = _uvMovieCharaIdArray;
        if (_stageController != null)
        {
            if (_live3DSettings._uvMovieAssetBundleCount == 0)
            {
                _stageController._UVMovieResources = null;
            }
            else
            {
                _stageController._UVMovieResources = new string[_live3DSettings._uvMovieAssetBundleCount];
                for (int j = 0; j < _live3DSettings._uvMovieAssetBundleCount; j++)
                {
                    _stageController._UVMovieResources[j] = _live3DSettings._uvMovieAssetBundleLabels[j];
                }
            }
            if (_live3DSettings._imgResourcesLabels != null)
            {
                _stageController._ImageResources = new Texture[_live3DSettings._imgResourcesLabels.Length];
                for (int k = 0; k < _live3DSettings._imgResourcesLabels.Length; k++)
                {
                    _stageController._ImageResources[k] = ResourcesManager.instance.LoadObject(_live3DSettings._imgResourcesLabels[k]) as Texture;
                }
            }
            if (_live3DSettings._mirrorScanMateriaLabels != null)
            {
                _stageController._mirorrScanLightMaterials = new Material[_live3DSettings._mirrorScanMateriaLabels.Length];
                for (int l = 0; l < _live3DSettings._mirrorScanMateriaLabels.Length; l++)
                {
                    _stageController._mirorrScanLightMaterials[l] = ResourcesManager.instance.LoadObject(_live3DSettings._mirrorScanMateriaLabels[l]) as Material;
                }
            }
        }
        CyalumeController3D pCyalume = GetComponentInChildren<CyalumeController3D>();
        if (pCyalume != null)
        {
            while (!pCyalume.initialized)
            {
                yield return null;
            }
        }
    }

    private IEnumerator fnCreateStageEffect()
    {
        for (int i = 0; i < _indirectLightShuft.Length; i++)
        {
            _indirectLightShuft[i].shaftType = _liveTimelineControl.data.indirectLightShuftSettings.shaftType;
        }
        if (_liveTimelineControl.data.indirectLightShuftSettings != null && LoadIndirectShaftTextures())
        {
            for (int j = 0; j < _indirectLightShuft.Length; j++)
            {
                _indirectLightShuft[j].SetTexture(_indirectLightShuftTexture, _indirectLightShuftTexture[2]);
            }
        }
        if (_colorCorrectionParameter != null)
        {
            _colorCorrectionParameter.Initialize();
        }
        LoadParticle();
        LoadMirrorScanLightBody();
        yield break;
    }

    private IEnumerator fnCreateProps()
    {
        List<string> listPropsNames = new List<string>();
        List<LiveTimelinePropsSettings.PropsConditionGroup> listProps = new List<LiveTimelinePropsSettings.PropsConditionGroup>(16);
        int i = 0;
        while (i < _characterObjects.Count)
        {
            if (_charaDirector[i] != null && _charaDirector[i].characterData != null)
            {
                listPropsNames.Clear();
                listProps.Clear();
                _charaDirector[i].characterData.GetCharacterPropsList(i, _liveTimelineControl, ref listPropsNames, listProps, isCheckOverlap: false, _charaDirector[i].characterData.cardId);
                if (listPropsNames.Count != 0)
                {
                    for (int j = 0; j < listPropsNames.Count; j++)
                    {
                        _propsManager.Create(i, listPropsNames[j], _characterObjects[i].gameObject, _characterObjects[i], listProps[j], true);
                        _propsManager.SetCutScale(i, _characterObjects[i].createInfo.charaData.heightId);
                        _propsManager.SetScale(i, _characterObjects[i].bodyScaleSubScale);
                    }
                    yield return null;
                }
            }
            int num = i + 1;
            i = num;
        }
    }

    /// <summary>
    /// リッチ用のカメラの生成を行う
    /// リッチではマルチカメラ、透過カメラ、クロスフェードカメラが有効になる
    /// </summary>
    private IEnumerator fnCreateRichCamera()
    {
        int cameraNum = 0;
        if (_liveTimelineControl.data.multiCameraSettings != null)
        {
            cameraNum = _liveTimelineControl.data.multiCameraSettings.cameraNum;
        }
        if (cameraNum > 0)
        {
            yield return InitializeMultiCamera(cameraNum);
        }
        _isEnableTransparentCamera = false;
        if (_liveTimelineControl.data.transparentFXCameraSettings != null && _liveTimelineControl.data.transparentFXCameraSettings.isEnable)
        {
            InitializeTransparentCamera();
        }
        InitializeCrossFadeCamera();
    }

    /*
    private void Load3DLiveSwapLocationDataCSV(out MasterMusicvideoSwapLocation data, string csvPath, bool isServer = false)
    {
        TextAsset textAsset = ResourcesManager.instance.LoadObject(csvPath) as TextAsset;
        if (textAsset != null)
        {
            ArrayList list = Utility.ConvertCSV(textAsset.ToString());
            data = new MasterMusicvideoSwapLocation(list);
        }
        else
        {
            data = null;
        }
    }
    */

    private IEnumerator fnCreateCamera()
    {
        if (!string.IsNullOrEmpty(_live3DSettings._cameraName))
        {
            _cameraRuntimeAnimator = LoadResource<RuntimeAnimatorController>(_live3DSettings._cameraName);
            _cameraAnimator.runtimeAnimatorController = _cameraRuntimeAnimator;
            yield return null;
        }
        if (_screenFadeArray == null)
        {
            _screenFadeArray = new ScreenFade[_cameraNodes.Length];
            for (int i = 0; i < _cameraNodes.Length; i++)
            {
                ScreenFade screenFade = _cameraNodes[i].AddComponent<ScreenFade>();
                screenFade.Initialize();
                _screenFadeArray[i] = screenFade;
                screenFade.enabled = false;
            }
        }
        bool flag = false;
        int count = _liveTimelineControl.data.GetWorkSheetList().Count;
        for (int j = 0; j < count; j++)
        {
            LiveTimelineWorkSheet workSheet = _liveTimelineControl.data.GetWorkSheet(j);
            if (workSheet.monitorCameraPosKeys.Count > 0 || workSheet.monitorLookAtPosKeys.Count > 0)
            {
                flag = true;
                LiveTimelineMonitorCameraSettings monitorCameraSettings = _liveTimelineControl.data.monitorCameraSettings;
                int cntCamera = monitorCameraSettings.cntCamera;
                if (workSheet.monitorCameraPosKeys.Count == 1 && workSheet.monitorLookAtPosKeys.Count == 1 && cntCamera == 0)
                {
                    monitorCameraSettings.cntCamera = 1;
                }
                break;
            }
        }
        if (flag)
        {
            yield return InitializeMonitorCamera();
        }
        if (_liveTimelineControl.data.multiCameraSettings.cameraNum > 0)
        {
            LoadMultiCameraMask();
        }
        yield return InitMainCameraRenderTarget();
    }

    /// <summary>
    /// CharacterObjectにIndexを割り当てる
    /// </summary>
    private void BindResourceCharacter(int index, CharacterObject charaObject)
    {
        if (index < _characterObjects.Count && index < MemberUnitNumber)
        {
            CharacterObject component = charaObject.GetComponent<CharacterObject>();
            if (!(component == null))
            {
                component.BindResource(index + 1);
            }
        }
    }

    private void LoadMirrorScanLightBody()
    {
        LoadStageObject(_liveTimelineControl.data.mirrorScanLightBodyPrefabNames, "3D/MirrorScanLight/Body/", "mirrorscan_body_", delegate (GameObject instance)
        {
            ProjectorController[] componentsInChildren = instance.GetComponentsInChildren<ProjectorController>();
            _stageController.SetProjectorControllers(componentsInChildren);
        });
        _stageController.SetupMirrorScanMaterial();
        _stageController.SetupMirrorScanLight();
    }

    private void LoadStageObject(string[] prefabNames, string rootPath, string baseName, Action<GameObject> registerAction, string hqSuffix = "")
    {
        if (_stageController == null || prefabNames == null)
        {
            return;
        }
        for (int i = 0; i < prefabNames.Length; i++)
        {
            if (baseName == "particle_" && hqSuffix != "")
            {
                if (!_live3DSettings._useHqParticlesDic.ContainsKey(prefabNames[i]))
                {
                    hqSuffix = "";
                }
                else if (!_live3DSettings._useHqParticlesDic[prefabNames[i]])
                {
                    hqSuffix = "";
                }
            }
            string text = rootPath + baseName + prefabNames[i] + hqSuffix;
            string path = text + "/Prefabs/pf_" + baseName + prefabNames[i] + hqSuffix;
            GameObject gameObject = LoadResource<GameObject>(path);
            if (!(gameObject == null))
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
                if (!(gameObject2 == null))
                {
                    gameObject2.transform.SetParent(_stageController.gameObject.transform);
                    registerAction(gameObject2);
                }
            }
        }
    }

    private void LoadParticle()
    {
        int num = _liveTimelineControl.data.particlePrefabNames.Length;
        if (num <= 0)
        {
            return;
        }
        List<string> list = new List<string>();
        List<string> list2 = new List<string>();
        List<string> list3 = new List<string>();
        for (int i = 0; i < num; i++)
        {
            string text = _liveTimelineControl.data.particlePrefabNames[i];
            switch (StageController.GetParticleType(text))
            {
                case StageController.eParticleType.Element:
                    list.Add(text);
                    break;
                case StageController.eParticleType.Confetti:
                    list2.Add(text);
                    break;
                case StageController.eParticleType.Effect:
                    list3.Add(text);
                    break;
                case StageController.eParticleType.Rich_Effect:
                    list3.Add(text);
                    break;
            }
        }
        string hqSuffix = ((_liveTimelineControl.data.isUseHQParticle) ? "_hq" : "");
        Action<List<string>, Action<GameObject>> obj = delegate (List<string> lstParticle, Action<GameObject> fn)
        {
            LoadStageObject(lstParticle.ToArray(), "3D/Particle/", "particle_", fn, hqSuffix);
        };
        obj(list, delegate
        {
        });
        obj(list2, delegate (GameObject instance)
        {
            _stageController.SetParticleControllers(instance.GetComponentsInChildren<ParticleController>());
        });
        _stageController.SetupParticle();
        obj(list3, delegate
        {
            EffectController componentInChildren = instance.GetComponentInChildren<EffectController>();
            if (componentInChildren.Initialize())
            {
                _stageController.effectControllerList.Add(componentInChildren);
            }
        });
    }

    private bool LoadIndirectShaftTextures()
    {
        bool flag = false;
        int count = _liveTimelineControl.data.GetWorkSheetList().Count;
        for (int i = 0; i < count; i++)
        {
            if (_liveTimelineControl.data.GetWorkSheet(i).lightShuftKeys.Count > 0)
            {
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            return false;
        }
        int id = _liveTimelineControl.data.indirectLightShuftSettings.id;
        if (_liveTimelineControl.data.indirectLightShuftSettings.shaftType == LiveTimelineIndirectLightShuftSettings.ShaftType.Vr03)
        {
            string[] shaftTextureNames = _liveTimelineControl.data.indirectLightShuftSettings.shaftTextureNames;
            Dictionary<string, Texture2D> dictionary = new Dictionary<string, Texture2D>();
            for (int j = 0; j < shaftTextureNames.Length; j++)
            {
                if (!dictionary.ContainsKey(shaftTextureNames[j]))
                {
                    string objectName = string.Format(IndirectLightShuftResourcePath, id, shaftTextureNames[j]);
                    Texture2D texture2D = ResourcesManager.instance.LoadObject<Texture2D>(objectName);
                    if (!(texture2D == null))
                    {
                        dictionary.Add(shaftTextureNames[j], texture2D);
                    }
                }
            }
            _indirectLightShuftTexture = new Texture2D[shaftTextureNames.Length];
            for (int k = 0; k < shaftTextureNames.Length; k++)
            {
                if (!dictionary.ContainsKey(shaftTextureNames[k]))
                {
                    _indirectLightShuftTexture[k] = Texture2D.blackTexture;
                }
                else
                {
                    _indirectLightShuftTexture[k] = dictionary[shaftTextureNames[k]];
                }
            }
        }
        else
        {
            string[] indirectLightShaftResourceNames = GetIndirectLightShaftResourceNames();
            _indirectLightShuftTexture = new Texture2D[indirectLightShaftResourceNames.Length];
            for (int l = 0; l < indirectLightShaftResourceNames.Length; l++)
            {
                string objectName2 = string.Format(IndirectLightShuftResourcePath, id, indirectLightShaftResourceNames[l]);
                _indirectLightShuftTexture[l] = ResourcesManager.instance.LoadObject<Texture2D>(objectName2);
                if (_indirectLightShuftTexture[l] == null)
                {
                    _indirectLightShuftTexture = null;
                    return false;
                }
            }
        }
        return true;
    }

    private void SetupCharacterLocator()
    {
        if (_liveTimelineControl == null)
        {
            return;
        }
        int num = _characterObjects.Count;
        if (num > LiveTimelineControl.liveCharaPositionMax)
        {
            num = LiveTimelineControl.liveCharaPositionMax;
        }
        int i;
        for (i = 0; i < num; i++)
        {
            if (_liveTimelineControl.liveCharactorLocators[i] == null)
            {
                ILiveTimelineCharactorLocator liveTimelineCharactorLocator = _characterObjects[i];
                if (liveTimelineCharactorLocator != null)
                {
                    _liveTimelineControl.SetCharactorLocator((LiveCharaPosition)i, liveTimelineCharactorLocator);
                }
            }
        }
        for (; i < LiveTimelineControl.liveCharaPositionMax; i++)
        {
            _liveTimelineControl.SetCharactorLocator((LiveCharaPosition)i, null);
        }
    }

    private void AddMultiCameraMaskAssetBundle()
    {
        string[] array = MultiCameraManager.MakeAssetBundleList(_liveTimelineControl.data.multiCameraMaskSettings);
        foreach (string assetName in array)
        {
            AddAssetBundleList(assetName);
        }
    }

    private void LoadMultiCameraMask()
    {
        if (!(_multiCameraManager == null))
        {
            _multiCameraManager.LoadMaskResource(_liveTimelineControl.data.multiCameraMaskSettings);
        }
    }

    /// <summary>
    /// LiveSettingを取得し、読み込みを行う
    /// </summary>
    private IEnumerator fnMakeLiveSetting()
    {
        /* この2つのアセットはMasterDBManagerで管理をする
        yield return AssetManager.instance.DownloadFromFilename("3d_live.unity3d");
        yield return AssetManager.instance.DownloadFromFilename("3d_chara_data.unity3d");
        yield return ResourcesManager.instance.LoadAsset("3d_live.unity3d", null);
        yield return ResourcesManager.instance.LoadAsset("3d_chara_data.unity3d", null);
        */
        yield return null;
        MakeLiveSettingInternal();
        MakeFaceTypeConvInternal();
    }

    private IEnumerator fnLoadAssetBundlePrepareTimelineData()
    {
        List<string> downloadList = new List<string>();
        Preload(downloadList);
        //yield return ResourcesManager.instance.DownloadAssetGroupWithLoadAsset(downloadList, null);
        yield return AssetManager.instance.DownloadFromFilenames(downloadList);
        yield return ResourcesManager.instance.LoadAssetGroup(downloadList);
        yield return fnPrepareTimelineData();
        downloadList.Clear();
        downloadList.AddRange(GetAssetBundleDLListForConditionMotionChange());
        //yield return ResourcesManager.instance.DownloadAssetGroupWithLoadAsset(downloadList, null);
        yield return AssetManager.instance.DownloadFromFilenames(downloadList);
        yield return ResourcesManager.instance.LoadAssetGroup(downloadList);
        yield return fnMakeAssetBundleList();
        //yield return ResourcesManager.instance.DownloadAssetGroupWithLoadAsset(lstAssetBundleName, null);
        yield return AssetManager.instance.DownloadFromFilenames(lstAssetBundleName);
        yield return ResourcesManager.instance.LoadAssetGroup(lstAssetBundleName);
        lstAssetBundleName.AddRange(downloadList);
    }

    /// <summary>
    /// HQパーティクルの読み込み
    /// </summary>
    private IEnumerator fnPrepareHQParticle()
    {
        for (int i = 0; i < _liveTimelineControl.data.particlePrefabNames.Length; i++)
        {
            string text = _liveTimelineControl.data.particlePrefabNames[i];
            string text2 = ((_liveTimelineControl.data.isUseHQParticle) ? "_hq" : "");
            if (!string.IsNullOrEmpty(text))
            {
                bool value = false;
                string assetName = "3d_particle_" + text + text2 + ".unity3d";
                if (text2 != "")
                {
                    value = true;
                }
                if (!ResourcesManager.instance.ExistsAssetBundleManifest(assetName))
                {
                    value = false;
                }
                if (_live3DSettings._useHqParticlesDic.ContainsKey(text))
                {
                    _live3DSettings._useHqParticlesDic[text] = value;
                }
                else
                {
                    _live3DSettings._useHqParticlesDic.Add(text, value);
                }
            }
        }
        yield break;
    }

    private void SetupLiveTimelineControl(LiveTimelineControl timelineControl)
    {
        if (_liveTimelineControl != null)
        {
            return;
        }
        _liveTimelineControl = timelineControl;
        if (_liveTimelineControl != null)
        {
            _liveTimelineControl.OnUpdateProps += UpdateProps;
            _liveTimelineControl.OnUpdatePropsAttach += UpdatePropsAttach;
            _liveTimelineControl.OnUpdateParticle += UpdateParticle;
            _liveTimelineControl.OnUpdateParticleGroup += UpdateParticleGroup;
            _liveTimelineControl.OnUpdateEffect += UpdateEffect;
            _liveTimelineControl.OnUpdateFormationOffset += UpdateFormationOffset;
            _liveTimelineControl.OnUpdateSweatLocator += UpdateSweatLocator;
            _liveTimelineControl.OnUpdateShaderControl += UpdateShaderControl;
            _liveTimelineControl.OnUpdateCySpring += UpdateCySpring;
            _liveTimelineControl.OnUpdateIK += UpdateIK;
            _liveTimelineControl.OnUpdateCharaFootLight += UpdateCharaFootLight;
            _liveTimelineControl.OnUpdateStageGazeControl += UpdateStageGazeControl;
            _liveTimelineControl.OnCharaHeightMotionUpdateInfo += UpdateCharaHeightMotion;
            _liveTimelineControl.OnCharaWindUpdate += UpdateCharaWind;
            _liveTimelineControl.OnUpdateDressChange += UpdateDressChange;
            _liveTimelineControl.OnUpdateCrossFadeCamera += UpdateCrossFadeCamera;
            _liveTimelineControl.OnUpdateMultiCamera += UpdateMultiCamera;
        }
        _sharedShaderParam = SharedShaderParam.instance;
        sPropsTimelineDataDictionary.Clear();
        sPropsTimelineDataDictionary.Add(FNVHash.Generate("PropsCenter"), 1);
        sPropsTimelineDataDictionary.Add(FNVHash.Generate("PropsOther"), 32766);
        sPropsTimelineDataDictionary.Add(FNVHash.Generate("PropsFlags"), 0);
    }

    private void DeleteTimelineDelegetes()
    {
        if (!(_liveTimelineControl == null))
        {
            _liveTimelineControl.OnUpdateProps -= UpdateProps;
            _liveTimelineControl.OnUpdatePropsAttach -= UpdatePropsAttach;
            _liveTimelineControl.OnUpdateParticle -= UpdateParticle;
            _liveTimelineControl.OnUpdateEffect -= UpdateEffect;
            _liveTimelineControl.OnUpdateFormationOffset -= UpdateFormationOffset;
            _liveTimelineControl.OnUpdateSweatLocator -= UpdateSweatLocator;
            _liveTimelineControl.OnUpdateShaderControl -= UpdateShaderControl;
            _liveTimelineControl.OnUpdateCySpring -= UpdateCySpring;
            _liveTimelineControl.OnUpdateIK -= UpdateIK;
            _liveTimelineControl.OnUpdateCharaFootLight -= UpdateCharaFootLight;
            _liveTimelineControl.OnUpdateStageGazeControl -= UpdateStageGazeControl;
            _liveTimelineControl.OnCharaHeightMotionUpdateInfo -= UpdateCharaHeightMotion;
            _liveTimelineControl.OnUpdateCrossFadeCamera -= UpdateCrossFadeCamera;
            _liveTimelineControl.OnUpdateMultiCamera -= UpdateMultiCamera;
            _liveTimelineControl.OnCharaWindUpdate -= UpdateCharaWind;
            _liveTimelineControl.OnUpdateDressChange -= UpdateDressChange;
        }
    }

    public bool isExistsRipSyncTimelineKeyframe()
    {
        if (isTimelineControlled && _liveTimelineControl.data.GetWorkSheet(0) != null)
        {
            return _liveTimelineControl.data.GetWorkSheet(0).ripSyncKeys.Count > 0;
        }
        return false;
    }

    public LiveTimelineData.ParticleControllMode GetParticleControllType()
    {
        if (_liveTimelineControl == null || _liveTimelineControl.data == null)
        {
            return LiveTimelineData.ParticleControllMode.Score;
        }
        return _liveTimelineControl.data.particleControllMode;
    }

    public bool IsEnableFlare()
    {
        int flareDataGroupCount = _liveTimelineControl.data.lensFlareSetting.flareDataGroupCount;
        if (flareDataGroupCount == 0 && _isHaveHqStage)
        {
            return true;
        }
        for (int i = 0; i < flareDataGroupCount; i++)
        {
            LiveTimelineLensFlareSetting.FlareDataGroup flareDataGroup = _liveTimelineControl.data.lensFlareSetting.flareDataGroup[i];
            if (flareDataGroup.objectId > 0 && (!flareDataGroup.isHqOnly))
            {
                return true;
            }
        }
        return false;
    }

    public float GetMaxForcalSize()
    {
        if (_liveTimelineControl == null || _liveTimelineControl.data.maxForcalSize < 30f)
        {
            return 30f;
        }
        return _liveTimelineControl.data.maxForcalSize;
    }

    public bool GetIsUseGameSettingToParticle()
    {
        if (_liveTimelineControl == null || _liveTimelineControl.data == null)
        {
            return false;
        }
        return _liveTimelineControl.data.isUseGameSettingToParticle;
    }

    private bool InstanciateTimeline(string prefabPath)
    {
        GameObject gameObject = null;
        LiveTimelineControl liveTimelineControl = null;
        GameObject gameObject2 = (GameObject)ResourcesManager.instance.LoadObject(prefabPath);
        if (!(gameObject2 == null))
        {
            gameObject = UnityEngine.Object.Instantiate(gameObject2);
            gameObject.name = gameObject2.name;
            if (!(gameObject == null))
            {
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;
                liveTimelineControl = gameObject.GetComponent<LiveTimelineControl>();
                if (!(liveTimelineControl == null) && InitializeTimeline(liveTimelineControl))
                {
                    SetupLiveTimelineControl(liveTimelineControl);
                    return true;
                }
            }
        }
        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
        return false;
    }

    private void CheckSwitchSheetCharacter(LiveTimelineControl timelineControl)
    {
        LiveTimelineSwitchSheetSetting switchSheetSettings = timelineControl.data.switchSheetSettings;
        _switchSheetCondition = true;
        _switchSheetEnable = false;
        if (switchSheetSettings == null)
        {
            return;
        }
        bool flag = false;
        int count = _characterObjects.Count;
        int num = timelineControl.data.switchSheetSettings.characterNo.Length;
        for (int i = 0; i < num && i < count; i++)
        {
            if (switchSheetSettings.characterNo[i] != 0)
            {
                flag = true;
                if (_characterObjects[i].data.charaId != switchSheetSettings.characterNo[i])
                {
                    _switchSheetCondition = false;
                    break;
                }
            }
        }
        if (flag)
        {
            _switchSheetEnable = true;
        }
    }

    private void CheckSwitchSheetAltCharacter(LiveTimelineControl timelineControl, CuttEventParam_SwitchSheetAltFromCharacterCondition eventParam)
    {
        LiveTimelineSwitchSheetAltSetting switchSheetAltSettings = timelineControl.data.switchSheetAltSettings;
        _switchSheetAltSheetNumber = -1;
        bool flag = eventParam != null && eventParam.alterSheetMode != LiveTimelineData.AlterSheetMode.None;
        if ((flag ? eventParam.alterSheetMode : switchSheetAltSettings.alterSheetMode) != LiveTimelineData.AlterSheetMode.LeftHanded)
        {
            return;
        }
        _switchSheetAltSheetNumber = 0;
        int num = 0;
        LiveTimelineSwitchSheetAltSetting.LeftHanded leftHanded = (flag ? eventParam.leftHanded : switchSheetAltSettings.leftHanded);
        for (int i = 0; i < leftHanded.targetCharaList.Length; i++)
        {
            int index = leftHanded.targetCharaList[i];
            bool flag2 = false;
            int charaId = _characterObjects[index].data.charaId;
            if (MasterDBManager.instance.masterCharaData.Get(charaId).hand == 3002)
            {
                flag2 = true;
            }
            _switchSheetAltSheetNumber |= (flag2 ? (1 << num) : 0);
            num++;
        }
    }

    private void InitializeCuttConditionVm(LiveTimelineControl timelineControl)
    {
        CuttConditionOption component = timelineControl.gameObject.GetComponent<CuttConditionOption>();
        if (component != null && component.changeSheet != null)
        {
            _cuttVmrun = new CuttMotionVmRun(component.changeSheet, ref _characterData);
            _cuttVmrun.ResultUpdate();
        }
    }

    private int CheckSwitchSheetCharacterConditionVm(LiveTimelineControl timelineControl, CuttEventParam_SwitchSheetFromCharacterConditionVm eventParam, CuttMotionVmRun vmrun)
    {
        int result = 0;
        if (vmrun.Result(eventParam.conditionIndex, out result))
        {
            return result;
        }
        return 0;
    }

    private bool InitializeTimeline(LiveTimelineControl timelineControl)
    {
        if (timelineControl == null)
        {
            return false;
        }
        IntiializeConditionForMotionChange(timelineControl);
        IntiializeConditionForDressChange(timelineControl);
        timelineControl.Initialize(_live3DSettings._charaMotion);
        for (int i = 0; i < kTimelineCameraIndices.Length; i++)
        {
            int num = kTimelineCameraIndices[i];
            if (num < _cameraObjects.Length)
            {
                timelineControl.SetTimelineCamera(_cameraObjects[num], i);
            }
        }
        timelineControl.eventPublisher.Subscribe(LiveTimelineEventID.SwitchCamera, delegate (LiveTimelineKeyEventData.EventData eventData)
        {
            CuttEventParam_SwitchCamera parameter4 = eventData.GetParameter<CuttEventParam_SwitchCamera>();
            if (parameter4 != null)
            {
                _activeCameraIndex = parameter4.cameraID;
            }
        });
        timelineControl.OnUpdateCameraSwitcher += delegate (int cameraIndex_)
        {
            if (cameraIndex_ < 0)
            {
                _activeCameraIndex = 0;
            }
            else if (cameraIndex_ < kTimelineCameraIndices.Length)
            {
                _activeCameraIndex = kTimelineCameraIndices[cameraIndex_];
            }
        };
        timelineControl.OnUpdateLipSync += delegate (LiveTimelineKeyRipSyncData keyData_, float liveTime_)
        {
            for (int k = 0; k < MemberUnitNumber; k++)
            {
                int num4 = k % MemberUnitNumber;
                if (_characterObjects[num4].isSetting)
                {
                    _characterObjects[num4].faceController.AlterUpdateAutoLip(liveTime_, keyData_, (LiveCharaPosition)num4);
                }
            }
        };
        timelineControl.OnUpdateFacial += delegate (FacialDataUpdateInfo updateInfo_, float liveTime_, LiveCharaPosition charaPos_)
        {
            if ((int)charaPos_ < MemberUnitNumber && !(_characterObjects[(int)charaPos_].faceController == null))
            {
                int activeDressId = _characterObjects[(int)charaPos_].createInfo.activeDressId;
                _characterObjects[(int)charaPos_].faceController.AlterUpdateFacialNew((int)charaPos_, activeDressId, liveTime_, 60, ref updateInfo_);
                if (updateInfo_.eyeTrack != null)
                {
                    if (_characterObjects[(int)charaPos_].eyeTraceController != null)
                    {
                        _characterObjects[(int)charaPos_].eyeTraceController.SetDelayRateRate(updateInfo_.eyeTrack.speed);
                    }
                    _characterObjects[(int)charaPos_].SetUpOffset(updateInfo_.eyeTrack.upOffset);
                    _characterObjects[(int)charaPos_].SetDownOffset(updateInfo_.eyeTrack.downOffset);
                    if (updateInfo_.eyeTrack.targetCharaIds != null && updateInfo_.eyeTrack.targetCharaIds.Length != 0)
                    {
                        for (int j = 0; j < updateInfo_.eyeTrack.targetCharaIds.Length; j++)
                        {
                            if (updateInfo_.eyeTrack.targetCharaIds[j] == _characterObjects[(int)charaPos_].createInfo.charaData.charaId)
                            {
                                _characterObjects[(int)charaPos_].eyeTrackAvert = !updateInfo_.eyeTrack.IsDisableAvertEye();
                                _characterObjects[(int)charaPos_].eyeTrackAvertFacial = !updateInfo_.eyeTrack.IsDisableAvertEyeFace();
                                _characterObjects[(int)charaPos_].eyeTrackForceAvert = updateInfo_.eyeTrack.IsForceAvertEye();
                                _characterObjects[(int)charaPos_].eyeTrackForceAvertFacial = updateInfo_.eyeTrack.IsForceAvertEyeFace();
                                break;
                            }
                            _characterObjects[(int)charaPos_].ResetEyeTrackAvertParameter();
                        }
                    }
                    else
                    {
                        _characterObjects[(int)charaPos_].eyeTrackAvert = !updateInfo_.eyeTrack.IsDisableAvertEye();
                        _characterObjects[(int)charaPos_].eyeTrackAvertFacial = !updateInfo_.eyeTrack.IsDisableAvertEyeFace();
                        _characterObjects[(int)charaPos_].eyeTrackForceAvert = updateInfo_.eyeTrack.IsForceAvertEye();
                        _characterObjects[(int)charaPos_].eyeTrackForceAvertFacial = updateInfo_.eyeTrack.IsForceAvertEyeFace();
                    }
                    if (updateInfo_.eyeTrack.IsWorldPosition())
                    {
                        _characterObjects[(int)charaPos_].eyeTrackTargetType = FacialEyeTrackTarget.World;
                    }
                    else
                    {
                        _characterObjects[(int)charaPos_].eyeTrackTargetType = updateInfo_.eyeTrack.target;
                    }
                    _characterObjects[(int)charaPos_].eyeTargetOffset = updateInfo_.eyeTrackOffset;
                    if (updateInfo_.eyeTrack.IsUp())
                    {
                        _characterObjects[(int)charaPos_].eyeTrackTargetVPos = CharacterObject.EyeTargetVerticalPos.Up;
                    }
                    else if (updateInfo_.eyeTrack.IsDown())
                    {
                        _characterObjects[(int)charaPos_].eyeTrackTargetVPos = CharacterObject.EyeTargetVerticalPos.Down;
                    }
                    else
                    {
                        _characterObjects[(int)charaPos_].eyeTrackTargetVPos = CharacterObject.EyeTargetVerticalPos.Middle;
                    }
                }
            }
        };
        timelineControl.eventPublisher.Subscribe(LiveTimelineEventID.SwitchSheetFromCharacterCondition, delegate (LiveTimelineKeyEventData.EventData eventData)
        {
            CuttEventParam_SwitchSheetFromCharacterCondition parameter3 = eventData.GetParameter<CuttEventParam_SwitchSheetFromCharacterCondition>();
            if (parameter3 != null)
            {
                int num3 = parameter3.sheetIndex;
                if (!_switchSheetEnable)
                {
                    num3 = -1;
                }
                else if (num3 == -1 || !_switchSheetCondition)
                {
                    num3 = 0;
                }
                timelineControl.SetActiveSheetIndex(num3);
            }
        });
        timelineControl.eventPublisher.Subscribe(LiveTimelineEventID.SwitchSheetAltFromCharacterCondition, delegate (LiveTimelineKeyEventData.EventData eventData)
        {
            CuttEventParam_SwitchSheetAltFromCharacterCondition parameter2 = eventData.GetParameter<CuttEventParam_SwitchSheetAltFromCharacterCondition>();
            if (parameter2 != null)
            {
                CheckSwitchSheetAltCharacter(timelineControl, parameter2);
                bool isSheetAlt = parameter2.isSheetAlt;
                timelineControl.SetActiveSheetIndex((isSheetAlt && _switchSheetAltSheetNumber >= 0) ? _switchSheetAltSheetNumber : 0, isAlt: true);
            }
        });
        InitializeCuttConditionVm(timelineControl);
        timelineControl.eventPublisher.Subscribe(LiveTimelineEventID.SwitchSheetFromCharacterConditionVm, delegate (LiveTimelineKeyEventData.EventData eventData)
        {
            CuttEventParam_SwitchSheetFromCharacterConditionVm parameter = eventData.GetParameter<CuttEventParam_SwitchSheetFromCharacterConditionVm>();
            if (parameter != null)
            {
                int index = 0;
                if (parameter.isSheetAlt && _cuttVmrun != null)
                {
                    int num2 = CheckSwitchSheetCharacterConditionVm(timelineControl, parameter, _cuttVmrun);
                    index = ((parameter.sheetIndex != 0) ? parameter.sheetIndex : num2);
                }
                timelineControl.SetActiveSheetIndex(index, isAlt: true);
            }
        });
        timelineControl.OnUpdatePostEffect += onUpdatePostEffect;
        timelineControl.OnUpdatePostFilm += onUpdatePostFilm;
        timelineControl.OnUpdateScreenFade += onUpdateScreenFade;
        timelineControl.OnUpdatePostFilm2 += onUpdatePostFilm2;
        timelineControl.OnEnvironmentCharacterShadow += onEnvironemntCharacterShadow;
        timelineControl.OnEnvironmentGlobalLight += onEnvironemntGlobalLight;
        timelineControl.OnGlobalFog += OnEnvironmentGlobalFog;
        timelineControl.OnUpdateTiltShift += OnUpdateTiltShift;
        timelineControl.OnLightShuft += OnLightShaft;
        timelineControl.OnColorCorrection += OnColorCorrection;
        return true;
    }

    private void onEnvironemntCharacterShadow(ref EnvironmentCharacterShadowUpdateInfo updateInfo)
    {
        for (int i = 0; i < _characterFlags.Length; i++)
        {
            CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID((LiveCharaPosition)i);
            if (!(characterObjectFromPositionID != null))
            {
                continue;
            }
            bool enableRealtimeShadow = updateInfo.positionFlag.hasFlag(_characterFlags[i]);
            CharacterObject.RealtimeShadowType realtimeShadowType2 = (characterObjectFromPositionID.realtimeShadowType = (updateInfo.softShadow ? CharacterObject.RealtimeShadowType.SoftShadow : CharacterObject.RealtimeShadowType.HardShadow));
            characterObjectFromPositionID.enableRealtimeShadow = enableRealtimeShadow;
            CharacterObject[] spareCharacters = characterObjectFromPositionID.spareCharacters;
            if (spareCharacters != null)
            {
                CharacterObject[] array = spareCharacters;
                foreach (CharacterObject obj in array)
                {
                    obj.realtimeShadowType = realtimeShadowType2;
                    obj.enableRealtimeShadow = enableRealtimeShadow;
                }
            }
        }
    }

    private void onEnvironemntGlobalLight(ref EnvironmentGlobalLightUpdateInfo updateInfo)
    {
        if (!_stageController.UpdateEnvironemntGlobalLight(ref updateInfo))
        {
            _GlobalLightDir.x = updateInfo.lightDirection.x;
            _GlobalLightDir.y = updateInfo.lightDirection.y;
            _GlobalLightDir.z = updateInfo.lightDirection.z;
            if (_enableTimelineGlobalRimParam)
            {
                _GlobalRimRate = updateInfo.globalRimRate;
                _GlobalRimShadowRate = updateInfo.globalRimShadowRate;
                _GlobalSpecularRate = updateInfo.globalRimSpecularRate;
                _GlobalToonRate = updateInfo.globalToonRate;
            }
        }
    }

    private void OnLightShaft(ref LightShuftUpdateInfo updateInfo)
    {
        if (_indirectLightShuft == null)
        {
            return;
        }
        for (int i = 0; i < _indirectLightShuft.Length; i++)
        {
            if (_indirectLightShuft[i] != null)
            {
                PostEffectLive3D.IndirectLightShafts obj = _indirectLightShuft[i];
                obj.enabled = updateInfo._enable;
                obj.angle = updateInfo._angle / 360f;
                obj.speed = updateInfo._speed;
                obj.scale = updateInfo._scale;
                obj.offset = updateInfo._offset;
                obj.alpha = updateInfo._alpha;
                obj.alpha2 = updateInfo._alpha2;
                obj.maskAlpha = updateInfo._maskAlpha;
            }
        }
    }

    private void OnColorCorrection(ref ColorCorrectionUpdateInfo updateInfo)
    {
        if (_colorCorrectionParameter == null)
        {
            return;
        }
        AnimationCurve redCurve = updateInfo._redCurve;
        AnimationCurve greenCurve = updateInfo._greenCurve;
        AnimationCurve blueCurve = updateInfo._blueCurve;
        if (redCurve != null && greenCurve != null && blueCurve != null)
        {
            _colorCorrectionParameter.saturation = updateInfo._saturation;
            _colorCorrectionParameter.selectiveCc = updateInfo._selective;
            _colorCorrectionParameter.selectiveFromColor = updateInfo._keyColor;
            _colorCorrectionParameter.selectiveToColor = updateInfo._targetColor;
            _colorCorrectionParameter.UpdateParameters(redCurve, greenCurve, blueCurve);
        }
        for (int i = 0; i < _colorCorrectionCurves.Length; i++)
        {
            PostEffectLive3D.ColorCorrection colorCorrection = _colorCorrectionCurves[i];
            if (colorCorrection != null)
            {
                colorCorrection.enabled = updateInfo._enable;
            }
        }
    }

    private void onUpdatePostFilm(ref PostFilmUpdateInfo updateInfo)
    {
        int activeCameraIndex = _activeCameraIndex;
        if (activeCameraIndex < _imageEffectLive3dList.Count)
        {
            PostEffectLive3D postEffectLive3D = _imageEffectLive3dList[activeCameraIndex];
            UpdatePostFilm(postEffectLive3D.screenOverlay, ref updateInfo);
            if (_crossFadeCamera != null)
            {
                UpdatePostFilm(_crossFadeCamera.postEffectLive3D.screenOverlay, ref updateInfo);
            }
        }
    }

    private void onUpdatePostFilm2(ref PostFilmUpdateInfo updateInfo)
    {
        int activeCameraIndex = _activeCameraIndex;
        if (activeCameraIndex < _imageEffectLive3dList.Count)
        {
            PostEffectLive3D postEffectLive3D = _imageEffectLive3dList[activeCameraIndex];
            UpdatePostFilm(postEffectLive3D.screenOverlay2, ref updateInfo);
            if (_crossFadeCamera != null)
            {
                UpdatePostFilm(_crossFadeCamera.postEffectLive3D.screenOverlay2, ref updateInfo);
            }
        }
    }

    private void onUpdateScreenFade(ref ScreenFadeUpdateInfo updateInfo)
    {
        int activeCameraIndex = _activeCameraIndex;
        if (activeCameraIndex < _screenFadeArray.Length)
        {
            ScreenFade screenFade = _screenFadeArray[activeCameraIndex];
            bool enableFade = updateInfo.enable;
            if (updateInfo.onlyQuality3DLight)
            {
                enableFade = false;
            }
            if (enableFade)
            {
                screenFade.enabled = true;
                screenFade.fadeColor = updateInfo.color;
            }
            else
            {
                screenFade.enabled = false;
            }
        }
    }

    private void OnEnvironmentGlobalFog(ref GlobalFogUpdateInfo updateInfo)
    {
        int activeCameraIndex = GetActiveCameraIndex();
        if (activeCameraIndex >= 0)
        {
            PostEffectLive3D postEffectLive3D = _imageEffectLive3dList[activeCameraIndex];
            postEffectLive3D.useRadialDistance = updateInfo.useRadialDistance;
            postEffectLive3D.heightFog = updateInfo.isHeight;
            postEffectLive3D.distanceFog = updateInfo.isDistance;
            postEffectLive3D.startDistance = updateInfo.startDistance;
            postEffectLive3D.fogHeight = updateInfo.height;
            postEffectLive3D.fogHeightDensity = updateInfo.heightDensity;
            postEffectLive3D.fogColor = updateInfo.color;
            postEffectLive3D.fogMode = updateInfo.fogMode;
            postEffectLive3D.expDensity = updateInfo.expDensity;
            postEffectLive3D.fogStart = updateInfo.start;
            postEffectLive3D.fogEnd = updateInfo.end;
            postEffectLive3D.fogHeightOption = updateInfo.heightOption;
            postEffectLive3D.fogDistanceOption = updateInfo.distanceOption;
        }
    }

    private void OnUpdateTiltShift(ref TilsShiftUpdateInfo updateInfo)
    {
        int activeCameraIndex = GetActiveCameraIndex();
        if (activeCameraIndex < 0)
        {
            return;
        }
        PostEffectLive3D postEffectLive3D = _imageEffectLive3dList[activeCameraIndex];
        if (postEffectLive3D != null)
        {
            PostEffectLive3D.TiltShift tiltShift = postEffectLive3D.tiltShift;
            if (!tiltShift.unlinkCutt)
            {
                tiltShift.mode = updateInfo.mode;
                tiltShift.quality = updateInfo.quality;
                tiltShift.blurArea = updateInfo.blurArea;
                tiltShift.maxBlurSize = updateInfo.maxBlurSize;
                tiltShift.downsample = updateInfo.downsample;
                tiltShift.offset = updateInfo.offset;
                tiltShift.roll = updateInfo.roll;
                Vector4 blurParam = default(Vector4);
                blurParam.x = updateInfo.blurDir.x;
                blurParam.y = updateInfo.blurDir.y;
                blurParam.z = updateInfo.blurAreaDir.x;
                blurParam.w = updateInfo.blurAreaDir.y;
                tiltShift.blurParam = blurParam;
            }
        }
    }

    private void SetPostEffect(PostEffectLive3D eff, ref PostEffectUpdateInfo updateInfo)
    {
        eff.SetForcalSize(updateInfo.forcalSize);
        eff.SetBloomDofWeight(updateInfo.bloomDofWeight);
        eff.SetThreshold(updateInfo.threshold);
        eff.SetIntensity(updateInfo.intensity);
        eff.SetDofBlurType(updateInfo.dofBlurType);
        eff.SetDofQuality(updateInfo.dofQuality);
        eff.SetForegroundSize(updateInfo.dofForegroundSize);
        eff.SetDofMaxBlurSpread(updateInfo.blurSpread);
        if (updateInfo.isUseFocalPoint)
        {
            eff.setFocalPoint(updateInfo.dofFocalPoint);
        }
        else
        {
            eff.setFocalPosition(updateInfo.forcalPosition);
        }
        if (updateInfo.dofMVFilterType != 0)
        {
            UVMovieController uvMovieController = _stageController.GetMoviceController(updateInfo.filterResId);
            if (uvMovieController != null)
            {
                eff.SetDOFMVFilter(updateInfo.dofMVFilterType, updateInfo.filterIntensity, uvMovieController.mainTexture, uvMovieController.mainTextureScale, uvMovieController.mainTextureOffset);
                eff.SetDisableDOFBlur(updateInfo.disableDOFBlur);
                _stageController.UpdateMoviceController(uvMovieController, 0, updateInfo.filterTime, bReverse: false);
            }
            else
            {
                eff.SetDOFMVFilter(PostEffectLive3D.eDofMVFilterType.NONE, 0f, null, Vector2.one, Vector2.zero);
                eff.SetDisableDOFBlur(disable: false);
            }
        }
        else
        {
            eff.SetDOFMVFilter(PostEffectLive3D.eDofMVFilterType.NONE, 0f, null, Vector2.one, Vector2.zero);
            eff.SetDisableDOFBlur(disable: false);
        }
    }

    private void onUpdatePostEffect(ref PostEffectUpdateInfo updateInfo)
    {
        int activeCameraIndex = _activeCameraIndex;
        if (activeCameraIndex < _imageEffectLive3dList.Count)
        {
            PostEffectLive3D eff = _imageEffectLive3dList[activeCameraIndex];
            SetPostEffect(eff, ref updateInfo);
            if (_crossFadeCamera != null)
            {
                SetPostEffect(_crossFadeCamera.postEffectLive3D, ref updateInfo);
            }
        }
    }

    private void UpdateProps(ref PropsUpdateInfo updateInfo)
    {
        if (!sPropsTimelineDataDictionary.TryGetValue(updateInfo.data.nameHash, out var value))
        {
            return;
        }
        if (value == 0)
        {
            value = ((updateInfo.settingFlags != 0) ? updateInfo.settingFlags : 32767);
        }
        float a = updateInfo.color.a;
        Vector4 vecColor = updateInfo.color * updateInfo.colorPower;
        vecColor.w = a;
        for (int i = 0; i < MemberUnitNumber; i++)
        {
            if ((value & (1 << i)) != 0)
            {
                _propsManager.ChangeRenderState(i, vecColor, ref updateInfo);
                _propsManager.UpdateMotion(i, ref updateInfo);
            }
        }
    }

    private void UpdatePropsAttach(ref PropsAttachUpdateInfo updateInfo)
    {
        if (!sPropsTimelineDataDictionary.TryGetValue(updateInfo.data.nameHash, out var value))
        {
            return;
        }
        if (value == 0)
        {
            value = ((updateInfo.settingFlags != 0) ? updateInfo.settingFlags : 32767);
        }
        for (int i = 0; i < MemberUnitNumber; i++)
        {
            if ((value & (1 << i)) != 0)
            {
                _propsManager.ChangeAttach(i, ref updateInfo);
            }
        }
    }

    private void UpdateParticle(ref ParticleUpdateInfo updateInfo)
    {
        if (_stageController == null)
        {
            return;
        }
        List<ParticleController> particleControllerList = _stageController.particleControllerList;
        if (particleControllerList == null)
        {
            return;
        }
        for (int i = 0; i < particleControllerList.Count; i++)
        {
            if (!(particleControllerList[i] == null))
            {
                particleControllerList[i].UpdateFromTimeline(ref updateInfo);
            }
        }
    }

    private void UpdateParticleGroup(ref ParticleGroupUpdateInfo updateInfo)
    {
        if (_stageController == null)
        {
            return;
        }
        List<ParticleController> particleControllerList = _stageController.particleControllerList;
        if (particleControllerList == null)
        {
            return;
        }
        for (int i = 0; i < particleControllerList.Count; i++)
        {
            if (!(particleControllerList[i] == null))
            {
                particleControllerList[i].UpdateGroupFromTimeline(ref updateInfo);
            }
        }
    }

    private void UpdateEffect(ref EffectUpdateInfo updateInfo)
    {
        if (!(_stageController == null))
        {
            List<EffectController> effectControllerList = _stageController.effectControllerList;
            for (int i = 0; i < effectControllerList.Count; i++)
            {
                effectControllerList[i].UpdateInfo(ref updateInfo);
            }
        }
    }

    private void UpdateFormationOffset(ref FormationOffsetUpdateInfo updateInfo)
    {
        if (!(_stageController == null))
        {
            List<EffectController> effectControllerList = _stageController.effectControllerList;
            eEffectOwner characterPosition = (eEffectOwner)updateInfo.characterPosition;
            characterPosition = ((characterPosition < eEffectOwner.World) ? characterPosition : (characterPosition + 1));
            bool isVisible = updateInfo.isVisible;
            bool isEffectClear = updateInfo.isEffectClear;
            for (int i = 0; i < effectControllerList.Count; i++)
            {
                effectControllerList[i].SetVisibleAndClear(characterPosition, isVisible, isEffectClear);
            }
        }
    }

    private void UpdateSweatLocator(ref SweatLocatorUpdateInfo updateInfo)
    {
        int owner = (int)updateInfo.owner;
        if (owner >= 0 && _characterObjects.Count > owner)
        {
            CharacterObject characterObject = _characterObjects[owner];
            if (!(null == characterObject))
            {
                characterObject.UpdateSweatLocator(ref updateInfo);
            }
        }
    }

    private void UpdatePostFilm(PostEffectLive3D.ScreenOverlay screenOverlay, ref PostFilmUpdateInfo updateInfo)
    {
        screenOverlay.postFilmMode = updateInfo.filmMode;
        screenOverlay.postFilmPower = updateInfo.filmPower;
        screenOverlay.postFilmOffsetParam = updateInfo.filmOffsetParam;
        screenOverlay.postFilmOptionParam = updateInfo.filmOptionParam;
        screenOverlay.postFilmColor0 = updateInfo.color0;
        screenOverlay.postFilmColor1 = updateInfo.color1;
        screenOverlay.postFilmColor2 = updateInfo.color2;
        screenOverlay.postFilmColor3 = updateInfo.color3;
        screenOverlay.inverseVignette = updateInfo.inverseVignette;
        screenOverlay.screenCircle = updateInfo.screenCircle;
        screenOverlay.screenCircleDir = updateInfo.screenCircleDir;
        screenOverlay.layerMode = updateInfo.layerMode;
        screenOverlay.movieResId = updateInfo.movieResId;
        screenOverlay.colorBlend = updateInfo.colorBlend;
        screenOverlay.colorBlendFactor = updateInfo.colorBlendFactor;
        UVMovieController uvMovieController = _stageController.GetMoviceController(screenOverlay.movieResId);
        if (uvMovieController != null)
        {
            screenOverlay.SetMovieInfo(uvMovieController.mainTexture, uvMovieController.existMaskTex ? uvMovieController.maskTexture : null, uvMovieController.mainTextureScale, uvMovieController.mainTextureOffset);
            _stageController.UpdateMoviceController(uvMovieController, updateInfo.movieFrameOffset, updateInfo.movieTime, updateInfo.movieReverse);
        }
        else
        {
            screenOverlay.SetMovieInfo(null, null, Vector2.zero, Vector2.zero);
        }
    }

    private void UpdateShaderControl(ref ShaderControlUpdateInfo updateInfo)
    {
        LiveTimelineKeyShaderControlData.eBehaviorFlag behaviorFlags = (LiveTimelineKeyShaderControlData.eBehaviorFlag)updateInfo.behaviorFlags;
        int num = 8;
        for (int i = 0; i <= num; i++)
        {
            CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID((LiveCharaPosition)i);
            if (characterObjectFromPositionID == null)
            {
                continue;
            }
            int num2 = 0;
            switch (updateInfo.condition)
            {
                case LiveTimelineKeyShaderControlData.eCondition.CharaId:
                    num2 = characterObjectFromPositionID.data.charaId;
                    break;
                case LiveTimelineKeyShaderControlData.eCondition.DressId:
                    num2 = characterObjectFromPositionID.createInfo.activeDressId;
                    break;
            }
            if (num2 == 0 || num2 == updateInfo.conditionParam)
            {
                int num3 = 1 << i;
                if ((updateInfo.targetFlags & num3) != 0 && characterObjectFromPositionID.renderController != null)
                {
                    Character3DBase.MaterialPack.UpdateInfo materialUpdateInfo = characterObjectFromPositionID.renderController.materialUpdateInfo;
                    materialUpdateInfo.useVtxClrB = updateInfo.useVtxClrB;
                    materialUpdateInfo.lerpDiffuse = updateInfo.lerpDiffuse;
                    materialUpdateInfo.lerpGradation = updateInfo.lerpGradation;
                    bool sw = (behaviorFlags & LiveTimelineKeyShaderControlData.eBehaviorFlag.Luminous) != 0;
                    characterObjectFromPositionID.renderController.SetShaderKeyword(Character3DBase.MaterialPack.eShaderKeyword.ENABLE_LUMINOUS, sw);
                }
            }
        }
        if (_stageController != null && _stageController.gimmickController != null && ((uint)updateInfo.targetFlags & 0x8000u) != 0)
        {
            _stageController.gimmickController.SetShaderBehavior(updateInfo.data.nameHash, behaviorFlags);
        }
    }

    private void UpdateCySpring(ref CySpringUpdateInfo info)
    {
        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID(info.position);
        if (!(characterObjectFromPositionID != null))
        {
            return;
        }
        if (info.resetCySpring)
        {
            characterObjectFromPositionID.isResetClothAll = true;
            characterObjectFromPositionID.isResetClothWarmingUp = true;
            if (characterObjectFromPositionID.spareCharacters != null)
            {
                CharacterObject[] spareCharacters = characterObjectFromPositionID.spareCharacters;
                foreach (CharacterObject obj in spareCharacters)
                {
                    obj.isResetClothAll = true;
                    obj.isResetClothWarmingUp = true;
                }
            }
        }
        if (!info.overrideParameter)
        {
            return;
        }
        characterObjectFromPositionID.OverrideCySpringClothParameter((CharacterObject.OverrideClothParameterType)info.overrideType, info.overrideParameterAllBone, info.overrideParameterAcc, info.overrideParameterAccSitffness, info.overrideParameterFurisode, info.overrideParameterFurisodeStiffness);
        if (characterObjectFromPositionID.spareCharacters != null)
        {
            CharacterObject[] spareCharacters = characterObjectFromPositionID.spareCharacters;
            for (int i = 0; i < spareCharacters.Length; i++)
            {
                spareCharacters[i].OverrideCySpringClothParameter((CharacterObject.OverrideClothParameterType)info.overrideType, info.overrideParameterAllBone, info.overrideParameterAcc, info.overrideParameterAccSitffness, info.overrideParameterFurisode, info.overrideParameterFurisodeStiffness);
            }
        }
    }

    private void UpdateIKParts(CharacterObject character, StageTwoBoneIK parts, CharacterObject.IKIndex index, ref CharaIKUpdateInfo.PartUpdateInfo partsInfo)
    {
        if (parts == null) return;

        parts.BlendValue = partsInfo.blendRate;
        switch (partsInfo.targetPositionType)
        {
            case StageTwoBoneIK.TargetType.Position:
                parts.SetTargetPosition(partsInfo.targetPosition);
                break;
            case StageTwoBoneIK.TargetType.Transform:
                {
                    Transform iKTargetNode = character.GetIKTargetNode(index);
                    parts.SetTargetTransform(iKTargetNode);
                    break;
                }
        }
    }

    private void UpdateMultiCamera(ref MultiCameraUpdateInfo info)
    {
        int activeCameraIndex = _activeCameraIndex;
        Camera camera = _cameraObjects[activeCameraIndex];
        if (info.isEnable)
        {
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.Nothing;
        }
        else
        {
            camera.clearFlags = CameraClearFlags.Color;
        }
    }

    private void UpdateCrossFadeCamera(ref CrossFadeCameraUpdateInfo info)
    {
        if (!(_crossFadeCamera == null))
        {
            bool enable = info.isEnable;
            RenderTexture rtColor = _crossFadeCamera.ColorTexture;
            if (info.alpha <= 0f || !info.isEnable)
            {
                enable = false;
                rtColor = null;
            }
            _crossFadeCamera.SetEnable(enable);
            int count = _imageEffectLive3dList.Count;
            for (int i = 0; i < count; i++)
            {
                _imageEffectLive3dList[i].SetCrossFadeBlendTexture(rtColor, info.alpha);
            }
        }
    }

    private void UpdateIK(ref CharaIKUpdateInfo info)
    {
        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID(info.position);
        if (characterObjectFromPositionID != null)
        {
            StageTwoBoneIK iK = characterObjectFromPositionID.GetIK(CharacterObject.IKIndex.Leg_L);
            StageTwoBoneIK iK2 = characterObjectFromPositionID.GetIK(CharacterObject.IKIndex.Leg_R);
            StageTwoBoneIK iK3 = characterObjectFromPositionID.GetIK(CharacterObject.IKIndex.Wrist_L);
            StageTwoBoneIK iK4 = characterObjectFromPositionID.GetIK(CharacterObject.IKIndex.Wrist_R);
            UpdateIKParts(characterObjectFromPositionID, iK, CharacterObject.IKIndex.Leg_L, ref info.leg_Left);
            UpdateIKParts(characterObjectFromPositionID, iK2, CharacterObject.IKIndex.Leg_R, ref info.leg_Right);
            UpdateIKParts(characterObjectFromPositionID, iK3, CharacterObject.IKIndex.Wrist_L, ref info.arm_Left);
            UpdateIKParts(characterObjectFromPositionID, iK4, CharacterObject.IKIndex.Wrist_R, ref info.arm_Right);
        }
    }

    private void UpdateCharaFootLight(ref CharaFootLightUpdateInfo info)
    {
        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID(info.position);
        List<Props> propsList = _propsManager.GetPropsList((int)info.position);
        if (characterObjectFromPositionID != null)
        {
            characterObjectFromPositionID.SetFootLightParameter(ref info, propsList);
        }
    }

    private void UpdateStageGazeControl(ref StageGazeControlUpdateInfo info)
    {
        _stageController.UpdateStageGazeController(info.groupNo, info.isEnable, info.targetPosition);
    }

    private void UpdateCharaWind(ref CharaWindUpdateInfo info)
    {
        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID(info.selfPosition);
        if (!(characterObjectFromPositionID == null))
        {
            if (info.enable)
            {
                characterObjectFromPositionID.SetCySpringForceScale(info.cySpringForceScale);
                characterObjectFromPositionID.SetWind((ClothController.WindCalcMode)info.windMode, info.windPower, info.loopTime);
            }
            else
            {
                characterObjectFromPositionID.SetCySpringForceScale(Vector3.one);
                characterObjectFromPositionID.SetWind(ClothController.WindCalcMode.None, Vector3.zero, 0f);
            }
        }
    }

    private void UpdateCharaHeightMotion(ref CharaHeightMotionUpdateInfo info)
    {
        CharacterObject characterObjectFromPositionID = getCharacterObjectFromPositionID(info.selfPosition);
        if (!(characterObjectFromPositionID != null))
        {
            return;
        }
        switch (info.blendType)
        {
            case LiveTimelineKeyCharaHeightMotionData.BlendType.Self:
                characterObjectFromPositionID.SetAttachIKParameterType(CharacterObject.IKFixType.None);
                break;
            case LiveTimelineKeyCharaHeightMotionData.BlendType.Target:
                characterObjectFromPositionID.SetAttachIKParameterType(CharacterObject.IKFixType.Position);
                characterObjectFromPositionID.SetAttachIKParameter(0, (CharacterObject.IKIndex)info.offsetPositionEffector, info.offsetRate);
                break;
            case LiveTimelineKeyCharaHeightMotionData.BlendType.SelfFix:
                characterObjectFromPositionID.SetAttachIKParameterType(CharacterObject.IKFixType.Root);
                characterObjectFromPositionID.SetAttachIKParameter(0, (CharacterObject.IKIndex)info.offsetPositionEffector, info.offsetRate);
                break;
            case LiveTimelineKeyCharaHeightMotionData.BlendType.TargetParts:
                {
                    characterObjectFromPositionID.SetAttachIKParameterType(CharacterObject.IKFixType.IKPositionSelfHeight);
                    for (int i = 0; i < info.extendNum; i++)
                    {
                        characterObjectFromPositionID.SetAttachIKParameter(i, (CharacterObject.IKIndex)info.offsetPositionEffectorExtend[i], info.offsetRateExtend[i]);
                    }
                    break;
                }
        }
    }

    private void UpdateDressChange(ref DressChangeUpdateInfo info)
    {
        int targetFlags = (int)info.targetFlags;
        for (LiveTimelineKeyDressChangeData.eTarget eTarget = LiveTimelineKeyDressChangeData.eTarget.Center; eTarget < LiveTimelineKeyDressChangeData.eTarget.MAX; eTarget++)
        {
            int num = (int)eTarget;
            int num2 = targetFlags & (1 << num);
            if (0 < num2)
            {
                LiveTimelineKeyDressChangeData.eDressType dressType = info.dressType[num];
                SwapSpareCharacter(num, (Character3DBase.CharacterData.eDressType)dressType);
            }
        }
    }

    public void IntiializeConditionForMotionChange(LiveTimelineControl timelineControl)
    {
        if (timelineControl != null)
        {
            _cuttMotionChange = new CuttMotionChange(timelineControl.gameObject.GetComponent<CuttConditionOption>(), _live3DSettings._charaMotion, _characterData, timelineControl.data.GetWorkSheetList(), timelineControl.data.characterSettings.motionSequenceIndices, timelineControl.data.characterSettings.motionOverwriteIndices);
        }
    }

    public string[] GetAssetBundleDLListForConditionMotionChange()
    {
        if (_cuttMotionChange != null)
        {
            return _cuttMotionChange.GetAssetBundleDLList();
        }
        return new string[0];
    }

    public void OverrideMotionClipForConditionMotionChange()
    {
        if (_cuttMotionChange != null)
        {
            _cuttMotionChange.OverrideMotionClip();
        }
    }

    public bool CanBeRestoredMotionForConditionMotionChange()
    {
        return true;
    }

    public void ResumeForConditionMotionChange()
    {
    }

    public void OverrideMotionClipExecForConditionMotionChange()
    {
        if (_cuttMotionChange != null)
        {
            _cuttMotionChange.OverrideMotionClipExec();
        }
    }

    public bool IsExtendMotionLoadForConditionMotionChange()
    {
        if (_cuttMotionChange != null)
        {
            return _cuttMotionChange.IsExtendMotionLoad;
        }
        return false;
    }

    public bool IsSwapMotionNameForConditionMotionChange(string name)
    {
        if (_cuttMotionChange != null)
        {
            return _cuttMotionChange.IsSwapMotionName(name);
        }
        return false;
    }

    /// <summary>
    /// ライブ中に画面のサイズを変更する
    /// </summary>
    public void ResizeScreen(int width, int height)
    {
        if (_renderTarget != null)
        {
            _renderTarget.ResizeScreen(width, height);

            foreach (PostEffectLive3D imageEffectLive3d in _imageEffectLive3dList)
            {
                if (imageEffectLive3d != null)
                {
                    imageEffectLive3d.SetCameraRenderTarget(_renderTarget.color, _renderTarget.depth, _renderTarget.result, _renderTarget.monitor);
                }
            }
        }
    }

    public void IntiializeConditionForDressChange(LiveTimelineControl timelineControl)
    {
        if (timelineControl != null)
        {
            _cuttDressChange = new CuttDressChange(timelineControl.gameObject.GetComponent<CuttConditionOption>());
        }
    }
}
