using System;
using System.Collections;
using UnityEngine;

namespace Stage.Cyalume
{
    public class CyalumeController3D : MonoBehaviour
    {
        public enum CyalumeShader
        {
            All,
            Random,
            Line,
            Div
        }

        private const string CYALUME_TEXTURE_PATH = "3D/Cyalume/m{0:D03}/tx_stg_cyalume_m{0:D03}_{1:D03}";

        private const string CYALUME_TEXTURE_RICH_PATH = "3D/Cyalume/m{0:D03}/tx_stg_cyalume_m{0:D03}_{1:D03}_hq";

        private const string MOB_TEXTURE_PATH = "3D/Stage/stg_common/Textures/tx_stg_mob_{0:D03}";

        private const string CYALUME_RICH_SHADER = "Cygames/3DLive/Cyalume/CyalumeDefault_hq";

        private const string CYALUME_CONTROL_SHADER = "Cygames/3DLive/Cyalume/CyalumeDefault_control";  

        private const float _AudienceZposMargin = 5f;

        private const float _FloatToIntCastBias = 0.1f;

        private const float _numRandColor = 10f;

        private const float _brightness = 0.8f;

        private static readonly CyalumeShader[] _cyalumeShaderTable = new CyalumeShader[12]
        {
        CyalumeShader.All,
        CyalumeShader.Random,
        CyalumeShader.Random,
        CyalumeShader.Random,
        CyalumeShader.Random,
        CyalumeShader.Line,
        CyalumeShader.Line,
        CyalumeShader.Div,
        CyalumeShader.Div,
        CyalumeShader.Div,
        CyalumeShader.Div,
        CyalumeShader.Div
        };

        private static readonly int[] CYALUME_DIV_TABLE = new int[50]
        {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
        0, 0, 0, 1, 1, 1, 1, 2, 2, 2,
        0, 0, 0, 1, 1, 2, 2, 3, 3, 3,
        0, 0, 1, 1, 2, 2, 3, 3, 4, 4
        };

        private CyalumeControllerCommon _cyalumeControllerCommon = new CyalumeControllerCommon();

        private ChoreographyCyalume.ChoreographyCyalumeData _nowCyalumeData;

        private Renderer[] _renderers;

        private Renderer _renderer;

        private MeshFilter _meshFilter;

        private Color[][] _colorsTable;

        private Material _material;

        private float _ChangedAnimTime;

        [SerializeField]
        private int _animationFrameCount = 16;

        [SerializeField]
        private float _animationSpeed = 1f;

        private float _animationFrame;

        [SerializeField]
        private Texture2D[] _cyalumeTexture;

        public Vector3 _cyalumeOffsetPos;

        public bool _useUVAudienceSpread = true;

        private bool _initialized;

        private bool _readTextureEnd;

        private Director _Director;

        private MobShadowController _mobController;

        [SerializeField]
        private bool _isControlEnabled;

        private Matrix4x4[] _groupMatrices;

        [SerializeField]
        private bool _externalTextureControl;

        public CyalumeControllerCommon cyalumeControllerCommon => _cyalumeControllerCommon;

        public Color[][] colorsTable => _colorsTable;

        public float animationSpeed
        {
            get
            {
                return _animationSpeed;
            }
            set
            {
                _animationSpeed = value;
            }
        }

        public float animationFrame
        {
            get
            {
                return _animationFrame;
            }
            set
            {
                _animationFrame = value;
            }
        }

        public Texture2D[] cyalumeTexture => _cyalumeTexture;

        public bool initialized => _initialized;

        public void SetMobController(MobShadowController mobController)
        {
            _mobController = mobController;
        }
        public void UpdateMaterial()
        {
            if (_groupMatrices != null)
            {
                int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.GroupMatrices);
                _material.SetMatrixArray(propertyID, _groupMatrices);
            }
        }
        public void SetGroupMatrix(int groupIndex, ref Matrix4x4 matrix)
        {
            if (_groupMatrices != null)
            {
                _groupMatrices[groupIndex] = matrix;
            }
        }
        private void OnDestroy()
        {
            if (_material != null)
            {
                UnityEngine.Object.DestroyImmediate(_material);
                _material = null;
            }
        }

        private void Start()
        {
            _cyalumeControllerCommon.Start();
            _renderers = GetComponentsInChildren<Renderer>();
            _Director = Director.instance;
            if (_Director != null)
            {
                _cyalumeControllerCommon.LoadChoreography(_Director.bootDirectLiveID, _Director.bootDirectLiveID);
            }
            CyalumeControllerCommon.ChoreographyInfo nowChoreographyCyalumeData = _cyalumeControllerCommon.getNowChoreographyCyalumeData();
            if (nowChoreographyCyalumeData.choreographyData != null)
            {
                _animationSpeed = nowChoreographyCyalumeData.choreographyData._playSpeed3D;
            }
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = false;
                if (nowChoreographyCyalumeData.choreographyData != null && _cyalumeShaderTable[(int)nowChoreographyCyalumeData.choreographyData._colorPattern] == CyalumeShader.Random)
                {
                    if (_renderers[i].name.Contains("cyalume_r"))
                    {
                        _renderer = _renderers[i];
                    }
                }
                else if (_renderers[i].name.Contains("cyalume_d"))
                {
                    _renderer = _renderers[i];
                }
            }
            if (_renderer != null)
            {
                _renderer.enabled = true;
                if (_externalTextureControl)
                {
                    _material = _renderer.sharedMaterial;
                }
                else
                {
                    _material = UnityEngine.Object.Instantiate(_renderer.sharedMaterial);
                }
                if (_isControlEnabled)
                {
                    if (!_externalTextureControl)
                    {
                        //Shader shader = SingletonMonoBehaviour<ResourcesManager>.instance.LoadObject<Shader>("Cygames/3DLive/Cyalume/CyalumeDefault_control");
                        Shader shader = ResourcesManager.instance.GetShader("CyalumeDefault_control");
                        _material.shader = shader;
                    }
                    //_groupMatrices = LiveUtils.CreateMobCyalumeGroupMatrices();
                    _groupMatrices = CreateMobCyalumeGroupMatrices();
                    int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.GroupMatrices);
                    _material.SetMatrixArray(propertyID, _groupMatrices);
                }
                else
                {
                    //Shader shader2 = SingletonMonoBehaviour<ResourcesManager>.instance.LoadObject<Shader>("Cygames/3DLive/Cyalume/CyalumeDefault_hq");
                    Shader shader2 = ResourcesManager.instance.GetShader("CyalumeDefault_hq");
                    _material.shader = shader2;
                }

                if (!_externalTextureControl)
                {
                    _renderer.sharedMaterial = _material;
                }
            }
            StartCoroutine(Initialize());
        }

        public static Matrix4x4[] CreateMobCyalumeGroupMatrices()
        {
            Matrix4x4[] array = new Matrix4x4[11];
            int i = 0;
            for (int num = 11; i < num; i++)
            {
                array[i] = Matrix4x4.identity;
            }
            return array;
        }

        private void UpdateCyalume(bool isForceUpdate)
        {
            if (_cyalumeControllerCommon.choreographyCyalume == null || !_initialized)
            {
                return;
            }
            CyalumeControllerCommon.ChoreographyInfo nowChoreographyCyalumeData = _cyalumeControllerCommon.getNowChoreographyCyalumeData();
            if (nowChoreographyCyalumeData.choreographyData == null || (!isForceUpdate && _nowCyalumeData == nowChoreographyCyalumeData.choreographyData))
            {
                return;
            }
            if (nowChoreographyCyalumeData.choreographyData.IsTypeChoreography)
            {
                if (_material != null && !_externalTextureControl)
                {
                    if (_cyalumeControllerCommon.choreographyCyalume.colorDataType == ChoreographyCyalume.ColorDataType.Texture)
                    {
                        _material.mainTexture = _cyalumeTexture[nowChoreographyCyalumeData.choreographyData._patternID];
                    }
                    else
                    {
                        _material.mainTexture = _cyalumeTexture[nowChoreographyCyalumeData.choreographyData._patternID];
                        _meshFilter.mesh.colors = _colorsTable[nowChoreographyCyalumeData.choreographyData._patternID];
                    }
                }
                _ChangedAnimTime = nowChoreographyCyalumeData.choreographyData._startTime;
            }
            if (nowChoreographyCyalumeData.choreographyData._playSpeed3D != -1f)
            {
                _animationSpeed = nowChoreographyCyalumeData.choreographyData._playSpeed3D;
            }
            _nowCyalumeData = nowChoreographyCyalumeData.choreographyData;
        }

        private int CalcAnimationFrameNo(float _time, float _speed = 0f, int _startImage = 0)
        {
            float num = _time * 60f / 40f;
            if (_speed == 0f)
            {
                _speed = _animationSpeed;
            }
            int num2 = (int)(num * _speed * (float)_animationFrameCount) % _animationFrameCount + _startImage;
            if (num2 < 0)
            {
                num2 = 0;
            }
            if (num2 > _animationFrameCount)
            {
                num2 -= _animationFrameCount;
            }
            return num2;
        }

        private void UpdateAnimation()
        {
            if (_animationFrameCount <= 0 || _material == null || _externalTextureControl)
            {
                return;
            }
            int num = 0;
            CyalumeControllerCommon.ChoreographyInfo nowChoreographyCyalumeData = _cyalumeControllerCommon.getNowChoreographyCyalumeData();
            if (nowChoreographyCyalumeData.choreographyData == null)
            {
                return;
            }
            if (nowChoreographyCyalumeData.choreographyData.IsTypeChoreography)
            {
                num = CalcAnimationFrameNo(_cyalumeControllerCommon.choreographyTime - _ChangedAnimTime, 0f, nowChoreographyCyalumeData.choreographyData._startFrame);
            }
            else if (nowChoreographyCyalumeData.choreographyData._choreographyType == ChoreographyCyalume.ChoreographyType.Stop)
            {
                num = 0;
            }
            else if (nowChoreographyCyalumeData.choreographyData._choreographyType == ChoreographyCyalume.ChoreographyType.Pause && nowChoreographyCyalumeData.choreographyData._dataNo > 0)
            {
                CyalumeControllerCommon.ChoreographyInfo choreographyCyalumeDataFromNo = _cyalumeControllerCommon.getChoreographyCyalumeDataFromNo(nowChoreographyCyalumeData.choreographyData._dataNo - 1);
                if (choreographyCyalumeDataFromNo.choreographyData != null)
                {
                    num = CalcAnimationFrameNo(nowChoreographyCyalumeData.choreographyData._startTime - choreographyCyalumeDataFromNo.choreographyData._startTime, choreographyCyalumeDataFromNo.choreographyData._playSpeed3D, nowChoreographyCyalumeData.choreographyData._startFrame);
                }
            }
            Vector2 mainTextureOffset = default(Vector2);
            mainTextureOffset.y = (float)num / (float)_animationFrameCount;
            mainTextureOffset.x = 0f;
            _material.mainTextureOffset = mainTextureOffset;
        }

        private void Update()
        {
            _cyalumeControllerCommon.Update();
            if (Director.instance != null)
            {
                _cyalumeControllerCommon.choreographyTime = Director.instance.musicScoreTime;
            }
            UpdateCyalume(isForceUpdate: false);
            UpdateAnimation();
            HideCyalume();
        }

        private void HideCyalume()
        {
            //アプリの設定で観客オフにした場合は消す
            /*
            if (_renderers != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].enabled = false;
                }
            }
            */
        }

        private string MakeCyalumeTexturePath(int musicId, int index)
        {
            return string.Format("3D/Cyalume/m{0:D03}/tx_stg_cyalume_m{0:D03}_{1:D03}_hq", musicId, index);
        }

        private IEnumerator LoadCyalumeTexture()
        {
            ResourcesManager resourcesManager = SingletonMonoBehaviour<ResourcesManager>.instance;
            ChoreographyCyalume.ChoreographyCyalumePattern[] choreographyPatterns = _cyalumeControllerCommon.choreographyCyalume.choreographyPatterns;
            _cyalumeTexture = new Texture2D[choreographyPatterns.Length];
            for (int j = 0; j < _cyalumeTexture.Length; j++)
            {
                _cyalumeTexture[j] = Texture2D.blackTexture;
            }
            for (int i = 0; i < choreographyPatterns.Length; i++)
            {
                string objectName = MakeCyalumeTexturePath(_cyalumeControllerCommon.musicId, i + 1);
                Texture2D texture2D = resourcesManager.LoadObject(objectName) as Texture2D;
                if (!(texture2D == null))
                {
                    _cyalumeTexture[i] = texture2D;
                    _cyalumeTexture[i].wrapMode = TextureWrapMode.Repeat;
                    yield return 0;
                }
            }
            _readTextureEnd = true;
            yield return 0;
        }

        private IEnumerator Initialize()
        {
            CyalumeControllerCommon.ChoreographyInfo nowChoreographyCyalumeData = _cyalumeControllerCommon.getNowChoreographyCyalumeData();
            ChoreographyCyalume.ChoreographyCyalumePattern[] choreographyPatterns = _cyalumeControllerCommon.choreographyCyalume.choreographyPatterns;
            if (nowChoreographyCyalumeData.choreographyData != null && (_cyalumeShaderTable[(int)nowChoreographyCyalumeData.choreographyData._colorPattern] == CyalumeShader.Line || _cyalumeShaderTable[(int)nowChoreographyCyalumeData.choreographyData._colorPattern] == CyalumeShader.Div || _cyalumeShaderTable[(int)nowChoreographyCyalumeData.choreographyData._colorPattern] == CyalumeShader.All) && _renderer != null)
            {
                _meshFilter = _renderer.GetComponentInChildren<MeshFilter>();
                Mesh mesh = _meshFilter.mesh;
                Color[] colors = mesh.colors;
                if ((bool)mesh)
                {
                    int num = Math.Min(choreographyPatterns.Length, 10);
                    _colorsTable = new Color[num][];
                    for (int i = 0; i < _colorsTable.Length; i++)
                    {
                        _colorsTable[i] = new Color[mesh.colors.Length];
                        int index = ((_cyalumeShaderTable[(int)nowChoreographyCyalumeData.choreographyData._colorPattern] != CyalumeShader.Div) ? 1 : 0);
                        Color color;
                        if (_cyalumeShaderTable[(int)nowChoreographyCyalumeData.choreographyData._colorPattern] == CyalumeShader.All)
                        {
                            for (int j = 0; j < _colorsTable[i].Length; j++)
                            {
                                color = (choreographyPatterns[i]._colorData[0]._inColor + choreographyPatterns[i]._colorData[0]._outColor) * 0.8f;
                                color.a = colors[j].a;
                                _colorsTable[i][j] = color;
                            }
                            continue;
                        }
                        for (int j = 0; j < _colorsTable[i].Length; j++)
                        {
                            int num2 = (int)(mesh.colors[j][index] * 10f + 0.1f) - 1;
                            num2 = CYALUME_DIV_TABLE[(choreographyPatterns[i]._colorCount - 1) * 10 + num2];
                            color = (choreographyPatterns[i]._colorData[num2]._inColor + choreographyPatterns[i]._colorData[num2]._outColor) * 0.8f;
                            color.a = colors[j].a;
                            _colorsTable[i][j] = color;
                        }
                    }
                }
            }
            yield return 0;
            _readTextureEnd = false;
            if (!_externalTextureControl)
            {
                StartCoroutine(LoadCyalumeTexture());
            }
            while (!_readTextureEnd)
            {
                yield return 0;
            }
            if (_useUVAudienceSpread)
            {
                MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
                for (int k = 0; k < componentsInChildren.Length; k++)
                {
                    Mesh mesh2 = componentsInChildren[k].mesh;
                    if (!mesh2 || !mesh2.name.Contains("cyalume_r"))
                    {
                        continue;
                    }
                    Vector2[] array = new Vector2[mesh2.uv.Length];
                    float num3 = 0f;
                    float num4 = 0f;
                    for (int l = 0; l < mesh2.uv.Length; l++)
                    {
                        if (mesh2.vertices[l].z >= num3 + 5f || mesh2.vertices[l].z <= num3 - 5f)
                        {
                            num4 = (float)UnityEngine.Random.Range(0, 9) * 0.1f;
                            num3 = mesh2.vertices[l].z;
                        }
                        array[l] = mesh2.uv[l];
                        array[l].x += num4;
                    }
                    mesh2.uv = array;
                }
            }
            base.transform.position = base.transform.position + _cyalumeOffsetPos;
            if (null != _mobController)
            {
                _mobController.SetMobColor(_cyalumeControllerCommon.GetMobColor());
                _mobController.SetOffsetPot(_cyalumeOffsetPos);
            }
            _initialized = true;
            yield return 0;
        }
    }
}
