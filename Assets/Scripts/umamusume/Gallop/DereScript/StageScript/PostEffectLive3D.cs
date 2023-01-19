using System;
using Cute;
using Cutt;
using Stage;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Cygames/Live3D")]
public class PostEffectLive3D : MyPostEffectsBase
{
    public enum eProp
    {
        offsets,
        _bloomDofWeight,
        _CurveParams,
        _InvRenderTargetSize,
        _TapLowBackground,
        _TapMedium,
        _Bloom,
        _FrustumCornersWS,
        _CameraWS,
        _Y,
        _FogColor,
        _FogTex,
        _MaskTex,
        _fogScroll,
        _RgbTex,
        _RgbDepthTex,
        _Saturation,
        _Parameter,
        _MainTex,
        _PixelSize,
        _ColorParam,
        _PostFilmPower,
        _PostFilmOffsetParam,
        _PostFilmOptionParam,
        _PostFilmColor0,
        _PostFilmColor1,
        _PostFilmColor2,
        _PostFilmColor3,
        _PostFilmAspect,
        _PostFilmAspectInv,
        _TapHigh,
        _TapLowForeground,
        _dofForegroundSize,
        _texMovie,
        _texMovieMask,
        _movieScale,
        _movieOffset,
        _colorBlendFactor,
        _BloomHighRange,
        _bloomHighRangeIntensity,
        _FilterTex,
        _filterScale,
        _filterOffset,
        _filterIntensity,
        _Composite
    }

    public enum DofResolution
    {
        High = 2,
        Medium,
        Low
    }

    private enum DofFocalType
    {
        Transform,
        Position,
        Point
    }

    public enum FilterType
    {
        NoEffect,
        DofBloom,
        DiffusionDofBloom
    }

    public enum RuntimeFilterType
    {
        NoEffect,
        Bloom,
        DofBloom,
        DiffusionBloom,
        DiffusionDofBloom
    }

    public class RuntimePostEffectInfomation
    {
        public bool _debug;

        public RuntimeFilterType _runtimeFilterType;

        public bool _runtimeEnableDof;

        public bool _runtimeEnableSunShafts;

        public bool _runtimeEnableRichBloom;

        public bool _runtimeEnableGlobalFog;

        public bool _runtimeEnableIndirectLightShafts;

        public bool _runtimeEnableColorCorrection;

        public bool _runtimeEnableTiltShift;

        public bool IsNeedPostEffect
        {
            get
            {
                if (_runtimeFilterType != 0)
                {
                    return true;
                }
                if (_runtimeEnableDof)
                {
                    return true;
                }
                if (_runtimeEnableSunShafts)
                {
                    return true;
                }
                if (_runtimeEnableRichBloom)
                {
                    return true;
                }
                if (_runtimeEnableGlobalFog)
                {
                    return true;
                }
                if (_runtimeEnableIndirectLightShafts)
                {
                    return true;
                }
                if (_runtimeEnableColorCorrection)
                {
                    return true;
                }
                if (_runtimeEnableTiltShift)
                {
                    return true;
                }
                return false;
            }
        }
    }

    [Serializable]
    public class ColorCorrection
    {
        public enum ColorCorrectionMode
        {
            Simple,
            Advanced
        }

        public bool enabled;

        private ColorCorrectionCurvesParameter _parameter;

        public ColorCorrectionCurvesParameter parameter
        {
            get
            {
                return _parameter;
            }
            set
            {
                _parameter = value;
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            bool useDepthCorrection = parameter.useDepthCorrection;
            bool selectiveCc = parameter.selectiveCc;
            Material ccMaterial = parameter.ccMaterial;
            Material ccDepthMaterial = parameter.ccDepthMaterial;
            Material selectiveCcMaterial = parameter.selectiveCcMaterial;
            Texture2D rgbChannelTex = parameter.rgbChannelTex;
            Texture2D rgbDepthChannelTex = parameter.rgbDepthChannelTex;
            Texture2D zCurveTex = parameter.zCurveTex;
            float saturation = parameter.saturation;
            RenderTexture renderTexture = destination;
            if (selectiveCc)
            {
                renderTexture = RenderTexture.GetTemporary(source.width, source.height);
            }
            if (useDepthCorrection)
            {
                ccDepthMaterial.SetTexture("_RgbTex", rgbChannelTex);
                ccDepthMaterial.SetTexture("_ZCurve", zCurveTex);
                ccDepthMaterial.SetTexture("_RgbDepthTex", rgbDepthChannelTex);
                ccDepthMaterial.SetFloat("_Saturation", saturation);
                Graphics.Blit(source, renderTexture, ccDepthMaterial);
            }
            else
            {
                ccMaterial.SetTexture("_RgbTex", rgbChannelTex);
                ccMaterial.SetFloat("_Saturation", saturation);
                Graphics.Blit(source, renderTexture, ccMaterial);
            }
            if (selectiveCc)
            {
                selectiveCcMaterial.SetColor("selColor", parameter.selectiveFromColor);
                selectiveCcMaterial.SetColor("targetColor", parameter.selectiveToColor);
                Graphics.Blit(renderTexture, destination, selectiveCcMaterial);
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }
    }

    [Serializable]
    public class ColorCorrectionCurvesParameter
    {
        public static readonly int TextureWidth = 256;

        public static readonly int TextureHeight = 4;

        public Color[] rgbChannel;

        public bool useDepthCorrection;

        public AnimationCurve depthRedChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public AnimationCurve depthGreenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public AnimationCurve depthBlueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public Material ccMaterial;

        public Material ccDepthMaterial;

        public Material selectiveCcMaterial;

        public Texture2D rgbChannelTex;

        public Texture2D rgbDepthChannelTex;

        public Texture2D zCurveTex;

        public float saturation = 1f;

        public bool selectiveCc;

        public Color selectiveFromColor = Color.white;

        public Color selectiveToColor = Color.white;

        public ColorCorrection.ColorCorrectionMode mode;

        public bool updateTextures = true;

        public Shader colorCorrectionCurvesShader;

        public Shader simpleColorCorrectionCurvesShader;

        public Shader colorCorrectionSelectiveShader;

        public bool isSupported;

        public void Initialize()
        {
            isSupported = true;
            mode = ColorCorrection.ColorCorrectionMode.Simple;

            //シェーダの設定
            simpleColorCorrectionCurvesShader = ResourcesManager.instance.GetShader("ColorCorrectionCurvesSimple");
            colorCorrectionCurvesShader = ResourcesManager.instance.GetShader("ColorCorrectionCurves");
            colorCorrectionSelectiveShader = ResourcesManager.instance.GetShader("ColorCorrectionSelective");
            ccMaterial = CheckShaderAndCreateMaterial(simpleColorCorrectionCurvesShader, ccMaterial);
            ccDepthMaterial = CheckShaderAndCreateMaterial(colorCorrectionCurvesShader, ccDepthMaterial);
            selectiveCcMaterial = CheckShaderAndCreateMaterial(colorCorrectionSelectiveShader, selectiveCcMaterial);
            rgbChannel = new Color[TextureWidth * TextureHeight];
            if (!rgbChannelTex)
            {
                rgbChannelTex = new Texture2D(TextureWidth, TextureHeight, TextureFormat.ARGB32, mipChain: false, linear: true);
            }
            if (!rgbDepthChannelTex)
            {
                rgbDepthChannelTex = new Texture2D(TextureWidth, TextureHeight, TextureFormat.ARGB32, mipChain: false, linear: true);
            }
            if (!zCurveTex)
            {
                zCurveTex = new Texture2D(TextureWidth, 1, TextureFormat.ARGB32, mipChain: false, linear: true);
            }
            rgbChannelTex.hideFlags = HideFlags.DontSave;
            rgbDepthChannelTex.hideFlags = HideFlags.DontSave;
            zCurveTex.hideFlags = HideFlags.DontSave;
            rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
            rgbDepthChannelTex.wrapMode = TextureWrapMode.Clamp;
            zCurveTex.wrapMode = TextureWrapMode.Clamp;
            AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            for (float num = 0f; num <= 1f; num += (1.0f / 255.0f))
            {
                float num2 = Mathf.Clamp(animationCurve.Evaluate(num), 0f, 1f);
                zCurveTex.SetPixel((int)Mathf.Floor(num * 255f), 0, new Color(num2, num2, num2));
            }
            zCurveTex.Apply();
        }

        public void Destroy()
        {
            if (rgbChannelTex != null)
            {
                UnityEngine.Object.DestroyImmediate(rgbChannelTex);
                rgbChannelTex = null;
            }
            if (rgbDepthChannelTex != null)
            {
                UnityEngine.Object.DestroyImmediate(rgbDepthChannelTex);
                rgbDepthChannelTex = null;
            }
            if (zCurveTex != null)
            {
                UnityEngine.Object.DestroyImmediate(zCurveTex);
                zCurveTex = null;
            }
            rgbChannel = null;
            if (ccMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(ccMaterial);
                ccMaterial = null;
            }
            if (ccDepthMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(ccDepthMaterial);
                ccDepthMaterial = null;
            }
            if (selectiveCcMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(selectiveCcMaterial);
                selectiveCcMaterial = null;
            }
        }

        public void UpdateParameters(AnimationCurve redCurve, AnimationCurve greenCurve, AnimationCurve blueCurve)
        {
            if (rgbChannel != null)
            {
                for (float num = 0f; num <= 1f; num += 0.003921569f)
                {
                    float num2 = Mathf.Clamp(redCurve.Evaluate(num), 0f, 1f);
                    float num3 = Mathf.Clamp(greenCurve.Evaluate(num), 0f, 1f);
                    float num4 = Mathf.Clamp(blueCurve.Evaluate(num), 0f, 1f);
                    int num5 = (int)Mathf.Floor(num * 255f);
                    rgbChannel[num5] = new Color(num2, num2, num2);
                    rgbChannel[num5 + TextureWidth] = new Color(num3, num3, num3);
                    rgbChannel[num5 + TextureWidth * 2] = new Color(num4, num4, num4);
                }
                rgbChannelTex.SetPixels(rgbChannel);
                rgbChannelTex.Apply();
            }
        }

        protected Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                isSupported = false;
                return null;
            }
            if (s.isSupported && (bool)m2Create && m2Create.shader == s)
            {
                return m2Create;
            }
            if (!s.isSupported)
            {
                isSupported = false;
                return null;
            }
            m2Create = new Material(s);
            m2Create.hideFlags = HideFlags.DontSave;
            if ((bool)m2Create)
            {
                return m2Create;
            }
            return null;
        }
    }


    [Serializable]
    public class IndirectLightShafts
    {
        public bool enabled;

        public Texture2D[] _shuftTexture;

        public Texture2D _maskTexture;

        public Vector4 speed = new Vector4(-60f, -30f, -120f, -30f);

        public Vector4 offset;

        public Vector4 angle = new Vector4(0f, 0f, 0.5f, 0.75f);

        [Range(0.01f, 10f)]
        public float scale = 1f;

        public Vector4 alpha = new Vector4(1f, 1f, 1f, 1f);

        public Vector4 alpha2 = new Vector4(1f, 1f, 1f, 1f);

        public Vector4 maskAlpha = new Vector4(1f, 1f, 1f, 1f);

        public LiveTimelineIndirectLightShuftSettings.ShaftType shaftType;

        public void SetTexture(Texture2D[] shuftTexture, Texture2D maskTexture)
        {
            _shuftTexture = shuftTexture;
            _maskTexture = maskTexture;
        }
    }


    [Serializable]
    public class ScreenOverlay
    {
        public readonly string[] arrShdKey_Mode = new string[11]
        {
            "MODE_NONE",
            "MODE_LERP",
            "MODE_ADD",
            "MODE_MUL",
            "MODE_VIGNETTE_LERP",
            "MODE_VIGNETTE_ADD",
            "MODE_VIGNETTE_MUL",
            "MODE_MONOCHROME",
            "MODE_NOISE",
            "MODE_WATERDROP",
            "MODE_FISHEYE"
        };

        public readonly string[] arrShdKey_Blend = new string[5]
        {
            "COLOR_ONLY",
            "BLEND_NONE",
            "BLEND_LERP",
            "BLEND_ADD",
            "BLEND_MUL"
        };

        public readonly string strShdKey_AlphaMasking = "ALPHA_MASKING";

        public readonly string strShdKey_InverseVignette = "INVERSE_VIGNETTE";

        public const string strShdKey_ScreenCircle = "SCREEN_CIRCLE";

        public PostFilmMode postFilmMode;

        public float postFilmPower;

        public Vector2 postFilmOffsetParam = Vector4.zero;

        public Vector4 postFilmOptionParam = Vector4.zero;

        public Color postFilmColor0 = Color.black;

        public Color postFilmColor1 = Color.black;

        public Color postFilmColor2 = Color.black;

        public Color postFilmColor3 = Color.black;

        public bool inverseVignette;

        public bool screenCircle;

        public Vector2 screenCircleDir = Vector2.one;

        public LiveTimelineKeyPostFilmData.eLayerMode layerMode;

        public LiveTimelineKeyPostFilmData.eColorBlend colorBlend;

        public int movieResId;

        private bool _existMovieMask;

        private Texture _movieTexture;

        private Texture _movieMaskTexture;

        private Vector2 _movieTextureScale = Vector2.one;

        private Vector2 _movieTextureOffset = Vector2.zero;

        public float colorBlendFactor;

        public bool existMovieMask => _existMovieMask;

        public Texture movieTexture => _movieTexture;

        public Texture movieMaskTexture => _movieMaskTexture;

        public Vector2 movieTextureScale => _movieTextureScale;

        public Vector2 movieTextureOffset => _movieTextureOffset;

        public void SetMovieInfo(Texture texMovie, Texture texMask, Vector2 scale, Vector2 offset)
        {
            _movieTexture = texMovie;
            _movieMaskTexture = texMask;
            _movieTextureScale = scale;
            _movieTextureOffset = offset;
            _existMovieMask = ((!(texMask == null)) ? true : false);
        }

        private void SetShaderKeyword(int id, string[] _arrKeywords, Material mtrl)
        {
            for (int i = 0; i < _arrKeywords.Length; i++)
            {
                if (id != i)
                {
                    mtrl.DisableKeyword(_arrKeywords[i]);
                }
            }
            if (0 < id && id < _arrKeywords.Length)
            {
                mtrl.EnableKeyword(_arrKeywords[id]);
            }
        }

        public void Update(PropertyID<eProp> propID, Material mtrl)
        {
            if (mtrl == null)
            {
                return;
            }
            SetShaderKeyword((int)postFilmMode, arrShdKey_Mode, mtrl);
            if (inverseVignette)
            {
                mtrl.EnableKeyword(strShdKey_InverseVignette);
            }
            else
            {
                mtrl.DisableKeyword(strShdKey_InverseVignette);
            }
            if (screenCircle)
            {
                mtrl.EnableKeyword(strShdKey_ScreenCircle);
                Vector4 value = default(Vector4);
                value.x = screenCircleDir.x;
                value.y = screenCircleDir.y;
                value.z = 0f;
                value.w = 0f;
                mtrl.SetVector("_ScreenCircleParam", value);
            }
            else
            {
                mtrl.DisableKeyword(strShdKey_ScreenCircle);
            }
            switch (layerMode)
            {
                case LiveTimelineKeyPostFilmData.eLayerMode.Color:
                    SetShaderKeyword((int)layerMode, arrShdKey_Blend, mtrl);
                    break;
                case LiveTimelineKeyPostFilmData.eLayerMode.UVMovie:
                    SetShaderKeyword((int)layerMode + (int)colorBlend, arrShdKey_Blend, mtrl);
                    if (movieTexture != null)
                    {
                        mtrl.SetTexture(propID[(int)eProp._texMovie], movieTexture);
                    }
                    break;
            }
            if (existMovieMask)
            {
                mtrl.EnableKeyword(strShdKey_AlphaMasking);
                if (movieMaskTexture != null)
                {
                    mtrl.SetTexture(propID[(int)eProp._texMovieMask], movieMaskTexture);
                }
            }
            else
            {
                mtrl.DisableKeyword(strShdKey_AlphaMasking);
            }
            mtrl.SetVector(propID[(int)eProp._movieScale], movieTextureScale);
            mtrl.SetVector(propID[(int)eProp._movieOffset], movieTextureOffset);
            mtrl.SetVector(propID[(int)eProp._movieOffset], movieTextureOffset);
            mtrl.SetFloat(propID[(int)eProp._colorBlendFactor], colorBlendFactor);
        }

        public bool isValid()
        {
            bool result = true;
            switch (postFilmMode)
            {
                case PostFilmMode.Monochrome:
                    result = ((postFilmColor0.a != 0f) ? true : false);
                    break;
                case PostFilmMode.None:
                    result = false;
                    break;
                default:
                    result = ((postFilmPower != 0f) ? true : false);
                    break;
                case PostFilmMode.Mul:
                case PostFilmMode.VignetteLerp:
                case PostFilmMode.VignetteMul:
                    break;
            }
            return result;
        }
    }

    public enum DofQualityMask
    {
        Low = 0,
        Normal = 3,
        High = 7
    }

    public enum DofQuality
    {
        OnlyBackground = 1,
        BackgroundAndForeground = 5
    }

    public enum DofBlurType
    {
        Horizon,
        Mixed,
        Disc
    }

    public enum eDofMVFilterType
    {
        NONE,
        TYPE_A,
        TYPE_B,
        TYPE_C
    }

    public enum eRenderTexture
    {
        Source,
        LowRes,
        Downsample,
        Bloom,
        BloomHighRange,
        MAX
    }

    public enum HDRBloomMode
    {
        Auto,
        On,
        Off
    }

    public enum BloomScreenBlendMode
    {
        Screen,
        Add
    }

    private struct BloomParameters
    {
        public float sepBlurSpread;

        public float bloomIntensity;

        public int bloomBlurIterations;

        public RenderTexture secondQuarterRezColor;

        public int pass;

        public Material blurAndFlaresMaterial;

        public Material brightPassFilterMaterial;
    }

    public enum SunShaftsResolution
    {
        Low,
        Normal,
        High
    }

    public enum ShaftsScreenBlendMode
    {
        Screen,
        Add
    }

    [Serializable]
    public class TiltShift
    {
        public bool unlinkCutt;

        public LiveTimelineKeyTiltShiftData.Mode mode;

        public LiveTimelineKeyTiltShiftData.Quality quality = LiveTimelineKeyTiltShiftData.Quality.Normal;

        [Range(0f, 15f)]
        public float blurArea = 1f;

        [Range(0f, 25f)]
        public float maxBlurSize = 5f;

        [Range(0f, 2f)]
        public int downsample;

        public Vector2 offset;

        [Range(-180f, 180f)]
        public float roll;

        public Vector4 blurParam;

        public Vector4 param;

        public Shader shader;

        private Material _mtrl;

        private bool _isEnable = true;

        public Material mtrl
        {
            get
            {
                return _mtrl;
            }
            set
            {
                _mtrl = value;
            }
        }

        public bool isEnable
        {
            get
            {
                return (mode != LiveTimelineKeyTiltShiftData.Mode.None) & _isEnable;
            }
            set
            {
                _isEnable = value;
            }
        }

        public void Destroy()
        {
            if (_mtrl != null)
            {
                UnityEngine.Object.DestroyImmediate(_mtrl);
                _mtrl = null;
            }
        }
    }

    private const int PASS_WEIGHTED_BLUR = 0;

    private const int PASS_FASTBLOOM_DOWNSAMPLE = 1;

    private const int PASS_FASTBLOOM_VERTICALBLUR = 2;

    private const int PASS_FASTBLOOM_HORIZONTALBLUR = 3;

    private const int PASS_POSTBLIT_DIMMERONLY = 0;

    private const int PASS_POSTBLIT_OVERLAY1 = 1;

    private const int PASS_POSTBLIT_OVERLAY1_INVERSE = 2;

    private const int PASS_POSTBLIT_OVERLAY2 = 3;

    private const int PASS_POSTBLIT_OVERLAY2_INVERSE = 4;

    private const int PASS_POSTBLOOM_BLOOM = 0;

    private const int PASS_POSTBLOOM_OVERLAY1 = 1;

    private const int PASS_POSTBLOOM_OVERLAY1_INVERSE = 2;

    private const int PASS_POSTBLOOM_OVERLAY2 = 3;

    private const int PASS_POSTBLOOM_OVERLAY2_INVERSE = 4;

    private const int PASS_POSTDOFBLOOM_APPLYBG = 0;

    private const int PASS_POSTDOFBLOOM_APPLYBGDEBUG = 1;

    private const int PASS_POSTDOFBLOOM_COC3ALPHA = 2;

    private const int PASS_POSTDOFBLOOM_DOWNSAMPLE = 3;

    private const int PASS_POSTDOFBLOOM_DOFBLOOM = 4;

    private const int PASS_POSTDOFBLOOM_FOGBLOOM = 5;

    private const int PASS_POSTDOFBLOOM_BLOOMCOLOR = 6;

    private const int PASS_POSTDOFBLOOM_COCBG_RICH = 7;

    private const int PASS_POSTDOFBLOOM_COCBGFG = 8;

    private const int PASS_POSTDOFBLOOM_OVERLAY1 = 9;

    private const int PASS_POSTDOFBLOOM_OVERLAY1_INVERSE = 10;

    private const int PASS_POSTDOFBLOOM_OVERLAY2 = 11;

    private const int PASS_POSTDOFBLOOM_OVERLAY2_INVERSE = 12;

    private const int PASS_POSTDOFBLOOM_COCFG = 13;

    private const int PASS_POSTDIFFUSIONBLOOM_VERTICALGAUSS = 0;

    private const int PASS_POSTDIFFUSIONBLOOM_HORIZONGAUSS = 1;

    private const int PASS_POSTDIFFUSIONBLOOM_BLOOM = 2;

    private const int PASS_POSTDIFFUSIONBLOOM_OVERLAY1 = 3;

    private const int PASS_POSTDIFFUSIONBLOOM_OVERLAY1_INVERSE = 4;

    private const int PASS_POSTDIFFUSIONBLOOM_OVERLAY2 = 5;

    private const int PASS_POSTDIFFUSIONBLOOM_OVERLAY2_INVERSE = 6;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_APPLYBG = 0;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_APPLYBGDEBUG = 1;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_COC2ALPHA = 2;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_DOWNSAMPLE = 3;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_FOGBLOOM = 4;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_BLOOMCOLOR = 5;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_VERTICALGAUSS = 6;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_HORIZONGAUSS = 7;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_BLOOM = 8;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_COCBG_RICH = 9;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_COCBGFG = 10;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_OVERLAY1 = 11;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_OVERLAY1_INVERSE = 12;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_OVERLAY2 = 13;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_OVERLAY2_INVERSE = 14;

    private const int PASS_POSTDIFFUSIONDOFBLOOM_COCFG = 15;

    private const int PASS_CYALUME3DBLOOM_DOWNSAMPLE = 0;

    private const int PASS_CYALUME3DBLOOM_VERTICAL = 1;

    private const int PASS_CYALUME3DBLOOM_HORIZONTAL = 2;

    private const int PASS_CYALUME3DBLOOM_COMPOSITE = 3;

    private const int PASS_CYALUME3DBLOOM_FINAL = 4;

    private const int BLOOM_DIVIDER = 4;

    private const float BLOOM_WIDTHMOD = 0.5f;

    private const float FILM_ASPECT_MAX = 1.77777779f;

    private const float FILM_ASPECT_MAX_INV = 0.5625f;

    private PropertyID<eProp> _propID;

    private CacheCamera _cacheCamera;

    private bool _bUsableDepthTexture;

    private float _dofFocalStartCurve = 2f;

    private float _dofFocalEndCurve = 2f;

    private float _dofFocalDistance01 = 0.1f;

    private float _dofWidthOverHeight = 1.25f;

    private float _dofOneOverBaseSize = 0.001953125f;

    private Vector4 _dofInvRenderTargetSize = Vector4.zero;

    private Vector4 _dofCurveParams = Vector4.zero;

    private Vector4 _bloomSizeParameter = Vector4.zero;

    private Vector4 _bloomBlurParameter = Vector4.zero;

    private Vector4 _dofOffsetsParam = Vector4.zero;

    private Vector4 _DiffusionPixelSize = Vector4.zero;

    private Vector4 _DiffusionColorParam = Vector4.one;

    private RenderTexture _colorTexture;

    private RenderTexture _depthTexture;

    private RenderTexture _resultTexture;

    private RenderTexture _monitorTexture;

    private bool _isMobCyalume3D;

    private RenderTexture _cyalumeGrowTexture;

    private CommandBuffer _cyalumeCommandBuffer;

    private Shader _cyalumeCopyShader;

    private Material _cyalumeCopyMaterial;

    [SerializeField]
    private float _cyalumeBlurHorizontalOffset = 2.4f;

    [SerializeField]
    private float _cyalumeBlurVerticalOffset = 0.6f;

    [SerializeField]
    private float _cyalumeThreshold = 1f;

    [SerializeField]
    private float _cyalumeGrowPower = 1.4f;

    private RenderTexture _crossFadeTexture;

    private float _crossFadeRate;

    private Shader _simpleBlendShader;

    private Material _simpleBlendMaterial;

    [SerializeField]
    private bool _isMainCamera;

    private PostEffectLive3D _mainCameraPostEffectLive3D;

    [SerializeField]
    private FilterType _filterType = FilterType.DiffusionDofBloom;

    private Shader _weightedBlurShader;

    private Material _weightedBlurMaterial;

    private Shader _fastBloomShader;

    private Material _fastBloomMaterial;

    private Shader _cyalume3dBloomShader;

    private Material _cyalume3dBloomMaterial;

    private Shader _postBlitShader;

    private Material _postBlitMaterial;

    private Shader _postBloomShader;

    private Material _postBloomMaterial;

    private Shader _postDofBloomShader;

    private Material _postDofBloomMaterial;

    private Shader _postDiffusionBloomShader;

    private Material _postDiffusionBloomMaterial;

    private Shader _postDiffusionDofBloomShader;

    private Material _postDiffusionDofBloomMaterial;

    private bool _isCreatedMaterials;

    private bool _valid;

    [SerializeField]
    [Header("DOF")]
    private bool _isDof;

    [SerializeField]
    private DofResolution _dofResolution = DofResolution.High;

    [SerializeField]
    private DofFocalType _dofFocalType;

    [SerializeField]
    private Transform _dofFocalTransfrom;

    [SerializeField]
    private Vector3 _dofFocalPosition = Vector3.zero;

    [SerializeField]
    private float _dofFocalPoint = 1f;

    [SerializeField]
    private float _dofSmoothness = 0.5f;

    [SerializeField]
    private float _focalSize;

    [SerializeField]
    private float _dofMaxBlurSpread = 1.75f;

    [SerializeField]
    private eDofMVFilterType _typeDOFMVFilter;

    [SerializeField]
    private Texture2D _texDOFMVFilter;

    [SerializeField]
    private float _intensityDOFMVFilter;

    [SerializeField]
    private Vector2 _scaleDOFMVFilter = Vector2.one;

    [SerializeField]
    private Vector2 _offsetDOFMVFilter = Vector2.zero;

    [SerializeField]
    private bool _disableDOFBlur;

    [Range(0f, 1f)]
    [SerializeField]
    [Header("Bloom")]
    private float _bloomDofWeight;

    [SerializeField]
    [Range(0f, 1.5f)]
    private float _bloomThreshhold = 0.25f;

    [SerializeField]
    [Range(0f, 2.5f)]
    private float _bloomIntensity = 0.75f;

    [SerializeField]
    [Range(0.25f, 5.5f)]
    private float _bloomBlurSize = 1f;

    [SerializeField]
    private bool _isEnableBloom = true;

    [SerializeField]
    private bool _enableDofAutoDisable = true;

    [SerializeField]
    private bool _disableDofTemporary;

    [SerializeField]
    [Range(0.1f, 10f)]
    [Header("Diffusion")]
    private float _diffusionBlurSize = 1f;

    [SerializeField]
    [Range(0f, 2f)]
    private float _bright = 1f;

    [SerializeField]
    [Range(0f, 2f)]
    private float _saturation = 1f;

    [SerializeField]
    [Range(0f, 2f)]
    private float _contrast = 1f;

    private static bool _enableScreenScale = false;

    private bool _stateScreenScale;

    private Shader _shdScreenScale;

    private Material _mtrlScreenScale;

    private Rect _rtBckViewPort = new Rect(0f, 0f, 1f, 1f);

    private Vector2 _prmScreenScale = Vector2.up;

    private RuntimePostEffectInfomation _runtimeInfo = new RuntimePostEffectInfomation
    {
        _debug = false
    };

    [SerializeField]
    [Header("Color Correction")]
    public ColorCorrection colorCorrection = new ColorCorrection();

    public static bool _enableGlobalFog = true;

    [SerializeField]
    [Header("Global Fog")]
    private Shader _globalFogShader;

    private Material _globalFogMaterial;

    [SerializeField]
    [Tooltip("Apply distance-based fog?")]
    private bool _distanceFog;

    [SerializeField]
    [Tooltip("Distance fog is based on radial distance from camera when checked")]
    private bool _useRadialDistance;

    [SerializeField]
    [Tooltip("Apply height-based fog?")]
    private bool _heightFog;

    [SerializeField]
    [Tooltip("Fog top Y coordinate")]
    private float _fogHeight = 1f;

    [SerializeField]
    [Range(0.001f, 10f)]
    private float _fogHeightDensity = 2f;

    [SerializeField]
    [Tooltip("Push fog away from the camera by this amount")]
    private float _startDistance;

    [SerializeField]
    private Color _fogColor;

    [SerializeField]
    private FogMode _sceneFogMode = FogMode.ExponentialSquared;

    [SerializeField]
    private float _sceneFogDensity = 0.01f;

    [SerializeField]
    private float _sceneFogStart;

    [SerializeField]
    private float _sceneFogEnd = 300f;

    [SerializeField]
    private Vector4 _fogHeightOption = new Vector4(1f, 0f, 0f, 0f);

    [SerializeField]
    private Vector4 _fogDistanceOption = new Vector4(1f, 0f, 1f, 0f);

    public Shader _shader;

    public Material _material;

    [SerializeField]
    [Header("Indirect LightShafts")]
    public IndirectLightShafts indirectLightShafts = new IndirectLightShafts();

    private readonly string _hdrBloomShaderKeyword = "HDR_BLOOM";

    [SerializeField]
    [Header("Screen Overlay - First layer")]
    private ScreenOverlay _screenOverlay = new ScreenOverlay();

    [SerializeField]
    [Header("Screen Overlay - Second layer")]
    private ScreenOverlay _screenOverlay2 = new ScreenOverlay();

    [NonSerialized]
    private float _fPrevTime = 1f;

    [SerializeField]
    private DofBlurType _dofBlurType;

    [SerializeField]
    private DofQuality _dofQuality = DofQuality.OnlyBackground;

    [SerializeField]
    private float _dofForegroundSize;

    [SerializeField]
    private float _dofFgBlurSpread = 1f;

    private float _dofHeightBaseSize = 0.00244140625f;

    private int _rezworkWidth;

    private int _rezworkHeight;

    private RenderTexture[] _tmpRenderTexture = new RenderTexture[5];

    [SerializeField]
    [Header("Rich Bloom")]
    public bool _isEnableBloomHighRange;

    public BloomScreenBlendMode screenBlendMode;

    public HDRBloomMode hdr = HDRBloomMode.On;

    private bool doHdr;

    public float sepBlurSpread = 2.5f;

    public float bloomIntensity = 3f;

    public int bloomBlurIterations = 1;

    public Shader screenBlendShader;

    private Material screenBlend;

    public Shader blurAndFlaresShader;

    private Material blurAndFlaresMaterial;

    public Shader brightPassFilterShader;

    private Material brightPassFilterMaterial;

    private SunShaftsResolution _resolution = SunShaftsResolution.Normal;

    private ShaftsScreenBlendMode _screenBlendMode;

    [SerializeField]
    [Header("Sun Shafts")]
    private Transform _sunTransform;

    public Shader sunShaftsShader;

    private Material _sunShaftsMaterial;

    public Shader simpleClearShader;

    private Material _simpleClearMaterial;

    private bool _isEnabledSunShaft;

    private Color _sunColor = Color.white;

    private float _sunPower = -1f;

    private float _centerBrightness = 1.5f;

    private float _centerMultiplex = 1f;

    private float _komorebiRate;

    private float _sunShaftBlurRadius = 2.5f;

    private bool _isEnabledBorderClear;

    private float _sunShaftIntensity = 1.15f;

    private float _blackLevel = 0.1f;

    private int _blurIterations = 2;

    [SerializeField]
    [Header("Tilt Shift")]
    private TiltShift _tiltShift = new TiltShift();

    public float cyalumeBlurHorizontalOffset
    {
        set
        {
            _cyalumeBlurHorizontalOffset = value;
        }
    }

    public float cyalumeBlurVerticalOffset
    {
        set
        {
            _cyalumeBlurVerticalOffset = value;
        }
    }

    public float cyalumeThreshold
    {
        set
        {
            _cyalumeThreshold = value;
        }
    }

    public float cyalumeGrowPower
    {
        set
        {
            _cyalumeGrowPower = value;
        }
    }

    public RenderTexture monitorTexture
    {
        get
        {
            return _monitorTexture;
        }
        set
        {
            _monitorTexture = value;
        }
    }

    public bool isMainCamera => _isMainCamera;

    public FilterType filterType
    {
        get
        {
            return _filterType;
        }
        set
        {
            _filterType = value;
        }
    }

    public bool valid
    {
        get
        {
            if (!_valid)
            {
                _valid = _isCreatedMaterials && _propID != null && _colorTexture != null && _resultTexture != null;
            }
            return _valid;
        }
    }

    public bool isDof
    {
        get
        {
            return _isDof;
        }
        set
        {
            _isDof = value;
        }
    }

    public bool isEnableBloom
    {
        set
        {
            _isEnableBloom = value;
        }
    }

    public static bool enableScreenScale
    {
        get
        {
            return _enableScreenScale;
        }
        set
        {
            _enableScreenScale = value;
        }
    }

    public RuntimePostEffectInfomation runtimeInfo => _runtimeInfo;

    private bool isGlobalFog => (_distanceFog || _heightFog) & _enableGlobalFog;

    public bool distanceFog
    {
        set
        {
            _distanceFog = value;
        }
    }

    public bool useRadialDistance
    {
        set
        {
            _useRadialDistance = value;
        }
    }

    public bool heightFog
    {
        set
        {
            _heightFog = value;
        }
    }

    public float fogHeight
    {
        set
        {
            _fogHeight = value;
        }
    }

    public float fogHeightDensity
    {
        set
        {
            _fogHeightDensity = value;
        }
    }

    public float startDistance
    {
        set
        {
            _startDistance = value;
        }
    }

    public Color fogColor
    {
        set
        {
            _fogColor = value;
        }
    }

    public FogMode fogMode
    {
        set
        {
            _sceneFogMode = value;
        }
    }

    public float expDensity
    {
        set
        {
            _sceneFogDensity = value;
        }
    }

    public float fogStart
    {
        set
        {
            _sceneFogStart = value;
        }
    }

    public float fogEnd
    {
        set
        {
            _sceneFogEnd = value;
        }
    }

    public Vector4 fogHeightOption
    {
        set
        {
            _fogHeightOption = value;
        }
    }

    public Vector4 fogDistanceOption
    {
        set
        {
            _fogDistanceOption = value;
        }
    }

    public Vector4 speed
    {
        get
        {
            return indirectLightShafts.speed;
        }
        set
        {
            indirectLightShafts.speed = value;
        }
    }

    public Vector4 offset
    {
        get
        {
            return indirectLightShafts.offset;
        }
        set
        {
            indirectLightShafts.offset = value;
        }
    }

    public Vector4 angle
    {
        get
        {
            return indirectLightShafts.angle;
        }
        set
        {
            indirectLightShafts.angle = value;
        }
    }

    public float scale
    {
        get
        {
            return indirectLightShafts.scale;
        }
        set
        {
            indirectLightShafts.scale = value;
        }
    }

    public Vector4 alpha
    {
        get
        {
            return indirectLightShafts.alpha;
        }
        set
        {
            indirectLightShafts.alpha = value;
        }
    }

    public Vector4 alpha2
    {
        get
        {
            return indirectLightShafts.alpha2;
        }
        set
        {
            indirectLightShafts.alpha2 = value;
        }
    }

    public Vector4 maskAlpha
    {
        get
        {
            return indirectLightShafts.maskAlpha;
        }
        set
        {
            indirectLightShafts.maskAlpha = value;
        }
    }

    public LiveTimelineIndirectLightShuftSettings.ShaftType shaftType
    {
        get
        {
            return indirectLightShafts.shaftType;
        }
        set
        {
            indirectLightShafts.shaftType = value;
        }
    }

    public ScreenOverlay screenOverlay => _screenOverlay;

    public ScreenOverlay screenOverlay2 => _screenOverlay2;

    public DofResolution dofResolution
    {
        get
        {
            return _dofResolution;
        }
        set
        {
            _dofResolution = value;
        }
    }

    public DofBlurType dofBlurType
    {
        get
        {
            return _dofBlurType;
        }
        set
        {
            _dofBlurType = value;
        }
    }

    public DofQuality dofQuality
    {
        get
        {
            return _dofQuality;
        }
        set
        {
            _dofQuality = value;
        }
    }

    public float dofForegroundSize
    {
        get
        {
            return _dofForegroundSize;
        }
        set
        {
            _dofForegroundSize = value;
        }
    }

    public float dofFgBlurSpread
    {
        get
        {
            return _dofFgBlurSpread;
        }
        set
        {
            _dofFgBlurSpread = value;
        }
    }

    public bool isRichBloom
    {
        get
        {
            if (!_isEnableBloomHighRange)
            {
                return false;
            }
            if (bloomIntensity <= 0f)
            {
                return false;
            }
            return true;
        }
    }

    public Transform sunTransform
    {
        get
        {
            return _sunTransform;
        }
        set
        {
            _sunTransform = value;
        }
    }

    public bool isEnabledSunShaft
    {
        get
        {
            return _isEnabledSunShaft;
        }
        set
        {
            _isEnabledSunShaft = value;
        }
    }

    public Color sunColor
    {
        set
        {
            _sunColor = value;
        }
    }

    public float sunPower
    {
        set
        {
            _sunPower = value;
        }
    }

    public float centerBrightness
    {
        set
        {
            _centerBrightness = value;
        }
    }

    public float centerMultiplex
    {
        set
        {
            _centerMultiplex = value;
        }
    }

    public float komorebiRate
    {
        set
        {
            _komorebiRate = value;
        }
    }

    public float sunShaftBlurRadius
    {
        set
        {
            _sunShaftBlurRadius = value;
        }
    }

    public bool isEnabledBorderClear
    {
        set
        {
            _isEnabledBorderClear = value;
        }
    }

    private bool isSunShafts => 0 == 0 && CheckResources() && _isEnabledSunShaft && !(null == _sunTransform) && !(_sunPower < 0.01f);

    public TiltShift tiltShift => _tiltShift;

    public Camera CameraCaching()
    {
        if (_cacheCamera == null)
        {
            _cacheCamera = new CacheCamera(GetComponent<Camera>());
        }
        return _cacheCamera.camera;
    }

    public void SetCyalumeGrareTexture(RenderTexture renderTexture)
    {
        _cyalumeCopyShader = ResourcesManager.instance.GetShader("StencilColorCopy");
        _cyalumeCopyMaterial = new Material(_cyalumeCopyShader);
        _cyalumeGrowTexture = new RenderTexture(_colorTexture.width, _colorTexture.height, 0, _colorTexture.format);
        _cyalumeCommandBuffer = new CommandBuffer();
        _cyalumeCommandBuffer.SetRenderTarget(_cyalumeGrowTexture.colorBuffer, _depthTexture.depthBuffer);
        _cyalumeCommandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
        _cyalumeCommandBuffer.Blit(_colorTexture, BuiltinRenderTextureType.CurrentActive, _cyalumeCopyMaterial);
        CameraCaching().AddCommandBuffer(CameraEvent.AfterHaloAndLensFlares, _cyalumeCommandBuffer);
    }

    public void SetCameraRenderTarget(RenderTexture rtColor, RenderTexture rtDepth, RenderTexture rtResult, RenderTexture rtMonitor)
    {
        Camera camera = CameraCaching();
        _colorTexture = rtColor;
        _depthTexture = rtDepth;
        _resultTexture = rtResult;
        _monitorTexture = rtMonitor;
        if (_depthTexture != null)
        {
            camera.SetTargetBuffers(_colorTexture.colorBuffer, _depthTexture.depthBuffer);
        }
        else
        {
            camera.targetTexture = _colorTexture;
        }
        ScreenFade component = GetComponent<ScreenFade>();
        if (component != null)
        {
            component.renderTexture = _resultTexture;
        }
        _isMobCyalume3D = ViewLauncher.instance.liveDirector.live3DData.mobCyalume3DResourceID != 0 || (Director.instance != null && _depthTexture != null && Director.instance._existsMobCyalume3DImitation);
        if (_isMobCyalume3D)
        {
            SetCyalumeGrareTexture(null);
        }
    }

    public void SetCrossFadeBlendTexture(RenderTexture rtColor, float blendRate)
    {
        _crossFadeTexture = rtColor;
        _crossFadeRate = blendRate;
    }

    private Vector2 CalcScreenScale(float scale)
    {
        Vector2 one = Vector2.one;
        scale = 1f / scale;
        one.x = 0.5f - 0.5f * scale;
        one.y = scale;
        return one;
    }

    public void UpdateScreenScale()
    {
        if (_cacheCamera != null && _cacheCamera.camera != null)
        {
            Camera camera = _cacheCamera.camera;
            if (_enableScreenScale && _enableScreenScale != _stateScreenScale)
            {
                _prmScreenScale = CalcScreenScale(camera.pixelRect.width / camera.pixelRect.height / 1.77777779f);
                _rtBckViewPort = camera.rect;
                _cacheCamera.camera.rect = new Rect(0f, 0f, 1f, 1f);
            }
            else if (_enableScreenScale != _stateScreenScale)
            {
                camera.rect = _rtBckViewPort;
                _prmScreenScale = Vector2.up;
            }
            if (_mtrlScreenScale != null)
            {
                _mtrlScreenScale.SetVector("_ScreenScale", _prmScreenScale);
            }
            _stateScreenScale = _enableScreenScale;
        }
    }

    public void SetForcalSize(float val)
    {
        _focalSize = val;
    }

    public void SetDofMaxBlurSpread(float val)
    {
        _dofMaxBlurSpread = val;
    }

    public void SetBloomDofWeight(float val)
    {
        _bloomDofWeight = val;
    }

    public void SetThreshold(float val)
    {
        _bloomThreshhold = val;
    }

    public void SetIntensity(float val)
    {
        _bloomIntensity = val;
    }

    public void setFocalTransform(Transform transform)
    {
        _dofFocalType = DofFocalType.Transform;
        _dofFocalTransfrom = transform;
    }

    public void setFocalPosition(Vector3 position)
    {
        _dofFocalType = DofFocalType.Position;
        _dofFocalPosition = position;
    }

    public void setFocalPoint(float point)
    {
        _dofFocalType = DofFocalType.Point;
        _dofFocalPoint = point;
    }

    public void SetDOFMVFilter(eDofMVFilterType type, float intensity, Texture2D tex, Vector2 scale, Vector2 offset)
    {
        _typeDOFMVFilter = type;
        _intensityDOFMVFilter = intensity;
        _texDOFMVFilter = tex;
        _scaleDOFMVFilter = scale;
        _offsetDOFMVFilter = offset;
    }

    public void SetDisableDOFBlur(bool disable)
    {
        _disableDOFBlur = disable;
    }

    private float FocalDistance01(float worldDist)
    {
        Camera camera = _cacheCamera.camera;
        return camera.WorldToViewportPoint((worldDist - camera.nearClipPlane) * _cacheCamera.cacheTransform.forward + _cacheCamera.cacheTransform.position).z / (camera.farClipPlane - camera.nearClipPlane);
    }

    private float GetDividerBasedOnQuality()
    {
        float result = 1f;
        switch (_dofResolution)
        {
            case DofResolution.Medium:
                result = 0.5f;
                break;
            case DofResolution.Low:
                result = 0.5f;
                break;
        }
        return result;
    }

    private float GetLowResolutionDividerBasedOnQuality(float baseDivider)
    {
        float num = baseDivider;
        switch (_dofResolution)
        {
            case DofResolution.High:
                num *= 0.5f;
                break;
            case DofResolution.Low:
                num *= 0.5f;
                break;
        }
        return num;
    }

    private void CreateMaterials()
    {
        bool flag = true;
        _weightedBlurShader = ResourcesManager.instance.GetShader("WeightedBlur");
        _weightedBlurMaterial = CheckShaderAndCreateMaterial(_weightedBlurShader, _weightedBlurMaterial);
        flag = flag && _weightedBlurMaterial != null;
        _fastBloomShader = ResourcesManager.instance.GetShader("FastBloom");
        _fastBloomMaterial = CheckShaderAndCreateMaterial(_fastBloomShader, _fastBloomMaterial);
        flag = flag && _fastBloomMaterial != null;
        if (_isMobCyalume3D)
        {
            _cyalume3dBloomShader = ResourcesManager.instance.GetShader("Cyalume3dBloom");
            _cyalume3dBloomMaterial = CheckShaderAndCreateMaterial(_cyalume3dBloomShader, _cyalume3dBloomMaterial);
            flag = flag && _cyalume3dBloomMaterial != null;
        }
        _postBlitShader = ResourcesManager.instance.GetShader("PostBlit_Rich");
        _postBlitMaterial = CheckShaderAndCreateMaterial(_postBlitShader, _postBlitMaterial);
        flag = flag && _postBlitMaterial != null;
        _postBloomShader = ResourcesManager.instance.GetShader("PostBloom_Rich");
        _postBloomMaterial = CheckShaderAndCreateMaterial(_postBloomShader, _postBloomMaterial);
        flag = flag && _postBloomMaterial != null;
        _postDofBloomShader = ResourcesManager.instance.GetShader("PostDofBloom_Rich");
        _postDofBloomMaterial = CheckShaderAndCreateMaterial(_postDofBloomShader, _postDofBloomMaterial);
        flag = flag && _postDofBloomMaterial != null;
        if (_enableScreenScale)
        {
            _shdScreenScale = ResourcesManager.instance.GetShader("ScreenScale");
            _mtrlScreenScale = CheckShaderAndCreateMaterial(_shdScreenScale, _mtrlScreenScale);
            flag = flag && _mtrlScreenScale != null;
        }
        flag = flag && CreateMaterialsGlobalFog() && CreateMaterialsSunShafts() && CreateMaterialsRichBloom() && CreateMaterialsTilfShift() && CreateMaterialsIndirectLightShafts();

        _simpleBlendShader = ResourcesManager.instance.GetShader("SimpleBlend");
        _simpleBlendMaterial = CheckShaderAndCreateMaterial(_simpleBlendShader, _simpleBlendMaterial);
        flag = flag && _simpleBlendMaterial != null;

        _postDiffusionBloomShader = ResourcesManager.instance.GetShader("PostDiffusionBloom_Rich");
        _postDiffusionBloomMaterial = CheckShaderAndCreateMaterial(_postDiffusionBloomShader, _postDiffusionBloomMaterial);
        flag = flag && _postDiffusionBloomMaterial != null;
        _postDiffusionDofBloomShader = ResourcesManager.instance.GetShader("PostDiffusionDofBloom_Rich");
        _postDiffusionDofBloomMaterial = CheckShaderAndCreateMaterial(_postDiffusionDofBloomShader, _postDiffusionDofBloomMaterial);
        flag = flag && _postDiffusionDofBloomMaterial != null;

        CreateMaterialsAntialiasing();

        _isCreatedMaterials = flag;
    }

    public override bool CheckResources()
    {
        if (_depthTexture != null)
        {
            CheckSupport(needDepth: false, out _bUsableDepthTexture);
            _bUsableDepthTexture = true;
        }
        else
        {
            CheckSupport(needDepth: true, out _bUsableDepthTexture);
        }
        if (!_isCreatedMaterials)
        {
            CreateMaterials();
        }
        if (!isSupported)
        {
            ReportAutoDisable();
        }
        return isSupported;
    }

    private void OnDestroy()
    {
        CameraCaching();
        if ((bool)_fastBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_fastBloomMaterial);
            _fastBloomMaterial = null;
        }
        if ((bool)_cyalume3dBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_cyalume3dBloomMaterial);
            _cyalume3dBloomMaterial = null;
        }
        if ((bool)_postDofBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_postDofBloomMaterial);
            _postDofBloomMaterial = null;
        }
        if ((bool)_weightedBlurMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_weightedBlurMaterial);
            _weightedBlurMaterial = null;
        }
        if ((bool)_postBlitMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_postBlitMaterial);
            _postBlitMaterial = null;
        }
        if ((bool)_postBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_postBloomMaterial);
            _postBloomMaterial = null;
        }
        if ((bool)_postDiffusionBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_postDiffusionBloomMaterial);
            _postDiffusionBloomMaterial = null;
        }
        if ((bool)_postDiffusionDofBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_postDiffusionDofBloomMaterial);
            _postDiffusionDofBloomMaterial = null;
        }
        DestroyGlobalFog();
        DestroyRichBloom();
        DestroyTilfShift();
        DestroyIndirectLightShafts();
        DestroySunShafts();
        if (_simpleBlendMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(_simpleBlendMaterial);
            _simpleBlendMaterial = null;
        }
        _valid = false;
        _isCreatedMaterials = false;
    }

    private void OnEnable()
    {
        isCheckSupportInitialize = false;
        CameraCaching();
    }

    private bool IsNeedDepthTextureForDOF()
    {
        return _filterType != FilterType.NoEffect;
    }

    private void ClearRuntimeRichPostEffectInfomation()
    {
        _runtimeInfo._debug = false;
        _runtimeInfo._runtimeEnableTiltShift = false;
        _runtimeInfo._runtimeEnableSunShafts = false;
        _runtimeInfo._runtimeEnableRichBloom = false;
        _runtimeInfo._runtimeEnableGlobalFog = false;
        _runtimeInfo._runtimeEnableIndirectLightShafts = false;
        _runtimeInfo._runtimeEnableColorCorrection = false;
        _runtimeInfo._runtimeEnableTiltShift = false;
    }

    private void DecideRuntimeRichPostEffectInfomation()
    {
        if (!_runtimeInfo._debug)
        {
            _runtimeInfo._runtimeEnableSunShafts = isSunShafts;
            _runtimeInfo._runtimeEnableRichBloom = isRichBloom;
            _runtimeInfo._runtimeEnableGlobalFog = isGlobalFog;
            _runtimeInfo._runtimeEnableIndirectLightShafts = indirectLightShafts.enabled;
            _runtimeInfo._runtimeEnableColorCorrection = colorCorrection.enabled;
            _runtimeInfo._runtimeEnableTiltShift = tiltShift.isEnable;
        }
    }

    public void UpdateRuntimePostEffectInfomation()
    {
        //セーブによって切り替え
        if (PostEffectSetting.config_Diffusion)
        {
            _filterType = FilterType.DiffusionDofBloom;
        }
        else
        {
            _filterType = FilterType.DofBloom;
        }


        if (!isDof || _disableDofTemporary || _cacheCamera == null || _cacheCamera.camera == null || !_bUsableDepthTexture)
        {
            _runtimeInfo._runtimeEnableDof = false;
        }
        else
        {
            _runtimeInfo._runtimeEnableDof = true;
        }
        switch (_filterType)
        {
            case FilterType.NoEffect:
                _runtimeInfo._runtimeFilterType = RuntimeFilterType.NoEffect;
                break;
            case FilterType.DofBloom:
                if ((!isDof || _disableDofTemporary) && !_isEnableBloom)
                {
                    _runtimeInfo._runtimeFilterType = RuntimeFilterType.NoEffect;
                }
                else if (_runtimeInfo._runtimeEnableDof)
                {
                    _runtimeInfo._runtimeFilterType = RuntimeFilterType.DofBloom;
                }
                else
                {
                    _runtimeInfo._runtimeFilterType = RuntimeFilterType.Bloom;
                }
                break;
            case FilterType.DiffusionDofBloom:
                if (_runtimeInfo._runtimeEnableDof)
                {
                    _runtimeInfo._runtimeFilterType = RuntimeFilterType.DiffusionDofBloom;
                }
                else
                {
                    _runtimeInfo._runtimeFilterType = RuntimeFilterType.DiffusionBloom;
                }
                break;
        }
        if (_filterType == FilterType.NoEffect)
        {
            ClearRuntimeRichPostEffectInfomation();
        }
        else
        {
            DecideRuntimeRichPostEffectInfomation();
        }
    }

    private RenderTexture OnRenderImageCyalume(RenderTexture source)
    {
        int num = source.width / 2;
        int num2 = source.height / 4;
        RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, source.format);
        temporary.filterMode = FilterMode.Bilinear;
        Graphics.Blit(_cyalumeGrowTexture, temporary, _cyalume3dBloomMaterial, 0);
        _bloomSizeParameter.z = _cyalumeThreshold;
        _bloomSizeParameter.y = _cyalumeGrowPower;
        int width = num / 2;
        num2 /= 2;
        RenderTexture temporary2 = RenderTexture.GetTemporary(width, num2, 0, temporary.format);
        RenderTexture temporary3 = RenderTexture.GetTemporary(width, num2, 0, temporary.format);
        _bloomSizeParameter.x = _cyalumeBlurHorizontalOffset;

        _cyalume3dBloomMaterial.SetVector(_propID[(int)eProp._Parameter], _bloomSizeParameter);
        Graphics.Blit(temporary, temporary2, _cyalume3dBloomMaterial, 2);

        _cyalume3dBloomMaterial.SetTexture(_propID[(int)eProp._Composite], null);
        Graphics.Blit(temporary2, temporary3, _cyalume3dBloomMaterial, 3);

        _bloomSizeParameter.x = _cyalumeBlurVerticalOffset;
        _cyalume3dBloomMaterial.SetVector(_propID[(int)eProp._Parameter], _bloomSizeParameter);
        Graphics.Blit(temporary, temporary2, _cyalume3dBloomMaterial, 1);

        _cyalume3dBloomMaterial.SetTexture(_propID[(int)eProp._Composite], temporary3);
        Graphics.Blit(temporary2, temporary, _cyalume3dBloomMaterial, 3);

        RenderTexture.ReleaseTemporary(temporary2);
        RenderTexture.ReleaseTemporary(temporary3);
        RenderTexture temporary4 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        _cyalume3dBloomMaterial.SetTexture(_propID[(int)eProp._Composite], temporary);
        _cyalume3dBloomMaterial.SetVector(_propID[(int)eProp._Parameter], _bloomSizeParameter);
        Graphics.Blit(source, temporary4, _cyalume3dBloomMaterial, 4);

        RenderTexture.ReleaseTemporary(temporary);
        return temporary4;
    }

    /// <summary>
    /// 高速なブルームのみ使用
    /// </summary>
    private void OnRenderImageFastBloom(RenderTexture source, RenderTexture destination)
    {
        _postBloomMaterial.SetFloat(_propID[(int)eProp._bloomDofWeight], _bloomDofWeight);
        _bloomSizeParameter.x = _bloomBlurSize * 0.5f;
        _bloomSizeParameter.z = _bloomThreshhold;
        _bloomSizeParameter.w = _bloomIntensity;
        _fastBloomMaterial.SetVector(_propID[(int)eProp._Parameter], _bloomSizeParameter);
        int width = source.width / 4;
        int height = source.height / 4;
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
        temporary.filterMode = FilterMode.Bilinear;
        if (_monitorTexture != null)
        {
            Graphics.Blit(source, _monitorTexture);
            Graphics.Blit(_monitorTexture, temporary, _fastBloomMaterial, PASS_FASTBLOOM_DOWNSAMPLE);
        }
        else
        {
            Graphics.Blit(source, temporary, _fastBloomMaterial, PASS_FASTBLOOM_DOWNSAMPLE);
        }
        if (_runtimeInfo._runtimeEnableRichBloom)
        {
            RenderTexture renderTexture = OnRenderRichBloom(source);
            _postBloomMaterial.SetTexture(_propID[(int)eProp._Bloom], temporary);
            _postBloomMaterial.SetTexture(_propID[(int)eProp._BloomHighRange], renderTexture);
            _postBloomMaterial.SetFloat(_propID[(int)eProp._bloomHighRangeIntensity], bloomIntensity);
            PostFilmBlit(source, destination, _postBloomMaterial, PASS_POSTBLOOM_BLOOM, PASS_POSTBLOOM_OVERLAY1, PASS_POSTBLOOM_OVERLAY2);
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        else
        {
            _postBloomMaterial.SetTexture(_propID[(int)eProp._Bloom], temporary);
            PostFilmBlit(source, destination, _postBloomMaterial, PASS_POSTBLOOM_BLOOM, PASS_POSTBLOOM_OVERLAY1, PASS_POSTBLOOM_OVERLAY2);
        }
        RenderTexture.ReleaseTemporary(temporary);
    }

    /// <summary>
    /// エフェクトを掛けず、ソースをコピーするだけ
    /// </summary>
    private void OnRenderImagaOnlyBlit(RenderTexture source, RenderTexture destination)
    {
        if (_monitorTexture != null)
        {
            Graphics.Blit(source, _monitorTexture);
        }
        Graphics.Blit(source, destination);
    }

    /// <summary>
    /// デフォルトエフェクト
    /// </summary>
    private void OnRenderImageDefault(RenderTexture source, RenderTexture destination)
    {
        if (_monitorTexture != null)
        {
            Graphics.Blit(source, _monitorTexture);
        }
        Graphics.Blit(source, destination);
    }

    /// <summary>
    /// DofとfastBloom(被写界深度とブルーム(光がもれる表現))をかける
    /// </summary>
    private void OnRenderImageDiffusionFastBloom(RenderTexture source, RenderTexture destination)
    {
        _postDiffusionBloomMaterial.SetFloat(_propID[(int)eProp._bloomDofWeight], _bloomDofWeight);
        _bloomSizeParameter.x = _bloomBlurSize * 0.5f;
        _bloomSizeParameter.z = _bloomThreshhold;
        _bloomSizeParameter.w = _bloomIntensity;
        _fastBloomMaterial.SetVector(_propID[(int)eProp._Parameter], _bloomSizeParameter);
        int width = source.width / 4;
        int height = source.height / 4;
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
        RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, source.format);
        temporary.filterMode = FilterMode.Bilinear;
        _postDiffusionBloomMaterial.mainTexture = source;
        Graphics.Blit(null, temporary, _postDiffusionBloomMaterial, PASS_POSTDIFFUSIONBLOOM_VERTICALGAUSS);
        _postDiffusionBloomMaterial.mainTexture = temporary;
        Graphics.Blit(null, temporary2, _postDiffusionBloomMaterial, PASS_POSTDIFFUSIONBLOOM_HORIZONGAUSS);
        if (_monitorTexture != null)
        {
            Graphics.Blit(source, _monitorTexture);
            Graphics.Blit(_monitorTexture, temporary, _fastBloomMaterial, PASS_FASTBLOOM_DOWNSAMPLE);
        }
        else
        {
            Graphics.Blit(source, temporary, _fastBloomMaterial, PASS_FASTBLOOM_DOWNSAMPLE);
        }
        float dividerBasedOnQuality = GetDividerBasedOnQuality();
        float lowResolutionDividerBasedOnQuality = GetLowResolutionDividerBasedOnQuality(dividerBasedOnQuality);
        int num = (int)((float)source.width * lowResolutionDividerBasedOnQuality);
        int num2 = (int)((float)source.height * lowResolutionDividerBasedOnQuality);
        _DiffusionPixelSize.x = _diffusionBlurSize * lowResolutionDividerBasedOnQuality / (float)num;
        _DiffusionPixelSize.y = _diffusionBlurSize * lowResolutionDividerBasedOnQuality / (float)num2;
        _DiffusionColorParam.x = _bright;
        _DiffusionColorParam.y = _saturation;
        _DiffusionColorParam.z = _contrast;
        _postDiffusionBloomMaterial.SetVector(_propID[(int)eProp._PixelSize], _DiffusionPixelSize);
        _postDiffusionBloomMaterial.SetVector(_propID[(int)eProp._ColorParam], _DiffusionColorParam);
        _postDiffusionBloomMaterial.SetTexture(_propID[(int)eProp._Bloom], temporary);
        _postDiffusionBloomMaterial.SetTexture(_propID[(int)eProp._RgbTex], temporary2);
        if (_runtimeInfo._runtimeEnableRichBloom)
        {
            RenderTexture renderTexture = OnRenderRichBloom(source);
            _postDiffusionBloomMaterial.SetTexture(_propID[(int)eProp._BloomHighRange], renderTexture);
            _postDiffusionBloomMaterial.SetFloat(_propID[(int)eProp._bloomHighRangeIntensity], bloomIntensity);
            PostFilmBlit(source, destination, _postDiffusionBloomMaterial, PASS_POSTDIFFUSIONBLOOM_BLOOM, PASS_POSTDIFFUSIONBLOOM_OVERLAY1, PASS_POSTDIFFUSIONBLOOM_OVERLAY2);
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        else
        {
            PostFilmBlit(source, destination, _postDiffusionBloomMaterial, PASS_POSTDIFFUSIONBLOOM_BLOOM, PASS_POSTDIFFUSIONBLOOM_OVERLAY1, PASS_POSTDIFFUSIONBLOOM_OVERLAY2);
        }
        RenderTexture.ReleaseTemporary(temporary2);
        RenderTexture.ReleaseTemporary(temporary);
    }

    private static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = dest;
        fxMaterial.SetTexture(SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.MainTex), source);
        GL.PushMatrix();
        GL.LoadOrtho();
        fxMaterial.SetPass(passNr);
        GL.Begin(7);
        GL.MultiTexCoord2(0, 0f, 0f);
        GL.Vertex3(0f, 0f, 3f);
        GL.MultiTexCoord2(0, 1f, 0f);
        GL.Vertex3(1f, 0f, 2f);
        GL.MultiTexCoord2(0, 1f, 1f);
        GL.Vertex3(1f, 1f, 1f);
        GL.MultiTexCoord2(0, 0f, 1f);
        GL.Vertex3(0f, 1f, 0f);
        GL.End();
        GL.PopMatrix();
        RenderTexture.active = active;
    }

    /// <summary>
    /// PostEffectはここから処理を行う
    /// 現在はFinalizeカメラから呼ばれる
    /// </summary>
    public void Work()
    {
        if (!valid)
        {
            return;
        }

        //ポストエフェクトが無効になっている場合は直出力
        if (!PostEffectSetting.config_PostEffect)
        {
            OnRenderImagaOnlyBlit(_colorTexture, _resultTexture);
            return;
        }

        RenderTexture colorTexture = _colorTexture;
        RenderTexture resultTexture = _resultTexture;
        if (!A2U.isEnabled || A2U.IsRenderingOrder(A2U.Order.InImageEffect) || !PostEffectSetting.config_A2U)
        {
            DoOnRenderImage(colorTexture, resultTexture);
        }
        else
        {
            RenderTexture temporary = RenderTexture.GetTemporary(colorTexture.width, colorTexture.height, colorTexture.depth);
            if (A2U.IsRenderingOrder(A2U.Order.PreImageEffect))
            {
                A2U.DoRenderImage(colorTexture, temporary);
                DoOnRenderImage(temporary, resultTexture);
            }
            else
            {
                DoOnRenderImage(colorTexture, temporary);
                A2U.DoRenderImage(temporary, resultTexture);
            }
            RenderTexture.ReleaseTemporary(temporary);
        }

        //スクリーンショットを取得する
        if (PostEffectSetting.captureScreenShot)
        {
            RenderTexture screenShot = RenderTexture.GetTemporary(resultTexture.width, resultTexture.height, 0, resultTexture.format);
            Graphics.Blit(resultTexture, screenShot);
            PostEffectSetting.ScreenShot = screenShot;

            PostEffectSetting.captureScreenShot = false;
        }

        if (_crossFadeRate >= 0f && _crossFadeTexture != null)
        {
            _simpleBlendMaterial.SetFloat(SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.BlendRate), _crossFadeRate);
            Graphics.Blit(_crossFadeTexture, resultTexture, _simpleBlendMaterial);
        }
        if (_enableScreenScale && (bool)_mtrlScreenScale)
        {
            RenderTexture temporary2 = RenderTexture.GetTemporary(colorTexture.width, colorTexture.height, 0);
            Graphics.Blit(null, temporary2);
            Graphics.Blit(temporary2, resultTexture, _mtrlScreenScale);
            RenderTexture.ReleaseTemporary(temporary2);
        }
    }

    /// <summary>
    /// 旧OnRenderImageを分離したもの
    /// OnRenderImageより呼ばれる
    /// </summary>
    private void DoOnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckResources())
        {
            OnRenderImagaOnlyBlit(source, destination);
            return;
        }
        if (_runtimeInfo._runtimeEnableRichBloom)
        {
            Shader.EnableKeyword(_hdrBloomShaderKeyword);
        }
        else
        {
            Shader.DisableKeyword(_hdrBloomShaderKeyword);
        }
        if (!_runtimeInfo.IsNeedPostEffect)
        {
            OnRenderImageDefault(source, destination);
            return;
        }
        RenderTexture renderTexture = source;
        //グローバルフォグ
        if (_runtimeInfo._runtimeEnableGlobalFog && PostEffectSetting.config_Globalfog)
        {
            renderTexture = OnRenderGlobalFog(renderTexture);
        }
        //サンシャフト
        if (_runtimeInfo._runtimeEnableSunShafts && PostEffectSetting.config_SunShafts)
        {
            RenderTexture tempTexture = OnRenderSunShafts(renderTexture);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture;
        }
        //ティルトシフト
        if (_runtimeInfo._runtimeEnableTiltShift && PostEffectSetting.config_TiltShift)
        {
            RenderTexture tempTexture = OnRenderImageTiltShift(renderTexture);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture;
        }
        //ダイレクトライトシャフト
        if (_runtimeInfo._runtimeEnableIndirectLightShafts && PostEffectSetting.config_IndirectLightShafts)
        {
            RenderTexture tempTexture = OnRenderImageIndirectLightShafts(renderTexture);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture;
        }

        if (_dofSmoothness < 0.1f)
        {
            _dofSmoothness = 0.1f;
        }
        _dofInvRenderTargetSize.x = 1f / (float)renderTexture.width;
        _dofInvRenderTargetSize.y = 1f / (float)renderTexture.height;

        if (_isMobCyalume3D)
        {
            RenderTexture tempTexture = OnRenderImageCyalume(renderTexture);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture;
        }

        //セーブから使用するタイプを取得
        RuntimeFilterType type = RuntimeFilterType.NoEffect;
        if (PostEffectSetting.config_Bloom && PostEffectSetting.config_Dof)
        {
            type = RuntimeFilterType.DofBloom;
        }
        else if (PostEffectSetting.config_Bloom)
        {
            type = RuntimeFilterType.Bloom;
        }
        if (type != RuntimeFilterType.NoEffect && PostEffectSetting.config_Diffusion)
        {
            type += 2; //Diffusionぶん足す
        }

        //デレステ側でDofが無効になっている場合はDofを無効にする
        if (_runtimeInfo._runtimeFilterType == RuntimeFilterType.Bloom && type == RuntimeFilterType.DofBloom)
        {
            type = RuntimeFilterType.Bloom;
        }
        else if (_runtimeInfo._runtimeFilterType == RuntimeFilterType.DiffusionBloom && type == RuntimeFilterType.DiffusionDofBloom)
        {
            type = RuntimeFilterType.DiffusionBloom;
        }

        RenderTexture tempTexture2 = RenderTexture.GetTemporary(source.width, source.height, source.depth);
        switch (type)
        {
            case RuntimeFilterType.DofBloom:
                OnRenderImageDofBloom(renderTexture, tempTexture2);
                break;
            case RuntimeFilterType.DiffusionDofBloom:
                OnRenderImageDiffusionDofBloom(renderTexture, tempTexture2);
                break;
            case RuntimeFilterType.Bloom:
                OnRenderImageFastBloom(renderTexture, tempTexture2);
                break;
            case RuntimeFilterType.DiffusionBloom:
                OnRenderImageDiffusionFastBloom(renderTexture, tempTexture2);
                break;
            case RuntimeFilterType.NoEffect:
            default:
                //本当は何もしない
                OnRenderImageDefault(renderTexture, tempTexture2);
                break;
        }
        if (renderTexture != source)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        renderTexture = tempTexture2;

        if (A2U.isEnabled && A2U.IsRenderingOrder(A2U.Order.InImageEffect) && PostEffectSetting.config_A2U)
        {
            RenderTexture tempTexture3 = RenderTexture.GetTemporary(source.width, source.height, source.depth);
            A2U.DoRenderImage(renderTexture, tempTexture3);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture3;
        }

        if (_runtimeInfo._runtimeEnableColorCorrection && PostEffectSetting.config_ColorCorrection)
        {
            RenderTexture tempTexture3 = RenderTexture.GetTemporary(source.width, source.height, source.depth);
            colorCorrection.OnRenderImage(renderTexture, tempTexture3);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture3;
        }

        //アンチエイリアス
        if (_materialFXAAIII != null && PostEffectSetting.config_AntiAliasing)
        {
            RenderTexture tempTexture3 = OnRenderAntialiasing(renderTexture);
            if (renderTexture != source)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            renderTexture = tempTexture3;
        }

        //最終書き出し
        Graphics.Blit(renderTexture, destination);

        if (renderTexture != source)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    public void InitParam(PostEffectLive3D postEffect)
    {
        if (this != postEffect)
        {
            _mainCameraPostEffectLive3D = postEffect;
        }
    }

    private new void Start()
    {
        base.Start();
        _valid = false;
        _propID = new PropertyID<eProp>();
        CameraCaching();
        UpdateScreenScale();
        ResetGlobalFogColor();

        Shader.EnableKeyword("IS_RICH");
        _dofResolution = DofResolution.High;
        /*
        if (LiveUtils.IsRich())
        {
            Shader.EnableKeyword("IS_RICH");
            _dofResolution = (LiveUtils.IsRich() ? DofResolution.High : DofResolution.Low);
        }
        else
        {
            Shader.DisableKeyword("IS_RICH");
        }
        */
    }

    private void LateUpdateStatusFromMainCamera()
    {
        if (!(_mainCameraPostEffectLive3D == null))
        {
            _diffusionBlurSize = _mainCameraPostEffectLive3D._diffusionBlurSize;
            _filterType = _mainCameraPostEffectLive3D.filterType;
            _bright = _mainCameraPostEffectLive3D._bright;
            _saturation = _mainCameraPostEffectLive3D._saturation;
            _contrast = _mainCameraPostEffectLive3D._contrast;
        }
    }

    private void LateUpdate()
    {
        Director instance = Director.instance;
        float num;

        //焦点距離
        if (instance == null)
        {
            num = 30f;
        }
        else
        {
            num = instance.GetMaxForcalSize();
        }

        //焦点距離がnumより大きい場合は無効化
        if (_enableDofAutoDisable)
        {
            if (_focalSize < num)
            {
                _disableDofTemporary = false;
            }
            else
            {
                _disableDofTemporary = true;
            }
        }
        else
        {
            _disableDofTemporary = false;
        }
        LateUpdateStatusFromMainCamera();
    }

    #region GlobalFog

    private bool CreateMaterialsGlobalFog()
    {
        _globalFogShader = ResourcesManager.instance.GetShader("GlobalFog");
        _globalFogMaterial = CheckShaderAndCreateMaterial(_globalFogShader, _globalFogMaterial);
        return _globalFogMaterial != null;
    }

    private void ResetGlobalFogColor()
    {
        _fogColor = RenderSettings.fogColor;
    }

    private void DestroyGlobalFog()
    {
        if ((bool)_globalFogMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_globalFogMaterial);
            _globalFogMaterial = null;
        }
    }

    private bool IsNeedDepthTextureForGlobalFog()
    {
        return isGlobalFog;
    }

    private RenderTexture OnRenderGlobalFog(RenderTexture source)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
        Camera camera = _cacheCamera.camera;
        Transform transform = camera.transform;
        float nearClipPlane = camera.nearClipPlane;
        float farClipPlane = camera.farClipPlane;
        float fieldOfView = camera.fieldOfView;
        float aspect = camera.aspect;
        Matrix4x4 identity = Matrix4x4.identity;
        float num = fieldOfView * 0.5f;
        Vector3 vector = transform.right * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f)) * aspect;
        Vector3 vector2 = transform.up * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f));
        Vector3 vector3 = transform.forward * nearClipPlane - vector + vector2;
        float num2 = vector3.magnitude * farClipPlane / nearClipPlane;
        vector3.Normalize();
        vector3 *= num2;
        Vector3 vector4 = transform.forward * nearClipPlane + vector + vector2;
        vector4.Normalize();
        vector4 *= num2;
        Vector3 vector5 = transform.forward * nearClipPlane + vector - vector2;
        vector5.Normalize();
        vector5 *= num2;
        Vector3 vector6 = transform.forward * nearClipPlane - vector - vector2;
        vector6.Normalize();
        vector6 *= num2;
        identity.SetRow(0, vector3);
        identity.SetRow(1, vector4);
        identity.SetRow(2, vector5);
        identity.SetRow(3, vector6);
        Vector3 position = transform.position;
        float num3 = position.y - _fogHeight;
        float z = ((num3 <= 0f) ? 1f : 0f);
        SharedShaderParam instance = SharedShaderParam.instance;
        _globalFogMaterial.SetMatrix(instance.getPropertyID(SharedShaderParam.ShaderProperty.FrustumCornersWS), identity);
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.CameraWS), position);
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.HeightParams), new Vector4(_fogHeight, num3, z, _fogHeightDensity * 0.5f));
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.DistanceParams), new Vector4(0f - Mathf.Max(_startDistance, 0f), 0f, 0f, 0f));
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.FogColor), _fogColor);
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.DistanceOption), _fogDistanceOption);
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.HeightOption), _fogHeightOption);
        FogMode sceneFogMode = _sceneFogMode;
        float sceneFogDensity = _sceneFogDensity;
        float sceneFogStart = _sceneFogStart;
        float sceneFogEnd = _sceneFogEnd;
        bool flag = sceneFogMode == FogMode.Linear;
        float num4 = (flag ? (sceneFogEnd - sceneFogStart) : 0f);
        float num5 = ((Mathf.Abs(num4) > 0.0001f) ? (1f / num4) : 0f);
        Vector4 value = default(Vector4);
        value.x = sceneFogDensity * 1.2011224f;
        value.y = sceneFogDensity * 1.442695f;
        value.z = (flag ? (0f - num5) : 0f);
        value.w = (flag ? (sceneFogEnd * num5) : 0f);
        _globalFogMaterial.SetVector(instance.getPropertyID(SharedShaderParam.ShaderProperty.SceneFogParams), value);
        int num6 = 0;
        if (_distanceFog && _heightFog)
        {
            num6 = 0;
            if (!_useRadialDistance)
            {
                num6 += 3;
            }
        }
        else if (_distanceFog)
        {
            num6 = 6;
            if (!_useRadialDistance)
            {
                num6 += 3;
            }
        }
        else
        {
            num6 = 12;
        }
        switch (sceneFogMode)
        {
            case FogMode.Exponential:
                num6++;
                break;
            case FogMode.ExponentialSquared:
                num6 += 2;
                break;
        }
        CustomGraphicsBlit(source, temporary, _globalFogMaterial, num6);
        return temporary;
    }

    #endregion

    #region LightShaft

    private bool CreateMaterialsIndirectLightShafts()
    {
        _shader = ResourcesManager.instance.GetShader("IndirectLightShuft");
        _material = CheckShaderAndCreateMaterial(_shader, _material);
        return _material != null;
    }

    private void DestroyIndirectLightShafts()
    {
        if (_material != null)
        {
            UnityEngine.Object.DestroyImmediate(_material);
            _material = null;
        }
    }

    private RenderTexture OnRenderImageIndirectLightShafts(RenderTexture source)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
        Vector4 value = Vector4.one * scale;
        _material.SetVector("_Angle", angle);
        _material.SetVector("_Speed", speed);
        _material.SetVector("_Offset", offset);
        _material.SetVector("_Scale", value);
        _material.SetVector("_Alpha", alpha);
        _material.SetVector("_Alpha2", alpha2);
        _material.SetVector("_MaskAlpha", maskAlpha);
        LiveTimelineIndirectLightShuftSettings.ShaftType shaftType = indirectLightShafts.shaftType;
        if (shaftType == LiveTimelineIndirectLightShuftSettings.ShaftType.Default || shaftType != LiveTimelineIndirectLightShuftSettings.ShaftType.Vr03)
        {
            _material.SetTexture("_ShfutTex1", indirectLightShafts._shuftTexture[0]);
            _material.SetTexture("_ShfutTex2", indirectLightShafts._shuftTexture[1]);
            _material.SetTexture("_MaskTex", indirectLightShafts._maskTexture);
            Graphics.Blit(source, temporary, _material, 0);
        }
        else
        {
            _material.SetTexture("_ShfutTex1", indirectLightShafts._shuftTexture[0]);
            _material.SetTexture("_ShfutTex2", indirectLightShafts._shuftTexture[1]);
            _material.SetTexture("_ShfutTex3", indirectLightShafts._shuftTexture[2]);
            _material.SetTexture("_ShfutTex4", indirectLightShafts._shuftTexture[3]);
            _material.SetTexture("_ShfutTex5", indirectLightShafts._shuftTexture[4]);
            _material.SetTexture("_MaskTex", indirectLightShafts._shuftTexture[5]);
            Graphics.Blit(source, temporary, _material, 1);
        }
        return temporary;
    }

    #endregion


    private void fnOverlayBlit(ScreenOverlay overlay, RenderTexture source, RenderTexture destination, Material material, int pass)
    {
        if (overlay.inverseVignette)
        {
            pass++;
        }
        overlay.Update(_propID, material);
        Vector4 value = new Vector4(overlay.postFilmOffsetParam.x, overlay.postFilmOffsetParam.y);
        material.SetFloat(_propID[(int)eProp._PostFilmPower], overlay.postFilmPower);
        material.SetVector(_propID[(int)eProp._PostFilmOffsetParam], value);
        if (overlay.postFilmMode != PostFilmMode.Noise)
        {
            material.SetVector(_propID[(int)eProp._PostFilmOptionParam], overlay.postFilmOptionParam);
        }
        else
        {
            Vector4 postFilmOptionParam = overlay.postFilmOptionParam;
            Director instance = Director.instance;
            if (instance != null && !instance.IsPauseLive())
            {
                _fPrevTime = Time.time;
            }
            postFilmOptionParam.x = Mathf.Floor(_fPrevTime * postFilmOptionParam.x);
            material.SetVector(_propID[(int)eProp._PostFilmOptionParam], postFilmOptionParam);
        }
        material.SetColor(_propID[(int)eProp._PostFilmColor0], overlay.postFilmColor0);
        material.SetColor(_propID[(int)eProp._PostFilmColor1], overlay.postFilmColor1);
        material.SetColor(_propID[(int)eProp._PostFilmColor2], overlay.postFilmColor2);
        material.SetColor(_propID[(int)eProp._PostFilmColor3], overlay.postFilmColor3);
        float num = Mathf.Min((float)source.width / (float)source.height, 1.77777779f) * 0.5625f;
        float value2 = 1f / num;
        material.SetFloat(_propID[(int)eProp._PostFilmAspect], num);
        material.SetFloat(_propID[(int)eProp._PostFilmAspectInv], value2);
        Graphics.Blit(source, destination, material, pass);
    }

    /// <summary>
    /// FilmBlitエフェクトを掛ける
    /// </summary>
    private void PostFilmBlit(RenderTexture source, RenderTexture destination, Material material, int defaultPass, int filmPass1st, int filmPass2nd)
    {
        if (material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        bool valid1 = screenOverlay.isValid();
        bool valid2 = screenOverlay2.isValid();
        bool flag2 = false;
        RenderTexture renderTexture = destination;
        if (valid2 && PostEffectSetting.config_screenOverlay)
        {
            renderTexture = RenderTexture.GetTemporary(source.width, source.height, source.depth);
            flag2 = true;
        }
        if (valid1 && PostEffectSetting.config_screenOverlay)
        {
            fnOverlayBlit(screenOverlay, source, renderTexture, material, filmPass1st);
        }
        else
        {
            Graphics.Blit(source, renderTexture, material, defaultPass);
        }
        if (valid2 && PostEffectSetting.config_screenOverlay)
        {
            fnOverlayBlit(screenOverlay2, renderTexture, destination, material, filmPass2nd);
            if (flag2)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }
    }

    public void SetDofQuality(DofQuality quality)
    {
        _dofQuality = quality & (DofQuality)7; //b111
    }

    public void SetDofBlurType(DofBlurType blurType)
    {
        _dofBlurType = blurType;
    }

    public void SetForegroundSize(float fgSize)
    {
        _dofForegroundSize = fgSize;
    }

    public void SetFgBlurSpread(float fgBlurSpread)
    {
        _dofFgBlurSpread = fgBlurSpread;
    }


    private RenderTexture CreateTempRenderTexture(eRenderTexture rt, int width, int height, int depthBuffer = 0)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer);
        if (temporary != null)
        {
            temporary.filterMode = FilterMode.Bilinear;
            temporary.wrapMode = TextureWrapMode.Clamp;
            _tmpRenderTexture[(int)rt] = temporary;
        }
        return temporary;
    }

    private RenderTexture CreateTempRenderTexture(eRenderTexture rt, RenderTexture source, int depthBuffer = 0)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, depthBuffer, source.format);
        if (temporary != null)
        {
            temporary.filterMode = FilterMode.Bilinear;
            temporary.wrapMode = TextureWrapMode.Clamp;
            _tmpRenderTexture[(int)rt] = temporary;
        }
        return temporary;
    }

    private RenderTexture GetTempRenderTexture(eRenderTexture rt)
    {
        return _tmpRenderTexture[(int)rt];
    }

    private void ReleaseTmpRenderTexture(eRenderTexture rt)
    {
        if (_tmpRenderTexture[(int)rt] != null)
        {
            RenderTexture.ReleaseTemporary(_tmpRenderTexture[(int)rt]);
            _tmpRenderTexture[(int)rt] = null;
        }
    }

    private void ReleaseAllTmpRenderTexture()
    {
        for (int i = 0; i < _tmpRenderTexture.Length; i++)
        {
            if (_tmpRenderTexture[i] != null)
            {
                RenderTexture.ReleaseTemporary(_tmpRenderTexture[i]);
                _tmpRenderTexture[i] = null;
            }
        }
    }

    /// <summary>
    /// Bloomテクスチャを生成
    /// </summary>
    private RenderTexture CreateBloomTexture(RenderTexture source)
    {
        RenderTexture renderTexture = null;
        int width = source.width / 4;
        int height = source.height / 4;
        _bloomBlurParameter.x = _bloomBlurSize * 0.5f;
        _bloomBlurParameter.z = _bloomThreshhold;
        _bloomBlurParameter.w = _bloomIntensity;
        _fastBloomMaterial.SetVector(_propID[(int)eProp._Parameter], _bloomBlurParameter);
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
        temporary.filterMode = FilterMode.Bilinear;
        Graphics.Blit(_monitorTexture, temporary, _fastBloomMaterial, PASS_FASTBLOOM_VERTICALBLUR);
        renderTexture = temporary;
        temporary = RenderTexture.GetTemporary(width, height, 0, source.format);
        temporary.filterMode = FilterMode.Bilinear;
        Graphics.Blit(renderTexture, temporary, _fastBloomMaterial, PASS_FASTBLOOM_HORIZONTALBLUR);
        RenderTexture.ReleaseTemporary(renderTexture);
        renderTexture = temporary;
        _tmpRenderTexture[3] = renderTexture;
        if (_runtimeInfo._runtimeEnableRichBloom)
        {
            _tmpRenderTexture[4] = OnRenderRichBloom(source);
        }
        return renderTexture;
    }

    private void ClearDOFMVFilterKeyword(Material mtrl)
    {
        mtrl.DisableKeyword("FILTER_TYPE_A");
        mtrl.DisableKeyword("FILTER_TYPE_B");
        mtrl.DisableKeyword("FILTER_TYPE_C");
        mtrl.DisableKeyword("DISABLE_BLUR");
    }

    private void SetDOFMVFilterParameter(Material mtrl)
    {
        if (_typeDOFMVFilter != 0 && (bool)_texDOFMVFilter)
        {
            switch (_typeDOFMVFilter)
            {
                case eDofMVFilterType.TYPE_A:
                    mtrl.EnableKeyword("FILTER_TYPE_A");
                    break;
                case eDofMVFilterType.TYPE_B:
                    mtrl.EnableKeyword("FILTER_TYPE_B");
                    break;
                case eDofMVFilterType.TYPE_C:
                    mtrl.EnableKeyword("FILTER_TYPE_C");
                    break;
            }
            if (_disableDOFBlur)
            {
                mtrl.EnableKeyword("DISABLE_BLUR");
            }
            mtrl.SetTexture(_propID[(int)eProp._FilterTex], _texDOFMVFilter);
            mtrl.SetVector(_propID[(int)eProp._filterScale], _scaleDOFMVFilter);
            mtrl.SetVector(_propID[(int)eProp._filterOffset], _offsetDOFMVFilter);
            mtrl.SetFloat(_propID[(int)eProp._filterIntensity], _intensityDOFMVFilter);
        }
    }

    /// <summary>
    /// ブラーを生成
    /// </summary>

    private void BlurBlt(RenderTexture from, RenderTexture to, float spread)
    {
        ClearDOFMVFilterKeyword(_weightedBlurMaterial);
        DofBlurType dofBlurType = ((!_disableDOFBlur) ? _dofBlurType : DofBlurType.Horizon);
        int pass = 0;
        switch (dofBlurType)
        {
            case DofBlurType.Horizon:
                SetDOFMVFilterParameter(_weightedBlurMaterial);
                _dofOffsetsParam.x = spread / _dofWidthOverHeight * _dofOneOverBaseSize;
                _weightedBlurMaterial.SetVector(_propID[(int)eProp.offsets], _dofOffsetsParam);
                Graphics.Blit(from, to, _weightedBlurMaterial, pass);
                break;
            case DofBlurType.Mixed:
                {
                    RenderTexture temporary2 = RenderTexture.GetTemporary(to.width, to.height);
                    _dofOffsetsParam.x = spread / _dofWidthOverHeight * _dofOneOverBaseSize;
                    _dofOffsetsParam.y = 0f;
                    _weightedBlurMaterial.SetVector(_propID[(int)eProp.offsets], _dofOffsetsParam);
                    Graphics.Blit(from, temporary2, _weightedBlurMaterial, pass);
                    SetDOFMVFilterParameter(_weightedBlurMaterial);
                    _dofOffsetsParam.y = spread * _dofHeightBaseSize;
                    _dofOffsetsParam.x = 0f;
                    _weightedBlurMaterial.SetVector(_propID[(int)eProp.offsets], _dofOffsetsParam);
                    Graphics.Blit(temporary2, to, _weightedBlurMaterial, pass);
                    RenderTexture.ReleaseTemporary(temporary2);
                    break;
                }
            case DofBlurType.Disc:
                {
                    float num = (float)to.width / (float)to.height;
                    RenderTexture temporary = RenderTexture.GetTemporary(to.width >> 1, to.height >> 1);
                    float num2 = spread / 50f * 4f + 6f;
                    float value = Mathf.Min(0.05f, num2 / (float)to.height);
                    _weightedBlurMaterial.SetFloat(SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.MaxCoC), value);
                    _weightedBlurMaterial.SetFloat(SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.Aspect), 1f / num);
                    Graphics.Blit(from, temporary, _weightedBlurMaterial, 1);
                    SetDOFMVFilterParameter(_weightedBlurMaterial);
                    Graphics.Blit(temporary, to, _weightedBlurMaterial, 1);
                    RenderTexture.ReleaseTemporary(temporary);
                    break;
                }
        }
        _dofOffsetsParam = Vector3.zero;
        ClearDOFMVFilterKeyword(_weightedBlurMaterial);
    }

    private void DiffusionFillterProcess(RenderTexture source, RenderTexture lowResTex)
    {
        float dividerBasedOnQuality = GetDividerBasedOnQuality();
        float lowResolutionDividerBasedOnQuality = GetLowResolutionDividerBasedOnQuality(dividerBasedOnQuality);
        RenderTexture temporary = RenderTexture.GetTemporary(_rezworkWidth, _rezworkHeight, 0);
        _DiffusionPixelSize.x = _diffusionBlurSize * lowResolutionDividerBasedOnQuality / (float)_rezworkWidth;
        _DiffusionPixelSize.y = _diffusionBlurSize * lowResolutionDividerBasedOnQuality / (float)_rezworkHeight;
        _DiffusionColorParam.x = _bright;
        _DiffusionColorParam.y = _saturation;
        _DiffusionColorParam.z = _contrast;
        _postDiffusionDofBloomMaterial.SetVector(_propID[(int)eProp._PixelSize], _DiffusionPixelSize);
        _postDiffusionDofBloomMaterial.SetVector(_propID[(int)eProp._ColorParam], _DiffusionColorParam);
        _postDiffusionDofBloomMaterial.mainTexture = null;
        _postDiffusionDofBloomMaterial.SetTexture(_propID[(int)eProp._RgbTex], temporary);
        _postDiffusionDofBloomMaterial.mainTexture = source;
        Graphics.Blit(null, temporary, _postDiffusionDofBloomMaterial, PASS_POSTDIFFUSIONDOFBLOOM_VERTICALGAUSS);
        _postDiffusionDofBloomMaterial.mainTexture = temporary;
        Graphics.Blit(null, lowResTex, _postDiffusionDofBloomMaterial, PASS_POSTDIFFUSIONDOFBLOOM_HORIZONGAUSS);
        if (temporary != null)
        {
            RenderTexture.ReleaseTemporary(temporary);
        }
    }

    /// <summary>
    /// DOFパラメータを初期化
    /// </summary>
    private void PrepareDofParam(RenderTexture source, Material mtrl)
    {
        source.filterMode = FilterMode.Bilinear;
        source.wrapMode = TextureWrapMode.Clamp;
        Camera camera = _cacheCamera.camera;
        float num = camera.farClipPlane - camera.nearClipPlane;
        switch (_dofFocalType)
        {
            case DofFocalType.Transform:
                if (_dofFocalTransfrom != null)
                {
                    _dofFocalDistance01 = camera.WorldToViewportPoint(_dofFocalTransfrom.position).z / num;
                }
                else
                {
                    _dofFocalDistance01 = FocalDistance01(_dofFocalPoint);
                }
                break;
            case DofFocalType.Position:
                _dofFocalDistance01 = camera.WorldToViewportPoint(_dofFocalPosition).z / num;
                break;
            case DofFocalType.Point:
                _dofFocalDistance01 = FocalDistance01(_dofFocalPoint);
                break;
        }
        if (_dofFocalDistance01 < 0f)
        {
            _dofFocalDistance01 = 0f;
        }
        _dofFocalStartCurve = _dofFocalDistance01 * _dofSmoothness;
        _dofFocalEndCurve = _dofFocalStartCurve;
        _dofWidthOverHeight = (float)source.width / (float)source.height;
        _dofHeightBaseSize = 1f / (512f * (float)source.height / (float)source.width);
        _dofCurveParams.x = 1f / _dofFocalStartCurve;
        _dofCurveParams.y = 1f / _dofFocalEndCurve;
        _dofCurveParams.z = _focalSize / num * 0.5f + _dofFocalDistance01;
        mtrl.SetFloat(_propID[(int)eProp._bloomDofWeight], _bloomDofWeight);
        mtrl.SetVector(_propID[(int)eProp._CurveParams], _dofCurveParams);
        mtrl.SetVector(_propID[(int)eProp._InvRenderTargetSize], _dofInvRenderTargetSize);
        float dividerBasedOnQuality = GetDividerBasedOnQuality();
        float lowResolutionDividerBasedOnQuality = GetLowResolutionDividerBasedOnQuality(dividerBasedOnQuality);
        _rezworkWidth = (int)((float)source.width * lowResolutionDividerBasedOnQuality);
        _rezworkHeight = (int)((float)source.height * lowResolutionDividerBasedOnQuality);
    }

    /// <summary>
    /// DofBloomを描画する
    /// </summary>
    private void OnRenderImageDofBloom(RenderTexture source, RenderTexture destination)
    {
        PrepareDofParam(source, _postDofBloomMaterial);
        int pass = 2;
        RenderTexture renderTexture = source;
        RenderTexture renderTexture2 = CreateTempRenderTexture(eRenderTexture.LowRes, _rezworkWidth, _rezworkHeight);
        RenderTexture renderTexture3 = null;
        if (_isEnableBloom && _runtimeInfo._runtimeEnableRichBloom)
        {
            renderTexture3 = CreateBloomTexture(source);
        }
        pass = 7;
        renderTexture = CreateTempRenderTexture(eRenderTexture.Source, source);
        Graphics.Blit(source, renderTexture);

        if (_dofQuality == DofQuality.BackgroundAndForeground)
        {
            _postDofBloomMaterial.SetFloat(_propID[(int)eProp._dofForegroundSize], _dofForegroundSize);
            pass = 8;
        }
        Graphics.Blit(source, renderTexture, _postDofBloomMaterial, pass);
        if (_runtimeInfo._runtimeEnableRichBloom)
        {
            int width = renderTexture.width / 2;
            int height = renderTexture.height / 2;
            RenderTextureFormat format = RenderTextureFormat.Default;
            RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, format);
            if (_monitorTexture != null)
            {
                Graphics.Blit(renderTexture, temporary, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOWNSAMPLE);
                Graphics.Blit(temporary, _monitorTexture, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOWNSAMPLE);
                BlurBlt(temporary, renderTexture2, _dofMaxBlurSpread);
            }
            else
            {
                Graphics.Blit(renderTexture, temporary, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOWNSAMPLE);
                Graphics.Blit(temporary, renderTexture2, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOWNSAMPLE);
                BlurBlt(temporary, renderTexture2, _dofMaxBlurSpread);
            }
            RenderTexture.ReleaseTemporary(temporary);
        }
        else if (_monitorTexture != null)
        {
            _monitorTexture.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(renderTexture, _monitorTexture, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOWNSAMPLE);
            BlurBlt(_monitorTexture, renderTexture2, _dofMaxBlurSpread);
        }
        else
        {
            RenderTexture renderTexture4 = CreateTempRenderTexture(eRenderTexture.Downsample, _rezworkWidth, _rezworkHeight);
            Graphics.Blit(renderTexture, renderTexture4, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOWNSAMPLE);
            BlurBlt(renderTexture4, renderTexture2, _dofMaxBlurSpread);
        }
        _postDofBloomMaterial.SetTexture(_propID[(int)eProp._TapLowBackground], renderTexture2);
        if (_isEnableBloom)
        {
            if (renderTexture3 == null)
            {
                renderTexture3 = CreateBloomTexture(renderTexture);
            }
            _postDofBloomMaterial.SetTexture(_propID[(int)eProp._Bloom], renderTexture3);
            if (_runtimeInfo._runtimeEnableRichBloom)
            {
                _postDofBloomMaterial.SetTexture(_propID[(int)eProp._BloomHighRange], _tmpRenderTexture[4]);
                _postDofBloomMaterial.SetFloat(_propID[(int)eProp._bloomHighRangeIntensity], bloomIntensity);
            }
            PostFilmBlit(renderTexture, destination, _postDofBloomMaterial, PASS_POSTDOFBLOOM_DOFBLOOM, PASS_POSTDOFBLOOM_OVERLAY1, PASS_POSTDOFBLOOM_OVERLAY2);
        }
        else
        {
            Graphics.Blit(renderTexture, destination, _postDofBloomMaterial, PASS_POSTDOFBLOOM_APPLYBG);
        }
        ReleaseAllTmpRenderTexture();
    }

    private void OnRenderImageDiffusionDofBloom(RenderTexture source, RenderTexture destination)
    {
        PrepareDofParam(source, _postDiffusionDofBloomMaterial);
        int pass = PASS_POSTDIFFUSIONDOFBLOOM_COC2ALPHA;
        RenderTexture renderTexture = source;
        RenderTexture renderTexture2 = null;
        if (_isEnableBloom && _runtimeInfo._runtimeEnableRichBloom)
        {
            renderTexture2 = CreateBloomTexture(source);
        }
        pass = PASS_POSTDIFFUSIONDOFBLOOM_COCBG_RICH;
        renderTexture = CreateTempRenderTexture(eRenderTexture.Source, source);
        Graphics.Blit(source, renderTexture);

        if (_dofQuality == DofQuality.BackgroundAndForeground)
        {
            _postDiffusionDofBloomMaterial.SetFloat(_propID[(int)eProp._dofForegroundSize], _dofForegroundSize);
            pass = PASS_POSTDIFFUSIONDOFBLOOM_COCBGFG;
        }
        Graphics.Blit(source, renderTexture, _postDiffusionDofBloomMaterial, pass);
        if (_monitorTexture != null)
        {
            _monitorTexture.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(renderTexture, _monitorTexture, _postDiffusionDofBloomMaterial, PASS_POSTDIFFUSIONDOFBLOOM_DOWNSAMPLE);
        }
        RenderTexture renderTexture3 = CreateTempRenderTexture(eRenderTexture.LowRes, _rezworkWidth, _rezworkHeight);
        DiffusionFillterProcess(renderTexture, renderTexture3);
        _postDiffusionDofBloomMaterial.SetTexture(_propID[(int)eProp._TapLowBackground], renderTexture3);
        if (_isEnableBloom)
        {
            if (renderTexture2 == null)
            {
                renderTexture2 = CreateBloomTexture(renderTexture);
            }
            _postDiffusionDofBloomMaterial.SetTexture(_propID[(int)eProp._Bloom], renderTexture2);
            if (_runtimeInfo._runtimeEnableRichBloom)
            {
                _postDiffusionDofBloomMaterial.SetTexture(_propID[(int)eProp._BloomHighRange], _tmpRenderTexture[4]);
                _postDiffusionDofBloomMaterial.SetFloat(_propID[(int)eProp._bloomHighRangeIntensity], bloomIntensity);
            }
            PostFilmBlit(renderTexture, destination, _postDiffusionDofBloomMaterial, PASS_POSTDIFFUSIONDOFBLOOM_BLOOM, PASS_POSTDIFFUSIONDOFBLOOM_OVERLAY1, PASS_POSTDIFFUSIONDOFBLOOM_OVERLAY2);
        }
        else
        {
            Graphics.Blit(renderTexture, destination, _postDiffusionDofBloomMaterial, PASS_POSTDIFFUSIONDOFBLOOM_APPLYBG);
        }
        ReleaseAllTmpRenderTexture();
    }


    #region Bloom

    public void Init(LiveTimelineHdrBloomSettings settings)
    {
        sepBlurSpread = settings.sepBlurSpread;
        bloomIntensity = settings.bloomIntensity;
        bloomBlurIterations = settings.bloomBlurIterations;
        _isEnableBloomHighRange = false;
    }

    public bool CreateMaterialsRichBloom()
    {
        screenBlendShader = ResourcesManager.instance.GetShader("BlendForBloom");
        blurAndFlaresShader = ResourcesManager.instance.GetShader("BlurAndFlares");
        brightPassFilterShader = ResourcesManager.instance.GetShader("BrightPassFilter2");
        screenBlend = CheckShaderAndCreateMaterial(screenBlendShader, screenBlend);
        blurAndFlaresMaterial = CheckShaderAndCreateMaterial(blurAndFlaresShader, blurAndFlaresMaterial);
        brightPassFilterMaterial = CheckShaderAndCreateMaterial(brightPassFilterShader, brightPassFilterMaterial);
        if (screenBlend != null && blurAndFlaresMaterial != null)
        {
            return brightPassFilterMaterial != null;
        }
        return false;
    }

    public void DestroyRichBloom()
    {
        if (brightPassFilterMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(brightPassFilterMaterial);
            brightPassFilterMaterial = null;
        }
        if (blurAndFlaresMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(blurAndFlaresMaterial);
            blurAndFlaresMaterial = null;
        }
        if (screenBlend != null)
        {
            UnityEngine.Object.DestroyImmediate(screenBlend);
            screenBlend = null;
        }
    }

    public RenderTexture OnRenderRichBloom(RenderTexture source)
    {
        doHdr = false;
        if (hdr == HDRBloomMode.Auto)
        {
            doHdr = source.format == RenderTextureFormat.ARGBHalf && GetComponent<Camera>().allowHDR;
        }
        else
        {
            doHdr = hdr == HDRBloomMode.On;
        }
        doHdr = doHdr && supportHDRTextures;
        int width = source.width / 2;
        int height = source.height / 2;
        int num = source.width / 4;
        int num2 = source.height / 4;
        RenderTextureFormat format = (doHdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default);
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, format);
        RenderTexture temporary2 = RenderTexture.GetTemporary(num, num2, 0, format);
        RenderTexture temporary3 = RenderTexture.GetTemporary(num, num2, 0, format);
        Graphics.Blit(source, temporary, screenBlend, 2);
        Graphics.Blit(temporary, temporary3, screenBlend, 2);
        Graphics.Blit(temporary3, temporary2, screenBlend, 3);
        RenderTexture.ReleaseTemporary(temporary3);
        RenderTexture.ReleaseTemporary(temporary);
        RenderTexture temporary4 = RenderTexture.GetTemporary(num, num2, 0, format);
        BloomParameters param = default(BloomParameters);
        param.bloomBlurIterations = bloomBlurIterations;
        param.bloomIntensity = bloomIntensity;
        param.sepBlurSpread = sepBlurSpread;
        param.secondQuarterRezColor = temporary4;
        param.blurAndFlaresMaterial = blurAndFlaresMaterial;
        param.brightPassFilterMaterial = brightPassFilterMaterial;
        param.pass = 0;
        RenderBloom(source, temporary2, num, num2, ref param);
        RenderTexture.ReleaseTemporary(temporary2);
        return param.secondQuarterRezColor;
    }

    /// <summary>
    /// ブルーム
    /// </summary>
    private void RenderBloom(RenderTexture source, RenderTexture sourceQuarterRezColor, int rtW4, int rtH4, ref BloomParameters param)
    {
        RenderTextureFormat format = (doHdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default);
        float num = 1f * (float)source.width / (1f * (float)source.height);
        float num2 = 0.001953125f;
        RenderTexture temporary = RenderTexture.GetTemporary(rtW4, rtH4, 0, format);
        BrightFilter(sourceQuarterRezColor, param.secondQuarterRezColor, param.pass, param);
        int num3 = param.bloomBlurIterations;
        Graphics.SetRenderTarget(temporary);
        GL.Clear(clearDepth: false, clearColor: true, Color.black);
        for (int i = 0; i < num3; i++)
        {
            float num4 = (1f + (float)i * 0.25f) * param.sepBlurSpread;
            float num5 = num4 / num * num2;
            float num6 = num4 * num2;
            RenderTexture temporary2 = RenderTexture.GetTemporary(rtW4, rtH4, 0, format);
            param.blurAndFlaresMaterial.SetVector("_Offsets", new Vector4(num5, num6, 0f - num5, 0f - num6));
            Graphics.Blit(param.secondQuarterRezColor, temporary2, param.blurAndFlaresMaterial, 0);
            RenderTexture.ReleaseTemporary(param.secondQuarterRezColor);
            param.secondQuarterRezColor = temporary2;
            if (i > 0)
            {
                temporary.MarkRestoreExpected();
                Graphics.Blit(param.secondQuarterRezColor, temporary, screenBlend, 4);
            }
        }
        Graphics.SetRenderTarget(param.secondQuarterRezColor);
        GL.Clear(clearDepth: false, clearColor: true, Color.black);
        RenderTexture.ReleaseTemporary(temporary);
    }

    private void BrightFilter(RenderTexture from, RenderTexture to, int pass, BloomParameters param)
    {
        Graphics.Blit(from, to, param.brightPassFilterMaterial, pass);
    }

    #endregion

    #region SunShafts

    /// <summary>
    /// サンシャフト用シェーダを作成する
    /// </summary>
    public bool CreateMaterialsSunShafts()
    {
        sunShaftsShader = ResourcesManager.instance.GetShader("SunShaftsComposite");
        simpleClearShader = ResourcesManager.instance.GetShader("SimpleClear");
        _sunShaftsMaterial = CheckShaderAndCreateMaterial(sunShaftsShader, _sunShaftsMaterial);
        _simpleClearMaterial = CheckShaderAndCreateMaterial(simpleClearShader, _simpleClearMaterial);
        if (_sunShaftsMaterial != null)
        {
            return _simpleClearMaterial != null;
        }
        return false;
    }

    public void Init(LiveTimelineSunshaftsSettings settings)
    {
        _isEnabledSunShaft = false;
        _resolution = settings.resolution;
        _screenBlendMode = settings.screenBlendMode;
        _sunColor = settings.sunColor;
        _sunPower = settings.sunPower;
        _komorebiRate = settings.komorebiRate;
        _sunShaftBlurRadius = settings.blurRadius;
        _sunShaftIntensity = settings.intensity;
        _blackLevel = settings.blackLevel;
        _blurIterations = settings.blurIterations;
        _isEnabledBorderClear = settings.isEnabledBorderClear;
    }

    public void DestroySunShafts()
    {
        if ((bool)_sunShaftsMaterial)
        {
            UnityEngine.Object.DestroyImmediate(_sunShaftsMaterial);
            _sunShaftsMaterial = null;
        }
        if (_simpleClearMaterial != null)
        {
            UnityEngine.Object.DestroyImmediate(_simpleClearMaterial);
            _simpleClearMaterial = null;
        }
    }

    protected void DrawBorder(RenderTexture dest, Material material)
    {
        RenderTexture.active = dest;
        bool flag = true;
        GL.PushMatrix();
        GL.LoadOrtho();
        for (int i = 0; i < material.passCount; i++)
        {
            material.SetPass(i);
            float y;
            float y2;
            if (flag)
            {
                y = 1f;
                y2 = 0f;
            }
            else
            {
                y = 0f;
                y2 = 1f;
            }
            float x = 0f + 1f / ((float)dest.width * 1f);
            float y3 = 0f;
            float y4 = 1f;
            GL.Begin(7);
            GL.TexCoord2(0f, y);
            GL.Vertex3(0f, y3, 0.1f);
            GL.TexCoord2(1f, y);
            GL.Vertex3(x, y3, 0.1f);
            GL.TexCoord2(1f, y2);
            GL.Vertex3(x, y4, 0.1f);
            GL.TexCoord2(0f, y2);
            GL.Vertex3(0f, y4, 0.1f);
            float x2 = 1f - 1f / ((float)dest.width * 1f);
            x = 1f;
            y3 = 0f;
            y4 = 1f;
            GL.TexCoord2(0f, y);
            GL.Vertex3(x2, y3, 0.1f);
            GL.TexCoord2(1f, y);
            GL.Vertex3(x, y3, 0.1f);
            GL.TexCoord2(1f, y2);
            GL.Vertex3(x, y4, 0.1f);
            GL.TexCoord2(0f, y2);
            GL.Vertex3(x2, y4, 0.1f);
            x = 1f;
            y3 = 0f;
            y4 = 0f + 1f / ((float)dest.height * 1f);
            GL.TexCoord2(0f, y);
            GL.Vertex3(0f, y3, 0.1f);
            GL.TexCoord2(1f, y);
            GL.Vertex3(x, y3, 0.1f);
            GL.TexCoord2(1f, y2);
            GL.Vertex3(x, y4, 0.1f);
            GL.TexCoord2(0f, y2);
            GL.Vertex3(0f, y4, 0.1f);
            x = 1f;
            y3 = 1f - 1f / ((float)dest.height * 1f);
            y4 = 1f;
            GL.TexCoord2(0f, y);
            GL.Vertex3(0f, y3, 0.1f);
            GL.TexCoord2(1f, y);
            GL.Vertex3(x, y3, 0.1f);
            GL.TexCoord2(1f, y2);
            GL.Vertex3(x, y4, 0.1f);
            GL.TexCoord2(0f, y2);
            GL.Vertex3(0f, y4, 0.1f);
            GL.End();
        }
        GL.PopMatrix();
    }

    protected void DrawBorderFast(RenderTexture dest, Material material, bool invertY, bool isDrawH, bool isDrawV)
    {
        if (!isDrawV && !isDrawH)
        {
            return;
        }
        RenderTexture.active = dest;
        GL.PushMatrix();
        GL.LoadOrtho();
        for (int i = 0; i < material.passCount; i++)
        {
            material.SetPass(i);
            float y;
            float y2;
            if (invertY)
            {
                y = 1f;
                y2 = 0f;
            }
            else
            {
                y = 0f;
                y2 = 1f;
            }
            GL.Begin(7);
            if (isDrawV)
            {
                float x = 0f + 1f / ((float)dest.width * 1f);
                float y3 = 0f;
                float y4 = 1f;
                GL.TexCoord2(0f, y);
                GL.Vertex3(0f, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(0f, y4, 0.1f);
                float x2 = 1f - 1f / ((float)dest.width * 1f);
                x = 1f;
                y3 = 0f;
                y4 = 1f;
                GL.TexCoord2(0f, y);
                GL.Vertex3(x2, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(x2, y4, 0.1f);
            }
            if (isDrawH)
            {
                float x = 1f;
                float y3 = 0f;
                float y4 = 0f + 1f / ((float)dest.height * 1f);
                GL.TexCoord2(0f, y);
                GL.Vertex3(0f, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(0f, y4, 0.1f);
                x = 1f;
                y3 = 1f - 1f / ((float)dest.height * 1f);
                y4 = 1f;
                GL.TexCoord2(0f, y);
                GL.Vertex3(0f, y3, 0.1f);
                GL.TexCoord2(1f, y);
                GL.Vertex3(x, y3, 0.1f);
                GL.TexCoord2(1f, y2);
                GL.Vertex3(x, y4, 0.1f);
                GL.TexCoord2(0f, y2);
                GL.Vertex3(0f, y4, 0.1f);
            }
            GL.End();
        }
        GL.PopMatrix();
    }

    private RenderTexture OnRenderSunShafts(RenderTexture source)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
        int num = 4;
        if (_resolution == SunShaftsResolution.Normal)
        {
            num = 2;
        }
        else if (_resolution == SunShaftsResolution.High)
        {
            num = 1;
        }
        Vector3 vector = GetComponent<Camera>().WorldToViewportPoint(_sunTransform.position);
        int width = source.width / num;
        int height = source.height / num;
        RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0);
        Vector2 vector2 = new Vector2(Mathf.Min(Mathf.Abs(vector.x - 0.5f) + 0.5f, 1f), Mathf.Min(Mathf.Abs(vector.y - 0.5f) + 0.5f, 1f));
        bool flag = false;
        float num2 = vector2.x * vector2.y;
        int num3 = _blurIterations * 2;
        float num4 = (float)(1 + 6 * num3) / (float)(4 + 6 * num3) * 0.9f;
        if (num2 < num4)
        {
            flag = true;
        }
        int num5 = (flag ? 4 : 0);
        int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.BlurRadius4);
        int propertyID2 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.SunPosition);
        int propertyID3 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.Power);
        int propertyID4 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.SunColor);
        int propertyID5 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.ColorBuffer);
        int propertyID6 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.CenterMultiplex);
        int propertyID7 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.CenterBrightness);
        int propertyID8 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.BlackLevel);
        int propertyID9 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.KomorebiRate);
        int propertyID10 = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.DrawArea);
        _sunShaftsMaterial.SetVector(propertyID, new Vector4(1f, 1f, 0f, 0f) * _sunShaftBlurRadius);
        _sunShaftsMaterial.SetVector(propertyID2, new Vector4(vector.x, vector.y, (vector.x < 0f) ? vector.x : ((vector.x > 1f) ? (1f - vector.x) : 0f), (vector.y < 0f) ? vector.y : ((vector.y > 1f) ? (1f - vector.y) : 0f)));
        _sunShaftsMaterial.SetFloat(propertyID3, _sunPower);
        _sunShaftsMaterial.SetFloat(propertyID6, _centerMultiplex);
        _sunShaftsMaterial.SetFloat(propertyID7, _centerBrightness);
        _sunShaftsMaterial.SetFloat(propertyID8, _blackLevel);
        _sunShaftsMaterial.SetFloat(propertyID9, _komorebiRate);
        _sunShaftsMaterial.SetVector(propertyID10, vector2);
        if (_komorebiRate < float.Epsilon)
        {
            Graphics.Blit(source, temporary2, _sunShaftsMaterial, num5);
        }
        else
        {
            Graphics.Blit(source, temporary2, _sunShaftsMaterial, 8 + (flag ? 1 : 0));
        }
        if (_isEnabledBorderClear)
        {
            if (flag)
            {
                bool invertY = source.texelSize.y < 0f;
                bool isDrawH = vector.y < 0f || 1f < vector.y;
                bool isDrawV = vector.x < 0f || 1f < vector.x;
                DrawBorderFast(temporary2, _simpleClearMaterial, invertY, isDrawH, isDrawV);
            }
            else
            {
                DrawBorder(temporary2, _simpleClearMaterial);
            }
        }
        _blurIterations = Mathf.Clamp(_blurIterations, 1, 4);
        float num6 = _sunShaftBlurRadius * 0.00130208337f;
        _sunShaftsMaterial.SetVector(propertyID, new Vector4(num6, num6, 0f, 0f));
        for (int i = 0; i < _blurIterations; i++)
        {
            RenderTexture temporary3 = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(temporary2, temporary3, _sunShaftsMaterial, num5 + 1);
            RenderTexture.ReleaseTemporary(temporary2);
            num6 = _sunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
            _sunShaftsMaterial.SetVector(propertyID, new Vector4(num6, num6, 0f, 0f));
            temporary2 = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(temporary3, temporary2, _sunShaftsMaterial, num5 + 1);
            RenderTexture.ReleaseTemporary(temporary3);
            num6 = _sunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
            _sunShaftsMaterial.SetVector(propertyID, new Vector4(num6, num6, 0f, 0f));
        }
        _sunShaftsMaterial.SetVector(propertyID4, new Vector4(_sunColor.r, _sunColor.g, _sunColor.b, _sunColor.a) * _sunShaftIntensity);
        _sunShaftsMaterial.SetTexture(propertyID5, temporary2);
        int pass = num5 + ((_screenBlendMode == ShaftsScreenBlendMode.Screen) ? 2 : 3);
        Graphics.Blit(source, temporary, _sunShaftsMaterial, pass);
        RenderTexture.ReleaseTemporary(temporary2);
        return temporary;
    }

    #endregion

    #region TilfShift

    private bool CreateMaterialsTilfShift()
    {
        _tiltShift.shader = ResourcesManager.instance.GetShader("TiltShiftHdrLensBlur");
        _tiltShift.mtrl = CheckShaderAndCreateMaterial(_tiltShift.shader, _tiltShift.mtrl);
        return _tiltShift.mtrl != null;
    }

    private void DestroyTilfShift()
    {
        if (_tiltShift != null)
        {
            _tiltShift.Destroy();
        }
    }

    private RenderTexture OnRenderImageTiltShift(RenderTexture source)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
        source.filterMode = FilterMode.Bilinear;
        _tiltShift.mtrl.SetFloat("_BlurSize", (_tiltShift.maxBlurSize < 0f) ? 0f : _tiltShift.maxBlurSize);
        _tiltShift.mtrl.SetFloat("_BlurArea", _tiltShift.blurArea);
        _tiltShift.param.x = _tiltShift.offset.x;
        _tiltShift.param.y = _tiltShift.offset.y;
        _tiltShift.param.z = Mathf.Sin(_tiltShift.roll * ((float)Math.PI / 180f));
        _tiltShift.param.w = Mathf.Cos(_tiltShift.roll * ((float)Math.PI / 180f));
        _tiltShift.mtrl.SetVector("_Params", _tiltShift.param);
        RenderTexture renderTexture = temporary;
        if ((float)_tiltShift.downsample > 0f)
        {
            renderTexture = RenderTexture.GetTemporary(source.width >> _tiltShift.downsample, source.height >> _tiltShift.downsample, 0, source.format);
            renderTexture.filterMode = FilterMode.Bilinear;
        }
        int quality = (int)_tiltShift.quality;
        quality *= 2;
        switch (_tiltShift.mode)
        {
            case LiveTimelineKeyTiltShiftData.Mode.IrisMode:
                quality++;
                break;
            case LiveTimelineKeyTiltShiftData.Mode.Circle:
                quality = (int)(7 + _tiltShift.quality);
                _tiltShift.mtrl.SetVector("_BlurDir", _tiltShift.blurParam);
                break;
        }
        Graphics.Blit(source, renderTexture, _tiltShift.mtrl, quality);
        if (_tiltShift.downsample > 0)
        {
            _tiltShift.mtrl.SetTexture("_Blurred", renderTexture);
            int pass = 6;
            if (_tiltShift.quality != 0)
            {
                pass = 10;
                switch (_tiltShift.mode)
                {
                    case LiveTimelineKeyTiltShiftData.Mode.IrisMode:
                        pass = 9;
                        break;
                    case LiveTimelineKeyTiltShiftData.Mode.TiltShiftMode:
                        pass = 10;
                        break;
                    case LiveTimelineKeyTiltShiftData.Mode.Circle:
                        pass = 11;
                        break;
                }
            }
            Graphics.Blit(source, temporary, _tiltShift.mtrl, pass);
        }
        if (renderTexture != temporary)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        return temporary;
    }

    #endregion

    #region antialiasing

    public Shader _shaderFXAAIII;
    private Material _materialFXAAIII;

    public float edgeThresholdMin = 0.05f;
    public float edgeThreshold = 0.2f;
    public float edgeSharpness = 4.0f;

    public void CreateMaterialsAntialiasing()
    {
        _shaderFXAAIII = Shader.Find("Hidden/FXAA III (Console)");

        _materialFXAAIII = CheckShaderAndCreateMaterial(_shaderFXAAIII, _materialFXAAIII);
    }

    /// <summary>
    /// アンチエイリアスを掛ける
    /// </summary>
    public RenderTexture OnRenderAntialiasing(RenderTexture source)
    {
        RenderTexture destination = RenderTexture.GetTemporary(source.width, source.height);

        if (_materialFXAAIII != null)
        {
            _materialFXAAIII.SetFloat("_EdgeThresholdMin", edgeThresholdMin);
            _materialFXAAIII.SetFloat("_EdgeThreshold", edgeThreshold);
            _materialFXAAIII.SetFloat("_EdgeSharpness", edgeSharpness);

            Graphics.Blit(source, destination, _materialFXAAIII);
        }
        else
        {
            Graphics.Blit(source, destination);
        }

        return destination;
    }
    #endregion
}
