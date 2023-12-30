using Cutt;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Props : MonoBehaviour
{
    [Serializable]
    private class AdjustmentData
    {
        public Transform transform;

        public Vector3 offsetRange = Vector3.zero;

        public Vector3 standardLength = Vector3.zero;

        public Vector3 transformRate = Vector3.one;
    }

    private class AttachJointInfo
    {
        public Transform transform;

        public bool isInfluenceOfBodyRootScale;

        public AttachJointInfo(Transform transform, bool isInfluenceOfBodyRootScale)
        {
            this.transform = transform;
            this.isInfluenceOfBodyRootScale = isInfluenceOfBodyRootScale;
        }
    }

    [SerializeField]
    private int _propsID;

    [SerializeField]
    private AdjustmentData[] _adjustmentData;

    [SerializeField]
    private string[] _attachJointNames;

    private Dictionary<int, AttachJointInfo> _attachJointDictionary = new Dictionary<int, AttachJointInfo>();

    private int _attachNowHash;

    [SerializeField]
    private bool _isInfluenceOfCharaHeight;

    [SerializeField]
    private bool _IsInfluenceOfAttachFloating;

    [SerializeField]
    private bool _isInfluenceAnimationSynchronization;

    private AnimationState[] _animationStateArray;

    private int _currentAnimationId;

    private const int PROPS_SORTING_ORDER = -99;

    private const float ANIMATION_PLAY_SPEED = 1f;

    private const float UPDATE_TIME = 0.0166666675f;

    private bool _bVisible;

    private bool _bStarted;

    private float _fStartTime;

    private float _bodyScaleSubScale;

    private bool _isInfluenceOfBodyRootScale;

    protected GameObject _parentObject;

    protected GameObject _bodyRootObject;

    protected CharacterObject _characterObject;

    protected Transform _characterTransform;

    protected Vector3 _offsetPosition = Vector3.zero;

    protected Vector3 _rotation = Vector3.zero;

    protected Vector3 _localScale = Vector3.one;

    protected Vector3 _defaultBodyRootPosition = Vector3.zero;

    protected Vector3 _defaultBodyRootPositionLocal = Vector3.zero;

    protected float _cuttScale = 1f;

    protected Transform _cachedTransform;

    protected bool _isParentTransform;

    protected Transform _parentTransform;

    protected Renderer[] _renderers;

    protected Material _material;

    protected MaterialPropertyBlock _propBlock;

    protected Animation _animation;

    protected AnimationState _curAnimationState;

    protected AfterImage _afterImage;

    private int _lightPositionPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _specularPowerPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _specularColorPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _luminousColorPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _propsColorId;

    private LiveTimelinePropsSettings.PropsConditionGroup _propsConditionGroup;

    private float _musicScoreTime;

    private bool _applyCharacterRotation;

    private bool _drawAfterImage;

    public Transform cachedTransform => _cachedTransform;

    public Material copyMaterial
    {
        get
        {
            if (_renderers == null)
            {
                return _material;
            }
            return _renderers[0].material;
        }
    }

    public MaterialPropertyBlock propBlock => _propBlock;

    public float musicScoreTime
    {
        set
        {
            _musicScoreTime = value;
        }
    }

    public bool isStarted => _bStarted;

    public float startTime
    {
        get
        {
            return _fStartTime;
        }
        set
        {
            _fStartTime = value;
        }
    }

    public bool isVisible
    {
        get
        {
            return _bVisible;
        }
        set
        {
            if (_renderers != null)
            {
                if (_drawAfterImage)
                {
                    _afterImage.Attach(value);
                }
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].enabled = value;
                }
            }
            _bVisible = value;
        }
    }

    public bool drawAfterImage
    {
        get
        {
            return _drawAfterImage;
        }
        set
        {
            if (_drawAfterImage != value)
            {
                _drawAfterImage = value;
                if (_bVisible && _afterImage != null)
                {
                    _afterImage.Attach(value);
                }
            }
        }
    }

    public void Init(GameObject parentObject, CharacterObject characterObject, int propsId, LiveTimelinePropsSettings.PropsConditionGroup propData, Camera targetCamera, bool applyCharaRotation)
    {
        _afterImage = GetComponent<AfterImage>();
        if (_afterImage != null)
        {
            _afterImage.currentTargetCamera = targetCamera;
        }
        _renderers = GetComponentsInChildren<Renderer>();
        if (_renderers.Length == 0)
        {
            return;
        }
        _propsID = propsId;
        _ = _propsID;
        InitAnimation();
        _propsConditionGroup = propData;
        _isParentTransform = false;
        _parentTransform = null;
        _material = _renderers[0].sharedMaterial;
        SharedShaderParam instance = SharedShaderParam.instance;
        _propBlock = new MaterialPropertyBlock();
        _propsColorId = instance.getPropertyID(SharedShaderParam.ShaderProperty.PropsColor);
        _propBlock.SetVector(_propsColorId, Vector4.one);
        _lightPositionPropertyIndex = Shader.PropertyToID("_lightPosition");
        _specularColorPropertyIndex = Shader.PropertyToID("_specularColor");
        _specularPowerPropertyIndex = Shader.PropertyToID("_specularPower");
        _luminousColorPropertyIndex = Shader.PropertyToID("_luminousColor");
        _propBlock.SetVector(_lightPositionPropertyIndex, Vector3.zero);
        _propBlock.SetColor(_specularColorPropertyIndex, Color.white);
        _propBlock.SetFloat(_specularPowerPropertyIndex, 5f);
        _propBlock.SetVector(_luminousColorPropertyIndex, new Color(1f, 1f, 1f, 0f));
        if (_renderers != null && (bool)_material)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetPropertyBlock(_propBlock);
                _renderers[i].sortingOrder = -99;
            }
        }
        _cachedTransform = base.gameObject.transform;
        isVisible = true;
        StartAttach(parentObject, characterObject);
        if (!SingletonMonoBehaviour<GeneralEventManager>.IsInstanceEmpty())
        {
            GeneralEventManager instance2 = SingletonMonoBehaviour<GeneralEventManager>.instance;
            instance2.RegistEventHandler(GeneralEventDefine.EVENT_TYPE_LIVE_PAUSE_START, PauseStartEvent);
            instance2.RegistEventHandler(GeneralEventDefine.EVENT_TYPE_LIVE_PAUSE_END, PauseEndEvent);
        }
        _applyCharacterRotation = applyCharaRotation;
    }

    private void InitAnimation()
    {
        _animation = GetComponentInChildren<Animation>();
        if (_animation == null)
        {
            return;
        }
        int num = 0;
        _animationStateArray = new AnimationState[_animation.GetClipCount()];
        foreach (AnimationState item in _animation)
        {
            _animationStateArray[num] = item;
            num++;
        }
    }

    private void OnDestroy()
    {
        if (!SingletonMonoBehaviour<GeneralEventManager>.IsInstanceEmpty())
        {
            GeneralEventManager instance = SingletonMonoBehaviour<GeneralEventManager>.instance;
            instance.UnregistEventHandler(PauseStartEvent);
            instance.UnregistEventHandler(PauseEndEvent);
        }
        _propsConditionGroup = null;
    }

    public void SetCuttScale(int heightId)
    {
        if (!_isInfluenceOfCharaHeight || _cachedTransform == null)
        {
            return;
        }
        float cuttScale = 1f;
        if (_propsConditionGroup != null)
        {
            switch (heightId)
            {
                case 0:
                    cuttScale = _propsConditionGroup.bodyScaleS;
                    break;
                case 1:
                    cuttScale = _propsConditionGroup.bodyScaleM;
                    break;
                case 2:
                    cuttScale = _propsConditionGroup.bodyScaleL;
                    break;
                case 3:
                    cuttScale = _propsConditionGroup.bodyScaleLL;
                    break;
            }
        }
        _cuttScale = cuttScale;
        _cachedTransform.localScale = new Vector3(_cuttScale, _cuttScale, _cuttScale);
    }

    public void SetScale(float bodyScaleSubScale)
    {
        if (_adjustmentData == null)
        {
            return;
        }
        _bodyScaleSubScale = bodyScaleSubScale;
        for (int i = 0; i < _adjustmentData.Length; i++)
        {
            if (!(_adjustmentData[i].transform == null))
            {
                Vector3 localPosition = _adjustmentData[i].standardLength * _bodyScaleSubScale;
                localPosition -= _adjustmentData[i].offsetRange;
                localPosition.Scale(_adjustmentData[i].transformRate);
                _adjustmentData[i].transform.localPosition = localPosition;
            }
        }
    }

    private void LateUpdate()
    {
        if (_isParentTransform && isVisible)
        {
            if (_isInfluenceOfCharaHeight && _isInfluenceOfBodyRootScale)
            {
                Transform transform = _bodyRootObject.transform;
                Vector3 localScale = transform.localScale;
                _parentTransform.localScale = localScale;
                _parentTransform.localPosition += _offsetPosition;
                if (((Director.instance != null) ? Director.instance.musicScoreTime : _musicScoreTime) <= 0.0166666675f)
                {
                    _defaultBodyRootPosition = _characterTransform.InverseTransformPoint(transform.position);
                    _defaultBodyRootPositionLocal = transform.localPosition;
                }
                Vector3 vector = _characterTransform.InverseTransformPoint(_parentTransform.position);
                Vector3 vector2 = (_IsInfluenceOfAttachFloating ? _characterTransform.InverseTransformPoint(transform.position) : _defaultBodyRootPosition);
                Vector3 vector3 = (_IsInfluenceOfAttachFloating ? transform.localPosition : _defaultBodyRootPositionLocal);
                Vector3 vector4 = vector - vector2;
                if ((bool)_characterObject && _characterObject.createInfo.positionMode == LiveTimelineData.CharacterPositionMode.Relative)
                {
                    vector4 = vector - (vector2 - vector3 + vector3 / _bodyScaleSubScale);
                }
                float num = localScale.x * _cuttScale;
                Vector3 position = default(Vector3);
                position.x = vector2.x + vector4.x * num;
                position.y = vector2.y + vector4.y * num;
                position.z = vector2.z + vector4.z * num;
                if (!_IsInfluenceOfAttachFloating)
                {
                    position.y = vector.y;
                }
                _parentTransform.position = _characterTransform.TransformPoint(position);
            }
            else
            {
                _parentTransform.localScale = Vector3.one;
                Vector3 localPosition = _parentTransform.localPosition;
                localPosition *= _bodyScaleSubScale;
                localPosition.x *= _localScale.x;
                localPosition.y *= _localScale.y;
                localPosition.z *= _localScale.z;
                _parentTransform.localPosition = localPosition;
                if (_applyCharacterRotation && _characterTransform != null)
                {
                    _parentTransform.position += _characterTransform.rotation * _offsetPosition;
                }
                else
                {
                    _parentTransform.position += _offsetPosition;
                }
            }
            _parentTransform.rotation *= Quaternion.Euler(_rotation);
        }
        if (_cachedTransform != null && _afterImage != null && Director.instance != null)
        {
            _afterImage.currentTargetCamera = Director.instance.mainCamera;
        }
    }

    public void UpdateMotion(float currentTime)
    {
        if (_isInfluenceAnimationSynchronization && _animationStateArray != null && _currentAnimationId < _animationStateArray.Length)
        {
            _animationStateArray[_currentAnimationId].speed = 0f;
            _animationStateArray[_currentAnimationId].time = currentTime;
        }
    }

    protected virtual void OnLoad(int id)
    {
    }

    private void SetAttachData(Transform attachTransform, bool isInfluenceOfBodyRootScale, int hash)
    {
        if (!(_cachedTransform == null))
        {
            _cachedTransform.SetParent(null, worldPositionStays: false);
            _cachedTransform.localScale = Vector3.one;
            _cachedTransform.SetParent(attachTransform, worldPositionStays: false);
            _isParentTransform = attachTransform != null;
            _parentTransform = attachTransform;
            _isInfluenceOfBodyRootScale = isInfluenceOfBodyRootScale;
            Vector3 lossyScale = _cachedTransform.lossyScale;
            _localScale.x = 1f / lossyScale.x;
            _localScale.y = 1f / lossyScale.y;
            _localScale.z = 1f / lossyScale.z;
            if (!_isInfluenceOfCharaHeight)
            {
                _cachedTransform.localScale = _localScale;
            }
            else
            {
                _cachedTransform.localScale = new Vector3(_cuttScale, _cuttScale, _cuttScale);
            }
            _attachNowHash = hash;
        }
    }

    public void StartAttach(GameObject parentObject, CharacterObject characterObject)
    {
        _parentObject = parentObject;
        _characterObject = characterObject;
        _characterTransform = (_characterObject ? _characterObject.transform : null);
        _bodyRootObject = (_characterObject ? _characterObject.bodyRoot.gameObject : null);
        _attachJointDictionary.Clear();
        if (!(_parentObject != null))
        {
            return;
        }
        bool flag = true;
        bool flag2 = false;
        for (int i = 0; i < _attachJointNames.Length; i++)
        {
            Transform transform = GameObjectUtility.FindChildTransform(_attachJointNames[i], _parentObject.transform);
            if (transform == null)
            {
                continue;
            }
            int num = FNVHash.Generate(_attachJointNames[i]);
            flag2 = _bodyRootObject != null;
            Transform transform2 = transform;
            while (flag2 && transform2 != null)
            {
                if (transform2 == _bodyRootObject.transform)
                {
                    flag2 = false;
                }
                transform2 = transform2.parent;
            }
            _attachJointDictionary.Add(num, new AttachJointInfo(transform, flag2));
            if (flag)
            {
                SetAttachData(transform, flag2, num);
                flag = false;
            }
        }
    }

    public void ChangeAttach(int attachNameHash, ref Vector3 offset_position, ref Vector3 rotation)
    {
        if (!(_parentObject == null))
        {
            if (_attachNowHash != attachNameHash && _attachJointDictionary.TryGetValue(attachNameHash, out var value))
            {
                SetAttachData(value.transform, value.isInfluenceOfBodyRootScale, attachNameHash);
            }
            _offsetPosition = offset_position;
            _rotation = rotation;
        }
    }

    public void ChangeRenderState(Vector4 vecColor, ref PropsUpdateInfo updateInfo)
    {
        if (isVisible != updateInfo.renderEnable)
        {
            isVisible = updateInfo.renderEnable;
        }
        if (updateInfo.renderEnable)
        {
            Color specularColor = updateInfo.specularColor;
            specularColor.r *= updateInfo.specularColor.a;
            specularColor.g *= updateInfo.specularColor.a;
            specularColor.b *= updateInfo.specularColor.a;
            Vector3 vector = updateInfo.lightPosition;
            if (updateInfo.useLocalAxis)
            {
                vector = _cachedTransform.rotation * vector;
            }
            propBlock.SetVector(_propsColorId, vecColor);
            _propBlock.SetVector(_lightPositionPropertyIndex, vector);
            _propBlock.SetColor(_specularColorPropertyIndex, specularColor);
            _propBlock.SetFloat(_specularPowerPropertyIndex, updateInfo.specularPower);
            _propBlock.SetVector(_luminousColorPropertyIndex, updateInfo.luminousColor);
            UpdateMaterialPropertyBlock();
            if (_afterImage != null)
            {
                _afterImage.color = vecColor;
                _afterImage.rootColor = updateInfo.rootColor;
                _afterImage.tipColor = updateInfo.tipColor;
                _afterImage.lightPosition = vector;
                _afterImage.specularPower = updateInfo.specularPower;
                _afterImage.specularColor = specularColor;
                _afterImage.luminousColor = updateInfo.luminousColor;
            }
        }
    }

    public void UpdateMaterialPropertyBlock()
    {
        if (_propBlock != null && _renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetPropertyBlock(_propBlock);
            }
        }
    }

    public void SetShader(Shader shader)
    {
        if (_material != null && (bool)shader)
        {
            _material.shader = shader;
        }
    }

    public Vector3 GetPosition()
    {
        if (_cachedTransform != null)
        {
            return _cachedTransform.position;
        }
        return Vector3.one;
    }

    public void SetPosition(ref Vector3 vecPos)
    {
        if (_cachedTransform != null)
        {
            _cachedTransform.position = vecPos;
        }
    }

    public void SetRotation(ref Quaternion qRot)
    {
        if (_cachedTransform != null)
        {
            _cachedTransform.rotation = qRot;
        }
    }

    public void SetRotation(float x, float y, float z)
    {
        if (_cachedTransform != null)
        {
            _cachedTransform.rotation = Quaternion.Euler(x, y, z);
        }
    }

    public void ChangeAnimationClip(int id, float speed, float offset)
    {
        _currentAnimationId = id;
        SetAnimationSpeed(speed);
        for (int i = 0; i < _animationStateArray.Length; i++)
        {
            if (i == _currentAnimationId)
            {
                _animation.clip = _animationStateArray[i].clip;
                _animationStateArray[i].time = offset;
                _animation.Play(_animationStateArray[i].clip.name);
            }
            else if (_animation.IsPlaying(_animationStateArray[i].clip.name))
            {
                _animation.Stop(_animationStateArray[i].clip.name);
            }
        }
    }

    public void SetAnimationClips(Dictionary<string, AnimationClip> dicClips, string strDefClip = "")
    {
        if (!(_animation != null) || dicClips.Count <= 0)
        {
            return;
        }
        foreach (AnimationState item in _animation)
        {
            _animation.RemoveClip(item.clip);
        }
        if (!string.IsNullOrEmpty(strDefClip) && dicClips.ContainsKey(strDefClip))
        {
            AnimationClip animationClip = dicClips[strDefClip];
            _animation.AddClip(animationClip, animationClip.name);
            _animation.clip = animationClip;
            _curAnimationState = _animation[strDefClip];
        }
        foreach (KeyValuePair<string, AnimationClip> dicClip in dicClips)
        {
            _animation.AddClip(dicClip.Value, dicClip.Value.name);
        }
    }

    public void AddAnimationClip(AnimationClip clip, bool bSetDef = false)
    {
        if (_animation != null && clip != null)
        {
            _animation.AddClip(clip, clip.name);
            if (bSetDef)
            {
                _animation.clip = clip;
                _curAnimationState = _animation[clip.name];
            }
        }
    }

    public void SetCurrentAnimationClip(string strName)
    {
        if (_animation != null)
        {
            AnimationState animationState = _animation[strName];
            if (animationState != null)
            {
                _curAnimationState = animationState;
                _animation.clip = animationState.clip;
                _animation.Stop();
            }
        }
    }

    public void RemoveAnimationClips()
    {
        if (!(_animation != null))
        {
            return;
        }
        foreach (AnimationState item in _animation)
        {
            if (_animation.isPlaying)
            {
                _animation.Stop();
            }
            _animation.RemoveClip(item.clip);
        }
        _animation.clip = null;
    }

    public void ResetCurrentAnimationState()
    {
        _fStartTime = 0f;
        _bStarted = false;
        if (_animation != null)
        {
            _animation.Stop();
            _animation.clip = null;
        }
        if (_curAnimationState != null)
        {
            _curAnimationState = null;
        }
    }

    public void StartAnimation(string strName, float fStartTime = 0f, WrapMode wrapMode = WrapMode.Default)
    {
        ResetCurrentAnimationState();
        _curAnimationState = _animation[strName];
        if (wrapMode == WrapMode.Loop)
        {
            _curAnimationState.wrapMode = wrapMode;
            _animation.clip = _curAnimationState.clip;
            _animation.Play();
            return;
        }
        _fStartTime = fStartTime;
        _curAnimationState.wrapMode = wrapMode;
        _curAnimationState.speed = 0f;
        _animation.clip = _curAnimationState.clip;
        _bStarted = true;
        UpdateAnimation(_fStartTime);
    }

    public void UpdateAnimation(float fElapsedTime)
    {
        if (_bStarted && _curAnimationState != null && _curAnimationState.wrapMode != WrapMode.Loop)
        {
            float num = fElapsedTime - _fStartTime;
            if (num >= 0f && num <= _curAnimationState.length)
            {
                _curAnimationState.time = num;
                _animation.Play();
            }
            else
            {
                _bStarted = false;
            }
        }
    }

    public void StopAnimation()
    {
        if (_animation != null)
        {
            _animation.Stop();
        }
    }

    public void PauseAnimation()
    {
        SetAnimationSpeed(0f);
    }

    public void ResumeAnimation()
    {
        SetAnimationSpeed(1f);
    }

    public void SetAnimationSpeed(float speed)
    {
        if (_animationStateArray != null)
        {
            for (int i = 0; i < _animationStateArray.Length; i++)
            {
                _animationStateArray[i].speed = speed;
            }
        }
    }

    public void ResetAttach()
    {
        _isParentTransform = false;
        _parentTransform = null;
    }

    private bool PauseStartEvent(IEventSender eventSender, GeneralEventDefine.EventData eventData)
    {
        PauseAnimation();
        return true;
    }

    private bool PauseEndEvent(IEventSender eventSender, GeneralEventDefine.EventData eventData)
    {
        ResumeAnimation();
        return true;
    }
}
