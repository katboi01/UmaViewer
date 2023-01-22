using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothController {
  public class Cloth {
    private CySpringCollisionDataAsset _colAsset;

    private CySpringParamDataAsset _prmAsset;

    private CySpringCollision _collision;

    private CySpring _spring;

    public CySpringCollisionDataAsset colAsset {
      get {
        return _colAsset;
      }
    }

    public CySpringParamDataAsset prmAsset {
      get {
        return _prmAsset;
      }
    }

    public CySpringCollision collision {
      get {
        return _collision;
      }
      set {
        _collision = value;
      }
    }

    public CySpring spring {
      get {
        return _spring;
      }
      set {
        _spring = value;
      }
    }
    public Cloth(CySpringCollisionDataAsset collisionDataAsset, CySpringParamDataAsset paramDataAsset) {
      _colAsset = collisionDataAsset;
      _prmAsset = paramDataAsset;
    }
  }

  public static readonly string UNION_COLLISION_NAME = "ColUnion";

  public static readonly float UNION_COLLISION_RADIUS = 0.45f;

  public static readonly string GROUND_COLLISION_NAME = "ColGround";

  public static readonly float GROUND_SPHERE_RADIUS = 1e+006f;

  private bool _bReady;

  private GameObject _targetObject;

  private Transform _targetTransform;

  private Vector3 _bakPosition = Vector3.zero;

  private Quaternion _bakRotation = Quaternion.identity;

  private List<Cloth> _lstCloth = new List<Cloth>();

  private bool _useUnionCollision;

  private CySpringCollisionComponent _unionCollision;

  public bool isReady {
    get {
      return _bReady;
    }
  }

  public bool useUnionCollision {
    get {
      return _useUnionCollision;
    }
  }

  public CySpringCollisionComponent unionCollision {
    get {
      return _unionCollision;
    }
  }

  public int clothCount {
    get {
      return _lstCloth.Count;
    }
  }

  public ClothController(GameObject target) {
    _targetObject = target;
    _targetTransform = target.transform;
    _bReady = false;
  }

  private void Destroy(UnityEngine.Object obj) {
    if (obj != null) {
      UnityEngine.Object.Destroy(obj);
    }
    _bReady = false;
  }

  private HashSet<string> BuildUsableCollisionList() {
    if (_lstCloth != null && _lstCloth.Count > 0) {
      HashSet<string> setUsableCollision = new HashSet<string>();

      _useUnionCollision = false;

      Action<CySpringParamDataAsset> fnBuildCollisionSet = delegate(CySpringParamDataAsset prmAsset) {
        Action<List<string>> fnAddusableCollision = delegate(List<string> lstCollision) {
          for (int l = 0; l < lstCollision.Count; l++) {
            if (!setUsableCollision.Contains(lstCollision[l])) {
              setUsableCollision.Add(lstCollision[l]);
            }
          }
        };
        Action<List<CySpringParamDataChildElement>> action = delegate(List<CySpringParamDataChildElement> lstChild) {
          for (int k = 0; k < lstChild.Count; k++) {
            CySpringParamDataChildElement cySpringParamDataChildElement = lstChild[k];
            fnAddusableCollision(cySpringParamDataChildElement._collisionNameList);
          }
        };
        for (int j = 0; j < prmAsset._elements.Count; j++) {
          CySpringParamDataElement cySpringParamDataElement = prmAsset._elements[j];
          fnAddusableCollision(cySpringParamDataElement._collisionNameList);
          action(cySpringParamDataElement._childElements);
        }
      };

      for (int i = 0; i < _lstCloth.Count; i++) {
        fnBuildCollisionSet(_lstCloth[i].prmAsset);
      }

      return setUsableCollision;
    }
    return null;
  }

  public void AddCloth(CySpringCollisionDataAsset colAsset, CySpringParamDataAsset prmAsset) {
    _lstCloth.Add(new Cloth(colAsset, prmAsset));
  }

  public void CreateCollision(CySpringCollisionComponent.ePurpose purpose, int idxMainCloth, float unionCollisionScale = 1f) {
    if (_lstCloth != null) {
      var hashlist = BuildUsableCollisionList();

      for (int i = 0; i < _lstCloth.Count; i++) {
        _lstCloth[i].collision = _targetObject.AddComponent<CySpringCollision>();
        _lstCloth[i].collision.Create(_lstCloth[i].colAsset, hashlist);
      }
      /*
      if (_lstCloth.Count > idxMainCloth)
      {
          _lstCloth[idxMainCloth].collision = _targetObject.AddComponent<CySpringCollision>();
          _lstCloth[idxMainCloth].collision.Create(_lstCloth[idxMainCloth].colAsset, hashlist);

          var comp = _lstCloth[idxMainCloth].collision.CreateCollision("Chest", UNION_COLLISION_NAME, UNION_COLLISION_RADIUS * unionCollisionScale);
          comp.purpose = CySpringCollisionComponent.ePurpose.Union;

          CySpring.AddUnionCollision(comp);

          if (hashlist.Contains(GROUND_COLLISION_NAME))
          {
              var comp2 = _lstCloth[idxMainCloth].collision.CreateCollision("Position", GROUND_COLLISION_NAME, GROUND_SPHERE_RADIUS);
              comp2.purpose = CySpringCollisionComponent.ePurpose.Ground;
          }
      }
      */
    }
  }

  public void BindSpring() {
    if (_lstCloth != null) {
      for (int i = 0; i < _lstCloth.Count; i++) {
        _lstCloth[i].spring = _targetObject.AddComponent<CySpring>();
        _lstCloth[i].spring.Bind(_lstCloth[i].prmAsset);
      }
    }
    _bReady = true;
  }

  public void Reset(bool bForceReset) {
    if (_lstCloth != null) {
      for (int i = 0; i < _lstCloth.Count; i++) {
        Debug.LogWarning(_lstCloth[i].spring);
        _lstCloth[i].spring.Reset(bForceReset);
      }
    }
  }

  public void Reset(int idx, bool bForceReset) {
    if (_lstCloth != null && _lstCloth.Count > idx) {
      _lstCloth[idx].spring.Reset(bForceReset);
    }
  }

  public void Release() {
    if (_lstCloth != null) {
      for (int i = 0; i < _lstCloth.Count; i++) {
        Destroy(_lstCloth[i].spring);
        Destroy(_lstCloth[i].collision);
      }
      _lstCloth.Clear();
      _bReady = false;
    }
  }

  public CySpringCollision GetCollision(int idx) {
    if (_lstCloth.Count > idx) {
      return _lstCloth[idx].collision;
    }
    return null;
  }

  public CySpring GetSpring(int idx) {
    if (_lstCloth.Count > idx) {
      return _lstCloth[idx].spring;
    }
    return null;
  }

  public bool Update(bool isGather = true) {
    if (_lstCloth != null && _lstCloth.Count > 0 && _bReady) {
      if (_unionCollision != null) {
        //_unionCollision.UpdateUnionCollision()
      }
      _bakPosition = _targetTransform.position;
      _bakRotation = _targetTransform.rotation;
      _targetTransform.position = Vector3.zero;
      _targetTransform.rotation = Quaternion.identity;

      if (isGather) {
        float smoothDeltaTime = Time.smoothDeltaTime;
        for (int i = 0; i < _lstCloth.Count; i++) {
          _lstCloth[i].spring.GatherSpring(smoothDeltaTime);
        }
      }
      for (int j = 0; j < _lstCloth.Count; j++) {
        _lstCloth[j].spring.UpdateSpring();
      }
      for (int j = 0; j < _lstCloth.Count; j++) {
        _lstCloth[j].spring.PostSpring();
      }
      _targetTransform.position = _bakPosition;
      _targetTransform.rotation = _bakRotation;

      return true;
    }
    return false;
  }

  public void ForceUpdate(float fCollisionOffSec, float fCollisionOnSec) {
    //最初の位置を確保
    Vector3 position = _targetTransform.position;
    Quaternion rotation = _targetTransform.rotation;

    _targetTransform.position = Vector3.zero;
    _targetTransform.rotation = Quaternion.identity;
    if (_lstCloth.Count > 0) {
      float num = 0.0166666675f;
      int num2 = (int) Mathf.Floor(fCollisionOffSec / num);
      int num3 = (int) Mathf.Floor(fCollisionOnSec / num);
      //CollisionSwitchをオフに
      for (int i = 0; i < _lstCloth.Count; i++) {
        _lstCloth[i].spring.CollisionSwitch = false;
      }
      //UpdateSpring
      for (int i = 0; i < num2; i++) {
        for (int j = 0; j < _lstCloth.Count; j++) {
          _lstCloth[j].spring.UpdateSpring();
        }
      }
      //CollisionSwitchをオンに
      for (int i = 0; i < _lstCloth.Count; i++) {
        _lstCloth[i].spring.CollisionSwitch = true;
      }
      //UpdateSpring
      for (int i = 0; i < num3; i++) {
        for (int j = 0; j < _lstCloth.Count; j++) {
          _lstCloth[j].spring.UpdateSpring();
        }
      }
      //PostSpring
      for (int i = 0; i < _lstCloth.Count; i++) {
        _lstCloth[i].spring.PostSpring();
      }
    }

    //戻すよ
    _targetTransform.position = position;
    _targetTransform.rotation = rotation;
  }
}