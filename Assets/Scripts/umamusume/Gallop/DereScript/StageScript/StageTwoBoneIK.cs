// Stage.StageTwoBoneIK
using IK;
using Stage;
using UnityEngine;

public class StageTwoBoneIK
{
    public enum TargetType
    {
        Position,
        Transform
    }

    private bool _isInitialize;

    private TwoBoneIK _twoBoneIk;

    private Transform _jointPole;

    private TargetType _targetPositionType;

    private Vector3 _targetPosition;

    private Transform _targetTransform;

    private Transform _rootTransform;

    public float BlendValue
    {
        get
        {
            return _twoBoneIk.BlendAlpha;
        }
        set
        {
            _twoBoneIk.BlendAlpha = value;
        }
    }

    public TwoBoneIK.EffectorRotationType EffectorRotationType
    {
        get
        {
            return _twoBoneIk.EndEffectorRotationType;
        }
        set
        {
            _twoBoneIk.EndEffectorRotationType = value;
        }
    }

    public void SetTargetTransform(Transform targetTransform)
    {
        _targetPositionType = TargetType.Transform;
        _targetTransform = targetTransform;
        _twoBoneIk.EndEffectorRotationType = TwoBoneIK.EffectorRotationType.World;
    }

    public void SetTargetPosition(Vector3 position)
    {
        _targetPositionType = TargetType.Position;
        _targetPosition = position;
    }

    public void Initialize(Transform endEffector, Transform jointPole, Transform root)
    {
        _isInitialize = jointPole != null;
        _jointPole = jointPole;
        _rootTransform = root;
        _twoBoneIk = new TwoBoneIK();
        _twoBoneIk.Initialize(endEffector);
    }

    public void Solve()
    {
        if (_isInitialize && !(BlendValue <= 0f))
        {
            TargetType targetPositionType = _targetPositionType;
            Vector3 targetPosition;
            if (targetPositionType == TargetType.Position || targetPositionType != TargetType.Transform)
            {
                targetPosition = _targetPosition;
                _twoBoneIk.EndEffectorRotation = _jointPole.localRotation;
            }
            else
            {
                targetPosition = _targetTransform.position;
                _twoBoneIk.EndEffectorRotation = _rootTransform.localRotation * _targetTransform.localRotation;
            }
            _twoBoneIk.TargetPosition = targetPosition;
            _twoBoneIk.JointTargetPosition = _jointPole.position;
            _twoBoneIk.Solve();
        }
    }
}
