// CySpringCollisionDataAsset
using System;
using System.Collections.Generic;
using UnityEngine;

public class CySpringCollisionDataAsset : ScriptableObject
{
    [SerializeField]
    private List<CySpringCollisionData> _dataList;

    [SerializeField]
    private List<CySpringPartsCollisionData> _partsDataList = new List<CySpringPartsCollisionData>();

    [NonSerialized]
    private List<CySpringCollisionData> _mergedDataList = new List<CySpringCollisionData>();

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

    public List<CySpringCollisionData> mergedDataList => _mergedDataList;

    public void Build(List<string> lstPartsCode)
    {
        Dictionary<string, CySpringPartsCollisionData> dictionary = new Dictionary<string, CySpringPartsCollisionData>();
        foreach (CySpringPartsCollisionData partsData in _partsDataList)
        {
            if (!dictionary.ContainsKey(partsData.partsCode))
            {
                dictionary.Add(partsData.partsCode, partsData);
            }
        }
        _mergedDataList.Clear();
        _mergedDataList.AddRange(_dataList);
        if (lstPartsCode == null)
        {
            return;
        }
        foreach (string item in lstPartsCode)
        {
            CySpringPartsCollisionData value = null;
            if (dictionary.TryGetValue(item, out value) && value.valid)
            {
                _mergedDataList.AddRange(value.dataList);
            }
        }
    }
}
