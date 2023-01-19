using System;
using System.Collections.Generic;
using UnityEngine;

public class CySpringParamDataAsset : ScriptableObject
{
    [SerializeField]
    private List<CySpringParamDataElement> _elements;

    [SerializeField]
    private List<CySpringPartsParamDataElement> _partsElements;

    [NonSerialized]
    private List<CySpringParamDataElement> _mergedDataList = new List<CySpringParamDataElement>();

    public List<CySpringParamDataElement> elements
    {
        get
        {
            return _elements;
        }
        set
        {
            _elements = value;
        }
    }

    public List<CySpringParamDataElement> mergeDataList => _mergedDataList;

    public void Build(List<string> lstPartsCode)
    {
        Dictionary<string, CySpringPartsParamDataElement> dictionary = new Dictionary<string, CySpringPartsParamDataElement>();
        foreach (CySpringPartsParamDataElement partsElement in _partsElements)
        {
            if (!dictionary.ContainsKey(partsElement.partsCode))
            {
                dictionary.Add(partsElement.partsCode, partsElement);
            }
        }
        _mergedDataList.Clear();
        _mergedDataList.AddRange(_elements);
        if (lstPartsCode == null)
        {
            return;
        }
        foreach (string item in lstPartsCode)
        {
            CySpringPartsParamDataElement value = null;
            if (dictionary.TryGetValue(item, out value) && value.valid)
            {
                _mergedDataList.AddRange(value.elements);
            }
        }
    }
}
