using System;
using UnityEngine;

public class CameraLookAt : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCamera;

    [SerializeField]
    private GameObject _attachObject;

    private Transform _tmAttachObject;

    [SerializeField]
    private GameObject _translateObject;

    private Transform _tmTranslateObject;

    [SerializeField]
    private GameObject _rotateObject;

    private Transform _tmRotateObject;

    [SerializeField]
    private GameObject _fovObject;

    private Transform _tmFOVObject;

    [SerializeField]
    private float _multiplyValue = 100f;

    [SerializeField]
    private float _fieldOfViewLimit = 5f;

    [SerializeField]
    private GameObject _cameraMotionObject;

    [SerializeField]
    private Animator _cameraAnimator;

    private bool _initializeAnimationLength;

    private float _animationLength;

    private Quaternion _currentQuaternion;

    public Animator cameraAnimator => _cameraAnimator;

    private void Awake()
    {
        _targetCamera.transform.parent = _attachObject.transform;
        _targetCamera.transform.localPosition = Vector3.zero;
        _targetCamera.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
        _targetCamera.transform.localScale = Vector3.one;
        _cameraAnimator = _cameraMotionObject.GetComponent<Animator>();
        _tmFOVObject = _fovObject.transform;
        _tmRotateObject = _rotateObject.transform;
        _tmAttachObject = _attachObject.transform;
        _tmTranslateObject = _translateObject.transform;
        UpdateCamera();
    }

    public void ActivationUpdate()
    {
        if (!(_targetCamera == null) && !(_cameraAnimator == null) && !(Director.instance == null))
        {
            AlterUpdate();
        }
    }

    private void Update()
    {
        if (Director.instance == null || !Director.instance.isTimelineControlled)
        {
            AlterUpdate();
        }
    }

    public void AlterUpdate()
    {
        if (!(_cameraAnimator.runtimeAnimatorController == null))
        {
            if (!_initializeAnimationLength)
            {
                _animationLength = _cameraAnimator.GetCurrentAnimatorStateInfo(0).length;
                _initializeAnimationLength = true;
            }
            float normalizedTime = Director.instance.CalcFrameJustifiedMusicTime() / _animationLength;
            _cameraAnimator.Play(0, -1, normalizedTime);
            _cameraAnimator.speed = 0f;
        }
    }

    private void LateUpdate()
    {
        UpdateCamera();
        if (_targetCamera.fieldOfView < _fieldOfViewLimit)
        {
            _targetCamera.fieldOfView = _fieldOfViewLimit;
        }
    }

    private void UpdateCamera()
    {
        _targetCamera.fieldOfView = (0f - _tmFOVObject.localPosition.x) * _multiplyValue;
        _tmAttachObject.localPosition = _tmTranslateObject.localPosition;
        _currentQuaternion = Quaternion.AngleAxis((0f - _tmRotateObject.localPosition.z) * _multiplyValue, Vector3.forward) * Quaternion.AngleAxis((0f - _tmRotateObject.localPosition.y) * _multiplyValue, Vector3.up) * Quaternion.AngleAxis((0f - _tmRotateObject.localPosition.x) * _multiplyValue, Vector3.right);
        _tmAttachObject.localRotation = _currentQuaternion;
    }
}
