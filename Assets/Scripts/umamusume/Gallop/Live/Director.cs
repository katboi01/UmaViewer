using System;
using Gallop.ImageEffect;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Gallop.Live.Cutt;
using CriWare;
using UnityEngine.SceneManagement;

namespace Gallop.Live
{
    public class Director : MonoBehaviour
    {
        private class OutLineOffLengthItem
        {
            public Camera _camera;
            public float _outlineOffLength;
        }

        private enum LoadAssetTask
        {
            Start = 0,
            TimelineEnd = 1,
            CharacterEnd = 2,
            StageEnd = 3,
            Finish = 4
        }

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
            Max = 8
        }

        public enum DisplayMode
        {
            None = 0,
            Landscape = 1,
            Portrait = 2
        }

        private enum State
        {
            None = 0,
            LiveInit_Start = 1,
            LiveInit = 2,
            LiveStart_Start = 3,
            LiveStart = 4,
            LivePlay_Start = 5,
            LivePlay = 6
        }

        private enum CameraIndex
        {
            Motion = 0,
            Timeline1 = 1,
            Timeline2 = 2,
            Timeline3 = 3
        }

        public enum IndirectLightShaftsTextureType
        {
            ShaftA = 0,
            ShaftB = 1,
            Mask = 2,
            ShaftC = 3
        }

        public class SpotLightEffectMaterialData
        {
            public Material AdditionMaterial;
            public Material MultiplyMaterial;
        }

        public class Live3DSettings
        {
            public int MusicId { get; set; }
            public string CuttPath { get; set; }
            public string BgPath { get; set; }
            public float DelayTime { get; set; }
            public string CameraPath { get; set; }
            //public Director.Live3DSettings.UvMovieData[] UvMovieDataArray { get; set; }
            public string[] MirrorScanLightTexturePathArray { get; set; }
            public int CharacterDataCount { get; set; }
            //public CharacterObject.CharacterData[] CharacterDataArray { get; }
            public bool IsEnabled { get; set; }
            public bool IsOP { get; set; }
            public bool IsED { get; set; }
            public bool IsSkipSkit { get; set; }
            public bool IsVoiceOP { get; set; }
            public bool IsVoiceED { get; set; }
            public bool IsLyricsOP { get; set; }
            public bool IsLyricsED { get; set; }
            public bool IsCyalumeOP { get; set; }
            public bool IsCyalumeED { get; set; }
            //public Director.Live3DSettings.AudienceAnimationData[] AudienceAnimationDataArray { get; set; }
            public Dictionary<int, string> ProjectorTextureDictionary { get; set; }
            public int ExtraResourceId { get; set; }
            public int MusicResourceId { get; set; }
        }

        public class LiveLoadSettings
        {
            public class CharacterInfo
            {
                public int CharaId { get; set; }
                public int MobId { get; set; }
                public int[] DressIdArray { get; set; }
                public int[] DressColorIdArray { get; set; }
                public int VocalCharaId { get; set; }
                public bool IsMob { get; }
            }

            public class RaceInfo
            {
                public int RaceInstanceId { get; set; }
                public RaceDefine.Weather Weather { get; set; }
                public string[] CharacterNameArray { get; set; }
                public string[] TrainerNameArray { get; set; }
                public string[] CharacterNameArrayForChampionsText { get; set; }
                public string[] TrainerNameArrayForChampionsText { get; set; }
                public RaceDefine.RaceType RaceType { get; set; }
                public int DateYear { get; set; }
                public int ChampionsMeetingResourceId { get; set; }
                public Texture GoalCaptureTexture { get; set; }
            }

            public int MusicId { get; set; }
            public List<Director.LiveLoadSettings.CharacterInfo> CharacterInfoList { get; }
            public Director.LiveLoadSettings.RaceInfo raceInfo { get; }
            public bool IsSkipSkit { get; set; }
            public Dictionary<int, int> StageObjectSelectTargetDictionary { get; set; }
        }



        private const float LiveStartFadeInTime = 1;
        private const string TransparentCameraName = "TransparentCamera";
        private const int TransparentCameraDepth = 15;
        private const string MonitorCameraPrefabPath = "Prefabs/Live/MonitorCamera";
        private const float LiveTotalTimeInit = 180;
        private const float LiveTotalTimeMargin = 0;
        private const float MaxOutlineOffLength = 99999;
        private const int FINALIZE_CAMERA_DEPTH = 19;
        private static Director _instance = null; 
        [SerializeField] 
        private Director.State _state; 
        private bool _isDestroyed;
        public LiveTimelineControl _liveTimelineControl; //Edited to public
        private bool _isFinished; 
        private readonly RandomTable<float> _randomTable; 
        private RandomRatioTable _blinkLightRandomRatioTable; 
        private float _sceneFrameRate; 
        [SerializeField] 
        public float _liveCurrentTime;  //Edited to public
        private bool _isRequestFadeOut; 
        public bool _isLiveSetup; //Edit to pulic
        [SerializeField] 
        private bool _isEnableBloom; 
        private bool _isBloom; 
        private bool _isDof; 
        public StageController _stageController; //Edited to public
        private bool _isEnabledStageController; 
        public RenderTexture[] MonitorTextureArray; 
        [SerializeField] 
        private Transform _globalLightTransform; 
        [SerializeField] 
        private GameObject[] _cameraNodes; 
        private Camera[] _cameraObjects; 
        private CameraEventCallback[] _cameraEventCallbackArray; 
        private GameObject _transparentCameraObject; 
        private Camera _transparentCamera; 
        private Transform _transparentCameraTransform; 
        private MultiCameraFinalComposite[] _multiCameraFinalCompositeArray; 
        private MultiCameraComposite[] _multiCameraCompositionArray; 
        private MultiCamera _multiCamera; 
        [SerializeField] 
        private CameraLookAt _cameraLookAt; 
        [SerializeField] 
        private Animator _cameraAnimator; 
        private RuntimeAnimatorController _cameraRuntimeAnimator; 
        private GameObject[] _monitorCameraObjectArray; 
        private MonitorCamera[] _monitorCameraArray; 
        [SerializeField] 
        private HandShakeCamera _handShakeCamera; 
        private int _activeCameraIndex; 
        private readonly int kMotionCameraIndex; 
        private readonly int[] kTimelineCameraIndices; 
        //private GallopFrameBuffer _frameBuffer; 
        //private LowResolutionCameraFrameBuffer[] _lowResolutionCameraArray; 
        private Camera _mainCameraObject; 
        private Transform _mainCameraTransform; 
        public Action OnChangeOriantationLandscapeCallback; 
        public Action OnChangeOriantationPortraitCallback; 
        private readonly List<CharacterObject> _characterObjectList; 
        private List<CharaEffectInfo> _listShadowEffectInfo; 
        private List<CharaEffectInfo> _listSpotLightEffectInfo;  
        private ModelController.ModelDepthInfo[] _depthOrderInfoArray; 
        private readonly List<ModelController> _modelControllerList; 
        private bool _isUpdateModelRenderQueue; 
        //private PropsManager _propsManager; 
        private SongPart _songPart; 
        //public LiveChampionsTextController ChampionsTextController; 
        public LiveTitleController TitleController; 
        private LiveFadeController _fadeController; 
        //private readonly List<LiveImageEffect> _imageEffectLive3dList; 
        //private IndirectLightShaftsParam[] _indirectLightShaftsParamArray; 
        private Texture2D[] _indirectLightShaftsTexture; 
        private ColorCorrectionParam _colorCorrectionParameter; 
        [SerializeField] 
        private float _outlineFovMin; 
        [SerializeField] 
        private float _outlineFovMinOffLength; 
        [SerializeField] 
        private float _outlineFovLessOffLength; 
        [SerializeField] 
        private float _outlineFovMax; 
        [SerializeField] 
        private float _outlineFovMaxOffLength; 
        [SerializeField] 
        private float _outlineFovGreaterOffLength; 
        private List<Director.OutLineOffLengthItem> _outlineOffLengthItems; 
        private const int VariationCallOn = 1;
        private const int VariationCallOff = 2;
        private const int CharaSongVariation = 1;
        private const int MOB_DEFAULT_DRESS_ID = 7;
        private const int PROPS_BLINKLIGHT_RENDERQUEUE_START = 100;
        private const string LIGHT_UVMOVIE_FILE_NAME = "gal_UVmovie_{0}_Light";
        private const string LIVE_IMAGE_SUFFIX_CHAMPIONS_LOGO = "champions_logo";
        private const string LIVE_IMAGE_SUFFIX_CHAMPIONS_JACKET = "champions_jacket"; 
        private Director.LoadAssetTask _loadAssetTask; 
        private bool _isClothInitialized; 
        private int _charaClothResetTimer; 
        private GameObject _initializeCamera; 
        private Transform _initializeCameraTransform; 
        private float _clothInitTimer;  
        //private List<PropsManager.PropsData> _propsDataList; 
        private int _usualMultiTrainedCharaId; 
        private bool _isInitializedMultiCamera; 
        private bool _isInitialized; 
        private static GameObject _cuttPrefab; 
        private static bool _usingAssetBundleCuttPrefab; 
        private static TextAsset _cyalumeChoreographyCsv; 
        private static TextAsset _songPartCsv; 
        private static TextAsset[] _uvMovieJsonArray; 
        private bool _enableTimelineGlobalRimParam; 
        private Vector3 _tempRateVector; 
        private Vector3 _tempAttachRateVector;

        public static Director instance => _instance;
        public static bool HasInstance { get; }
        public LiveTimelineControl LiveTimelineControl { get; }
        public Director.DisplayMode displayMode { get; set; }
        public bool IsStarted { get; }
        public bool IsFinished { get; set; }
        public static bool IsLiveCall { get; set; }
        public static bool IsDisplayCyalume { get; set; }
        public static bool IsDisplayAudience { get; set; }
        public RandomTable<float> randomTable { get; }
        public RandomRatioTable BlinkLightRandomRatioTable { get; }
        public float SceneFrameRate { get; }
        public LiveTimeController LiveTimeController { get; }
        public float LiveCurrentTime { get; }
        public float LiveTotalTime { get; }
        public bool isRequestFadeOut { get; }
        public bool DoStartLive { get; set; }
        public int screenOriginWidth { get; }
        public int screenOriginHeight { get; }
        public StageController StageController { get; }
        public bool IsEnabledStageController { get; }
        public Transform GlobalLightTransform { get; set; }
        public Camera[] CameraObjects { get; }
        public Animator CameraAnimator { get; }
        public HandShakeCamera HandShakeCamera { get; }
        //public GallopFrameBuffer FrameBuffer { get; }
        public Camera MainCameraObject { get; }
        public Transform MainCameraTransform { get; }
        public List<CharacterObject> CharacterObjectList { get; }
        public List<CharaEffectInfo> listShadowEffectInfo { get; }
        public List<CharaEffectInfo> listSpotLightEffectInfo { get; }
        public List<Director.SpotLightEffectMaterialData> SpotLightEffectMaterialDataList { get; set; }
        public PropsManager PropsManager { get; }
        public SongPart SongPart { get; }
        public LiveFadeController FadeController { get; }
        public LiveTimelineDefine.SheetIndex SheetType { get; }
        public bool IsTimelineControlled { get; }
        public static Director.LiveLoadSettings LoadSettings { get; }
        public static Director.Live3DSettings LiveSettings { get; }
        public static List<string> SongCueSheetPathList { get; set; }
        public int UsualMultiTrainedCharaId { get; set; }
        public bool IsInitialized { get; }
        public static RegisterInfo[] IndirectLightShaftsTextureRegisterInfoArray { get; set; }
        public static RegisterInfo[] CharacterRegisterInfoArray { get; set; }
        public static RegisterInfo[] CommonRegisterInfoArray { get; set; }
        public static RegisterInfo[] EffectRegisterInfoArray { get; set; }
        public static Dictionary<string, List<string>> FlashSeCueNameDict { get; set; }

        //real work start
        public LiveEntry live;
        private const string CUTT_PATH = "cutt/cutt_son{0}/cutt_son{0}";
        private const string STAGE_PATH = "3d/env/live/live{0}/pfb_env_live{0}_controller000";
        private const string SONG_PATH = "sound/l/{0}/snd_bgm_live_{0}_oke_01";
        private const string VOCAL_PATH = "sound/l/{0}/snd_bgm_live_{0}_chara_{1}_01";
        private const string RANDOM_VOCAL_PATH = "sound/l/{0}/snd_bgm_live_{0}_chara";
        private const string LIVE_PART_PATH = "live/musicscores/m{0}/m{0}_part";

        private UmaViewerBuilder Builder => UmaViewerBuilder.Instance;

        public List<Transform> charaObjs;

        public List<Animation> charaAnims;

        public List<LiveTimelineCharaMotSeqData> seqData;

        public bool _isLiveScene;

        public AudioSource liveSong;

        public List<CriAtomSource> liveVocal;
        public CriAtomSource liveMusic;

        public PartEntry partInfo;

        public bool _syncTime = false;
        public bool _soloMode = false;

        public int characterCount = 0;
        public int allowCount = 0;

        public int liveMode = 0;

        private void Awake()
        {
            if (live != null)
            {
                _instance = this;
                Debug.Log(string.Format(CUTT_PATH, live.MusicId));
                Debug.Log(live.BackGroundId);
                Builder.LoadAssetPath(string.Format(CUTT_PATH, live.MusicId), this.gameObject.transform);
                //Builder.LoadAssetPath(string.Format(STAGE_PATH, live.BackGroundId), this.gameObject.transform);

                //Make CharacterObject
                
                var characterStandPos = this._liveTimelineControl.transform.Find("CharacterStandPos");
                int counter = 0;
                foreach (Transform trans in characterStandPos.GetComponentsInChildren<Transform>()) {
                    if (trans == characterStandPos)
                    {
                        continue;
                    }
                    var newObj = Instantiate(trans, this.transform);
                    newObj.gameObject.name = string.Format("CharacterObject{0}", counter);
                    charaObjs.Add(newObj.transform);
                    counter++;
                };

                //Get live parts info
                UmaDatabaseEntry partAsset = UmaViewerMain.Instance.AbList.FirstOrDefault(a => a.Name.Contains(string.Format(LIVE_PART_PATH, live.MusicId)));

                Debug.Log(partAsset.Name);

                AssetBundle bundle = UmaViewerBuilder.LoadOrGet(partAsset);
                TextAsset partData = bundle.LoadAsset<TextAsset>($"m{live.MusicId}_part");
                partInfo = new PartEntry(partData.text);

            }
        }

        public void InitializeTimeline(List<LiveCharacterSelect> characters, int mode)
        {
            liveMode = mode;

            allowCount = characters.Count;

            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].CharaEntry.Name != "")
                {
                    characterCount += 1;
                }
            }
            if (characterCount == 1)
            {
                _soloMode = true;
            }

            _liveTimelineControl.InitCharaMotionSequence(_liveTimelineControl.data.characterSettings.motionSequenceIndices);

            _liveTimelineControl.OnUpdateLipSync += delegate (LiveTimelineKeyLipSyncData keyData_, float liveTime_)
            {
                for (int k = 0; k < charaObjs.Count; k++)
                {
                    var container = charaObjs[k].GetComponentInChildren<UmaContainer>();
                    if(container != null)
                    {
                        container.FaceDrivenKeyTarget.AlterUpdateAutoLip(keyData_, liveTime_, ((int)keyData_.character >> k) % 2);
                    }
                }
            };
            _liveTimelineControl.OnUpdateFacial += delegate (FacialDataUpdateInfo updateInfo_, float liveTime_, int position)
            {
                if( position < charaObjs.Count)
                {
                    var container = charaObjs[position].GetComponentInChildren<UmaContainer>();
                    container.FaceDrivenKeyTarget.AlterUpdateFacialNew(ref updateInfo_, liveTime_);
                }
            };
        }

        public void Play(int songid, List<LiveCharacterSelect> characters)
        {
            Debug.Log(songid);

            if (!_soloMode)
            {
                for (int i = 0; i < characters.Count; i++)
                {
                    if (characters[i].CharaEntry.Name != "" && i < 3)
                    {
                        var charaid = characters[i].CharaEntry.Id;

                        var entry = UmaViewerMain.Instance.AbSounds.FirstOrDefault(a => a.Name.Contains(string.Format(VOCAL_PATH, songid, charaid)) && a.Name.EndsWith("awb"));
                        if (entry == null)
                        {
                            List<UmaDatabaseEntry> entries = new List<UmaDatabaseEntry>();
                            foreach (var random in UmaViewerMain.Instance.AbSounds.Where(a => (a.Name.Contains(string.Format(RANDOM_VOCAL_PATH, songid)) && a.Name.EndsWith("awb"))))
                            {
                                entries.Add(random);
                            }
                            if (entries.Count > 0)
                            {
                                entry = entries[UnityEngine.Random.Range(0, entries.Count - 1)];
                            }
                        }

                        if (entry != null)
                        {
                            Debug.Log(entry.Name);
                            liveVocal.Add(UmaViewerAudio.ApplyCueSheet(entry.Name.Split('.')[0]));
                        }
                    }
                }


                liveMusic = UmaViewerAudio.ApplyCueSheet(string.Format(SONG_PATH, songid));

                foreach (var vocal in liveVocal)
                {
                    vocal.Play(0);
                }
                liveMusic.Play(0);
            }
            else
            {
                var entry = UmaViewerMain.Instance.AbSounds.FirstOrDefault(a => a.Name.Contains(string.Format(VOCAL_PATH, songid, characters[0].CharaEntry.Id)) && a.Name.EndsWith("awb"));
                if (entry == null)
                {
                    List<UmaDatabaseEntry> entries = new List<UmaDatabaseEntry>();
                    foreach (var random in UmaViewerMain.Instance.AbSounds.Where(a => (a.Name.Contains(string.Format(RANDOM_VOCAL_PATH, songid)) && a.Name.EndsWith("awb"))))
                    {
                        entries.Add(random);
                    }
                    if (entries.Count > 0)
                    {
                        entry = entries[UnityEngine.Random.Range(0, entries.Count - 1)];
                    }
                }
                if (entry != null)
                {
                    Debug.Log(entry.Name);
                }

                UmaViewerBuilder.Instance.LoadLiveSound(songid, entry, false);
                liveSong = GameObject.Find("SoundController").GetComponent<AudioSource>();
            }

            _isLiveSetup = true;
            _liveCurrentTime = 0;
        }

        private void OnTimelineUpdate(float _liveCurrentTime)
        {
            _liveTimelineControl.AlterUpdate(_liveCurrentTime);
            if (!_soloMode)
            {
                UmaViewerAudio.AlterUpdate(_liveCurrentTime, partInfo, liveVocal);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Version2");
                AssetBundle.UnloadAllAssetBundles(true);
            }

            if (_isLiveSetup)
            {
                if(_syncTime == false)
                {
                    if (!_soloMode)
                    {
                        if(liveMusic.time > 0)
                        {
                            _liveCurrentTime = liveMusic.time / 1000;
                            _syncTime = true;
                        }
                    }
                    else
                    {
                        if(liveSong.time > 0)
                        {
                            _liveCurrentTime = liveSong.time;
                            _syncTime = true;
                        }
                    }
                }
                else
                {
                    _liveCurrentTime += Time.deltaTime;
                    //Debug.Log(_liveCurrentTime * 60);
                    OnTimelineUpdate(_liveCurrentTime);
                }
            }
        }
    }

}