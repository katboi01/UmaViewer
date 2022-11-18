using Unity;
using System.Collections.Generic;
using UnityEngine;
using static DynamicBone;

[System.Serializable]
public class AssetTable
{
    public Object this[string key]
    {
        get 
        {
            var item = list.Find(a => a.Key.Equals(key));
            if(item != null)
            {
                return item.Value;
            }
            else
            {
                return null;
            }
        }
    }

    public List<StringObjectPair> list;
}


[System.Serializable]
public class AssetTableValue
{
    public float this[string key]
    {
        get
        {
            var item = list.Find(a => a.Key.Equals(key));
            if (item != null)
            {
                return item.Value;
            }
            else
            {
                return -1;
            }
        }
    }
    public List<StringValuePair> list;
}


[System.Serializable]
public class StringObjectPair
{
    public string Key;
    public Object Value;
}

[System.Serializable]
public class StringValuePair
{
    public string Key;
    public float Value;
}

[System.Serializable]
public class TrsArray
{
    public string _path;
    public Transform transform;
    public bool _isValidScaleTransform;
    public Vector3 _position;
    public Vector3 _scale;
    public Vector3 _rotation;
    public bool IsOverrideTarget;

    public bool isPhysics;
    public Particle physicsParticle;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

[System.Serializable]
public class FaceGroupInfo
{
    public List<TrsArray> _trsArray;
}

[System.Serializable]
public class EyeTarget
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[System.Serializable]
public class EyebrowTarget
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[System.Serializable]
public class MouthTarget
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[System.Serializable]
public class TargetInfomation
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[System.Serializable]
public class CySpringCollisionData
{
    public string _collisionName;
    public string _targetObjectName;
    public bool _isOtherTarget;
    public Vector3 _offset;
    public Vector3 _offset2;
    public float _radius;
    public float _distance;
    public Vector3 _normal;
    public CollisionType _type;
    public bool _isInner;
    public enum CollisionType
    {   
	    Sphere,
        None,
        Capsule,
        Plane,
    }
    
}

[System.Serializable]
public class CySpringParamDataElement
{
    public string _boneName;
    public float _stiffnessForce;
    public float _dragForce;
    public float _gravity;
    public float _collisionRadius;
    public bool _needEnvCollision;
    public List<string> _collisionNameList;
    public bool _isLimit;
    public Vector3 _limitAngleMin;
    public Vector3 _limitAngleMax;
    public float MoveSpringApplyRate;
    public bool _needSimulateEndBone;
    public float _verticalWindRateSlow;
    public float _horizontalWindRateSlow;
    public float _verticalWindRateFast;
    public float _horizontalWindRateFast;
    public List<CySpringParamDataChildElement> _childElements;
}

[System.Serializable]
public class CySpringParamDataChildElement
{
    public string _boneName;
    public float _stiffnessForce;
    public float _dragForce;
    public float _gravity;
    public float _collisionRadius;
    public bool _needEnvCollision;
    public List<string> _collisionNameList;
    public float _verticalWindRateSlow;
    public float _horizontalWindRateSlow;
    public float _verticalWindRateFast;
    public float _horizontalWindRateFast;
    public bool _isLimit;
    public Vector3 _limitAngleMin;
    public Vector3 _limitAngleMax;
    public float MoveSpringApplyRate;
}

[System.Serializable]
public class ConnectedBoneData 
{
    public string Bone1;
    public string Bone2;
    public float Intensity;
    public bool IsFold;
}
