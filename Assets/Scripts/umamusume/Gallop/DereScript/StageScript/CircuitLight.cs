using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CircuitLight : Gimmick
{
    [Serializable]
    public class Config
    {
        public Color color = Color.white;

        [Range(1f, 32f)]
        public float tight = 16f;

        [Range(0.0001f, 0.01f)]
        public float highLight = 0.0001f;

        [Range(0f, 3f)]
        public float speed = 1f;

        public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

        public eScroll direction;

        public bool reverse;

        [NonSerialized]
        private Vector2 _curveRange = Vector2.zero;

        public Vector2 curveRange
        {
            get
            {
                int num = curve.keys.Length;
                if (curve.keys.Length != 0)
                {
                    _curveRange.x = curve.keys[0].time;
                    _curveRange.y = curve.keys[num - 1].time;
                }
                else
                {
                    _curveRange = Vector2.zero;
                }
                return _curveRange;
            }
        }
    }

    public class LightMaterial
    {
        private int _shdPropID_ScrPos = INVALID_ID;

        private int _shdPropID_Color = INVALID_ID;

        private int _shdPropID_Tight = INVALID_ID;

        private int _shdPropID_HighLight = INVALID_ID;

        private Material _targetMaterial;

        private float _accumulatedTime;

        private eScroll _direction;

        public LightMaterial(Material mtrlTarget)
        {
            _targetMaterial = mtrlTarget;
            _shdPropID_Color = Shader.PropertyToID("_lumBaseColor");
            _shdPropID_Tight = Shader.PropertyToID("_lumTight");
            _shdPropID_HighLight = Shader.PropertyToID("_lumHighLight");
            _shdPropID_ScrPos = Shader.PropertyToID("_lumScrPos");
            UpdateScrollDirection(_direction, bForceUpdate: true);
        }

        public void UpdateScrollDirection(eScroll direction, bool bForceUpdate = false)
        {
            if (_direction != direction || bForceUpdate)
            {
                _direction = direction;
                switch (_direction)
                {
                    case eScroll.Y:
                        _targetMaterial.DisableKeyword("SCROLL_X");
                        break;
                    case eScroll.X:
                        _targetMaterial.EnableKeyword("SCROLL_X");
                        break;
                }
            }
        }

        public void Update(bool bPause, MaterialPropertyBlock mtrlPropBlock, Config config)
        {
            if (!bPause)
            {
                if (!config.reverse)
                {
                    if (_accumulatedTime >= config.curveRange.y)
                    {
                        _accumulatedTime = config.curveRange.x;
                    }
                    _accumulatedTime += Time.deltaTime * config.speed;
                }
                else
                {
                    if (config.curveRange.x > _accumulatedTime)
                    {
                        _accumulatedTime = config.curveRange.y;
                    }
                    _accumulatedTime -= Time.deltaTime * config.speed;
                }
            }
            float num = config.curve.Evaluate(_accumulatedTime);
            mtrlPropBlock.SetColor(_shdPropID_Color, config.color);
            mtrlPropBlock.SetFloat(_shdPropID_Tight, config.tight);
            mtrlPropBlock.SetFloat(_shdPropID_HighLight, config.highLight);
            mtrlPropBlock.SetFloat(_shdPropID_ScrPos, 1f - num);
            UpdateScrollDirection(config.direction);
        }
    }
    
    public enum eScroll
    {
        Y,
        X
    }

    public const int INVALID_ID = -1;

    public const string CIRCUIT_LIGHT_SHADER = "StageCircuitLight";

    public const eScroll DEFAULT_SCROLL_DIRECTION = eScroll.Y;

    private Director _director;

    private Dictionary<Material, LightMaterial> _mapLightMaterial = new Dictionary<Material, LightMaterial>();

    [SerializeField]
    private Config _config = new Config();

    public void Start()
    {
        _director = Director.instance;
    }

    public void Update()
    {
        bool flag = _director.IsPauseLive();
        foreach (KeyValuePair<Material, LightMaterial> item in _mapLightMaterial)
        {
            item.Value.Update(flag, _mtrlPropBlock, _config);
        }
        if (_mapLightMaterial.Count > 0 && !flag)
        {
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
            if (material.shader.name.Contains("StageCircuitLight") && !_mapLightMaterial.ContainsKey(material))
            {
                _lstMaterial.Add(material);
                _mapLightMaterial.Add(material, new LightMaterial(material));
            }
        }
    }
}
