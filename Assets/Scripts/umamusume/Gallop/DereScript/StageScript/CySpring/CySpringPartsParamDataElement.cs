using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CySpringPartsParamDataElement
{
    [SerializeField]
    private string _partsCode = string.Empty;

    [SerializeField]
    private List<CySpringParamDataElement> _elements;

    public string partsCode => _partsCode;

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

    public bool valid
    {
        get
        {
            if (Character3DBase.Parts.IsValidPartsCode(_partsCode))
            {
                if (_elements != null)
                {
                    return _elements.Count > 0;
                }
                return false;
            }
            return false;
        }
    }
}
