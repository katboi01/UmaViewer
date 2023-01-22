using System;
using System.Collections.Generic;
using UnityEngine;

public class CySpring : MonoBehaviour {
  public enum eCapability {
    None,
    GravityControl,
    WindForce
  }

  private static List<CySpringCollisionComponent> _lstUnionCollision;

  private static CySpringCollisionComponent _groundCollision;

  private const float DEFAULT_SPECIFIC_RATE = 0.1f;

  private static float _stiffnessForceRate = 1f;

  private static float _dragForceRate = 1f;

  private static float _gravityRate = 1f;

  private bool _bCollisionSwitch = true;

  [SerializeField]
  private CySpringParamDataAsset _springData;

  //追加
  private bool _bUseUnionCollision;

  //追加
  private CySpringCollisionComponent _unionCollision;

  private List<CySpringCollisionComponent> _collisionList;

  public NativeClothCollision[] _nativeCollisions;

  [NonSerialized]
  private float _sqartDeltaTime;

  [SerializeField]
  private CySpringRootBone[] _boneList;

  [NonSerialized]
  private Transform _cachedTransform;

  //追加
  private bool _isUseSpecificValue;

  //追加
  private float _specificStiffnessForceRate = 0.1f;

  //追加
  private float _specificDragForceRate = 0.1f;

  //追加
  private float _specificGravityRate = 0.1f;

  //追加
  private const float HALF_FPS_RATE = 1.5f;

  public static float stiffnessForceRate {
    set {
      _stiffnessForceRate = value;
    }
  }

  public static float dragForceRate {
    set {
      _dragForceRate = value;
    }
  }

  public static float gravityRate {
    set {
      _gravityRate = value;
    }
  }

  public bool CollisionSwitch {
    get {
      return _bCollisionSwitch;
    }
    set {
      _bCollisionSwitch = value;
    }
  }

  public CySpringParamDataAsset springData {
    set {
      _springData = value;
    }
  }

  public List<CySpringCollisionComponent> CollisionList {
    get {
      return _collisionList;
    }
  }

  public float sqartDeltaTime {
    get {
      return _sqartDeltaTime;
    }
  }

  public CySpringRootBone[] boneList {
    get {
      return _boneList;
    }
  }

  private void Awake() {
    Create();
  }

  private void Start() {
    _cachedTransform = base.gameObject.transform;
    SetCollision();
    Reset();
  }

  public void Bind(CySpringParamDataAsset springData) {
    _springData = springData;
    Create();
    SetCollision();
    Reset();
  }

  public void SetSpecificSpringRatio(float fTargetFPS, float fSpringFactor, float fGravityFactor) {
    float num = 1f / fTargetFPS;
    num = 1f / num;
    num = (float) (int) (num + 0.5f);
    num = 1f / num;
    float f = num / 0.0166666675f;
    _isUseSpecificValue = true;
    _specificStiffnessForceRate = Mathf.Pow(f, 1f / fSpringFactor);
    _specificDragForceRate = 1f / _specificStiffnessForceRate;
    _specificGravityRate = Mathf.Pow(f, fGravityFactor);
  }

  public void GatherSpring(float deltaTime) {
    if (_boneList != null) {
      _sqartDeltaTime = deltaTime;
      for (int i = 0; i < CollisionList.Count; i++) {
        switch (CollisionList[i].CollisionType) {
          case CySpringCollisionData.CollisionType.Sphere:
            _nativeCollisions[i]._position = CollisionList[i].cacheTransform.position;
            break;
          case CySpringCollisionData.CollisionType.Capsule:
            _nativeCollisions[i]._position = CollisionList[i].cacheTransform.TransformPoint(CollisionList[i].Offset);
            _nativeCollisions[i]._position2 = CollisionList[i].cacheTransform.TransformPoint(CollisionList[i].Offset2);
            break;
        }
      }
      for (int j = 0; j < _boneList.Length; j++) {
        _boneList[j].GathreAllSpring();
        _boneList[j]._natives[0]._isSkip = _boneList[j]._isSkip;
      }
    }
  }

  public void UpdateSpring() {
    if (boneList != null) {
      for (int i = 0; i < boneList.Length; i++) {
        CySpringNative.UpdateNativeCloth(_boneList[i]._natives, _boneList[i]._natives.Length, _nativeCollisions, _stiffnessForceRate, _dragForceRate, _gravityRate, _bCollisionSwitch);
      }
    }
  }

  public void PostSpring() {
    if (_boneList != null) {
      for (int i = 0; i < _boneList.Length; i++) {
        _boneList[i].PostAllSpring();
      }
    }
  }

  private void Create() {
    _sqartDeltaTime = 0f;
    if (_collisionList == null) {
      _collisionList = new List<CySpringCollisionComponent>(GetComponentsInChildren<CySpringCollisionComponent>());
    }
    if (!(_springData == null)) {
      List<CySpringRootBone> list = new List<CySpringRootBone>();
      for (int i = 0; i < _springData._elements.Count; i++) {
        CySpringRootBone cySpringRootBone = new CySpringRootBone();
        cySpringRootBone.CreateRoot(_springData._elements[i], this, cySpringRootBone, null);
        if (cySpringRootBone.Exist) {
          list.Add(cySpringRootBone);
        }
      }
      _boneList = list.ToArray();
      _nativeCollisions = new NativeClothCollision[_collisionList.Count];
      for (int j = 0; j < _collisionList.Count; j++) {
        _nativeCollisions[j]._radius = _collisionList[j].Radius;
        _nativeCollisions[j]._type = (int) _collisionList[j].CollisionType;
      }
    }
  }

  private void SetCollision() {
    if (_boneList != null) {
      for (int i = 0; i < _boneList.Length; i++) {
        _boneList[i].SetCollision();
      }
    }
  }

  public void Reset(bool bForceReset = true) {
    if (_boneList != null && _boneList.Length > 0) {
      if (_cachedTransform == null) {
        _cachedTransform = base.gameObject.transform;
      }
      Vector3 position = _cachedTransform.transform.position;
      Quaternion rotation = _cachedTransform.transform.rotation;
      _cachedTransform.position = Vector3.zero;
      _cachedTransform.rotation = Quaternion.identity;
      for (int i = 0; i < _boneList.Length; i++) {
        _boneList[i].Reset(bForceReset);
      }
      _cachedTransform.position = position;
      _cachedTransform.rotation = rotation;
    }
  }

  public static GameObject FindGameObject(GameObject rootObject, string objectName) {
    if (!objectName.Contains("/")) {
      return FindGameObjectLoop(rootObject, objectName);
    }
    string[] array = objectName.Split('/');
    if (array.Length == 0) {
      return null;
    }
    GameObject gameObject = rootObject;
    for (int i = 0; i < array.Length; i++) {
      gameObject = FindGameObjectLoop(gameObject, array[i]);
      if (gameObject == null) {
        return null;
      }
    }
    return gameObject;
  }

  private static GameObject FindGameObjectLoop(GameObject rootObject, string objectName) {
    if (rootObject == null) {
      return null;
    }
    foreach (Transform item in rootObject.transform) {
      if (item.name.Contains(objectName)) {
        return item.gameObject;
      }
    }
    foreach (Transform item2 in rootObject.transform) {
      GameObject gameObject = FindGameObjectLoop(item2.gameObject, objectName);
      if (gameObject != null) {
        return gameObject;
      }
    }
    return null;
  }

  public static void SetAttenuationFactor(float fTargetFPS, float fSpringFactor, float fGravityFactor) {
    float num = 1f / fTargetFPS;
    num = 1f / num;
    num = (float) (int) (num + 0.5f);
    num = 1f / num;
    float f = num / 0.0166666675f;
    _stiffnessForceRate = Mathf.Pow(f, 1f / fSpringFactor);
    _dragForceRate = 1f / _stiffnessForceRate;
    _gravityRate = Mathf.Pow(f, fGravityFactor);
  }

  public static void ClearUnionCollision() {
    if (_lstUnionCollision != null) {
      _lstUnionCollision.Clear();
      _lstUnionCollision = null;
    }
  }

  public static void AddUnionCollision(CySpringCollisionComponent comp) {
    if (_lstUnionCollision == null) {
      _lstUnionCollision = new List<CySpringCollisionComponent>();
    }
    if (_lstUnionCollision != null) {
      _lstUnionCollision.Add(comp);
    }
  }
}