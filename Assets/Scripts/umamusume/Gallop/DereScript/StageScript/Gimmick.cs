using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class Gimmick : MonoBehaviour
{
    protected Renderer _renderer;

    protected MaterialPropertyBlock _mtrlPropBlock;

    protected List<Material> _lstMaterial = new List<Material>();

    protected bool _isInit;

    protected bool _isPause;

    protected bool _isDirty;

    protected LiveTimelineKeyShaderControlData.eBehaviorFlag _behaviorFlag = LiveTimelineKeyShaderControlData.eBehaviorFlag.Invalid;

    public MaterialPropertyBlock mtrlPropBlock => _mtrlPropBlock;

    public List<Material> lstMaterial => _lstMaterial;

    public void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mtrlPropBlock = new MaterialPropertyBlock();
    }

    public void LateUpdate()
    {
        if (_renderer != null && _isDirty)
        {
            _renderer.SetPropertyBlock(_mtrlPropBlock);
            _isDirty = false;
        }
    }

    public void SetShaderBehavior(LiveTimelineKeyShaderControlData.eBehaviorFlag behaviorFlags)
    {
        if (behaviorFlags == _behaviorFlag)
        {
            return;
        }
        if ((behaviorFlags & LiveTimelineKeyShaderControlData.eBehaviorFlag.Luminous) != 0)
        {
            foreach (Material item in _lstMaterial)
            {
                item.EnableKeyword("ENABLE_LUMINOUS");
            }
        }
        else
        {
            foreach (Material item2 in _lstMaterial)
            {
                item2.DisableKeyword("ENABLE_LUMINOUS");
            }
        }
        _behaviorFlag = behaviorFlags;
    }

    public bool Initialize()
    {
        OnCollectMaterial();
        if (_lstMaterial.Count > 0)
        {
            SetShaderBehavior(LiveTimelineKeyShaderControlData.eBehaviorFlag.Luminous);
            _isInit = OnInitialize();
            return _isInit;
        }
        return false;
    }

    protected virtual void OnCollectMaterial()
    {
        if (_renderer != null)
        {
            Material[] sharedMaterials = _renderer.sharedMaterials;
            foreach (Material item in sharedMaterials)
            {
                _lstMaterial.Add(item);
            }
        }
    }
    protected virtual bool OnInitialize()
    {
        return _renderer != null;
    }
}
