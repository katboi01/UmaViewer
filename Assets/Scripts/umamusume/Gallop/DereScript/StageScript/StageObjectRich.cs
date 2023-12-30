using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class StageObjectRich : MonoBehaviour
{
    private const string SHADER_NAME = "StageObjectRich";

    private const int INVALID_ID = -1;

    private int _shdId_LightDirection = INVALID_ID;

    private int _shdId_RimRate = INVALID_ID;

    private int _shdId_RimSpecRate = INVALID_ID;

    private int _shdId_RimShadowRate = INVALID_ID;

    private Renderer[] _arrRenderer;

    private MaterialPropertyBlock _mtrlProp;

    private bool _bDirty;

    private Vector3 _lightDirection = Vector3.up;

    private float _rimRate;

    private float _rimSpecRate;

    private float _rimShadowRate;

    private void InitShaderPropID()
    {
        _shdId_LightDirection = Shader.PropertyToID("_GlobalLightDir");
        _shdId_RimRate = Shader.PropertyToID("_RimRate");
        _shdId_RimSpecRate = Shader.PropertyToID("_RimSpecRate");
        _shdId_RimShadowRate = Shader.PropertyToID("_GlobalRimShadowRate");
    }

    private void Start()
    {
        if (!base.enabled)
        {
            return;
        }
        List<Renderer> list = new List<Renderer>();
        Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (componentsInChildren[i].sharedMaterial.shader.name.Contains(SHADER_NAME))
            {
                list.Add(componentsInChildren[i]);
            }
        }
        _arrRenderer = list.ToArray();
        _mtrlProp = new MaterialPropertyBlock();
        InitShaderPropID();
    }

    private void LateUpdate()
    {
        if (_bDirty)
        {
            for (int i = 0; i < _arrRenderer.Length; i++)
            {
                _arrRenderer[i].SetPropertyBlock(_mtrlProp);
            }
            _bDirty = false;
        }
    }

    public void UpdateInfo(ref EnvironmentGlobalLightUpdateInfo updateInfo)
    {
        if (_mtrlProp != null)
        {
            if (_lightDirection != updateInfo.lightDirection)
            {
                _lightDirection = updateInfo.lightDirection;
                _mtrlProp.SetVector(_shdId_LightDirection, _lightDirection);
                _bDirty = true;
            }
            if (_rimRate != updateInfo.globalRimRate)
            {
                _rimRate = updateInfo.globalRimRate;
                _mtrlProp.SetFloat(_shdId_RimRate, _rimRate);
                _bDirty = true;
            }
            if (_rimSpecRate != updateInfo.globalRimSpecularRate)
            {
                _rimSpecRate = updateInfo.globalRimSpecularRate;
                _mtrlProp.SetFloat(_shdId_RimSpecRate, _rimSpecRate);
                _bDirty = true;
            }
            if (_rimShadowRate != updateInfo.globalRimShadowRate)
            {
                _rimShadowRate = updateInfo.globalRimShadowRate;
                _mtrlProp.SetFloat(_shdId_RimShadowRate, _rimShadowRate);
                _bDirty = true;
            }
        }
    }
}
