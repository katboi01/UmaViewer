using System.Collections.Generic;
using UnityEngine;

public class CySpringCollision : MonoBehaviour
{
    [SerializeField]
    private CySpringCollisionDataAsset _collisionData;

    private float _collisionScale = 1f;

    public float CollisionScale
    {
        get
        {
            return _collisionScale;
        }
        set
        {
            _collisionScale = value;
        }
    }

    public void Create(CySpringCollisionDataAsset data, HashSet<string> setUsableCollision)
    {
        _collisionData = data;
        if (_collisionData == null)
        {
            return;
        }
        foreach (CySpringCollisionData data2 in _collisionData.dataList)
        {
            if (setUsableCollision.Contains(data2.collisionName))
            {
                data2.Create(this);
            }
        }
    }

    public void Create(CySpringCollisionDataAsset data)
    {
        _collisionData = data;
        if (_collisionData == null)
        {
            return;
        }
        foreach (CySpringCollisionData mergedData in _collisionData.mergedDataList)
        {
            mergedData.Create(this);
        }
    }

    public CySpringCollisionComponent CreateCollision(string targetName, string collisionName, float radius)
    {
        return new CySpringCollisionData(collisionName, targetName, Vector3.zero, radius * CollisionScale).Create(this);
    }
}
