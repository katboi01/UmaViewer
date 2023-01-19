using System;
using UnityEngine;

[Serializable]
public class EyeTraceObject
{
    private Transform _tmBodyParent;

    private Transform _tmBody;

    private GameObject _body;

    private bool _isBody;

    private Transform _tmEye;

    private GameObject _eye;

    private Transform _tmTarget;

    private GameObject _target;

    private bool _isTarget;

    private const float EYE_ANGLE = 5f;

    private const float EYE_ANGLE_RAD = 0.08726646f;

    private readonly float EYE_ANGLE_LIMIT = Mathf.Cos(0.08726646f);

    private float _rangeMinX = -0.007f;

    private float _rangeMaxX = 0.007f;

    private float _rangeMinY = -0.003f;

    private float _rangeMaxY = 0.003f;

    public const float kBaseDelayRate = 0.3f;

    private float _delayRate = 0.3f;

    private float _moveRateX = 0.02f;

    private float _moveRateY = 0.02f;

    private float _offsetX;

    private float _offsetY;

    public GameObject body
    {
        get
        {
            return _body;
        }
        set
        {
            _body = value;
            _isBody = _body != null;
            if (_isBody)
            {
                _tmBody = _body.transform;
                _tmBodyParent = _tmBody.parent.transform;
            }
        }
    }

    public Transform eyeTrans => _tmEye;

    public GameObject eye
    {
        get
        {
            return _eye;
        }
        set
        {
            _eye = value;
            if (_eye != null)
            {
                _tmEye = _eye.transform;
            }
        }
    }

    public GameObject target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;
            _isTarget = _target != null;
            if (_isTarget)
            {
                _tmTarget = _target.transform;
            }
        }
    }

    public float delayRate
    {
        set
        {
            _delayRate = value;
        }
    }

    public void LateUpdate(float deltaTimeRate)
    {
        if (_isBody && _isTarget)
        {
            Vector3 right = _tmBodyParent.right;
            Vector3 up = _tmBodyParent.up;
            Vector3 forward = _tmBodyParent.forward;
            Vector3 position = _tmBody.position;
            Vector3 vector = _tmEye.position - position;
            Vector3 vector2 = position + vector;
            Vector3 normalized = (_tmTarget.position - vector2).normalized;
            float num = Vector3.Dot(forward, normalized);
            float b = 0f;
            float b2 = 0f;
            if (num < EYE_ANGLE_LIMIT)
            {
                normalized = Vector3.RotateTowards(normalized, forward, 0.08726646f, 0f);
                b = Vector3.Dot(right, normalized) * _moveRateX;
                b2 = Vector3.Dot(up, normalized) * _moveRateY;
                b = Mathf.Clamp(b, _rangeMinX, _rangeMaxX);
                b2 = Mathf.Clamp(b2, _rangeMinY, _rangeMaxY);
            }
            _offsetX = Mathf.Lerp(_offsetX, b, _delayRate * deltaTimeRate);
            _offsetY = Mathf.Lerp(_offsetY, b2, _delayRate * deltaTimeRate);
            right = _tmBody.right;
            up = _tmBody.up;
            _tmEye.position = vector2 + (right * _offsetX + up * _offsetY);
        }
    }

    public void UpdateRange(Rect rect)
    {
        _rangeMinX = Mathf.Min(rect.x, rect.width);
        _rangeMaxX = Mathf.Max(rect.x, rect.width);
        _rangeMinY = Mathf.Min(rect.y, rect.height);
        _rangeMaxY = Mathf.Max(rect.y, rect.height);
    }
}
