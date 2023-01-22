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
        Plane
    }

    [SerializeField]
    public string _collisionName;

    [SerializeField]
    public string _targetObjectName;

    [SerializeField]
    public Vector3 _offset;

    [SerializeField]
    public Vector3 _offset2;

    [SerializeField]
    public float _radius;

    [SerializeField]
    public CollisionType _type;

    public Transform _transform;

    public float _distance;
    public Vector3 _normal;
    public bool _isInner;
    public string collisionName
    {
        get
        {
            return _collisionName;
        }
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
        if (_collisionName == string.Empty)
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
        _transform.SetParent(gameObject.transform, false);
        switch (_type)
        {
            case CollisionType.Sphere:
                _transform.position += _offset;
                break;
        }
        _transform.name = _collisionName;
        CySpringCollisionComponent cySpringCollisionComponent = gameObject2.AddComponent<CySpringCollisionComponent>();
        cySpringCollisionComponent.CollisionType = _type;
        cySpringCollisionComponent.Radius = _radius;
        cySpringCollisionComponent.Offset = _offset;
        cySpringCollisionComponent.Offset2 = _offset2;
        return cySpringCollisionComponent;
    }
}