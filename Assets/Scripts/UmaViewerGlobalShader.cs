using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UmaViewerGlobalShader : MonoBehaviour
{
    //仅用于设置游戏开局shader全局变量的初始值，若想即时调整请使用Editor内部工具
    public float _Global_MaxDensity = 1.0f;
    public float _Global_MaxHeight = 10.0f;
    public float _GlobalOutlineOffset = 1f;
    public float _GlobalOutlineWidth = 1.0f;
    public float _GlobalCameraFov = 0.1f;
    public float _CylinderBlend = 0.0f;
    public Color _GlobalToonColor = new Color(1, 1, 1, 0);
    public Color _Global_FogColor;
    public Color _GlobalDirtRimSpecularColor;
    public Color _GlobalDirtToonColor;
    public Color _GlobalRimColor = new Color(1, 1, 1, 0);
    public Color _GlobalDirtColor;
    public Color _Global_LightmapColor = Color.white;
    public Color _Global_LightmapDensityAddColor = Color.clear;
    public Color _Global_LightmapModulateColor = Color.white;
    public Color _RimColor2;
    List<Vector4> _MainParam = new List<Vector4>();
    public Vector4 _MainParam_0;
    public Vector4 _MainParam_1;
    List<Vector4> _HighParam1 = new List<Vector4>();
    public Vector4 _HighParam1_0 = new Vector4(0, 0, 0, 1);
    public Vector4 _HighParam1_1 = new Vector4(0, 0, 0, 1);
    public Vector4 _HighParam1_2;
    List<Vector4> _HighParam2 = new List<Vector4>();
    public Vector4 _HighParam2_0 = new Vector4(0, 0, 0, 1);
    public Vector4 _HighParam2_1 = new Vector4(0, 0, 0, 1);

    List<Vector4> _ColorArray = new List<Vector4>();
    public Color _ColorArray_0;
    public Color _ColorArray_1;
    public Color _ColorArray_2;
    public Color _ColorArray_3;
    public Color _ColorArray_4;
    public Color _ColorArray_5;
    public Color _ColorArray_6;
    public Color _ColorArray_7;
    public Color _ColorArray_8;
    public Color _ColorArray_9;

    List<float> _DirtRate = new List<float>();
    public float _DirtRate_0;
    public float _DirtRate_1;
    public float _DirtRate_2;

    public Vector4 _Global_FogMinDistance;
    public Vector4 _Global_FogLength;

    public float _RimHorizonOffset;
    public float _UVEmissivePower;
    public Color _AmbientColor;

    public Vector2 scrollPosition = Vector2.zero;

    void setGlobal()
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
        #if UNITY_ANDROID
        Shader.SetGlobalFloat("_GlobalFarClipLog", 1f);
        Shader.SetGlobalFloat("_GlobalVertexDepthLinear", 1f);
        #endif
    }


    // Start is called before the first frame update
    void Start()
    {
        setGlobal();
    }

    private void FixedUpdate()
    {
        //Used to calculate the correct outline
        //Outline need more adjust in live
        var umaContainer = UmaViewerBuilder.Instance.CurrentUMAContainer;
        if (umaContainer != null && umaContainer.UpBodyBone)
        {
            var upBone = umaContainer.UpBodyBone;
            var aniCamera = UmaViewerBuilder.Instance.AnimationCamera;
            var camera = aniCamera.enabled ? aniCamera : Camera.main;
            var distance = Vector3.Distance(camera.transform.position, upBone.transform.position);
            var outlineWidth = (umaContainer.IsMini ? 20f : 40.0f) * (distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            Shader.SetGlobalFloat("_GlobalCameraFov", outlineWidth);
        }
        else
        {
            Shader.SetGlobalFloat("_GlobalCameraFov", 30);
        }
       
    }

   
}
