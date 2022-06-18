using Unity;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AssetTable
{
    public List<StringObjectPair> list;
}

[System.Serializable]
public class AssetTableValue
{
    public List<StringValuePair> i;
}


[System.Serializable]
public class StringObjectPair
{
    public string Key;
    public GameObject Value;
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
    public string _path { get; set; }
    public int _isValidScaleTransform { get; set; }
    public Vector3 _position { get; set; }
    public Vector3 _scale { get; set; }
    public Vector3 _rotation { get; set; }
    public int IsOverrideTarget { get; set; }
}

[System.Serializable]
public class FaceGroupInfo
{
    public List<TrsArray> _trsArray { get; set; }
}

[System.Serializable]
public class EyeTarget
{
    public List<FaceGroupInfo> _faceGroupInfo { get; set; }
}

[System.Serializable]
public class EyebrowTarget
{
    public List<FaceGroupInfo> _faceGroupInfo { get; set; }
}

[System.Serializable]
public class MouthTarget
{
    public List<FaceGroupInfo> _faceGroupInfo { get; set; }
}

[System.Serializable]
public class TargetInfomation
{
    public List<FaceGroupInfo> _faceGroupInfo { get; set; }
}