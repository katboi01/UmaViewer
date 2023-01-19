using System;
using UnityEngine;

public class StageGazeController : MonoBehaviour
{
    public enum ForwardType
    {
        Z,
        Y,
        X
    }

    [SerializeField]
    private ForwardType _forwardType;

    [SerializeField]
    private int _groupNo;

    private Transform _cacheTransform;

    private bool _isEnable;

    private Vector3 _targetPosition;

    private static Action<StageGazeController> UpdateGazeForwardZ = OnUpdateGazeForwardZ;

    private static Action<StageGazeController> UpdateGaze = OnUpdateGaze;

    private Action<StageGazeController> _updateGaze;

    public int GroupNo => _groupNo;

    public bool IsEnableLookAt
    {
        get
        {
            return _isEnable;
        }
        set
        {
            _isEnable = value;
        }
    }

    private static Quaternion GetLookAtRotation(Vector3 dir, Vector3 forward)
    {
        float num = Mathf.Acos(Vector3.Dot(forward, dir));
        if (num > 0.01f)
        {
            Vector3 normalized = Vector3.Cross(forward, dir).normalized;
            return Quaternion.AngleAxis(57.29578f * num, normalized);
        }
        return Quaternion.identity;
    }

    private static void OnUpdateGazeForwardZ(StageGazeController _this)
    {
        _this._cacheTransform.LookAt(_this._targetPosition);
    }

    private static void OnUpdateGaze(StageGazeController _this)
    {
        Vector3 dir = Vector3.Normalize(_this._targetPosition - _this._cacheTransform.position);
        switch (_this._forwardType)
        {
            case ForwardType.Y:
                _this._cacheTransform.rotation = GetLookAtRotation(dir, Vector3.up);
                break;
            case ForwardType.X:
                _this._cacheTransform.rotation = GetLookAtRotation(dir, Vector3.right);
                break;
        }
    }

    private void Awake()
    {
        _cacheTransform = base.transform;
        if (_forwardType == ForwardType.Z)
        {
            _updateGaze = UpdateGazeForwardZ;
        }
        else
        {
            _updateGaze = UpdateGaze;
        }
    }

    public void SetGazePosition(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
    }

    public void AlterLateUpdate()
    {
        if (_isEnable)
        {
            _updateGaze(this);
        }
    }
}
