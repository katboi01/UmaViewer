using System;
using UnityEngine;

public class SharedShaderParam
{
    public enum ShaderProperty
    {
        MulColor0,
        MulColor1,
        ColorPower,
        ColorClampPower,
        AmbientColor,
        CharaColor,
        BrightColor,
        BaseColor,
        PropsColor,
        BorderLightDir,
        LocalTimer,
        RandomValue,
        BlurRadius4,
        SunPosition,
        Power,
        SunColor,
        ColorBuffer,
        CenterMultiplex,
        CenterBrightness,
        BlackLevel,
        KomorebiRate,
        BlendFactor,
        MonitorWidth,
        MonitorHeight,
        UVAdjust,
        MonitorColorPower,
        ColorPalette,
        GlobalEnvColor,
        GlobalEnvTex,
        GlobalEnvRate,
        GlobalLightDir,
        GlobalRimRate,
        GlobalRimShadowRate,
        GlobalSpecRate,
        GlobalToonRate,
        RimColorMulti,
        ReflectionTex,
        ReflectionRate,
        HeightLightParam,
        HeightLightColor,
        FrustumCornersWS,
        CameraWS,
        HeightParams,
        DistanceParams,
        FogColor,
        SceneFogParams,
        MainTex,
        Aspect,
        MaxCoC,
        DistanceOption,
        HeightOption,
        MobColor,
        GroupMatrices,
        BlendRate,
        DrawArea,
        DisplayMirror,
        TextureMappingMatrix,
        LocalTime,
        RefractionTex,
        CameraProjector,
        WaveOffset,
        SunMirrorPosition,
        Color,
        MainTexAlpha,
        BlendSrc,
        BlendDst,
        ZTest
    }

    public static readonly int INVALID_PROPERTY_ID = -1;

    private static readonly int sMaxShaderProperty = Enum.GetNames(typeof(ShaderProperty)).Length;

    private static readonly string[] sShaderPropertyName = new string[sMaxShaderProperty];

    private static bool _isStart = false;

    private static int[] _PropertyID = new int[sMaxShaderProperty];

    public static readonly Vector2 _vector2ZeroZero = new Vector2(0f, 0f);

    public static readonly Vector2 _vector2OneOne = new Vector2(1f, 1f);

    private static SharedShaderParam _instance;

    public static SharedShaderParam instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SharedShaderParam();
                _instance.Start();
            }
            return _instance;
        }
    }

    public string getPropertyName(ShaderProperty property)
    {
        return sShaderPropertyName[(int)property];
    }

    public int getPropertyID(ShaderProperty property)
    {
        return _PropertyID[(int)property];
    }

    public void Start()
    {
        if (!_isStart)
        {
            for (int i = 0; i < sMaxShaderProperty; i++)
            {
                string[] array = sShaderPropertyName;
                int num = i;
                ShaderProperty shaderProperty = (ShaderProperty)i;
                array[num] = "_" + shaderProperty;
                _PropertyID[i] = Shader.PropertyToID(sShaderPropertyName[i]);
            }
            _isStart = true;
        }
    }
}
