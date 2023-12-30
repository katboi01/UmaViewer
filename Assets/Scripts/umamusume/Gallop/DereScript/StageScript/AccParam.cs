using System;
using System.Collections.Generic;
using UnityEngine;

public class AccParam : ScriptableObject
{
    [Serializable]
    public class Info
    {
        public int charaId;

        public string attachMain = String.Empty;

        public Vector3 offsetMain = Vector3.zero;

        public Vector3 rotateMain = Vector3.zero;

        public Vector3 scaleMain = Vector3.one;

        public int idxSub = -1;

        public string attachSub = String.Empty;

        public Vector3 offsetSub = Vector3.zero;

        public Vector3 rotateSub = Vector3.zero;

        public Vector3 scaleSub = Vector3.one;

        public void CopyTo(ref Info info, bool bSkipCharaId = false)
        {
            if (!bSkipCharaId)
            {
                info.charaId = charaId;
            }
            info.attachMain = attachMain;
            info.offsetMain = offsetMain;
            info.rotateMain = rotateMain;
            info.scaleMain = scaleMain;
            info.idxSub = idxSub;
            info.attachSub = attachSub;
            info.offsetSub = offsetSub;
            info.rotateSub = rotateSub;
            info.scaleSub = scaleSub;
        }
    }

    [SerializeField]
    public string _attachmentNode = String.Empty;

    [SerializeField]
    [HideInInspector]
    private Info[] _arrInfo = new Info[0];

    [NonSerialized]
    private Dictionary<int, Info> _mapInfo = new Dictionary<int, Info>();

    [NonSerialized]
    private bool _bBuilded;

    public Dictionary<int, Info> mapInfo => _mapInfo;

    public bool isBuilded => _bBuilded;

    public void BuildMap()
    {
        _mapInfo.Clear();
        for (int i = 0; i < _arrInfo.Length; i++)
        {
            try
            {
                _mapInfo.Add(_arrInfo[i].charaId, _arrInfo[i]);
            }
            catch (Exception)
            {
            }
        }
        _bBuilded = true;
    }

    public Info GetInfo(int charaId)
    {
        Info value = null;
        _mapInfo.TryGetValue(charaId, out value);
        return value;
    }
}
