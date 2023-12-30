using Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EffectController : MonoBehaviour
{
    public class Effect
    {
        private static int _idShaderParam_CalcedColor;

        private static int _idShaderParam_Color;

        private static int _idShaderParam_ColorPower;

        private static int _idShaderParam_BlendOp;

        private static int _idShaderParam_BlendSrc;

        private static int _idShaderParam_BlendDst;

        private List<ParticleSystem> _lstParticleSystem = new List<ParticleSystem>();

        private List<Renderer> _lstRenderer = new List<Renderer>();

        private List<Material> _lstMaterial = new List<Material>();

        private Transform _defParentTransform;

        private Transform _curParentTransform;

        private Transform _cachedTransform;

        private Transform _handlerTransform;

        private Transform _anchorTransform;

        private Vector3 _stdPosition = Vector3.zero;

        private Vector3 _stdRotation = Vector3.zero;

        private bool _isPlay;

        private bool _isLoop;

        private bool _isStayPRS;

        private eEffectOwner _curOwner = eEffectOwner.World;

        private eEffectOccurrenceSpot _curOccurrenceSpot = eEffectOccurrenceSpot.Hand_Attach_L;

        private BlendOp _curBlendOption;

        private eEffectBlendMode _curBlendMode;

        private Color _color = Color.white;

        private Color _revColor = Color.black;

        private float _colorPower = 1f;

        private bool _bUnbind;

        private Vector3 _offset = Vector3.zero;

        private Vector3 _offsetAngle = Vector3.zero;

        public bool play
        {
            set
            {
                if (_isPlay == value)
                {
                    return;
                }
                _isPlay = value;
                for (int i = 0; i < _lstParticleSystem.Count; i++)
                {
                    ParticleSystem particleSystem = _lstParticleSystem[i];
                    ParticleSystem.EmissionModule emission = particleSystem.emission;
                    emission.enabled = _isPlay;
                    if (_isPlay)
                    {
                        particleSystem.Play(withChildren: false);
                    }
                    else
                    {
                        particleSystem.Stop(withChildren: false);
                    }
                }
            }
        }

        public bool pause
        {
            set
            {
                for (int i = 0; i < _lstParticleSystem.Count; i++)
                {
                    ParticleSystem particleSystem = _lstParticleSystem[i];
                    if (value && particleSystem.isPlaying)
                    {
                        if (!particleSystem.isPaused)
                        {
                            particleSystem.Pause(withChildren: false);
                        }
                    }
                    else if (particleSystem.isPaused)
                    {
                        ParticleSystem.EmissionModule emission = particleSystem.emission;
                        emission.enabled = _isPlay;
                        particleSystem.Play(withChildren: false);
                    }
                }
            }
        }

        public bool loop
        {
            set
            {
                if (_isLoop == value)
                {
                    return;
                }
                _isLoop = value;
                for (int i = 0; i < _lstParticleSystem.Count; i++)
                {
                    ParticleSystem particleSystem = _lstParticleSystem[i];
                    ParticleSystem.MainModule main = particleSystem.main;
                    main.loop = value;
                    if (value && !particleSystem.isPlaying)
                    {
                        _isPlay = value;
                        particleSystem.Play(withChildren: false);
                    }
                }
            }
        }

        public eEffectOwner owner => _curOwner;

        static Effect()
        {
            _idShaderParam_CalcedColor = Shader.PropertyToID("_CalcedColor");
            _idShaderParam_Color = Shader.PropertyToID("_Color");
            _idShaderParam_ColorPower = Shader.PropertyToID("_ColorPower");
            _idShaderParam_BlendOp = Shader.PropertyToID("_BlendOp");
            _idShaderParam_BlendSrc = Shader.PropertyToID("_BlendSrc");
            _idShaderParam_BlendDst = Shader.PropertyToID("_BlendDst");
        }

        public Effect(ParticleSystem particleSystem, Transform defParentTransform)
        {
            _lstParticleSystem.Add(particleSystem);
            _lstRenderer.Add(particleSystem.GetComponentInChildren<Renderer>());
            _lstMaterial.Add(_lstRenderer[0].material);
            _defParentTransform = defParentTransform;
            _curParentTransform = _defParentTransform;
            _cachedTransform = particleSystem.transform;
            _ = $"handler_{_cachedTransform.name}";
            _handlerTransform = _cachedTransform.parent;
            string text = $"anchor_{_cachedTransform.name}";
            Transform[] componentsInChildren = _cachedTransform.GetComponentsInChildren<Transform>();
            foreach (Transform transform in componentsInChildren)
            {
                if (transform.name == text && transform.childCount == 0)
                {
                    _anchorTransform = transform;
                    break;
                }
            }
            _anchorTransform.SetParent(_curParentTransform, worldPositionStays: false);
            _anchorTransform.position = Vector3.zero;
            _anchorTransform.rotation = Quaternion.identity;
            _isPlay = false;
            _isLoop = false;
            particleSystem.Stop(withChildren: false);
            particleSystem.Clear(withChildren: false);
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = false;
        }

        public void SetVisibleAndClear(bool bVisible, bool bClear)
        {
            for (int i = 0; i < _lstRenderer.Count; i++)
            {
                _lstRenderer[i].enabled = bVisible;
            }
            if (bClear)
            {
                for (int j = 0; j < _lstParticleSystem.Count; j++)
                {
                    _lstParticleSystem[j].Clear(withChildren: false);
                }
            }
        }

        public void Update()
        {
            if (!_bUnbind && _isPlay && _lstRenderer[0].enabled)
            {
                _cachedTransform.localPosition = _anchorTransform.position;
                _cachedTransform.localRotation = _anchorTransform.rotation;
            }
        }

        public void UpdateInfo(ref EffectUpdateInfo updateInfo, Transform[,] arrCharacterTransform)
        {
            if (_bUnbind)
            {
                return;
            }
            _offset = updateInfo.offset;
            _offsetAngle = updateInfo.offsetAngle;
            bool flag = false;
            play = updateInfo.isPlay;
            loop = updateInfo.isLoop;
            if (updateInfo.isClear)
            {
                for (int i = 0; i < _lstParticleSystem.Count; i++)
                {
                    _lstParticleSystem[i].Clear(withChildren: false);
                }
            }
            if (_curBlendMode != updateInfo.blendMode)
            {
                _curBlendMode = updateInfo.blendMode;
                BlendMode value = BlendMode.One;
                BlendMode value2 = BlendMode.One;
                switch (_curBlendMode)
                {
                    case eEffectBlendMode.Additive:
                        _curBlendOption = BlendOp.Add;
                        value = BlendMode.One;
                        value2 = BlendMode.One;
                        break;
                    case eEffectBlendMode.SoftAdditive:
                        _curBlendOption = BlendOp.Add;
                        value = BlendMode.OneMinusDstColor;
                        value2 = BlendMode.One;
                        break;
                    case eEffectBlendMode.Multiply:
                        _curBlendOption = BlendOp.ReverseSubtract;
                        value = BlendMode.One;
                        value2 = BlendMode.One;
                        break;
                    case eEffectBlendMode.Multiply2:
                        _curBlendOption = BlendOp.ReverseSubtract;
                        value = BlendMode.OneMinusDstColor;
                        value2 = BlendMode.One;
                        break;
                    case eEffectBlendMode.AlphaBlend:
                        _curBlendOption = BlendOp.Add;
                        value = BlendMode.SrcAlpha;
                        value2 = BlendMode.OneMinusSrcAlpha;
                        break;
                }
                for (int j = 0; j < _lstMaterial.Count; j++)
                {
                    Material material = _lstMaterial[j];
                    material.SetInt(_idShaderParam_BlendOp, (int)_curBlendOption);
                    material.SetInt(_idShaderParam_BlendSrc, (int)value);
                    material.SetInt(_idShaderParam_BlendDst, (int)value2);
                }
                flag = true;
            }
            if (_color != updateInfo.color)
            {
                _color = updateInfo.color;
                _revColor = Color.white - _color;
                for (int k = 0; k < _lstMaterial.Count; k++)
                {
                    _lstMaterial[k].SetColor(_idShaderParam_Color, updateInfo.color);
                }
                flag = true;
            }
            if (_colorPower != updateInfo.colorPower)
            {
                _colorPower = updateInfo.colorPower;
                for (int l = 0; l < _lstMaterial.Count; l++)
                {
                    _lstMaterial[l].SetFloat(_idShaderParam_ColorPower, _colorPower);
                }
                flag = true;
            }
            if (flag)
            {
                if (_curBlendOption == BlendOp.ReverseSubtract)
                {
                    updateInfo.color = _revColor;
                }
                for (int m = 0; m < _lstMaterial.Count; m++)
                {
                    _lstMaterial[m].SetColor(_idShaderParam_CalcedColor, updateInfo.color * _colorPower);
                }
            }
            if (_curOwner != updateInfo.owner || _curOccurrenceSpot != updateInfo.occurrenceSpot || _isStayPRS != updateInfo.isStayPRS)
            {
                for (int n = 0; n < _lstRenderer.Count; n++)
                {
                    _lstRenderer[n].enabled = updateInfo.isEnable;
                }
                _curOwner = updateInfo.owner;
                _curOccurrenceSpot = updateInfo.occurrenceSpot;
                _isStayPRS = updateInfo.isStayPRS;
                if (_curOwner == eEffectOwner.World)
                {
                    if (!_isStayPRS)
                    {
                        _anchorTransform.localPosition = Vector3.zero;
                        _anchorTransform.localRotation = Quaternion.identity;
                    }
                    _curParentTransform = _defParentTransform;
                }
                else
                {
                    _anchorTransform.localPosition = Vector3.zero;
                    _anchorTransform.localRotation = Quaternion.identity;
                    int num = eEffectOwnerUtil.ToCharaPosIndex(updateInfo.owner);
                    int occurrenceSpot = (int)updateInfo.occurrenceSpot;
                    _curParentTransform = arrCharacterTransform[num, occurrenceSpot];
                }
                _anchorTransform.SetParent(_curParentTransform, _isStayPRS);
                _stdPosition = _anchorTransform.localPosition;
                _stdRotation = _anchorTransform.localRotation.eulerAngles;
            }
            if (_curOwner == eEffectOwner.World)
            {
                _anchorTransform.localPosition = _stdPosition + updateInfo.offset;
                _anchorTransform.localRotation = Quaternion.Euler(_stdRotation + updateInfo.offsetAngle);
            }
            else
            {
                _anchorTransform.localPosition = updateInfo.offset;
                _anchorTransform.localRotation = Quaternion.Euler(updateInfo.offsetAngle);
            }
        }

        public string GetName()
        {
            if (!(_cachedTransform != null))
            {
                return "";
            }
            return _cachedTransform.name;
        }

        public void AddChild(ParticleSystem ps)
        {
            if (!(null == ps))
            {
                _lstParticleSystem.Add(ps);
                Renderer componentInChildren = ps.GetComponentInChildren<Renderer>();
                _lstRenderer.Add(componentInChildren);
                _lstMaterial.Add(componentInChildren.material);
                ps.Stop(withChildren: false);
                ps.Clear(withChildren: false);
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
            }
        }

        public void Bind(Transform[,] arrCharacterTransform)
        {
            if (_bUnbind)
            {
                _anchorTransform.localPosition = Vector3.zero;
                int curOwner = (int)_curOwner;
                int curOccurrenceSpot = (int)_curOccurrenceSpot;
                _curParentTransform = arrCharacterTransform[curOwner, curOccurrenceSpot];
                _anchorTransform.SetParent(_curParentTransform, worldPositionStays: false);
                _stdPosition = _anchorTransform.localPosition;
                _anchorTransform.localPosition = _offset;
                _stdRotation = _anchorTransform.localRotation.eulerAngles;
                _anchorTransform.localRotation = Quaternion.Euler(_offsetAngle);
                _bUnbind = false;
            }
        }

        public void Unbind()
        {
            if (!_bUnbind)
            {
                eEffectOwner eEffectOwner = owner;
                if ((uint)eEffectOwner <= 4u || (uint)(eEffectOwner - 6) <= 9u)
                {
                    _anchorTransform.SetParent(_defParentTransform, _isStayPRS);
                    _anchorTransform.position = Vector3.zero;
                    _anchorTransform.rotation = Quaternion.identity;
                    _bUnbind = true;
                }
            }
        }
    }

    private Director _director;

    private Effect _tmpEffect;

    private Dictionary<int, Effect> _dicEffect = new Dictionary<int, Effect>();

    private List<Effect> _lstEffect = new List<Effect>();

    private Transform[,] _arrCharacterTransform = new Transform[0, 0];

    private bool _bPause;

    public Dictionary<int, Effect> dicEffect => _dicEffect;

    public bool Initialize()
    {
        _director = Director.instance;
        if (_director == null)
        {
            return false;
        }
        Transform defParentTransform = base.gameObject.transform;
        ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (componentsInChildren[i] != null)
            {
                ParticleSystem.MainModule main = componentsInChildren[i].main;
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            }
            if (componentsInChildren[i].name.IndexOf("child_", StringComparison.Ordinal) != 0 && componentsInChildren[i].name.IndexOf("sub_", StringComparison.Ordinal) != 0)
            {
                Effect effect = new Effect(componentsInChildren[i], defParentTransform);
                int key = FNVHash.Generate(componentsInChildren[i].name);
                _dicEffect.Add(key, effect);
                _lstEffect.Add(effect);
            }
        }
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            if (componentsInChildren[j].name.IndexOf("child_", StringComparison.Ordinal) != 0)
            {
                continue;
            }
            for (int k = 0; k < _lstEffect.Count; k++)
            {
                string value = $"child_{_lstEffect[k].GetName()}";
                if (componentsInChildren[j].name.IndexOf(value, StringComparison.Ordinal) == 0)
                {
                    _lstEffect[k].AddChild(componentsInChildren[j]);
                }
            }
        }
        string[] names = Enum.GetNames(typeof(eEffectOccurrenceSpot));
        _arrCharacterTransform = new Transform[_director.MemberUnitNumber, names.Length];
        for (int l = 0; l < _director.MemberUnitNumber; l++)
        {
            CharacterObject characterObjectFromPositionID = _director.getCharacterObjectFromPositionID((LiveCharaPosition)l);
            if (!(characterObjectFromPositionID == null))
            {
                Transform[] array = characterObjectFromPositionID.effectSpot.Build<eEffectOccurrenceSpot>();
                for (int m = 0; m < names.Length; m++)
                {
                    _arrCharacterTransform[l, m] = array[m];
                }
            }
        }
        return true;
    }

    private void Update()
    {
        int count = _lstEffect.Count;
        for (int i = 0; i < count; i++)
        {
            _lstEffect[i].Update();
        }
    }

    public Effect UpdateInfo(ref EffectUpdateInfo updateInfo)
    {
        int nameHash = updateInfo.data.nameHash;
        if (_dicEffect.TryGetValue(nameHash, out _tmpEffect))
        {
            _tmpEffect.UpdateInfo(ref updateInfo, _arrCharacterTransform);
        }
        return _tmpEffect;
    }

    public void SetVisibleAndClear(eEffectOwner owner, bool bVisible, bool bClear)
    {
        for (int i = 0; i < _lstEffect.Count; i++)
        {
            if (_lstEffect[i].owner == owner)
            {
                _lstEffect[i].SetVisibleAndClear(bVisible, bClear);
            }
        }
    }

    public void Pause(bool bSwitch)
    {
        if (_bPause != bSwitch)
        {
            _bPause = bSwitch;
            for (int i = 0; i < _lstEffect.Count; i++)
            {
                _lstEffect[i].pause = bSwitch;
            }
        }
    }
    public void Bind(bool bBind, int iCharaPos)
    {
        if (_director == null)
        {
            return;
        }
        int count = _lstEffect.Count;
        if (bBind)
        {
            CharacterObject characterObjectFromPositionID = _director.getCharacterObjectFromPositionID((LiveCharaPosition)iCharaPos);
            if (characterObjectFromPositionID == null)
            {
                return;
            }
            Transform[] array = characterObjectFromPositionID.effectSpot.GetTransformCategory<eEffectOccurrenceSpot>();
            if (array == null)
            {
                array = characterObjectFromPositionID.effectSpot.Build<eEffectOccurrenceSpot>();
            }
            for (int i = 0; i < array.Length; i++)
            {
                _arrCharacterTransform[iCharaPos, i] = array[i];
            }
            for (int j = 0; j < count; j++)
            {
                if (_lstEffect[j].owner == eEffectOwnerUtil.FromLiveCharaPosition((LiveCharaPosition)iCharaPos))
                {
                    _lstEffect[j].Bind(_arrCharacterTransform);
                }
            }
            return;
        }
        for (int k = 0; k < count; k++)
        {
            if (_lstEffect[k].owner == eEffectOwnerUtil.FromLiveCharaPosition((LiveCharaPosition)iCharaPos))
            {
                _lstEffect[k].Unbind();
            }
        }
    }
}