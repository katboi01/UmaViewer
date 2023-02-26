// TextureSplatting
using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureSplatting : Gimmick
{
    [Serializable]
    public class Channel
    {
        public enum eBlendMode
        {
            NotUse,
            Lerp,
            Additive,
            Multiply
        }

        public const int INVALID_PROP_ID = -1;

        [SerializeField]
        private eBlendMode _blendMode;

        [SerializeField]
        private bool _enableScroll;

        [SerializeField]
        private Vector2 _scrollFactor = Vector2.zero;

        [NonSerialized]
        private int _propId_UVInfo = -1;

        [NonSerialized]
        private Vector4 _uvInfo = Vector4.zero;

        public eBlendMode blendMode => _blendMode;

        public bool enableScroll => _enableScroll;

        public Vector2 scrollFactor => _scrollFactor;

        public Vector4 uvInfo => _uvInfo;

        public int propId_UVInfo => _propId_UVInfo;

        public void Initialize(string strChannel, Material mtrl)
        {
            string name = $"_TileTex{strChannel}_ST";
            _propId_UVInfo = Shader.PropertyToID(name);
            _uvInfo = mtrl.GetVector(_propId_UVInfo);
            _uvInfo.z = 0f;
            _uvInfo.w = 0f;
            Dictionary<eBlendMode, string> dictionary = new Dictionary<eBlendMode, string>();
            dictionary.Add(eBlendMode.NotUse, $"NONE_{strChannel}");
            dictionary.Add(eBlendMode.Lerp, $"LERP_{strChannel}");
            dictionary.Add(eBlendMode.Additive, $"ADD_{strChannel}");
            dictionary.Add(eBlendMode.Multiply, $"MUL_{strChannel}");
            foreach (KeyValuePair<eBlendMode, string> item in dictionary)
            {
                mtrl.DisableKeyword(item.Value);
            }
            string keyword = dictionary[_blendMode];
            mtrl.EnableKeyword(keyword);
        }

        public void Update(float frame)
        {
            if (enableScroll && blendMode != 0)
            {
                _uvInfo.z += scrollFactor.x / frame;
                _uvInfo.w += scrollFactor.y / frame;
                _uvInfo.z %= 1f;
                _uvInfo.w %= 1f;
            }
            else
            {
                _uvInfo.z = 0f;
                _uvInfo.w = 0f;
            }
        }
    }

    public readonly string SHADER_NAME = "Cygames/3DLive/Stage/StageSplatting";

    [SerializeField]
    private Material _material;

    [SerializeField]
    private bool _useUV2;

    [SerializeField]
    private Channel _channelR;

    [SerializeField]
    private Channel _channelG;

    [SerializeField]
    private Channel _channelB;

    private Director _director;

    public Material material => _material;

    public bool useUV2 => _useUV2;

    public Channel channelR => _channelR;

    public Channel channelG => _channelG;

    public Channel channelB => _channelB;

    private void Update()
    {
        if (_isInit && !_director.IsPauseLive())
        {
            float frame = 1f / Time.deltaTime;
            if (_channelR != null)
            {
                _channelR.Update(frame);
                _mtrlPropBlock.SetVector(_channelR.propId_UVInfo, _channelR.uvInfo);
            }
            if (_channelG != null)
            {
                _channelG.Update(frame);
                _mtrlPropBlock.SetVector(_channelG.propId_UVInfo, _channelG.uvInfo);
            }
            if (_channelB != null)
            {
                _channelB.Update(frame);
                _mtrlPropBlock.SetVector(_channelB.propId_UVInfo, _channelB.uvInfo);
            }
            _isDirty = true;
        }
    }

    protected override void OnCollectMaterial()
    {
        _lstMaterial.Clear();
        if (_renderer == null)
        {
            return;
        }
        Material[] sharedMaterials = _renderer.sharedMaterials;
        foreach (Material material in sharedMaterials)
        {
            if (material.shader.name.Contains(SHADER_NAME))
            {
                _lstMaterial.Add(material);
            }
        }
    }

    protected override bool OnInitialize()
    {
        _director = Director.instance;
        if (_material == null)
        {
            _material = _renderer.sharedMaterial;
        }
        Action<Channel, string> obj = delegate (Channel channel, string strChannel)
        {
            if (channel != null && channel.blendMode != 0)
            {
                channel.Initialize(strChannel, _material);
            }
        };
        obj(_channelR, "R");
        obj(_channelG, "G");
        obj(_channelB, "B");
        if (_useUV2)
        {
            _material.EnableKeyword("USE_UV2");
        }
        else
        {
            _material.DisableKeyword("USE_UV2");
        }
        return base.OnInitialize();
    }
}
