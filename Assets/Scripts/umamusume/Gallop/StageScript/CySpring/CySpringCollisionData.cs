using System;
using UnityEngine;

[Serializable]
public class CySpringCollisionData
{
    public enum CollisionType
    {
        Sphere,
        None,
        Capsule,
        Plane,
    }

    private const string CollisionScaleTargetName = "S_Col";

    [SerializeField]
    private string _collisionName;

    [SerializeField]
    private string _targetObjectName;

    [SerializeField]
    private Vector3 _offset;

    [SerializeField]
    private Vector3 _offset2;

    [SerializeField]
    private float _radius;

    [SerializeField]
    private CollisionType _type;

    private Transform _transform;

    public bool _isOtherTarget;
    public float _distance;
    public Vector3 _normal;
    public bool _isInner;

    public string collisionName
    {
        get
        {
            return _collisionName;
        }
        set
        {
            _collisionName = value;
        }
    }

    public CySpringCollisionData()
    {
    }

    public CySpringCollisionData(CySpringCollisionData refData)
    {
        _type = refData._type;
        _collisionName = refData._collisionName;
        _targetObjectName = refData._targetObjectName;
        _offset = refData._offset;
        _radius = refData._radius;
    }

    public CySpringCollisionData(string collisionName, string targetObjectName, Vector3 offset, float radius)
    {
        _type = CollisionType.Sphere;
        _collisionName = collisionName;
        _targetObjectName = targetObjectName;
        _offset = offset;
        _radius = radius;
    }

    public CySpringCollisionData(CollisionType type, string collisionName, string targetObjectName, Vector3 offset, Vector3 offset2, float radius)
    {
        _type = type;
        _collisionName = collisionName;
        _targetObjectName = targetObjectName;
        _offset = offset;
        _offset2 = offset2;
        _radius = radius;
    }

    public CySpringCollisionComponent Create(CySpringCollision root)
    {
        if (root == null)
        {
            return null;
        }
        if (_collisionName == "")
        {
            return null;
        }
        if (_radius == 0f)
        {
            return null;
        }
        GameObject gameObject = CySpring.FindGameObject(root.gameObject, _targetObjectName);
        if (gameObject == null)
        {
            return null;
        }
        GameObject gameObject2 = new GameObject();
        _transform = gameObject2.transform;
        _transform.SetParent(gameObject.transform, worldPositionStays: false);
        float num = 1f;
        if (_collisionName.IndexOf("S_Col", StringComparison.Ordinal) == 0)
        {
            num = root.CollisionScale;
        }
        switch (_type)
        {
            case CollisionType.Sphere:
                _transform.position += _offset * num;
                break;
        }
        _transform.name = _collisionName;
        CySpringCollisionComponent cySpringCollisionComponent = gameObject2.AddComponent<CySpringCollisionComponent>();
        cySpringCollisionComponent.CollisionType = _type;
        cySpringCollisionComponent.Radius = _radius * num;
        cySpringCollisionComponent.Offset = _offset * num;
        cySpringCollisionComponent.Offset2 = _offset2 * num;
        return cySpringCollisionComponent;
    }
}
