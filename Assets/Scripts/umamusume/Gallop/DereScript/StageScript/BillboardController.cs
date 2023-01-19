using System;
using UnityEngine;

public class BillboardController : MonoBehaviour
{
    private enum eType
    {
        LOCK_Y_AXIS,
        LOCK_FREE,
        LOCK_Z_AXIS
    }

    private Director _director;

    private Transform _tmTargetCamera;

    private Transform _tm;

    private Action _fnUpdate;

    private bool _isDirector;

    private bool _isCamera;

    [SerializeField]
    private eType _type;

    [SerializeField]
    private Camera _targetCamera;

    private Camera targetCamera
    {
        get
        {
            return _targetCamera;
        }
        set
        {
            if (!(_targetCamera == value))
            {
                _isCamera = value != null;
                _targetCamera = value;
                if (_targetCamera != null)
                {
                    _tmTargetCamera = _targetCamera.transform;
                }
            }
        }
    }

    private void Start()
    {
        _tm = base.transform;
        _director = Director.instance;
        SetUpdateFunction();
        _isDirector = _director != null;
    }

    private void SetUpdateFunction()
    {
        switch (_type)
        {
            case eType.LOCK_Y_AXIS:
                _fnUpdate = UpdateLockYAxis;
                break;
            case eType.LOCK_Z_AXIS:
                _fnUpdate = UpdateLockZAxis;
                break;
            default:
                _fnUpdate = UpdateLockFree;
                break;
        }
    }

    private void LateUpdate()
    {
        if (_isDirector)
        {
            targetCamera = _director.mainCamera;
        }
        else
        {
            targetCamera = Camera.main;
        }
        if (_isCamera)
        {
            _fnUpdate();
        }
    }

    private void UpdateLockYAxis()
    {
        Quaternion rotation = _tm.rotation;
        Vector3 up = rotation * Vector3.up;
        Vector3 vector = _tmTargetCamera.position - _tm.position;
        float num = up.x * vector.x + up.y * vector.y + up.z * vector.z;
        Vector3 vector2 = default(Vector3);
        vector2.x = up.x * num;
        vector2.y = up.y * num;
        vector2.z = up.z * num;
        vector.x -= vector2.x;
        vector.y -= vector2.y;
        vector.z -= vector2.z;
        if (!(vector == Vector3.zero))
        {
            rotation.SetLookRotation(vector, up);
            _tm.rotation = rotation;
        }
    }

    private void UpdateLockZAxis()
    {
        Vector3 vector = _tmTargetCamera.position - _tm.position;
        vector = Quaternion.Inverse(_tm.rotation) * vector;
        vector.z = 0f;
        Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, vector.normalized);
        _tm.rotation *= quaternion;
    }

    private void UpdateLockFree()
    {
        _tm.LookAt(_tmTargetCamera);
    }
}
