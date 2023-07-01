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
    }
}
