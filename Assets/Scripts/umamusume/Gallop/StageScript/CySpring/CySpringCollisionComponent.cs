using System;
using UnityEngine;

public class CySpringCollisionComponent : MonoBehaviour
{
    public enum ePurpose
    {
        None = 0,
        Generic = 1,
        Union = 2,
        Ground = 4
    }

    [NonSerialized]
    public float _radius;

    [SerializeField]
    private CySpringCollisionData.CollisionType _collisionType;

    private Vector3 _offset = new Vector3(0f, 0f, 0f);

    private Vector3 _offset2 = new Vector3(0f, 0f, 0f);

    private Transform _cacheTransform;

    private ePurpose _purpose = ePurpose.Generic;

    private Vector3 _unionCollisionPosition = Vector3.zero;

    private int _nativeIndex;

    private NativeClothCollision[] _nativeClothCollision;

    public float Radius
    {
        get
        {
            return _radius;
        }
        set
        {
            _radius = value;
        }
    }

    public CySpringCollisionData.CollisionType CollisionType
    {
        get
        {
            return _collisionType;
        }
        set
        {
            _collisionType = value;
        }
    }

    public Vector3 Offset
    {
        get
        {
            return _offset;
        }
        set
        {
            _offset = value;
        }
    }

    public Vector3 Offset2
    {
        get
        {
            return _offset2;
        }
        set
        {
            _offset2 = value;
        }
    }

    public Vector3 Rotation
    {
        get
        {
            return _offset2;
        }
        set
        {
            _offset2 = value;
        }
    }

    public Transform cacheTransform => _cacheTransform;

    public Vector3 unionCollisionPosition => _unionCollisionPosition;

    public ePurpose purpose
    {
        get
        {
            return _purpose;
        }
        set
        {
            _purpose = value;
        }
    }

    private void Awake()
    {
        _cacheTransform = GetComponent<Transform>();
    }

    public void UpdateUnionCollision()
    {
        _unionCollisionPosition = _cacheTransform.position;
    }

    public void EnableUnionCollision(bool bEnable)
    {
        _collisionType = ((!bEnable) ? CySpringCollisionData.CollisionType.None : CySpringCollisionData.CollisionType.Sphere);
    }

    private void UpdateNativeParameter()
    {
        if (_nativeClothCollision == null)
        {
            return;
        }
        switch (CollisionType)
        {
            case CySpringCollisionData.CollisionType.Sphere:
                switch (purpose)
                {
                    case ePurpose.Ground:
                        {
                            Vector3 offset = Offset;
                            offset.y -= _radius;
                            _nativeClothCollision[_nativeIndex]._position = offset;
                            break;
                        }
                    default:
                        _nativeClothCollision[_nativeIndex]._position = Offset;
                        break;
                    case ePurpose.Union:
                        break;
                }
                break;
            case CySpringCollisionData.CollisionType.Capsule:
                if (_cacheTransform != null)
                {
                    _nativeClothCollision[_nativeIndex]._position = Vector3.Scale(Offset, _cacheTransform.lossyScale);
                    _nativeClothCollision[_nativeIndex]._position2 = Vector3.Scale(Offset2, _cacheTransform.lossyScale);
                }
                else
                {
                    _nativeClothCollision[_nativeIndex]._position = Vector3.Scale(Offset, base.transform.lossyScale);
                    _nativeClothCollision[_nativeIndex]._position2 = Vector3.Scale(Offset2, base.transform.lossyScale);
                }
                break;
            case CySpringCollisionData.CollisionType.None:
                break;
        }
    }

    public void UpdateNativeUnionCollision(ref Vector3 rootPosition, ref Quaternion rootRotation)
    {
        if (CollisionType == CySpringCollisionData.CollisionType.Sphere && purpose == ePurpose.Union)
        {
            Vector3 vector = unionCollisionPosition - rootPosition;
            vector = Quaternion.Inverse(rootRotation) * vector;
            _nativeClothCollision[_nativeIndex]._position = vector;
        }
    }

    public void SetNativeCollision(NativeClothCollision[] collision, int index, int parentIndex)
    {
        _nativeIndex = index;
        _nativeClothCollision = collision;
        _nativeClothCollision[index]._radius = Radius;
        _nativeClothCollision[index]._type = (int)CollisionType;
        _nativeClothCollision[index]._purpose = (int)purpose;
        _nativeClothCollision[index]._parentIndex = parentIndex;
        UpdateNativeParameter();
    }
}
