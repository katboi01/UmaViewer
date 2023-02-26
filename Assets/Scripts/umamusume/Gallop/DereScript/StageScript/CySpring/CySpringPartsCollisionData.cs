using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CySpringPartsCollisionData
{
    [SerializeField]
    private string _partsCode = string.Empty;

    [SerializeField]
    private List<CySpringCollisionData> _dataList;

    public string partsCode => _partsCode;

    public List<CySpringCollisionData> dataList
    {
        get
        {
            return _dataList;
        }
        set
        {
            _dataList = value;
        }
    }

    public bool valid
    {
        get
        {
            if (Character3DBase.Parts.IsValidPartsCode(_partsCode))
            {
                if (_dataList != null)
                {
                    return _dataList.Count > 0;
                }
                return false;
            }
            return false;
        }
    }
}
