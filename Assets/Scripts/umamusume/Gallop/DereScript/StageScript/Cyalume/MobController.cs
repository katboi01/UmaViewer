using Cutt;
using System.Collections.Generic;
using UnityEngine;

namespace Stage.Cyalume
{
    public class MobController : MonoBehaviour
    {
        public const string BUNDLE_NAME_RESOURCE = "3d_cyalume_3d_son{0:D4}.unity3d";

        public const string BUNDLE_NAME_MOTION = "3d_cyalume_3d_anim_cyalume3d_son{0:D4}_01.unity3d";

        private const string MOB_PATH = "3D/Cyalume3D/son{0:D3}/";

        private const string MOB_PREFAB_DIR = "3D/Cyalume3D/son{0:D3}/Prefab/";

        private const string MOB_MOTION_NAME_SINGLE = "3D/Cyalume3D/Motions/anim_cyalume3d_son{0:D4}_01";

        private const string MOB_MOTION_NAME_SINGLE_DEFAULT = "3D/Cyalume3DBase/Motions/anim_cyalume3d_default";

        private const string MOB_PREFAB_NAME = "3D/Cyalume3D/son{0:D3}/Prefab/pf_stg_cyalume3d_son{0:D3}";

        private const string MOB_ROOT_NAME = "MobRoot";

        private const string ANIM_NODE_NAME_FORMAT = "Anim_{0}";

        private const int ANIM_TRANSFORM_INDEX_SINGLE = 0;

        private const int ANIM_TRANSFORM_INDEX_MULTI = 1;

        private const float MOTION_TIME_INTERVAL_SCALE = 0.0166666675f;

        private const float MOTION_DELAY_BASE_MULTIPLE = 3.5f;

        private const string WRIST_ROOT_NAME = "Anim_0";

        private const string WRIST_ONE_NAME_FORMAT = "{0}_One";

        public const float COLOR_LINE_OFFSET = 0.5f;

        public const float COLOR_LINE_MULTI = 60f;

        private const int PartsNum = 9;

        private MobParts[] _mobParts;

        private float _motionTime;

        private bool _isPrevEnableMotionMultiSample;

        private bool _isEnableMotionTimeOffset;

        private bool _isEnableMotionMultiSample;

        private float _motionTimeOffset;

        private float _motionTimeInterval = 1f;

        private Director _director;

        private GameObject _rootObj;

        private Animation[] _animation = new Animation[1];

        private string[] _clipName = new string[1];

        private const int LOOK_AT_POSITION_NUM = 11;

        private Vector4[] _lookAtPosition = new Vector4[LOOK_AT_POSITION_NUM];

        private const int POSITION_NUM = 11;

        private Vector4[] _position = new Vector4[POSITION_NUM];

        private Vector2 _colorPalette = Vector2.zero;

        private Vector3 _cameraArrow = Vector3.zero;

        private bool _isVisibleMob = true;

        private bool _isVisibleCyalume = true;

        private bool _isInitialize;

        private string _assetBundleNameResource = string.Empty;

        private string _assetBundleNameMotion = string.Empty;

        private Transform _wristLTrans;

        private Transform _wristLOneTrans;

        private Transform _wristRTrans;

        private Transform _wristROneTrans;

        public bool IsInitialize => _isInitialize;

        public static void RegisterDownload(List<string> downloadList)
        {
            int mobCyalume3DResourceID = ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DResourceID;
            int mobCyalume3DMotionID = ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DMotionID;

            string item = string.Format(BUNDLE_NAME_RESOURCE, mobCyalume3DResourceID);
            string item2 = string.Format(BUNDLE_NAME_MOTION, mobCyalume3DMotionID);

            downloadList.Add(item);
            downloadList.Add(item2);
        }

        private void OnDestroy()
        {
            _animation = null;
            _director = null;
            if (_rootObj != null)
            {
                UnityEngine.Object.Destroy(_rootObj);
                _rootObj = null;
            }
            _isInitialize = false;
        }

        private void LateUpdate()
        {
            if (!_isInitialize)
            {
                return;
            }
            if ((bool)_director)
            {
                _cameraArrow = _director.mainCamera.transform.forward;
            }
            bool flag = _isEnableMotionMultiSample || _isPrevEnableMotionMultiSample;
            bool flag2 = !_isEnableMotionMultiSample && _isPrevEnableMotionMultiSample;
            MobParts[] mobParts;
            if (flag)
            {
                float num = (flag2 ? _motionTime : (_motionTime + _motionTimeOffset + _motionTimeInterval * MOTION_DELAY_BASE_MULTIPLE));
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 1; j++)
                    {
                        AnimationState animationState = _animation[j][_clipName[j]];
                        animationState.blendMode = AnimationBlendMode.Blend;
                        animationState.wrapMode = WrapMode.Loop;
                        animationState.time = num - _motionTimeInterval * i;
                        animationState.enabled = true;
                        _animation[j].Sample();
                        animationState.enabled = false;
                    }
                    PrepareSingleAndMultiMotion();
                    mobParts = _mobParts;
                    for (int k = 0; k < mobParts.Length; k++)
                    {
                        mobParts[k].SetMotionSample(i);
                    }
                }
            }
            else
            {
                float time = (_isEnableMotionTimeOffset ? (_motionTime + _motionTimeOffset + 0.0583333373f) : _motionTime);
                for (int l = 0; l < 1; l++)
                {
                    AnimationState animationState2 = _animation[l][_clipName[l]];
                    animationState2.blendMode = AnimationBlendMode.Blend;
                    animationState2.wrapMode = WrapMode.Loop;
                    animationState2.time = time;
                    animationState2.enabled = true;
                    _animation[l].Sample();
                    animationState2.enabled = false;
                }
                PrepareSingleAndMultiMotion();
            }
            mobParts = _mobParts;
            for (int k = 0; k < mobParts.Length; k++)
            {
                mobParts[k].AlterLateUpdate(_lookAtPosition, _position, _colorPalette, _cameraArrow, !flag);
            }
            _isPrevEnableMotionMultiSample = _isEnableMotionMultiSample;
        }

        private void Update()
        {
            if (_isInitialize && _director != null)
            {
                _motionTime = _director.musicScoreTime;
            }
        }

        private void PrepareSingleAndMultiMotion()
        {
            Vector3 localPosition = _wristLTrans.localPosition;
            Quaternion localRotation = _wristLTrans.localRotation;
            Vector3 localPosition2 = _wristLOneTrans.localPosition;
            Quaternion localRotation2 = _wristLOneTrans.localRotation;
            Vector3 localPosition3 = _wristRTrans.localPosition;
            Quaternion localRotation3 = _wristRTrans.localRotation;
            Vector3 localPosition4 = _wristROneTrans.localPosition;
            Quaternion localRotation4 = _wristROneTrans.localRotation;
            MobParts obj = _mobParts[3];
            Transform obj2 = obj._animationTransform[ANIM_TRANSFORM_INDEX_SINGLE];
            obj2.localPosition = localPosition2;
            obj2.localRotation = localRotation2;
            Transform obj3 = obj._animationTransform[ANIM_TRANSFORM_INDEX_MULTI];
            obj3.localPosition = localPosition;
            obj3.localRotation = localRotation;
            MobParts obj4 = _mobParts[7];
            Transform obj5 = obj4._animationTransform[ANIM_TRANSFORM_INDEX_SINGLE];
            obj5.localPosition = localPosition4;
            obj5.localRotation = localRotation4;
            Transform obj6 = obj4._animationTransform[ANIM_TRANSFORM_INDEX_MULTI];
            obj6.localPosition = localPosition3;
            obj6.localRotation = localRotation3;
        }

        private T LoadResource<T>(string assetPath) where T : Object
        {
            ResourcesManager.instance.LoadAsset(_assetBundleNameResource, null);

            return ResourcesManager.instance.LoadObject<Object>(assetPath) as T;
        }

        private T LoadMotion<T>(string assetPath) where T : Object
        {
            ResourcesManager.instance.LoadAsset(_assetBundleNameMotion, null);

            return ResourcesManager.instance.LoadObject<Object>(assetPath) as T;
        }

        private Shader LoadShader(string assetPath)
        {
            return Shader.Find(assetPath);
        }

        private void SetupPartsContext(MobParts.Context context, MobParts.Part part)
        {
            string text = part.ToString();
            Renderer[] componentsInChildren = _rootObj.GetComponentsInChildren<Renderer>();
            List<GameObject> list = new List<GameObject>();
            Renderer[] array = componentsInChildren;
            foreach (Renderer renderer in array)
            {
                if (renderer.name.Contains(text))
                {
                    list.Add(renderer.gameObject);
                }
            }
            context.PartArray = list.ToArray();
            context.AnimationTransform = new Transform[2];
            for (int j = 0; j < 2; j++)
            {
                string text2 = text;
                if (text2 == MobParts.Part.Cyalume_L.ToString())
                {
                    text2 = MobParts.Part.Wrist_L.ToString();
                }
                if (text2 == MobParts.Part.Cyalume_R.ToString())
                {
                    text2 = MobParts.Part.Wrist_R.ToString();
                }
                int num = ((text2 == MobParts.Part.Wrist_L.ToString() || text2 == MobParts.Part.Wrist_R.ToString()) ? j : 0);
                context.AnimationTransform[j] = GameObjectUtility.FindChild(text2, _rootObj.transform.Find(string.Format(ANIM_NODE_NAME_FORMAT, num)));
            }
        }

        private void GetMobMeshFilter(List<MeshFilter> meshFilterList, GameObject mobObj)
        {
            MeshRenderer[] componentsInChildren = mobObj.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in componentsInChildren)
            {
                meshFilterList.Add(meshRenderer.gameObject.GetComponent<MeshFilter>());
            }
        }

        private void MakeChildTransform(GameObject mobObj)
        {
            Transform parent = base.gameObject.transform;
            int childCount = mobObj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                new GameObject(mobObj.transform.GetChild(i).name).transform.SetParent(parent, worldPositionStays: false);
            }
        }

        private void OnSetup()
        {
            _isInitialize = false;
            int mobCyalume3DResourceID = 0;
            int mobCyalume3DMotionID = 0;
            if (Director.instance == null)
            {
                return;
            }
            mobCyalume3DResourceID = ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DResourceID;
            mobCyalume3DMotionID = ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DMotionID;

            _assetBundleNameResource = string.Format(BUNDLE_NAME_RESOURCE, mobCyalume3DResourceID);
            _assetBundleNameMotion = string.Format(BUNDLE_NAME_MOTION, mobCyalume3DMotionID);

            GameObject original = LoadResource<GameObject>(string.Format(MOB_PREFAB_NAME, mobCyalume3DResourceID));
            _rootObj = Object.Instantiate(original);
            _rootObj.transform.SetParent(base.transform, worldPositionStays: false);
            _director = Director.instance;
            _mobParts = new MobParts[PartsNum];
            for (int i = 0; i < 1; i++)
            {
                AnimationClip animationClip = LoadMotion<AnimationClip>(string.Format(MOB_MOTION_NAME_SINGLE, mobCyalume3DMotionID));
                if (animationClip == null)
                {
                    animationClip = LoadMotion<AnimationClip>(MOB_MOTION_NAME_SINGLE_DEFAULT);
                }
                _clipName[i] = animationClip.name;
                _animation[i] = _rootObj.transform.Find(string.Format(ANIM_NODE_NAME_FORMAT, i)).gameObject.AddComponent<Animation>();
                _animation[i].playAutomatically = false;
                _animation[i].AddClip(animationClip, _clipName[i]);
                _animation[i].Stop();
            }
            _wristLTrans = _rootObj.transform.Find(WRIST_ROOT_NAME).Find(MobParts.Part.Wrist_L.ToString());
            _wristLOneTrans = _rootObj.transform.Find(WRIST_ROOT_NAME).Find(string.Format(WRIST_ONE_NAME_FORMAT, MobParts.Part.Wrist_L.ToString()));
            _wristRTrans = _rootObj.transform.Find(WRIST_ROOT_NAME).Find(MobParts.Part.Wrist_R.ToString());
            _wristROneTrans = _rootObj.transform.Find(WRIST_ROOT_NAME).Find(string.Format(WRIST_ONE_NAME_FORMAT, MobParts.Part.Wrist_R.ToString()));
            MobParts.Context context = new MobParts.Context();
            context.RootObj = _rootObj;
            for (int j = 0; j < PartsNum; j++)
            {
                _mobParts[j] = new MobParts();
                MobParts.Part part = (MobParts.Part)j;
                SetupPartsContext(context, part);
                _mobParts[j].Setup(context);
            }
            for (int k = 0; k < 1; k++)
            {
                _animation[k].Play(_clipName[k]);
                _animation[k][_clipName[k]].enabled = false;
            }
            _isInitialize = true;
        }

        public void Setup()
        {
            OnSetup();
        }

        public static MobController CreateMobController(Transform parent)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(parent, worldPositionStays: false);
            return obj.AddComponent<MobController>();
        }

        public void UpdateRootParam(Vector3 rootTranslate, Vector3 rootRotate, Vector3 rootScale, bool isVisibleMob, bool isVisibleCyalume, bool isEnableTimeOffset, bool isEnableMultiSample, float motionTimeOffset, float motionTimeInterval)
        {
            base.transform.localPosition = rootTranslate;
            base.transform.localEulerAngles = rootRotate;
            base.transform.localScale = rootScale;
            if (isVisibleMob != _isVisibleMob)
            {
                _isVisibleMob = isVisibleMob;
                _mobParts[0].SetVisible(_isVisibleMob);
                _mobParts[1].SetVisible(_isVisibleMob);
                _mobParts[2].SetVisible(_isVisibleMob);
                _mobParts[3].SetVisible(_isVisibleMob);
                _mobParts[5].SetVisible(_isVisibleMob);
                _mobParts[6].SetVisible(_isVisibleMob);
                _mobParts[7].SetVisible(_isVisibleMob);
            }
            if (isVisibleCyalume != _isVisibleCyalume)
            {
                _isVisibleCyalume = isVisibleCyalume;
                _mobParts[4].SetVisible(_isVisibleCyalume);
                _mobParts[8].SetVisible(_isVisibleCyalume);
            }
            _isEnableMotionTimeOffset = isEnableTimeOffset;
            _isEnableMotionMultiSample = isEnableTimeOffset && isEnableMultiSample;
            _motionTimeOffset = motionTimeOffset * MOTION_TIME_INTERVAL_SCALE;
            _motionTimeInterval = motionTimeInterval * MOTION_TIME_INTERVAL_SCALE;
        }

        public void UpdateLightingParam(float gradiation, float rimlight, float blendRange)
        {
            for (int i = 0; i < PartsNum; i++)
            {
                MobParts obj = _mobParts[i];
                obj.gradiation = gradiation;
                obj.rimlight = rimlight;
                obj.blendRange = blendRange;
            }
        }

        public void UpdateColorParam(float paletteScrollSection)
        {
            float num = (paletteScrollSection + 0.5f) * 60f / 1024f;
            _colorPalette = new Vector2(num - (int)num, ((int)num * 256 + 0.5f) / 1024f);
        }

        public void UpdateLookAtModeParam(LiveMobCyalume3DLookAtMode lookAtMode)
        {
            for (int i = 0; i < PartsNum; i++)
            {
                _mobParts[i].SetMaterialLookAtMode(lookAtMode);
            }
        }

        public void UpdateLookAtPositionParam(int lookAtPositionCount, Vector3[] lookAtPositionList)
        {
            if (lookAtPositionCount != 0)
            {
                for (int i = 0; i < LOOK_AT_POSITION_NUM; i++)
                {
                    _lookAtPosition[i] = lookAtPositionList[i % lookAtPositionCount];
                }
            }
        }

        public void UpdatePositionParam(int positionCount, Vector3[] positionList)
        {
            if (positionCount != 0)
            {
                for (int i = 0; i < POSITION_NUM; i++)
                {
                    _position[i] = positionList[i % positionCount];
                }
            }
        }

        public void UpdateWaveModeParam(LiveMobCyalume3DWaveMode waveMode, Vector3 waveBasePosition, float waveWidth, float waveHeight, float waveRoughness, float waveProgress, float waveColorBasePower, float waveColorGainPower)
        {
            for (int i = 0; i < 9; i++)
            {
                _mobParts[i].SetMaterialWaveModeParam(waveMode, waveBasePosition, waveWidth, waveHeight, waveRoughness, waveProgress, waveColorBasePower, waveColorGainPower);
            }
        }
    }
}
