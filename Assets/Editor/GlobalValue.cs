using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GlobalValue : EditorWindow
{
    float _Global_MaxDensity = 1.0f;
    float _Global_MaxHeight = 10.0f;
    float _GlobalOutlineOffset = 0.0f;
    float _GlobalOutlineWidth = 1.0f;
    float _GlobalCameraFov = 0.0f;
    float _CylinderBlend = 0.0f;
    Color _GlobalToonColor = new Color(1, 1, 1, 0);
    Color _Global_FogColor;
    Color _GlobalDirtRimSpecularColor;
    Color _GlobalDirtToonColor;
    Color _GlobalRimColor = new Color(1, 1, 1, 0);
    Color _GlobalDirtColor;
    Color _Global_LightmapColor = Color.white;
    Color _Global_LightmapDensityAddColor = Color.clear;
    Color _Global_LightmapModulateColor = Color.white;
    Color _RimColor2;
    List<Vector4> _MainParam = new List<Vector4>();
    Vector4 _MainParam_0;
    Vector4 _MainParam_1;
    List<Vector4> _HighParam1 = new List<Vector4>();
    Vector4 _HighParam1_0 = new Vector4(0, 0, 0, 1);
    Vector4 _HighParam1_1 = new Vector4(0, 0, 0, 1);
    Vector4 _HighParam1_2;
    List<Vector4> _HighParam2 = new List<Vector4>();
    Vector4 _HighParam2_0 = new Vector4(0, 0, 0, 1);
    Vector4 _HighParam2_1 = new Vector4(0, 0, 0, 1);

    List<Vector4> _ColorArray = new List<Vector4>();
    Vector4 _ColorArray_0;
    Vector4 _ColorArray_1;
    Vector4 _ColorArray_2;
    Vector4 _ColorArray_3;
    Vector4 _ColorArray_4;
    Vector4 _ColorArray_5;
    Vector4 _ColorArray_6;
    Vector4 _ColorArray_7;
    Vector4 _ColorArray_8;
    Vector4 _ColorArray_9;

    List<float> _DirtRate = new List<float>();
    float _DirtRate_0;
    float _DirtRate_1;
    float _DirtRate_2;


    Vector4 _Global_FogMinDistance;
    Vector4 _Global_FogLength;

    float _RimHorizonOffset;
    float _UVEmissivePower;

    Color _AmbientColor;

    Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Global/Value Set")]
    public static void ValueWindow()
    {
        EditorWindow.GetWindow<GlobalValue>("SetValue");
    }

    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true);
        GUILayout.Label("Set Your Global Value!", EditorStyles.boldLabel);
        //Vector
        _Global_FogMinDistance = EditorGUILayout.Vector4Field("雾气出现距离", _Global_FogMinDistance);
        _Global_FogLength = EditorGUILayout.Vector4Field("雾气长度", _Global_FogLength);
        //Float
        _Global_MaxDensity = EditorGUILayout.Slider("最低能见度", _Global_MaxDensity, 0, 1);
        _Global_MaxHeight = EditorGUILayout.Slider("最高可见高度", _Global_MaxHeight, 0, 10);
        _GlobalOutlineOffset = EditorGUILayout.Slider("_GlobalOutlineOffset", _GlobalOutlineOffset, -100, 100);
        _GlobalOutlineWidth = EditorGUILayout.Slider("全局轮廓线宽度", _GlobalOutlineWidth, 0, 100);
        _GlobalCameraFov = EditorGUILayout.Slider("_GlobalCameraFov", _GlobalCameraFov, 0, 100);
        _CylinderBlend = EditorGUILayout.Slider("_CylinderBlend", _CylinderBlend, 0, 100);
        _RimHorizonOffset = EditorGUILayout.Slider("_RimHorizonOffset", _RimHorizonOffset, 0, 1);
        _UVEmissivePower = EditorGUILayout.Slider("_UVEmissivePower", _UVEmissivePower, 0, 10);
        //Color
        _GlobalToonColor = EditorGUILayout.ColorField("阴影颜色", _GlobalToonColor);
        _Global_FogColor = EditorGUILayout.ColorField("_Global_FogColor", _Global_FogColor);
        _GlobalDirtRimSpecularColor = EditorGUILayout.ColorField("_GlobalDirtRimSpecularColor", _GlobalDirtRimSpecularColor);
        _GlobalDirtToonColor = EditorGUILayout.ColorField("_GlobalDirtToonColor", _GlobalDirtToonColor);
        _GlobalRimColor = EditorGUILayout.ColorField("边缘光颜色", _GlobalRimColor);
        _GlobalDirtColor = EditorGUILayout.ColorField("_GlobalDirtColor", _GlobalDirtColor);
        _Global_LightmapColor = EditorGUILayout.ColorField("_Global_LightmapColor", _Global_LightmapColor);
        _Global_LightmapDensityAddColor = EditorGUILayout.ColorField("_Global_LightmapDensityAddColor", _Global_LightmapDensityAddColor);
        _Global_LightmapModulateColor = EditorGUILayout.ColorField("_Global_LightmapModulateColor", _Global_LightmapModulateColor);
        _RimColor2 = EditorGUILayout.ColorField("_RimColor2", _RimColor2);

        _AmbientColor = EditorGUILayout.ColorField("_AmbientColor", _AmbientColor);
        //Vector Array
        _MainParam_0 = EditorGUILayout.Vector4Field("左眼偏移", _MainParam_0);
        _MainParam_1 = EditorGUILayout.Vector4Field("右眼偏移", _MainParam_1);

        _HighParam1_0 = EditorGUILayout.Vector4Field("左眼上高光（w值为强度）", _HighParam1_0);
        _HighParam1_1 = EditorGUILayout.Vector4Field("左眼下高光", _HighParam1_1);
        _HighParam1_2 = EditorGUILayout.Vector4Field("双眼闪烁效果", _HighParam1_2);

        _HighParam2_0 = EditorGUILayout.Vector4Field("右眼上高光", _HighParam2_0);
        _HighParam2_1 = EditorGUILayout.Vector4Field("右眼下高光", _HighParam2_1);

        _ColorArray_0 = EditorGUILayout.ColorField("_ColorArray_0", _ColorArray_0);
        _ColorArray_1 = EditorGUILayout.ColorField("_ColorArray_1", _ColorArray_1);
        _ColorArray_2 = EditorGUILayout.ColorField("_ColorArray_2", _ColorArray_2);
        _ColorArray_3 = EditorGUILayout.ColorField("_ColorArray_3", _ColorArray_3);
        _ColorArray_4 = EditorGUILayout.ColorField("_ColorArray_4", _ColorArray_4);
        _ColorArray_5 = EditorGUILayout.ColorField("_ColorArray_5", _ColorArray_5);
        _ColorArray_6 = EditorGUILayout.ColorField("_ColorArray_6", _ColorArray_6);
        _ColorArray_7 = EditorGUILayout.ColorField("_ColorArray_7", _ColorArray_7);
        _ColorArray_8 = EditorGUILayout.ColorField("_ColorArray_8", _ColorArray_8);
        _ColorArray_9 = EditorGUILayout.ColorField("_ColorArray_9", _ColorArray_9);



        //Float Array
        _DirtRate_0 = EditorGUILayout.Slider("_DirtRate_0", _DirtRate_0, 0, 1);
        _DirtRate_1 = EditorGUILayout.Slider("_DirtRate_1", _DirtRate_1, 0, 1);
        _DirtRate_2 = EditorGUILayout.Slider("_DirtRate_2", _DirtRate_2, 0, 1);
        GUILayout.EndScrollView();
    }

    void Update()
    {
        //Color
        Shader.SetGlobalColor("_GlobalToonColor", _GlobalToonColor);
        Shader.SetGlobalColor("_Global_FogColor", _Global_FogColor);
        Shader.SetGlobalColor("_GlobalDirtRimSpecularColor", _GlobalDirtRimSpecularColor);
        Shader.SetGlobalColor("_GlobalDirtToonColor", _GlobalDirtToonColor);
        Shader.SetGlobalColor("_GlobalRimColor", _GlobalRimColor);
        Shader.SetGlobalColor("_GlobalDirtColor", _GlobalDirtColor);
        Shader.SetGlobalColor("_Global_LightmapColor", _Global_LightmapColor);
        Shader.SetGlobalColor("_Global_LightmapDensityAddColor", _Global_LightmapDensityAddColor);
        Shader.SetGlobalColor("_Global_LightmapModulateColor", _Global_LightmapModulateColor);
        Shader.SetGlobalColor("_RimColor2", _RimColor2);
        //Float
        Shader.SetGlobalFloat("_Global_MaxDensity", _Global_MaxDensity);
        Shader.SetGlobalFloat("_Global_MaxHeight", _Global_MaxHeight);
        Shader.SetGlobalFloat("_GlobalOutlineOffset", _GlobalOutlineOffset);
        Shader.SetGlobalFloat("_GlobalOutlineWidth", _GlobalOutlineWidth);
        Shader.SetGlobalFloat("_GlobalCameraFov", _GlobalCameraFov);
        Shader.SetGlobalFloat("_CylinderBlend", _CylinderBlend);
        Shader.SetGlobalFloat("_RimHorizonOffset", _RimHorizonOffset);
        Shader.SetGlobalFloat("_UVEmissivePower", _UVEmissivePower);
        //Vector Array
        _MainParam.Clear();
        _MainParam.Add(_MainParam_0);
        _MainParam.Add(_MainParam_1);
        Shader.SetGlobalVectorArray("_MainParam", _MainParam);

        _HighParam1.Clear();
        _HighParam1.Add(_HighParam1_0);
        _HighParam1.Add(_HighParam1_1);
        _HighParam1.Add(_HighParam1_2);
        Shader.SetGlobalVectorArray("_HighParam1", _HighParam1);

        _HighParam2.Clear();
        _HighParam2.Add(_HighParam2_0);
        _HighParam2.Add(_HighParam2_1);
        Shader.SetGlobalVectorArray("_HighParam2", _HighParam2);

        _ColorArray.Clear();
        _ColorArray.Add(_ColorArray_0);
        _ColorArray.Add(_ColorArray_1);
        _ColorArray.Add(_ColorArray_2);
        _ColorArray.Add(_ColorArray_3);
        _ColorArray.Add(_ColorArray_4);
        _ColorArray.Add(_ColorArray_5);
        _ColorArray.Add(_ColorArray_6);
        _ColorArray.Add(_ColorArray_7);
        _ColorArray.Add(_ColorArray_8);
        _ColorArray.Add(_ColorArray_9);
        Shader.SetGlobalVectorArray("_ColorArray", _ColorArray);

        Shader.SetGlobalVector("_Global_FogMinDistance", _Global_FogMinDistance);
        Shader.SetGlobalVector("_Global_FogLength", _Global_FogLength);

        _DirtRate.Clear();
        _DirtRate.Add(_DirtRate_0);
        _DirtRate.Add(_DirtRate_1);
        _DirtRate.Add(_DirtRate_2);
        Shader.SetGlobalFloatArray("_DirtRate", _DirtRate);

        Shader.SetGlobalColor("_AmbientColor", _AmbientColor);
    }
}
