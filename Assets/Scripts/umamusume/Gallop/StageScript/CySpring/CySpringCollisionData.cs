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
}