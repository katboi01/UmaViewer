using System.Collections.Generic;
using UnityEngine;
using static DynamicBone;
using System;
using Object = UnityEngine.Object;

[Serializable]
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


[Serializable]
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


[Serializable]
public class StringObjectPair
{
    public string Key;
    public Object Value;
}

[Serializable]
public class StringValuePair
{
    public string Key;
    public float Value;
}

[Serializable]
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

[Serializable]
public class FaceGroupInfo
{
    public List<TrsArray> _trsArray;
}

[Serializable]
public class EyeTarget
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[Serializable]
public class EyebrowTarget
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[Serializable]
public class MouthTarget
{
    public List<FaceGroupInfo> _faceGroupInfo;
}

[Serializable]
public class TargetInfomation
{
    public List<FaceGroupInfo> _faceGroupInfo;
}


[Serializable]
public class ConnectedBoneData 
{
    public string Bone1;
    public string Bone2;
    public float Intensity;
    public bool IsFold;
}


[Serializable]
public class FaceOverrideReplaceDataSet 
{
    public bool IsOnlyBaseReplace; 
    public Gallop.Live.Cutt.LiveTimelineDefine.FacialEyeId BaseReplaceFaceType;
    public FaceOverrideReplaceData[] DataArray;
}

[Serializable]
public class FaceOverrideReplaceData
{
    public Gallop.Live.Cutt.LiveTimelineDefine.FacialEyeId[] SrcArray;
    public FaceOverrideElement[] DstArray;
}

[Serializable]
public class FaceOverrideElement 
{
    public Gallop.Live.Cutt.LiveTimelineDefine.FacialEyeId Index; 
    public float Weight; 
}

