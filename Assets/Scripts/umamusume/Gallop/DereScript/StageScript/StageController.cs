using Cutt;
using Live3D;
using ShaderParam;
using Stage;
using Stage.Cyalume;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(ShaderParamController))]
public class StageController : MonoBehaviour
{
    public enum LightLabel : byte
    {
        Wash,
        MirrorScan,
        Foot,
        Neon
    }

    private enum ResouceImageLabel : byte
    {
        Capture,
        MainMonitor,
        SubMonitor
    }

    public enum eParticleType
    {
        Element,
        Confetti,
        Effect,
        Rich_Effect
    }

    public enum eParticleAttribute
    {
        Cute = 1,
        Cool,
        Passion,
        All
    }

    private struct StageObjectInfo
    {
        public int _prefabHash;

        public Animation[] _Animations;

        public Renderer[] _Renderers;

        public StageObject[] _StageObjects;

        public GameObject[] _RendererGameObjects;
    }

    private struct ScoreRankEffect
    {
        public float _colorRate;

        public float _powerRate;

        public bool _enableMovie;

        public ScoreRankEffect(float color, float power, bool movie)
        {
            _colorRate = color;
            _powerRate = power;
            _enableMovie = movie;
        }
    }

    private class StageObjectWorkInfo
    {
        public Transform cacheTransform;

        public Renderer renderer;

        public AnimationObjectController animationController;

        public LiveTimelineTransformData.TransformBaseData startData;

        public GameObject cacheGameObject;
    }

    private class BillboardManager
    {
        private bool[][] _isBillboardController;

        private BillboardController[][] _billboardController;

        public void Initialize(MaterialController[] washLightContoller)
        {
            int num = washLightContoller.Length;
            _billboardController = new BillboardController[num][];
            _isBillboardController = new bool[num][];
            for (int i = 0; i < num; i++)
            {
                int num2 = washLightContoller[i]._Renderer.Length;
                _billboardController[i] = new BillboardController[num2];
                _isBillboardController[i] = new bool[num2];
                for (int j = 0; j < washLightContoller[i]._Renderer.Length; j++)
                {
                    _billboardController[i][j] = washLightContoller[i]._Renderer[j].gameObject.GetComponent<BillboardController>();
                    _isBillboardController[i][j] = _billboardController[i][j] != null;
                }
            }
        }

        public void Get(int index, out BillboardController[] resultController, out bool[] resultEnable)
        {
            if (_billboardController.Length <= index)
            {
                resultController = null;
                resultEnable = null;
            }
            else
            {
                resultController = _billboardController[index];
                resultEnable = _isBillboardController[index];
            }
        }
    }

    [Serializable]
    public class MipmapBias
    {
        private const int INVALID = -127;

        private const float STAGE_DEFAULT_BIAS = 0f;

        private int _idxStgParam = INVALID;

        public float _stage;

        public void Init()
        {
        }

        private void Update(int idx, float val, float defVal)
        {
        }

        public void Update()
        {
            Update(_idxStgParam, _stage, STAGE_DEFAULT_BIAS);
        }
    }

    private const int CHARA_COUNT = 15;

    private const int PARTICLE_DEFAULT_SIZE = 4;

    public const string STAGE_COMMON_CYALUME_PREFAB_NAME = "pf_stg_cyalume_controller";

    public const string STAGE_COMMON_ROUND_CYALUME_PREFAB_NAME = "pf_stg_cyalume_controller_round";

    public const string HQ_STAGE_COMMON_PREFAB_PATH = "3D/Stage/stg_common_hq/Prefab/pf_stg_cyalume_controller_hq";

    public const string HQ_STAGE_COMMON_ROUND_PREFAB_PATH = "3D/Stage/stg_common_hq/Prefab/pf_stg_cyalume_controller_round_hq";

    public const string MOB_STAGE_COMMON_PREFAB_PATH = "3D/Stage/stg_mob_common/Prefab/pf_stg_mob_shadow";

    public const string MOB_STAGE_COMMON_ROUND_PREFAB_PATH = "3D/Stage/stg_mob_common/Prefab/pf_stg_mob_shadow_round";

    public const string GLOBAL_ENV_COMMON_TEX_PATH = "3D/Stage/stg_common_hq/Textures/tx_stg_env_common_hq";

    private static readonly LabelToIDDictionary sLabelToIDMovieColor = new LabelToIDDictionary("MovieColor");

    private static readonly LabelToIDDictionary sLabelToIDBgWash = new LabelToIDDictionary("BgWash");

    private static readonly LabelToIDDictionary sLabelToIDBgMirrorScan = new LabelToIDDictionary("BgMirrorScan");

    private static readonly LabelToIDDictionary sLabelToIDBgFoot = new LabelToIDDictionary("BgFoot");

    private static readonly LabelToIDDictionary sLabelToIDBgNeon = new LabelToIDDictionary("BgNeon");

    private static readonly LabelToIDDictionary sLabelToIDBgMonitor = new LabelToIDDictionary("Monitor");

    private static readonly LabelToIDDictionary sLabelToIDLaser = new LabelToIDDictionary("Laser");

    private static readonly LabelToIDDictionary sLabelToIDBgAnim = new LabelToIDDictionary("BgAnim");

    private static readonly LabelToIDDictionary sLabelToIDAnimeObj = new LabelToIDDictionary("AnimeObj");

    private static readonly int HASH_FLLOW_SPOT_CENTER = FNVHash.Generate("FollowSpotCenter");

    private static readonly int HASH_FLLOW_SPOT_OHTER = FNVHash.Generate("FollowSpotOther");

    private static readonly int HASH_FLLOW_SPOT_COLOR = FNVHash.Generate("FollowSpotColor");

    private static readonly int HASH_CHARA_CENTER = FNVHash.Generate("CharaCenter");

    private static readonly int HASH_CHARA_OTHER = FNVHash.Generate("CharaOther");

    private static readonly int HASH_CHARA_COLOR = FNVHash.Generate("CharaColor");

    private static readonly int HASH_CHARA_RIMCOLOR = FNVHash.Generate("CharaRimColor");

    private static readonly int HASH_CHARA_SHADOW = FNVHash.Generate("Shadow");

    private static readonly int HASH_BG_BL = FNVHash.Generate("BgBL");

    private static readonly string[] MIRRORSCAN_LIGHT_DUMMYMATERIAL_NAMES = new string[8]
    {
        "mt_stg_MirrorScanLight_A",
        "mt_stg_MirrorScanLight_B",
        "mt_stg_MirrorScanLight_C",
        "mt_stg_MirrorScanLight_D",
        "mt_stg_MirrorScanLight_E",
        "mt_stg_MirrorScanLight_F",
        "mt_stg_MirrorScanLight_G",
        "mt_stg_MirrorScanLight_H"
    };

    private static readonly ScoreRankEffect[] _ScoreRankEffect = new ScoreRankEffect[5]
    {
        new ScoreRankEffect(0.1f, 0.1f, movie: false),
        new ScoreRankEffect(1f, 1f, movie: true),
        new ScoreRankEffect(1f, 1f, movie: true),
        new ScoreRankEffect(1f, 1f, movie: true),
        new ScoreRankEffect(1f, 1f, movie: true)
    };

    [SerializeField]
    [Tooltip("背景に設定するオブジェクトを設定して下さい。")]
    private GameObject[] _stageObjects;

    private StageObjectInfo[] _stageObjectsInfo;

    private Dictionary<int, int> _stageObjectsHashToID;

    private List<Renderer> _startDisableRenderers;

    [SerializeField]
    [Tooltip("GamePlay時無効にするノード,Prefab名")]
    private string[] _invalidGamePlayStageObject;

    [SerializeField]
    private Vector3 _cyalumeOffsetPosition = Vector3.zero;

    [Tooltip("背景後方で演出をするためのライトのマテリアルを設定してください。\n設定したマテリアルの順にタイムラインのBgWashA～BgWashZで制御することができます。")]
    public Material[] _washLightMaterials;

    private MaterialController[] _washLightMaterialCtrls;

    public const int WashLightMaterialGroupNum = 26;

    [Tooltip("背景後方で演出をするためのライトグループのマテリアルを設定してください")]
    public Material[] _washLightMaterialGroup;

    public Color[][] _washLightMaterialGroupColor1;

    public Color[][] _washLightMaterialGroupColor2;

    public float[][] _washLightMaterialGroupPower;

    private MaterialController[] _washLightMaterialGroupCtrl;

    [Tooltip("Laser用Materials")]
    public Material[] _laserMaterials;

    public const int LaserMaterialGroupNum = 26;

    [Tooltip("Laser用Materialsグループ")]
    public Material[] _laserMaterialGroup;

    public Color[][] _laserMaterialGroupColor1;

    public Color[][] _laserMaterialGroupColor2;

    public float[][] _laserMaterialGroupPower;

    public float[][] _laserMaterialGroupRandomValue;

    [Tooltip("地面や背景に投影するライトで使用するマテリアルを設定して下さい。\n設定したマテリアルはDirectorタイムラインののMirrorScanA～MirrorScanZで制御することができます。")]
    public Material[] _mirorrScanLightMaterials;

    private bool _isInitializeMirrorScanLightMaterial;

    [Tooltip("足元で点滅するためのライトのマテリアルを設定してください。\n設定したマテリアルの順にタイムラインのBgFootA～BgFootZで制御することができます。")]
    public Material[] _footLightMaterials;

    public const int FootLightMaterialGroupNum = 26;

    [Tooltip("足元で点滅するためのライトグループのマテリアルを設定してください")]
    public Material[] _footLightMaterialGroup;

    public Color[][] _footLightMaterialGroupColor1;

    public Color[][] _footLightMaterialGroupColor2;

    public float[][] _footLightMaterialGroupPower;

    [SerializeField]
    [Tooltip("ネオンライト用のパラメータを設定します。")]
    public NeonMaterialController.MoveMaterialInfo[] _neonMaterialInfos;

    private NeonMaterialController[] _neonMaterialCtrls;

    [Tooltip("ムービー演出を行うマテリアルを設定してください。\n設定したマテリアルの順にタイムラインのBgMovieA～BgMovieZで制御することができます。")]
    public MovieMaterialController.MoveMaterialInfo[] _movieMaterialInfos;

    private MovieMaterialController[] _movieMaterialCtrls;

    [Tooltip("アニメーション演出を行うマテリアルを設定してください。\n設定したマテリアルの順にタイムラインのBgAnimA～BgAnimZで制御することができます。")]
    public Material[] _animationMaterials;

    [Tooltip("背景カラーパレットを使うマテリアルを設定してください。")]
    public Material[] _paletteObjectMaterials;

    private Dictionary<int, Material> _instanceMaterialDictionary = new Dictionary<int, Material>();

    [Tooltip("UVムービーの名前を指定して下さい。")]
    public string[] _UVMovieResources;

    [Tooltip("イメージのリソースを指定して下さい。")]
    public Texture[] _ImageResources;

    [SerializeField]
    private Material[] _LightModeHideMaterials;

    [SerializeField]
    private GameObject[] _animationObjects;

    private AnimationObjectController[] _animationObjectController;

    [SerializeField]
    private string _borderLightObjectName = "BorderLightObject";

    private Transform _borderLightTransform;

    [SerializeField]
    private string _sunObjectName = "SunShafts";

    [Tooltip("環境マップのテクスチャを指定したい場合はここに設定してください")]
    public Texture _globalEnvironmentTexture;

    private static readonly string[] _LightShaderName = new string[3]
    {
        "StageAddLight1",
        "StageAddLightV",
        "StageAddLightF"
    };
    private static Shader[] _LightShader = null;

    private Dictionary<int, List<StageObjectWorkInfo>> _objectsWorkInfoDictionary = new Dictionary<int, List<StageObjectWorkInfo>>();

    private Dictionary<int, List<LaserController>> _laserControllerDictionary = new Dictionary<int, List<LaserController>>();

    private List<ParticleController> _particleControllerList = new List<ParticleController>(4);

    private List<EffectController> _effectControllerList = new List<EffectController>();

    private Dictionary<int, StageObjectRich> _mapStageObjectRich = new Dictionary<int, StageObjectRich>();

    private GimmickController _gimmickController = new GimmickController();

    private int[] _charaIdArray;

    private List<ProjectorController> _projectorControllerList = new List<ProjectorController>(10);

    private CyalumeController3D[] _cyalumeController3D;

    // 0x94
    //private AnalyzerController[] _analyzerController;

    private MobShadowController _mobController;

    private UnityLensFlareController[] _lensFlareController;

    private UnityLensFlareController[][][] _lensFlareControllerGroupTable;

    private UnityLensFlareController[][] _lensFlareControllerGroup;

    private GlassController[] _glassController;

    private MobController _mobCyalume3DController;

    private StageMob3DImitationController[] _stageMob3dImitationControllerArray;

    // 0xA4
    //private TempData.LiveTempData _liveTempData;

    private Director _Director;

    private LiveTimelineControl _liveTimelineControl;

    private ShaderParamVector4 _AmbientColorParam;

    private ShaderParamVector4 _LocalTimerParam;

    private UVMovieManager _MovieManager;

    private SharedShaderParam _sharedShaderParam;

    //private GameDefine.eScoreRank _nowScoreRank;

    //private ScoreRankEffect _nowScoreRankEffect = _ScoreRankEffect[0];

    //private LocalData.OptionData.eQualityType _qualityType = LocalData.OptionData.eQualityType.Quality3D;

    private Transform[] _charaLocatorTransform = new Transform[15];

    private int _shaderLevel;

    private bool _isLastPause;

    private bool _isEnabledConfetti;

    private bool _isVertical;

    private DynamicBatchHelper[] _dynamicBatchHelpers;

    private Material[] _dynamicBatchHelperMaterials;

    private bool _isMirrorScanOn = true;

    private bool _isCheckMirrorScan;

    private Matrix4x4 _invisibleMobCyalumeMatrix = Matrix4x4.Translate(new Vector3(0f, 2000f, 0f));

    private BillboardManager _billboardManager;

    private Dictionary<int, List<StageGazeController>> _stageGazeControllerTable;

    private StageGazeController[] _stageGazeController;

    private bool _isStageGazeController;

    [SerializeField]
    private MipmapBias _mipmapBias = new MipmapBias();

    private Dictionary<string, string> _renderReplaceMirrorDic = new Dictionary<string, string>
    {
        { "StageAddLight1", "AddLight1" },
        { "StageAddLight1_group", "AddLight1_group" }
    };

    private Regex _regexCheckRendererMirrorName;

    private const string SHADER_MIRROR_TAG = "Mirror";

    private Camera _mainCamera;

    private PostEffectLive3D _VolumeLightSunShaftsComponent;

    private float _sunShaftsPower;

    private float _SunShaftsIntensity = 0.2f;

    private float _SunShaftsFadeStart = 20f;

    private float _SunShaftsFadeMix = 18f;

    private MirrorReflection[] _mirrorPlaneObjects;

    private bool _environmentMirror = true;

    public List<ParticleController> particleControllerList => _particleControllerList;

    public List<EffectController> effectControllerList => _effectControllerList;

    public Dictionary<int, StageObjectRich> mapStageObjectRich => _mapStageObjectRich;

    public GimmickController gimmickController => _gimmickController;

    public int[] charaIdArray
    {
        set
        {
            _charaIdArray = value;
        }
    }

    private bool hasMainCamera => null != _mainCamera;

    public bool isRich
    {
        get
        {
            return true;
        }
    }

    public static eParticleType GetParticleType(string strParticleID)
    {
        return (eParticleType)int.Parse(strParticleID.Substring(0, 1));
    }

    public static eParticleAttribute GetParticleAttribute(string strParticleID)
    {
        return (eParticleAttribute)int.Parse(strParticleID.Substring(1, 1));
    }

    private void SetupLiveTimelineControl()
    {
        if (_liveTimelineControl != null)
        {
            return;
        }
        _liveTimelineControl = UnityEngine.Object.FindObjectOfType<LiveTimelineControl>();
        if (!(_liveTimelineControl != null))
        {
            return;
        }
        _liveTimelineControl.playMode = TimelinePlayerMode.Default;

        _liveTimelineControl.OnUpdateCameraPos += UpdateCameraPos;
        _liveTimelineControl.OnStartUpdate += StartUpdateTimeline;
        _liveTimelineControl.OnPreUpdateAllTimeline += PreUpdateAllTimeline;
        _liveTimelineControl.OnUpdateBgColor1 += UpdateBgColor1;
        _liveTimelineControl.OnUpdateBgColor2 += UpdateBgColor2;
        _liveTimelineControl.OnUpdateBgColor3 += UpdateBgColor3;
        _liveTimelineControl.OnUpdateMonitorControl += UpdateMonitorControl;
        _liveTimelineControl.OnUpdateProjector += UpdateProjector;
        _liveTimelineControl.OnUpdateAnimation += UpdateAnimation;
        _liveTimelineControl.OnUpdateTextureAnimation += UpdateTextureAnimation;
        _liveTimelineControl.OnUpdateTransform += UpdateTransform;
        _liveTimelineControl.OnUpdateRenderer += UpdateRenderer;
        _liveTimelineControl.OnUpdateHdrBloom += UpdateHdrBloom;
        _liveTimelineControl.OnUpdateObject += UpdateObject;
        _liveTimelineControl.OnUpdateGazingObject += UpdateGazingObject;
        _liveTimelineControl.OnUpdateLaser += UpdateLaser;
        _liveTimelineControl.OnUpdateGlass += UpdateGlass;
        _liveTimelineControl.OnEnvironmentMirror += onEnvironemntMirror;
        _liveTimelineControl.OnUpdateVolumeLight += UpdateVolumeLight;
        _liveTimelineControl.OnUpdateLensFlare += UpdateLensFlare;
        if ((bool)_mobCyalume3DController)
        {
            _liveTimelineControl.OnUpdateMobCyalume3D += UpdateMobCyalume3D;
        }
        else
        {
            _liveTimelineControl.OnUpdateMobCyalume += UpdateMobCyalume;
        }
        if (_Director._existsMobCyalume3DImitation)
        {
            _liveTimelineControl.OnUpdateMobCyalume3D += UpdateStageMob3DImitation;
        }
    }

    private void UpdateCameraPos(ref CameraPosUpdateInfo updateInfo)
    {
        CharacterObject characterObject = null;
        for (int i = 0; i < _Director.MemberUnitNumber; i++)
        {
            characterObject = _Director.getCharacterObjectFromPositionID((LiveCharaPosition)i);
            if (characterObject != null && characterObject.renderController != null)
            {
                int num = characterObject.gameObject.layer - 22;
                bool flag = ((1 << num) & updateInfo.characterLODMask) != 0;
                bool enableShaderLOD = ((32768 << num) & updateInfo.characterLODMask) != 0;
                characterObject.renderController.enableOutlineLOD = flag;
                characterObject.renderController.enableShaderLOD = enableShaderLOD;
                characterObject.renderController.SetOutlineZOffset(updateInfo.outlineZOffset);
                characterObject.EnableCheekLOD(flag);
            }
        }
    }

    private void StartUpdateTimeline()
    {
        for (int i = 0; i < _Director.MemberUnitNumber; i++)
        {
            _liveTimelineControl.SetCharactorParentLocator(i, _charaLocatorTransform[i]);
        }
    }

    private void PreUpdateAllTimeline()
    {
        /*
        _nowScoreRank = GetScoreRank();
        if ((bool)_liveTempData.isMV && !SingletonMonoBehaviour<LocalData>.IsInstanceEmpty() && SingletonMonoBehaviour<LocalData>.instance.option.mv_confetti)
        {
            _nowScoreRank = GameDefine.eScoreRank.Rank_S;
        }
        _nowScoreRankEffect = _ScoreRankEffect[(int)_nowScoreRank];
        */
    }

    private string MakeStageWorkObjectName(string name)
    {
        string text = name.Replace("(Clone)", "");
        int num = text.LastIndexOf("_hq", StringComparison.Ordinal);
        if (num >= 0 && num == text.Length - 3)
        {
            text = text.Remove(num);
        }
        return text;
    }

    private void ReplaceStageObject()
    {
        if (_stageObjects == null || Director.instance == null || Director.instance.isHaveHqStage)
        {
            return;
        }
        bool flag = false;
        bool flag2 = false;
        if (!_mobCyalume3DController)
        {
            for (int i = 0; i < _stageObjects.Length; i++)
            {
                if (_stageObjects[i].name == "pf_stg_cyalume_controller")
                {
                    flag = true;
                    _stageObjects[i] = ResourcesManager.instance.LoadObject("3D/Stage/stg_common_hq/Prefab/pf_stg_cyalume_controller_hq") as GameObject;
                }
                else if (_stageObjects[i].name == "pf_stg_cyalume_controller_round")
                {
                    flag2 = true;
                    _stageObjects[i] = ResourcesManager.instance.LoadObject("3D/Stage/stg_common_hq/Prefab/pf_stg_cyalume_controller_round_hq") as GameObject;
                }
            }
        }
        if (flag || flag2)
        {
            GameObject[] array = new GameObject[_stageObjects.Length + 1];
            for (int j = 0; j < _stageObjects.Length; j++)
            {
                array[j] = _stageObjects[j];
            }
            if (flag)
            {
                array[array.Length - 1] = ResourcesManager.instance.LoadObject("3D/Stage/stg_mob_common/Prefab/pf_stg_mob_shadow") as GameObject;
            }
            else if (flag2)
            {
                array[array.Length - 1] = ResourcesManager.instance.LoadObject("3D/Stage/stg_mob_common/Prefab/pf_stg_mob_shadow_round") as GameObject;
            }
            _stageObjects = array;
        }
    }

    /// <summary>
    /// ステージオブジェクトの初期化
    /// </summary>
    private void StartStageObject()
    {
        if (_stageObjects == null || _stageObjects.Length == 0)
        {
            return;
        }
        _animationObjectController = new AnimationObjectController[_animationObjects.Length];
        _stageObjectsInfo = new StageObjectInfo[_stageObjects.Length];
        _stageObjectsHashToID = new Dictionary<int, int>();
        _startDisableRenderers = new List<Renderer>();
        Director instance = Director.instance;
        bool flag = instance.SearchParticleConfettiPrefabName();
        bool flag2 = instance.SearchMirrorScanPrefabName();
        bool flag3 = false;
        for (int i = 0; i < _stageObjects.Length; i++)
        {
            if (_stageObjects[i] == null || (flag && _stageObjects[i].name.Contains("Confetti")) || (flag2 && _stageObjects[i].name.Contains("MirrorScan")) || ((bool)_mobCyalume3DController && (_stageObjects[i].name.Contains("cyalume_controller") || _stageObjects[i].name.Contains("mob_shadow"))))
            {
                continue;
            }
            if (_stageObjects[i].name.Contains("mob_shadow"))
            {
                continue;
            }
            else if (flag3)
            {
                bool flag4 = false;
                if (_invalidGamePlayStageObject != null)
                {
                    string[] invalidGamePlayStageObject = _invalidGamePlayStageObject;
                    foreach (string value in invalidGamePlayStageObject)
                    {
                        if (_stageObjects[i].name.Equals(value))
                        {
                            flag4 = true;
                            break;
                        }
                    }
                }
                if (flag4)
                {
                    continue;
                }
            }
            int num = -1;
            for (int k = 0; k < _animationObjects.Length; k++)
            {
                if (_animationObjects[k] == _stageObjects[i])
                {
                    num = k;
                    break;
                }
            }
            _stageObjects[i] = UnityEngine.Object.Instantiate(_stageObjects[i]);
            _stageObjects[i].transform.parent = base.transform;
            if (_stageObjects[i].isStatic)
            {
                StaticBatchingUtility.Combine(_stageObjects[i]);
            }
            if (num != -1)
            {
                _animationObjects[num] = _stageObjects[i];
                _animationObjectController[num] = _animationObjects[num].GetComponentInChildren<AnimationObjectController>();
            }
            _stageObjectsInfo[i] = default(StageObjectInfo);
            Animation[] componentsInChildren = _stageObjects[i].GetComponentsInChildren<Animation>();
            foreach (Animation animation in componentsInChildren)
            {
                List<AnimationClip> list = new List<AnimationClip>();
                List<AnimationClip> list2 = new List<AnimationClip>();
                bool flag5 = false;
                foreach (AnimationState item in animation)
                {
                    AnimationClip animationClip = UnityEngine.Object.Instantiate(item.clip);
                    animationClip.name = item.clip.name;
                    //FPSを拡張
                    animationClip.frameRate = Application.targetFrameRate;
                    list.Add(animationClip);
                    list2.Add(item.clip);
                }
                foreach (AnimationClip item2 in list2)
                {
                    animation.RemoveClip(item2.name);
                }
                foreach (AnimationClip item3 in list)
                {
                    animation.AddClip(item3, item3.name);
                    if (!flag5)
                    {
                        animation.Play(item3.name);
                        flag5 = true;
                    }
                }
            }
            GameObject[] children = _stageObjects[i].GetChildren(includeInactive: true);
            if (children.Length == 0)
            {
                continue;
            }
            Common.GetArrayComponentFromArrayObject<Animation>(out _stageObjectsInfo[i]._Animations, children);
            Common.GetArrayComponentFromArrayObject<Renderer>(out _stageObjectsInfo[i]._Renderers, children);
            Common.GetArrayComponentFromArrayObject<StageObject>(out _stageObjectsInfo[i]._StageObjects, children);
            if (_stageObjectsInfo[i]._Renderers != null)
            {
                _stageObjectsInfo[i]._RendererGameObjects = new GameObject[_stageObjectsInfo[i]._Renderers.Length];
                for (int k = 0; k < _stageObjectsInfo[i]._Renderers.Length; k++)
                {
                    _stageObjectsInfo[i]._RendererGameObjects[k] = _stageObjectsInfo[i]._Renderers[k].gameObject;
                    if (!_stageObjectsInfo[i]._Renderers[k].enabled)
                    {
                        _startDisableRenderers.Add(_stageObjectsInfo[i]._Renderers[k]);
                        _stageObjectsInfo[i]._Renderers[k].enabled = true;
                    }
                }
            }
            string seed = MakeStageWorkObjectName(_stageObjects[i].name);
            _stageObjectsInfo[i]._prefabHash = FNVHash.Generate(seed);
            _stageObjectsHashToID.Add(_stageObjectsInfo[i]._prefabHash, i);
        }
        if (flag3 && _invalidGamePlayStageObject != null)
        {
            Transform target = base.transform;
            string[] invalidGamePlayStageObject = _invalidGamePlayStageObject;
            for (int j = 0; j < invalidGamePlayStageObject.Length; j++)
            {
                Transform transform = GameObjectUtility.FindChild(invalidGamePlayStageObject[j], target);
                if (transform != null)
                {
                    transform.gameObject.SetActive(value: false);
                }
            }
        }
        _Director = Director.instance;
        SetParticleControllers(base.gameObject.GetComponentsInChildren<ParticleController>());
        SetupParticle();
        SetProjectorControllers(base.gameObject.GetComponentsInChildren<ProjectorController>());
        SetupMirrorScanLight();
        /*
        _analyzerController = GetComponentsInChildren<AnalyzerController>();
        for (int i = 0; i < _analyzerController.Length; i++)
        {
            _analyzerController[i].isLight = isLight;
        }
        */
        if ((bool)_mobCyalume3DController)
        {
            CyalumeController3D[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<CyalumeController3D>();
            if (componentsInChildren2 != null)
            {
                GameObject gameObject = new GameObject();
                for (int i = 0; i < componentsInChildren2.Length; i++)
                {
                    gameObject.transform.SetParent(componentsInChildren2[i].transform.parent);
                    componentsInChildren2[i].transform.SetParent(gameObject.transform);
                }
                gameObject.SetActive(value: false);
            }
            MobShadowController componentInChildren = base.gameObject.GetComponentInChildren<MobShadowController>();
            if ((bool)componentInChildren)
            {
                GameObject gameObject2 = new GameObject();
                gameObject2.transform.SetParent(componentInChildren.transform.parent);
                componentInChildren.transform.SetParent(gameObject2.transform);
                gameObject2.SetActive(value: false);
            }
            _mobCyalume3DController.Setup();
        }
        else
        {
            _mobController = base.gameObject.GetComponentInChildren<MobShadowController>();
            _cyalumeController3D = base.gameObject.GetComponentsInChildren<CyalumeController3D>();
            if (_cyalumeController3D != null)
            {
                for (int i = 0; i < _cyalumeController3D.Length; i++)
                {
                    _cyalumeController3D[i]._cyalumeOffsetPos = _cyalumeOffsetPosition;
                    _cyalumeController3D[i].SetMobController(_mobController);
                }
            }
        }
        LaserController[] componentsInChildren3 = GetComponentsInChildren<LaserController>();
        List<LaserController> value2 = null;
        for (int l = 0; l < componentsInChildren3.Length; l++)
        {
            string seed = componentsInChildren3[l].transform.name;
            seed = seed.Replace("(Clone)", "");
            int key = FNVHash.Generate(seed);
            if (!_laserControllerDictionary.TryGetValue(key, out value2))
            {
                value2 = new List<LaserController>();
                _laserControllerDictionary.Add(key, value2);
            }
            value2.Add(componentsInChildren3[l]);
        }
        Transform[] componentsInChildren4 = GetComponentsInChildren<Transform>();
        for (int i = 0; i < componentsInChildren4.Length; i++)
        {
            string seed = componentsInChildren4[i].gameObject.name;
            seed = MakeStageWorkObjectName(seed);
            int key2 = FNVHash.Generate(seed);
            StageObjectWorkInfo stageObjectWorkInfo = new StageObjectWorkInfo();
            stageObjectWorkInfo.cacheTransform = componentsInChildren4[i];
            stageObjectWorkInfo.cacheGameObject = componentsInChildren4[i].gameObject;
            stageObjectWorkInfo.renderer = stageObjectWorkInfo.cacheGameObject.GetComponent<Renderer>();
            stageObjectWorkInfo.animationController = stageObjectWorkInfo.cacheGameObject.GetComponent<AnimationObjectController>();
            stageObjectWorkInfo.startData.position = stageObjectWorkInfo.cacheTransform.localPosition;
            stageObjectWorkInfo.startData.rotation = stageObjectWorkInfo.cacheTransform.localRotation;
            stageObjectWorkInfo.startData.scale = stageObjectWorkInfo.cacheTransform.localScale;
            if (_objectsWorkInfoDictionary.TryGetValue(key2, out var value3))
            {
                value3.Add(stageObjectWorkInfo);
                continue;
            }
            value3 = new List<StageObjectWorkInfo>();
            value3.Add(stageObjectWorkInfo);
            _objectsWorkInfoDictionary.Add(key2, value3);
        }
        if (!string.IsNullOrEmpty(_borderLightObjectName))
        {
            _borderLightTransform = GameObjectUtility.FindChild(_borderLightObjectName, base.gameObject.transform);
        }
        _mirrorPlaneObjects = GetComponentsInChildren<MirrorReflection>();
        _lensFlareController = GetComponentsInChildren<UnityLensFlareController>();
        if (_lensFlareController != null)
        {
            UnityLensFlareController[] lensFlareController = _lensFlareController;
            for (int j = 0; j < lensFlareController.Length; j++)
            {
                lensFlareController[j].Initialize();
            }
            if (_washLightMaterialGroup != null)
            {
                List<UnityLensFlareController> list3 = new List<UnityLensFlareController>(8);
                int num2 = _washLightMaterialGroup.Length;
                _lensFlareControllerGroupTable = new UnityLensFlareController[num2][][];
                for (int m = 0; m < num2; m++)
                {
                    _lensFlareControllerGroupTable[m] = new UnityLensFlareController[26][];
                    for (int k = 0; k < 26; k++)
                    {
                        list3.Clear();
                        lensFlareController = _lensFlareController;
                        foreach (UnityLensFlareController unityLensFlareController in lensFlareController)
                        {
                            if (unityLensFlareController.IsGroup(_washLightMaterialGroup[m], k))
                            {
                                list3.Add(unityLensFlareController);
                            }
                        }
                        _lensFlareControllerGroupTable[m][k] = list3.ToArray();
                    }
                }
            }
            if (_washLightMaterials != null)
            {
                List<UnityLensFlareController> list4 = new List<UnityLensFlareController>(8);
                int num3 = _washLightMaterials.Length;
                _lensFlareControllerGroup = new UnityLensFlareController[num3][];
                for (int n = 0; n < num3; n++)
                {
                    list4.Clear();
                    lensFlareController = _lensFlareController;
                    foreach (UnityLensFlareController unityLensFlareController2 in lensFlareController)
                    {
                        if (unityLensFlareController2.IsGroup(_washLightMaterials[n]))
                        {
                            list4.Add(unityLensFlareController2);
                        }
                    }
                    _lensFlareControllerGroup[n] = list4.ToArray();
                }
            }
        }
        _glassController = GetComponentsInChildren<GlassController>();
        _stageMob3dImitationControllerArray = GetComponentsInChildren<StageMob3DImitationController>();
        for (int num4 = 0; num4 < _stageMob3dImitationControllerArray.Length; num4++)
        {
            _stageMob3dImitationControllerArray[num4].Setup();
        }
        _Director._existsMobCyalume3DImitation = _stageMob3dImitationControllerArray != null && _stageMob3dImitationControllerArray.Length != 0;
        StageObjectRich[] componentsInChildren5 = GetComponentsInChildren<StageObjectRich>();
        foreach (StageObjectRich stageObjectRich in componentsInChildren5)
        {
            int key3 = FNVHash.Generate(stageObjectRich.name);
            if (!_mapStageObjectRich.ContainsKey(key3))
            {
                _mapStageObjectRich.Add(key3, stageObjectRich);
            }
        }
        _dynamicBatchHelpers = GetComponentsInChildren<DynamicBatchHelper>();
        if (_dynamicBatchHelpers != null)
        {
            DynamicBatchHelper[] array = new DynamicBatchHelper[_dynamicBatchHelpers.Length];
            List<Material> list5 = new List<Material>();
            DynamicBatchHelper[] dynamicBatchHelpers = _dynamicBatchHelpers;
            foreach (DynamicBatchHelper dynamicBatchHelper in dynamicBatchHelpers)
            {
                bool flag6 = false;
                int num5 = 0;
                for (int count = list5.Count; num5 < count; num5++)
                {
                    DynamicBatchHelper dynamicBatchHelper2 = array[num5];
                    if (dynamicBatchHelper.renderQueue == dynamicBatchHelper2.renderQueue && dynamicBatchHelper.materialName.Equals(dynamicBatchHelper2.materialName))
                    {
                        flag6 = true;
                        dynamicBatchHelper.Initialize(list5[num5]);
                        break;
                    }
                }
                if (!flag6)
                {
                    Material original = dynamicBatchHelper.FindMaterial();
                    original = UnityEngine.Object.Instantiate(original);
                    original.renderQueue = dynamicBatchHelper.renderQueue;
                    dynamicBatchHelper.Initialize(original);
                    array[list5.Count] = dynamicBatchHelper;
                    list5.Add(original);
                }
            }
            _dynamicBatchHelperMaterials = list5.ToArray();
        }
        string[] array2 = new string[_renderReplaceMirrorDic.Keys.Count];
        _renderReplaceMirrorDic.Keys.CopyTo(array2, 0);
        string pattern = "/(" + string.Join("|", array2) + ")$";
        _regexCheckRendererMirrorName = new Regex(pattern, RegexOptions.IgnoreCase);
        SetMirrorTag();
    }

    public void AddObjectWorkInfo(Transform objectTransform)
    {
        if (!(objectTransform == null))
        {
            StageObjectWorkInfo stageObjectWorkInfo = new StageObjectWorkInfo();
            stageObjectWorkInfo.cacheTransform = objectTransform;
            stageObjectWorkInfo.cacheGameObject = objectTransform.gameObject;
            stageObjectWorkInfo.renderer = stageObjectWorkInfo.cacheGameObject.GetComponent<Renderer>();
            stageObjectWorkInfo.animationController = stageObjectWorkInfo.cacheGameObject.GetComponent<AnimationObjectController>();
            stageObjectWorkInfo.startData.position = stageObjectWorkInfo.cacheTransform.localPosition;
            stageObjectWorkInfo.startData.rotation = stageObjectWorkInfo.cacheTransform.localRotation;
            stageObjectWorkInfo.startData.scale = stageObjectWorkInfo.cacheTransform.localScale;
            string text = objectTransform.gameObject.name;
            text = MakeStageWorkObjectName(text);
            int key = FNVHash.Generate(text);
            if (_objectsWorkInfoDictionary.TryGetValue(key, out var value))
            {
                value.Add(stageObjectWorkInfo);
                return;
            }
            value = new List<StageObjectWorkInfo>();
            value.Add(stageObjectWorkInfo);
            _objectsWorkInfoDictionary.Add(key, value);
        }
    }

    public void AddObjectWorkInfo(Transform[] objectTransforms)
    {
        if (objectTransforms != null)
        {
            for (int i = 0; i < objectTransforms.Length; i++)
            {
                AddObjectWorkInfo(objectTransforms[i]);
            }
        }
    }

    private void StartShaderParam()
    {
        ShaderParamController component = GetComponent<ShaderParamController>();
        string propertyName = _sharedShaderParam.getPropertyName(SharedShaderParam.ShaderProperty.LocalTimer);
        _LocalTimerParam = new ShaderParamVector4(propertyName);
        _LocalTimerParam.Start();
        component.AddShaderParam(_LocalTimerParam);
        component.FindShaderParam(name: _sharedShaderParam.getPropertyName(SharedShaderParam.ShaderProperty.AmbientColor), param: out _AmbientColorParam);
        if (_mipmapBias != null)
        {
            _mipmapBias.Init();
            _mipmapBias.Update();
        }
    }

    public void ReadOtherResource()
    {
        if (_UVMovieResources != null)
        {
            GameObject gameObject = new GameObject("UVMovieManager", typeof(UVMovieManager));
            gameObject.transform.parent = base.transform;
            _MovieManager = gameObject.GetComponent<UVMovieManager>();
            _MovieManager._MovieResouces = _UVMovieResources;
            _MovieManager.updateTimeline = true;
            _MovieManager.charaIdArray = _charaIdArray;
        }
    }

    private void Start()
    {
        /*
        TempData tempData = null;
        if (!SingletonMonoBehaviour<TempData>.IsInstanceEmpty())
        {
            tempData = SingletonMonoBehaviour<TempData>.instance;
        }
        _qualityType = Director.GetQualityTypeForStage();
        */
        _sharedShaderParam = SharedShaderParam.instance;
        _isVertical = ViewLauncher.instance.liveDirector.isVertical;
        /*
        _isVertical = LiveUtils.IsVerticalLive() || LiveUtils.IsVerticalMV();
        if (tempData != null)
        {
            _liveTempData = tempData.liveTemp;
        }
        */
        if (_LightShader == null)
        {
            _LightShader = new Shader[_LightShaderName.Length];
            for (int i = 0; i < _LightShader.Length; i++)
            {
                //_LightShader[i] = Shader.Find("Cygames/3DLive/Stage/" + _LightShaderName[i]);
                _LightShader[i] = ResourcesManager.instance.GetShader(_LightShaderName[i]);
            }
        }
        ReplaceStageObject();
        Director.instance.LoadVariableObject();

        if (ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DResourceID != 0)
        {
            _mobCyalume3DController = MobController.CreateMobController(base.gameObject.transform);
        }

        StartStageObject();
        StartShaderParam();
        ReadOtherResource();
        Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
        Material[] array = null;
        MaterialController.MaterialExtensionData materialExtensionData = new MaterialController.MaterialExtensionData();
        NeonMaterialController.NeonMaterialExtensionData neonMaterialExtensionData = new NeonMaterialController.NeonMaterialExtensionData();
        _shaderLevel = 0;
        if (array != null)
        {
            for (int j = 0; j < componentsInChildren.Length; j++)
            {
                for (int k = 0; k < array.Length; k++)
                {
                    if (componentsInChildren[j].sharedMaterial == array[k])
                    {
                        componentsInChildren[j].gameObject.SetActive(value: false);
                        break;
                    }
                }
            }
        }
        materialExtensionData.Clear();
        materialExtensionData.materialNames = null;
        materialExtensionData.rootGameObject = base.gameObject;
        materialExtensionData.renderQueueOffsets = new int[1] { 3010 };
        _washLightMaterialCtrls = MaterialController.Initialize(ref _washLightMaterials, materialExtensionData);
        if (_washLightMaterialCtrls == null)
        {
            _washLightMaterialCtrls = new MaterialController[0];
        }
        if (_washLightMaterialGroup != null)
        {
            _washLightMaterialGroupColor1 = new Color[_washLightMaterialGroup.Length][];
            _washLightMaterialGroupColor2 = new Color[_washLightMaterialGroup.Length][];
            _washLightMaterialGroupPower = new float[_washLightMaterialGroup.Length][];
            for (int l = 0; l < _washLightMaterialGroup.Length; l++)
            {
                _washLightMaterialGroupColor1[l] = new Color[26];
                _washLightMaterialGroupColor2[l] = new Color[26];
                _washLightMaterialGroupPower[l] = new float[26];
                for (int m = 0; m < 26; m++)
                {
                    _washLightMaterialGroupColor1[l][m] = (_washLightMaterialGroupColor2[l][m] = Color.white);
                    _washLightMaterialGroupPower[l][m] = 1f;
                }
            }
            materialExtensionData.Clear();
            materialExtensionData.materialNames = null;
            materialExtensionData.rootGameObject = base.gameObject;
            materialExtensionData.renderQueueOffsets = new int[1] { 3010 };
            _washLightMaterialGroupCtrl = MaterialController.Initialize(ref _washLightMaterialGroup, materialExtensionData);
            if (_washLightMaterialGroupCtrl == null)
            {
                _washLightMaterialGroupCtrl = new MaterialController[0];
            }
        }
        materialExtensionData.Clear();
        materialExtensionData.materialNames = null;
        materialExtensionData.rootGameObject = base.gameObject;
        materialExtensionData.renderQueueOffsets = new int[1] { 3030 };
        MaterialController.Initialize(ref _laserMaterials, materialExtensionData);
        if (_laserMaterialGroup != null)
        {
            _laserMaterialGroupColor1 = new Color[_laserMaterialGroup.Length][];
            _laserMaterialGroupColor2 = new Color[_laserMaterialGroup.Length][];
            _laserMaterialGroupPower = new float[_laserMaterialGroup.Length][];
            _laserMaterialGroupRandomValue = new float[_laserMaterialGroup.Length][];
            for (int n = 0; n < _laserMaterialGroup.Length; n++)
            {
                _laserMaterialGroupColor1[n] = new Color[26];
                _laserMaterialGroupColor2[n] = new Color[26];
                _laserMaterialGroupPower[n] = new float[26];
                _laserMaterialGroupRandomValue[n] = new float[26];
                for (int num = 0; num < 26; num++)
                {
                    _laserMaterialGroupColor1[n][num] = Color.white;
                    _laserMaterialGroupColor2[n][num] = Color.black;
                    _laserMaterialGroupPower[n][num] = 1f;
                    _laserMaterialGroupRandomValue[n][num] = 1f;
                }
            }
            materialExtensionData.Clear();
            materialExtensionData.materialNames = null;
            materialExtensionData.rootGameObject = base.gameObject;
            materialExtensionData.renderQueueOffsets = new int[1] { 3030 };
            MaterialController.Initialize(ref _laserMaterialGroup, materialExtensionData);
        }
        materialExtensionData.Clear();
        materialExtensionData.materialNames = null;
        materialExtensionData.rootGameObject = base.gameObject;
        materialExtensionData.renderQueueOffsets = new int[1] { 3050 };
        MaterialController.Initialize(ref _footLightMaterials, materialExtensionData);
        if (_footLightMaterialGroup != null)
        {
            _footLightMaterialGroupColor1 = new Color[_footLightMaterialGroup.Length][];
            _footLightMaterialGroupColor2 = new Color[_footLightMaterialGroup.Length][];
            _footLightMaterialGroupPower = new float[_footLightMaterialGroup.Length][];
            for (int num2 = 0; num2 < _footLightMaterialGroup.Length; num2++)
            {
                _footLightMaterialGroupColor1[num2] = new Color[26];
                _footLightMaterialGroupColor2[num2] = new Color[26];
                _footLightMaterialGroupPower[num2] = new float[26];
                for (int num3 = 0; num3 < 26; num3++)
                {
                    _footLightMaterialGroupColor1[num2][num3] = (_footLightMaterialGroupColor2[num2][num3] = Color.white);
                    _footLightMaterialGroupPower[num2][num3] = 1f;
                }
            }
            materialExtensionData.Clear();
            materialExtensionData.materialNames = null;
            materialExtensionData.rootGameObject = base.gameObject;
            materialExtensionData.renderQueueOffsets = new int[1] { 3050 };
            MaterialController.Initialize(ref _footLightMaterialGroup, materialExtensionData);
        }
        neonMaterialExtensionData.Clear();
        neonMaterialExtensionData.materialNames = null;
        neonMaterialExtensionData.rootGameObject = base.gameObject;
        neonMaterialExtensionData.renderQueueOffsets = null;
        neonMaterialExtensionData.renderMainQueueOffset = 1990;
        neonMaterialExtensionData.renderBackQueueOffset = 3060;
        _neonMaterialCtrls = NeonMaterialController.Initialize(ref _neonMaterialInfos, neonMaterialExtensionData);
        materialExtensionData.Clear();
        materialExtensionData.materialNames = null;
        materialExtensionData.rootGameObject = base.gameObject;
        materialExtensionData.renderQueueOffsets = null;
        _movieMaterialCtrls = MovieMaterialController.Initialize(ref _movieMaterialInfos, materialExtensionData);
        materialExtensionData.Clear();
        materialExtensionData.materialNames = null;
        materialExtensionData.rootGameObject = base.gameObject;
        materialExtensionData.renderQueueOffsets = null;
        MaterialController.Initialize(ref _animationMaterials, materialExtensionData);
        materialExtensionData.Clear();
        materialExtensionData.materialNames = null;
        materialExtensionData.rootGameObject = base.gameObject;
        materialExtensionData.renderQueueOffsets = null;
        MaterialController.Initialize(ref _paletteObjectMaterials, materialExtensionData);
        GameObjectUtility.SetLayer(20, base.transform, (int layer, GameObject obj) => (obj.layer != 18) ? true : false);
        if (null != _mobController)
        {
            GameObjectUtility.SetLayer(19, _mobController.gameObject.transform);
        }
        if ((bool)_mobCyalume3DController)
        {
            GameObjectUtility.SetLayer(19, _mobCyalume3DController.gameObject.transform);
        }
        else if (_cyalumeController3D != null)
        {
            for (int num4 = 0; num4 < _cyalumeController3D.Length; num4++)
            {
                GameObjectUtility.SetLayer(19, _cyalumeController3D[num4].gameObject.transform);
            }
        }
        for (int num5 = 0; num5 < _Director.MemberUnitNumber; num5++)
        {
            GameObject gameObject = GameObjectUtility.FindGameObjectOfParent($"chara_locator_{num5 + 1:D02}", base.gameObject);
            if (!(gameObject == null))
            {
                _charaLocatorTransform[num5] = gameObject.transform;
            }
        }
        ShaderLodManager component = GetComponent<ShaderLodManager>();
        if (component != null && component.enabled)
        {
            component.SwitchShader(_shaderLevel);
            component.enabled = false;
        }
        for (int num6 = 0; num6 < componentsInChildren.Length; num6++)
        {
            Material sharedMaterial = componentsInChildren[num6].sharedMaterial;
            if (sharedMaterial == null)
            {
                continue;
            }
            string text = sharedMaterial.name;
            text = text.Replace("(Clone)", "");
            int key = FNVHash.Generate(text);
            bool flag = sharedMaterial.name != text;
            if (_instanceMaterialDictionary.ContainsKey(key))
            {
                continue;
            }
            Material material;
            if (flag)
            {
                material = sharedMaterial;
            }
            else
            {
                material = UnityEngine.Object.Instantiate(sharedMaterial);
                for (int num7 = num6; num7 < componentsInChildren.Length; num7++)
                {
                    if (componentsInChildren[num7].sharedMaterial == sharedMaterial)
                    {
                        componentsInChildren[num7].sharedMaterial = material;
                    }
                }
            }
            _instanceMaterialDictionary.Add(key, material);
        }
        SetupLiveTimelineControl();
        SetMainCamera(_liveTimelineControl);
        LiveTimelineSunshaftsSettings sunshaftsSettings = _liveTimelineControl.data.sunshaftsSettings;
        InitSunshaftsControl(sunshaftsSettings);
        LiveTimelineHdrBloomSettings hdrBloomSettings = _liveTimelineControl.data.hdrBloomSettings;
        InitHdrBloomControl(hdrBloomSettings);
        if (_globalEnvironmentTexture == null && !SingletonMonoBehaviour<ResourcesManager>.IsInstanceEmpty())
        {
            _globalEnvironmentTexture = SingletonMonoBehaviour<ResourcesManager>.instance.LoadObject("3D/Stage/stg_common_hq/Textures/tx_stg_env_common_hq") as Texture;
        }
        Director.instance.SetGlobalEnvTex(_globalEnvironmentTexture);
        Director.instance.InitializeA2UManager(instatialOnly: false);
        _SetupLensFlareParam();
        CollectBillboardController();
        CollectStageGazeController();
        if (_gimmickController != null)
        {
            _gimmickController.Initialize(base.gameObject);
        }
    }

    /*
    private GameDefine.eScoreRank GetScoreRank()
    {
        GameDefine.eScoreRank eScoreRank = (_Director != null) ? _Director.GetCurrentScoreRank() : GameDefine.eScoreRank.None;
        if (eScoreRank == GameDefine.eScoreRank.None)
        {
            eScoreRank = GameDefine.eScoreRank.Rank_C;
        }
        return eScoreRank;
    }
    */


    private void UpdateStageObjectAnimation()
    {
        if (_Director == null)
        {
            return;
        }
        bool flag = _Director.IsPauseLive();
        if (_isLastPause == flag)
        {
            return;
        }
        float speed = (flag ? 0f : 1f);
        _isLastPause = flag;
        for (int i = 0; i < _stageObjects.Length; i++)
        {
            if (_stageObjectsInfo[i]._Animations == null)
            {
                continue;
            }
            for (int j = 0; j < _stageObjectsInfo[i]._Animations.Length; j++)
            {
                foreach (AnimationState item in _stageObjectsInfo[i]._Animations[j])
                {
                    item.speed = speed;
                }
            }
        }
    }

    /// <summary>
    /// 紙吹雪を降らせる処理
    /// </summary>
    private void UpdateParticle()
    {
        if (_particleControllerList == null || _particleControllerList.Count == 0 || _Director == null || !_isEnabledConfetti)
        {
            return;
        }
        if (_Director.IsPauseLive())
        {
            for (int i = 0; i < _particleControllerList.Count; i++)
            {
                _particleControllerList[i].Pause();
            }
            return;
        }
        for (int i = 0; i < _particleControllerList.Count; i++)
        {
            _particleControllerList[i].Resume();
        }
        if (_Director.GetParticleControllType() != 0)
        {
            return;
        }
        for (int i = 0; i < _particleControllerList.Count; i++)
        {
            if (_particleControllerList[i].playStatus == ParticleController.Status.Stop)
            {
                _particleControllerList[i].PlayEmit();
            }
            else
            {
                _particleControllerList[i].ResumeEmit();
            }
        }
    }

    private void UpdateEffectStatus()
    {
        if (!(_Director == null) && _effectControllerList != null)
        {
            bool bSwitch = _Director.IsPauseLive();
            for (int i = 0; i < _effectControllerList.Count; i++)
            {
                _effectControllerList[i].Pause(bSwitch);
            }
        }
    }

    private void UpdateAnalyzer()
    {
        /*
        if (_analyzerController != null && _analyzerController.Length != 0 && !(_Director == null))
        {
            for (int i = 0; i < _analyzerController.Length; i++)
            {
                _analyzerController[i].timer = _Director.musicScoreTime;
            }
        }*/
    }

    private void Update()
    {
        SetupLiveTimelineControl();
        float num = (!(_Director != null)) ? Time.time : _Director.musicScoreTime;
        _LocalTimerParam.Param.Set(num * 0.05f, num, num * 2f, num * 3f);
        UpdateStageObjectAnimation();
        UpdateParticle();
        UpdateAnalyzer();
        UpdateEffectStatus();
        UpdateSunshaftsControl();
    }

    private void LateUpdate()
    {
        if (_borderLightTransform != null)
        {
            Vector4 vector = _borderLightTransform.TransformDirection(Vector3.back);
            vector.w = 0f;
            vector = Vector4.Normalize(vector);
            Shader.SetGlobalVector(_sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.BorderLightDir), vector);
        }
        if (_isStageGazeController)
        {
            StageGazeController[] stageGazeController = _stageGazeController;
            for (int i = 0; i < stageGazeController.Length; i++)
            {
                stageGazeController[i].AlterLateUpdate();
            }
        }
        if (_mobController != null)
        {
            _mobController.UpdateMaterial();
        }
        if (!_mobCyalume3DController && _cyalumeController3D != null)
        {
            int j = 0;
            for (int num = _cyalumeController3D.Length; j < num; j++)
            {
                _cyalumeController3D[j].UpdateMaterial();
            }
        }
    }

    private void OnDestroy()
    {
        if (_liveTimelineControl != null)
        {
            _liveTimelineControl.OnUpdateCameraPos -= UpdateCameraPos;
            _liveTimelineControl.OnStartUpdate -= StartUpdateTimeline;
            _liveTimelineControl.OnPreUpdateAllTimeline -= PreUpdateAllTimeline;
            _liveTimelineControl.OnUpdateBgColor1 -= UpdateBgColor1;
            _liveTimelineControl.OnUpdateBgColor2 -= UpdateBgColor2;
            _liveTimelineControl.OnUpdateBgColor3 -= UpdateBgColor3;
            _liveTimelineControl.OnUpdateMonitorControl -= UpdateMonitorControl;
            _liveTimelineControl.OnUpdateProjector -= UpdateProjector;
            _liveTimelineControl.OnUpdateAnimation -= UpdateAnimation;
            _liveTimelineControl.OnUpdateTransform -= UpdateTransform;
            _liveTimelineControl.OnUpdateLaser -= UpdateLaser;
            _liveTimelineControl.OnUpdateLensFlare -= UpdateLensFlare;
            _liveTimelineControl.OnUpdateGlass -= UpdateGlass;
            _liveTimelineControl.OnUpdateMobCyalume -= UpdateMobCyalume;
            _liveTimelineControl.OnUpdateMobCyalume3D -= UpdateMobCyalume3D;
            _liveTimelineControl.OnUpdateVolumeLight -= UpdateVolumeLight;
            _liveTimelineControl.OnUpdateLensFlare -= UpdateLensFlare;
        }
        A2U.Final();
        if (_dynamicBatchHelperMaterials != null)
        {
            Material[] dynamicBatchHelperMaterials = _dynamicBatchHelperMaterials;
            for (int i = 0; i < dynamicBatchHelperMaterials.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(dynamicBatchHelperMaterials[i]);
            }
            _dynamicBatchHelperMaterials = null;
        }
    }

    private bool UpdateShaderParamColor1(ShaderParamVector4 param, ref BgColor1UpdateInfo updateInfo)
    {
        if (param == null)
        {
            return false;
        }
        if (updateInfo.data.nameHash == HASH_BG_BL)
        {
            UpdateShaderParamColor1NoCheck(ref param.Param, ref updateInfo);
            if (null != _mobController)
            {
                _mobController.SetAmbientColor(updateInfo.color * updateInfo.value);
            }
            return true;
        }
        return false;
    }

    private void UpdateShaderParamColor1NoCheck(ref Vector4 outValue, ref BgColor1UpdateInfo updateInfo)
    {
        Vector4 vector = (outValue = updateInfo.color * updateInfo.value);
    }

    private bool UpdateCharaColor(ref BgColor1UpdateInfo updateInfo)
    {
        if (_Director == null)
        {
            return false;
        }
        bool flag = false;
        int num;
        if (updateInfo.data.nameHash == HASH_CHARA_CENTER)
        {
            num = 1;
        }
        else if (updateInfo.data.nameHash == HASH_CHARA_OTHER)
        {
            num = 32766;
        }
        else
        {
            if (updateInfo.data.nameHash != HASH_CHARA_COLOR && updateInfo.data.nameHash != HASH_CHARA_RIMCOLOR)
            {
                return false;
            }
            flag = updateInfo.data.nameHash == HASH_CHARA_RIMCOLOR;
            num = ((updateInfo.flags != 0) ? updateInfo.flags : 32767);
        }
        Vector4 outValue = Vector4.one;
        int num2 = 0;
        UpdateShaderParamColor1NoCheck(ref outValue, ref updateInfo);
        if (flag)
        {
            for (num2 = 0; num2 < _Director.MemberUnitNumber; num2++)
            {
                if ((num & (1 << num2)) != 0)
                {
                    _Director.getCharacterObjectFromPositionID((LiveCharaPosition)num2).SetRimColorMulti(outValue);
                }
            }
        }
        else
        {
            for (num2 = 0; num2 < _Director.MemberUnitNumber; num2++)
            {
                if ((num & (1 << num2)) != 0)
                {
                    _Director.getCharacterObjectFromPositionID((LiveCharaPosition)num2).renderController.SetColor(ref outValue);
                }
            }
        }
        return true;
    }

    private bool UpdateCharaFollowSpotColor(ref BgColor1UpdateInfo updateInfo)
    {
        if (_Director == null)
        {
            return false;
        }
        int num;
        if (updateInfo.data.nameHash == HASH_FLLOW_SPOT_CENTER)
        {
            num = 1;
        }
        else if (updateInfo.data.nameHash == HASH_FLLOW_SPOT_OHTER)
        {
            num = 32766;
        }
        else
        {
            if (updateInfo.data.nameHash != HASH_FLLOW_SPOT_COLOR)
            {
                return false;
            }
            num = ((updateInfo.flags != 0) ? updateInfo.flags : 32767);
        }
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor0);
        int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
        for (int i = 0; i < _Director.MemberUnitNumber; i++)
        {
            if ((num & (1 << i)) == 0)
            {
                continue;
            }
            FollowSpotLightController[] followSpotLightController = _Director.getCharacterObjectFromPositionID((LiveCharaPosition)i).followSpotLightController;
            if (followSpotLightController != null && followSpotLightController.Length != 0 && !(followSpotLightController[0] == null))
            {
                MaterialPropertyBlock materialPropertyBlock = followSpotLightController[0].materialPropertyBlock;
                if (materialPropertyBlock != null)
                {
                    materialPropertyBlock.SetColor(propertyID, updateInfo.color);
                    materialPropertyBlock.SetFloat(propertyID2, updateInfo.value);
                }
            }
        }
        return true;
    }

    private bool UpdateShadowColor1(ref BgColor1UpdateInfo updateInfo)
    {
        if (_Director == null)
        {
            return false;
        }
        if (updateInfo.data.nameHash != HASH_CHARA_SHADOW)
        {
            return false;
        }
        int flags = updateInfo.flags;
        CharacterObject characterObject = null;
        for (int i = 0; i < _Director.MemberUnitNumber; i++)
        {
            if ((flags & (1 << i)) != 0)
            {
                characterObject = _Director.getCharacterObjectFromPositionID((LiveCharaPosition)i);
                if (characterObject != null)
                {
                    characterObject.UpdateShadowFactor(updateInfo.value, ref updateInfo.color);
                }
            }
        }
        return true;
    }

    private void UpdateBgColor1(ref BgColor1UpdateInfo updateInfo)
    {
        if (!UpdateShaderParamColor1(_AmbientColorParam, ref updateInfo) && !UpdateCharaColor(ref updateInfo) && !UpdateCharaFollowSpotColor(ref updateInfo) && !UpdateShadowColor1(ref updateInfo))
        {
            UpdateMovieMaterialColor1(ref updateInfo, sLabelToIDMovieColor);
        }
    }

    private bool UpdateMovieMaterialColor1(ref BgColor1UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (_movieMaterialInfos == null)
        {
            return false;
        }
        if (_movieMaterialInfos.Length == 0)
        {
            return false;
        }
        if (sLabelToIDMovieColor == null)
        {
            return false;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return false;
        }
        if (id >= _movieMaterialInfos.Length)
        {
            return false;
        }
        Vector4 value = updateInfo.color;
        value *= updateInfo.value;
        value.w = updateInfo.color.a;
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor0);
        _movieMaterialInfos[id]._movieMaterial.SetVector(propertyID, value);
        return true;
    }

    private void UpdateBillboard(ref BgColor2UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return;
        }
        _billboardManager.Get(id, out var resultController, out var resultEnable);
        if (resultController == null || resultEnable == null)
        {
            return;
        }
        if (!updateInfo.enableManualAngle)
        {
            for (int i = 0; i < resultController.Length; i++)
            {
                if (resultEnable[i])
                {
                    resultController[i].enabled = true;
                }
            }
            return;
        }
        for (int j = 0; j < resultController.Length; j++)
        {
            if (resultEnable[j])
            {
                resultController[j].enabled = false;
                resultController[j].transform.localRotation = updateInfo.manualRotation;
            }
        }
    }

    private void CalclulateColor2Parameter(ref BgColor2UpdateInfo updateInfo, out Color color1, out Color color2, out float power)
    {
        float num = 1f;
        float num2 = 1f;
        //num = _nowScoreRankEffect._colorRate;
        //num2 = _nowScoreRankEffect._powerRate;
        Color grayScaleColor = Common.GetGrayScaleColor(updateInfo.color1);
        Color grayScaleColor2 = Common.GetGrayScaleColor(updateInfo.color2);
        color1 = Color.Lerp(grayScaleColor, updateInfo.color1, num);
        color2 = Color.Lerp(grayScaleColor2, updateInfo.color2, num);
        power = updateInfo.value * num2;
    }

    private float UpdateMaterialColor2Parameter(Material material, ref BgColor2UpdateInfo updateInfo)
    {
        CalclulateColor2Parameter(ref updateInfo, out var color, out var color2, out var power);
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor0);
        int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor1);
        int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
        material.SetColor(propertyID, color);
        material.SetColor(propertyID2, color2);
        material.SetFloat(propertyID3, power);
        return power;
    }

    private Material UpdateMaterialColor2(MaterialController[] materialController, Material[] materials, ref BgColor2UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary, out int materialIndex)
    {
        materialIndex = -1;
        if (materials == null)
        {
            return null;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return null;
        }
        if (id >= materials.Length)
        {
            return null;
        }
        materialIndex = id;
        Material result = materials[id];
        UpdateMaterialColor2Parameter(materials[id], ref updateInfo);
        return result;
    }

    private Material UpdateMaterialColor2Group(Material[] materials, ref BgColor2UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary, Color[][] groupColor1, Color[][] groupColor2, float[][] groupPower, out int materialIndex, int groupNum)
    {
        materialIndex = -1;
        if (materials == null)
        {
            return null;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return null;
        }
        id = (materialIndex = id / groupNum);
        Material obj = materials[id];
        CalclulateColor2Parameter(ref updateInfo, out var color, out var color2, out var power);
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor0);
        int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor1);
        int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
        int groupIndex = updateInfo.groupIndex;
        groupColor1[id][groupIndex] = color;
        groupColor2[id][groupIndex] = color2;
        groupPower[id][groupIndex] = power;
        obj.SetColorArray(propertyID, groupColor1[id]);
        obj.SetColorArray(propertyID2, groupColor2[id]);
        obj.SetFloatArray(propertyID3, groupPower[id]);
        return obj;
    }

    private Material UpdateMaterialColor2Group(MaterialController[] materialController, Material[] materials, ref BgColor2UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary, Color[][] groupColor1, Color[][] groupColor2, float[][] groupPower, out int materialIndex, int groupNum)
    {
        materialIndex = -1;
        if (materials == null)
        {
            return null;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return null;
        }
        id = (materialIndex = id / groupNum);
        Material obj = materials[id];
        CalclulateColor2Parameter(ref updateInfo, out var color, out var color2, out var power);
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor0);
        int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor1);
        int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
        int groupIndex = updateInfo.groupIndex;
        groupColor1[id][groupIndex] = color;
        groupColor2[id][groupIndex] = color2;
        groupPower[id][groupIndex] = power;
        obj.SetColorArray(propertyID, groupColor1[id]);
        obj.SetColorArray(propertyID2, groupColor2[id]);
        obj.SetFloatArray(propertyID3, groupPower[id]);
        return obj;
    }

    private Material UpdateMaterialColor2(Material[] materials, ref BgColor2UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (materials == null)
        {
            return null;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return null;
        }
        if (id >= materials.Length)
        {
            return null;
        }
        Material material = materials[id];
        UpdateMaterialColor2Parameter(material, ref updateInfo);
        return material;
    }

    private bool UpdateMaterialColorNeon(ref BgColor2UpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (_neonMaterialCtrls == null)
        {
            return false;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return false;
        }
        if (id >= _neonMaterialCtrls.Length)
        {
            return false;
        }
        NeonMaterialController.MoveMaterialInfo obj = _neonMaterialInfos[id];
        float num = 1f;
        float num2 = 1f;
        //num = _nowScoreRankEffect._colorRate;
        //num2 = _nowScoreRankEffect._powerRate;
        Color grayScaleColor = Common.GetGrayScaleColor(updateInfo.color1);
        Color grayScaleColor2 = Common.GetGrayScaleColor(updateInfo.color2);
        Color value = Color.Lerp(grayScaleColor, updateInfo.color1, num);
        Color value2 = Color.Lerp(grayScaleColor2, updateInfo.color2, num);
        float num3 = updateInfo.value * num2;
        float num4 = Mathf.Clamp(num3, 0f, 2f);
        float value3 = Mathf.Clamp(num3 - 0.5f, 0f, 0.5f);
        float value4 = Mathf.Min(num4, 1f);
        Material mainMaterial = obj._mainMaterial;
        Material backMaterial = obj._backMaterial;
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.BrightColor);
        int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.BaseColor);
        int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
        int propertyID4 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorClampPower);
        mainMaterial.SetColor(propertyID, value2);
        mainMaterial.SetColor(propertyID2, value);
        mainMaterial.SetFloat(propertyID3, num4);
        mainMaterial.SetFloat(propertyID4, value4);
        backMaterial.SetColor(propertyID2, value);
        backMaterial.SetFloat(propertyID3, value3);
        return true;
    }

    private void UpdateBgColor2(ref BgColor2UpdateInfo updateInfo)
    {
        UpdateBillboard(ref updateInfo, sLabelToIDBgWash);
        UpdateMaterialColorNeon(ref updateInfo, sLabelToIDBgNeon);
        Material material;
        if (updateInfo.groupIndex >= 0)
        {
            UpdateMaterialColor2Group(_washLightMaterialGroupCtrl, _washLightMaterialGroup, ref updateInfo, sLabelToIDBgWash, _washLightMaterialGroupColor1, _washLightMaterialGroupColor2, _washLightMaterialGroupPower, out var materialIndex, 26);
            if (materialIndex != -1)
            {
                int groupIndex = updateInfo.groupIndex;
                Color color = _washLightMaterialGroupColor1[materialIndex][groupIndex];
                float power = _washLightMaterialGroupPower[materialIndex][groupIndex];
                UnityLensFlareController[] array = _lensFlareControllerGroupTable[materialIndex][groupIndex];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].SetWashLightColor(color, power);
                }
            }
            else
            {
                material = UpdateMaterialColor2Group(_laserMaterialGroup, ref updateInfo, sLabelToIDLaser, _laserMaterialGroupColor1, _laserMaterialGroupColor2, _laserMaterialGroupPower, out materialIndex, 26);
                if (materialIndex != -1)
                {
                    int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.RandomValue);
                    float num = Mathf.Clamp01(_Director.randomTable[updateInfo.rndValueIdx] + updateInfo.color1.a);
                    _laserMaterialGroupRandomValue[materialIndex][updateInfo.groupIndex] = num;
                    material.SetFloatArray(propertyID, _laserMaterialGroupRandomValue[materialIndex]);
                }
                else
                {
                    UpdateMaterialColor2Group(_footLightMaterialGroup, ref updateInfo, sLabelToIDBgFoot, _footLightMaterialGroupColor1, _footLightMaterialGroupColor2, _footLightMaterialGroupPower, out materialIndex, 26);
                }
            }
            return;
        }
        int materialIndex2;
        Material material2 = UpdateMaterialColor2(_washLightMaterialCtrls, _washLightMaterials, ref updateInfo, sLabelToIDBgWash, out materialIndex2);
        if (materialIndex2 != -1)
        {
            int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MulColor0);
            int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPower);
            Color color2 = material2.GetColor(propertyID2);
            float @float = material2.GetFloat(propertyID3);
            UnityLensFlareController[] array = _lensFlareControllerGroup[materialIndex2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetWashLightColor(color2, @float);
            }
        }
        UpdateMaterialColor2(_footLightMaterials, ref updateInfo, sLabelToIDBgFoot);
        material = UpdateMaterialColor2(_laserMaterials, ref updateInfo, sLabelToIDLaser);
        if (material != null)
        {
            int propertyID4 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.RandomValue);
            float value = Mathf.Clamp01(_Director.randomTable[updateInfo.rndValueIdx] + updateInfo.color1.a);
            material.SetFloat(propertyID4, value);
        }
    }

    private void UpdateBgColor3(ref BgColor3UpdateInfo updateInfo)
    {
        Material value = null;
        if (_instanceMaterialDictionary.TryGetValue(updateInfo.data.nameHash, out value))
        {
            int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.ColorPalette);
            value.SetColorArray(propertyID, updateInfo.colorArray);
        }
    }

    private void SetUVAdjustInfoToShader(Material movieMaterial, ref Vector2 resolution)
    {
        int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.UVAdjust);
        int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MonitorWidth);
        int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MonitorHeight);
        float @float = movieMaterial.GetFloat(propertyID2);
        float float2 = movieMaterial.GetFloat(propertyID3);
        if (@float >= 1f && float2 >= 1f)
        {
            float aspectRatio = @float / float2;
            TransformAspectRatio(ref resolution, aspectRatio, out var uvScale, out var uvOffset);
            movieMaterial.SetVector(propertyID, new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y));
        }
        else
        {
            movieMaterial.SetVector(propertyID, new Vector4(1f, 1f, 0f, 0f));
        }
    }

    public static void TransformAspectRatio(ref Vector2 resolution, float aspectRatio, out Vector2 uvScale, out Vector2 uvOffset)
    {
        float num = resolution.x / resolution.y;
        float num2 = resolution.x;
        float num3 = resolution.y;
        uvScale = Vector2.one;
        uvOffset = Vector2.zero;
        if (num < aspectRatio)
        {
            num3 = num2 / aspectRatio;
            uvScale.y = num3 / resolution.y;
            uvOffset.y = (1f - uvScale.y) * 0.5f;
        }
        else
        {
            num2 = num3 * aspectRatio;
            uvScale.x = num2 / resolution.x;
            uvOffset.x = (1f - uvScale.x) * 0.5f;
        }
        resolution.x = num2;
        resolution.y = num3;
    }

    /// <summary>
    /// RenderTextureからモニターへ出力する
    /// </summary>
    private void UpdateMonitorByRenderTexture(MonitorController monitorController, Material monitorMaterial, RenderTexture rtInputSource, ref MonitorControlUpdateInfo updateInfo)
    {
        if (!(_Director == null) && !(rtInputSource == null))
        {
            monitorController.m_CenterPosition = updateInfo.pos;
            monitorController.m_Scale = updateInfo.size;
            monitorController.m_Multi = updateInfo.multiFlag;
            monitorController.UpdateParam();
            monitorMaterial.mainTexture = rtInputSource;
            monitorMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
            monitorMaterial.mainTextureOffset = monitorController.m_Offset;
            monitorMaterial.mainTextureScale = monitorController.m_Scale;
            Vector2 resolution = new Vector2(rtInputSource.width, rtInputSource.height);
            SetUVAdjustInfoToShader(monitorMaterial, ref resolution);
        }
    }

    /// <summary>
    /// モニターにカメラ映像を表示する
    /// </summary>
    private void UpdateMonitorInCaptureScreen(MonitorController monitorController, Material monitorMaterial, ref MonitorControlUpdateInfo updateInfo)
    {
        if (_Director == null)
        {
            return;
        }
        if (_Director.IsMonitorCameraOn())
        {
            RenderTexture monitorCameraTexture = _Director.monitorCameraManager.GetMonitorCameraTexture(0);
            if (monitorCameraTexture != null)
            {
                UpdateMonitorByRenderTexture(monitorController, monitorMaterial, monitorCameraTexture, ref updateInfo);
            }
            else
            {
                UpdateMonitorByRenderTexture(monitorController, monitorMaterial, _Director.renderTarget.monitor, ref updateInfo);
            }
        }
        else
        {
            UpdateMonitorByRenderTexture(monitorController, monitorMaterial, _Director.renderTarget.monitor, ref updateInfo);
        }
    }

    /// <summary>
    /// モニターにモニターカメラの映像を投影する
    /// </summary>
    private void UpdateMonitorByMonitorCamera(MonitorController monitorController, Material monitorMaterial, ref MonitorControlUpdateInfo updateInfo)
    {
        if (!(_Director == null))
        {
            RenderTexture rtInputSource = _Director.monitorCameraManager.GetMonitorCameraTexture(updateInfo.dispID) ?? _Director.renderTarget.monitor;
            UpdateMonitorByRenderTexture(monitorController, monitorMaterial, rtInputSource, ref updateInfo);
        }
    }

    /// <summary>
    /// モニターに動画を描画する
    /// </summary>
    private void UpdateMonitorInMovie(Material monitorMaterial, ref MonitorControlUpdateInfo updateInfo)
    {
        int num = updateInfo.dispID - 1;
        if (_isVertical && updateInfo.dispID == 0 && _MovieManager._MovieControllers.Length != 0)
        {
            num = 0;
        }
        if (num < 0 || _MovieManager == null || _MovieManager._MovieControllers == null)
        {
            return;
        }
        if (num >= _MovieManager._MovieControllers.Length)
        {
            num = 0;
        }
        UVMovieController uvMovieController = _MovieManager._MovieControllers[num];
        if (uvMovieController == null)
        {
            uvMovieController = _MovieManager._MovieControllers[0];
            if (uvMovieController == null)
            {
                return;
            }
        }
        uvMovieController.SetPlayStartSec(updateInfo.playStartOffsetFrame);
        if (_Director == null)
        {
            uvMovieController.UpdateAddTime(Time.deltaTime, updateInfo.reversePlayFlag);
        }
        else
        {
            uvMovieController.UpdateSetTime(updateInfo.progressTime * updateInfo.speed, updateInfo.reversePlayFlag);
        }
        if (string.IsNullOrEmpty(updateInfo.textureLabel))
        {
            monitorMaterial.mainTexture = uvMovieController.mainTexture;
            monitorMaterial.mainTextureOffset = uvMovieController.mainTextureOffset;
            monitorMaterial.mainTextureScale = uvMovieController.mainTextureScale;
        }
        else
        {
            monitorMaterial.SetTexture(updateInfo.textureLabel, uvMovieController.mainTexture);
            monitorMaterial.SetTextureOffset(updateInfo.textureLabel, uvMovieController.mainTextureOffset);
            monitorMaterial.SetTextureScale(updateInfo.textureLabel, uvMovieController.mainTextureScale);
        }
        if (uvMovieController.existMaskTex)
        {
            int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MainTexAlpha);
            monitorMaterial.SetTexture(propertyID, uvMovieController.maskTexture);
        }
    }

    /// <summary>
    /// モニターに静止画を描画する
    /// </summary>
    private void UpdateMonitorInTexture(Material monitorMaterial, ResouceImageLabel imageLabel)
    {
        if (_ImageResources == null || _ImageResources.Length == 0)
        {
            return;
        }
        int num = (int)imageLabel;
        if (num >= _ImageResources.Length)
        {
            num = 0;
        }
        Texture texture = _ImageResources[num];
        if (texture == null)
        {
            texture = _ImageResources[num];
            if (texture == null)
            {
                return;
            }
        }
        monitorMaterial.mainTexture = texture;
        monitorMaterial.mainTextureOffset = SharedShaderParam._vector2ZeroZero;
        monitorMaterial.mainTextureScale = SharedShaderParam._vector2OneOne;
    }

    /// <summary>
    /// モニターに表示する画を切り替え
    /// </summary>
    private bool UpdateMonitor(ref MonitorControlUpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (updateInfo.dispID < 0)
        {
            return false;
        }
        if (_movieMaterialCtrls == null)
        {
            return false;
        }
        if (_movieMaterialCtrls.Length == 0)
        {
            return false;
        }
        Material material = null;
        if (labelToIDDictionary == null)
        {
            material = _movieMaterialInfos[0]._movieMaterial;
            UpdateMonitorInMovie(material, ref updateInfo);
        }
        else
        {
            if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
            {
                return false;
            }
            if (id >= _movieMaterialCtrls.Length)
            {
                return false;
            }
            if (_movieMaterialCtrls[id]._Renderer == null)
            {
                return false;
            }
            if (_movieMaterialCtrls[id]._Renderer.Length == 0)
            {
                return false;
            }
            material = _movieMaterialInfos[id]._movieMaterial;
            int propertyID = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.BlendFactor);
            material.SetFloat(propertyID, updateInfo.blendFactor);
            int propertyID2 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.MonitorColorPower);
            material.SetFloat(propertyID2, updateInfo.colorPower);
            int propertyID3 = _sharedShaderParam.getPropertyID(SharedShaderParam.ShaderProperty.UVAdjust);
            material.SetVector(propertyID3, new Vector4(1f, 1f, 0f, 0f));
            bool flag = true;
            //flag = _nowScoreRankEffect._enableMovie;
            /*
            if (_qualityType == LocalData.OptionData.eQualityType.Quality3D_Light)
            {
                if (flag)
                {
                    switch (_movieMaterialInfos[id]._movieType)
                    {
                        case MovieMaterialController.MovieType.MainMonitor:
                        case MovieMaterialController.MovieType.SubMonitor:
                            if (updateInfo.dispID == 0 && !_isVertical)
                            {
                                UpdateMonitorInTexture(material, ResouceImageLabel.Capture);
                            }
                            else
                            {
                                UpdateMonitorInMovie(material, ref updateInfo);
                            }
                            break;
                        case MovieMaterialController.MovieType.Parts:
                            UpdateMonitorInMovie(material, ref updateInfo);
                            break;
                        case MovieMaterialController.MovieType.Overlay:
                            if (updateInfo.dispID == 0 && !_isVertical)
                            {
                                UpdateMonitorInTexture(material, ResouceImageLabel.Capture);
                            }
                            else
                            {
                                UpdateMonitorInMovie(material, ref updateInfo);
                            }
                            break;
                    }
                }
                else
                {
                    switch (_movieMaterialInfos[id]._movieType)
                    {
                        case MovieMaterialController.MovieType.MainMonitor:
                            UpdateMonitorInTexture(material, ResouceImageLabel.MainMonitor);
                            break;
                        case MovieMaterialController.MovieType.SubMonitor:
                            UpdateMonitorInTexture(material, ResouceImageLabel.SubMonitor);
                            break;
                        case MovieMaterialController.MovieType.Parts:
                        case MovieMaterialController.MovieType.Overlay:
                            UpdateMonitorInMovie(material, ref updateInfo);
                            break;
                    }
                }
            }
            else
            */
            if (flag)
            {
                if (!_isVertical)
                {
                    switch (updateInfo.inputSource)
                    {
                        case LiveTimelineKeyMonitorControlData.eInputSource.Default:
                            if (updateInfo.dispID == 0)
                            {
                                UpdateMonitorInCaptureScreen(_movieMaterialCtrls[id]._monitorContoroller, material, ref updateInfo);
                            }
                            else
                            {
                                UpdateMonitorInMovie(material, ref updateInfo);
                            }
                            break;
                        case LiveTimelineKeyMonitorControlData.eInputSource.MonitorCamera:
                            if (_Director.IsMonitorCameraOn())
                            {
                                UpdateMonitorByMonitorCamera(_movieMaterialCtrls[id]._monitorContoroller, material, ref updateInfo);
                            }
                            else
                            {
                                UpdateMonitorInCaptureScreen(_movieMaterialCtrls[id]._monitorContoroller, material, ref updateInfo);
                            }
                            break;
                    }
                }
                else
                {
                    UpdateMonitorInMovie(material, ref updateInfo);
                }
            }
            else
            {
                switch (_movieMaterialInfos[id]._movieType)
                {
                    case MovieMaterialController.MovieType.MainMonitor:
                        UpdateMonitorInTexture(material, ResouceImageLabel.MainMonitor);
                        break;
                    case MovieMaterialController.MovieType.SubMonitor:
                        UpdateMonitorInTexture(material, ResouceImageLabel.SubMonitor);
                        break;
                    case MovieMaterialController.MovieType.Parts:
                    case MovieMaterialController.MovieType.Overlay:
                        UpdateMonitorInMovie(material, ref updateInfo);
                        break;
                }
            }
        }
        return true;
    }

    private bool UpdateAnimationObjectOld(ref AnimationUpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return false;
        }
        if (_animationObjectController.Length <= id)
        {
            return false;
        }
        if (_animationObjectController[id] == null)
        {
            return false;
        }
        _animationObjectController[id].UpdateStatus(ref updateInfo);
        return true;
    }

    private void UpdateMaterialTextureAnimaionCommon(Material material, ref TextureAnimationUpdateInfo updateInfo)
    {
        Vector2 vector;
        if (updateInfo.scrollInterval == 0f)
        {
            vector = updateInfo.offset;
        }
        else
        {
            int num = (int)(updateInfo.progressTime * 60f / updateInfo.scrollInterval);
            vector = updateInfo.offset + updateInfo.scrollSpeed * num;
        }
        vector.x %= 1f;
        vector.y %= 1f;
        if (updateInfo.textureNameEmpty)
        {
            material.mainTextureScale = updateInfo.tiling;
            material.mainTextureOffset = vector;
        }
        else
        {
            material.SetTextureScale(updateInfo.textureName, updateInfo.tiling);
            material.SetTextureOffset(updateInfo.textureName, vector);
        }
    }

    private bool UpdateMaterialTextureAnimaion(Material[] materials, ref TextureAnimationUpdateInfo updateInfo, LabelToIDDictionary labelToIDDictionary)
    {
        if (materials == null)
        {
            return false;
        }
        if (labelToIDDictionary == null)
        {
            return false;
        }
        if (!labelToIDDictionary.getID(updateInfo.data.nameHash, out var id))
        {
            return false;
        }
        if (id >= materials.Length)
        {
            return false;
        }
        UpdateMaterialTextureAnimaionCommon(materials[id], ref updateInfo);
        return true;
    }

    private bool UpdateMaterialTextureAnimaionDictionary(ref TextureAnimationUpdateInfo updateInfo)
    {
        if (!_instanceMaterialDictionary.TryGetValue(updateInfo.data.nameHash, out var value))
        {
            return false;
        }
        UpdateMaterialTextureAnimaionCommon(value, ref updateInfo);
        return true;
    }

    private bool UpdateMovieMaterialTextureAnimaion(ref TextureAnimationUpdateInfo updateInfo)
    {
        if (_movieMaterialCtrls == null)
        {
            return false;
        }
        if (!sLabelToIDBgMonitor.getID(updateInfo.data.nameHash, out var id))
        {
            return false;
        }
        if (id >= _movieMaterialCtrls.Length)
        {
            return false;
        }
        Material movieMaterial = _movieMaterialInfos[id]._movieMaterial;
        movieMaterial.mainTextureScale = updateInfo.tiling;
        Vector2 mainTextureOffset;
        if (updateInfo.scrollInterval == 0f)
        {
            mainTextureOffset = updateInfo.offset;
        }
        else
        {
            int num = (int)(updateInfo.progressTime * 60f / updateInfo.scrollInterval);
            mainTextureOffset = updateInfo.offset + updateInfo.scrollSpeed * num;
        }
        mainTextureOffset.x %= 1f;
        mainTextureOffset.y %= 1f;
        movieMaterial.mainTextureOffset = mainTextureOffset;
        return true;
    }

    private void UpdateMonitorControl(ref MonitorControlUpdateInfo updateInfo)
    {
        UpdateMonitor(ref updateInfo, sLabelToIDBgMonitor);
    }

    private void UpdateProjector(ref ProjectorUpdateInfo updateInfo)
    {
        if (_projectorControllerList != null)
        {
            for (int i = 0; i < _projectorControllerList.Count; i++)
            {
                _projectorControllerList[i].UpdateStatus(ref updateInfo, 1f);
            }
        }
    }

    /// <summary>
    /// ステージオブジェクトのアニメーションを動かす
    /// </summary>
    private void UpdateAnimation(ref AnimationUpdateInfo updateInfo)
    {
        if (_animationObjectController != null && UpdateAnimationObjectOld(ref updateInfo, sLabelToIDAnimeObj))
        {
            return;
        }
        int nameHash = updateInfo.data.nameHash;
        if (!_objectsWorkInfoDictionary.TryGetValue(nameHash, out var value))
        {
            return;
        }
        for (int i = 0; i < value.Count; i++)
        {
            StageObjectWorkInfo stageObjectWorkInfo = value[i];
            if (!(stageObjectWorkInfo.animationController == null))
            {
                stageObjectWorkInfo.animationController.UpdateStatus(ref updateInfo);
            }
        }
    }

    /// <summary>
    /// テクスチャアニメーションを行う
    /// </summary>
    private void UpdateTextureAnimation(ref TextureAnimationUpdateInfo updateInfo)
    {
        if (!UpdateMaterialTextureAnimaionDictionary(ref updateInfo) && !UpdateMaterialTextureAnimaion(_washLightMaterials, ref updateInfo, sLabelToIDBgWash) && (!IsMirrorScaneOn() || !UpdateMaterialTextureAnimaion(_mirorrScanLightMaterials, ref updateInfo, sLabelToIDBgMirrorScan)) && !UpdateMaterialTextureAnimaion(_footLightMaterials, ref updateInfo, sLabelToIDBgFoot) && !UpdateMovieMaterialTextureAnimaion(ref updateInfo))
        {
            UpdateMaterialTextureAnimaion(_animationMaterials, ref updateInfo, sLabelToIDBgAnim);
        }
    }


    /// <summary>
    /// オブジェクトのトランスフォームのみ行う
    /// </summary>
    private void UpdateTransform(ref TransformUpdateInfo updateInfo)
    {
        if (_objectsWorkInfoDictionary.TryGetValue(updateInfo.data.nameHash, out var value))
        {
            for (int i = 0; i < value.Count; i++)
            {
                StageObjectWorkInfo stageObjectWorkInfo = value[i];
                updateInfo.data.OnUpdateTimelineTransform(stageObjectWorkInfo.cacheTransform, ref updateInfo.updateData, ref stageObjectWorkInfo.startData);
            }
        }
    }

    /// <summary>
    /// オブジェクトの表示非表示の設定のみ行う
    /// </summary>
    private void UpdateRenderer(ref RendererUpdateInfo updateInfo)
    {
        if (_stageObjectsHashToID.TryGetValue(updateInfo.data.nameHash, out var value))
        {
            for (int i = 0; i < _stageObjectsInfo[value]._Renderers.Length; i++)
            {
                _stageObjectsInfo[value]._Renderers[i].enabled = updateInfo.renderEnable;
            }
        }
        else
        {
            if (!_objectsWorkInfoDictionary.TryGetValue(updateInfo.data.nameHash, out var value2))
            {
                return;
            }
            for (value = 0; value < value2.Count; value++)
            {
                StageObjectWorkInfo stageObjectWorkInfo = value2[value];
                if (!(stageObjectWorkInfo.renderer == null))
                {
                    stageObjectWorkInfo.renderer.enabled = updateInfo.renderEnable;
                }
            }
        }
    }

    /// <summary>
    /// オブジェクトの表示非表示設定
    /// およびオブジェクトのトランスフォームを行う
    /// </summary>
    private void UpdateObject(ref ObjectUpdateInfo updateInfo)
    {
        int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.Color);
        int layer = (updateInfo.reflectionEnable ? 20 : 19);
        if (_stageObjectsHashToID.TryGetValue(updateInfo.data.nameHash, out var value))
        {
            for (int i = 0; i < _stageObjectsInfo[value]._Renderers.Length; i++)
            {
                _stageObjectsInfo[value]._Renderers[i].enabled = updateInfo.renderEnable;
                _stageObjectsInfo[value]._RendererGameObjects[i].layer = layer;
            }
            if (updateInfo.colorEnable)
            {
                Renderer[] renderers = _stageObjectsInfo[value]._Renderers;
                for (int j = 0; j < renderers.Length; j++)
                {
                    renderers[j].material.SetColor(propertyID, updateInfo.color);
                }
            }
        }
        if (!_objectsWorkInfoDictionary.TryGetValue(updateInfo.data.nameHash, out var value2))
        {
            return;
        }
        for (value = 0; value < value2.Count; value++)
        {
            StageObjectWorkInfo stageObjectWorkInfo = value2[value];
            updateInfo.data.OnUpdateTimelineTransform(stageObjectWorkInfo.cacheTransform, ref updateInfo.updateData, ref stageObjectWorkInfo.startData);
            if (!(stageObjectWorkInfo.renderer == null))
            {
                stageObjectWorkInfo.renderer.enabled = updateInfo.renderEnable;
                stageObjectWorkInfo.cacheGameObject.layer = layer;
                if (updateInfo.colorEnable)
                {
                    stageObjectWorkInfo.renderer.material.SetColor(propertyID, updateInfo.color);
                }
            }
        }
    }

    private void UpdateGazingObject(ref GazingObjectUpdateInfo updateInfo)
    {
        if (!_objectsWorkInfoDictionary.TryGetValue(updateInfo.data.nameHash, out var value) || value.Count == 0)
        {
            return;
        }
        for (int i = 0; i < value.Count; i++)
        {
            StageObjectWorkInfo stageObjectWorkInfo = value[i];
            Vector3 position = stageObjectWorkInfo.cacheTransform.position;
            Quaternion rot = Quaternion.LookRotation((updateInfo.lookAtPos - position).normalized);
            if (updateInfo.data.forwardAxis != LiveTimelineGazingObjectData.Axis.Z)
            {
                updateInfo.data.ApplyFixRotation(ref rot);
            }
            stageObjectWorkInfo.cacheTransform.rotation = rot;
        }
    }

    private void UpdateMobCyalume(ref MobCyalumeUpdateInfo updateInfo)
    {
        if (_mobController == null && _cyalumeController3D == null && _cyalumeController3D.Length != 0)
        {
            return;
        }
        int groupNo = updateInfo.groupNo;
        Matrix4x4 matrix = default(Matrix4x4);
        matrix.SetTRS(updateInfo.translate, Quaternion.Euler(updateInfo.rotate), updateInfo.scale);
        Matrix4x4 matrix2 = _invisibleMobCyalumeMatrix;
        if (_mobController != null)
        {
            if (updateInfo.isVisibleMob)
            {
                _mobController.SetGroupMatrix(groupNo, ref matrix);
            }
            else
            {
                _mobController.SetGroupMatrix(groupNo, ref matrix2);
            }
        }
        if (_cyalumeController3D == null || _cyalumeController3D.Length == 0)
        {
            return;
        }
        if (updateInfo.isVisibleCyalume)
        {
            int i = 0;
            for (int num = _cyalumeController3D.Length; i < num; i++)
            {
                _cyalumeController3D[i].SetGroupMatrix(groupNo, ref matrix);
            }
        }
        else
        {
            int j = 0;
            for (int num2 = _cyalumeController3D.Length; j < num2; j++)
            {
                _cyalumeController3D[j].SetGroupMatrix(groupNo, ref matrix2);
            }
        }
    }

    private void UpdateMobCyalume3D(ref MobCyalume3DUpdateInfo updateInfo)
    {
        if ((bool)_mobCyalume3DController)
        {
            _mobCyalume3DController.UpdateRootParam(updateInfo.rootTranslate, updateInfo.rootRotate, updateInfo.rootScale, updateInfo.isVisibleMob, updateInfo.isVisibleCyalume, _liveTimelineControl.data.mobCyalume3DSettings.isEnableTimeOffset, updateInfo.isEnableMotionMultiSample, updateInfo.motionTimeOffset, updateInfo.motionTimeInterval);
            _mobCyalume3DController.UpdateWaveModeParam(updateInfo.waveMode, updateInfo.waveBasePosition, updateInfo.waveWidth, updateInfo.waveHeight, updateInfo.waveRoughness, updateInfo.waveProgress, updateInfo.waveColorBasePower, updateInfo.waveColorGainPower);
            _mobCyalume3DController.UpdateLightingParam(updateInfo.gradiation, updateInfo.rimlight, updateInfo.blendRange);
            _mobCyalume3DController.UpdateColorParam(updateInfo.paletteScrollSection);
            _mobCyalume3DController.UpdateLookAtModeParam(updateInfo.lookAtMode);
            _mobCyalume3DController.UpdateLookAtPositionParam(updateInfo.lookAtPositionCount, updateInfo.lookAtPositionList);
            _mobCyalume3DController.UpdatePositionParam(updateInfo.positionCount, updateInfo.positionList);
            PostEffectLive3D postEffectLive3D = (_Director ? _Director.GetActivePostEffect() : null);
            if ((bool)postEffectLive3D)
            {
                postEffectLive3D.cyalumeBlurHorizontalOffset = updateInfo.horizontalOffset;
                postEffectLive3D.cyalumeBlurVerticalOffset = updateInfo.verticalOffset;
                postEffectLive3D.cyalumeThreshold = updateInfo.threshold;
                postEffectLive3D.cyalumeGrowPower = updateInfo.growPower;
            }
        }
    }

    public void UpdateStageMob3DImitation(ref MobCyalume3DUpdateInfo updateInfo)
    {
        if (_stageMob3dImitationControllerArray != null)
        {
            for (int i = 0; i < _stageMob3dImitationControllerArray.Length; i++)
            {
                _stageMob3dImitationControllerArray[i].UpdateParams(ref updateInfo);
            }
            PostEffectLive3D postEffectLive3D = (_Director ? _Director.GetActivePostEffect() : null);
            if ((bool)postEffectLive3D)
            {
                postEffectLive3D.cyalumeBlurHorizontalOffset = updateInfo.horizontalOffset;
                postEffectLive3D.cyalumeBlurVerticalOffset = updateInfo.verticalOffset;
                postEffectLive3D.cyalumeThreshold = updateInfo.threshold;
                postEffectLive3D.cyalumeGrowPower = updateInfo.growPower;
            }
        }
    }

    public void SetStartDisableRenderer()
    {
        if (_startDisableRenderers != null)
        {
            for (int i = 0; i < _startDisableRenderers.Count; i++)
            {
                _startDisableRenderers[i].enabled = false;
            }
            _startDisableRenderers.Clear();
        }
    }

    public void SetParticleControllers(ParticleController[] particleControllers)
    {
        if (particleControllers != null)
        {
            for (int i = 0; i < particleControllers.Length; i++)
            {
                AddParticleController(particleControllers[i]);
            }
        }
    }

    public void AddParticleController(ParticleController particleController)
    {
        if (!(particleController == null))
        {
            _particleControllerList.Add(particleController);
            if (particleController.gameObject.isStatic)
            {
                StaticBatchingUtility.Combine(particleController.gameObject);
            }
        }
    }

    public void SetupParticle()
    {
        //セーブの読み込み
        int save = SaveManager.GetInt("Confetti", 1);

        if (save == 1)
        {
            _isEnabledConfetti = true;
        }
        else
        {
            _isEnabledConfetti = false;
        }
        /*
        if (_liveTempData != null && (bool)_liveTempData.isMV)
        {
            if (_qualityType != LocalData.OptionData.eQualityType.Quality3D)
            {
                _isEnabledConfetti = false;
            }
            else
            {
                Director instance = Director.instance;
                if (instance.GetParticleControllType() == LiveTimelineData.ParticleControllMode.Score)
                {
                    _isEnabledConfetti = SingletonMonoBehaviour<LocalData>.instance.option.mv_confetti;
                }
                else if (instance.GetParticleControllType() == LiveTimelineData.ParticleControllMode.Timeline && instance.GetIsUseGameSettingToParticle())
                {
                    _isEnabledConfetti = SingletonMonoBehaviour<LocalData>.instance.option.mv_confetti;
                }
            }
        }
        else if (_qualityType != LocalData.OptionData.eQualityType.Quality3D)
        {
            _isEnabledConfetti = false;
        }
        */

        if (!_isEnabledConfetti)
        {
            SetActiveParticleController(false);
        }
    }

    private void SetActiveParticleController(bool isActive)
    {
        for (int i = 0; i < _particleControllerList.Count; i++)
        {
            _particleControllerList[i].gameObject.SetActive(isActive);
        }
    }

    public void SetProjectorControllers(ProjectorController[] projectorControllers)
    {
        if (projectorControllers != null)
        {
            for (int i = 0; i < projectorControllers.Length; i++)
            {
                AddProjectorController(projectorControllers[i]);
            }
        }
    }

    public void AddProjectorController(ProjectorController projectController)
    {
        if (!(projectController == null))
        {
            _projectorControllerList.Add(projectController);
            if (projectController.gameObject.isStatic)
            {
                StaticBatchingUtility.Combine(projectController.gameObject);
            }
        }
    }

    private bool IsMirrorScaneOn()
    {
        if (_isCheckMirrorScan)
        {
            return _isMirrorScanOn;
        }
        _isCheckMirrorScan = true;
        if (_Director != null && !_Director.IsMirrorScanOn())
        {
            _isMirrorScanOn = false;
            return false;
        }
        /*
        LocalData.OptionData.eQualityType qualityType = _qualityType;
        if ((uint)(qualityType - 3) <= 1u)
        {
            _isMirrorScanOn = true;
            return true;
        }
        _isMirrorScanOn = false;
        return false;
        */
        _isMirrorScanOn = true;
        return true;
    }

    private bool IsSunShaftOn()
    {
        LiveTimelineGamePlaySettings gamePlaySettings = _liveTimelineControl.data.gamePlaySettings;
        if (gamePlaySettings.isSunShaftOff)
        {
            return false;
        }
        return true;
    }

    public void SetupMirrorScanLight()
    {
        if (IsMirrorScaneOn())
        {
            for (int i = 0; i < _projectorControllerList.Count; i++)
            {
                _projectorControllerList[i].projectorMaterials = _mirorrScanLightMaterials;
            }
        }
        else
        {
            for (int j = 0; j < _projectorControllerList.Count; j++)
            {
                _projectorControllerList[j].gameObject.SetActive(value: false);
            }
        }
    }

    public void SetupMirrorScanMaterial()
    {
        if (!_isInitializeMirrorScanLightMaterial)
        {
            if (IsMirrorScaneOn())
            {
                MaterialController.MaterialExtensionData materialExtensionData = new MaterialController.MaterialExtensionData();
                materialExtensionData.Clear();
                materialExtensionData.materialNames = MIRRORSCAN_LIGHT_DUMMYMATERIAL_NAMES;
                materialExtensionData.rootGameObject = base.gameObject;
                materialExtensionData.renderQueueOffsets = new int[1] { 3040 };
                MaterialController.Initialize(ref _mirorrScanLightMaterials, materialExtensionData);
            }
            _isInitializeMirrorScanLightMaterial = true;
        }
    }

    /// <summary>
    /// レーザー光線を更新する
    /// </summary>
    private void UpdateLaser(ref LaserUpdateInfo updateInfo, Dictionary<int, bool> ignoreDic)
    {
        List<LaserController> value = null;
        if (!_laserControllerDictionary.TryGetValue(updateInfo.data.nameHash, out value))
        {
            return;
        }
        int count = value.Count;
        for (int i = 0; i < count; i++)
        {
            value[i].UpdateInfo(ref updateInfo, ignoreDic);
            if (_Director.IsPauseLive())
            {
                value[i].Pause();
            }
            else
            {
                value[i].Resume();
            }
        }
    }

    public void ResetStageObjectsCamera(Camera camera)
    {
        for (int i = 0; i < _stageObjectsInfo.Length; i++)
        {
            if (_stageObjectsInfo[i]._StageObjects != null)
            {
                for (int j = 0; j < _stageObjectsInfo[i]._StageObjects.Length; j++)
                {
                    _stageObjectsInfo[i]._StageObjects[j].ResetCamera(camera);
                }
            }
        }
    }

    /// <summary>
    /// メインカメラをセットする
    /// </summary>
    private void SetMainCamera(LiveTimelineControl timelineControl)
    {
        if (!hasMainCamera)
        {
            int targetCameraIndex = timelineControl.data.GetWorkSheet(0).targetCameraIndex;
            Camera camera = timelineControl.GetCamera(targetCameraIndex).camera;
            if (!(null == camera))
            {
                _mainCamera = camera;
            }
        }
    }

    private void InitSunshaftsControl(LiveTimelineSunshaftsSettings settings)
    {
        PostEffectLive3D component = _mainCamera.gameObject.gameObject.GetComponent<PostEffectLive3D>();
        if (!(null == component))
        {
            Transform transform = base.gameObject.transform.Find(_sunObjectName);
            if (!(null == transform))
            {
                _VolumeLightSunShaftsComponent = component;
                _VolumeLightSunShaftsComponent.sunTransform = transform;
                _VolumeLightSunShaftsComponent.enabled = true;
                _VolumeLightSunShaftsComponent.Init(settings);
                _sunShaftsPower = settings.sunPower;
                _SunShaftsFadeStart = settings.fadeStart;
                _SunShaftsFadeMix = settings.fadeMix;
                _SunShaftsIntensity = settings.intensity;
            }
        }
    }

    private void UpdateSunshaftsControl()
    {
        if (!(_VolumeLightSunShaftsComponent == null) && _VolumeLightSunShaftsComponent.isEnabledSunShaft)
        {
            Vector3 lhs = Vector3.Normalize(_mainCamera.transform.forward);
            Vector3 position = _VolumeLightSunShaftsComponent.sunTransform.position;
            Vector3 position2 = _mainCamera.transform.position;
            Vector3 rhs = Vector3.Normalize(position - position2);
            Vector3 rhs2 = new Vector3(0f, -1f, 0f);
            float f = Vector3.Dot(lhs, rhs);
            float num = Vector3.Dot(lhs, rhs2);
            float num2 = ((num < 0f) ? 0f : (90f - Mathf.Acos(num) / (float)Math.PI * 180f));
            float num3 = Mathf.Acos(f) / (float)Math.PI * 180f;
            num3 += num2 / 2f;
            float num4 = 1f;
            float num5 = 0f;
            num5 = ((num3 < _SunShaftsFadeStart) ? 1f : ((!(num3 < _SunShaftsFadeStart + _SunShaftsFadeMix)) ? 0f : (1f - (num3 - _SunShaftsFadeStart) / _SunShaftsFadeMix)));
            num5 *= num4;
            if (_VolumeLightSunShaftsComponent != null)
            {
                _VolumeLightSunShaftsComponent.sunPower = _sunShaftsPower * _SunShaftsIntensity * num5;
            }
        }
    }

    /// <summary>
    /// サンシャフトのパラメータ設定
    /// </summary>
    private void UpdateVolumeLight(ref VolumeLightUpdateInfo updateInfo)
    {
        if (_VolumeLightSunShaftsComponent != null)
        {
            _sunShaftsPower = updateInfo.power;
            _VolumeLightSunShaftsComponent.sunColor = updateInfo.color1;
            _VolumeLightSunShaftsComponent.sunTransform.localPosition = updateInfo.sunPosition;
            _VolumeLightSunShaftsComponent.komorebiRate = updateInfo.komorebi;
            _VolumeLightSunShaftsComponent.sunShaftBlurRadius = updateInfo.blurRadius;
            _VolumeLightSunShaftsComponent.isEnabledBorderClear = updateInfo.isEnabledBorderClear;
            if (IsSunShaftOn())
            {
                _VolumeLightSunShaftsComponent.isEnabledSunShaft = updateInfo.enable;
            }
            else
            {
                _VolumeLightSunShaftsComponent.isEnabledSunShaft = false;
            }
        }
    }

    /// <summary>
    /// ミラーのパラメータ設定
    /// </summary>
    private void onEnvironemntMirror(ref EnvironmentMirrorUpdateInfo updateInfo)
    {
        if (_mirrorPlaneObjects == null)
        {
            return;
        }
        for (int i = 0; i < _mirrorPlaneObjects.Length; i++)
        {
            if (updateInfo.mirror != _environmentMirror)
            {
                _environmentMirror = updateInfo.mirror;
                _mirrorPlaneObjects[i].gameObject.SetActive(updateInfo.mirror);
            }
            _mirrorPlaneObjects[i].UpdateReflectionRate(updateInfo.mirrorReflectionRate);
        }
    }

    /// <summary>
    /// MoviceControllerを取得する
    /// </summary>
    public UVMovieController GetMoviceController(int idx)
    {
        if (_MovieManager != null && _MovieManager._MovieControllers.Length != 0)
        {
            if (idx >= _MovieManager._MovieControllers.Length)
            {
                idx = 0;
            }
            return _MovieManager._MovieControllers[idx];
        }
        return null;
    }

    /// <summary>
    /// MoviceControllerの設定を行う
    /// </summary>
    public void UpdateMoviceController(UVMovieController ctrl, int playStartOffsetFrame, float fTime, bool bReverse)
    {
        if (ctrl != null)
        {
            ctrl.SetPlayStartSec(playStartOffsetFrame);
            if (_Director == null)
            {
                ctrl.UpdateAddTime(Time.deltaTime, bReverse);
            }
            else
            {
                ctrl.UpdateSetTime(fTime, bReverse);
            }
        }
    }

    private void InitHdrBloomControl(LiveTimelineHdrBloomSettings settings)
    {
    }

    private void UpdateHdrBloom(ref HdrBloomUpdateInfo updateInfo)
    {
    }

    private void _SetupRichLensFlare()
    {
        LensFlare[] componentsInChildren = GetComponentsInChildren<LensFlare>(includeInactive: true);
        if (componentsInChildren == null)
        {
            return;
        }
        LiveTimelineLensFlareSetting lensFlareSetting = _liveTimelineControl.data.lensFlareSetting;
        for (int i = 0; i < lensFlareSetting.flareDataGroup.Length; i++)
        {
            LiveTimelineLensFlareSetting.FlareDataGroup flareDataGroup = lensFlareSetting.flareDataGroup[i];
            if (!flareDataGroup.isHqOnly)
            {
                continue;
            }
            string value = $"fl_flare_{flareDataGroup.objectId:D4}";
            for (int j = 0; j < componentsInChildren.Length; j++)
            {
                if (componentsInChildren[j].flare.name.IndexOf(value, StringComparison.Ordinal) == 0)
                {
                    componentsInChildren[j].enabled = false;
                }
            }
        }
    }

    private void _SetupLensFlareParam()
    {
        if (_lensFlareController != null && (bool)_liveTimelineControl)
        {
            LiveTimelineLensFlareSetting lensFlareSetting = _liveTimelineControl.data.lensFlareSetting;
            for (int i = 0; i < _lensFlareController.Length; i++)
            {
                _lensFlareController[i].SetLimitColorPower(lensFlareSetting.lightStandard, lensFlareSetting.underLimit);
            }
        }
    }

    private void CollectBillboardController()
    {
        _billboardManager = new BillboardManager();
        _billboardManager.Initialize(_washLightMaterialCtrls);
    }

    private void CollectStageGazeController()
    {
        _stageGazeControllerTable = new Dictionary<int, List<StageGazeController>>();
        _stageGazeController = GetComponentsInChildren<StageGazeController>();
        StageGazeController[] stageGazeController = _stageGazeController;
        foreach (StageGazeController stageGazeController2 in stageGazeController)
        {
            if (!_stageGazeControllerTable.TryGetValue(stageGazeController2.GroupNo, out var value))
            {
                value = new List<StageGazeController>(16);
                _stageGazeControllerTable.Add(stageGazeController2.GroupNo, value);
            }
            value.Add(stageGazeController2);
        }
        _isStageGazeController = true;
    }

    public void UpdateStageGazeController(int groupNo, bool isEnable, Vector3 targetPosition)
    {
        if (_stageGazeControllerTable.TryGetValue(groupNo, out var value))
        {
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                value[i].IsEnableLookAt = isEnable;
                value[i].SetGazePosition(targetPosition);
            }
        }
    }

    /// <summary>
    /// レンズフレアのパラメータを更新する
    /// </summary>
    private void UpdateLensFlare(ref LensFlareUpdateInfo updateInfo)
    {
        for (int i = 0; i < _lensFlareController.Length; i++)
        {
            _lensFlareController[i].UpdateInfo(ref updateInfo);
        }
    }

    /// <summary>
    /// Glassの更新を行う
    /// </summary>
    private void UpdateGlass(ref GlassUpdateInfo updateInfo)
    {
        for (int i = 0; i < _glassController.Length; i++)
        {
            _glassController[i].UpdateInfo(ref updateInfo);
        }
    }

    public bool UpdateEnvironemntGlobalLight(ref EnvironmentGlobalLightUpdateInfo updateInfo)
    {
        bool result = false;
        int nameHash = updateInfo.data.nameHash;
        StageObjectRich value = null;
        if (_mapStageObjectRich.TryGetValue(nameHash, out value))
        {
            value.UpdateInfo(ref updateInfo);
            result = true;
        }
        return result;
    }

    private void SetMirrorTag()
    {
        Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
        Dictionary<string, Material> dictionary = new Dictionary<string, Material>(componentsInChildren.Length);
        Renderer[] array = componentsInChildren;
        foreach (Renderer renderer in array)
        {
            if (renderer.sharedMaterial != null)
            {
                dictionary[renderer.sharedMaterial.name] = renderer.sharedMaterial;
            }
        }
        int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.DisplayMirror);
        foreach (Material value2 in dictionary.Values)
        {
            string input = value2.shader.name;
            Match match = _regexCheckRendererMirrorName.Match(input);
            if (match.Success)
            {
                string value = match.Groups[1].Value;
                if (value2.HasProperty(propertyID) && (int)value2.GetFloat(propertyID) == 1)
                {
                    value2.SetOverrideTag("Mirror", _renderReplaceMirrorDic[value]);
                }
            }
        }
    }
}
