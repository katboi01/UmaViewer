using System.Collections.Generic;
using UnityEngine;

public class CySpringBoneBase {
  protected string _boneName;

  protected CySpring _root;

  protected CySpringRootBone _rootBone;

  protected CySpringBoneBase _parentBone;

  protected GameObject _gameObject;

  protected Transform _transform;

  protected bool _exist;

  public int _index;

  private List<int> _collisionIndexList;

  public string BoneName {
    get {
      return _boneName;
    }
  }

  public GameObject GameObject {
    get {
      return _gameObject;
    }
  }

  public float StiffnessForce {
    get {
      return _rootBone._natives[_index]._stiffnessForce;
    }
  }

  public float DragForce {
    get {
      return _rootBone._natives[_index]._dragForce;
    }
  }

  public Vector3 SpringForce {
    get {
      return _rootBone._natives[_index]._springForce;
    }
  }

  public float CollisionRadius {
    get {
      return _rootBone._natives[_index]._collisionRadius;
    }
  }

  public float Gravity {
    get {
      return _rootBone._natives[_index]._gravity;
    }
  }

  public float InitBoneDistance {
    get {
      return _rootBone._natives[_index]._initBoneDistance;
    }
    set {
      _rootBone._natives[_index]._initBoneDistance = value;
    }
  }

  public Vector3 BoneAxis {
    get {
      return _rootBone._natives[_index]._boneAxis;
    }
    set {
      _rootBone._natives[_index]._boneAxis = value;
    }
  }

  public bool Exist {
    get {
      return _exist;
    }
    set {
      _exist = value;
    }
  }

  public List<int> CollisionIndexList {
    get {
      return _collisionIndexList;
    }
  }

  public CySpringBoneBase() { }

  public CySpringBoneBase(string name) {
    _boneName = name;
  }

  public void Create(CySpring root, CySpringRootBone rootBone, CySpringBoneBase parentBone) {
    _root = root;
    _rootBone = rootBone;
    _parentBone = parentBone;
    _exist = false;
    if (!(_root == null) && _rootBone != null) {
      if (_parentBone != null) {
        _gameObject = CySpring.FindGameObject(_parentBone.GameObject, _boneName);
      } else {
        _gameObject = CySpring.FindGameObject(_root.gameObject, _boneName);
      }
      if (_gameObject == null) {
        _exist = false;
      } else {
        _transform = _gameObject.transform;
        _exist = true;
      }
    }
  }

  public void CreateChild(CySpring root, CySpringRootBone rootBone, CySpringBoneBase parentBone) {
    Create(root, rootBone, parentBone);
    foreach (Transform item in _transform) {
      if (!(item == _transform) && !(item.GetComponent<CySpringCollisionComponent>() != null)) {
        CySpringBoneBase cySpringBoneBase = new CySpringBoneBase(item.name);
        _rootBone.ChildBoneList.Add(cySpringBoneBase);
        cySpringBoneBase.CreateChild(_root, _rootBone, this);
      }
    }
  }

  public virtual void Initialize(int idx) {
    _index = idx;
    _rootBone._natives[_index]._initLocalRotation = _transform.localRotation;
    _rootBone._natives[_index]._currentPosition = _transform.position;
    _rootBone._natives[_index]._prevPosition = _rootBone._natives[_index]._currentPosition;
    _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
    if (_parentBone != null) {
      _parentBone.InitBoneDistance = Vector3.Distance(_transform.position, _parentBone.GameObject.transform.position);
      if (_parentBone.InitBoneDistance == 0f) {
        _parentBone.Exist = false;
        _exist = false;
      } else {
        _parentBone.BoneAxis = (_gameObject.transform.position - _parentBone.GameObject.transform.position).normalized;
      }
      if (_transform.childCount == 0) {
        _rootBone._natives[_index]._boneAxis = _parentBone.BoneAxis;
        _rootBone._natives[_index]._initBoneDistance = _parentBone.InitBoneDistance;
      }
    } else {
      _rootBone._natives[_index]._boneAxis = Vector3.down;
      _rootBone._natives[_index]._initBoneDistance = 1f;
    }
  }

  public void CreateCollisionIndexList() {
    _collisionIndexList = new List<int>();
    _rootBone._natives[_index]._activeCollision = 0;
  }

  protected void AddCollisionIndex(int i) {
    CollisionIndexList.Add(i);
    switch (_rootBone._natives[_index]._activeCollision) {
      case 0:
        _rootBone._natives[_index]._cIndex0 = (short) i;
        break;
      case 1:
        _rootBone._natives[_index]._cIndex1 = (short) i;
        break;
      case 2:
        _rootBone._natives[_index]._cIndex2 = (short) i;
        break;
      case 3:
        _rootBone._natives[_index]._cIndex3 = (short) i;
        break;
      case 4:
        _rootBone._natives[_index]._cIndex4 = (short) i;
        break;
      case 5:
        _rootBone._natives[_index]._cIndex5 = (short) i;
        break;
      case 6:
        _rootBone._natives[_index]._cIndex6 = (short) i;
        break;
      case 7:
        _rootBone._natives[_index]._cIndex7 = (short) i;
        break;
    }
    _rootBone._natives[_index]._activeCollision++;
  }

  public void GatherSpring() {
    if (_root.sqartDeltaTime == 0f) {
      _transform.localRotation = _rootBone._natives[_index]._initLocalRotation;
    } else {
      _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
      if (_index == 0) {
        _rootBone._natives[_index]._oldPosition = _transform.position;
        _rootBone._natives[_index]._oldRotation = _rootBone._natives[_index]._initLocalRotation * _transform.parent.rotation;
      }
    }
  }

  public void PostSpring() {
    if (_root.sqartDeltaTime != 0f) {
      _transform.rotation = _rootBone._natives[_index]._finalRotation;
    }
  }

  public virtual bool Reset(bool bForceReset = true) {
    if (_transform == null || (_rootBone._natives[_index]._gravity != 0f && !bForceReset)) {
      return false;
    }
    _transform.localRotation = _rootBone._natives[_index]._initLocalRotation;
    _rootBone._natives[_index]._oldPosition = _transform.position;
    _rootBone._natives[_index]._currentPosition = _rootBone._natives[_index]._diff * _rootBone._natives[_index]._initBoneDistance + _rootBone._natives[_index]._oldPosition;
    _rootBone._natives[_index]._oldRotation = _rootBone._natives[_index]._initLocalRotation * _transform.parent.rotation;
    _rootBone._natives[_index]._aimVector = _rootBone._natives[_index]._oldRotation * _rootBone._natives[_index]._boneAxis;
    _rootBone._natives[_index]._prevPosition = _rootBone._natives[_index]._currentPosition - _rootBone._natives[_index]._aimVector;
    _rootBone._natives[_index]._finalRotation = _transform.rotation;
    return true;
  }

  public void ChangeValue() {
    _rootBone._natives[_index]._existCollision = _rootBone.ApplyCollision;
    CreateCollisionIndexList();
    if (_rootBone.ChildElements != null) {
      string boneName = (_index != 0) ? _rootBone.ChildBoneList[_index - 1].BoneName : _boneName;
      CySpringParamDataChildElement cySpringParamDataChildElement = _rootBone.ChildElements.Find((CySpringParamDataChildElement child) => child._boneName == boneName);
      if (cySpringParamDataChildElement != null) {
        _rootBone._natives[_index]._stiffnessForce = cySpringParamDataChildElement._stiffnessForce;
        _rootBone._natives[_index]._dragForce = cySpringParamDataChildElement._dragForce;
        _rootBone._natives[_index]._springForce = cySpringParamDataChildElement._springForce;
        _rootBone._natives[_index]._collisionRadius = cySpringParamDataChildElement._collisionRadius;
        _rootBone._natives[_index]._gravity = cySpringParamDataChildElement._gravity;
        if (cySpringParamDataChildElement._collisionNameList != null && cySpringParamDataChildElement._collisionNameList.Count > 0) {
          foreach (string collisionName in cySpringParamDataChildElement._collisionNameList) {
            for (int i = 0; i < _root.CollisionList.Count; i++) {
              CySpringCollisionComponent cySpringCollisionComponent = _root.CollisionList[i];
              if (!(collisionName != cySpringCollisionComponent.name)) {
                AddCollisionIndex((short) i);
              }
            }
          }
        }
        return;
      }
    }
    _rootBone._natives[_index]._stiffnessForce = _rootBone.StiffnessForce;
    _rootBone._natives[_index]._dragForce = _rootBone.DragForce;
    _rootBone._natives[_index]._springForce = _rootBone.SpringForce;
    _rootBone._natives[_index]._collisionRadius = _rootBone.CollisionRadius;
    _rootBone._natives[_index]._gravity = _rootBone.Gravity;
    if (_rootBone.CollisionIndexList != null) {
      for (int j = 0; j < _rootBone.CollisionIndexList.Count; j++) {
        AddCollisionIndex(_rootBone.CollisionIndexList[j]);
      }
    }
  }
}