using UnityEngine;

public class CySpringCollisionComponent : MonoBehaviour {
  public enum ePurpose {
    None = 0,
    Generic = 1,
    Union = 2,
    Ground = 4
  }

  [SerializeField]
  public float _radius;

  [SerializeField]
  private CySpringCollisionData.CollisionType _collisionType;

  private Vector3 _offset = new Vector3(0f, 0f, 0f);

  private Vector3 _offset2 = new Vector3(0f, 0f, 0f);

  private Transform _cacheTransform;

  private ePurpose _purpose = ePurpose.Generic;

  private Vector3 _unionCollisionPosition = Vector3.zero;

  public float Radius {
    get {
      return _radius;
    }
    set {
      _radius = value;
    }
  }

  public CySpringCollisionData.CollisionType CollisionType {
    get {
      return _collisionType;
    }
    set {
      _collisionType = value;
    }
  }

  public Vector3 Offset {
    get {
      return _offset;
    }
    set {
      _offset = value;
    }
  }

  public Vector3 Offset2 {
    get {
      return _offset2;
    }
    set {
      _offset2 = value;
    }
  }

  public ePurpose purpose {
    get {
      return _purpose;
    }
    set {
      _purpose = value;
    }
  }

  public Vector3 unionCollisionPosition {
    get {
      return _unionCollisionPosition;
    }
  }

  public Transform cacheTransform {
    get {
      return _cacheTransform;
    }
  }

  private void Awake() {
    _cacheTransform = GetComponent<Transform>();
  }

  public void UpdateUnionCollision() {
    _unionCollisionPosition = _cacheTransform.position;
  }

  public void EnableUnionCollision(bool bEnable) {
    if (bEnable) {
      _collisionType = CySpringCollisionData.CollisionType.Sphere;
    } else {
      _collisionType = CySpringCollisionData.CollisionType.None;
    }
  }
}