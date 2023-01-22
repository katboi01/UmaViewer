using System.Collections.Generic;
using UnityEngine;

public class CySpringCollision : MonoBehaviour {
  [SerializeField]
  private CySpringCollisionDataAsset _collisionData;

  public void Create(CySpringCollisionDataAsset data, HashSet<string> setUsableCollision) {
    _collisionData = data;
    if (!(_collisionData == null)) {
      foreach (CySpringCollisionData data2 in _collisionData._dataList) {
        if (setUsableCollision.Contains(data2.collisionName)) {
          data2.Create(this);
        }
      }
    }
  }

  public void Create(CySpringCollisionDataAsset data) {
    _collisionData = data;
    if (!(_collisionData == null)) {
      foreach (CySpringCollisionData data2 in _collisionData._dataList) {
        data2.Create(this);
      }
    }
  }

  public CySpringCollisionComponent CreateCollision(string targetName, string collisionName, float radius) {
    CySpringCollisionData data = new CySpringCollisionData(collisionName, targetName, Vector3.zero, radius);
    return data.Create(this);
  }
}