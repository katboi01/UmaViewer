using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallop.Live
{
    public class CameraLookAt : MonoBehaviour
    {
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private Camera _targetCamera; // 0x18
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private GameObject _attachObject; // 0x20
        private Transform _tmAttachObject; // 0x28
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private GameObject _translateObject; // 0x30
        private Transform _tmTranslateObject; // 0x38
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private GameObject _rotateObject; // 0x40
        private Transform _tmRotateObject; // 0x48
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private GameObject _fovObject; // 0x50
        private Transform _tmFOVObject; // 0x58
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _multiplyValue; // 0x60
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private float _fieldOfViewLimit; // 0x64
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private GameObject _cameraMotionObject; // 0x68
        [SerializeField] // RVA: 0xEBE0 Offset: 0xEBE0 VA: 0x7FF85CCBEBE0
        private Animator _cameraAnimator; // 0x70
        private Quaternion _currentQuaternion; // 0x78

        private bool _initializeAnimationLength;

        private float _animationLength;



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
}
