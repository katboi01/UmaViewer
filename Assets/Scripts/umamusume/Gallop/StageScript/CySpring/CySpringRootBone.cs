using System.Collections.Generic;
using UnityEngine;

public class CySpringRootBone : CySpringBoneBase {
  public List<string> _collisionNameList;

  private List<CySpringParamDataChildElement> _childElements;

  private int _depth;

  private List<CySpringBoneBase> _childBoneList;

  private bool _applyCollision = true;

  public NativeClothWorking[] _natives;

  public bool _isSkip;

  public bool ApplyCollision {
    get {
      return _applyCollision;
    }
  }

  public List<CySpringParamDataChildElement> ChildElements {
    get {
      return _childElements;
    }
  }

  public List<CySpringBoneBase> ChildBoneList {
    get {
      return _childBoneList;
    }
  }

  public void CreateRoot(CySpringParamDataElement element, CySpring root, CySpringRootBone rootBone, CySpringBoneBase parentBone) {
    _boneName = element._boneName;
    _collisionNameList = element._collisionNameList;
    _childElements = element._childElements;
    Transform transform = root.gameObject.transform;
    Vector3 position = transform.position;
    Quaternion rotation = transform.rotation;
    transform.position = Vector3.zero;
    transform.rotation = Quaternion.identity;
    Create(root, rootBone, parentBone);
    if (_exist) {
      _childBoneList = new List<CySpringBoneBase>();
      foreach (Transform item in _transform) {
        if (!(item == _transform) && !(item.GetComponent<CySpringCollisionComponent>() != null)) {
          CySpringBoneBase cySpringBoneBase = new CySpringBoneBase(item.name);
          _childBoneList.Add(cySpringBoneBase);
          cySpringBoneBase.CreateChild(_root, _rootBone, this);
        }
      }
      _depth = _childBoneList.Count;
      _natives = new NativeClothWorking[_childBoneList.Count + 1];
      for (int i = 0; i < _natives.Length; i++) {
        _natives[i]._stiffnessForce = 0.0001f;
        _natives[i]._dragForce = 0.05f;
        _natives[i]._springForce = new Vector3(0f, 0f, 0f);
        _natives[i]._collisionRadius = 0.01f;
        _natives[i]._boneAxis = new Vector3(0f, 1f, 0f);
        _natives[i]._gravity = 0f;
      }
      Initialize(0);
      for (int j = 0; j < _childBoneList.Count; j++) {
        _childBoneList[j].Initialize(j + 1);
      }
      _natives[0]._stiffnessForce = element._stiffnessForce;
      _natives[0]._dragForce = element._dragForce;
      _natives[0]._springForce = element._springForce;
      _natives[0]._collisionRadius = element._collisionRadius;
      _natives[0]._gravity = element._gravity;
      transform.position = position;
      transform.rotation = rotation;
    }
  }

  private void UpdateCollisionIndexList() {
    if (_collisionNameList != null && _collisionNameList.Count != 0 && _root.CollisionList != null && _root.CollisionList.Count != 0) {
      CreateCollisionIndexList();
      foreach (string collisionName in _collisionNameList) {
        for (int i = 0; i < _root.CollisionList.Count; i++) {
          CySpringCollisionComponent cySpringCollisionComponent = _root.CollisionList[i];
          if (!(collisionName != cySpringCollisionComponent.name)) {
            AddCollisionIndex(i);
          }
        }
      }
    }
  }

  public void SetCollision() {
    UpdateCollisionIndexList();
    ChangeRootValue();
  }

  public void GathreAllSpring() {
    if (_depth < 0) {
      _isSkip = true;
    } else {
      _isSkip = false;
      GatherSpring();
      for (int i = 0; i < _childBoneList.Count; i++) {
        _childBoneList[i].GatherSpring();
      }
    }
  }

  public void PostAllSpring() {
    if (!_isSkip) {
      PostSpring();
      for (int i = 0; i < _childBoneList.Count; i++) {
        _childBoneList[i].PostSpring();
      }
    }
  }

  public override bool Reset(bool bForceReset = true) {
    if (base.Reset(bForceReset)) {
      if (_childBoneList == null) {
        return false;
      }
      foreach (CySpringBoneBase childBone in _childBoneList) {
        childBone.Reset(bForceReset);
      }
    }
    return true;
  }

  public void ChangeRootValue() {
    if (_childBoneList != null) {
      _rootBone._natives[_index]._existCollision = _rootBone._applyCollision;
      foreach (CySpringBoneBase childBone in _childBoneList) {
        childBone.ChangeValue();
      }
    }
  }
}