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
    public bool _isOtherTarget;

    [SerializeField]
    public Vector3 _offset;

    [SerializeField]
    public Vector3 _offset2;

    [SerializeField]
    public float _radius;

    [SerializeField]
    public float _distance;

    [SerializeField]
    public Vector3 _normal;

    [SerializeField]
    public CollisionType _type;

    [SerializeField]
    public bool _isInner;

    public Transform _transform;
}