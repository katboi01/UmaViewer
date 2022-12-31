using System.Collections.Generic;
using UnityEngine;

public class CySpringBoneBase
{
    protected string _boneName;

    protected CySpring _root;

    protected CySpringRootBone _rootBone;

    protected CySpringBoneBase _parentBone;

    protected GameObject _gameObject;

    protected Transform _transform;

    protected Transform _parentTransform;

    protected bool _exist;

    public int _index;

    public CySpringParamDataChildElement _element;

    private List<int> _collisionIndexList;

    public string BoneName => _boneName;

    public GameObject GameObject => _gameObject;

    public float StiffnessForce => _rootBone._natives[_index]._stiffnessForce;

    public float DragForce => _rootBone._natives[_index]._dragForce;

    public Vector3 SpringForce => _rootBone._natives[_index]._springForce;

    public float CollisionRadius => _rootBone._natives[_index]._collisionRadius;

    public float Gravity => _rootBone._natives[_index]._gravity;

    public CySpring.eCapability Capability => _rootBone._natives[_index]._capability;

    public int CapsGroupIndex => _rootBone._natives[_index]._capsGroupIndex;

    public float InitBoneDistance
    {
        get
        {
            return _rootBone._natives[_index]._initBoneDistance;
        }
        set
        {
            _rootBone._natives[_index]._initBoneDistance = value;
        }
    }

    public Vector3 BoneAxis
    {
        get
        {
            return _rootBone._natives[_index]._boneAxis;
        }
        set
        {
            _rootBone._natives[_index]._boneAxis = value;
        }
    }

    public bool Exist
    {
        get
        {
            return _exist;
        }
        set
        {
            _exist = value;
        }
    }

    public bool IsWind
    {
        get
        {
            return (_rootBone._natives[_index]._wind & 1) != 0;
        }
        set
        {
            if (value)
            {
                _rootBone._natives[_index]._wind |= 1;
            }
            else
            {
                _rootBone._natives[_index]._wind &= -2;
            }
        }
    }

    public float WindGroupIndex
    {
        get
        {
            return _rootBone._natives[_index]._windGroupIndex;
        }
        set
        {
            _rootBone._natives[_index]._windGroupIndex = value;
        }
    }

    public List<int> CollisionIndexList => _collisionIndexList;

    public CySpringBoneBase()
    {
    }

    public CySpringBoneBase(string name)
    {
        _boneName = name;
    }

    public void Create(CySpring root, CySpringRootBone rootBone, CySpringBoneBase parentBone)
    {
        _root = root;
        _rootBone = rootBone;
        _parentBone = parentBone;
        _exist = false;
        if (!(_root == null) && _rootBone != null)
        {
            if (_parentBone != null)
            {
                _gameObject = CySpring.FindGameObject(_parentBone.GameObject, _boneName);
            }
            else
            {
                _gameObject = CySpring.FindGameObject(_root.gameObject, _boneName);
            }
            if (_gameObject == null)
            {
                _exist = false;
                return;
            }
            _transform = _gameObject.transform;
            _parentTransform = _transform.parent;
            _exist = true;
        }
    }

    public void CreateChild(CySpring root, CySpringRootBone rootBone, CySpringBoneBase parentBone)
    {
        Create(root, rootBone, parentBone);
        foreach (Transform item in _transform)
        {
            if (!(item == _transform) && !(item.GetComponent<CySpringCollisionComponent>() != null))
            {
                CySpringBoneBase cySpringBoneBase = new CySpringBoneBase(item.name);
                _rootBone.TmpChildBoneList.Add(cySpringBoneBase);
                cySpringBoneBase.CreateChild(_root, _rootBone, this);
            }
        }
    }

    public virtual void Initialize(int idx)
    {
        _index = idx;
        _rootBone._natives[_index]._initLocalRotation = _transform.localRotation;
        _rootBone._natives[_index]._currentPosition = _transform.position;
        _rootBone._natives[_index]._prevPosition = _rootBone._natives[_index]._currentPosition;
        _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
        if (_parentBone != null)
        {
            _parentBone.InitBoneDistance = Vector3.Distance(_transform.position, _parentBone.GameObject.transform.position);
            if (_parentBone.InitBoneDistance == 0f)
            {
                _parentBone.Exist = false;
                _exist = false;
            }
            else
            {
                _parentBone.BoneAxis = (_gameObject.transform.position - _parentBone.GameObject.transform.position).normalized;
            }
            if (_transform.childCount == 0)
            {
                _rootBone._natives[_index]._boneAxis = _parentBone.BoneAxis;
                _rootBone._natives[_index]._initBoneDistance = _parentBone.InitBoneDistance;
            }
        }
        else
        {
            _rootBone._natives[_index]._boneAxis = Vector3.down;
            _rootBone._natives[_index]._initBoneDistance = 1f;
        }
    }

    public void CreateCollisionIndexList()
    {
        _collisionIndexList = new List<int>();
        _rootBone._natives[_index]._activeCollision = 0;
    }

    protected void AddCollisionIndex(int i)
    {
        CollisionIndexList.Add(i);
        switch (_rootBone._natives[_index]._activeCollision)
        {
            case 0:
                _rootBone._natives[_index]._cIndex0 = (short)i;
                break;
            case 1:
                _rootBone._natives[_index]._cIndex1 = (short)i;
                break;
            case 2:
                _rootBone._natives[_index]._cIndex2 = (short)i;
                break;
            case 3:
                _rootBone._natives[_index]._cIndex3 = (short)i;
                break;
            case 4:
                _rootBone._natives[_index]._cIndex4 = (short)i;
                break;
            case 5:
                _rootBone._natives[_index]._cIndex5 = (short)i;
                break;
            case 6:
                _rootBone._natives[_index]._cIndex6 = (short)i;
                break;
            case 7:
                _rootBone._natives[_index]._cIndex7 = (short)i;
                break;
            case 8:
                _rootBone._natives[_index]._cIndex8 = (short)i;
                break;
            case 9:
                _rootBone._natives[_index]._cIndex9 = (short)i;
                break;
            case 10:
                _rootBone._natives[_index]._cIndex10 = (short)i;
                break;
            case 11:
                _rootBone._natives[_index]._cIndex11 = (short)i;
                break;
            case 12:
                _rootBone._natives[_index]._cIndex12 = (short)i;
                break;
            case 13:
                _rootBone._natives[_index]._cIndex13 = (short)i;
                break;
            case 14:
                _rootBone._natives[_index]._cIndex14 = (short)i;
                break;
            case 15:
                _rootBone._natives[_index]._cIndex15 = (short)i;
                break;
            case 16:
                _rootBone._natives[_index]._cIndex16 = (short)i;
                break;
            case 17:
                _rootBone._natives[_index]._cIndex17 = (short)i;
                break;
            case 18:
                _rootBone._natives[_index]._cIndex18 = (short)i;
                break;
            case 19:
                _rootBone._natives[_index]._cIndex19 = (short)i;
                break;
        }
        _rootBone._natives[_index]._activeCollision++;
    }

    public void GatherSpring()
    {
        if (_root.sqartDeltaTime == 0f)
        {
            _transform.localRotation = _rootBone._natives[_index]._initLocalRotation;
        }
        else
        {
            _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
        }
    }

    public void DoGatherSpring()
    {
        _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
    }

    public void DoGatherSpringReset()
    {
        _transform.localRotation = _rootBone._natives[_index]._initLocalRotation;
    }

    public void PostSpring()
    {
        if (_root.sqartDeltaTime != 0f)
        {
            _transform.rotation = _rootBone._natives[_index]._finalRotation;
        }
    }

    public void DoPostSpring()
    {
        _transform.rotation = _rootBone._natives[_index]._finalRotation;
    }

    public virtual bool Reset(bool bForceReset = true)
    {
        if (_transform == null || (_rootBone._natives[_index]._gravity != 0f && !bForceReset))
        {
            return false;
        }
        _transform.localRotation = _rootBone._natives[_index]._initLocalRotation;
        _rootBone._natives[_index]._oldPosition = _transform.position;
        _rootBone._natives[_index]._currentPosition = _rootBone._natives[_index]._diff * _rootBone._natives[_index]._initBoneDistance + _rootBone._natives[_index]._oldPosition;
        _rootBone._natives[_index]._oldRotation = _rootBone._natives[_index]._initLocalRotation * _parentTransform.rotation;
        _rootBone._natives[_index]._aimVector = _rootBone._natives[_index]._oldRotation * _rootBone._natives[_index]._boneAxis;
        _rootBone._natives[_index]._prevPosition = _rootBone._natives[_index]._currentPosition - _rootBone._natives[_index]._aimVector;
        _rootBone._natives[_index]._finalRotation = _transform.rotation;
        return true;
    }

    public void ChangeValue()
    {
        _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
        CreateCollisionIndexList();
        if (_rootBone.ChildElements != null)
        {
            string boneName = ((_index == 0) ? _boneName : _rootBone.ChildBoneList[_index - 1].BoneName);
            CySpringParamDataChildElement cySpringParamDataChildElement = (_element = _rootBone.ChildElements.Find((CySpringParamDataChildElement child) => child._boneName.Equals(boneName)));
            if (cySpringParamDataChildElement != null)
            {
                _rootBone._natives[_index]._stiffnessForce = cySpringParamDataChildElement._stiffnessForce;
                _rootBone._natives[_index]._dragForce = cySpringParamDataChildElement._dragForce;
                _rootBone._natives[_index]._springForce = cySpringParamDataChildElement._springForce;
                _rootBone._natives[_index]._collisionRadius = cySpringParamDataChildElement._collisionRadius;
                _rootBone._natives[_index]._gravity = cySpringParamDataChildElement._gravity;
                if (cySpringParamDataChildElement._collisionNameList == null || cySpringParamDataChildElement._collisionNameList.Count <= 0)
                {
                    return;
                }
                foreach (string collisionName in cySpringParamDataChildElement._collisionNameList)
                {
                    int i = 0;
                    for (int num = _root.CollisionList.Length; i < num; i++)
                    {
                        CySpringCollisionComponent cySpringCollisionComponent = _root.CollisionList[i];
                        if (!(cySpringCollisionComponent == null) && !(collisionName != cySpringCollisionComponent.name))
                        {
                            AddCollisionIndex((short)i);
                        }
                    }
                }
                return;
            }
        }
        _element = null;
        _rootBone._natives[_index]._stiffnessForce = _rootBone.StiffnessForce;
        _rootBone._natives[_index]._dragForce = _rootBone.DragForce;
        _rootBone._natives[_index]._springForce = _rootBone.SpringForce;
        _rootBone._natives[_index]._collisionRadius = _rootBone.CollisionRadius;
        _rootBone._natives[_index]._gravity = _rootBone.Gravity;
        if (_rootBone.CollisionIndexList != null)
        {
            for (int j = 0; j < _rootBone.CollisionIndexList.Count; j++)
            {
                AddCollisionIndex(_rootBone.CollisionIndexList[j]);
            }
        }
    }

    public virtual void ResetClothParameter()
    {
        if (_element == null)
        {
            _rootBone._natives[_index]._stiffnessForce = _rootBone.StiffnessForce;
            _rootBone._natives[_index]._dragForce = _rootBone.DragForce;
            _rootBone._natives[_index]._gravity = _rootBone.Gravity;
        }
        else
        {
            _rootBone._natives[_index]._stiffnessForce = _element._stiffnessForce;
            _rootBone._natives[_index]._dragForce = _element._dragForce;
            _rootBone._natives[_index]._gravity = _element._gravity;
        }
    }
}
